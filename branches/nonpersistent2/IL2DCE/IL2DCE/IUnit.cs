using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public interface IUnit : IDetector
    {
        event UnitEventHandler Idle;

        event UnitEventHandler Pending;

        event UnitEventHandler Busy;

        event UnitEventHandler Destroyed;

        string Id
        {
            get;
        }

        UnitState State
        {
            get;
        }

        Army Army
        {
            get;
        }

        Tuple<double, double, double> Position
        {
            get;
        }

        void RaiseIdle();

        void RaisePending();

        void RaiseBusy();

        void RaiseDestroyed();
    }
}
