// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2011 Stefan Rothdach
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU AFFERO GENERAL PUBLIC LICENSE cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.Collections;
using System.Collections.Generic;

using maddox.game;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE
{
    namespace Engine
    {
        public class AirGroupWaypoint
        {
            # region Public enums

            public enum AirGroupWaypointTypes
            {
                NORMFLY,
                TAKEOFF,
                LANDING,
                COVER,
                HUNTING,
                RECON,
                GATTACK_POINT,
                GATTACK_TARG,
                ESCORT,
                AATTACK_FIGHTERS,
                AATTACK_BOMBERS,
            };

            #endregion

            #region Public constructors

            public AirGroupWaypoint(AirGroupWaypointTypes type, Point3d position, double v, string target = null)
            {
                Type = type;
                X = position.x;
                Y = position.y;
                Z = position.z;
                V = v;
                Target = target;
            }

            public AirGroupWaypoint(AirGroupWaypointTypes type, double x, double y, double z, double v, string target = null)
            {
                Type = type;
                X = x;
                Y = y;
                Z = z;
                V = v;
                Target = target;
            }

            public AirGroupWaypoint(ISectionFile sectionFile, string airGroupName, int line)
            {
                string key;
                string value;
                sectionFile.get(airGroupName + "_Way", line, out key, out value);

                string[] valueList = value.Split(new char[] { ' ' });
                if (valueList != null && valueList.Length == 4)
                {
                    Type = (AirGroupWaypointTypes)Enum.Parse(typeof(AirGroupWaypointTypes), key);
                    double.TryParse(valueList[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out X);
                    double.TryParse(valueList[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Y);
                    double.TryParse(valueList[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Z);
                    double.TryParse(valueList[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out V);
                }
            }

            #endregion

            #region Public properties

            public AirGroupWaypointTypes Type
            {
                get;
                set;
            }

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

            public double V;
            
            public string Target
            {
                get;
                set;
            }

            #endregion
        }
    }
}