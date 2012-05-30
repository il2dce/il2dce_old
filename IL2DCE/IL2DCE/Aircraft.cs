using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE 
{
    public class Aircraft : IVehicle
    {
        public string Id
        {
            get
            {
                return this.id;
            }
        }
        private string id = "";

        string Serial
        {
            get
            {
                return this.serial;
            }
        }
        private string serial = "";
    }
}
