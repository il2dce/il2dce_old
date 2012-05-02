using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public class UnitEventArgs : EventArgs
    {
        public IUnit Unit
        {
            get
            {
                return this.unit;
            }
        }
        private IUnit unit;

        public UnitEventArgs(IUnit unit)
        {
            this.unit = unit;
        }
    }

    public delegate void UnitEventHandler(object sender, UnitEventArgs e);

    public enum UnitState
    {
        Idle,
        Pending,
        Busy,
        Destroyed,
    }

    public interface IUnit : IDetector
    {
        event UnitEventHandler Created;

        event UnitEventHandler Destroyed;

        event UnitEventHandler Idle;

        event UnitEventHandler Busy;
        
        string Id
        {
            get;
        }

        UnitState State
        {
            get;
        }        

        Tuple<double, double, double> Position
        {
            get;
        }

        void RaiseCreated();

        void RaiseDestroyed();

        void RaiseIdle();

        void RaiseBusy();
    }
}
