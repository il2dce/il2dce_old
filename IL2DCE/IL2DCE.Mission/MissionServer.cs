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
using maddox.game.world;
using maddox.GP;

namespace IL2DCE
{
    namespace Mission
    {
        public class MissionServer : Mission
        {
            private ICore core;

            protected override ICore Core
            {
                get
                {
                    return this.core;
                }
            }


            private void setMainMenu(Player player)
            {
                GamePlay.gpSetOrderMissionMenu(player, false, 0, new string[] { "Select career", "", "3" }, new bool[] { true, false, false });
            }
            private void setSubMenu(Player player)
            {
                GamePlay.gpSetOrderMissionMenu(player, true, 1, new string[] { "1-1", "1-2", "1-3" }, new bool[] { false, false, false });
            }

            public override void OnOrderMissionMenuSelected(Player player, int ID, int menuItemIndex)
            {
                if (ID == 0)
                { // main menu
                    if (menuItemIndex == 1)
                    {
                        GamePlay.gpHUDLogCenter("Menu selected Loading mission aaa2.mis");
                        //GamePlay.gpPostMissionLoad("missions\\aaa2.mis");
                        setSubMenu(player);
                    }
                }
                else if (ID == 1)
                { // sub menu
                    setMainMenu(player);
                }
            }

            public override void OnPlayerConnected(Player player)
            {
                /*if (MissionNumber == 0)
                {
                    setMainMenu(player);
                }*/
            }

            public override void Inited()
            {
                if (MissionNumber == 0)
                {
                    setMainMenu(GamePlay.gpPlayer());
                }
            }

            public override void OnBattleStarted()
            {
                base.OnBattleStarted();

                ISectionFile confFile = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/conf.ini");
                string campaignsFolderPath = confFile.get("Main", "campaignsFolder");
                string campaignsFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\missions\IL2DCE\Campaigns";
                string careersFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\mission\IL2DCE";

                this.core = new Core(GamePlay, confFile, campaignsFolderSystemPath, careersFolderSystemPath);

                core.Career = core.Careers[0];
                core.InitCampaign();
                
                ISectionFile missionFile = null;
                IBriefingFile briefingFile = null;
                core.Generate(core.Career.CampaignInfo.TemplateFilePath, "test", out missionFile, out briefingFile);

                GamePlay.gpBattleStop();

                GamePlay.gpPostMissionLoad(missionFile);
            }
        }
    }
}