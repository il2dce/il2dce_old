// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2011 Stefan Rothdach
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;

using maddox.game;
using maddox.game.play;
using maddox.game.page;
using maddox.GP;

namespace IL2DCE
{
    public class Core : ICore
    {
        private Random rand = new Random();

        private List<AirGroup> availableAirGroups = new List<AirGroup>();
        private List<GroundGroup> availableGroundGroups = new List<GroundGroup>();

        private int additionalAirOperations = 0;
        private int additionalGroundOperations = 0;

        private List<Stationary> redStationaries = new List<Stationary>();
        private List<Stationary> blueStationaries = new List<Stationary>();

        private class AirOperationTarget
        {
            public EMissionType? MissionType
            {
                get
                {
                    return this.missionType;
                }
                set
                {
                    this.missionType = value;
                }
            }
            private EMissionType? missionType = null;

            public double Altitude
            {
                get
                {
                    return this.altitude;
                }
                set
                {
                    this.altitude = value;
                }
            }
            private double altitude = 0.0;

            public Stationary TargetStationary
            {
                get
                {
                    return this.targetStationary;
                }
                set
                {
                    this.targetStationary = value;
                }
            }
            private Stationary targetStationary = null;

            public GroundGroup TargetGroundGroup
            {
                get
                {
                    return this.targetGroundGroup;
                }
                set
                {
                    this.targetGroundGroup = value;
                }
            }
            private GroundGroup targetGroundGroup = null;

            public AirGroup TargetAirGroup
            {
                get
                {
                    return this.targetAirGroup;
                }
                set
                {
                    this.targetAirGroup = value;
                }
            }
            private AirGroup targetAirGroup = null;

            public Point3d? TargetArea
            {
                get
                {
                    return this.targetArea;
                }
                set
                {
                    this.targetArea = value;
                }
            }
            private Point3d? targetArea = null;
        }

