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
    public class Road
    {
        public Road(ISectionFile sectionFile, string groundGroupId)
        {
            if (sectionFile.lines(groundGroupId + "_Road") > 2)
            {
                _start = new GroundGroupWaypoint(sectionFile, groundGroupId, 0);

                for (int i = 1; i < sectionFile.lines(groundGroupId + "_Road") - 1; i++)
                {
                    string key;
                    string value;
                    sectionFile.get(groundGroupId + "_Road", i, out key, out value);
                    _roadPoints.Add(new Tuple<string, string>(key, value));
                }

                _end = new GroundGroupWaypoint(sectionFile, groundGroupId, sectionFile.lines(groundGroupId + "_Road") - 1);
            }
        }

        public GroundGroupWaypoint Start
        {
            get
            {
                return _start;
            }
        }
        private GroundGroupWaypoint _start;

        public GroundGroupWaypoint End
        {
            get
            {
                return _end;
            }
        }
        private GroundGroupWaypoint _end;

        public List<Tuple<string, string>> RoadPoints
        {
            get
            {
                return _roadPoints;
            }
        }
        private List<Tuple<string, string>> _roadPoints = new List<Tuple<string, string>>();
    }
}