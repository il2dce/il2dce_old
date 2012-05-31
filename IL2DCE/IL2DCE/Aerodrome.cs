using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;

namespace IL2DCE
{
    public class Aerodrome : IStrategicPoint
    {
        public string Id
        {
            get
            {
                return this.id;
            }
        }
        private string id;

        public Army Army
        {
            get
            {
                return this.army;
            }
        }
        private Army army;

        public Tuple<double, double, double> Position
        {
            get
            {
                return this.position;
            }
        }
        private Tuple<double, double, double> position;

        public Aerodrome()
        {
            this.id = "";
            this.army = IL2DCE.Army.None;
            this.position = new Tuple<double, double, double>(0.0, 0.0, 0.0);
        }
    }
}
