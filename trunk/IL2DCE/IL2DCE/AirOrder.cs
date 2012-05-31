using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class AirOrder : IOrder
    {
        public IUnit Unit
        {
            get
            {
                return this.unit;
            }
        }
        private IUnit unit;

        public IStrategicPoint Target
        {
            get
            {
                return this.target;
            }
        }
        private IStrategicPoint target;

        public AirOrder(IUnit unit, IStrategicPoint target)
        {
            this.unit = unit;
            this.target = target;
        }
    }
}
