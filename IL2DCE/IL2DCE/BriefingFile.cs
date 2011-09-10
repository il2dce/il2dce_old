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

namespace IL2DCE
{
    public class BriefingFile : IBriefingFile
    {
        public BriefingFile()
        {

        }

        public string MissionName
        {
            get
            {
                return _missionName;
            }
            set
            {
                _missionName = value;
            }
        }
        private string _missionName = "";

        public string MissionDescription
        {
            get
            {
                return _missionDescription;
            }
            set
            {
                _missionDescription = value;
            }
        }
        private string _missionDescription = "";

        public IDictionary<string, string> Name
        {
            get
            {
                return _name;
            }
        }
        private Dictionary<string, string> _name = new Dictionary<string, string>();

        public IDictionary<string, string> Description
        {
            get
            {
                return _description;
            }
        }
        private Dictionary<string, string> _description = new Dictionary<string, string>();

        public void save(string systemFileName)
        {
            System.IO.TextWriter briefingFileWriter = new System.IO.StreamWriter(systemFileName, false);

            briefingFileWriter.WriteLine("[Info]");
            briefingFileWriter.WriteLine("<Name>");
            briefingFileWriter.WriteLine("Info");
            briefingFileWriter.WriteLine("<Caption>");
            briefingFileWriter.WriteLine(MissionName); 
            briefingFileWriter.WriteLine("<Caption>");
            briefingFileWriter.WriteLine(MissionDescription);

            foreach (string key in Name.Keys)
            {
                briefingFileWriter.WriteLine("[" + key + "]");
                briefingFileWriter.WriteLine("<Name>");
                briefingFileWriter.WriteLine(Name[key]);

                briefingFileWriter.WriteLine("<Description>");
                if(Description.ContainsKey(key))
                {
                    briefingFileWriter.WriteLine(Description[key]);
                }
                else
                {
                    briefingFileWriter.WriteLine("");
                }
            }

            briefingFileWriter.Close();
        }
    }
}