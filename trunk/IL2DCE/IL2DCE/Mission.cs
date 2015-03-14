using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;
using maddox.game.world;

namespace IL2DCE
{
    public class Mission : AMission, IPersistentWorld
    {
        protected string missionFileName = "";
        protected string aircraftInfoFileName = "";
        protected bool spawnParked = false;
        protected uint pendingDuration = 1;
        protected double flightSizeFactor = 1.0;
        protected double flightCountFactor = 0.5;
        
        public event EventHandler MissionSlice;

        public event EventHandler DetectionSlice;

        public event UnitEventHandler UnitDiscovered;

        public event UnitEventHandler UnitCovered;

        public double FlightSizeFactor
        {
            get
            {
                return flightSizeFactor;
            }
        }

        public double FlightCountFactor
        {
            get
            {
                return flightCountFactor;
            }
        }

        public Dictionary<string, IUnit> Units
        {
            get
            {
                return this.units;
            }
        }
        private Dictionary<string, IUnit> units = new Dictionary<string, IUnit>();

        public Dictionary<string, IBuilding> Buildings
        {
            get
            {
                return this.buildings;
            }
        }
        private Dictionary<string, IBuilding> buildings = new Dictionary<string, IBuilding>();

        public IList<IHeadquarters> Headquarters
        {
            get
            {
                return this.headquarters;
            }
        }
        private List<IHeadquarters> headquarters = new List<IHeadquarters>();

        public Map Map
        {
            get
            {
                return this.map;
            }
        }
        private Map map;

        public Random Random
        {
            get
            {
                return this.random;
            }
        }
        private Random random;

        public ISectionFile AircraftInfoFile
        {
            get
            {
                return this.aircraftInfoFile;
            }
        }
        private ISectionFile aircraftInfoFile;

        public void Debug(string line)
        {
            List<Player> players = new List<Player>();
            players.AddRange(GamePlay.gpRemotePlayers());
            if (GamePlay.gpPlayer() != null && !players.Contains(GamePlay.gpPlayer()))
            {
                players.Add(GamePlay.gpPlayer());
            }
            GamePlay.gpLogServer(players.ToArray(), line, null);
        }

        public void Chat(string line, IPlayer to)
        {
            if (GamePlay is GameDef)
            {
                (GamePlay as GameDef).gameInterface.CmdExec("chat " + line + " TO " + to.Name());
            }
        }
        
        private void RaisePeriodicMissionSlice()
        {
            RaiseMissionSlice();

            Timeout(5 * 60, () => 
            {
                RaisePeriodicMissionSlice();
            });
        }

        private void RaiseMissionSlice()
        {
            if (MissionSlice != null)
            {
                MissionSlice(this, new EventArgs());
            }
        }

        private void RaisePeriodicDetectionSlice()
        {
            RaiseDetectionSlice();

            Timeout(1 * 60, () =>
            {
                RaisePeriodicDetectionSlice();
            });
        }

        private void RaiseDetectionSlice()
        {
            if (DetectionSlice != null)
            {
                DetectionSlice(this, new EventArgs());
            }
        }

        public void TakeOrder(IOrder order)
        {
            if (order is AirOrder)
            {
                AirOrder airOrder = order as AirOrder;                
                AirGroup airGroup = airOrder.AirGroup;
                AirGroup escortAirGroup = airOrder.EscortAirGroup;
                AirGroup interceptAirGroup = airOrder.InterceptAirGroup;

                ISectionFile missionFile = GamePlay.gpCreateSectionFile();

                if (airGroup != null)
                {
                    airGroup.SetOnParked = spawnParked;
                    airGroup.WriteTo(missionFile);
                    missionFile.add(airGroup.Id, "Idle", "1");
                }
                if (escortAirGroup != null)
                {
                    escortAirGroup.SetOnParked = spawnParked;
                    escortAirGroup.WriteTo(missionFile);
                    missionFile.add(escortAirGroup.Id, "Idle", "1");
                }
                if (interceptAirGroup != null)
                {
                    interceptAirGroup.SetOnParked = spawnParked;
                    interceptAirGroup.WriteTo(missionFile);
                    missionFile.add(interceptAirGroup.Id, "Idle", "1");
                }

                GamePlay.gpPostMissionLoad(missionFile);

                if (airGroup != null)
                {
                    Debug(airGroup.Id + " new order: " + airGroup.MissionType + "@" + airGroup.Altitude);
                }
                if (escortAirGroup != null)
                {
                    Debug(escortAirGroup.Id + " new order: " + escortAirGroup.MissionType + "@" + escortAirGroup.Altitude);
                }
                if (interceptAirGroup != null)
                {
                    Debug(interceptAirGroup.Id + " new order: " + interceptAirGroup.MissionType + "@" + interceptAirGroup.Altitude);
                }
            }
        }

