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
    namespace Game
    {
        public class GameServer : maddox.game.GameServerDef, IGame
        {
            public GameServer(GameServerIterface game, bool dedicated)
                : base(game, dedicated)
            {
                ISectionFile confFile = game.SectionFileLoad("$home/parts/IL2DCE/conf.ini");

                core = new Engine.Core(this, confFile);

                if (confFile.exist("MAIN", "campaignsFolder"))
                {
                    string campaignsFolderName = confFile.get("MAIN", "campaignsFolder");
                    ISectionFile campaignsFile = game.SectionFileLoad(campaignsFolderName + "/campaigns.ini");

                    if (campaignsFile.exist("Campaigns"))
                    {
                        for (int i = 0; i < campaignsFile.lines("Campaigns"); i++)
                        {
                            string key;
                            string value;
                            campaignsFile.get("Campaigns", i, out key, out value);

                            Campaign campaign = new Campaign(this, campaignsFile, key);
                            Campaigns.Add(campaign);
                        }
                    }
                }
            }

            public override maddox.game.play.PageInterface getStartPage()
            {
                return new Pages.ServerOptionsPage();
            }

            public ICore Core
            {
                get
                {
                    return core;
                }
            }
            private ICore core;

            public GameIterface GameInterface
            {
                get
                {
                    return gameInterface;
                }
            }

            public List<ICampaign> Campaigns
            {
                get
                {
                    return campaigns;
                }
            }
            private List<ICampaign> campaigns = new List<ICampaign>();

            public ICampaign CurrentCampaign
            {
                get
                {
                    return currentCampaign;
                }
                set
                {
                    currentCampaign = value as Campaign;
                }

            }
            private Campaign currentCampaign;
        }
    }
}