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
        protected string airUnitFileName = "";
        protected string aircraftInfoFileName = "";

        public event EventHandler NextPhase;
        
        public Dictionary<string, IUnit> Units
        {
            get
            {
                return this.units;
            }
        }
        private Dictionary<string, IUnit> units = new Dictionary<string, IUnit>();

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
        
        private void RaisePeriodicPhase()
        {
            RaiseNextPhase();

            Timeout(5 * 60, () => 
            {
                RaisePeriodicPhase();
            });
        }

        private void RaiseNextPhase()
        {
            if (NextPhase != null)
            {
                NextPhase(this, new EventArgs());
            }
        }

        public void NewMission(IUnit unit)
        {
            if (unit is AirUnit)
            {
                AirUnit airUnit = unit as AirUnit;
                
                ISectionFile missionFile = GamePlay.gpCreateSectionFile();
                Generator generator = new Generator(null);
                
                AirGroup airGroup = new AirGroup();
                airGroup.Id = airUnit.Id;
                airGroup.AirGroupKey = airUnit.Regiment;
                airGroup.Class = airUnit.AircraftType;
                airGroup.CallSign = 1;
                airGroup.Fuel = 100;
                airGroup.Formation = airUnit.AirGroupInfo.DefaultFormation;
                airGroup.Airstart = false;
                airGroup.Position = new maddox.GP.Point3d(airUnit.Position.Item1, airUnit.Position.Item2, airUnit.Position.Item3);

                airGroup.Flights[0] = new List<string>() {"1", "2"};




                airGroup.Transfer(EMissionType.RECON, 1000);

                airGroup.writeTo(missionFile);

                GamePlay.gpPostMissionLoad(missionFile);
            }

            Debug(unit.Id + " new mission.");
        }
        
        #region AMission

        public override void OnBattleInit()
        {
            base.OnBattleInit();

            MissionNumberListener = -1;

            random = new System.Random();

            // Parse mission file and fill the unit dictionary.
            ISectionFile missionFile = GamePlay.gpLoadSectionFile(airUnitFileName);
            this.aircraftInfoFile = GamePlay.gpLoadSectionFile(aircraftInfoFileName);
            
            this.map = new IL2DCE.Map(this, missionFile);

            AirHeadquarters redAirHeadquarters = new AirHeadquarters(this, Army.Red);
            Headquarters.Add(redAirHeadquarters);

            AirHeadquarters blueAirHeadquarters = new AirHeadquarters(this, Army.Blue);
            Headquarters.Add(blueAirHeadquarters);
            
            for (int i = 0; i < missionFile.lines("AirUnits"); i++)
            {
                string key;
                string value;
                missionFile.get("AirUnits", i, out key, out value);
                AirUnit airUnit = new AirUnit(this, key, missionFile);
                Units.Add(key, airUnit);

                if (airUnit.Army == Army.Red)
                {
                    redAirHeadquarters.Register(airUnit);
                }
                else if (airUnit.Army == Army.Blue)
                {
                    blueAirHeadquarters.Register(airUnit);
                }

                //airUnit.RaiseIdle();
            }
        }

        public override void OnBattleStarted()
        {
            base.OnBattleStarted();
            
            RaisePeriodicPhase();
        }

        public override void OnActorCreated(int missionNumber, string shortName, maddox.game.world.AiActor actor)
        {
            base.OnActorCreated(missionNumber, shortName, actor);

            string id = actor.Name().Remove(0, actor.Name().IndexOf(":") + 1);
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirUnit)
                {
                    Units[id].RaisePending();

                    Timeout(1 * 60, () =>
                    {
                        Units[id].RaiseBusy();
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
                if (Units[id] is AirUnit)
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
                if (Units[id] is AirUnit)
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