        public Tuple<double, double, double> GetPositionOf(IUnit unit)
        {
            if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
            {
                foreach (int armyIndex in GamePlay.gpArmies())
                {
                    if (GamePlay.gpAirGroups(armyIndex) != null && GamePlay.gpAirGroups(armyIndex).Length > 0)
                    {
                        foreach (AiAirGroup airGroup in GamePlay.gpAirGroups(armyIndex))
                        {
                            string id = airGroup.Name().Remove(0, airGroup.Name().IndexOf(":") + 1);
                            if (unit.Id == id)
                            {
                                return new Tuple<double, double, double>(airGroup.Pos().x, airGroup.Pos().y, airGroup.Pos().z);
                            }
                        }
                    }
                }                
            }
            return null;
        }
        
        #region AMission

        public override void OnBattleInit()
        {
            base.OnBattleInit();

            if (GamePlay is GameDef)
            {
                // The use of GameDef requires a special line in conf.ini or confs.ini:
                //
                // [rts]
                // scriptAppDomain = 0
                //
                if (!GamePlay.gpConfigUserFile().exist("rts", "scriptAppDomain") || GamePlay.gpConfigUserFile().get("rts", "scriptAppDomain") != "0")
                {
                    Debug("Please add the following lines to your conf.ini and confs.ini:");
                    Debug("[rts]");
                    Debug("scriptAppDomain = 0");
                }
                (GamePlay as GameDef).EventChat += new GameDef.Chat(Mission_EventChat);
            }

            MissionNumberListener = -1;

            random = new System.Random();            

            // Parse mission file and fill the unit dictionary.
            ISectionFile missionFile = GamePlay.gpLoadSectionFile(missionFileName);
            this.aircraftInfoFile = GamePlay.gpLoadSectionFile(aircraftInfoFileName);

            this.map = new IL2DCE.Map(this, missionFile);

            AirHeadquarters redAirHeadquarters = new AirHeadquarters(this, Army.Red);
            Headquarters.Add(redAirHeadquarters);

            AirHeadquarters blueAirHeadquarters = new AirHeadquarters(this, Army.Blue);
            Headquarters.Add(blueAirHeadquarters);

            for (int i = 0; i < missionFile.lines("AirGroups"); i++)
            {
                string key;
                string value;
                missionFile.get("AirGroups", i, out key, out value);

                AirGroup airGroup = new AirGroup(this, key, missionFile);
                Units.Add(key, airGroup);

                redAirHeadquarters.Register(airGroup);
                blueAirHeadquarters.Register(airGroup);

                airGroup.Discovered += new UnitEventHandler(OnDiscovered);
                airGroup.Covered += new UnitEventHandler(OnCovered);

                airGroup.RaiseIdle();
            }

            for (int i = 0; i < missionFile.lines("Stationary"); i++)
            {
                string key;
                string value;
                missionFile.get("Stationary", i, out key, out value);

                if(value.StartsWith("Stationary.Radar.EnglishRadar1"))
                {
                    Radar radar = new Radar(this, key, missionFile);
                    Buildings.Add(key, radar);
                }
                else if (value.StartsWith("Stationary"))
                {
                    StationaryAircraft stationaryAircraft = new StationaryAircraft(this, key, missionFile);
                    Buildings.Add(key, stationaryAircraft);
                }
            }
        }

