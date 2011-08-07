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
    namespace Core
    {
        public class GroundGroupWaypoint
        {
            #region Public constructors

            public GroundGroupWaypoint(Point3d position, double v)
            {
                X = position.x;
                Y = position.y;
                Z = position.z;
                V = v;                
            }

            public GroundGroupWaypoint(double x, double y, double z, double v)
            {
                X = x;
                Y = y;
                Z = z;
                V = v;
            }

            public GroundGroupWaypoint(ISectionFile sectionFile, string groundGroupId, int line)
            {
                string key;
                string value;
                sectionFile.get(groundGroupId + "_Road", line, out key, out value);

                string[] valueList = value.Split(new char[] { ' ' });
                if (valueList != null && valueList.Length >= 5)
                {
                    double.TryParse(key, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out X);
                    double.TryParse(valueList[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Y);
                    double.TryParse(valueList[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Z);
                    int type;
                    if (int.TryParse(new string(value[3], 1), out type))
                    {
                        Type = type;
                    }
                    double v;
                    if (double.TryParse(valueList[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out v))
                    {
                        V = v;
                    }
                }
                else if (valueList != null && valueList.Length == 2)
                {
                    double.TryParse(key, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out X);
                    double.TryParse(valueList[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Y);
                    double.TryParse(valueList[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Z);
                }
            }

            #endregion

            #region Public properties

            public Point3d Position
            {
                get
                {
                    return new Point3d(X, Y, Z);
                }
            }

            public double X;

            public double Y;

            public double Z;

            public int? Type
            {
                get
                {
                    return _type;
                }
                set
                {
                    _type = value;
                }
            }
            public int? _type;

            public double? V
            {
                get
                {
                    return _v;
                }
                set
                {
                    _v = value;
                }
            }
            public double? _v;

            #endregion
        }
    }
}