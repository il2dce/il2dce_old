// von Pilsner's AI Random for CloD / IL2DCG

// Give it aircraft name and skill and it will return tweaked AI skill values
// Ignores already customized skills so as not to mess up anything important!

// Call this with:
//	string Class="Aircraft.Blenheim";
//	string skillsLine = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
//	
//	AIRandom ai = new AIRandom(Class, skillsLine);
//	skillsLine = ai.ToString();

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace IL2DCE
{
    class AIRandom
    {
        private string pname;
        private string pskill;

        public AIRandom()
        {
            pname = "";
            pskill = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
        }

        public int RandomNumx(int min, int max)     // random digit generator - best so far...
        {                                           // you need this in head of doc: using System.Security.Cryptography;
            int result = 0;
            RNGCryptoServiceProvider c = new RNGCryptoServiceProvider();
            for (int x = 0; x < 20; x++)
            {
                // Create a byte array to hold the random values.
                byte[] randomNumber = new byte[4];
                // Fill the array with a random value.
                c.GetBytes(randomNumber);
                //Convert to a number
                result = Math.Abs(BitConverter.ToInt32(randomNumber, 0));
                //Console.WriteLine(result % max + min);
            }
            return (result % max + min);
        }

        public int RandomNumz()  // random digit generator - seems like poor distribution of #'s
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // Buffer storage.
                byte[] data = new byte[4];

                // Fill buffer.
                rng.GetBytes(data);

                // Convert to int 32.
                int value = BitConverter.ToInt32(data, 0);

                //Console.WriteLine(abs1);

                // int ib = Math.Abs(value);
                // ib = ib * 58;            // Change the distribution of #'s a little, so it's not 1 so much
                // while (ib >= 10)
                // {
                //     ib /= 10;
                // }

                int ib = Math.Abs(value);
                ib = ib * 58;            // Change the distribution of #'s a little
                while (ib >= 10)
                {
                    ib /= 10;
                }

                return (ib);
            }
        }

        public AIRandom(string pname, string pskill, string diff)	// Plane name, input skill, campaign difficulty
        {
            // Fighter
            string frookie = "0 0 0 0 0 0 0 0";
            string faverage = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
            string fexperienced = "0.5 0.5 0.5 0.5 0.5 0.5 0.5 0.5";
            string fveteran = "0.7 0.7 0.7 0.7 0.7 0.7 0.7 0.7";
            string face = "1 1 1 1 1 1 1 1";

            // Fighter Bomber
            string xrookie = "0 0 0 0 0 0 0 0";
            string xaverage = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
            string xexperienced = "0.5 0.5 0.5 0.5 0.5 0.5 0.5 0.5";
            string xveteran = "0.7 0.7 0.7 0.7 0.7 0.7 0.7 0.7";
            string xace = "1 1 1 1 1 1 1 1";

            // Bomber
            string brookie = "0 0 0 0 0 0 0 0";
            string baverage = "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3";
            string bexperienced = "0.5 0.5 0.5 0.5 0.5 0.5 0.5 0.5";
            string bveteran = "0.7 0.7 0.7 0.7 0.7 0.7 0.7 0.7";
            string bace = "1 1 1 1 1 1 1 1";

            this.pname = pname.Trim();
            int planeskill = 1;

            //int randomNumber = RandomNumz();
            int randomNumber = RandomNumx(0, 20);
            int difficult = Convert.ToInt32(diff);		// new
            difficult = Math.Abs(difficult);		// new

            randomNumber = randomNumber + difficult;	// new

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


            if (randomNumber < 8)
            {
                planeskill = 0;					// Rookie
            }
            if ((randomNumber >= 8) && (randomNumber < 13))
            {
                planeskill = 1;					// Average
            }
            if ((randomNumber >= 13) && (randomNumber < 18))
            {
                planeskill = 2;					// Experienced
            }
            if ((randomNumber >= 18) && (randomNumber < 23))
            {
                planeskill = 3;					// Veteran
            }
            if (randomNumber >= 23)
            {
                planeskill = 4;					// Ace
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
            if (planeskill == 2)    // if we have an Experienced pilot
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
            if (planeskill == 3)    // if we have an Veteran pilot
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
