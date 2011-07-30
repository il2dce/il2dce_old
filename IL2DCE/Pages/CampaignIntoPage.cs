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
                FrameworkElement.listBoxAirGroups.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(listBoxAirGroup_SelectionChanged);
                FrameworkElement.listBoxAircraft.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(listBoxAircraft_SelectionChanged);

                Game.Core.Init("$home/parts/IL2DCE/Campaigns/Prototype/Template.mis");

                FrameworkElement.listBoxAirGroups.ItemsSource = Game.Core.AirGroups;
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

            private void listBoxAirGroup_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    IAirGroup airGroup = e.AddedItems[0] as IAirGroup;

                    FrameworkElement.listBoxAircraft.Items.Clear();
                    foreach (int flightIndex in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[flightIndex].Count > 0)
                        {
                            foreach (string acNumber in airGroup.Flights[flightIndex])
                            {
                                FrameworkElement.listBoxAircraft.Items.Add(acNumber);
                            }
                        }
                    }
                }
            }

            private void listBoxAircraft_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {

            }
        }
    }
}
