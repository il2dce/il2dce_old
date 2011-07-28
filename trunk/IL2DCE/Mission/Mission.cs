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
using maddox.game.world;
using maddox.GP;

namespace IL2DCE
{
    namespace Mission
    {
        public class Mission : AMission
        {
            Engine.Core Core
            {
                get
                {
                    Game.GameSingle game = GamePlay as Game.GameSingle;
                    return game.Core;
                }
            }

            public override void OnBattleStarted()
            {
                base.OnBattleStarted();

                GamePlay.gpPostMissionLoad(Core.radarsTemplate);
                GamePlay.gpPostMissionLoad(Core.markersTemplate);

                // Assign the closest airport to each air unit.
                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int armyIndex in GamePlay.gpArmies())
                    {
                        foreach (Engine.AirGroup airGroup in Core.getAirGroups(armyIndex))
                        {
                            // Check type of waypoint. LANDING = use airport; NORMALFLY = use position
                            if (airGroup.Waypoints.Count > 0 && airGroup.Waypoints[0].Type == Engine.AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF)
                            {
                                // Find the closest airport.
                                AiAirport closestAirport = null;
                                foreach (AiAirport airport in GamePlay.gpAirports())
                                {
                                    if (closestAirport == null)
                                    {
                                        closestAirport = airport;
                                    }
                                    else
                                    {
                                        Point3d airportPos = airport.Pos();
                                        Point3d closestAirportPos = closestAirport.Pos();

                                        Point3d p = new Point3d(airGroup.Waypoints[0].X, airGroup.Waypoints[0].Y, airGroup.Waypoints[0].Z);
                                        if (p.distance(ref airportPos) < p.distance(ref closestAirportPos))
                                        {
                                            closestAirport = airport;
                                        }
                                    }
                                }
                                airGroup.Airport = closestAirport;
                            }
                        }
                    }
                }

                ISectionFile sectionFile = GamePlay.gpCreateSectionFile();
                // Generate flights
                if (Core.playerAirGroup != null)
                {
                    Engine.Core.availableAirGroups.Remove(Core.playerAirGroup);

                    Core.createRandomFlight(sectionFile, Core.playerAirGroup);
                }

                for (int i = 0; i < Core.maxRandomSpawn; i++)
                {
                    int randomAirGroupIndex = Engine.Core.rand.Next(Engine.Core.availableAirGroups.Count);
                    Engine.AirGroup randomAirGroup = Engine.Core.availableAirGroups[randomAirGroupIndex];
                    Engine.Core.availableAirGroups.Remove(randomAirGroup);

                    Core.createRandomFlight(sectionFile, randomAirGroup);
                }

                GamePlay.gpPostMissionLoad(sectionFile);

                #if DEBUG

                Engine.Core.debug.save("$user/missions/__Debug.mis");

                #endif
            }

            public override void OnMissionLoaded(int missionNumber)
            {
                base.OnMissionLoaded(missionNumber);

                placePlayer();
            }

            public void placePlayer()
            {
                // Place player
                if (GamePlay.gpPlayer().Place() == null && Core.playerAirGroupKey != null && Core.playerSquadronIndex != null && Core.playerFlightIndex != null && Core.playerAircraftIndex != null)
                {
                    if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                    {
                        foreach (int armyIndex in GamePlay.gpArmies())
                        {
                            if (GamePlay.gpAirGroups(armyIndex) != null && GamePlay.gpAirGroups(armyIndex).Length > 0)
                            {
                                foreach (AiAirGroup aiAirGroup in GamePlay.gpAirGroups(armyIndex))
                                {
                                    if (aiAirGroup.GetItems() != null && aiAirGroup.GetItems().Length > 0)
                                    {
                                        foreach (AiActor aiActor in aiAirGroup.GetItems())
                                        {
                                            if (aiActor.Name().Remove(0, aiActor.Name().IndexOf(":") + 1) == Core.playerAirGroupKey + "." + Core.playerSquadronIndex.Value.ToString() + Core.playerFlightIndex.Value.ToString() + Core.playerAircraftIndex.Value.ToString())
                                            {
                                                GamePlay.gpPlayer().PlaceEnter(aiActor, 0);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public override void OnTickGame()
            {
                base.OnTickGame();
            }
        }
    }
}