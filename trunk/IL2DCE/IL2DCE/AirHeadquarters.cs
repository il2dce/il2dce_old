using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class AirHeadquarters : IHeadquarters
    {
        private Army Army
        {
            get
            {
                return this.army;
            }
        }
        private Army army;

        private List<IUnit> FriendlyUnits
        {
            get
            {
                return this.friendlyUnits;
            }
        }
        private List<IUnit> friendlyUnits = new List<IUnit>();

        private List<IUnit> EnemyUnits
        {
            get
            {
                return this.enemyUnits;
            }
        }
        private List<IUnit> enemyUnits = new List<IUnit>();

        private List<IUnit> IdleUnits
        {
            get
            {
                List<IUnit> idleUnits = new List<IUnit>();
                foreach (IUnit unit in FriendlyUnits)
                {
                    if(unit.State == UnitState.Idle)
                    {
                        idleUnits.Add(unit);
                    }
                }
                return idleUnits;
            }
        }
        
        public AirHeadquarters(IPersistentWorld persistentWorld, Army army)
        {
            PersistentWorld = persistentWorld;
            PersistentWorld.MissionSlice += new EventHandler(OnMissionSlice);

            PersistentWorld.UnitDiscovered += new UnitEventHandler(OnUnitDiscovered);
            PersistentWorld.UnitCovered += new UnitEventHandler(OnUnitCovered);

            this.army = army;
        }

        void OnUnitDiscovered(object sender, UnitEventArgs e)
        {
            PersistentWorld.Debug(e.Unit.Id + " detected. Intercept!");
        }

        void OnUnitCovered(object sender, UnitEventArgs e)
        {
            PersistentWorld.Debug(e.Unit.Id + " lost. RTB!");
        }

        public IPersistentWorld PersistentWorld
        {
            set
            {
                this.persistentWorld = value;
            }
            get
            {
                return this.persistentWorld;
            }
        }
        private IPersistentWorld persistentWorld;

        public void Register(IUnit unit)
        {
            if (unit is AirUnit && !FriendlyUnits.Contains(unit))
            {
                FriendlyUnits.Add(unit);

                PersistentWorld.Debug(unit.Id + " is now under control of AirHeadquarters.");
            }
        }

        public void Deregister(IUnit unit)
        {
            if (FriendlyUnits.Contains(unit))
            {
                FriendlyUnits.Remove(unit);

                PersistentWorld.Debug(unit.Id + " is now release from control of AirHeadquarters.");
            }
        }

        void OnMissionSlice(object sender, EventArgs e)
        {
            if(IdleUnits.Count > 0)
            {
                int unitIndex = PersistentWorld.Random.Next(IdleUnits.Count);
                int strategicPointIndex = PersistentWorld.Random.Next(PersistentWorld.Map.StrategicPoints.Count);

                PersistentWorld.TakeOrder(new AirOrder(IdleUnits[unitIndex], PersistentWorld.Map.StrategicPoints[strategicPointIndex]));
            }
        }
    }
}
