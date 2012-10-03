using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public interface IBuilding : IDetector
    {
        string Id
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
    }
}