        void Mission_EventChat(IPlayer from, string msg)
        {
            if (msg.StartsWith("!help"))
            {
                Chat("Commands: !aircraft, !select#", from);
            }
            if (msg.StartsWith("!aircraft") || msg.StartsWith("!select"))
            {
                List<Tuple<AiAircraft, int>> aircraftPlaces = new List<Tuple<AiAircraft, int>>();
                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int army in GamePlay.gpArmies())
                    {
                        if (GamePlay.gpAirGroups(army) != null && GamePlay.gpAirGroups(army).Length > 0)
                        {
                            foreach (AiAirGroup airGroup in GamePlay.gpAirGroups(army))
                            {
                                if (airGroup.GetItems() != null && airGroup.GetItems().Length > 0)
                                {
                                    foreach (AiActor actor in airGroup.GetItems())
                                    {
                                        if (actor is AiAircraft)
                                        {
                                            AiAircraft Aircraft = actor as AiAircraft;
                                            for (int place = 0; place < Aircraft.Places(); place++)
                                            {
                                                aircraftPlaces.Add(new Tuple<AiAircraft, int>(Aircraft, place));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (msg.StartsWith("!aircraft"))
                {
                    int i = 0;
                    foreach (Tuple<AiAircraft, int> aircraftPlace in aircraftPlaces)
                    {
                        string playerName = "";
                        Player player = aircraftPlace.Item1.Player(aircraftPlace.Item2);
                        if (player != null)
                        {
                            playerName = " " + player.Name();
                        }
                        Chat("#" + i + ": " + aircraftPlace.Item1.Name() + " " + aircraftPlace.Item1.TypedName() + " " + aircraftPlace.Item1.CrewFunctionPlace(aircraftPlace.Item2) + " " + playerName, from);
                        i++;
                    }
                }
                else if (msg.StartsWith("!select"))
                {
                    msg = msg.Replace("!select", "");

                    int i = -1;
                    if (int.TryParse(msg, out i) && i < aircraftPlaces.Count)
                    {
                        Tuple<AiAircraft, int> aircraftPlace = aircraftPlaces[i];
                        if (aircraftPlace.Item1.Player(aircraftPlace.Item2) == null)
                        {
                            from.PlaceEnter(aircraftPlace.Item1, aircraftPlace.Item2);
                        }
                        else
                        {
                            Chat("Place occupied.", from);
                        }
                    }
                    else
                    {
                        Chat("Please enter a valid aircraft number, e.g. !select0, !select1, !select2, ...", from);
                    }
                }
            }
        }

        void OnDiscovered(object sender, UnitEventArgs e)
        {
            if (UnitDiscovered != null)
            {
                UnitDiscovered(this, e);
            }
        }

        void OnCovered(object sender, UnitEventArgs e)
        {
            if (UnitCovered != null)
            {
                UnitCovered(this, e);
            }
        }

        public override void OnBattleStarted()
        {
            base.OnBattleStarted();
            
            RaisePeriodicDetectionSlice();
            RaisePeriodicMissionSlice();
        }
        
        public override void OnPlaceEnter(Player player, AiActor actor, int placeIndex)
        {
            base.OnPlaceEnter(player, actor, placeIndex);

            if (actor is AiAircraft)
            {
                AiAircraft aiAircraft = actor as AiAircraft;
                AiAirGroup aiAirGroup = aiAircraft.AirGroup();
                if (aiAirGroup != null)
                {
                    string id = aiAirGroup.Name().Remove(0, actor.Name().IndexOf(":") + 1);
                    if (Units.ContainsKey(id))
                    {
                        if (Units[id] is AirGroup)
                        {
                            AirGroup airGroup = Units[id] as AirGroup;
                            foreach (AirGroupWaypoint waypoint in airGroup.Waypoints)
                            {
                                GPUserLabel userLabel = GamePlay.gpMakeUserLabel(new maddox.GP.Point2d(waypoint.Position.x, waypoint.Position.y), GamePlay.gpPlayer(), waypoint.Type.ToString() + "@" + waypoint.Position.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), GamePlay.gpTimeofDay(), (int)GPUserIconType.Waypoint);
                                GamePlay.gpDrawUserLabel(new Player[] { player }, userLabel);

                                Debug("New UserLabel: " + userLabel.pos.x + " " + userLabel.pos.y + " " + userLabel.Text);
                            }
                        }
                    }
                }
            }
        }
        
        public override void OnAircraftTookOff(int missionNumber, string shortName, AiAircraft aircraft)
        {
            base.OnAircraftTookOff(missionNumber, shortName, aircraft);

            if (aircraft.Player(0) != null)
            {
                Player player = aircraft.Player(0);

                player.PlaceLeave(0);

                Timeout(0.1, () =>
                {
                    player.PlaceEnter(aircraft, 0);
                });
            }
        }

        public override void OnActorCreated(int missionNumber, string shortName, maddox.game.world.AiActor actor)
        {
            base.OnActorCreated(missionNumber, shortName, actor);

            string id = actor.Name().Remove(0, actor.Name().IndexOf(":") + 1);
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirGroup && actor is AiAirGroup)
                {                    
                    Units[id].RaisePending();

                    Timeout(pendingDuration * 60, () =>
                    {
                        (actor as AiGroup).Idle = false;
                        Units[id].RaiseBusy();

                        //// Destroy birth places.
                        //if (GamePlay.gpBirthPlaces() != null && GamePlay.gpBirthPlaces().Length > 0)
                        //{
                        //    foreach (AiBirthPlace birthPlace in GamePlay.gpBirthPlaces())
                        //    {
                        //        if (birthPlace.Name() == id)
                        //        {
                        //            birthPlace.destroy();
                        //        }
                        //    }
                        //}
                    });
                }
                //else if (Units[id] is GroundUnit)
                //{
                //    Units[id].RaiseCreated();
                //}
            }
        }
        
        public override void OnActorDestroyed(int missionNumber, string shortName, maddox.game.world.AiActor actor)
        {
            base.OnActorDestroyed(missionNumber, shortName, actor);

            string id = actor.Name().Remove(0, actor.Name().IndexOf(":") + 1);
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirGroup)
                {
                    Units[id].RaiseDestroyed();
                }
                //else if (Units[id] is GroundUnit)
                //{
                //    Units[id].RaiseDestroyed();
                //}
            }
        }

        public override void OnActorTaskCompleted(int missionNumber, string shortName, maddox.game.world.AiActor actor)
        {
            base.OnActorTaskCompleted(missionNumber, shortName, actor);

            string id = actor.Name().Remove(0, actor.Name().IndexOf(":") + 1);
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirGroup)
                {
                    Units[id].RaiseIdle();
                }
                //if (Units[id] is GroundUnit)
                //{
                //    Units[id].RaiseIdle();
                //}
            }
        }
        
        #endregion
    }
}
