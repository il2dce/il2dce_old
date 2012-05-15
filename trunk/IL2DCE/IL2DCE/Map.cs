using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;

namespace IL2DCE
{
    public class Map
    {
        public List<IStrategicPoint> StrategicPoints
        {
            get
            {
                return this.strategicPoints;
            }
        }
        private List<IStrategicPoint> strategicPoints = new List<IStrategicPoint>();

        public Map(IPersistentWorld persistentWorld, ISectionFile missionFile)
        {
            for (int i = 0; i < missionFile.lines("FrontMarker"); i++)
            {
                string key;
                string value;
                missionFile.get("FrontMarker", i, out key, out value);

                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length == 3)
                {
                    double x;
                    double y;
                    int army;
                    if (double.TryParse(valueParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x)
                        && double.TryParse(valueParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y)
                        && int.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out army))
                    {                        
                        StrategicPoints.Add(new City(persistentWorld, key, missionFile));
                    }
                }
            }
        }
    }
}