        public Core(IGame game)
        {
            _game = game;

            ISectionFile confFile = game.gameInterface.SectionFileLoad("$home/parts/IL2DCE/conf.ini");

            if (confFile.exist("Core", "forceSetOnPark"))
            {
                string value = confFile.get("Core", "forceSetOnPark");
                if (value == "1")
                {
                    SpawnParked = true;
                }
                else
                {
                    SpawnParked = false;
                }
            }

            if (confFile.exist("Core", "additionalAirOperations"))
            {
                string value = confFile.get("Core", "additionalAirOperations");
                int.TryParse(value, out additionalAirOperations);
            }

            if (confFile.exist("Core", "additionalGroundOperations"))
            {
                string value = confFile.get("Core", "additionalGroundOperations");
                int.TryParse(value, out additionalGroundOperations);
            }

            if (confFile.exist("Core", "debug"))
            {
                string value = confFile.get("Core", "debug");
                int.TryParse(value, out _debug);
            }
            else
            {
                _debug = 0;
            }

            if (confFile.exist("Main", "campaignsFolder"))
            {
                string campaignsFolderPath = confFile.get("Main", "campaignsFolder");
                string campaignsFolderSystemPath = game.gameInterface.ToFileSystemPath(campaignsFolderPath);

                System.IO.DirectoryInfo campaignsFolder = new System.IO.DirectoryInfo(campaignsFolderSystemPath);
                if (campaignsFolder.Exists && campaignsFolder.GetDirectories() != null && campaignsFolder.GetDirectories().Length > 0)
                {
                    ISectionFile globalAircraftInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + "AircraftInfo.ini");
                    foreach (System.IO.DirectoryInfo campaignFolder in campaignsFolder.GetDirectories())
                    {
                        if (campaignFolder.GetFiles("CampaignInfo.ini") != null && campaignFolder.GetFiles("CampaignInfo.ini").Length == 1)
                        {
                            ISectionFile campaignInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + campaignFolder.Name + "/CampaignInfo.ini");
                            
                            ISectionFile localAircraftInfoFile = null;
                            System.IO.FileInfo localAircraftInfoFileInfo = new System.IO.FileInfo(game.gameInterface.ToFileSystemPath(campaignsFolderPath + "/" + campaignFolder.Name + "/AircraftInfo.ini"));
                            if (localAircraftInfoFileInfo.Exists)
                            {
                                localAircraftInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + campaignFolder.Name + "/AircraftInfo.ini");
                            }

                            CampaignInfo campaignInfo = new CampaignInfo(campaignFolder.Name, campaignsFolderPath + "/" + campaignFolder.Name + "/", campaignInfoFile, globalAircraftInfoFile, localAircraftInfoFile);
                            CampaignInfos.Add(campaignInfo);
                        }
                    }
                }
            }

            string careersFolderSystemPath = game.gameInterface.ToFileSystemPath("$user/mission/IL2DCE");
            System.IO.DirectoryInfo careersFolder = new System.IO.DirectoryInfo(careersFolderSystemPath);
            if (careersFolder.Exists && careersFolder.GetDirectories() != null && careersFolder.GetDirectories().Length > 0)
            {
                foreach (System.IO.DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    if (careerFolder.GetFiles("Career.ini") != null && careerFolder.GetFiles("Career.ini").Length == 1)
                    {
                        ISectionFile careerFile = game.gameInterface.SectionFileLoad("$user/mission/IL2DCE" + "/" + careerFolder.Name + "/Career.ini");

                        Career career = new Career(careerFolder.Name, CampaignInfos, careerFile);
                        Careers.Add(career);
                    }
                }
            }
        }

        public void DeleteCareer(ICareer career)
        {
            Careers.Remove(career);
            if (Game.Core.Career == career)
            {
                Game.Core.Career = null;
            }

            List<System.IO.DirectoryInfo> deleteFolders = new List<System.IO.DirectoryInfo>();
            string careersFolderSystemPath = Game.gameInterface.ToFileSystemPath("$user/mission/IL2DCE");
            System.IO.DirectoryInfo careersFolder = new System.IO.DirectoryInfo(careersFolderSystemPath);
            if (careersFolder.Exists && careersFolder.GetDirectories() != null && careersFolder.GetDirectories().Length > 0)
            {
                foreach (System.IO.DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    if (career.PilotName == careerFolder.Name)
                    {
                        deleteFolders.Add(careerFolder);
                    }
                }
            }

            for (int i = 0; i < deleteFolders.Count; i++)
            {
                deleteFolders[i].Delete(true);
            }
        }

        public ICareer Career
        {
            get
            {
                return _campaign;
            }
            set
            {
                if (_campaign != value)
                {
                    _campaign = value;
                }
            }
        }
        private ICareer _campaign;

        public List<ICareer> Careers
        {
            get
            {
                return _careers;
            }
        }
        private List<ICareer> _careers = new List<ICareer>();

        public List<ICampaignInfo> CampaignInfos
        {
            get
            {
                return campaigns;
            }
        }
        private List<ICampaignInfo> campaigns = new List<ICampaignInfo>();

        private IGame Game
        {
            get
            {
                return _game;
            }
        }
        private IGame _game;

        public bool SpawnParked
        {
            get
            {
                return _spawnParked;
            }
            set
            {
                _spawnParked = value;
            }
        }
        public static bool _spawnParked = false;

        public int Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
            }
        }
        private int _debug = 0;

        public System.Collections.Generic.IList<Waterway> Roads
        {
            get
            {
                return _roads;
            }
        }
        System.Collections.Generic.List<Waterway> _roads = new System.Collections.Generic.List<Waterway>();

        public System.Collections.Generic.IList<Waterway> Waterways
        {
            get
            {
                return _waterways;
            }
        }
        System.Collections.Generic.List<Waterway> _waterways = new System.Collections.Generic.List<Waterway>();

        public System.Collections.Generic.IList<maddox.GP.Point3d> RedFrontMarkers
        {
            get
            {
                return redFrontMarkers;
            }
        }

        public System.Collections.Generic.IList<maddox.GP.Point3d> BlueFrontMarkers
        {
            get
            {
                return blueFrontMarkers;
            }
        }
        private List<Point3d> redFrontMarkers = new List<Point3d>();
        private List<Point3d> blueFrontMarkers = new List<Point3d>();
        private List<Point3d> neutralFrontMarkers = new List<Point3d>();

        public System.Collections.Generic.IList<IAirGroup> AirGroups
        {
            get
            {
                List<IAirGroup> airGroups = new List<IAirGroup>();
                airGroups.AddRange(redAirGroups);
                airGroups.AddRange(blueAirGroups);
                return airGroups;
            }
        }

        public System.Collections.Generic.IList<IAirGroup> RedAirGroups
        {
            get
            {
                List<IAirGroup> airGroups = new List<IAirGroup>();
                airGroups.AddRange(redAirGroups);
                return airGroups;
            }
        }

        public System.Collections.Generic.IList<IAirGroup> BlueAirGroups
        {
            get
            {
                List<IAirGroup> airGroups = new List<IAirGroup>();
                airGroups.AddRange(blueAirGroups);
                return airGroups;
            }
        }

        private List<AirGroup> redAirGroups = new List<AirGroup>();
        private List<AirGroup> blueAirGroups = new List<AirGroup>();

        public System.Collections.Generic.IList<IGroundGroup> GroundGroups
        {
            get
            {
                List<IGroundGroup> groundGroups = new List<IGroundGroup>();
                groundGroups.AddRange(redGroundGroups);
                groundGroups.AddRange(blueGroundGroups);
                return groundGroups;
            }
        }

        public System.Collections.Generic.IList<IGroundGroup> RedGroundGroups
        {
            get
            {
                List<IGroundGroup> groundGroups = new List<IGroundGroup>();
                groundGroups.AddRange(redGroundGroups);
                return groundGroups;
            }
        }

        public System.Collections.Generic.IList<IGroundGroup> BlueGroundGroups
        {
            get
            {
                List<IGroundGroup> groundGroups = new List<IGroundGroup>();
                groundGroups.AddRange(blueGroundGroups);
                return groundGroups;
            }
        }

        private List<GroundGroup> redGroundGroups = new List<GroundGroup>();
        private List<GroundGroup> blueGroundGroups = new List<GroundGroup>();

        public void ResetCampaign()
        {
            // Reset campaign state
            Career.Date = null;

            AdvanceCampaign();
        }

        public void AdvanceCampaign()
        {
            if (!Career.Date.HasValue)
            {
                Career.Date = Career.CampaignInfo.StartDate;
                Career.Experience = Career.RankIndex * 1000;
            }
            else
            {
                Career.Date = Career.Date.Value.Add(new TimeSpan(1, 0, 0, 0));
                Career.Experience += 100;

                if (Career.Experience >= (Career.RankIndex + 1) * 1000)
                {
                    Career.RankIndex += 1;
                }
            }

            string missionFolderSystemPath = Game.gameInterface.ToFileSystemPath("$user/mission/IL2DCE/" + Career.PilotName);
            if (!System.IO.Directory.Exists(missionFolderSystemPath))
            {
                System.IO.Directory.CreateDirectory(missionFolderSystemPath);
            }

            string missionId = Career.CampaignInfo.Id + "_" + Career.Date.Value.Date.Year.ToString() + "-" + Career.Date.Value.Date.Month.ToString() + "-" + Career.Date.Value.Date.Day.ToString();
            ISectionFile missionFile = null;
            IBriefingFile briefingFile = null;
            
            ISectionFile careerFile = Game.gameInterface.SectionFileCreate();
            
            generate(Career.CampaignInfo.TemplateFilePath, missionId, out missionFile, out briefingFile);

            string missionFileName = string.Format("$user/mission/IL2DCE/" + Career.PilotName + "/{0}.mis", missionId);
            string briefingFileName = string.Format("$user/mission/IL2DCE/" + Career.PilotName + "/{0}.briefing", missionId);
            string scriptFileName = string.Format("$user/mission/IL2DCE/" + Career.PilotName + "/{0}.cs", missionId);
            string careerFileName = "$user/mission/IL2DCE/" + Career.PilotName + "/Career.ini";


            string scriptSourceFileSystemPath = Game.gameInterface.ToFileSystemPath(Career.CampaignInfo.ScriptFilePath);
            string scriptDestinationFileSystemPath = Game.gameInterface.ToFileSystemPath(scriptFileName);
            System.IO.File.Copy(scriptSourceFileSystemPath, scriptDestinationFileSystemPath, true);

            missionFile.save(missionFileName);
            briefingFile.save(briefingFileName);

#if DEBUG
            string debugPath = Game.gameInterface.ToFileSystemPath("$user/missions/IL2DCE/Debug");
            if (!System.IO.Directory.Exists(debugPath))
            {
                System.IO.Directory.CreateDirectory(debugPath);
            }
            missionFile.save("$user/missions/IL2DCE/Debug/IL2DCEDebug.mis");
            briefingFile.save("$user/missions/IL2DCE/Debug/IL2DCEDebug.briefing");
            System.IO.File.Copy(scriptSourceFileSystemPath, debugPath + "\\IL2DCEDebug.cs", true);
#else
            string debugPath = Game.gameInterface.ToFileSystemPath("$user/missions/IL2DCE/Debug");
            if (Debug == 1)
            {
                debugPath = Game.gameInterface.ToFileSystemPath("$user/missions/IL2DCE/Debug");
                if (!System.IO.Directory.Exists(debugPath))
                {
                    System.IO.Directory.CreateDirectory(debugPath);
                }
                missionFile.save("$user/missions/IL2DCE/Debug/IL2DCEDebug.mis");
                briefingFile.save("$user/missions/IL2DCE/Debug/IL2DCEDebug.briefing");
            }
#endif

            Career.MissionFileName = missionFileName;
            Career.writeTo(careerFile);
            careerFile.save(careerFileName);
        }
        
        public void InitCampaign()
        {
            _roads.Clear();
            _waterways.Clear();
            redStationaries.Clear();
            blueStationaries.Clear();
            redFrontMarkers.Clear();
            blueFrontMarkers.Clear();
            redAirGroups.Clear();
            blueAirGroups.Clear();
            redGroundGroups.Clear();
            blueGroundGroups.Clear();

            ISectionFile templateFile = Game.gpLoadSectionFile(Career.CampaignInfo.TemplateFilePath);

            for (int i = 0; i < templateFile.lines("Stationary"); i++)
            {
                string key;
                string value;
                templateFile.get("Stationary", i, out key, out value);

                // Radar
                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length > 4)
                {
                    if (valueParts[0] == "Stationary.Radar.EnglishRadar1")
                    {
                        double x;
                        double y;
                        double.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x);
                        double.TryParse(valueParts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y);
                        Stationary radar = new Stationary(key, x, y);
                        redStationaries.Add(radar);
                    }
                }
            }

            for (int i = 0; i < templateFile.lines("FrontMarker"); i++)
            {
                string key;
                string value;
                templateFile.get("FrontMarker", i, out key, out value);

                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length == 3)
                {
                    double x;
                    double y;
                    int army;
                    if (double.TryParse(valueParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x)
                        && double.TryParse(valueParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y)
                        && int.TryParse(valueParts[2], out army))
                    {
                        if (army == 0)
                        {
                            neutralFrontMarkers.Add(new Point3d(x, y, 0.0));
                        }
                        else if (army == 1)
                        {
                            redFrontMarkers.Add(new Point3d(x, y, 0.0));
                        }
                        else if (army == 2)
                        {
                            blueFrontMarkers.Add(new Point3d(x, y, 0.0));
                        }
                    }
                }
            }

            availableAirGroups.Clear();
            for (int i = 0; i < templateFile.lines("AirGroups"); i++)
            {
                string key;
                string value;
                templateFile.get("AirGroups", i, out key, out value);

                AirGroup airGroup = new AirGroup(this, templateFile, key);
                availableAirGroups.Add(airGroup);

                if (AirGroupInfo.GetAirGroupInfo(1, airGroup.AirGroupKey) != null)
                {
                    getAirGroups(1).Add(airGroup);
                }
                else if (AirGroupInfo.GetAirGroupInfo(2, airGroup.AirGroupKey) != null)
                {
                    getAirGroups(2).Add(airGroup);
                }
            }


            for (int i = 0; i < templateFile.lines("Chiefs"); i++)
            {
                string key;
                string value;
                templateFile.get("Chiefs", i, out key, out value);

                GroundGroup groundGroup = new GroundGroup(templateFile, key);

                if (groundGroup.Army == 1)
                {
                    redGroundGroups.Add(groundGroup);
                }
                else if (groundGroup.Army == 2)
                {
                    blueGroundGroups.Add(groundGroup);
                }
                else
                {
                    Waterway road = new Waterway(templateFile, key);
                    if (value.StartsWith("Vehicle") || value.StartsWith("Armor"))
                    {
                        _roads.Add(road);
                    }
                    else if (value.StartsWith("Ship"))
                    {
                        _waterways.Add(road);
                    }
                }
            }
        }


        private void generate(string templateFileName, string missionId, out ISectionFile missionFile, out IBriefingFile briefingFile)
        {
            availableAirGroups.Clear();
            availableGroundGroups.Clear();

            foreach (AirGroup airGroup in AirGroups)
            {
                availableAirGroups.Add(airGroup);
            }

            foreach (GroundGroup groundGroup in GroundGroups)
            {
                availableGroundGroups.Add(groundGroup);
            }

            missionFile = Game.gpLoadSectionFile(templateFileName);
            briefingFile = new BriefingFile(Game);

            briefingFile.MissionName = missionId;
            briefingFile.MissionDescription = "Mission generated by IL2DCE.";
            
            if(missionFile.exist("AirGroups"))
            {
                // Delete all air groups from the template file.
                for (int i = 0; i < missionFile.lines("AirGroups"); i++)
                {
                    string key;
                    string value;
                    missionFile.get("AirGroups", i, out key, out value);
                    missionFile.delete(key);
                    missionFile.delete(key + "_Way");
                }
                missionFile.delete("AirGroups");
            }

            if (missionFile.exist("Chiefs"))
            {
                // Delete all ground groups from the template file.
                for (int i = 0; i < missionFile.lines("Chiefs"); i++)
                {
                    string key;
                    string value;
                    missionFile.get("Chiefs", i, out key, out value);
                    missionFile.delete(key + "_Road");
                }
                missionFile.delete("Chiefs");
            }

            for (int i = 0; i < missionFile.lines("MAIN"); i++)
            {
                string key;
                string value;
                missionFile.get("MAIN", i, out key, out value);
                if (key == "player")
                {
                    missionFile.delete("MAIN", i);
                    break;
                }
            }

            // Preload mission file for path calculation.
            Game.gameInterface.MissionLoad(missionFile);

            foreach(AirGroup airGroup in getAirGroups(Career.ArmyIndex))
            {
                if(airGroup.Id == Career.AirGroup)
                {
                    List<string> aircraftOrder = new List<string>();
                    if (airGroup.AirGroupInfo.FlightSize % 3 == 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i)
                                {
                                    aircraftOrder.Add(key.ToString() + i.ToString());
                                }
                            }

                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i + 3)
                                {
                                    aircraftOrder.Add(key.ToString() + (i + 3).ToString());
                                }
                            }
                        }
                    }
                    else if (airGroup.AirGroupInfo.FlightSize % 2 == 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i)
                                {
                                    aircraftOrder.Add(key.ToString() + i.ToString());
                                }
                            }

                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i + 2)
                                {
                                    aircraftOrder.Add(key.ToString() + (i + 2).ToString());
                                }
                            }
                        }
                    }
                    else if (airGroup.AirGroupInfo.FlightSize % 1 == 0)
                    {
                        foreach (int key in airGroup.Flights.Keys)
                        {
                            if (airGroup.Flights[key].Count == 1)
                            {
                                aircraftOrder.Add(key.ToString() + "0");
                            }
                        }
                    }
                    
                    
                    string playerAirGroupKey = airGroup.AirGroupKey;
                    int playerSquadronIndex = airGroup.SquadronIndex;
                    string playerPosition = aircraftOrder[aircraftOrder.Count-1];

                    double factor = aircraftOrder.Count / 6;
                    int playerPositionIndex = (int)(Math.Floor(Career.RankIndex * factor));
                                        
                    playerPosition = aircraftOrder[aircraftOrder.Count - 1 - playerPositionIndex];

                    if (missionFile.exist("MAIN", "player"))
                    {
                        missionFile.set("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString() + playerPosition);
                    }
                    else
                    {
                        missionFile.add("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString() + playerPosition);
                    }
                                        
                    createRandomAirOperation(missionFile, briefingFile, airGroup);
                    break;
                }
            }

            if (availableAirGroups != null && availableAirGroups.Count > 0)
            {
                for (int i = 0; i < additionalAirOperations; i++)
                {
                    if (availableAirGroups.Count > 0)
                    {
                        int randomAirGroupIndex = rand.Next(availableAirGroups.Count);
                        AirGroup randomAirGroup = availableAirGroups[randomAirGroupIndex];
                        createRandomAirOperation(missionFile, briefingFile, randomAirGroup);
                    }
                }
            }

            if (availableGroundGroups != null && availableGroundGroups.Count > 0)
            {
                for (int i = 0; i < additionalGroundOperations; i++)
                {
                    if (availableGroundGroups.Count > 0)
                    {
                        int randomGroundGroupIndex = rand.Next(availableGroundGroups.Count);
                        GroundGroup randomGroundGroup = availableGroundGroups[randomGroundGroupIndex];
                        availableGroundGroups.Remove(randomGroundGroup);

                        createRandomGroundOperation(missionFile, randomGroundGroup);
                    }
                }
            }

            // Stop the preloaded battle to prevent a postload.
            Game.gameInterface.BattleStop();
        }

        private void findPath(GroundGroup groundGroup, Point2d start, Point2d end)
        {
            IRecalcPathParams pathParams = null;
            if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
            {
                pathParams = Game.gpFindPath(start, 10.0, end, 20.0, PathType.GROUND, groundGroup.Army);
            }
            else if (groundGroup.Type == EGroundGroupType.Ship)
            {
                pathParams = Game.gpFindPath(start, 10000.0, end, 10000.0, PathType.WATER, groundGroup.Army);
            }

            if (pathParams != null)
            {
                while (pathParams.State == RecalcPathState.WAIT)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Wait for path.", null);
                    System.Threading.Thread.Sleep(100);
                }

                if (pathParams.State == RecalcPathState.SUCCESS)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path found (" + pathParams.Path.Length.ToString() + ").", null);

                    GroundGroupWaypoint lastGroundGroupWaypoint = null;
                    foreach (maddox.game.world.AiWayPoint aiWayPoint in pathParams.Path)
                    {
                        if (aiWayPoint is maddox.game.world.AiGroundWayPoint)
                        {
                            maddox.game.world.AiGroundWayPoint aiGroundWayPoint = aiWayPoint as maddox.game.world.AiGroundWayPoint;

                            if (aiGroundWayPoint.P.z == -1)
                            {
                                GroundGroupWaypoint groundGroupWaypoint = new GroundGroupWaypoint(aiGroundWayPoint.P.x, aiGroundWayPoint.P.y, aiGroundWayPoint.roadWidth, aiGroundWayPoint.Speed);
                                lastGroundGroupWaypoint = groundGroupWaypoint;
                                groundGroup.Waypoints.Add(groundGroupWaypoint);
                            }
                            else if (lastGroundGroupWaypoint != null)
                            {
                                string s = aiGroundWayPoint.P.x.ToString() + " " + aiGroundWayPoint.P.y.ToString() + " " + aiGroundWayPoint.P.z.ToString() + " " + aiGroundWayPoint.roadWidth.ToString();
                                GroundGroupSubWaypoint groundGroupSubWaypoint = new GroundGroupSubWaypoint(s, null);
                                lastGroundGroupWaypoint.SubWaypoints.Add(groundGroupSubWaypoint);
                            }
                        }
                    }
                }
                else if (pathParams.State == RecalcPathState.FAILED)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path not found.", null);
                }
            }
        }

        private void findRoad(GroundGroup groundGroup, Point2d start, Point2d end, IList<Waterway> roads)
        {
            if (roads != null && roads.Count > 0)
            {
                Waterway closestRoad = null;
                double closestRoadDistance = 0.0;
                foreach (Waterway road in roads)
                {
                    if (road.Start != null && road.End != null)
                    {
                        Point2d roadStart = new Point2d(road.Start.Position.x, road.Start.Position.y);
                        double distanceStart = start.distance(ref roadStart);
                        Point2d roadEnd = new Point2d(road.End.Position.x, road.End.Position.y);
                        double distanceEnd = end.distance(ref roadEnd);

                        Point2d p = new Point2d(end.x, end.y);
                        if (distanceEnd < start.distance(ref p))
                        {
                            if (closestRoad == null)
                            {
                                closestRoad = road;
                                closestRoadDistance = distanceStart + distanceEnd;
                            }
                            else
                            {
                                if (distanceStart + distanceEnd < closestRoadDistance)
                                {
                                    closestRoad = road;
                                    closestRoadDistance = distanceStart + distanceEnd;
                                }
                            }
                        }
                    }
                }

                if (closestRoad != null)
                {
                    //findPath(groundGroup, start, new Point2d(closestRoad.Start.X, closestRoad.Start.Y));

                    groundGroup.Waypoints.AddRange(closestRoad.Waypoints);

                    List<Waterway> availableRoads = new List<Waterway>(roads);
                    availableRoads.Remove(closestRoad);

                    findRoad(groundGroup, new Point2d(closestRoad.End.Position.x, closestRoad.End.Position.y), end, availableRoads);
                }
            }
        }

        private void createRandomGroundOperation(ISectionFile missionFile, GroundGroup groundGroup)
        {
            availableGroundGroups.Remove(groundGroup);

            List<Point3d> friendlyMarkers = getFriendlyMarkers(groundGroup.Army);
            if (friendlyMarkers.Count > 0)
            {
                List<Point3d> availableFriendlyMarkers = new List<Point3d>(friendlyMarkers);

                // Find closest friendly marker
                Point3d? closestMarker = null;
                foreach (Point3d marker in availableFriendlyMarkers)
                {
                    if (closestMarker == null)
                    {
                        closestMarker = marker;
                    }
                    else if (closestMarker.HasValue)
                    {
                        Point3d p1 = new Point3d(marker.x, marker.y, marker.z);
                        Point3d p2 = new Point3d(closestMarker.Value.x, closestMarker.Value.y, closestMarker.Value.z);
                        if (groundGroup.Position.distance(ref p1) < groundGroup.Position.distance(ref p2))
                        {
                            closestMarker = marker;
                        }
                    }
                }

                if (closestMarker != null && closestMarker.HasValue)
                {
                    availableFriendlyMarkers.Remove(closestMarker.Value);

                    if (availableFriendlyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(availableFriendlyMarkers.Count);

                        groundGroup.Waypoints.Clear();

                        if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
                        {
                            findPath(groundGroup, new Point2d(closestMarker.Value.x, closestMarker.Value.y), new Point2d(availableFriendlyMarkers[markerIndex].x, availableFriendlyMarkers[markerIndex].y));
                            groundGroup.writeTo(missionFile);
                        }
                        else
                        {
                            Point2d start = new Point2d(closestMarker.Value.x, closestMarker.Value.y);
                            Point2d end = new Point2d(availableFriendlyMarkers[markerIndex].x, availableFriendlyMarkers[markerIndex].y);
                            findRoad(groundGroup, start, end, Waterways);

                            if (groundGroup.Waypoints.Count > 0)
                            {
                                //findPath(groundGroup, new Point2d(groundGroup.Waypoints[groundGroup.Waypoints.Count-1].Position.x,groundGroup.Waypoints[groundGroup.Waypoints.Count-1].Position.y), end);
                            }

                            if (groundGroup.Waypoints.Count >= 2)
                            {
                                int startIndex = rand.Next(groundGroup.Waypoints.Count - 2); // do not start at the last waypoint

                                groundGroup.Waypoints.RemoveRange(0, startIndex);

                                groundGroup.writeTo(missionFile);

                                for (int i = 1; i < 3; i++)
                                {
                                    double xOffset = -1.0;
                                    double yOffset = -1.0;

                                    bool subWaypointUsed = false;
                                    Point2d p1 = new Point2d(groundGroup.Waypoints[0].X, groundGroup.Waypoints[0].Y);
                                    if (groundGroup.Waypoints[0].SubWaypoints.Count > 0)
                                    {
                                        foreach (GroundGroupSubWaypoint subWaypoint in groundGroup.Waypoints[0].SubWaypoints)
                                        {
                                            if (subWaypoint.P.HasValue)
                                            {
                                                Point2d p2 = new Point2d(subWaypoint.P.Value.x, subWaypoint.P.Value.y);
                                                double distance = p1.distance(ref p2);
                                                xOffset = 500 * ((p2.x - p1.x) / distance);
                                                yOffset = 500 * ((p2.y - p1.y) / distance);
                                                subWaypointUsed = true;
                                                break;
                                            }
                                        }
                                    }
                                    if(subWaypointUsed == false)
                                    {
                                        Point2d p2 = new Point2d(groundGroup.Waypoints[1].X, groundGroup.Waypoints[1].Y);
                                        double distance = p1.distance(ref p2);
                                        xOffset = 500 * ((p2.x - p1.x) / distance);
                                        yOffset = 500 * ((p2.y - p1.y) / distance);
                                    }

                                    groundGroup.Waypoints[0].X += xOffset;
                                    groundGroup.Waypoints[0].Y += yOffset;

                                    subWaypointUsed = false;
                                    p1 = new Point2d(groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].X, groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].Y);
                                    if (groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].SubWaypoints.Count > 0)
                                    {
                                        for (int j = groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].SubWaypoints.Count-1; j >= 0; j--)
                                        {
                                            GroundGroupSubWaypoint subWaypoint = groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].SubWaypoints[j];
                                            if (subWaypoint.P.HasValue)
                                            {

                                                Point2d p2 = new Point2d(subWaypoint.P.Value.x, subWaypoint.P.Value.y);
                                                double distance = p1.distance(ref p2);
                                                xOffset = 500 * ((p2.x - p1.x) / distance);
                                                yOffset = 500 * ((p2.y - p1.y) / distance);
                                                subWaypointUsed = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (subWaypointUsed == false)
                                    {
                                        Point2d p2 = new Point2d(groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].X, groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].Y);
                                        double distance = p1.distance(ref p2);
                                        xOffset = 500 * ((p2.x - p1.x) / distance);
                                        yOffset = 500 * ((p2.y - p1.y) / distance);
                                    }

                                    groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].X -= xOffset;
                                    groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].Y -= yOffset;

                                    groundGroup._id = i.ToString() + "0" + groundGroup.Id;

                                    groundGroup.writeTo(missionFile);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<GroundGroup> getGroundGroups(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redGroundGroups;
            }
            else if (armyIndex == 2)
            {
                return blueGroundGroups;
            }
            else
            {
                return new List<GroundGroup>();
            }
        }

        private List<AirGroup> getAirGroups(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redAirGroups;
            }
            else if (armyIndex == 2)
            {
                return blueAirGroups;
            }
            else
            {
                return new List<AirGroup>();
            }
        }

        private List<Stationary> getFriendlyRadars(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redStationaries;
            }
            else if (armyIndex == 2)
            {
                return blueStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        private List<Stationary> getEnemyRadars(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return blueStationaries;
            }
            else if (armyIndex == 2)
            {
                return redStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        private List<Point3d> getFriendlyMarkers(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redFrontMarkers;
            }
            else if (armyIndex == 2)
            {
                return blueFrontMarkers;
            }
            else
            {
                return new List<Point3d>();
            }
        }

        private List<Point3d> getEnemyMarkers(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return blueFrontMarkers;
            }
            else if (armyIndex == 2)
            {
                return redFrontMarkers;
            }
            else
            {
                return new List<Point3d>();
            }
        }

        private List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in availableGroundGroups)
            {
                if (groundGroup.Army != armyIndex)
                {
                    groundGroups.Add(groundGroup);
                }            
            }
            return groundGroups;
        }
        
        private List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in availableGroundGroups)
            {
                if (groundGroup.Army == armyIndex)
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        private List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach(GroundGroup groundGroup in getAvailableEnemyGroundGroups(armyIndex))
            {
                if (groundGroupTypes.Contains(groundGroup.Type))
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        private List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in getAvailableFriendlyGroundGroups(armyIndex))
            {
                if (groundGroupTypes.Contains(groundGroup.Type))
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }
        
        private double getRandomAltitude(IAircraftParametersInfo missionParameters)
        {
            if (missionParameters.MinAltitude != null && missionParameters.MinAltitude.HasValue && missionParameters.MaxAltitude != null && missionParameters.MaxAltitude.HasValue)
            {
                return (double)rand.Next((int)missionParameters.MinAltitude.Value, (int)missionParameters.MaxAltitude.Value);
            }
            else
            {
                // Use some default altitudes
                return (double)rand.Next(300, 7000);
            }
        }
                
        private List<AirGroup> getAvailableOffensiveAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex != armyIndex)
                {
                    foreach (EMissionType missionType in airGroup.AircraftInfo.MissionTypes)
                    {
                        if (isMissionTypeOffensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private AirGroup getAvailableRandomOffensiveAirGroup(int armyIndex)
        {
            List<AirGroup> airGroups = getAvailableOffensiveAirGroups(armyIndex);
            
            if (airGroups.Count > 0)
            {
                int interceptedAirGroupIndex = rand.Next(airGroups.Count);
                AirGroup interceptedAirGroup = airGroups[interceptedAirGroupIndex];

                return interceptedAirGroup;
            }
            else
            {
                return null;
            }
        }

        private List<AirGroup> getAvailableEscortedAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex == armyIndex)
                {
                    foreach (EMissionType missionType in airGroup.AircraftInfo.MissionTypes)
                    {
                        if (isMissionTypeEscorted(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private AirGroup getAvailableRandomEscortedAirGroup(int armyIndex)
        {
            List<AirGroup> airGroups = getAvailableEscortedAirGroups(armyIndex);
            if (airGroups.Count > 0)
            {
                int escortedAirGroupIndex = rand.Next(airGroups.Count);
                AirGroup escortedAirGroup = airGroups[escortedAirGroupIndex];

                return escortedAirGroup;
            }
            else
            {
                return null;
            }
        }

        private AirGroup getAvailableRandomEscortAirGroup(AirGroup targetAirUnit)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex == targetAirUnit.ArmyIndex)
                {
                    if (airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.ESCORT))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

            if (airGroups.Count > 0)
            {
                int escortAirGroupIndex = rand.Next(airGroups.Count);
                AirGroup escortAirGroup = airGroups[escortAirGroupIndex];

                return escortAirGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex)
        {
            List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex, groundGroupTypes);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomFriendlyGroundGroup(int armyIndex)
        {
            List<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(armyIndex);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomFriendlyGroundGroup(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(armyIndex, groundGroupTypes);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private AirGroup getAvailableRandomInterceptAirGroup(AirGroup targetAirUnit)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex != targetAirUnit.ArmyIndex)
                {
                    if (airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.INTERCEPT))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

            if (airGroups.Count > 0)
            {
                int interceptAirGroupIndex = rand.Next(airGroups.Count);
                AirGroup interceptAirGroup = airGroups[interceptAirGroupIndex];

                return interceptAirGroup;
            }
            else
            {
                return null;
            }
        }

        private bool isMissionTypeEscorted(EMissionType missionType)
        {
            if(missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        private bool isMissionTypeOffensive(EMissionType missionType)
        {
            if(missionType == EMissionType.ARMED_MARITIME_RECON
                || missionType == EMissionType.ARMED_RECON
                || missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.MARITIME_RECON
                || missionType == EMissionType.RECON)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isMissionTypeAvailable(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.COVER)
            {
                List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ARMED_RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ATTACK_ARMOR)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor });
                if(groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ATTACK_RADAR)
            {
                List<Stationary> radars = getEnemyRadars(airGroup.ArmyIndex);
                if(radars.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ATTACK_SHIP)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ATTACK_VEHICLE)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Vehicle});
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.ESCORT)
            {
                List<AirGroup> airGroups = getAvailableEscortedAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.INTERCEPT)
            {
                List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.MARITIME_RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor, EGroundGroupType.Vehicle});
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void createBriefing(AirOperationTarget airOperationTarget, AirGroup airGroup, AirGroup escortAirGroup)
        {
            airGroup.Briefing = airOperationTarget.MissionType.ToString();

            if (escortAirGroup != null)
            {
                escortAirGroup.Briefing = EMissionType.ESCORT.ToString();
            }
        }

        private AirOperationTarget createAirOperation(ISectionFile sectionFile, IBriefingFile briefingFile, AirGroup airGroup, EMissionType missionType)
        {
            AirOperationTarget airOperationTarget = new AirOperationTarget();           

            if(isMissionTypeAvailable(airGroup, missionType))
            {
                availableAirGroups.Remove(airGroup);
                
                airOperationTarget.MissionType = missionType;

                List<IAircraftParametersInfo> aircraftParametersInfos = airGroup.AircraftInfo.GetAircraftParametersInfo(missionType);
                int aircraftParametersInfoIndex = rand.Next(aircraftParametersInfos.Count);
                IAircraftParametersInfo randomAircraftParametersInfo = aircraftParametersInfos[aircraftParametersInfoIndex];
                IAircraftLoadoutInfo aircraftLoadoutInfo = airGroup.AircraftInfo.GetAircraftLoadoutInfo(randomAircraftParametersInfo.LoadoutId);
                airGroup.Weapons = aircraftLoadoutInfo.Weapons;
                
                AirGroup escortAirGroup = null;
                if (isMissionTypeEscorted(missionType))
                {
                    escortAirGroup = getAvailableRandomEscortAirGroup(airGroup);
                    if (escortAirGroup != null)
                    {
                        availableAirGroups.Remove(escortAirGroup);

                        List<IAircraftParametersInfo> escortAircraftParametersInfos = escortAirGroup.AircraftInfo.GetAircraftParametersInfo(EMissionType.ESCORT);
                        int escortAircraftParametersInfoIndex = rand.Next(escortAircraftParametersInfos.Count);
                        IAircraftParametersInfo escortRandomAircraftParametersInfo = escortAircraftParametersInfos[escortAircraftParametersInfoIndex];
                        IAircraftLoadoutInfo escortAircraftLoadoutInfo = escortAirGroup.AircraftInfo.GetAircraftLoadoutInfo(escortRandomAircraftParametersInfo.LoadoutId);
                        escortAirGroup.Weapons = escortAircraftLoadoutInfo.Weapons;

                        escortAirGroup.Escort(sectionFile, airGroup);
                    }
                }

                if (missionType == EMissionType.COVER)
                {
                    AirGroup offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup.ArmyIndex);
                    if (offensiveAirGroup != null)
                    {
                        List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in offensiveAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(offensiveAirGroup, targetMissionType) && isMissionTypeOffensive(targetMissionType))
                            {
                                availableOffensiveMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableOffensiveMissionTypes.Count > 0)
                        {
                            int offensiveMissionTypeIndex = rand.Next(availableOffensiveMissionTypes.Count);
                            EMissionType randomOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];
                            AirOperationTarget offensiveAirOperationTarget = createAirOperation(sectionFile, briefingFile, offensiveAirGroup, randomOffensiveMissionType);

                            if (offensiveAirOperationTarget.TargetGroundGroup != null)
                            {
                                airGroup.Cover(sectionFile, offensiveAirOperationTarget.TargetGroundGroup, offensiveAirOperationTarget.Altitude);

                                airOperationTarget.TargetGroundGroup = offensiveAirOperationTarget.TargetGroundGroup;
                                airOperationTarget.Altitude = offensiveAirOperationTarget.Altitude;
                            }
                            else if (offensiveAirOperationTarget.TargetStationary != null)
                            {
                                airGroup.Cover(sectionFile, offensiveAirOperationTarget.TargetStationary, offensiveAirOperationTarget.Altitude);

                                airOperationTarget.TargetStationary = offensiveAirOperationTarget.TargetStationary;
                                airOperationTarget.Altitude = offensiveAirOperationTarget.Altitude;
                            }
                            else if (offensiveAirOperationTarget.TargetArea != null && offensiveAirOperationTarget.TargetArea.HasValue)
                            {
                                airGroup.Cover(sectionFile, offensiveAirOperationTarget.TargetArea.Value, offensiveAirOperationTarget.Altitude);

                                airOperationTarget.TargetArea = offensiveAirOperationTarget.TargetArea;
                                airOperationTarget.Altitude = offensiveAirOperationTarget.Altitude;
                            }
                        }
                    }
                }
                else if (missionType == EMissionType.ARMED_MARITIME_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                else if (missionType == EMissionType.ARMED_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor, EGroundGroupType.Vehicle});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                else if (missionType == EMissionType.ATTACK_ARMOR)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                else if (missionType == EMissionType.ATTACK_RADAR)
                {
                    List<Stationary> radars = getEnemyRadars(airGroup.ArmyIndex);
                    if (radars.Count > 0)
                    {
                        int radarIndex = rand.Next(radars.Count);
                        Stationary radar = radars[radarIndex];
                        double altitude = getRandomAltitude(randomAircraftParametersInfo);
                        
                        airGroup.GroundAttack(sectionFile, radar, altitude, escortAirGroup);
                        
                        airOperationTarget.TargetStationary = radar;
                        airOperationTarget.Altitude = altitude;
                    }
                }
                else if (missionType == EMissionType.ATTACK_SHIP)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                else if (missionType == EMissionType.ATTACK_VEHICLE)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Vehicle});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);
                    
                    airGroup.GroundAttack(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                else if (missionType == EMissionType.ESCORT)
                {
                    AirGroup escortedAirGroup = getAvailableRandomEscortedAirGroup(airGroup.ArmyIndex);
                    if (escortedAirGroup != null)
                    {
                        List<EMissionType> availableEscortedMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in escortedAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(escortedAirGroup, targetMissionType) && isMissionTypeEscorted(targetMissionType))
                            {
                                availableEscortedMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableEscortedMissionTypes.Count > 0)
                        {
                            int escortedMissionTypeIndex = rand.Next(availableEscortedMissionTypes.Count);
                            EMissionType randomEscortedMissionType = availableEscortedMissionTypes[escortedMissionTypeIndex];
                            AirOperationTarget targetAirOperation = createAirOperation(sectionFile, briefingFile, escortedAirGroup, randomEscortedMissionType);

                            airGroup.Escort(sectionFile, escortedAirGroup);

                            airOperationTarget.TargetAirGroup = escortedAirGroup;
                            airOperationTarget.Altitude = targetAirOperation.Altitude;
                        }
                    }
                }
                else if (missionType == EMissionType.INTERCEPT)
                {
                    AirGroup interceptedAirGroup = getAvailableRandomOffensiveAirGroup(airGroup.ArmyIndex);
                    if (interceptedAirGroup != null)
                    {
                        List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in interceptedAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(interceptedAirGroup, targetMissionType) && isMissionTypeOffensive(targetMissionType))
                            {
                                availableOffensiveMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableOffensiveMissionTypes.Count > 0)
                        {
                            int offensiveMissionTypeIndex = rand.Next(availableOffensiveMissionTypes.Count);
                            EMissionType randomOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];
                            AirOperationTarget offensiveAirOperationTarget = createAirOperation(sectionFile, briefingFile, interceptedAirGroup, randomOffensiveMissionType);

                            airGroup.Intercept(sectionFile, interceptedAirGroup);
                            
                            airOperationTarget.TargetAirGroup = interceptedAirGroup;
                            airOperationTarget.Altitude = offensiveAirOperationTarget.Altitude;
                        }
                    }
                }
                else if (missionType == EMissionType.MARITIME_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);

                    airGroup.Recon(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                else if (missionType == EMissionType.RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = getRandomAltitude(randomAircraftParametersInfo);

                    airGroup.Recon(sectionFile, groundGroup, altitude, escortAirGroup);

                    airOperationTarget.TargetGroundGroup = groundGroup;
                    airOperationTarget.Altitude = altitude;
                }
                                
                if (isMissionTypeOffensive(missionType))
                {
                    AirGroup interceptAirGroup = getAvailableRandomInterceptAirGroup(airGroup);
                    if (interceptAirGroup != null)
                    {
                        availableAirGroups.Remove(interceptAirGroup);

                        List<IAircraftParametersInfo> interceptAircraftParametersInfos = interceptAirGroup.AircraftInfo.GetAircraftParametersInfo(EMissionType.INTERCEPT);
                        int interceptAircraftParametersInfoIndex = rand.Next(interceptAircraftParametersInfos.Count);
                        IAircraftParametersInfo interceptRandomAircraftParametersInfo = interceptAircraftParametersInfos[interceptAircraftParametersInfoIndex];
                        IAircraftLoadoutInfo interceptAircraftLoadoutInfo = interceptAirGroup.AircraftInfo.GetAircraftLoadoutInfo(interceptRandomAircraftParametersInfo.LoadoutId);
                        interceptAirGroup.Weapons = interceptAircraftLoadoutInfo.Weapons;

                        interceptAirGroup.Intercept(sectionFile, airGroup);                        
                    }
                }

                createBriefing(airOperationTarget, airGroup, escortAirGroup);
            }
            else
            {
                throw new ArgumentException(missionType.ToString());
            }

            return airOperationTarget;
        }

        private void createRandomAirOperation(ISectionFile sectionFile, IBriefingFile briefingFile, AirGroup airGroup)
        {
            List<EMissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
            if (missionTypes != null && missionTypes.Count > 0)
            {
                List<EMissionType> availableMissionTypes = new List<EMissionType>();
                foreach(EMissionType missionType in missionTypes)
                {
                    if(isMissionTypeAvailable(airGroup, missionType))
                    {
                        availableMissionTypes.Add(missionType);
                    }
                }

                if (availableMissionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = rand.Next(availableMissionTypes.Count);
                    EMissionType randomMissionType = availableMissionTypes[randomMissionTypeIndex];

                    createAirOperation(sectionFile, briefingFile, airGroup, randomMissionType);
                }
            }
        }
    }
}