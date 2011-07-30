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
        public class Campaign : ICampaign
        {
            public Campaign(IGame game, ISectionFile campaignsFile, string campaignId)
            {
                if (campaignsFile.exist(campaignId) && campaignsFile.exist(campaignId, "campaignName") && campaignsFile.exist(campaignId, "campaignFolder"))
                {
                    name = campaignsFile.get(campaignId, "campaignName");
                    string campaignFolderName = campaignsFile.get(campaignId, "campaignFolder");

                    ISectionFile campaignFile = game.GameInterface.SectionFileLoad(campaignFolderName + "/campaign.ini");
                    if (campaignFile.exist("Campaign", "templateFile"))
                    {
                        templateFileName = campaignFile.get("Campaign", "templateFile");
                        templateFile = game.GameInterface.SectionFileLoad(templateFileName);
                    }
                }
            }

            public override string ToString()
            {
                return Name;
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }
            string name;

            public ISectionFile TemplateFile
            {
                get
                {
                    return templateFile;
                }
            }
            private ISectionFile templateFile;

            public string TemplateFileName
            {
                get
                {
                    return templateFileName;
                }
            }
            private string templateFileName;
        }
    }
}