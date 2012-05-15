using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    public class UnitEventArgs : EventArgs
    {
        public IUnit Unit
        {
            get
            {
                return this.unit;
            }
        }
        private IUnit unit;

        public UnitEventArgs(IUnit unit)
        {
            this.unit = unit;
        }
    }

    public delegate void UnitEventHandler(object sender, UnitEventArgs e);
}
