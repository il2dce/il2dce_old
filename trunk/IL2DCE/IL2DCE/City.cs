using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;

namespace IL2DCE
{
    public class City : IStrategicPoint
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

        public City(IPersistentWorld persistentWorld, string id, ISectionFile missionFile)
        {
            this.id = id;
            string value = missionFile.get("FrontMarker", id);
            string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts.Length == 3)
            {
                double x;
                double y;
                if (double.TryParse(valueParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x)
                    && double.TryParse(valueParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y))
                {
                    this.position = new Tuple<double, double, double>(x, y, 0.0);
                }
                this.army = (IL2DCE.Army)Enum.Parse(Army.GetType(), valueParts[2]);
            }
        }
    }
}
