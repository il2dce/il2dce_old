﻿// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
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

using maddox.game;
using maddox.game.play;
using maddox.game.page;

namespace IL2DCE
{
    namespace Pages
    {
        public class CampaignIntoPage : PageDefImpl
        {
            public CampaignIntoPage(IGame game)
                : base("Campaign into", new CampaignIntro())
            {
                _game = game;

                FrameworkElement.Fly.Click += new System.Windows.RoutedEventHandler(Fly_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
                FrameworkElement.comboBoxSelectAircraft.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(comboBoxSelectAircraft_SelectionChanged);

                if (Game.CurrentCampaign != null)
                {
                    Game.Core.Init(Game.CurrentCampaign.TemplateFileName);

                    System.Windows.Controls.ComboBoxItem itemArmyRed = new System.Windows.Controls.ComboBoxItem();
                    itemArmyRed.Content = "Red";
                    FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyRed);
                    System.Windows.Controls.ComboBoxItem itemArmyBlue = new System.Windows.Controls.ComboBoxItem();
                    itemArmyBlue.Content = "Blue";
                    FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyBlue);
                    FrameworkElement.comboBoxSelectArmy.SelectedIndex = 0;
                }
            }

            private CampaignIntro FrameworkElement
            {
                get
                {
                    return FE as CampaignIntro;
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

            private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            private void Fly_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.Core.Generate();
                Game.Core.Load();
                
                Game.gameInterface.UIMainHide();
                Game.gameInterface.BattleStart();
            }

            private void comboBoxSelectArmy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    FrameworkElement.comboBoxSelectAirGroup.Items.Clear();
                    System.Windows.Controls.ComboBoxItem itemSelected = e.AddedItems[0] as System.Windows.Controls.ComboBoxItem;
                    if ((string)itemSelected.Content == "Red")
                    {
                        foreach (IAirGroup airGroup in Game.Core.RedAirGroups)
                        {
                            if (airGroup.AircraftInfo.IsFlyable)
                            {
                                System.Windows.Controls.ComboBoxItem itemAirGroup = new System.Windows.Controls.ComboBoxItem();
                                itemAirGroup.Content = airGroup.AirGroupKey + "." + airGroup.SquadronIndex + "(" + airGroup.AircraftInfo.Aircraft + ")";
                                itemAirGroup.Tag = airGroup;
                                FrameworkElement.comboBoxSelectAirGroup.Items.Add(itemAirGroup);
                            }
                        }
                    }

                    if ((string)itemSelected.Content == "Blue")
                    {
                        foreach (IAirGroup airGroup in Game.Core.BlueAirGroups)
                        {
                            if (airGroup.AircraftInfo.IsFlyable)
                            {
                                System.Windows.Controls.ComboBoxItem itemAirGroup = new System.Windows.Controls.ComboBoxItem();
                                itemAirGroup.Content = airGroup.AirGroupKey + "." + airGroup.SquadronIndex + "(" + airGroup.AircraftInfo.Aircraft + ")";
                                itemAirGroup.Tag = airGroup;
                                FrameworkElement.comboBoxSelectAirGroup.Items.Add(itemAirGroup);
                            }
                        }
                    }

                    if (FrameworkElement.comboBoxSelectAirGroup.Items.Count > 0)
                    {
                        FrameworkElement.comboBoxSelectAirGroup.SelectedIndex = 0;
                    }
                    else
                    {
                        FrameworkElement.comboBoxSelectAirGroup.SelectedIndex = -1;
                    }
                }
            }

            private void comboBoxSelectAirGroup_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    FrameworkElement.comboBoxSelectAircraft.Items.Clear();

                    System.Windows.Controls.ComboBoxItem itemSelected = e.AddedItems[0] as System.Windows.Controls.ComboBoxItem;
                    IAirGroup airGroup = itemSelected.Tag as IAirGroup;

                    foreach (int flightIndex in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[flightIndex].Count > 0)
                        {
                            foreach (string acNumber in airGroup.Flights[flightIndex])
                            {
                                System.Windows.Controls.ComboBoxItem itemAircraft = new System.Windows.Controls.ComboBoxItem();
                                itemAircraft.Content = acNumber;
                                Tuple<int, int> tupel = new Tuple<int, int>(flightIndex, airGroup.Flights[flightIndex].IndexOf(acNumber));
                                itemAircraft.Tag = tupel;
                                FrameworkElement.comboBoxSelectAircraft.Items.Add(itemAircraft);
                            }
                        }
                    }

                    Game.Core.PlayerAirGroup = airGroup;
                    Game.Core.PlayerAirGroupKey = airGroup.AirGroupKey;
                    Game.Core.PlayerSquadronIndex = airGroup.SquadronIndex;

                    if (FrameworkElement.comboBoxSelectAircraft.Items.Count > 0)
                    {
                        FrameworkElement.comboBoxSelectAircraft.SelectedIndex = 0;
                    }
                    else
                    {
                        FrameworkElement.comboBoxSelectAircraft.SelectedIndex = -1;
                    }
                }
            }

            private void comboBoxSelectAircraft_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    System.Windows.Controls.ComboBoxItem itemSelected = e.AddedItems[0] as System.Windows.Controls.ComboBoxItem;
                    Tuple<int, int> tupel = itemSelected.Tag as Tuple<int, int>;
                    Game.Core.PlayerFlightIndex = tupel.Item1;
                    Game.Core.PlayerAircraftIndex = tupel.Item2;
                }
            }
        }
    }
}