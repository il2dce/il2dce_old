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
    public class Campaign : ICampaign
    {
        public Campaign(string id, string campaignFolderPath, ISectionFile campaignFile)
        {
            _id = id;

            if (campaignFile.exist("Campaign", "name"))
            {
                name = campaignFile.get("Campaign", "name");
            }

            if (campaignFile.exist("Campaign", "templateFile"))
            {
                templateFilePath = campaignFolderPath + campaignFile.get("Campaign", "templateFile");
            }

            if (campaignFile.exist("Campaign", "scriptFile"))
            {
                _scriptFilePath = campaignFolderPath + campaignFile.get("Campaign", "scriptFile");
            }

            if (campaignFile.exist("Campaign", "startDate"))
            {
                string startDateString = campaignFile.get("Campaign", "startDate");
                _startDate = DateTime.Parse(startDateString);
            }

            if (campaignFile.exist("Campaign", "endDate"))
            {
                string endDateString = campaignFile.get("Campaign", "endDate");
                _endDate = DateTime.Parse(endDateString);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }
        string _id;

        public string Name
        {
            get
            {
                return name;
            }
        }
        string name;

        public string TemplateFilePath
        {
            get
            {
                return templateFilePath;
            }
        }
        private string templateFilePath;

        public string ScriptFilePath
        {
            get
            {
                return _scriptFilePath;
            }
        }
        private string _scriptFilePath;

        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
        }
        private DateTime _startDate;

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
        }
        private DateTime _endDate;

        public string CurrentMissionFileName
        {
            get
            {
                return _currentMissionFileName;
            }
            set
            {
                _currentMissionFileName = value;
            }
        }
        private string _currentMissionFileName;

        public void Save()
        {

        }
    }
}