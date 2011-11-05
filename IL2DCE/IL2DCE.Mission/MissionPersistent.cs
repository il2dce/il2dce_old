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

            internal class AirGroupProxy
            {
                public enum ETask
                {
                    READY,
                    PENDING_INTERCEPT,
                    INTERCEPT,
                    PENDING_ATTACK,
                    ATTACK,
                    RTB,
                }

                public AirGroupProxy(AiAirGroup aiAirGroup, IGamePlay gamePlay)
                {
                    AiAirport closestAiAirport = null;
                    if (gamePlay.gpAirports() != null && gamePlay.gpAirports().Length > 0)
                    {
                        foreach (AiAirport aiAirport in gamePlay.gpAirports())
                        {
                            Point3d p = new Point3d(aiAirGroup.Pos().x, aiAirGroup.Pos().y, aiAirGroup.Pos().z);
                            if (closestAiAirport == null)
                            {
                                closestAiAirport = aiAirport;
                            }
                            else
                            {
                                if (aiAirport.Pos().distance(ref p) < closestAiAirport.Pos().distance(ref p))
                                {
                                    closestAiAirport = aiAirport;
                                }
                            }
                            Airport = closestAiAirport;
                        }
                    }

                    Task = ETask.READY;
                    Detected = false;
                }

                public ETask Task
                {
                    get;
                    set;
                }

                public bool Detected
                {
                    get;
                    set;
                }

                public AiActor Target
                {
                    get;
                    set;
                }

                public AiAirport Airport
                {
                    get;
                    set;
                }
            }

            private System.Collections.Generic.Dictionary<AiAirGroup, AirGroupProxy> airGroupProxies = new System.Collections.Generic.Dictionary<AiAirGroup, AirGroupProxy>();

            private System.Collections.Generic.Dictionary<AiGroundGroup, GroundGroup> groundGroupProxies = new System.Collections.Generic.Dictionary<AiGroundGroup, GroundGroup>();
            private ISectionFile triggerFile;

            public override void OnActorCreated(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorCreated(missionNumber, shortName, actor);

                if (actor is AiGroundGroup)
                {
                    foreach (GroundGroup groundGroup in this.core.Generator.GroundGroups)
                    {
                        string aiGroundGroupName = actor.Name();
                        aiGroundGroupName = aiGroundGroupName.Remove(0, aiGroundGroupName.IndexOf(":") + 1);

                        if (groundGroup.Id == aiGroundGroupName)
                        {
                            groundGroupProxies.Add((actor as AiGroundGroup), groundGroup);
                            this.Core.Generator.Created(groundGroup);
                        }
                    }
                }
            }

            public override void OnActorDestroyed(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorDestroyed(missionNumber, shortName, actor);

                if (actor is AiGroundGroup && groundGroupProxies.ContainsKey(actor as AiGroundGroup))
                {
                    this.Core.Generator.Destroyed(groundGroupProxies[(actor as AiGroundGroup)]);
                    groundGroupProxies.Remove(actor as AiGroundGroup);
                }
            }

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
            }

            public override void OnActorTaskCompleted(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorTaskCompleted(missionNumber, shortName, actor);

                if (actor is AiGroundGroup)
                {
                    AiGroundGroup aiGroundGroup = actor as AiGroundGroup;
                    if (groundGroupProxies.ContainsKey(aiGroundGroup))
                    {
                        Point2d start = new Point2d(aiGroundGroup.Pos().x, aiGroundGroup.Pos().y);

                        if (aiGroundGroup.GroupType() == AiGroundGroupType.Vehicle)
                        {
                            AiGroundGroup closestTarget = null;
                            foreach (AiGroundGroup target in groundGroupProxies.Keys)
                            {
                                if (target.IsAlive() && target.IsValid() && target.Army() != aiGroundGroup.Army())
                                {
                                    if (closestTarget == null)
                                    {
                                        closestTarget = target;
                                    }
                                    else
                                    {
                                        if (new Point2d(target.Pos().x, target.Pos().y).distance(ref start) < new Point2d(closestTarget.Pos().x, closestTarget.Pos().y).distance(ref start))
                                        {
                                            closestTarget = target;
                                        }
                                    }
                                }
                            }
                            FrontMarker closestFrontMarker = null;
                            foreach (FrontMarker frontMarker in this.Core.Generator.FrontMarkers)
                            {
                                if (frontMarker.Army != aiGroundGroup.Army())
                                {
                                    if (closestFrontMarker == null)
                                    {
                                        closestFrontMarker = frontMarker;
                                    }
                                    else
                                    {
                                        if (frontMarker.Position.distance(ref start) < closestFrontMarker.Position.distance(ref start))
                                        {
                                            closestFrontMarker = frontMarker;
                                        }
                                    }
                                }
                            }

                            if (((closestTarget != null && closestFrontMarker != null) && (new Point2d(closestTarget.Pos().x, closestTarget.Pos().y).distance(ref start) < closestFrontMarker.Position.distance(ref start)))
                                || closestTarget != null && closestFrontMarker == null)
                            {
                                Point2d end = new Point2d(closestTarget.Pos().x, closestTarget.Pos().y);
                                groundGroupProxies[aiGroundGroup].PathParams = GamePlay.gpFindPath(start, 10, end, 20, PathType.GROUND, aiGroundGroup.Army());
                            }
                            else if (closestFrontMarker != null)
                            {
                                Point2d end = new Point2d(closestFrontMarker.Position.x, closestFrontMarker.Position.y);
                                groundGroupProxies[aiGroundGroup].PathParams = GamePlay.gpFindPath(start, 10, end, 20, PathType.GROUND, aiGroundGroup.Army());
                            }
                        }
                    }
                }                
            }
            
            public override void OnTickGame()
            {
                base.OnTickGame();

                if (Time.tickCounter() % 3000 == 0)
                {
                    ISectionFile airMissionFile = Core.Generator.GenerateRandomAirOperation();
                    if (airMissionFile != null)
                    {
                        GamePlay.gpPostMissionLoad(airMissionFile);
                    }

                    ISectionFile groundMissionFile = Core.Generator.GenerateRandomGroundOperation();
                    if (groundMissionFile != null)
                    {
                        GamePlay.gpPostMissionLoad(groundMissionFile);
                    }
                }

                if (Time.tickCounter() % 300 == 0)
                {
                    if (groundGroupProxies.Count > 0)
                    {
                        foreach (AiGroundGroup aiGroundGroup in groundGroupProxies.Keys)
                        {
                            GroundGroup groundGroup = groundGroupProxies[aiGroundGroup];
                            if (groundGroup.PathParams != null)
                            {
                                if (groundGroup.PathParams.State == RecalcPathState.SUCCESS)
                                {
                                    aiGroundGroup.SetWay(groundGroup.PathParams.Path);
                                    groundGroup.PathParams = null;

                                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiGroundGroup.Name() + " new path.", null);
                                }
                                else if (groundGroup.PathParams.State == RecalcPathState.FAILED)
                                {
                                    groundGroup.PathParams = null;
                                }
                            }
                        }
                    }
                }
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