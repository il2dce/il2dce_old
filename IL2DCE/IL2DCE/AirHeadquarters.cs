using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class AirHeadquarters : IHeadquarters
    {
        private List<IUnit> Units
        {
            get
            {
                return this.units;
            }
        }
        private List<IUnit> units = new List<IUnit>();

        public AirHeadquarters(IPersistentWorld persistentWorld)
        {
            PersistentWorld = persistentWorld;
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
                unit.Idle += new UnitEventHandler(OnUnitIdle);

                PersistentWorld.Debug(unit.Id + " is now under control of AirHeadquarters.");
            }
        }

        public void Deregister(IUnit unit)
        {
            if (Units.Contains(unit))
            {
                Units.Remove(unit);
                unit.Idle -= new UnitEventHandler(OnUnitIdle);

                PersistentWorld.Debug(unit.Id + " is now release from control of AirHeadquarters.");
            }
        }

        void OnUnitIdle(object sender, UnitEventArgs e)
        {
            // TODO: Give new order
            PersistentWorld.Debug(e.Unit.Id + " is idle.");
        }
    }
}
