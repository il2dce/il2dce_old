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
        public Random rand = new Random();

        private List<AirGroup> availableAirGroups = new List<AirGroup>();
        private List<GroundGroup> availableGroundGroups = new List<GroundGroup>();

        private int additionalAirOperations = 0;
        private int additionalGroundOperations = 0;

        private List<Radar> redRadars = new List<Radar>();
        private List<Radar> blueRadars = new List<Radar>();

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
                            ISectionFile localAircraftInfoFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + campaignFolder.Name + "/AircraftInfo.ini");

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

        public System.Collections.Generic.IList<Road> Roads
        {
            get
            {
                return _roads;
            }
        }
        System.Collections.Generic.List<Road> _roads = new System.Collections.Generic.List<Road>();

        public System.Collections.Generic.IList<Road> Waterways
        {
            get
            {
                return _waterways;
            }
        }
        System.Collections.Generic.List<Road> _waterways = new System.Collections.Generic.List<Road>();

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
            }
            else
            {
                Career.Date = Career.Date.Value.Add(new TimeSpan(1, 0, 0, 0));
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
            redRadars.Clear();
            blueRadars.Clear();
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
                        Radar radar = new Radar(key, x, y);
                        redRadars.Add(radar);
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
                    Road road = new Road(templateFile, key);
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


        public void generate(string templateFileName, string missionId, out ISectionFile missionFile, out IBriefingFile briefingFile)
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
                if(airGroup.Name == Career.AirGroup)
                {
                    string playerAirGroupKey = airGroup.AirGroupKey;
                    int playerSquadronIndex = airGroup.SquadronIndex;
                    int playerFlightIndex = airGroup.Flights.Count-1;
                    int playerAircraftIndex = airGroup.Flights[playerFlightIndex].Count - 1;

                    if (missionFile.exist("MAIN", "player"))
                    {
                        missionFile.set("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString() + playerFlightIndex.ToString() + playerAircraftIndex.ToString());
                    }
                    else
                    {
                        missionFile.add("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString() + playerFlightIndex.ToString() + playerAircraftIndex.ToString());
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
                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Wait for path.", null);
                    System.Threading.Thread.Sleep(100);
                }

                if (pathParams.State == RecalcPathState.SUCCESS)
                {
                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path found (" + pathParams.Path.Length.ToString() + ").", null);

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
                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path not found.", null);
                }
            }
        }

        private void findRoad(GroundGroup groundGroup, Point2d start, Point2d end, IList<Road> roads)
        {
            if (roads != null && roads.Count > 0)
            {
                Road closestRoad = null;
                double closestRoadDistance = 0.0;
                foreach (Road road in roads)
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

                    List<Road> availableRoads = new List<Road>(roads);
                    availableRoads.Remove(closestRoad);

                    findRoad(groundGroup, new Point2d(closestRoad.End.Position.x, closestRoad.End.Position.y), end, availableRoads);
                }
            }
        }

        public void createRandomGroundOperation(ISectionFile missionFile, GroundGroup groundGroup)
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
        
        public List<GroundGroup> getGroundGroups(int armyIndex)
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

        public List<AirGroup> getAirGroups(int armyIndex)
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

        public List<Radar> getFriendlyRadars(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redRadars;
            }
            else if (armyIndex == 2)
            {
                return blueRadars;
            }
            else
            {
                return new List<Radar>();
            }
        }

        public List<Radar> getEnemyRadars(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return blueRadars;
            }
            else if (armyIndex == 2)
            {
                return redRadars;
            }
            else
            {
                return new List<Radar>();
            }
        }

        public List<Point3d> getFriendlyMarkers(int armyIndex)
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

        public List<Point3d> getEnemyMarkers(int armyIndex)
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

        public List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex)
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
        
        public List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex)
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

        public List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
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

        public List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
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
        
        public double createRandomAltitude(EMissionType missionType, IAircraftInfo aircraftInfo)
        {
            // TODO: Altitude range depends on mission type.
            return (double)rand.Next(500, 6000);
        }

        public List<AirGroup> getAvailableInterceptedAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex != armyIndex)
                {
                    foreach (EMissionType missionType in airGroup.AircraftInfo.MissionTypes)
                    {
                        if (isMissionTypeIntercepted(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        public AirGroup getAvailableRandomInterceptedAirGroup(int armyIndex)
        {
            List<AirGroup> airGroups = getAvailableInterceptedAirGroups(armyIndex);
            
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

        public List<AirGroup> getAvailableEscortedAirGroups(int armyIndex)
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

        public AirGroup getAvailableRandomEscortedAirGroup(int armyIndex)
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

        public AirGroup getAvailableRandomEscortAirGroup(AirGroup targetAirUnit)
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

        public GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex)
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

        public GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex, List<EGroundGroupType> groundGroupTypes)
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

        public GroundGroup getAvailableRandomFriendlyGroundGroup(int armyIndex)
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

        public GroundGroup getAvailableRandomFriendlyGroundGroup(int armyIndex, List<EGroundGroupType> groundGroupTypes)
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

        public AirGroup getAvailableRandomInterceptAirGroup(AirGroup targetAirUnit)
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

        public bool isMissionTypeEscorted(EMissionType missionType)
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

        public bool isMissionTypeIntercepted(EMissionType missionType)
        {
            if(missionType == EMissionType.ARMED_MARITIME_RECON
                || missionType == EMissionType.ARMED_RECON
                || missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.LIASON
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

        public bool isMissionTypeAvailable(AirGroup airGroup, EMissionType missionType)
        {
            if(missionType == EMissionType.ARMED_MARITIME_RECON)
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
                List<Radar> radars = getEnemyRadars(airGroup.ArmyIndex);
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
                List<AirGroup> airGroups = getAvailableInterceptedAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(missionType == EMissionType.LIASON)
            {
                return false;
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

        public void createAirOperation(ISectionFile sectionFile, IBriefingFile briefingFile, AirGroup airGroup, EMissionType missionType)
        {
            if(isMissionTypeAvailable(airGroup, missionType))
            {
                availableAirGroups.Remove(airGroup);

                AirGroup escortAirGroup = null;
                if (isMissionTypeEscorted(missionType))
                {
                    escortAirGroup = getAvailableRandomEscortAirGroup(airGroup);
                    if (escortAirGroup != null)
                    {
                        availableAirGroups.Remove(escortAirGroup);
                        escortAirGroup.CreateEscortFlight(sectionFile, airGroup);
                    }
                }


                if (missionType == EMissionType.ARMED_MARITIME_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                    createRandomGroundOperation(sectionFile, groundGroup);

                    airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                else if (missionType == EMissionType.ARMED_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor, EGroundGroupType.Vehicle});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    
                    airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_ARMOR)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Armor});
                    createRandomGroundOperation(sectionFile, groundGroup);

                    airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_RADAR)
                {
                    List<Radar> radars = getEnemyRadars(airGroup.ArmyIndex);
                    if (radars.Count > 0)
                    {
                        int radarIndex = rand.Next(radars.Count);
                        Radar radar = radars[radarIndex];
                        
                        airGroup.CreateGroundAttackRadarMission(sectionFile, radar, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                    }
                }
                else if (missionType == EMissionType.ATTACK_SHIP)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                    createRandomGroundOperation(sectionFile, groundGroup);

                    airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_VEHICLE)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Vehicle});
                    createRandomGroundOperation(sectionFile, groundGroup);
                    
                    airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                else if (missionType == EMissionType.ESCORT)
                {
                    AirGroup targetAirGroup = getAvailableRandomEscortedAirGroup(airGroup.ArmyIndex);
                    if (targetAirGroup != null)
                    {
                        List<EMissionType> availableTargetMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in targetAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(targetAirGroup, targetMissionType) && isMissionTypeEscorted(targetMissionType))
                            {
                                availableTargetMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableTargetMissionTypes.Count > 0)
                        {
                            int targetMissionTypeIndex = rand.Next(availableTargetMissionTypes.Count);
                            EMissionType randomTargetMissionType = availableTargetMissionTypes[targetMissionTypeIndex];
                            createAirOperation(sectionFile, briefingFile, targetAirGroup, randomTargetMissionType);

                            airGroup.CreateEscortFlight(sectionFile, targetAirGroup);
                        }
                    }
                }
                else if (missionType == EMissionType.INTERCEPT)
                {
                    AirGroup targetAirGroup = getAvailableRandomInterceptedAirGroup(airGroup.ArmyIndex);
                    if (targetAirGroup != null)
                    {
                        List<EMissionType> availableTargetMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in targetAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(targetAirGroup, targetMissionType) && isMissionTypeIntercepted(targetMissionType))
                            {
                                availableTargetMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableTargetMissionTypes.Count > 0)
                        {
                            int targetMissionTypeIndex = rand.Next(availableTargetMissionTypes.Count);
                            EMissionType randomTargetMissionType = availableTargetMissionTypes[targetMissionTypeIndex];
                            createAirOperation(sectionFile, briefingFile, targetAirGroup, randomTargetMissionType);

                            airGroup.CreateInterceptFlight(sectionFile, targetAirGroup);
                        }
                    }
                }
                else if (missionType == EMissionType.LIASON)
                {

                }
                else if (missionType == EMissionType.MARITIME_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> {EGroundGroupType.Ship});
                    createRandomGroundOperation(sectionFile, groundGroup);

                    airGroup.CreateReconTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                else if (missionType == EMissionType.RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                    createRandomGroundOperation(sectionFile, groundGroup);

                    airGroup.CreateReconTargetMission(sectionFile, groundGroup, createRandomAltitude(missionType, airGroup.AircraftInfo), escortAirGroup);
                }
                                
                if (isMissionTypeIntercepted(missionType))
                {
                    AirGroup interceptAirGroup = getAvailableRandomInterceptAirGroup(airGroup);
                    if (interceptAirGroup != null)
                    {
                        availableAirGroups.Remove(interceptAirGroup);
                        interceptAirGroup.CreateInterceptFlight(sectionFile, airGroup);                        
                    }
                }
            }
            else
            {
                throw new ArgumentException(missionType.ToString());
            }
        }

        public void createRandomAirOperation(ISectionFile sectionFile, IBriefingFile briefingFile, AirGroup airGroup)
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
                    airGroup.Briefing = airGroup.Name;

                    int randomMissionTypeIndex = rand.Next(availableMissionTypes.Count);
                    EMissionType randomMissionType = availableMissionTypes[randomMissionTypeIndex];

                    createAirOperation(sectionFile, briefingFile, airGroup, randomMissionType);

                    /*
                    // Bomber mission types
                    if (randomMissionType == EMissionType.RECON_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType, airGroup.AircraftInfo));
                            airGroup.CreateReconFlight(sectionFile, targetArea);

                            briefingFile.Name[airGroup.Name] = airGroup.Name;
                            briefingFile.Description[airGroup.Name] = "Recon area " + targetArea.x + "," + targetArea.y + "," + targetArea.z + ".";

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Recon flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                            AirGroup interceptAirGroup = getRandomInterceptAirGroup(airGroup);
                            if (interceptAirGroup != null)
                            {
                                availableAirGroups.Remove(interceptAirGroup);
                                interceptAirGroup.CreateInterceptFlight(sectionFile, airGroup);

                                briefingFile.Name[interceptAirGroup.Name] = interceptAirGroup.Name;
                                briefingFile.Description[interceptAirGroup.Name] = "Intercept " + airGroup.Name + ".";

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + airGroup.Name + ")", null);
                            }
                        }
                        else
                        {
                            briefingFile.Name[airGroup.Name] = airGroup.Name;
                            briefingFile.Description[airGroup.Name] = "Circuit.";

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Circuit", null);

                            airGroup.CreateTransferFlight(sectionFile);
                        }
                    }
                    //else if (randomMissionType == EMissionType.GROUND_ATTACK_AREA)
                    //{
                    //    List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                    //    if (enemyMarkers.Count > 0)
                    //    {
                    //        int markerIndex = rand.Next(enemyMarkers.Count);
                    //        Point3d marker = enemyMarkers[markerIndex];
                    //        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType, airGroup.AircraftInfo));

                    //        AirGroup escortAirGroup = getRandomEscortAirGroup(airGroup);
                    //        if (escortAirGroup != null)
                    //        {
                    //            availableAirGroups.Remove(escortAirGroup);

                    //            Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                    //            airGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                    //            escortAirGroup.CreateEscortFlight(sectionFile, airGroup);
                    //        }
                    //        else
                    //        {
                    //            airGroup.CreateGroundAttackFlight(sectionFile, targetArea);
                    //        }

                    //        briefingFile.Name[airGroup.Name] = airGroup.Name;
                    //        briefingFile.Description[airGroup.Name] = "Attack area " + targetArea.x + "," + targetArea.y + "," + targetArea.z + ".";

                    //        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                    //        AirGroup interceptAirGroup = getRandomInterceptAirGroup(airGroup);
                    //        if (interceptAirGroup != null)
                    //        {
                    //            availableAirGroups.Remove(interceptAirGroup);
                    //            interceptAirGroup.CreateInterceptFlight(sectionFile, airGroup);

                    //            briefingFile.Name[interceptAirGroup.Name] = interceptAirGroup.Name;
                    //            briefingFile.Description[interceptAirGroup.Name] = "Intercept " + airGroup.Name + ".";

                    //            Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + airGroup.Name + ")", null);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        briefingFile.Name[airGroup.Name] = airGroup.Name;
                    //        briefingFile.Description[airGroup.Name] = "Circuit.";

                    //        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Circuit", null);

                    //        airGroup.CreateTransferFlight(sectionFile);
                    //    }
                    //}
                    else if (randomMissionType == EMissionType.GROUND_ATTACK_TARGET)
                    {
                        GroundGroup groundGroup = getRandomEnemyGroundGroup(airGroup.ArmyIndex);
                        if (groundGroup != null)
                        {
                            availableGroundGroups.Remove(groundGroup);
                            createRandomGroundOperation(sectionFile, groundGroup);

                            double altitude = createRandomAltitude(randomMissionType, airGroup.AircraftInfo);

                            AirGroup escortAirGroup = getRandomEscortAirGroup(airGroup);
                            if (escortAirGroup != null)
                            {
                                availableAirGroups.Remove(escortAirGroup);
                                Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortAirGroup.Position.y - airGroup.Position.y), altitude);
                                airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, altitude, rendevouzPosition);
                                escortAirGroup.CreateEscortFlight(sectionFile, airGroup);
                            }
                            else
                            {
                                airGroup.CreateGroundAttackTargetMission(sectionFile, groundGroup, altitude);
                            }

                            briefingFile.Name[airGroup.Name] = airGroup.Name;
                            briefingFile.Description[airGroup.Name] = "Attack " + groundGroup.Id + ".";

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + groundGroup.Id + ")", null);

                            AirGroup interceptAirGroup = getRandomInterceptAirGroup(airGroup);
                            if (interceptAirGroup != null)
                            {
                                availableAirGroups.Remove(interceptAirGroup);
                                interceptAirGroup.CreateInterceptFlight(sectionFile, airGroup);

                                briefingFile.Name[interceptAirGroup.Name] = interceptAirGroup.Name;
                                briefingFile.Description[interceptAirGroup.Name] = "Intercept " + airGroup.Name + ".";

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + airGroup.Name + ")", null);
                            }
                        }
                        else
                        {
                            briefingFile.Name[airGroup.Name] = airGroup.Name;
                            briefingFile.Description[airGroup.Name] = "Circuit.";

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Circuit", null);

                            airGroup.CreateTransferFlight(sectionFile);
                        }
                    }

                    // Fighter mission types
                    //else if (randomMissionType == AircraftInfo.MissionType.DEFENSIVE_PATROL_AREA)
                    //{
                    //    List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                    //    if (friendlyMarkers.Count > 0)
                    //    {
                    //        int markerIndex = rand.Next(friendlyMarkers.Count);
                    //        Point3d marker = friendlyMarkers[markerIndex];
                    //        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                    //        airGroup.CreateCoverFlight(sectionFile, targetArea);

                    //        GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Defensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                    //    }
                    //}
                    //else if (randomMissionType == EMissionType.OFFENSIVE_PATROL_AREA)
                    //{
                    //    List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                    //    if (enemyMarkers.Count > 0)
                    //    {
                    //        int markerIndex = rand.Next(enemyMarkers.Count);
                    //        Point3d marker = enemyMarkers[markerIndex];
                    //        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType, airGroup.AircraftInfo));
                    //        airGroup.CreateHuntingFlight(sectionFile, targetArea);

                    //        briefingFile.Name[airGroup.Name] = airGroup.Name;
                    //        briefingFile.Description[airGroup.Name] = "Offensive patrol area " + targetArea.x + "," + targetArea.y + "," + targetArea.z + ".";

                    //        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                    //        AirGroup interceptAirGroup = getRandomInterceptAirGroup(airGroup);
                    //        if (interceptAirGroup != null)
                    //        {
                    //            availableAirGroups.Remove(interceptAirGroup);
                    //            interceptAirGroup.CreateInterceptFlight(sectionFile, airGroup);

                    //            briefingFile.Name[interceptAirGroup.Name] = interceptAirGroup.Name;
                    //            briefingFile.Description[interceptAirGroup.Name] = "Intercept " + airGroup.Name + ".";

                    //            Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + airGroup.Name + ")", null);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        briefingFile.Name[airGroup.Name] = airGroup.Name;
                    //        briefingFile.Description[airGroup.Name] = "Circuit.";

                    //        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Circuit", null);

                    //        airGroup.CreateTransferFlight(sectionFile);
                    //    }
                    //}
                    else if (randomMissionType == EMissionType.ESCORT)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType, airGroup.AircraftInfo));

                            AirGroup escortedAirGroup = getRandomEscortedAirGroup(airGroup.ArmyIndex);
                            if (escortedAirGroup != null)
                            {
                                availableAirGroups.Remove(escortedAirGroup);

                                Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortedAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortedAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                                escortedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                                airGroup.CreateEscortFlight(sectionFile, escortedAirGroup);

                                briefingFile.Name[airGroup.Name] = airGroup.Name;
                                briefingFile.Description[airGroup.Name] = "Escort " + escortedAirGroup.Name + ".";

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Escort flight(" + escortedAirGroup.Name + ")", null);

                                AirGroup interceptAirGroup = getRandomInterceptAirGroup(escortedAirGroup);
                                if (interceptAirGroup != null)
                                {
                                    availableAirGroups.Remove(interceptAirGroup);
                                    interceptAirGroup.CreateInterceptFlight(sectionFile, escortedAirGroup);

                                    briefingFile.Name[interceptAirGroup.Name] = interceptAirGroup.Name;
                                    briefingFile.Description[interceptAirGroup.Name] = "Intercept " + airGroup.Name + ".";

                                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + escortedAirGroup.Name + ")", null);
                                }
                            }
                            else
                            {
                                airGroup.CreateHuntingFlight(sectionFile, targetArea);

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": No escort required. Instead offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                                AirGroup interceptAirGroup = getRandomInterceptAirGroup(airGroup);
                                if (interceptAirGroup != null)
                                {
                                    availableAirGroups.Remove(interceptAirGroup);
                                    interceptAirGroup.CreateInterceptFlight(sectionFile, airGroup);

                                    briefingFile.Name[interceptAirGroup.Name] = interceptAirGroup.Name;
                                    briefingFile.Description[interceptAirGroup.Name] = "Intercept " + airGroup.Name + ".";

                                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + airGroup.Name + ")", null);
                                }
                            }
                        }
                        else
                        {
                            briefingFile.Name[airGroup.Name] = airGroup.Name;
                            briefingFile.Description[airGroup.Name] = "Circuit.";

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Circuit", null);

                            airGroup.CreateTransferFlight(sectionFile);
                        }
                    }
                    else if (randomMissionType == EMissionType.INTERCEPT)
                    {
                        List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                        if (friendlyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(friendlyMarkers.Count);
                            Point3d marker = friendlyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType, airGroup.AircraftInfo));

                            List<EMissionType> subMissionTypes = new List<EMissionType>() { EMissionType.RECON_AREA, EMissionType.GROUND_ATTACK_TARGET };
                            int randomSubMissionTypeIndex = rand.Next(subMissionTypes.Count);
                            EMissionType randomSubMissionType = subMissionTypes[randomSubMissionTypeIndex];

                            AirGroup interceptedAirGroup = getRandomInterceptedAirGroup(airGroup.ArmyIndex);
                            if (interceptedAirGroup != null)
                            {
                                availableAirGroups.Remove(interceptedAirGroup);

                                if (randomSubMissionType == EMissionType.OFFENSIVE_PATROL_AREA)
                                {
                                    interceptedAirGroup.CreateHuntingFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == EMissionType.RECON_AREA)
                                {
                                    interceptedAirGroup.CreateReconFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == EMissionType.GROUND_ATTACK_AREA)
                                {
                                    AirGroup escortAirGroup = getRandomEscortAirGroup(interceptedAirGroup);
                                    if (escortAirGroup != null)
                                    {
                                        availableAirGroups.Remove(escortAirGroup);

                                        Point3d rendevouzPosition = new Point3d(interceptedAirGroup.Position.x + 0.50 * (escortAirGroup.Position.x - interceptedAirGroup.Position.x), interceptedAirGroup.Position.y + 0.50 * (escortAirGroup.Position.y - interceptedAirGroup.Position.y), targetArea.z);

                                        interceptedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                                        escortAirGroup.CreateEscortFlight(sectionFile, interceptedAirGroup);
                                    }
                                    else
                                    {
                                        interceptedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea);
                                    }
                                }

                                airGroup.CreateInterceptFlight(sectionFile, interceptedAirGroup);

                                briefingFile.Name[airGroup.Name] = airGroup.Name;
                                briefingFile.Description[airGroup.Name] = "Intercept " + interceptedAirGroup.Name + ".";

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Intercept flight(" + interceptedAirGroup.Name + ")", null);
                            }
                            else
                            {
                                airGroup.CreateCoverFlight(sectionFile, targetArea);

                                briefingFile.Name[airGroup.Name] = airGroup.Name;
                                briefingFile.Description[airGroup.Name] = "Defensive patrol area " + targetArea.x + "," + targetArea.y + "," + targetArea.z + ".";

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": No intercept required. Instead defensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                            }
                        }
                        else
                        {
                            briefingFile.Name[airGroup.Name] = airGroup.Name;
                            briefingFile.Description[airGroup.Name] = "Circuit.";

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Circuit", null);

                            airGroup.CreateTransferFlight(sectionFile);
                        }
                    }*/
                }
            }
        }
    }
}