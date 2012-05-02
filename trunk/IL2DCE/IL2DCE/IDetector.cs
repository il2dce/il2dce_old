using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public interface IDetector
    {
        void DetectUnit(IUnit unit);
    }
}
