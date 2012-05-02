using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    interface IPersistentWorld
    {
        Map Map
        {
            get;
        }

        Dictionary<string, IUnit> Units
        {
            get;
        }

        void Debug(string line);
    }    
}
