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

        private List<IUnit> Units
        {
            get
            {
                return this.units;
            }
        }
        private List<IUnit> units = new List<IUnit>();

        private List<IUnit> IdleUnits
        {
            get
            {
                List<IUnit> idleUnits = new List<IUnit>();
                foreach (IUnit unit in Units)
                {
                    if(unit.State == UnitState.Idle)
                    {
                        idleUnits.Add(unit);
                    }
                }
                return idleUnits;
            }
        }

        private List<IUnit> Enemies
        {
            get
            {
                return this.enemies;
            }
        }
        private List<IUnit> enemies = new List<IUnit>();

        public AirHeadquarters(IPersistentWorld persistentWorld, Army army)
        {
            PersistentWorld = persistentWorld;
            PersistentWorld.NextPhase += new EventHandler(OnNextPhase);
            PersistentWorld.UnitDiscovered += new UnitEventHandler(OnUnitDiscovered);

            this.army = army;
        }

        void OnUnitDiscovered(object sender, UnitEventArgs e)
        {
            throw new NotImplementedException();
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
            if (unit is AirUnit && !Units.Contains(unit))
            {
                Units.Add(unit);

                PersistentWorld.Debug(unit.Id + " is now under control of AirHeadquarters.");
            }
        }

        public void Deregister(IUnit unit)
        {
            if (Units.Contains(unit))
            {
                Units.Remove(unit);

                PersistentWorld.Debug(unit.Id + " is now release from control of AirHeadquarters.");
            }
        }

        void OnNextPhase(object sender, EventArgs e)
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
