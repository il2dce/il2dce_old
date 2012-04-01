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
        public class MissionPersistent : AMission
        {
            public string missionFileName = null;

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

                Timeout((1 * 60), () =>
                {
                    UpdateWaypoints();
                });

                Timeout((2 * 60), () =>
                {
                    SpawnGroundGroups();
                });

                Timeout((3 * 60), () =>
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

                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiGroup.Name() + " new path.", null);
                            }
                            else if (groundGroup.PathParams.State == RecalcPathState.FAILED)
                            {
                                groundGroup.PathParams = null;
                                groundGroup.Fails++;
                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiGroup.Name() + " path failed (" + groundGroup.Fails.ToString() + ").", null);

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
                                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiGroup.Name() + " is stuck.", null);
                                    groundGroup.Stuck = true;
                                    this.Core.Generator.GenerateGroundOperation(groundGroup);
                                }
                                // This is also used for ships as they don't seem to fire the OnActorTaskComplete event.
                                else if (groundGroup.Type == EGroundGroupType.Ship)
                                {
                                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiGroup.Name() + " task complete (Ship).", null);
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
                ISectionFile airMissionFile = Core.Generator.GenerateRandomAirOperation();
                if (airMissionFile != null)
                {
                    GamePlay.gpPostMissionLoad(airMissionFile);
                }
                
                Timeout((15 * 60), () =>
                {
                    SpawnAirGroups();
                });
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
        }
    }
}