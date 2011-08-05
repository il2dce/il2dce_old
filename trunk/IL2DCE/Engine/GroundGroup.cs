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
    namespace Engine
    {
        public class GroundGroup : IGroundGroup
        {
            public GroundGroup(ISectionFile sectionFile, string groundGroupId)
            {
                _id = groundGroupId;

                string value = sectionFile.get("Chiefs", groundGroupId);

                // Class
                Class = value.Substring(0, value.IndexOf(" "));
                value = value.Remove(0, Class.Length + 1);

                // Army
                Country = (EGroundGroupCountry)Enum.Parse(typeof(EGroundGroupCountry), value.Substring(0, 2));
                value = value.Remove(0, 2);

                // Options
                Options = value.Trim();
                                                
                // Waypoints
                Waypoints = new List<GroundGroupWaypoint>();
                for (int i = 0; i < sectionFile.lines(groundGroupId + "_Road"); i++)
                {
                    GroundGroupWaypoint waypoint = new GroundGroupWaypoint(sectionFile, groundGroupId, i);
                    Waypoints.Add(waypoint);
                }

                if (Waypoints.Count > 0)
                {
                    Position = new Point3d(Waypoints[0].X, Waypoints[0].Y, Waypoints[0].Z);
                }
            }

            public EGroundGroupType Type
            {
                get
                {
                    // Type
                    if (Class.StartsWith("Vehicle"))
                    {
                        return EGroundGroupType.Vehicle;
                    }
                    else if (Class.StartsWith("Armor"))
                    {
                        return EGroundGroupType.Armor;
                    }
                    else if (Class.StartsWith("Ship"))
                    {
                        return EGroundGroupType.Ship;
                    }
                    else
                    {
                        throw new System.FormatException("Unknown EType of GroundGroup");
                    }
                }
            }

            public Point3d Position
            {
                get
                {
                    return _position;
                }
                set
                {
                    _position = value;
                }
            }
            private Point3d _position;

            public string Id
            {
                get
                {
                    return _id;
                }
            }
            public string _id;

            public string Class
            {
                get;
                set;
            }

            public EGroundGroupCountry Country
            {
                get;
                set;
            }

            public int Army
            {
                get
                {
                    if (Country == EGroundGroupCountry.gb)
                    {
                        return 1;
                    }
                    else if (Country == EGroundGroupCountry.de)
                    {
                        return 2;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public string Options
            {
                get;
                set;
            }

            public List<GroundGroupWaypoint> Waypoints;


            public void writeTo(ISectionFile sectionFile)
            {
                sectionFile.add("Chiefs", Id,  Class + " " + Country.ToString() + " " + Options);

                foreach (GroundGroupWaypoint waypoint in Waypoints)
                {
                    if(Waypoints.IndexOf(waypoint) != Waypoints.Count - 1)
                    {
                        if (waypoint.V.HasValue)
                        {
                            sectionFile.add(Id + "_Road", waypoint.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), waypoint.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " 0 2 " + waypoint.V.Value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }
                        // TODO: Use the default V.
                    }
                    else
                    {
                        sectionFile.add(Id + "_Road", waypoint.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), waypoint.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                    }
                }
            }
        }
    }
}