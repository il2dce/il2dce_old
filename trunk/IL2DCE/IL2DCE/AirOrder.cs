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
                return this.airGroup;
            }
        }
        private AirGroup airGroup;

        public AirGroup AirGroup
        {
            get
            {
                return this.airGroup;
            }
        }
        
        public AirGroup EscortAirGroup
        {
            get
            {
                return this.escortAirGroup;
            }
        }
        private AirGroup escortAirGroup;

        public IStrategicPoint Target
        {
            get
            {
                return this.target;
            }
        }
        private IStrategicPoint target;

        public AirOrder(AirGroup airGroup, AirGroup escortAirGroup, IStrategicPoint target)
        {
            this.airGroup = airGroup;
            this.escortAirGroup = escortAirGroup;
            this.target = target;
        }
    }
}
