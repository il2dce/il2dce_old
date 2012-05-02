using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;

namespace IL2DCE
{
    public class Mission : maddox.game.AMission, IPersistentWorld
    {
        protected string fileName = "";
        
        public Dictionary<string, IUnit> Units
        {
            get
            {
                return this.units;
            }
        }
        private Dictionary<string, IUnit> units = new Dictionary<string, IUnit>();

        public Map Map
        {
            get
            {
                return this.map;
            }
        }
        private Map map;

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
        
        #region AMission

        public override void OnBattleInit()
        {
            base.OnBattleInit();
            
            Debug("Initialize battle ...");

            // Parse mission file and fill the unit dictionary.
            maddox.game.ISectionFile missionFile = GamePlay.gpLoadSectionFile(fileName);
            
            this.map = new IL2DCE.Map(missionFile);

            AirHeadquarters airHeadquarters = new AirHeadquarters(this);

            for (int i = 0; i < missionFile.lines("AirGroups"); i++)
            {
                string key;
                string value;
                missionFile.get("AirGroups", i, out key, out value);
                AirUnit airUnit = new AirUnit(this, key);
                Units.Add(key, airUnit);

                airHeadquarters.Register(airUnit);

                if (missionFile.get(key, "SpawnFromScript") == "1")
                {
                    airUnit.RaiseIdle();
                }
                else
                {
                    airUnit.RaiseBusy();
                }
            }
        }

        public override void OnActorCreated(int missionNumber, string shortName, maddox.game.world.AiActor actor)
        {
            base.OnActorCreated(missionNumber, shortName, actor);

            string id = actor.Name().Replace(missionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ":", "");
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirUnit)
                {
                    Units[id].RaiseBusy();
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

            string id = actor.Name().Replace(missionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ":", "");
            if (Units.ContainsKey(id))
            {
                if (Units[id] is AirUnit)
                {
                    Units[id].RaiseIdle();
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

            string id = actor.Name().Replace(missionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ":", "");
            if (Units.ContainsKey(id))
            {
                //if (Units[id] is GroundUnit)
                //{
                //    Units[id].RaiseIdle();
                //}
            }
        }

        #endregion
    }
}
