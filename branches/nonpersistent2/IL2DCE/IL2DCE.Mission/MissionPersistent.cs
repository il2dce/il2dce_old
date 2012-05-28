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
    namespace MissionOld
    {
        public class MissionPersistent : AMission
        {
            public string missionFileName = null;
            public string missionFolderName = null;

            protected ICore Core
            {
                get
                {
                    return this.core;
                }
            }
            private ICore core;

            private System.Collections.Generic.Dictionary<AiGroup, GroundGroup> groundGroupProxies = new System.Collections.Generic.Dictionary<AiGroup, GroundGroup>();
            private ISectionFile triggerFile;

            public override void OnBattleInit()
            {
                base.OnBattleInit();

                ISectionFile confFile = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/conf.ini");
                string campaignsFolderPath = confFile.get("Main", "campaignsFolder");
                string campaignsFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\missions\IL2DCE\Campaigns";
                string careersFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\mission\IL2DCE";
                string debugFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\missions\IL2DCE\Debug";

                this.core = new Core(GamePlay, confFile, campaignsFolderSystemPath, careersFolderSystemPath, debugFolderSystemPath);
                core.CurrentCareer = core.Careers[0];
                
                ISectionFile missionFile = GamePlay.gpLoadSectionFile(missionFileName);
                
                core.Generator.Init(missionFile);

                this.triggerFile = GamePlay.gpCreateSectionFile();

                foreach(FrontMarker frontMarker in this.Core.Generator.FrontMarkers)
                {
                    triggerFile.add("Trigger", "changeArmy" + this.Core.Generator.FrontMarkers.IndexOf(frontMarker).ToString() + "_1", " TPassThrough 3 1 " + frontMarker.Position.x + " " + frontMarker.Position.y + " 500");
                    triggerFile.add("Trigger", "changeArmy" + this.Core.Generator.FrontMarkers.IndexOf(frontMarker).ToString() + "_2", " TPassThrough 3 2 " + frontMarker.Position.x + " " + frontMarker.Position.y + " 500");
                }
            }

            public override void OnBattleStarted()
            {
                base.OnBattleStarted();

                MissionNumberListener = -1;
                GamePlay.gpPostMissionLoad(triggerFile);

                Timeout((10), () =>
                {
                    UpdateWaypoints();
                });

                Timeout((20), () =>
                {
                    SpawnGroundGroups();
                });

                Timeout((30), () =>
                {
                    SpawnAirGroups();
                });
            }
            
            public override void OnActorCreated(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorCreated(missionNumber, shortName, actor);
                
                if (actor is AiGroup)
                {
                    foreach (GroundGroup groundGroup in this.core.Generator.GroundGroups)
                    {
                        string actorName = actor.Name();
                        actorName = actorName.Remove(0, actorName.IndexOf(":") + 1);

                        if (groundGroup.Id == actorName)
                        {
                            groundGroupProxies.Add(actor as AiGroup, groundGroup);
                            groundGroup.AiGroup = actor as AiGroup;
                            this.Core.Generator.Created(groundGroup);
                            break;
                        }
                    }
                }
            }

            public override void OnActorDestroyed(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorDestroyed(missionNumber, shortName, actor);

                if (actor is AiGroup)
                {
                    this.Core.Generator.Destroyed(groundGroupProxies[actor as AiGroup]);
                    groundGroupProxies[actor as AiGroup].AiGroup = null;
                    groundGroupProxies[actor as AiGroup].Fails = 0;
                    groundGroupProxies[actor as AiGroup].Stuck = false;
                    groundGroupProxies[actor as AiGroup].Target = null;

                    groundGroupProxies.Remove(actor as AiGroup);
                }
            }

            public override void OnActorTaskCompleted(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorTaskCompleted(missionNumber, shortName, actor);

                if (actor is AiGroup)
                {
                    if (groundGroupProxies.ContainsKey(actor as AiGroup))
                    {
                        this.Core.Generator.GenerateGroundOperation(groundGroupProxies[actor as AiGroup]);
                    }
                }
            }

            public void UpdateWaypoints()
            {
                // Ground
                if (groundGroupProxies.Count > 0)
                {
                    foreach (AiGroup aiGroup in groundGroupProxies.Keys)
                    {
                        Point2d currentPosition = new Point2d(aiGroup.Pos().x, aiGroup.Pos().y);
                        GroundGroup groundGroup = groundGroupProxies[aiGroup];
                        if (groundGroup.PathParams != null)
                        {
                            if (groundGroup.PathParams.State == RecalcPathState.SUCCESS)
                            {
                                aiGroup.SetWay(groundGroup.PathParams.Path);
                                groundGroup.PathParams = null;
                                groundGroup.Fails = 0;
                                groundGroup.Target = null;

                                this.Core.Debug(aiGroup.Name() + " new path.");
                            }
                            else if (groundGroup.PathParams.State == RecalcPathState.FAILED)
                            {
                                groundGroup.PathParams = null;
                                groundGroup.Fails++;
                                this.Core.Debug(aiGroup.Name() + " path failed (" + groundGroup.Fails.ToString() + ").");

                                this.Core.Generator.GenerateGroundOperation(groundGroup);
                            }
                        }
                        else
                        {
                            if (groundGroup.LastPosition.distance(ref currentPosition) < 1)
                            {
                                // Check for stuck ground groups.
                                if (groundGroup.Type != EGroundGroupType.Ship)
                                {
                                    this.Core.Debug(aiGroup.Name() + " is stuck.");
                                    groundGroup.Stuck = true;
                                    this.Core.Generator.GenerateGroundOperation(groundGroup);
                                }
                                // This is also used for ships as they don't seem to fire the OnActorTaskComplete event.
                                else if (groundGroup.Type == EGroundGroupType.Ship)
                                {
                                    this.Core.Debug(aiGroup.Name() + " task complete (Ship).");
                                    this.Core.Generator.GenerateGroundOperation(groundGroup);
                                }
                            }

                            // TODO: Check for disconnected trailers.
                        }

                        groundGroup.LastPosition = currentPosition;
                    }
                }

                // Air
                this.Core.Generator.UpdateWaypoints();

                // Delay
                Timeout((1 * 60), () =>
                {
                    UpdateWaypoints();
                });
            }

            public void SpawnAirGroups()
            {
                ISectionFile missionFile = this.Core.GamePlay.gpCreateSectionFile();
                IBriefingFile briefingFile = new BriefingFile();
                Core.Generator.GenerateRandomAirOperation(missionFile, briefingFile);

                for (int airGroupIndex = 0; airGroupIndex < missionFile.lines("AirGroups"); airGroupIndex++)
                {
                    string airGroupKey;
                    string value;
                    missionFile.get("AirGroups", airGroupIndex, out airGroupKey, out value);

                    if (!(missionFile.get(airGroupKey, "Idle") == "1"))
                    {
                        missionFile.set(airGroupKey, "Idle", true);
                    }
                }

                string briefingFileSystemPath = missionFolderName + "mission.briefing";
                missionFile.save(missionFolderName + "mission.mis");
                briefingFile.save(briefingFileSystemPath);

                int missionNumber = GamePlay.gpNextMissionNumber();
                GamePlay.gpPostMissionLoad(missionFolderName + "mission.mis");

                Timeout((5 * 60), () =>
                {
                    RemoveIdle(missionNumber);
                });

                Timeout((15 * 60), () =>
                {
                    SpawnAirGroups();
                });
            }

            public void RemoveIdle(int missionNumber)
            {
                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int army in GamePlay.gpArmies())
                    {
                        if (GamePlay.gpAirGroups(army) != null && GamePlay.gpAirGroups(army).Length > 0)
                        {
                            foreach (AiAirGroup airGroup in GamePlay.gpAirGroups(army))
                            {
                                if (airGroup.Name().StartsWith(missionNumber + ":"))
                                {
                                    airGroup.Idle = false;
                                }
                            }
                        }
                    }
                }
            }

            public void SpawnGroundGroups()
            {
                ISectionFile groundMissionFile = Core.Generator.GenerateRandomGroundOperation();
                if (groundMissionFile != null)
                {
                    GamePlay.gpPostMissionLoad(groundMissionFile);
                }

                Timeout((5 * 60), () =>
                {
                    SpawnGroundGroups();
                });
            }

            internal ISectionFile CreateNewFrontLineMission(int markerNum, int newArmy)
            {
                this.Core.Generator.FrontMarkers[markerNum].Army = newArmy;

                ISectionFile f = GamePlay.gpCreateSectionFile();
                string sect;
                string key;
                string value;

                for (int i = 0; i < this.Core.Generator.FrontMarkers.Count; i++)
                {
                    sect = "FrontMarker";
                    key = "FrontMarker" + i.ToString();
                    value = this.Core.Generator.FrontMarkers[i].Position.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + this.Core.Generator.FrontMarkers[i].Position.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + this.Core.Generator.FrontMarkers[i].Army.ToString();
                    f.add(sect, key, value);
                }
                return f;
            }

            public override void OnTrigger(int missionNumber, string shortName, bool active)
            {
                base.OnTrigger(missionNumber, shortName, active);

                for (int i = 0; i < this.Core.Generator.FrontMarkers.Count; i++)
                {
                    for (int j = 1; j < 3; j++)
                    {
                        string str = "changeArmy" + i.ToString() + "_" + (j).ToString();
                        if (str.Equals(shortName))
                        {
                            if (this.Core.Generator.FrontMarkers[i].Army != j)
                            {
                                GamePlay.gpPostMissionLoad(CreateNewFrontLineMission(i, j));
                            }
                            break;
                        }
                    }
                }
            }
            
            public override void OnPlayerArmy(Player player, int army)
            {
                base.OnPlayerArmy(player, army);

                assignToLobbyAircraft(player);
            }

            private void assignToLobbyAircraft(Player player)
            {
                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int army in GamePlay.gpArmies())
                    {
                        if (GamePlay.gpAirGroups(army) != null && GamePlay.gpAirGroups(army).Length > 0)
                        {
                            foreach (AiAirGroup airGroup in GamePlay.gpAirGroups(army))
                            {
                                // Lobby aircrafts always have the mission index 0.
                                if (airGroup.Name().StartsWith("0:"))
                                {
                                    if (airGroup.GetItems() != null && airGroup.GetItems().Length > 0)
                                    {
                                        foreach (AiActor actor in airGroup.GetItems())
                                        {
                                            if (actor is AiAircraft)
                                            {
                                                AiAircraft aircraft = actor as AiAircraft;
                                                for (int placeIndex = 0; placeIndex < aircraft.Places(); placeIndex++)
                                                {
                                                    if (aircraft.Player(placeIndex) == null)
                                                    {
                                                        player.PlaceEnter(aircraft, placeIndex);
                                                        // Place found.
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
                }
                
                GamePlay.gpLogServer(new Player[] { player }, "No unoccupied place available in the lobby aircrafts.", null);
            }

            public override void OnPlaceEnter(Player player, AiActor actor, int placeIndex)
            {
                base.OnPlaceEnter(player, actor, placeIndex);

                if (actor.Name().StartsWith("0:"))
                {
                    menuOffsets[player] = 0;
                    SetMenu(player);
                    GamePlay.gpHUDLogCenter(new Player[] { player }, "Use 'Mission Menu' (TAB + 4) to select aircraft.");
                }
                else
                {
                    menuOffsets[player] = 0;
                    SetEmptyMenu(player);
                    GamePlay.gpHUDLogCenter(new Player[] { player }, "Select army to return to aircraft selection.");
                }
            }

            private Dictionary<Player, int> menuOffsets = new Dictionary<Player, int>();

            public void SetEmptyMenu(Player player)
            {
                GamePlay.gpSetOrderMissionMenu(player, false, 1, new string[] { }, new bool[] { });
            }

            public void SetMenu(Player player)
            {
                int entryCount = 9;
                string[] entry = new string[entryCount];
                bool[] hasSubEntry = new bool[entryCount];
                                
                List<string> aircraftPlaceDisplayNames = getAircraftPlaceDisplayNames(player);
                List<string> aircraftPlaces = getAircraftPlaces(player);

                if (menuOffsets[player] < 0)
                {
                    menuOffsets[player] = (int)aircraftPlaceDisplayNames.Count / 7;
                }
                else if ((menuOffsets[player] * 7) > aircraftPlaceDisplayNames.Count)
                {
                    menuOffsets[player] = 0;
                }

                for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
                {
                    if (entryIndex == entryCount - 2)
                    {
                        entry[entryIndex] = "Page up";
                        hasSubEntry[entryIndex] = true;
                    }
                    else if (entryIndex == entryCount - 1)
                    {
                        entry[entryIndex] = "Page down";
                        hasSubEntry[entryIndex] = true;
                    }
                    else
                    {
                        if (entryIndex + (menuOffsets[player] * 7) < aircraftPlaceDisplayNames.Count)
                        {
                            entry[entryIndex] = aircraftPlaceDisplayNames[entryIndex + (menuOffsets[player] * 7)];
                            hasSubEntry[entryIndex] = false;
                        }
                    }
                }
                
                GamePlay.gpSetOrderMissionMenu(player, false, 0, entry, hasSubEntry);
            }
       
            public List<string> getAircraftPlaceDisplayNames(Player player)
            {
                List<string> aircraftPlaceDisplayNames = new List<string>();
                
                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int armyIndex in GamePlay.gpArmies())
                    {
                        if (GamePlay.gpAirGroups(armyIndex) != null && GamePlay.gpAirGroups(armyIndex).Length > 0)
                        {
                            foreach (AiAirGroup airGroup in GamePlay.gpAirGroups(armyIndex))
                            {
                                if (airGroup.GetItems() != null && airGroup.GetItems().Length > 0)
                                {
                                    foreach (AiActor actor in airGroup.GetItems())
                                    {
                                        if (actor is AiAircraft)
                                        {
                                            AiAircraft aircraft = actor as AiAircraft;
                                            if (!aircraft.Name().StartsWith("0:"))
                                            {
                                                if (aircraft.Places() > 0)
                                                {
                                                    for (int placeIndex = 0; placeIndex < aircraft.Places(); placeIndex++)
                                                    {
                                                        if (aircraft.ExistCabin(placeIndex))
                                                        {
                                                            string aircraftPlaceDisplayName = aircraft.Name() + " " + aircraft.TypedName() + " " + aircraft.CrewFunctionPlace(placeIndex).ToString();
                                                            aircraftPlaceDisplayNames.Add(aircraftPlaceDisplayName);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }                            
                            }
                        }
                    }
                }

                return aircraftPlaceDisplayNames;
            }

            private List<string> getAircraftPlaces(Player player)
            {
                List<string> aircraftPlaces = new List<string>();

                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int armyIndex in GamePlay.gpArmies())
                    {
                        if (GamePlay.gpAirGroups(armyIndex) != null && GamePlay.gpAirGroups(armyIndex).Length > 0)
                        {
                            foreach (AiAirGroup airGroup in GamePlay.gpAirGroups(armyIndex))
                            {
                                if (airGroup.GetItems() != null && airGroup.GetItems().Length > 0)
                                {
                                    foreach (AiActor actor in airGroup.GetItems())
                                    {
                                        if (actor is AiAircraft)
                                        {
                                            AiAircraft aircraft = actor as AiAircraft;
                                            if(!aircraft.Name().StartsWith("0:"))
                                            {
                                                if (aircraft.Places() > 0)
                                                {
                                                    for (int placeIndex = 0; placeIndex < aircraft.Places(); placeIndex++)
                                                    {
                                                        if (aircraft.ExistCabin(placeIndex))
                                                        {
                                                            string aircraftPlace = aircraft.Name() + "@" + placeIndex;
                                                            aircraftPlaces.Add(aircraftPlace);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return aircraftPlaces;
            }

            public override void OnOrderMissionMenuSelected(Player player, int ID, int menuItemIndex)
            {
                if (ID == 0)
                {
                    if (menuItemIndex == 0)
                    {
                        menuOffsets[player] = 0;
                        SetMenu(player);
                    }
                    else
                    {
                        if (menuItemIndex == 8)
                        {
                            menuOffsets[player] = menuOffsets[player] - 1;
                            SetMenu(player);
                        }
                        else if (menuItemIndex == 9)
                        {
                            menuOffsets[player] = menuOffsets[player] + 1;
                            SetMenu(player);
                        }
                        else
                        {
                            if (menuItemIndex - 1 + (menuOffsets[player] * 7) < getAircraftPlaces(player).Count)
                            {                                
                                List<string> aircraftPlaces = getAircraftPlaces(player);
                                List<string> aircraftPlaceDisplayNames = getAircraftPlaceDisplayNames(player);
                                                                    
                                string aircraftPlace = aircraftPlaces[menuItemIndex - 1 + (menuOffsets[player] * 7)];
                                placePlayer(player, aircraftPlace);
                            }
                            else
                            {
                                // No handling needed as menu item is not displayed.
                            }
                        }
                    }
                }                
            }

            private void placePlayer(Player player, string aircraftPlace)
            {
                string aircraftName = aircraftPlace.Remove(aircraftPlace.IndexOf("@"), aircraftPlace.Length - aircraftPlace.IndexOf("@"));
                string place = aircraftPlace.Replace(aircraftName + "@", "");
                int placeIndex;
                if (int.TryParse(place, out placeIndex))
                {
                    AiActor actor = GamePlay.gpActorByName(aircraftName);
                    if (actor != null && actor is AiAircraft)
                    {
                        AiAircraft aircraft = actor as AiAircraft;
                        if (aircraft.ExistCabin(placeIndex))
                        {
                            player.PlaceEnter(aircraft, placeIndex);
                        }
                    }
                }
            }
            
            public override void OnAircraftTookOff(int missionNumber, string shortName, AiAircraft aircraft)
            {
                base.OnAircraftTookOff(missionNumber, shortName, aircraft);

                if (aircraft.Player(0) != null)
                {
                    GamePlay.gpHUDLogCenter(new Player[] { aircraft.Player(0) }, "Please enable autopilot for a few seconds to activate the AI aircraft of your squadron.");
                }
            }
        }
    }
}