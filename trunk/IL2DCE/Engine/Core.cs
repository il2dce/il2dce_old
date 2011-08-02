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
    namespace Engine
    {
        // Looks like the mission requires the core to be serializable. As core is not used in the mission at the moment there is no need to make it serializable.
        //[Serializable]
        public class Core : ICore
        {
            private Random rand = new Random();

            private List<AirGroup> availableAirGroups = new List<AirGroup>();

            private int randomSpawn = 0;

            private List<Point3d> redMarkers = new List<Point3d>();
            private List<Point3d> blueMarkers = new List<Point3d>();
            private List<Point3d> neutralMarkers = new List<Point3d>();

            private List<Radar> redRadars = new List<Radar>();
            private List<Radar> blueRadars = new List<Radar>();

            public Core(IGame game, ISectionFile confFile)
            {
                _game = game;
                if (confFile.exist("Core", "setOnPark"))
                {
                    string value = confFile.get("Core", "setOnPark");
                    if (value == "1")
                    {
                        SpawnParked = true;
                    }
                    else
                    {
                        SpawnParked = false;
                    }
                }

                if (confFile.exist("Core", "randomSpawn"))
                {
                    string value = confFile.get("Core", "randomSpawn");
                    int.TryParse(value, out randomSpawn);
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
            }

            public IGame Game
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

            public void Init(string templateFileName)
            {
                redRadars.Clear();
                blueRadars.Clear();
                redMarkers.Clear();
                blueMarkers.Clear();
                redAirGroups.Clear();
                blueAirGroups.Clear();
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
                                neutralMarkers.Add(new Point3d(x, y, 0.0));
                            }
                            else if (army == 1)
                            {
                                redMarkers.Add(new Point3d(x, y, 0.0));
                            }
                            else if (army == 2)
                            {
                                blueMarkers.Add(new Point3d(x, y, 0.0));
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


            public ISectionFile Generate(string templateFileName)
            {
                availableAirGroups.Clear();
                foreach (AirGroup airGroup in AirGroups)
                {
                    availableAirGroups.Add(airGroup);
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

                    createRandomFlight(missionFile, playerAirGroup);
                }

                for (int i = 0; i < randomSpawn; i++)
                {
                    int randomAirGroupIndex = rand.Next(availableAirGroups.Count);
                    Engine.AirGroup randomAirGroup = availableAirGroups[randomAirGroupIndex];
                    availableAirGroups.Remove(randomAirGroup);

                    createRandomFlight(missionFile, randomAirGroup);
                }

                return missionFile;
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
                    return redMarkers;
                }
                else if (armyIndex == 2)
                {
                    return blueMarkers;
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
                    return blueMarkers;
                }
                else if (armyIndex == 2)
                {
                    return redMarkers;
                }
                else
                {
                    return new List<Point3d>();
                }
            }

            public double createRandomAltitude(MissionType missionType)
            {
                // TODO: Altitude range depends on mission type.
                return (double)rand.Next(500, 6000);
            }

            public AirGroup getRandomInterceptedFlight(AirGroup interceptingAirUnit, MissionType missionType)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex != interceptingAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(missionType))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (escortAirGroup == null)
                            //{
                            //    escortAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    Point3d targetAirUnitPos = targetAirUnit.Position;
                            //    if (airGroup.Position.distance(ref targetAirUnitPos) < escortAirGroup.Position.distance(ref targetAirUnitPos))
                            //    {
                            //        escortAirGroup = airGroup;
                            //    }
                            //}
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

            public AirGroup getRandomEscortedFlight(AirGroup escortingAirUnit)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex == escortingAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(MissionType.GROUND_ATTACK_AREA))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (escortAirGroup == null)
                            //{
                            //    escortAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    Point3d targetAirUnitPos = targetAirUnit.Position;
                            //    if (airGroup.Position.distance(ref targetAirUnitPos) < escortAirGroup.Position.distance(ref targetAirUnitPos))
                            //    {
                            //        escortAirGroup = airGroup;
                            //    }
                            //}
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

            public AirGroup getRandomEscortFlight(AirGroup targetAirUnit)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex == targetAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(MissionType.ESCORT))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (escortAirGroup == null)
                            //{
                            //    escortAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    Point3d targetAirUnitPos = targetAirUnit.Position;
                            //    if (airGroup.Position.distance(ref targetAirUnitPos) < escortAirGroup.Position.distance(ref targetAirUnitPos))
                            //    {
                            //        escortAirGroup = airGroup;
                            //    }
                            //}
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

            public void createRandomInterceptFlight(ISectionFile sectionFile, AirGroup targetAirUnit, Point3d targetArea)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex != targetAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(MissionType.INTERCEPT))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (interceptAirGroup == null)
                            //{
                            //    interceptAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    if (airGroup.Position.distance(ref targetArea) < interceptAirGroup.Position.distance(ref targetArea))
                            //    {
                            //        interceptAirGroup = airGroup;
                            //    }
                            //}
                        }
                    }
                }

                if (airGroups.Count > 0)
                {
                    int interceptAirGroupIndex = rand.Next(airGroups.Count);
                    AirGroup interceptAirGroup = airGroups[interceptAirGroupIndex];

                    availableAirGroups.Remove(interceptAirGroup);

                    interceptAirGroup.CreateInterceptFlight(sectionFile, targetAirUnit, targetArea);
                    
                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + targetAirUnit.Name + ")", null);
                }
            }

            public void createRandomFlight(ISectionFile sectionFile, AirGroup airGroup)
            {
                List<MissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
                if (missionTypes != null && missionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = rand.Next(missionTypes.Count);
                    MissionType randomMissionType = missionTypes[randomMissionTypeIndex];

                    // Bomber mission types
                    if (randomMissionType == MissionType.RECON_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                            airGroup.CreateReconFlight(sectionFile, targetArea);

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Recon flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                        }
                    }
                    else if (randomMissionType == MissionType.GROUND_ATTACK_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            AirGroup escortAirGroup = getRandomEscortFlight(airGroup);
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

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);
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
                    else if (randomMissionType == MissionType.OFFENSIVE_PATROL_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                            airGroup.CreateHuntingFlight(sectionFile, targetArea);

                            Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                        }
                    }
                    else if (randomMissionType == MissionType.ESCORT)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            AirGroup escortedAirGroup = getRandomEscortedFlight(airGroup);
                            if (escortedAirGroup != null)
                            {
                                availableAirGroups.Remove(escortedAirGroup);

                                Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortedAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortedAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                                escortedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                                airGroup.CreateEscortFlight(sectionFile, escortedAirGroup);

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": Escort flight(" + escortedAirGroup.Name + ")", null);

                                createRandomInterceptFlight(sectionFile, escortedAirGroup, targetArea);
                            }
                            else
                            {
                                airGroup.CreateHuntingFlight(sectionFile, targetArea);

                                Game.gpLogServer(new Player[] { Game.gpPlayer() }, airGroup.Name + ": No escort required. Instead offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                                createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                            }
                        }
                    }
                    else if (randomMissionType == MissionType.INTERCEPT)
                    {
                        List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                        if (friendlyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(friendlyMarkers.Count);
                            Point3d marker = friendlyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            List<MissionType> subMissionTypes = new List<MissionType>() { MissionType.OFFENSIVE_PATROL_AREA, MissionType.RECON_AREA, MissionType.GROUND_ATTACK_AREA };
                            int randomSubMissionTypeIndex = rand.Next(subMissionTypes.Count);
                            MissionType randomSubMissionType = subMissionTypes[randomSubMissionTypeIndex];

                            AirGroup interceptedAirGroup = getRandomInterceptedFlight(airGroup, randomSubMissionType);
                            if (interceptedAirGroup != null)
                            {
                                availableAirGroups.Remove(interceptedAirGroup);

                                if (randomSubMissionType == MissionType.OFFENSIVE_PATROL_AREA)
                                {
                                    interceptedAirGroup.CreateHuntingFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == MissionType.RECON_AREA)
                                {
                                    interceptedAirGroup.CreateReconFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == MissionType.GROUND_ATTACK_AREA)
                                {
                                    AirGroup escortAirGroup = getRandomEscortFlight(interceptedAirGroup);
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

                                airGroup.CreateInterceptFlight(sectionFile, interceptedAirGroup, targetArea);

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
}