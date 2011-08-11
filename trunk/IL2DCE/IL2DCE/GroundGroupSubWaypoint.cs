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
    public class GroundGroupSubWaypoint
    {
        #region Public constructors

        public GroundGroupSubWaypoint(ISectionFile sectionFile, string groundGroupId, int line)
        {
            string key;
            string value;
            sectionFile.get(groundGroupId + "_Road", line, out key, out value);

            System.Text.RegularExpressions.Regex subWaypoint = new System.Text.RegularExpressions.Regex(@"^([0-9]+) ([0-9]+) ([0-9]+[.0-9]*) ([0-9]+[.0-9]*) P ([0-9]+[.0-9]*) ([0-9]+[.0-9]*)$");

            if (subWaypoint.IsMatch(value))
            {
                System.Text.RegularExpressions.Match match = subWaypoint.Match(value);

                if (match.Groups.Count == 7)
                {
                    _s = match.Groups[1].Value + " " + match.Groups[2].Value + " " + match.Groups[3].Value + " " + match.Groups[4].Value;

                    double.TryParse(match.Groups[5].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out X);
                    double.TryParse(match.Groups[6].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Y);
                }
            }            
        }

        #endregion

        #region Public properties

        public string S
        {
            get
            {
                return _s;
            }
        }
        private string _s;

        public Point2d P
        {
            get
            {
                return new Point2d(X, Y);
            }
        }

        public double X;

        public double Y;

        #endregion
    }
}