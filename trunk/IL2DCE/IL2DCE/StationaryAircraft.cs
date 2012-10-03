using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using maddox.game;

namespace IL2DCE
{
    public class StationaryAircraft : IBuilding
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
        
        private IPersistentWorld PersistentWorld
        {
            get
            {
                return this.persistentWorld;
            }
        }
        private IPersistentWorld persistentWorld;

        public StationaryAircraft(IPersistentWorld persistentWorld, string id, ISectionFile missionFile)
        {
            this.persistentWorld = persistentWorld;

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

            //persistentWorld.DetectionSlice += new EventHandler(OnDetectionSlice);
        }

        void OnDetectionSlice(object sender, EventArgs e)
        {
            //foreach (IUnit unit in PersistentWorld.Units.Values)
            //{
            //    if (unit.Army != Army && unit is AirGroup)
            //    {
            //        double distance = Math.Sqrt(Math.Pow(unit.Position.Item1 - this.Position.Item1, 2) + Math.Pow(unit.Position.Item2 - this.Position.Item2, 2) + Math.Pow(unit.Position.Item3 - this.Position.Item3, 2));
            //        if (distance < 50000.0)
            //        {
            //            unit.RaiseDiscovered();
            //        }
            //        else
            //        {
            //            unit.RaiseCovered();
            //        }
            //    }
            //}
        }
    }
}
