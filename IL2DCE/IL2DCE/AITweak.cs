// von Pilsner's AI Tweak for CloD / IL2DCG

// Give it aircraft name and skill and it will return tweaked AI skill values
// Ignores already customized skills so as not to mess up anything important!

// Call this with:
//	string Class="Aircraft.Blenheim";
//	string skillsLine = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
//	
//	AITweak ai = new AITweak(Class, skillsLine);
//	skillsLine = ai.ToString();

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class AITweak
    {
        private string pname;
        private string pskill;

        public AITweak()
        {
            pname = "";
            pskill = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
        }

        public AITweak(string pname, string pskill)
        {
            // Fighter (the leading space is important)
            string frookie = " 0.30 0.11 0.78 0.40 0.64 0.85 0.85 0.21";
            string faverage = " 0.32 0.12 0.87 0.60 0.74 0.90 0.90 0.31";
            string fexperienced = " 0.52 0.13 0.89 0.70 0.74 0.95 0.92 0.31";
            string fveteran = " 0.73 0.14 0.92 0.80 0.74 1 0.95 0.41";
            string face = " 0.93 0.15 0.96 0.92 0.84 1 1 0.51";

            // Fighter Bomber (the leading space is important)
            string xrookie = " 0.30 0.11 0.78 0.30 0.74 0.85 0.90 0.40";
            string xaverage = " 0.32 0.12 0.87 0.35 0.74 0.90 0.95 0.52";
            string xexperienced = " 0.52 0.13 0.89 0.38 0.74 0.92 0.95 0.52";
            string xveteran = " 0.73 0.14 0.92 0.40 0.74 0.95 0.95 0.55";
            string xace = " 0.93 0.15 0.96 0.45 0.74 1 1 0.65";

            // Bomber (the leading space is important)
            string brookie = " 0.30 0.11 0.78 0.20 0.74 0.85 0.90 0.88";
            string baverage = " 0.32 0.12 0.87 0.25 0.74 0.90 0.95 0.91";
            string bexperienced = " 0.52 0.13 0.89 0.28 0.74 0.92 0.95 0.91";
            string bveteran = " 0.73 0.14 0.92 0.30 0.74 0.95 0.95 0.95";
            string bace = " 0.93 0.15 0.96 0.35 0.74 1 1 0.97";

            this.pname = pname.Trim();

            // My crappy code to determine planetype, we only use this to set AI levels

            int planetype = 0;  // 0= Fighter, 1=Dive Bomber/Ground Attack, 2=Level Bomber
            // We default to fighter for the time being... 

            if (pname.IndexOf("Aircraft.Blenheim") != -1)
            {
                planetype = 1;
            }

            if (pname.IndexOf("Aircraft.Anson") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Sunderland") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Walrus") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Wellington") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.BR-20") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Ju-87") != -1)
            {
                planetype = 1;
            }

            if (pname.IndexOf("Aircraft.Ju-88") != -1)
            {
                planetype = 1;
            }

            if (pname.IndexOf("Aircraft.He-111") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.FW-200") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Do-17") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Do-215") != -1)
            {
                planetype = 2;
            }

            if (pname.IndexOf("Aircraft.Defiant") != -1)
            {
                planetype = 1;
            }

            //Now we know plane type - let's mess with skills...

            int planeskill = 11;     // 11 means - Don't Process Skill!!! It's already custom!!

            if (pskill.IndexOf("0 0 0 0 0 0 0 0") != -1)
            {
                planeskill = 0;     // Rookie
            }
            if (pskill.IndexOf("0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3") != -1)
            {
                planeskill = 1;     // Average
            }
            if (pskill.IndexOf("0.5 0.5 0.5 0.5 0.5 0.5 0.5 0.5") != -1)
            {
                planeskill = 2;     // Experienced
            }
            if (pskill.IndexOf("0.7 0.7 0.7 0.7 0.7 0.7 0.7 0.7") != -1)
            {
                planeskill = 3;     // Veteran
            }
            if (pskill.IndexOf("1 1 1 1 1 1 1 1") != -1)
            {
                planeskill = 4;     // ACE
            }

            // OK, we know plane type and pilot skill level - time to modify the AI

            if (planeskill == 0)    // if we have a rookie pilot
            {
                if (planetype == 0)   // Flying a Fighter
                {
                    this.pskill = frookie.Trim();
                }
                if (planetype == 1)   // Flying a Ground Attack
                {
                    this.pskill = xrookie.Trim();
                }
                if (planetype == 2)   // Flying a Level Bomber
                {
                    this.pskill = brookie.Trim();
                }
            }
            if (planeskill == 1)    // if we have an Average pilot
            {
                if (planetype == 0)   // Flying a Fighter
                {
                    this.pskill = faverage.Trim();
                }
                if (planetype == 1)   // Flying a Ground Attack
                {
                    this.pskill = xaverage.Trim();
                }
                if (planetype == 2)   // Flying a Level Bomber
                {
                    this.pskill = baverage.Trim();
                }
            }
            if (planeskill == 2)    // if we have a Experienced pilot
            {
                if (planetype == 0)   // Flying a Fighter
                {
                    this.pskill = fexperienced.Trim();
                }
                if (planetype == 1)   // Flying a Ground Attack
                {
                    this.pskill = xexperienced.Trim();
                }
                if (planetype == 2)   // Flying a Level Bomber
                {
                    this.pskill = bexperienced.Trim();
                }
            }
            if (planeskill == 3)    // if we have a Veteran pilot
            {
                if (planetype == 0)   // Flying a Fighter
                {
                    this.pskill = fveteran.Trim();
                }
                if (planetype == 1)   // Flying a Ground Attack
                {
                    this.pskill = xveteran.Trim();
                }
                if (planetype == 2)   // Flying a Level Bomber
                {
                    this.pskill = bveteran.Trim();
                }
            }
            if (planeskill == 4)    // if we have an ACE pilot
            {
                if (planetype == 0)   // Flying a Fighter
                {
                    this.pskill = face.Trim();
                }
                if (planetype == 1)   // Flying a Ground Attack
                {
                    this.pskill = xace.Trim();
                }
                if (planetype == 2)   // Flying a Level Bomber
                {
                    this.pskill = bace.Trim();
                }
            }
            if (planeskill == 11)    // Already a custom skill!!!
            {
                this.pskill = pskill.Trim();
            }
        }
        public override string ToString()
        {
            return string.Format("{0}", this.pskill);
        }
    }
}
