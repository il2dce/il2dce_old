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

        public Core(GameDef game)
        {
            _game = game;

            Game.EventChat += new GameDef.Chat(Game_EventChat);

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

            if (confFile.exist("MAIN", "campaignsFolder"))
            {
                string campaignsFolderPath = confFile.get("MAIN", "campaignsFolder");
                string campaignsFolderSystemPath = game.gameInterface.ToFileSystemPath(campaignsFolderPath);

                System.IO.DirectoryInfo campaignsFolder = new System.IO.DirectoryInfo(campaignsFolderSystemPath);
                if (campaignsFolder.GetDirectories() != null && campaignsFolder.GetDirectories().Length > 0)
                {
                    foreach (System.IO.DirectoryInfo campaignFolder in campaignsFolder.GetDirectories())
                    {
                        if (campaignFolder.GetFiles("campaign.ini") != null && campaignFolder.GetFiles("campaign.ini").Length == 1)
                        {
                            ISectionFile campaignFile = game.gameInterface.SectionFileLoad(campaignsFolderPath + "/" + campaignFolder.Name + "/campaign.ini");

                            Campaign campaign = new Campaign(campaignsFolderPath + "/" + campaignFolder.Name + "/", campaignFile);
                            Campaigns.Add(campaign);
                        }
                    }
                }
            }
        }

        void Game_EventChat(IPlayer from, string msg)
        {
            if(msg.Contains("!hello"))
            {
                Game.gpLogServer(new Player[] { from }, "Hello World!", null);
            }
        }

        public ICampaign CurrentCampaign
        {
            get
            {
                return _currentCampaign;
            }
            set
            {
                if (_currentCampaign != value)
                {
                    _currentCampaign = value;
                    init(_currentCampaign.TemplateFilePath);
                }
            }
        }
        private ICampaign _currentCampaign;

        public List<ICampaign> Campaigns
        {
            get
            {
                return campaigns;
            }
        }
        private List<ICampaign> campaigns = new List<ICampaign>();

        private GameDef Game
        {
            get
            {
                return _game;
            }
        }
        private GameDef _game;

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

        public System.Collections.Generic.IList<Road> Routes
        {
            get
            {
                return _routes;
            }
        }
        System.Collections.Generic.List<Road> _routes = new System.Collections.Generic.List<Road>();

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

        public int? PlayerSquadronIndex
        {
            get
            {
                return playerSquadronIndex;
            }
            set
            {
                playerSquadronIndex = value;
            }
        }

        public int? PlayerFlightIndex
        {
            get
            {
                return playerFlightIndex;
            }
            set
            {
                playerFlightIndex = value;
            }
        }

        public int? PlayerAircraftIndex
        {
            get
            {
                return playerAircraftIndex;
            }
            set
            {
                playerAircraftIndex = value;
            }
        }

        public string PlayerAirGroupKey
        {
            get
            {
                return playerAirGroupKey;
            }
            set
            {
                playerAirGroupKey = value;
            }
        }

        public IAirGroup PlayerAirGroup
        {
            get
            {
                return playerAirGroup;
            }
            set
            {
                playerAirGroup = value as AirGroup;
            }
        }

        public int? playerSquadronIndex = null;
        public int? playerFlightIndex = null;
        public int? playerAircraftIndex = null;
        public string playerAirGroupKey = null;
        public AirGroup playerAirGroup = null;

        public ISectionFile StartCampaign()
        {
            return ContinueCampaign();
        }

        public ISectionFile ContinueCampaign()
        {
            ISectionFile missionFile = generate(CurrentCampaign.TemplateFilePath);

#if DEBUG
            string debugPath = Game.gameInterface.ToFileSystemPath("$user/missions/IL2DCE/Debug");
            if (!System.IO.Directory.Exists(debugPath))
            {
                System.IO.Directory.CreateDirectory(debugPath);
            }
            missionFile.save("$user/missions/IL2DCE/Debug/IL2DCEDebug.mis");
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
                }
