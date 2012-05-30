using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class PlaneType
    {
        private string pname;
        private string ptype;
        public PlaneType(string pname)
        {
            this.pname = pname.Trim();
            ptype = "0";       // We default to fighter for the time being... 

            // My crappy code to determine planetype, we only use this to set AI levels
            // int ptype = 0;  // 0= Fighter, 1=Dive Bomber/Ground Attack, 2=Level Bomber
            if (pname.IndexOf("Aircraft.Blenheim") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Anson") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Sunderland") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Walrus") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Wellington") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.BR-20") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Ju-87") != -1)
            {
                this.ptype = "1";
            }

            if (pname.IndexOf("Aircraft.Bf-109E-3B") != -1)
            {
                this.ptype = "1";
            }

            if (pname.IndexOf("Aircraft.Bf-109E-4B") != -1)
            {
                this.ptype = "1";
            }

            if (pname.IndexOf("Aircraft.Bf-110C-7") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Ju-88") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.He-111") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.FW-200") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Do-17") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Do-215") != -1)
            {
                this.ptype = "2";
            }

            if (pname.IndexOf("Aircraft.Defiant") != -1)
            {
                this.ptype = "2";
            }



        }
        public override string ToString()
        {
            return string.Format("{0}", this.ptype);
        }
    }
}
