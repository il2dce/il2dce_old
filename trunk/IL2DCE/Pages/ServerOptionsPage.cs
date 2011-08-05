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
        public class ServerOptionsPage : PageDefImpl
        {
            public ServerOptionsPage()
                : base("Select campaign", new ServerOptions())
            {
                FrameworkElement.Apply.Click += new System.Windows.RoutedEventHandler(Apply_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGameServer;
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
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
                //Game.init("");
                //Game.Server.init(Game.dedicated, Game.Server.getPublicIP(), 27018, 27018, true, "0.1.0.129");
                //Game.Server.sendBasicServerData(false, "Hello World", 12);
                
                //if (Game.Server.bConnected)
                {
                    Game.gameInterface.PageChange(new SelectCampaignPage(), null);
                }
            }
        }
    }
}
