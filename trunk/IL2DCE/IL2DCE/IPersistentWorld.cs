using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public interface IPersistentWorld
    {
        event EventHandler NextPhase;

        event UnitEventHandler UnitDiscovered;

        event UnitEventHandler UnitCovered;

        Map Map
        {
            get;
        }

        Dictionary<string, IUnit> Units
        {
            get;
        }

        Random Random
        {
            get;
        }

        maddox.game.ISectionFile AircraftInfoFile
        {
            get;
        }

        void Debug(string line);

        void TakeOrder(IOrder order);
    }
}
