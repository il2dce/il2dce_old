﻿// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
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
        [Serializable] 
        public class Core
        {
            #region Public variables

            public static ISectionFile debug = null;

            public static bool spawnParked = false;

            public static Random rand = new Random();

            public static List<AirGroup> availableAirGroups = new List<AirGroup>();

            public int maxRandomSpawn = 1;

            public int? playerSquadronIndex = null;
            public int? playerFlightIndex = null;
            public int? playerAircraftIndex = null;
            public string playerAirGroupKey = null;
            public AirGroup playerAirGroup = null;


            public ISectionFile airGroupsTemplate;
            public List<AirGroup> redAirGroups = new List<AirGroup>();
            public List<AirGroup> blueAirGroups = new List<AirGroup>();

            public ISectionFile markersTemplate;
            public List<Point3d> redMarkers = new List<Point3d>();
            public List<Point3d> blueMarkers = new List<Point3d>();
            public List<Point3d> neutralMarkers = new List<Point3d>();

            public ISectionFile radarsTemplate;
            public List<Radar> redRadars = new List<Radar>();
            public List<Radar> blueRadars = new List<Radar>();

            #endregion

            public Core(IGame game)
            {
                Game = game;
            }

            public IGame Game
            {
                get;
                set;
            }

            public void Init()
            {
                debug = Game.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/Main.mis");

                radarsTemplate = Game.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/__Radars.mis");

                for (int i = 0; i < radarsTemplate.lines("Stationary"); i++)
                {
                    string key;
                    string value;
                    radarsTemplate.get("Stationary", i, out key, out value);

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

                markersTemplate = Game.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/__Markers.mis");

                for (int i = 0; i < markersTemplate.lines("FrontMarker"); i++)
                {
                    string key;
                    string value;
                    markersTemplate.get("FrontMarker", i, out key, out value);

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

                airGroupsTemplate = Game.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/__AirGroups.mis");

                availableAirGroups.Clear();

                for (int i = 0; i < airGroupsTemplate.lines("AirGroups"); i++)
                {
                    string key;
                    string value;
                    airGroupsTemplate.get("AirGroups", i, out key, out value);

                    AirGroup airGroup = new AirGroup(airGroupsTemplate, key);
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

                if (airGroupsTemplate.exist("MAIN", "player"))
                {
                    string playerAircraftId = airGroupsTemplate.get("MAIN", "player");

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
                                airGroup.Flight.Length > playerFlightIndex &&
                                airGroup.Flight[(int)playerFlightIndex] != null &&
                                airGroup.Flight[(int)playerFlightIndex].Length > playerAircraftIndex)
                            {
                                playerAirGroup = airGroup;
                            }
                        }
                        foreach (AirGroup airGroup in getAirGroups(2))
                        {
                            if (airGroup.AirGroupKey == playerAirGroupKey &&
                                airGroup.SquadronIndex == playerSquadronIndex &&
                                airGroup.Flight.Length > playerFlightIndex &&
                                airGroup.Flight[(int)playerFlightIndex] != null &&
                                airGroup.Flight[(int)playerFlightIndex].Length > playerAircraftIndex)
                            {
                                playerAirGroup = airGroup;
                            }
                        }
                    }
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

            public double createRandomAltitude(AircraftInfo.MissionType missionType)
            {
                // TODO: Altitude range depends on mission type.
                return (double)rand.Next(500, 6000);
            }

            public AirGroup getRandomInterceptedFlight(AirGroup interceptingAirUnit, AircraftInfo.MissionType missionType)
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
                        if (airGroup.AircraftInfo.MissionTypes.Contains(AircraftInfo.MissionType.GROUND_ATTACK_AREA))
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
                        if (airGroup.AircraftInfo.MissionTypes.Contains(AircraftInfo.MissionType.ESCORT))
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
                        if (airGroup.AircraftInfo.MissionTypes.Contains(AircraftInfo.MissionType.INTERCEPT))
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
                    //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + targetAirUnit.Name + ")", null);
                }
            }

            public void createRandomFlight(ISectionFile sectionFile, AirGroup airGroup)
            {
                List<AircraftInfo.MissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
                if (missionTypes != null && missionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = rand.Next(missionTypes.Count);
                    AircraftInfo.MissionType randomMissionType = missionTypes[randomMissionTypeIndex];

                    // Bomber mission types
                    if (randomMissionType == AircraftInfo.MissionType.RECON_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                            airGroup.CreateReconFlight(sectionFile, targetArea);

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);

                            //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Recon flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                        }
                    }
                    else if (randomMissionType == AircraftInfo.MissionType.GROUND_ATTACK_AREA)
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

                                //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ") with " + escortAirGroup.Name + ": Escort flight(" + airGroup.Name + ")", null);
                            }
                            else
                            {
                                airGroup.CreateGroundAttackFlight(sectionFile, targetArea);
                                //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                            }

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
                    else if (randomMissionType == AircraftInfo.MissionType.OFFENSIVE_PATROL_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                            airGroup.CreateHuntingFlight(sectionFile, targetArea);

                            //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                        }
                    }
                    else if (randomMissionType == AircraftInfo.MissionType.ESCORT)
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

                                //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Escort flight(" + airGroup.Name + ") for " + escortedAirGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                                createRandomInterceptFlight(sectionFile, escortedAirGroup, targetArea);
                            }
                            else
                            {
                                airGroup.CreateHuntingFlight(sectionFile, targetArea);

                                //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": No escort required. Instead offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                                createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                            }
                        }
                    }
                    else if (randomMissionType == AircraftInfo.MissionType.INTERCEPT)
                    {
                        List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                        if (friendlyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(friendlyMarkers.Count);
                            Point3d marker = friendlyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            List<AircraftInfo.MissionType> subMissionTypes = new List<AircraftInfo.MissionType>() { AircraftInfo.MissionType.OFFENSIVE_PATROL_AREA, AircraftInfo.MissionType.RECON_AREA, AircraftInfo.MissionType.GROUND_ATTACK_AREA };
                            int randomSubMissionTypeIndex = rand.Next(subMissionTypes.Count);
                            AircraftInfo.MissionType randomSubMissionType = subMissionTypes[randomSubMissionTypeIndex];

                            AirGroup interceptedAirGroup = getRandomInterceptedFlight(airGroup, randomSubMissionType);
                            if (interceptedAirGroup != null)
                            {
                                availableAirGroups.Remove(interceptedAirGroup);

                                if (randomSubMissionType == AircraftInfo.MissionType.OFFENSIVE_PATROL_AREA)
                                {
                                    interceptedAirGroup.CreateHuntingFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == AircraftInfo.MissionType.RECON_AREA)
                                {
                                    interceptedAirGroup.CreateReconFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == AircraftInfo.MissionType.GROUND_ATTACK_AREA)
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
                                //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Intercept flight(" + interceptedAirGroup.Name + ")", null);
                            }
                            else
                            {
                                airGroup.CreateCoverFlight(sectionFile, targetArea);
                                //GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": No intercept required. Instead defensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                            }
                        }
                    }
                }
            }
        }
    }
}