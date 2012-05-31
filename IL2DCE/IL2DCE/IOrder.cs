using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public interface IOrder
    {
        IUnit Unit
        {
            get;
        }

        IStrategicPoint Target
        {
            get;
        }
    }
}
