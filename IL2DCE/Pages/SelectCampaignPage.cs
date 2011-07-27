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
        public class SelectCampaignPage : PageDefImpl
        {
            public SelectCampaignPage()
                : base("Select campaign", new SelectCampaign())
            {
                FrameworkElement.bBack.Click += new System.Windows.RoutedEventHandler(bBack_Click);
                FrameworkElement.bNew.Click += new System.Windows.RoutedEventHandler(bNew_Click);
                FrameworkElement.bContinue.Click += new System.Windows.RoutedEventHandler(bContinue_Click);

                // TODO: Make button visible when it is possible to continue a campaign.
                FrameworkElement.bContinue.Visibility = System.Windows.Visibility.Hidden;
            }

            private SelectCampaign FrameworkElement
            {
                get
                {
                    return FE as SelectCampaign;
                }
            }

            private IGame Engine;

            public override void _enter(IGame play, object arg)
            {
                Engine = play;
            }
            
            private void bBack_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Engine.gameInterface.PagePop(null);
            }

            private void bNew_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Engine.gameInterface.MissionLoad("$home/parts/IL2DCE/Campaigns/Prototype/Main.mis");
                Engine.gameInterface.UIMainHide();
                Engine.gameInterface.BattleStart();
            }

            private void bContinue_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Engine.gameInterface.PagePop(null);
            }
        }
    }
}
