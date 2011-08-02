// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2011 Stefan Rothdach
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU AFFERO GENERAL PUBLIC LICENSE cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

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
        public class SelectCampaignPage : PageDefImpl
        {
            public SelectCampaignPage(IGame game)
                : base("Select campaign", new SelectCampaign())
            {
                _game = game;

                FrameworkElement.bBack.Click += new System.Windows.RoutedEventHandler(bBack_Click);
                FrameworkElement.bNew.Click += new System.Windows.RoutedEventHandler(bNew_Click);
                FrameworkElement.bContinue.Click += new System.Windows.RoutedEventHandler(bContinue_Click);

                FrameworkElement.ListCampaign.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(listCampaign_SelectionChanged);

                // TODO: Make button visible when it is possible to continue a campaign.
                FrameworkElement.bNew.IsEnabled = false;
                FrameworkElement.bContinue.Visibility = System.Windows.Visibility.Hidden;

                FrameworkElement.ListCampaign.ItemsSource = Game.Campaigns;

                if (FrameworkElement.ListCampaign.Items.Count > 0)
                {
                    FrameworkElement.ListCampaign.SelectedIndex = 0;
                }
                else
                {
                    FrameworkElement.ListCampaign.SelectedIndex = -1;
                }
            }

            private SelectCampaign FrameworkElement
            {
                get
                {
                    return FE as SelectCampaign;
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

            private void bBack_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            private void bNew_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PageChange(new CampaignIntoPage(Game), null);
            }

            private void bContinue_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            private void listCampaign_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    ICampaign campaignSelected = e.AddedItems[0] as ICampaign;
                    Game.CurrentCampaign = campaignSelected;
                }

                if (Game.CurrentCampaign != null)
                {
                    FrameworkElement.bNew.IsEnabled = true;
                }
                else
                {
                    FrameworkElement.bNew.IsEnabled = false;
                }
            }
        }
    }
}
