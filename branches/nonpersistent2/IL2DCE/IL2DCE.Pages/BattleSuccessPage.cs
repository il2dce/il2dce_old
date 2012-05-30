// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2011 Stefan Rothdach
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using maddox.game;
using maddox.game.play;
using maddox.game.page;

namespace IL2DCE
{
    namespace Pages
    {
        public class BattleSuccessPage : PageDefImpl
        {
            public BattleSuccessPage()
                : base("Battle Success", new CampaignBattleSuccess())
            {
                FrameworkElement.Fly.Click += new System.Windows.RoutedEventHandler(Fly_Click);
                FrameworkElement.ReFly.Click += new System.Windows.RoutedEventHandler(ReFly_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
            }

            void ReFly_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PageChange(new BattleIntroPage(), null);
            }

            void Back_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            void Fly_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.Core.AdvanceCampaign(Game);

                Game.gameInterface.PageChange(new BattleIntroPage(), null);
            }



            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                if (Game is IGameSingle)              /// Changed by vP
                {

                    FrameworkElement.textBoxDescription.Text = "Mission Report: ";
                    FrameworkElement.textBoxDescription.Text += Game.Core.Career.CampaignInfo + "     ";
                    string tmpX = Game.Core.Career.Date.ToString();
                    tmpX = tmpX.Replace(" ", "_");
                    string[] tmp = Regex.Split(tmpX, @"_");
                    FrameworkElement.textBoxDescription.Text += tmp[0] + "\n";
                    FrameworkElement.textBoxDescription.Text += "\n";

                    // Get Army and Rank
                    string armee = Game.Core.Career.ArmyIndex.ToString();
                    int army = Convert.ToInt32(armee);
                    string rankz = Game.Core.Career.RankIndex.ToString();
                    int rank = Convert.ToInt32(rankz);

                    if (rank == 0)
                    {
                        if (army == 1)
                        {
                            FrameworkElement.textBoxDescription.Text += "Pilot Officer";
                        }
                        else
                        {
                            FrameworkElement.textBoxDescription.Text += "Leutnant";
                        }
                    }
                    if (rank == 1)
                    {
                        if (army == 1)
                        {
                            FrameworkElement.textBoxDescription.Text += "Flying Officer";
                        }
                        else
                        {
                            FrameworkElement.textBoxDescription.Text += "Oberleutnant";
                        }
                    }

                    if (rank == 2)
                    {
                        if (army == 1)
                        {
                            FrameworkElement.textBoxDescription.Text += "Flight Lieutenant";
                        }
                        else
                        {
                            FrameworkElement.textBoxDescription.Text += "Hauptmann";
                        }
                    }

                    if (rank == 3)
                    {
                        if (army == 1)
                        {
                            FrameworkElement.textBoxDescription.Text += "Squadron Leader";
                        }
                        else
                        {
                            FrameworkElement.textBoxDescription.Text += "Major";
                        }
                    }

                    if (rank == 4)
                    {
                        if (army == 1)
                        {
                            FrameworkElement.textBoxDescription.Text += "Wing Commander";
                        }
                        else
                        {
                            FrameworkElement.textBoxDescription.Text += "Oberstleutnant";
                        }
                    }

                    if (rank >= 5)
                    {
                        if (army == 1)
                        {
                            FrameworkElement.textBoxDescription.Text += "Group Captain";
                        }
                        else
                        {
                            FrameworkElement.textBoxDescription.Text += "Oberst";
                        }
                    }

                    FrameworkElement.textBoxDescription.Text += " ";
                    FrameworkElement.textBoxDescription.Text += Game.Core.Career.PilotName + "\n";

                    if (army == 1)
                    {
                        FrameworkElement.textBoxDescription.Text += "Squadron: ";
                    }
                    else
                    {
                        FrameworkElement.textBoxDescription.Text += "Staffel: ";
                    }

                    tmpX = Game.Core.Career.AirGroup;
                    tmpX = tmpX.Replace("_", " ");
                    tmpX = tmpX.Replace(".", "_");
                    tmp = Regex.Split(tmpX, @"_");
                    FrameworkElement.textBoxDescription.Text += tmp[0] + "\n";
                    string missns = Game.Core.Career.Missions.ToString();
                    int mission = Convert.ToInt32(missns);
                    mission = mission + 1;
                    FrameworkElement.textBoxDescription.Text += "Missions Flown: " + mission + " \n";
                    FrameworkElement.textBoxDescription.Text += "\n";
                    FrameworkElement.textBoxDescription.Text += " Mission Results:";
                    FrameworkElement.textBoxDescription.Text += " " + (Game as IGameSingle).BattleSuccess.ToString();
                    FrameworkElement.textBoxDescription.Text += "\n";

                    FrameworkElement.textBoxDescription.Text += Game.Core.Careers.ToString();

                    FrameworkElement.textBoxDescription.Text += "\n";
                    string gp = Game.Core.Career.CampaignInfo.TemplateFilePath;
                   // gp = Game.Core.Career.
                    //FrameworkElement.textBoxDescription.Text += gp + "\n";
                        
                        ////////
                    string newproblemza = "1";
                    int ANAme = Game.Core.Career.Phase;
                    string CampInfoFileName = "CampaignInfo" + ANAme + ".ini";
                    FrameworkElement.textBoxDescription.Text += "Phase: " + ANAme + "\n";
                    FrameworkElement.textBoxDescription.Text += "File Name: " + CampInfoFileName + "\n";
                    FrameworkElement.textBoxDescription.Text += "\n";

                    string df = Game.Core.Career.Difficulty.ToString();
                    double af = Convert.ToDouble(df);
                    double ez = af / 10;
                    double ed = 50 * ez;
                    int ex = Convert.ToInt32(ed);
                    int exps = ex + 175;
                    int expd = ex + 75;
                    // FrameworkElement.textBoxDescription.Text += " af" + af + " ex" + ex + " ed" + ed + " exps" + exps + " expd" + expd; 

                    if ((Game as IGameSingle).BattleSuccess == EBattleResult.DRAW)
                    {
                        FrameworkElement.textBoxDescription.Text += " Experience: " + exps;
                        FrameworkElement.textBoxDescription.Text += "\n";
                        FrameworkElement.textBoxDescription.Text += " Career Experience: " + (Game.Core.Career.Experience + exps) + " / " + ((Game.Core.Career.RankIndex + 1) * 1000);
                        // FrameworkElement.textBoxDescription.Text += " Experience: " + Game.Core.Career.Experience + " " + expd + "/" + ((Game.Core.Career.RankIndex + 1) * 1000);
                        FrameworkElement.textBoxDescription.Text += "\n";
                        if (Game.Core.Career.Experience + exps >= (Game.Core.Career.RankIndex + 1) * 1000)
                        {
                            FrameworkElement.textBoxDescription.Text += "\n";
                            if (army == 1)
                            {

                                string newrank = Career.RafRanks[Game.Core.Career.RankIndex + 1].ToString();
                                FrameworkElement.textBoxDescription.Text += "** Congratulations, you have earned a promition to " + newrank + ". **" + "\n";
                            }
                            else
                            {
                                string newrank = Career.LwRanks[Game.Core.Career.RankIndex + 1].ToString();
                                FrameworkElement.textBoxDescription.Text += "** Herzlichen Glückwunsch, sie wurden zum  " + newrank + " befördert. **" + "\n";
                            }
                        }
                    }
                    else
                    {
                        FrameworkElement.textBoxDescription.Text += " Experience: " + exps ;
                        FrameworkElement.textBoxDescription.Text += "\n";
                        FrameworkElement.textBoxDescription.Text += " Career Experience: " + (Game.Core.Career.Experience + exps) + " / " + ((Game.Core.Career.RankIndex + 1) * 1000);
                        // FrameworkElement.textBoxDescription.Text += " Experience: " + Game.Core.Career.Experience + " " + expd + "/" + ((Game.Core.Career.RankIndex + 1) * 1000);
                        FrameworkElement.textBoxDescription.Text += "\n";
                        if (Game.Core.Career.Experience + exps >= (Game.Core.Career.RankIndex + 1) * 1000)
                        {
                            FrameworkElement.textBoxDescription.Text += "\n";
                            if (army == 1)
                            {
                                string newrank = Career.RafRanks[Game.Core.Career.RankIndex + 1].ToString();
                                FrameworkElement.textBoxDescription.Text += "\n";
                                //FrameworkElement.textBoxDescription.Text += "Congratulations, you have earned a Promition!" + "\n";
                                FrameworkElement.textBoxDescription.Text += "** Congratulations, you have earned a promition to " + newrank + ". **" + "\n";
                            }
                            else
                            {
                                string newrank = Career.LwRanks[Game.Core.Career.RankIndex + 1].ToString();
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += "** Herzlichen Glückwunsch, sie wurden zum  " + newrank + " befördert. **" + "\n";
                            }
                        }
                    }

                    // Going the long way to get a medal name...

                    string tmpY = Game.Core.Career.ToString();
                    tmp = Regex.Split(tmpY, @",");
                    string rankandname = tmp[0];
                    string medals = tmp[1];         // string of medals 100000000
                    string difficulty = tmp[2];
                    int medalss = Convert.ToInt32(medals);      // int of medals 100000000
                    int medalss0 = GetDigit(medalss, 9);        // Placeholeder - should always return 1
                    int medalss1 = GetDigit(medalss, 8);        // lowest medal
                    int medalss2 = GetDigit(medalss, 7);
                    int medalss3 = GetDigit(medalss, 6);
                    int medalss4 = GetDigit(medalss, 5);
                    int medalss5 = GetDigit(medalss, 4);
                    int medalss6 = GetDigit(medalss, 3);
                    int medalss7 = GetDigit(medalss, 2);
                    int medalss8 = GetDigit(medalss, 1);        // Highest Medal

                    if (medals != "100000000")
                    {
                        // Sanity Check!!!
                        // this is where we limit the amount of medalsss for each type

                        if (medalss0 > 8)
                        {
                            medalss0 = 8;
                        }
                        if (medalss1 > 8)
                        {
                            medalss1 = 8;
                        }
                        if (medalss2 > 8)
                        {
                            medalss2 = 8;
                        }
                        if (medalss3 > 8)
                        {
                            medalss3 = 8;
                        }
                        if (medalss4 > 8)
                        {
                            medalss4 = 8;
                        }
                        if (medalss5 > 8)
                        {
                            medalss5 = 8;
                        }
                        if (medalss6 > 8)
                        {
                            medalss6 = 8;
                        }
                        if (medalss7 > 8)
                        {
                            medalss7 = 8;
                        }
                        if (medalss8 > 8)
                        {
                            medalss8 = 8;
                        }

                        //FrameworkElement.textBoxDescription.Text += "\n";
                        FrameworkElement.textBoxDescription.Text += "\n";
                        FrameworkElement.textBoxDescription.Text += "Medals:";
                        if (army == 2)
                        {
                            if (medalss1 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss1 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss1 - 1);
                                }
                            }
                            if (medalss2 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss2 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss2 - 1);
                                }
                            }
                            if (medalss3 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss3 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss3 - 1);
                                }
                            }
                            if (medalss4 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss4 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss4 - 1);
                                }
                            }
                            if (medalss5 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss5 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss5 - 1);
                                }
                            }
                            if (medalss6 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss6 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss6 - 1);
                                }
                            }
                            if (medalss7 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss7 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss7 - 1);
                                }
                            }
                            if (medalss8 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Iron Cross 2nd Class ";
                                if (medalss8 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss8 - 1);
                                }
                            }
                            FrameworkElement.textBoxDescription.Text += "\n";
                        }

                        if (army != 2)
                        {
                            if (medalss1 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Defence Medal ";
                                if (medalss1 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss1 - 1);
                                }
                            }
                            if (medalss2 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Distinguished Flying Medal ";
                                if (medalss2 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss2 - 1);
                                }
                            }
                            if (medalss3 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Distinguished Flying Cross ";
                                if (medalss3 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss3 - 1);
                                }
                            }
                            if (medalss4 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Bar for the Distinguished Flying Cross ";
                                if (medalss4 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss4 - 1);
                                }
                            }
                            if (medalss5 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Distinguished Service Order ";
                                if (medalss5 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss5 - 1);
                                }
                            }
                            if (medalss6 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " George Cross ";
                                if (medalss6 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss6 - 1);
                                }
                            }
                            if (medalss7 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " Victoria Cross ";
                                if (medalss7 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss7 - 1);
                                }
                            }
                            if (medalss8 > 0)
                            {
                                FrameworkElement.textBoxDescription.Text += "\n";
                                FrameworkElement.textBoxDescription.Text += " 1939 - 1945 Star ";
                                if (medalss8 > 1)
                                {
                                    FrameworkElement.textBoxDescription.Text += " x" + (medalss8 - 1);
                                }
                            }
                        }
                        FrameworkElement.textBoxDescription.Text += "\n";
                    }
                    // FrameworkElement.textBoxDescription.Text += "\n";
                    FrameworkElement.textBoxDescription.Text += "\n";
                    FrameworkElement.textBoxDescription.Text += "Campaign Difficulty: ";
                    FrameworkElement.textBoxDescription.Text += " " + Game.Core.Career.Difficulty;
                    FrameworkElement.textBoxDescription.Text += "\n";
                }
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            private CampaignBattleSuccess FrameworkElement
            {
                get
                {
                    return FE as CampaignBattleSuccess;
                }
            }

            private IGame Game
            {
                get
                {
                    return _game;
                }
            }
            private IGame _game;


            static int GetDigit(int number, int digit)
            {
                return (number / (int)Math.Pow(10, digit - 1)) % 10;
            }
        }
    }
}