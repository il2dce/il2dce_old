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

        public event EventHandler MissionSlice;

        public event EventHandler DetectionSlice;

        public event UnitEventHandler UnitDiscovered;

        public event UnitEventHandler UnitCovered;
        
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
                if (order.Unit is AirGroup)
                {
                    AirGroup airGroup = order.Unit as AirGroup;

                    ISectionFile missionFile = GamePlay.gpCreateSectionFile();
                    airGroup.WriteTo(missionFile);

                    // Force idle.
                    missionFile.add(airGroup.Id, "Idle", "1");

                    GamePlay.gpPostMissionLoad(missionFile);

                    Debug(order.Unit.Id + " new order: " + airGroup.MissionType + "@" + airGroup.Altitude);
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

                if (airGroup.Army == Army.Red)
                {
                    redAirHeadquarters.Register(airGroup);
                }
                else if (airGroup.Army == Army.Blue)
                {
                    blueAirHeadquarters.Register(airGroup);
                }

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

        public override void OnActorCreated(int missionNumber, string shortName, maddox.game.world.AiActor actor)
        {
            base.OnActorCreated(missionNumber, shortName, actor);

            string id = actor.Name().Remove(0, actor.Name().IndexOf(":") + 1);
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirGroup)
                {
                    Units[id].RaisePending();
                    
                    Timeout(1 * 60, () =>
                    {
                        (actor as AiGroup).Idle = false;
                        Units[id].RaiseBusy();

                        // Destroy birth places.
                        if (GamePlay.gpBirthPlaces() != null && GamePlay.gpBirthPlaces().Length > 0)
                        {
                            foreach (AiBirthPlace birthPlace in GamePlay.gpBirthPlaces())
                            {
                                if (birthPlace.Name() == id)
                                {
                                    birthPlace.destroy();
                                }
                            }
                        }
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
