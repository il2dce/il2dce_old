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
            protected ICore Core
            {
                get
                {
                    return this.core;
                }
            }
            private ICore core;

            internal class Radar
            {
                public Radar(Point3d p, int armyIndex)
                {
                    Pos = p;
                    Army = armyIndex;
                }

                public Point3d Pos
                {
                    get;
                    set;
                }

                public int Army
                {
                    get;
                    set;
                }

                public bool Detect(AiAirGroup aiAirGroup)
                {
                    Point3d p = new Point3d(Pos.x, Pos.y, Pos.z);
                    if (aiAirGroup.Army() != this.Army && aiAirGroup.Pos().distance(ref p) <= 50000.0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            internal class FrontMarker
            {
                public FrontMarker(Point2d p, int army)
                {
                    this.P = p;
                    this.Army = army;
                }

                public Point2d P
                {
                    get;
                    set;
                }

                public int Army
                {
                    get;
                    set;
                }
            }

            internal class GroundGroupProxy
            {
                public GroundGroupProxy(AiGroundGroup aiGroundGroup, IGamePlay gamePlay)
                {
                    Detected = false;
                    PathParams = null;
                }

                public bool Detected
                {
                    get;
                    set;
                }

                public IRecalcPathParams PathParams
                {
                    get;
                    set;
                }
            }

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
            private System.Collections.Generic.Dictionary<AiGroundGroup, GroundGroupProxy> groundGroupProxies = new System.Collections.Generic.Dictionary<AiGroundGroup, GroundGroupProxy>();
            private System.Collections.Generic.Dictionary<string, Radar> radars = new System.Collections.Generic.Dictionary<string, Radar>();
            private System.Collections.Generic.List<FrontMarker> frontMarkers = new System.Collections.Generic.List<FrontMarker>();
            private ISectionFile triggerFile;

            private Queue<AirGroup> availableAirGroups = new Queue<AirGroup>();

            public override void OnActorCreated(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorCreated(missionNumber, shortName, actor);

                if (actor is AiAirGroup)
                {
                    AirGroupProxy airGroup = new AirGroupProxy(actor as AiAirGroup, this.GamePlay);
                    airGroupProxies.Add(actor as AiAirGroup, airGroup);

                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, actor.Name() + "now under control of Wizard.", null);
                }
                else if (actor is AiGroundGroup)
                {
                    GroundGroupProxy groundGroup = new GroundGroupProxy(actor as AiGroundGroup, this.GamePlay);
                    groundGroupProxies.Add(actor as AiGroundGroup, groundGroup);

                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, actor.Name() + "now under control of Wizard.", null);
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

                core.Career = core.Careers[0];
                core.InitCampaign();


                ISectionFile templateFile = GamePlay.gpLoadSectionFile("$user/missions/IL2DCE/Campaigns/IL2DCE.Persistent.mis");

                this.triggerFile = GamePlay.gpCreateSectionFile();

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
                            frontMarkers.Add(new FrontMarker(new Point2d(x, y), army));

                            triggerFile.add("Trigger", "changeArmy" + i.ToString() + "_1", " TPassThrough 3 1 " + x + " " + y + " 500");
                            triggerFile.add("Trigger", "changeArmy" + i.ToString() + "_2", " TPassThrough 3 2 " + x + " " + y + " 500");
                        }
                    }
                }

                for (int i = 0; i < templateFile.lines("AirGroups"); i++)
                {
                    string key;
                    string value;
                    templateFile.get("AirGroups", i, out key, out value);

                    IL2DCE.AirGroup airGroup = new IL2DCE.AirGroup(this.Core, templateFile, key);
                    availableAirGroups.Enqueue(airGroup);                    
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
                            foreach (FrontMarker frontMarker in frontMarkers)
                            {
                                if (frontMarker.Army != aiGroundGroup.Army())
                                {
                                    if (closestFrontMarker == null)
                                    {
                                        closestFrontMarker = frontMarker;
                                    }
                                    else
                                    {
                                        if (frontMarker.P.distance(ref start) < closestFrontMarker.P.distance(ref start))
                                        {
                                            closestFrontMarker = frontMarker;
                                        }
                                    }
                                }
                            }

                            if (((closestTarget != null && closestFrontMarker != null) && (new Point2d(closestTarget.Pos().x, closestTarget.Pos().y).distance(ref start) < closestFrontMarker.P.distance(ref start)))
                                || closestTarget != null && closestFrontMarker == null)
                            {
                                Point2d end = new Point2d(closestTarget.Pos().x, closestTarget.Pos().y);
                                groundGroupProxies[aiGroundGroup].PathParams = GamePlay.gpFindPath(start, 10, end, 20, PathType.GROUND, aiGroundGroup.Army());
                            }
                            else if (closestFrontMarker != null)
                            {
                                Point2d end = new Point2d(closestFrontMarker.P.x, closestFrontMarker.P.y);
                                groundGroupProxies[aiGroundGroup].PathParams = GamePlay.gpFindPath(start, 10, end, 20, PathType.GROUND, aiGroundGroup.Army());
                            }
                        }
                    }
                }
                //else if (actor is AiAirGroup)
                //{
                //    AiAirGroup aiAirGroup = actor as AiAirGroup;
                //    if (airGroups.ContainsKey(aiAirGroup))
                //    {
                //        AirGroup airGroup = airGroups[aiAirGroup];


                //        if (airGroup.Task == AirGroup.ETask.PENDING_INTERCEPT && airGroup.Target != null && airGroup.Target.IsAlive() && airGroup.Target.IsValid())
                //        {
                //            AiWayPoint[] result = new AiWayPoint[1];

                //            double speed = (aiAirGroup.GetItems()[0] as AiAircraft).getParameter(part.ParameterTypes.Z_VelocityTAS, -1);
                //            Point3d p = new Point3d(airGroup.Target.Pos().x, airGroup.Target.Pos().y, airGroup.Target.Pos().z);
                //            AiAirWayPoint aiAirWayPoint = new AiAirWayPoint(ref p, speed);
                //            aiAirWayPoint.Target = airGroup.Target;
                //            if (aiAirGroup.isAircraftType(AircraftType.Bomber) || aiAirGroup.isAircraftType(AircraftType.DiveBomber) || aiAirGroup.isAircraftType(AircraftType.TorpedoBomber))
                //            {
                //                aiAirWayPoint.Action = AiAirWayPointType.AATTACK_BOMBERS;
                //            }
                //            else
                //            {
                //                aiAirWayPoint.Action = AiAirWayPointType.AATTACK_FIGHTERS;
                //            }
                //            result[0] = aiAirWayPoint;

                //            aiAirGroup.SetWay(result);

                //            airGroup.Task = AirGroup.ETask.INTERCEPT;

                //            GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiAirGroup.Name() + " intercept " + airGroup.Target.Name() + ".", null);
                //        }
                //        else if (airGroup.Task == AirGroup.ETask.PENDING_ATTACK && airGroup.Target != null && airGroup.Target.IsAlive() && airGroup.Target.IsValid())
                //        {
                //            AiWayPoint[] result = new AiWayPoint[1];

                //            double speed = (aiAirGroup.GetItems()[0] as AiAircraft).getParameter(part.ParameterTypes.Z_VelocityTAS, -1);
                //            Random rand = new Random();
                //            double altitude = (double)rand.Next(300, 3000);

                //            Point3d p = new Point3d(airGroup.Target.Pos().x, airGroup.Target.Pos().y, altitude);
                //            AiAirWayPoint aiAirWayPoint = new AiAirWayPoint(ref p, speed);
                //            aiAirWayPoint.Action = AiAirWayPointType.GATTACK_TARG;
                //            aiAirWayPoint.Target = airGroup.Target;
                //            result[0] = aiAirWayPoint;

                //            aiAirGroup.SetWay(result);

                //            airGroup.Task = AirGroup.ETask.ATTACK;

                //            GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiAirGroup.Name() + " attack " + airGroup.Target.Name() + ".", null);

                //        }
                //        else
                //        {
                //            AiWayPoint[] result = new AiWayPoint[2];

                //            double speed = (aiAirGroup.GetItems()[0] as AiAircraft).getParameter(part.ParameterTypes.Z_VelocityTAS, -1);
                //            Point3d p0 = new Point3d(airGroup.Airport.Pos().x, airGroup.Airport.Pos().y, aiAirGroup.Pos().z);
                //            AiAirWayPoint aiAirWayPoint0 = new AiAirWayPoint(ref p0, speed);
                //            aiAirWayPoint0.Action = AiAirWayPointType.NORMFLY;
                //            result[0] = aiAirWayPoint0;

                //            Point3d p1 = new Point3d(airGroup.Airport.Pos().x, airGroup.Airport.Pos().y, airGroup.Airport.Pos().z);
                //            AiAirWayPoint aiAirWayPoint1 = new AiAirWayPoint(ref p1, 0.0);
                //            aiAirWayPoint1.Action = AiAirWayPointType.LANDING;
                //            aiAirWayPoint1.Target = airGroup.Airport;
                //            result[1] = aiAirWayPoint1;

                //            aiAirGroup.SetWay(result);

                //            airGroup.Task = AirGroup.ETask.RTB;
                //            airGroup.Target = null;

                //            GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiAirGroup.Name() + " task completed. RTB.", null);
                //        }
                //    }
                //}
            }

            private void attack(AiAirGroup aiAirGroup)
            {
                AirGroupProxy airGroup = airGroupProxies[aiAirGroup];

                int enemyArmyIndex = 0;
                if (aiAirGroup.Army() == 1)
                {
                    enemyArmyIndex = 2;
                }
                else if (aiAirGroup.Army() == 2)
                {
                    enemyArmyIndex = 1;
                }

                if (GamePlay.gpGroundGroups(enemyArmyIndex) != null && GamePlay.gpGroundGroups(enemyArmyIndex).Length > 0)
                {
                    AiGroundGroup closestAiGroundGroup = null;
                    foreach (AiGroundGroup aiGroundGroup in GamePlay.gpGroundGroups(enemyArmyIndex))
                    {
                        if (aiGroundGroup.IsAlive() && aiGroundGroup.IsValid() && aiGroundGroup.Army() != aiAirGroup.Army())
                        {
                            if (closestAiGroundGroup == null)
                            {
                                closestAiGroundGroup = aiGroundGroup;
                            }
                            else
                            {
                                Point3d p = new Point3d(airGroup.Airport.Pos().x, airGroup.Airport.Pos().y, airGroup.Airport.Pos().z);
                                if (aiGroundGroup.Pos().distance(ref p) < closestAiGroundGroup.Pos().distance(ref p))
                                {
                                    closestAiGroundGroup = aiGroundGroup;
                                }
                            }
                        }
                    }

                    if (closestAiGroundGroup != null)
                    {
                        airGroup.Target = closestAiGroundGroup;
                        airGroup.Task = AirGroupProxy.ETask.PENDING_ATTACK;

                        aiAirGroup.Idle = false;
                    }
                }
            }

            private void intercept(AiAirGroup aiAirGroup)
            {
                foreach (AiAirGroup idleAiAirGroup in airGroupProxies.Keys)
                {
                    if ((aiAirGroup.Army() != idleAiAirGroup.Army() && idleAiAirGroup.Idle == true)
                        && (idleAiAirGroup.isAircraftType(AircraftType.Fighter) || idleAiAirGroup.isAircraftType(AircraftType.HeavyFighter)))
                    {
                        AirGroupProxy airGroup = airGroupProxies[idleAiAirGroup];

                        airGroup.Target = aiAirGroup;
                        airGroup.Task = AirGroupProxy.ETask.PENDING_INTERCEPT;

                        idleAiAirGroup.Idle = false;

                        break;
                    }
                }
            }

            private void radarDetection(AiAirGroup aiAirGroup)
            {
                if (radars.Count > 0)
                {
                    foreach (Radar radar in radars.Values)
                    {
                        if (radar.Detect(aiAirGroup) && airGroupProxies[aiAirGroup].Detected == false)
                        {
                            GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiAirGroup.Name() + " detected.", null);

                            intercept(aiAirGroup);

                            airGroupProxies[aiAirGroup].Detected = true;
                            break;
                        }
                    }
                }
            }

            private void updateWaypoints(AiAirGroup aiAirGroup)
            {
                if (aiAirGroup.IsAlive() && aiAirGroup.IsValid())
                {
                    if (airGroupProxies[aiAirGroup].Task == AirGroupProxy.ETask.INTERCEPT || airGroupProxies[aiAirGroup].Task == AirGroupProxy.ETask.ATTACK)
                    {
                        if (aiAirGroup.GetWay() != null && aiAirGroup.GetWay().Length > 0)
                        {
                            AiWayPoint[] result = new AiWayPoint[aiAirGroup.GetWay().Length];
                            AiActor target = null;
                            for (int i = 0; i < aiAirGroup.GetWay().Length; i++)
                            {
                                AiAirWayPoint aiAirWayPoint = aiAirGroup.GetWay()[i] as AiAirWayPoint;
                                if ((aiAirWayPoint.Action == AiAirWayPointType.AATTACK_BOMBERS || aiAirWayPoint.Action == AiAirWayPointType.AATTACK_FIGHTERS) && aiAirWayPoint.Target != null && aiAirWayPoint.Target.IsAlive())
                                {
                                    aiAirWayPoint.P = aiAirWayPoint.Target.Pos();
                                    target = aiAirWayPoint.Target;
                                }
                                else if (aiAirWayPoint.Action == AiAirWayPointType.GATTACK_TARG && aiAirWayPoint.Target != null && aiAirWayPoint.Target.IsAlive())
                                {
                                    aiAirWayPoint.P = aiAirWayPoint.Target.Pos();
                                    target = aiAirWayPoint.Target;
                                }

                                result[i] = aiAirWayPoint;

                                if (target != null)
                                {
                                    Point3d pTarget = new Point3d(target.Pos().x, target.Pos().y, target.Pos().z);
                                    if (aiAirGroup.Pos().distance(ref pTarget) > 1000.0)
                                    {
                                        aiAirGroup.SetWay(result);
                                        GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiAirGroup.Name() + " waypoint update.", null);
                                    }
                                    else
                                    {
                                        GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, aiAirGroup.Name() + " no waypoint update.", null);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            bool isAvailable(AirGroup airGroup)
            {
                List<string> aiAirGroupNames = new List<string>();
                if (GamePlay.gpAirGroups(airGroup.ArmyIndex) != null && GamePlay.gpAirGroups(airGroup.ArmyIndex).Length > 0)
                {
                    foreach (AiAirGroup aiAirGroup in GamePlay.gpAirGroups(airGroup.ArmyIndex))
                    {
                        string aiAirGroupName = aiAirGroup.Name();
                        aiAirGroupName = aiAirGroupName.Remove(0, aiAirGroupName.IndexOf(":") + 1);
                        aiAirGroupNames.Add(aiAirGroupName);
                    }
                }

                if (aiAirGroupNames.Contains(airGroup.Id))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public override void OnTickGame()
            {
                base.OnTickGame();

                if (Time.tickCounter() % 300 == 0)
                {
                    if (availableAirGroups.Count > 0)
                    {
                        AirGroup airGroup = availableAirGroups.Dequeue();
                        availableAirGroups.Enqueue(airGroup);

                        if (isAvailable(airGroup))
                        {
                            if (airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.ATTACK_ARMOR))
                            {
                                if (GamePlay.gpGroundGroups(airGroup.ArmyIndex) != null && GamePlay.gpGroundGroups(airGroup.ArmyIndex).Length > 0)
                                {
                                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, "Hello", null);

                                    AiGroundGroup closestAiGroundGroup = null;

                                    int armyIndex = 0;
                                    if (airGroup.ArmyIndex == 1)
                                    {
                                        armyIndex = 2;
                                    }
                                    else
                                    {
                                        armyIndex = 1;
                                    }

                                    foreach (AiGroundGroup aiGroundGroup in GamePlay.gpGroundGroups(armyIndex))
                                    {
                                        if (aiGroundGroup.IsAlive())
                                        {
                                            if (closestAiGroundGroup == null)
                                            {
                                                closestAiGroundGroup = aiGroundGroup;
                                            }
                                            else
                                            {
                                                Point3d p = new Point3d(airGroup.Position.x, airGroup.Position.y, airGroup.Position.z);
                                                if (aiGroundGroup.Pos().distance(ref p) < closestAiGroundGroup.Pos().distance(ref p))
                                                {
                                                    closestAiGroundGroup = aiGroundGroup;
                                                }
                                            }
                                        }
                                    }

                                    if (closestAiGroundGroup != null)
                                    {
                                        ISectionFile airMission = GamePlay.gpCreateSectionFile();

                                        Random rand = new Random();

                                        List<IAircraftParametersInfo> aircraftParametersInfos = airGroup.AircraftInfo.GetAircraftParametersInfo(EMissionType.ATTACK_ARMOR);
                                        int aircraftParametersInfoIndex = rand.Next(aircraftParametersInfos.Count);
                                        IAircraftParametersInfo randomAircraftParametersInfo = aircraftParametersInfos[aircraftParametersInfoIndex];
                                        IAircraftLoadoutInfo aircraftLoadoutInfo = airGroup.AircraftInfo.GetAircraftLoadoutInfo(randomAircraftParametersInfo.LoadoutId);
                                        airGroup.Weapons = aircraftLoadoutInfo.Weapons;
                                        airGroup.Detonator = aircraftLoadoutInfo.Detonator;

                                        Point2d pos = new Point2d(closestAiGroundGroup.Pos().x, closestAiGroundGroup.Pos().y);
                                        double alt = this.Core.GetRandomAltitude(randomAircraftParametersInfo);
                                        airGroup.GroundAttack(EMissionType.ATTACK_ARMOR, closestAiGroundGroup, alt);

                                        airGroup.SetOnParked = true;

                                        airGroup.writeTo(airMission);

                                        GamePlay.gpPostMissionLoad(airMission);
                                    }
                                }
                            }
                        }
                    }
                }

                if (Time.tickCounter() % 300 == 0)
                {
                    //if (airGroups.Count > 0)
                    //{
                    //    foreach (AiAirGroup aiAirGroup in airGroups.Keys)
                    //    {
                    //        if (aiAirGroup.Idle == false)
                    //        {
                    //            radarDetection(aiAirGroup);
                    //            //updateWaypoints(aiAirGroup);
                    //        }
                    //        else
                    //        {
                    //            if (aiAirGroup.isAircraftType(AircraftType.Bomber) || aiAirGroup.isAircraftType(AircraftType.DiveBomber) || aiAirGroup.isAircraftType(AircraftType.TorpedoBomber))
                    //            {
                    //                attack(aiAirGroup);
                    //            }
                    //        }
                    //    }
                    //}

                    if (groundGroupProxies.Count > 0)
                    {
                        foreach (AiGroundGroup aiGroundGroup in groundGroupProxies.Keys)
                        {
                            GroundGroupProxy groundGroup = groundGroupProxies[aiGroundGroup];
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
                frontMarkers[markerNum].Army = newArmy;

                ISectionFile f = GamePlay.gpCreateSectionFile();
                string sect;
                string key;
                string value;

                for (int i = 0; i < frontMarkers.Count; i++)
                {
                    sect = "FrontMarker";
                    key = "FrontMarker" + i.ToString();
                    value = frontMarkers[i].P.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + frontMarkers[i].P.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + frontMarkers[i].Army.ToString();
                    f.add(sect, key, value);
                }
                return f;
            }

            public override void OnTrigger(int missionNumber, string shortName, bool active)
            {
                base.OnTrigger(missionNumber, shortName, active);

                for (int i = 0; i < this.frontMarkers.Count; i++)
                {
                    for (int j = 1; j < 3; j++)
                    {
                        string str = "changeArmy" + i.ToString() + "_" + (j).ToString();
                        if (str.Equals(shortName))
                        {
                            if (frontMarkers[i].Army != j)
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