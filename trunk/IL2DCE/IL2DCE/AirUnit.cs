using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class AirUnit : IUnit
    {
        public event UnitEventHandler Created;

        public event UnitEventHandler Destroyed;

        public event UnitEventHandler Idle;

        public event UnitEventHandler Busy;

        public string Id
        {
            get
            {
                return this.id;
            }
        }
        private string id;

        private IPersistentWorld PersistentWorld
        {
            get
            {
                return this.persistentWorld;
            }
        }
        private IPersistentWorld persistentWorld;


        public Tuple<double, double, double> Position
        {
            get
            {
                return new Tuple<double, double, double>(0.0, 0.0, 0.0);
            }
        }

        public UnitState State
        {
            get
            {
                return this.state;
            }
        }
        public UnitState state;
        
        public AirUnit(IPersistentWorld persistentWorld, string id)
        {
            this.persistentWorld = persistentWorld;
            this.id = id;
        }

        public void DetectUnit(IUnit unit)
        {
            
        }

        public void RaiseCreated()
        {
            Created(this, new UnitEventArgs(this));
        }

        public void RaiseDestroyed()
        {
            this.state = UnitState.Destroyed;

            Destroyed(this, new UnitEventArgs(this));
        }

        public void RaiseIdle()
        {
            this.state = UnitState.Idle;

            Idle(this, new UnitEventArgs(this));
        }

        public void RaiseBusy()
        {
            this.state = UnitState.Busy;

            Busy(this, new UnitEventArgs(this));
        }  
    }
}
