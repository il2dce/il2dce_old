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
    namespace MissionOld
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

            public override void OnBattleStarted()
            {
                base.OnBattleStarted();

                ISectionFile confFile = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/conf.ini");
                string campaignsFolderPath = confFile.get("Main", "campaignsFolder");
                string campaignsFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\missions\IL2DCE\Campaigns";
                string careersFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\mission\IL2DCE";
                string debugFolderSystemPath = @"C:\Users\stefan.rothdach\Documents\1C SoftClub\il-2 sturmovik cliffs of dover\missions\IL2DCE\Debug";

                this.core = new Core(GamePlay, confFile, campaignsFolderSystemPath, careersFolderSystemPath, debugFolderSystemPath);

                core.CurrentCareer = core.Careers[0];
                core.InitCampaign();
                
                ISectionFile missionFile = null;
                IBriefingFile briefingFile = null;
                core.Generate(core.CurrentCareer.CampaignInfo.TemplateFilePath, "test", out missionFile, out briefingFile);

                GamePlay.gpBattleStop();

                GamePlay.gpPostMissionLoad(missionFile);
            }
        }
    }
}