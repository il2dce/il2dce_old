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

        public void writeTo(ISectionFile sectionFile, IList<Road> roads)
        {
            sectionFile.add("Chiefs", Id, Class + " " + Country.ToString() + " " + Options);
            for (int i = 0; i < Waypoints.Count - 1; i++)
            {
                //if (Waypoints[i].V.HasValue && Waypoints[i].Type.HasValue)
                //{
                //    sectionFile.add(Id + "_Road", Waypoints[i].X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), Waypoints[i].Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Waypoints[i].Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "  0 " + Waypoints[i].Type.Value.ToString() + " " + Waypoints[i].V.Value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                //}
                // TODO: Use the default V.

                if (roads != null && roads.Count > 0)
                {
                    Road closestRoad = null;
                    double closestRoadDistance = 0.0;
                    foreach (Road road in roads)
                    {
                        if (road.Start != null && road.End != null)
                        {
                            Point3d pStart = new Point3d(road.Start.Position.x, road.Start.Position.y, road.Start.Position.z);
                            double distanceStart = Waypoints[i].Position.distance(ref pStart);
                            Point3d pEnd = new Point3d(road.End.Position.x, road.End.Position.y, road.End.Position.z);
                            double distanceEnd = Waypoints[i + 1].Position.distance(ref pEnd);
                            if (closestRoad == null)
                            {
                                closestRoad = road;
                                closestRoadDistance = distanceStart + distanceEnd;
                            }
                            else
                            {
                                if (distanceStart + distanceEnd < closestRoadDistance)
                                {
                                    closestRoad = road;
                                    closestRoadDistance = distanceStart + distanceEnd;
                                }
                            }
                        }
                    }

                    if (closestRoad != null)
                    {
                        if (closestRoad.Start.V.HasValue && closestRoad.Start.Type.HasValue)
                        {
                            sectionFile.add(Id + "_Road", closestRoad.Start.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), closestRoad.Start.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + closestRoad.Start.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "  0 " + closestRoad.Start.Type.Value.ToString() + " " + closestRoad.Start.V.Value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

                            if (closestRoad.RoadPoints != null && closestRoad.RoadPoints.Count > 0)
                            {
                                foreach (Tuple<string, string> tuple in closestRoad.RoadPoints)
                                {
                                    sectionFile.add(Id + "_Road", tuple.Item1, tuple.Item2);
                                }
                            }
                            sectionFile.add(Id + "_Road", closestRoad.End.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), closestRoad.End.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + closestRoad.End.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }                        
                    }
                }
            }

            //sectionFile.add(Id + "_Road", Waypoints[Waypoints.Count - 1].X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat), Waypoints[Waypoints.Count - 1].Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Waypoints[Waypoints.Count - 1].Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
        }
    }
}