#endif

            return missionFile;
        }

        private void init(string templateFileName)
        {
            _roads.Clear();
            redRadars.Clear();
            blueRadars.Clear();
            redFrontMarkers.Clear();
            blueFrontMarkers.Clear();
            redAirGroups.Clear();
            blueAirGroups.Clear();
            redGroundGroups.Clear();
            blueGroundGroups.Clear();
            playerSquadronIndex = null;
            playerFlightIndex = null;
            playerAircraftIndex = null;
            playerAirGroupKey = null;
            playerAirGroup = null;

            ISectionFile templateFile = Game.gpLoadSectionFile(templateFileName);

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

                AirGroup airGroup = new AirGroup(templateFile, key);
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
                        _routes.Add(road);
                    }
                }
            }

            if (templateFile.exist("MAIN", "player"))
            {
                string playerAircraftId = templateFile.get("MAIN", "player");

                int result;
                int.TryParse(playerAircraftId.Substring(playerAircraftId.LastIndexOf(".") + 1, 1), out result);
                playerSquadronIndex = result;
                int.TryParse(playerAircraftId.Substring(playerAircraftId.LastIndexOf(".") + 2, 1), out result);
                playerFlightIndex = result;
                int.TryParse(playerAircraftId.Substring(playerAircraftId.LastIndexOf(".") + 3, 1), out result);
                playerAircraftIndex = result;

                playerAirGroupKey = playerAircraftId.Substring(playerAircraftId.IndexOf(":") + 1, playerAircraftId.LastIndexOf(".") - playerAircraftId.IndexOf(":") - 1);

                // Find the air group of the player.
                if (playerAirGroupKey != null && playerSquadronIndex != null && playerFlightIndex != null && playerAircraftIndex != null)
                {
                    foreach (AirGroup airGroup in getAirGroups(1))
                    {
                        if (airGroup.AirGroupKey == playerAirGroupKey &&
                            airGroup.SquadronIndex == playerSquadronIndex &&
                            airGroup.Flights.Count > playerFlightIndex &&
                            airGroup.Flights.ContainsKey((int)playerFlightIndex) &&
                            airGroup.Flights[(int)playerFlightIndex] != null &&
                            airGroup.Flights[(int)playerFlightIndex].Count > playerAircraftIndex)
                        {
                            playerAirGroup = airGroup;
                        }
                    }
                    foreach (AirGroup airGroup in getAirGroups(2))
                    {
                        if (airGroup.AirGroupKey == playerAirGroupKey &&
                            airGroup.SquadronIndex == playerSquadronIndex &&
                            airGroup.Flights.Count > playerFlightIndex &&
                            airGroup.Flights.ContainsKey((int)playerFlightIndex) &&
                            airGroup.Flights[(int)playerFlightIndex] != null &&
                            airGroup.Flights[(int)playerFlightIndex].Count > playerAircraftIndex)
                        {
                            playerAirGroup = airGroup;
                        }
                    }
                }
            }
        }


        public ISectionFile generate(string templateFileName)
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

            ISectionFile missionFile = Game.gpLoadSectionFile(templateFileName);

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

            // Delete all ground groups from the template file.
            for (int i = 0; i < missionFile.lines("Chiefs"); i++)
            {
                string key;
                string value;
                missionFile.get("Chiefs", i, out key, out value);
                missionFile.delete(key + "_Road");
            }
            missionFile.delete("Chiefs");

            if (playerAirGroupKey != null && playerSquadronIndex != null && playerFlightIndex != null && playerAircraftIndex != null)
            {
                if (missionFile.exist("MAIN", "player"))
                {
                    missionFile.set("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString() + playerFlightIndex.ToString() + playerAircraftIndex.ToString());
                }
                else
                {
                    missionFile.add("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString() + playerFlightIndex.ToString() + playerAircraftIndex.ToString());
                }
            }

            if (playerAirGroup != null)
            {
                availableAirGroups.Remove(playerAirGroup);

                createRandomAirOperation(missionFile, playerAirGroup);
            }

            for (int i = 0; i < additionalAirOperations; i++)
            {
                int randomAirGroupIndex = rand.Next(availableAirGroups.Count);
                AirGroup randomAirGroup = availableAirGroups[randomAirGroupIndex];
                availableAirGroups.Remove(randomAirGroup);

                createRandomAirOperation(missionFile, randomAirGroup);
            }

            for (int i = 0; i < additionalGroundOperations; i++)
            {
                int randomGroundGroupIndex = rand.Next(availableGroundGroups.Count);
                GroundGroup randomGroundGroup = availableGroundGroups[randomGroundGroupIndex];
                availableGroundGroups.Remove(randomGroundGroup);

                createRandomGroundOperation(missionFile, randomGroundGroup);
            }            

            return missionFile;
        }

        public void FindPath(GroundGroup groundGroup, Point2d start, Point2d end, IList<Road> roads)
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
                    groundGroup.Waypoints.AddRange(closestRoad.Waypoints);

                    List<Road> availableRoads = new List<Road>(roads);
                    availableRoads.Remove(closestRoad);

                    FindPath(groundGroup, new Point2d(closestRoad.End.Position.x, closestRoad.End.Position.y), end, availableRoads);
                }                
            }

        }

        public void createRandomGroundOperation(ISectionFile missionFile, GroundGroup groundGroup)
        {
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

                        if (groundGroup.Type != EGroundGroupType.Ship)
                        {
                            FindPath(groundGroup, new Point2d(closestMarker.Value.x, closestMarker.Value.y), new Point2d(availableFriendlyMarkers[markerIndex].x, availableFriendlyMarkers[markerIndex].y), Roads);
                        }
                        else
                        {
                            FindPath(groundGroup, new Point2d(closestMarker.Value.x, closestMarker.Value.y), new Point2d(availableFriendlyMarkers[markerIndex].x, availableFriendlyMarkers[markerIndex].y), Routes);
                        }

                        groundGroup.writeTo(missionFile, Roads);
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

        public List<Radar> getRadars(int armyIndex)
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

        public List<GroundGroup> getEnemyGroundGroups(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return blueGroundGroups;
            }
            else if (armyIndex == 2)
            {
                return redGroundGroups;
            }
            else
            {
                return new List<GroundGroup>();
            }
        }
        
        public List<GroundGroup> getFriendlyGroundGroups(int armyIndex)
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

        public double createRandomAltitude(EMissionType missionType)
        {
            // TODO: Altitude range depends on mission type.
            return (double)rand.Next(500, 6000);
        }

        public AirGroup getRandomInterceptedMission(AirGroup interceptingAirUnit, EMissionType missionType)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex != interceptingAirUnit.ArmyIndex)
                {
                    if (airGroup.AircraftInfo.MissionTypes.Contains(missionType))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

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

        public AirGroup getRandomEscortedMission(AirGroup escortingAirUnit)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex == escortingAirUnit.ArmyIndex)
                {
                    if (airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.GROUND_ATTACK_AREA)
                        || airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.GROUND_ATTACK_TARGET))
                    {
                        airGroups.Add(airGroup);
                    }                    
                }
            }

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

        public AirGroup getRandomEscortMission(AirGroup targetAirUnit)
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

        public void createRandomInterceptMission(ISectionFile sectionFile, AirGroup targetAirUnit)
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

                availableAirGroups.Remove(interceptAirGroup);

                interceptAirGroup.CreateInterceptFlight(sectionFile, targetAirUnit);

                Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + targetAirUnit.Name + ")", null);
            }
        }

        public void createRandomAirOperation(ISectionFile sectionFile, AirGroup airGroup)
        {
            List<EMissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
            if (missionTypes != null && missionTypes.Count > 0)
            {
                int randomMissionTypeIndex = rand.Next(missionTypes.Count);
                EMissionType randomMissionType = missionTypes[randomMissionTypeIndex];

                // Bomber mission types
                if (randomMissionType == EMissionType.RECON_AREA)
                {
                    List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                    if (enemyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(enemyMarkers.Count);
                        Point3d marker = enemyMarkers[markerIndex];
                        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                        airGroup.CreateReconFlight(sectionFile, targetArea);

                        createRandomInterceptMission(sectionFile, airGroup);

                        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Recon flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                    }
                }
                else if (randomMissionType == EMissionType.GROUND_ATTACK_AREA)
                {
                    List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                    if (enemyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(enemyMarkers.Count);
                        Point3d marker = enemyMarkers[markerIndex];
                        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                        AirGroup escortAirGroup = getRandomEscortMission(airGroup);
                        if (escortAirGroup != null)
                        {
                            availableAirGroups.Remove(escortAirGroup);

                            Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                            airGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                            escortAirGroup.CreateEscortFlight(sectionFile, airGroup);
                        }
                        else
                        {
                            airGroup.CreateGroundAttackFlight(sectionFile, targetArea);
                        }

                        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                        createRandomInterceptMission(sectionFile, airGroup);
                    }
                }
                else if (randomMissionType == EMissionType.GROUND_ATTACK_TARGET)
                {
                    List<GroundGroup> enemyGroundGroups = getEnemyGroundGroups(airGroup.ArmyIndex);
                    if (enemyGroundGroups.Count > 0)
                    {
                        int groundGroupIndex = rand.Next(enemyGroundGroups.Count);
                        GroundGroup groundGroup = enemyGroundGroups[groundGroupIndex];
                        availableGroundGroups.Remove(groundGroup);
                        createRandomGroundOperation(sectionFile, groundGroup);
                        
                        double altitude = createRandomAltitude(randomMissionType);

                        AirGroup escortAirGroup = getRandomEscortMission(airGroup);
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

                        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + groundGroup.Id + ")", null);

                        createRandomInterceptMission(sectionFile, airGroup);
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
                else if (randomMissionType == EMissionType.OFFENSIVE_PATROL_AREA)
                {
                    List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                    if (enemyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(enemyMarkers.Count);
                        Point3d marker = enemyMarkers[markerIndex];
                        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                        airGroup.CreateHuntingFlight(sectionFile, targetArea);

                        Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                        createRandomInterceptMission(sectionFile, airGroup);
                    }
                }
                else if (randomMissionType == EMissionType.ESCORT)
                {
                    List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                    if (enemyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(enemyMarkers.Count);
                        Point3d marker = enemyMarkers[markerIndex];
                        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                        AirGroup escortedAirGroup = getRandomEscortedMission(airGroup);
                        if (escortedAirGroup != null)
                        {
                            availableAirGroups.Remove(escortedAirGroup);

                            Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortedAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortedAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                            escortedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                            airGroup.CreateEscortFlight(sectionFile, escortedAirGroup);

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Escort flight(" + escortedAirGroup.Name + ")", null);

                            createRandomInterceptMission(sectionFile, escortedAirGroup);
                        }
                        else
                        {
                            airGroup.CreateHuntingFlight(sectionFile, targetArea);

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": No escort required. Instead offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                            createRandomInterceptMission(sectionFile, airGroup);
                        }
                    }
                }
                else if (randomMissionType == EMissionType.INTERCEPT)
                {
                    List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                    if (friendlyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(friendlyMarkers.Count);
                        Point3d marker = friendlyMarkers[markerIndex];
                        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                        List<EMissionType> subMissionTypes = new List<EMissionType>() { EMissionType.OFFENSIVE_PATROL_AREA, EMissionType.RECON_AREA, EMissionType.GROUND_ATTACK_AREA };
                        int randomSubMissionTypeIndex = rand.Next(subMissionTypes.Count);
                        EMissionType randomSubMissionType = subMissionTypes[randomSubMissionTypeIndex];

                        AirGroup interceptedAirGroup = getRandomInterceptedMission(airGroup, randomSubMissionType);
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
                                AirGroup escortAirGroup = getRandomEscortMission(interceptedAirGroup);
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

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Intercept flight(" + interceptedAirGroup.Name + ")", null);
                        }
                        else
                        {
                            airGroup.CreateCoverFlight(sectionFile, targetArea);

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": No intercept required. Instead defensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                        }
                    }
                }
            }
        }
    }
}