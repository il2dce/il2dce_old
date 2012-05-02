using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    interface IStrategicPoint
    {
        string Id
        {
            get;
        }

        Tuple<double, double, double> Position
        {
            get;
        }
    }
}
