using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;

namespace IL2DCE
{
    public class Radar : IBuilding
    {
        public string Id
        {
            get
            {
                return this.id;
            }
        }
        private string id;
        
        public Tuple<double, double, double> Position
        {
            get
            {
                return this.position;
            }
        }
        private Tuple<double, double, double> position;
        
        public Army Army
        {
            get
            {
                return this.army;
            }
        }
        private Army army;

        public Radar(IPersistentWorld persistentWorld, string id, ISectionFile missionFile)
        {
            this.id = id;
            string value = missionFile.get("Stationary", id);
            string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts.Length > 4)
            {
                string army = valueParts[1];
                if (army == "gb")
                {
                    this.army = IL2DCE.Army.Red;
                }
                else if (army == "de")
                {
                    this.army = IL2DCE.Army.Blue;
                }
                else
                {
                    this.army = IL2DCE.Army.None;
                }                
                double x = double.Parse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                double y = double.Parse(valueParts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                this.position = new Tuple<double,double,double>(x, y, 0.0);
            }
        }

        public void DetectUnit(IUnit unit)
        {

        }
    }
}
