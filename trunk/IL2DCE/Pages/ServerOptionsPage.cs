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
        public class ServerOptionsPage : PageDefImpl
        {
            public ServerOptionsPage(IGameServer game)
                : base("Select campaign", new ServerOptions())
            {
                _game = game;

                FrameworkElement.Apply.Click += new System.Windows.RoutedEventHandler(Apply_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
            }

            private ServerOptions FrameworkElement
            {
                get
                {
                    return FE as ServerOptions;
                }
            }

            private IGameServer Game
            {
                get
                {
                    return _game;
                }
            }
            private IGameServer _game;

            private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            private void Apply_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                if (Game.init("") == true)
                {
                    Game.gameInterface.PageChange(new SelectCampaignPage(Game as IGame), null);
                }
            }
        }
    }
}
