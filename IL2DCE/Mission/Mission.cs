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
    namespace Mission
    {
        public class Mission : AMission
        {
            #region AirGroup

            private class AirGroup
            {
                private Point3d position;

                #region Public constructors

                public AirGroup(ISectionFile sectionFile, string airGroupId)
                {
                    // airGroupId = <airGroupKey>.<squadronIndex><flightMask>

                    // AirGroupKey
                    AirGroupKey = airGroupId.Substring(0, airGroupId.IndexOf("."));

                    // SquadronIndex
                    int.TryParse(airGroupId.Substring(airGroupId.LastIndexOf(".") + 1, 1), out SquadronIndex);

                    // Flight
                    Flight = new int[4][];
                    for (int i = 0; i < 4; i++)
                    {
                        if (sectionFile.exist(airGroupId, "Flight" + i.ToString()))
                        {
                            string acNumberLine = sectionFile.get(airGroupId, "Flight" + i.ToString());
                            string[] acNumberList = acNumberLine.Split(new char[] { ' ' });
                            if (acNumberList != null && acNumberList.Length > 0)
                            {
                                Flight[i] = new int[acNumberList.Length];
                                for (int j = 0; j < acNumberList.Length; j++)
                                {
                                    int.TryParse(acNumberList[j], out Flight[i][j]);
                                }
                            }
                            else
                            {
                                Flight[i] = null;
                            }
                        }
                        else
                        {
                            Flight[i] = null;
                        }
                    }

                    // Class
                    Class = sectionFile.get(airGroupId, "Class");

                    // Formation
                    Formation = sectionFile.get(airGroupId, "Formation");

                    // CallSign
                    int.TryParse(sectionFile.get(airGroupId, "CallSign"), out CallSign);

                    // Fuel
                    int.TryParse(sectionFile.get(airGroupId, "Fuel"), out Fuel);

                    // Weapons
                    string weaponsLine = sectionFile.get(airGroupId, "Weapons");
                    string[] weaponsList = weaponsLine.Split(new char[] { ' ' });
                    if (weaponsList != null && weaponsList.Length > 0)
                    {
                        Weapons = new int[weaponsList.Length];
                        for (int i = 0; i < weaponsList.Length; i++)
                        {
                            int.TryParse(weaponsList[i], out Weapons[i]);
                        }
                    }

                    // Belt
                    // TODO: Parse belt

                    // Skill
                    // TODO: Parse skill

                    // Waypoints
                    Waypoints = new List<AirGroupWaypoint>();
                    for (int i = 0; i < sectionFile.lines(Name + "_Way"); i++)
                    {
                        AirGroupWaypoint waypoint = new AirGroupWaypoint(sectionFile, Name, i);
                        Waypoints.Add(waypoint);
                    }

                    if (Waypoints.Count > 0)
                    {
                        Position = new Point3d(Waypoints[0].X, Waypoints[0].Y, Waypoints[0].Z);
                    }
                }

                #endregion

                #region Public properties

                public string AirGroupKey;

                public int SquadronIndex;

                public int[][] Flight;

                public string Class;

                public string Formation;

                public int CallSign;

                public int Fuel;

                public int[] Weapons;

                public AiAirport Airport;

                public Point3d Position
                {
                    get
                    {
                        if (Airport != null)
                        {
                            return Airport.Pos();
                        }
                        else
                        {
                            return position;
                        }
                    }
                    set
                    {
                        position = value;
                    }
                }

                public double Speed
                {
                    get
                    {
                        // TODO: Use aicraft to determine speed
                        return 300.0;
                    }
                }

                public string Name
                {
                    get
                    {
                        int flightMask = 0x0;

                        if (Flight != null && Flight.Length > 0)
                        {
                            for (int i = 0; i < Flight.Length; i++)
                            {
                                if (Flight[i] != null && Flight[i].Length > 0)
                                {
                                    int bit = (0x1 << i);
                                    flightMask = (flightMask | bit);
                                }
                            }
                        }

                        return AirGroupKey + "." + SquadronIndex.ToString() + flightMask.ToString("X");
                    }
                }

                public int ArmyIndex
                {
                    get
                    {
                        if (GetAirGroupInfo(1, AirGroupKey) != null)
                        {
                            return 1;
                        }
                        else if (GetAirGroupInfo(2, AirGroupKey) != null)
                        {
                            return 2;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }

                public AircraftInfo AircraftInfo
                {
                    get
                    {
                        return new AircraftInfo(Class);
                    }
                }

                public List<AirGroupWaypoint> Waypoints;

                #endregion

                #region Private methods

                private void writeTo(ISectionFile sectionFile)
                {
                    sectionFile.add("AirGroups", Name, "");

                    for (int i = 0; i < 4; i++)
                    {
                        if (Flight[i] != null && Flight[i].Length > 0)
                        {
                            string acNumberLine = "";
                            foreach (int acNumber in Flight[i])
                            {
                                acNumberLine += acNumber.ToString() + " ";
                            }
                            sectionFile.add(Name, "Flight" + i, acNumberLine.TrimEnd());
                        }
                    }

                    sectionFile.add(Name, "Class", Class);
                    sectionFile.add(Name, "Formation", Formation);
                    sectionFile.add(Name, "CallSign", CallSign.ToString());
                    sectionFile.add(Name, "Fuel", Fuel.ToString());

                    if (Weapons != null && Weapons.Length > 0)
                    {
                        string weaponsLine = "";
                        foreach (int weapon in Weapons)
                        {
                            weaponsLine += weapon.ToString() + " ";
                        }
                        sectionFile.add(Name, "Weapons", weaponsLine.TrimEnd());
                    }

                    if (spawnParked == true)
                    {
                        sectionFile.add(Name, "SetOnPark", "1");
                    }
                    sectionFile.add(Name, "Skill", "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3");

                    foreach (AirGroupWaypoint waypoint in Waypoints)
                    {
                        if (waypoint.Target == null)
                        {
                            sectionFile.add(Name + "_Way", waypoint.Type.ToString(), waypoint.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        }
                        else
                        {
                            sectionFile.add(Name + "_Way", waypoint.Type.ToString(), waypoint.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Target);
                        }
                    }
                }

                private double distanceTo(AirGroupWaypoint waypoint)
                {
                    double distance = 0.0;
                    AirGroupWaypoint previousWaypoint = null;
                    if (Waypoints.Contains(waypoint))
                    {
                        foreach (AirGroupWaypoint wp in Waypoints)
                        {
                            if (previousWaypoint == null)
                            {
                                distance = 0.0;
                            }
                            else
                            {
                                Point3d p = wp.Position;
                                distance += previousWaypoint.Position.distance(ref p);
                            }
                            previousWaypoint = wp;

                            if (wp == waypoint)
                            {
                                break;
                            }
                        }
                    }

                    return distance;
                }

                private void createStartWaypoints(ISectionFile sectionFile)
                {
                    if (Airport != null)
                    {
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, Airport.Pos(), 0.0));
                    }
                    else
                    {
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
                    }
                }

                private void createEndWaypoints(ISectionFile sectionFile, AiAirport landingAirport = null)
                {
                    if (landingAirport == null)
                    {
                        landingAirport = Airport;
                    }

                    if (landingAirport != null)
                    {
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, landingAirport.Pos(), 0.0));
                    }
                    else
                    {
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
                    }
                }

                private void createInbetweenWaypoints(ISectionFile sectionFile, Point3d a, Point3d b)
                {
                    Point3d p1 = new Point3d(a.x + 0.25 * (b.x - a.x), a.y + 0.25 * (b.y - a.y), a.z + 0.25 * (b.z - a.z));
                    Point3d p2 = new Point3d(a.x + 0.50 * (b.x - a.x), a.y + 0.50 * (b.y - a.y), a.z + 0.50 * (b.z - a.z));
                    Point3d p3 = new Point3d(a.x + 0.75 * (b.x - a.x), a.y + 0.75 * (b.y - a.y), a.z + 0.75 * (b.z - a.z));

                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p1, 300.0));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p2, 300.0));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p3, 300.0));
                }

                private void createStartInbetweenPoints(ISectionFile sectionFile, Point3d target)
                {
                    if (Airport != null)
                    {
                        createInbetweenWaypoints(sectionFile, Airport.Pos(), target);
                    }
                    else
                    {
                        createInbetweenWaypoints(sectionFile, Position, target);
                    }
                }

                private void createEndInbetweenPoints(ISectionFile sectionFile, Point3d target, AiAirport landingAirport = null)
                {
                    if (landingAirport == null)
                    {
                        landingAirport = Airport;
                    }

                    if (landingAirport != null)
                    {
                        createInbetweenWaypoints(sectionFile, target, landingAirport.Pos());
                    }
                    else
                    {
                        createInbetweenWaypoints(sectionFile, target, Position);
                    }
                }

                #endregion

                #region Public methods

                public void CreateTransferFlight(ISectionFile sectionFile, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);

                    createEndInbetweenPoints(sectionFile, Position, landingAirport);

                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                public void CreateReconFlight(ISectionFile sectionFile, Point3d targetArea, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);
                    createStartInbetweenPoints(sectionFile, targetArea);

                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, targetArea.x, targetArea.y, targetArea.z, 300.0));

                    createEndInbetweenPoints(sectionFile, targetArea, landingAirport);
                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                public void CreateCoverFlight(ISectionFile sectionFile, Point3d targetArea, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);
                    createStartInbetweenPoints(sectionFile, targetArea);

                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetArea.x, targetArea.y, targetArea.z, 300.0));

                    createEndInbetweenPoints(sectionFile, targetArea, landingAirport);
                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                public void CreateHuntingFlight(ISectionFile sectionFile, Point3d targetArea, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);
                    createStartInbetweenPoints(sectionFile, targetArea);

                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.HUNTING, targetArea.x, targetArea.y, targetArea.z, 300.0));

                    createEndInbetweenPoints(sectionFile, targetArea, landingAirport);
                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                public void CreateGroundAttackFlight(ISectionFile sectionFile, Point3d targetArea, Point3d? rendevouzPosition = null, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);

                    if (rendevouzPosition == null)
                    {
                        createStartInbetweenPoints(sectionFile, targetArea);
                    }
                    else
                    {
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                        createInbetweenWaypoints(sectionFile, rendevouzPosition.Value, targetArea);
                    }


                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_POINT, targetArea.x, targetArea.y, targetArea.z, 300.0));

                    createEndInbetweenPoints(sectionFile, targetArea, landingAirport);
                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                public void CreateEscortFlight(ISectionFile sectionFile, AirGroup targetAirUnit, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);

                    foreach (AirGroupWaypoint waypoint in targetAirUnit.Waypoints)
                    {
                        if (waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                        {
                            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.ESCORT, waypoint.X, waypoint.Y, waypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(waypoint)));
                        }
                    }

                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                public void CreateInterceptFlight(ISectionFile sectionFile, AirGroup targetAirUnit, Point3d targetArea, AiAirport landingAirport = null)
                {
                    Waypoints.Clear();

                    createStartWaypoints(sectionFile);

                    AirGroupWaypoint interceptWaypoint = null;
                    AirGroupWaypoint closestInterceptWaypoint = null;
                    foreach (AirGroupWaypoint waypoint in targetAirUnit.Waypoints)
                    {
                        Point3d p = Position;
                        if (targetAirUnit.distanceTo(waypoint) > waypoint.Position.distance(ref p))
                        {
                            interceptWaypoint = waypoint;
                            break;
                        }
                        else
                        {
                            if (closestInterceptWaypoint == null)
                            {
                                closestInterceptWaypoint = waypoint;
                            }
                            else
                            {
                                if (targetAirUnit.distanceTo(waypoint) < closestInterceptWaypoint.Position.distance(ref p))
                                {
                                    closestInterceptWaypoint = waypoint;
                                }
                            }
                        }
                    }

                    if (interceptWaypoint != null)
                    {
                        createStartInbetweenPoints(sectionFile, interceptWaypoint.Position);
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, interceptWaypoint.X, interceptWaypoint.Y, interceptWaypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(interceptWaypoint)));


                        if (targetAirUnit.Waypoints.IndexOf(interceptWaypoint) - 1 >= 0)
                        {
                            AirGroupWaypoint nextInterceptWaypoint = targetAirUnit.Waypoints[targetAirUnit.Waypoints.IndexOf(interceptWaypoint) - 1];
                            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(nextInterceptWaypoint)));

                            createEndInbetweenPoints(sectionFile, nextInterceptWaypoint.Position, landingAirport);
                        }
                        else
                        {
                            createEndInbetweenPoints(sectionFile, interceptWaypoint.Position, landingAirport);
                        }
                    }
                    else if (closestInterceptWaypoint != null)
                    {
                        createStartInbetweenPoints(sectionFile, closestInterceptWaypoint.Position);
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, closestInterceptWaypoint.X, closestInterceptWaypoint.Y, closestInterceptWaypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(closestInterceptWaypoint)));


                        if (targetAirUnit.Waypoints.IndexOf(closestInterceptWaypoint) + 1 < targetAirUnit.Waypoints.Count)
                        {
                            AirGroupWaypoint nextInterceptWaypoint = targetAirUnit.Waypoints[targetAirUnit.Waypoints.IndexOf(interceptWaypoint) + 1];
                            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(nextInterceptWaypoint)));

                            createEndInbetweenPoints(sectionFile, nextInterceptWaypoint.Position, landingAirport);
                        }
                        else
                        {
                            createEndInbetweenPoints(sectionFile, closestInterceptWaypoint.Position, landingAirport);
                        }
                    }

                    //createStartInbetweenPoints(sectionFile, targetArea);

                    //AirGroupWaypoint lastInterceptWaypoint = null;
                    //for (int i = targetAirUnit.Waypoints.Count - 1; i >= 0; i--)
                    //{
                    //    AirGroupWaypoint waypoint = targetAirUnit.Waypoints[i];
                    //    if (waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.HUNTING || waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.RECON || waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_POINT || waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG)
                    //    {
                    //        lastInterceptWaypoint = waypoint;
                    //        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, waypoint.X, waypoint.Y, waypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(waypoint)));
                    //    }
                    //}
                    //if (lastInterceptWaypoint != null)
                    //{
                    //    for (int i = 1; i < 2; i++)
                    //    {
                    //        if (targetAirUnit.Waypoints.IndexOf(lastInterceptWaypoint) - i >= 0)
                    //        {
                    //            AirGroupWaypoint previousWaypoint = targetAirUnit.Waypoints[targetAirUnit.Waypoints.IndexOf(lastInterceptWaypoint) - i];
                    //            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, previousWaypoint.X, previousWaypoint.Y, previousWaypoint.Z, 300.0, targetAirUnit.Name + " " + targetAirUnit.Waypoints.IndexOf(previousWaypoint)));
                    //        }
                    //    }
                    //}

                    //createEndInbetweenPoints(sectionFile, targetArea, landingAirport);
                    createEndWaypoints(sectionFile, landingAirport);

                    writeTo(sectionFile);
                    writeTo(debug);
                }

                #endregion
            }

            #endregion

            #region AirGroupWaypoint

            private class AirGroupWaypoint
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

                public AirGroupWaypointTypes Type;

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

                public string Target;

                #endregion
            }

            #endregion

            #region AircraftInfo

            public class AircraftInfo
            {
                public enum MissionType
                {
                    RECON_AREA,
                    GROUND_ATTACK_AREA,
                    OFFENSIVE_PATROL_AREA,
                    DEFENSIVE_PATROL_AREA,
                    ESCORT,
                    INTERCEPT,
                };

                public AircraftInfo(string aircraft)
                {
                    Aircraft = aircraft;
                }

                public List<MissionType> MissionTypes
                {
                    get
                    {
                        List<MissionType> missionTypes = new List<MissionType>();

                        if (reconAircrafts.Contains(Aircraft))
                        {
                            if (!missionTypes.Contains(MissionType.RECON_AREA))
                            {
                                missionTypes.Add(MissionType.RECON_AREA);
                            }
                        }

                        if (bomberAircrafts.Contains(Aircraft))
                        {
                            if (!missionTypes.Contains(MissionType.GROUND_ATTACK_AREA))
                            {
                                missionTypes.Add(MissionType.GROUND_ATTACK_AREA);
                            }
                        }

                        if (fighterAircrafts.Contains(Aircraft))
                        {
                            if (!missionTypes.Contains(MissionType.OFFENSIVE_PATROL_AREA))
                            {
                                missionTypes.Add(MissionType.OFFENSIVE_PATROL_AREA);
                            }
                            //if (!missionTypes.Contains(MissionType.DEFENSIVE_PATROL_AREA))
                            //{
                            //    missionTypes.Add(MissionType.DEFENSIVE_PATROL_AREA);
                            //}
                            if (!missionTypes.Contains(MissionType.ESCORT))
                            {
                                missionTypes.Add(MissionType.ESCORT);
                            }
                            if (!missionTypes.Contains(MissionType.INTERCEPT))
                            {
                                missionTypes.Add(MissionType.INTERCEPT);
                            }
                        }

                        return missionTypes;
                    }
                }

                public string Aircraft;

                public List<string> liaisonAircrafts = new List<string>
        {
            "Aircraft.AnsonMkI",
            "Aircraft.DH82A",
            "Aircraft.Bf-108B-2",
        };

                public List<string> reconAircrafts = new List<string>
        {
            "Aircraft.BlenheimMkI",
            "Aircraft.BlenheimMkIV",
            //"Aircraft.SunderlandMkI",
            //"Aircraft.WalrusMkI",
            "Aircraft.WellingtonMkIc", 
            //"Aircraft.Ju-87B-2", NO RECON
            "Aircraft.Do-17Z-1",
            "Aircraft.Do-17Z-2",
            "Aircraft.Do-215B-1",
            "Aircraft.FW-200C-1",
            "Aircraft.He-111H-2",
            "Aircraft.He-111P-2",
            "Aircraft.Ju-88A-1",
            //"Aircraft.He-115B-2"
            "Aircraft.BR-20M", 
        };

                public List<string> fighterAircrafts = new List<string>
        {
            "Aircraft.BeaufighterMkIF",
            "Aircraft.DefiantMkI",
            "Aircraft.GladiatorMkII",
            "Aircraft.HurricaneMkI_dH5-20",
            "Aircraft.HurricaneMkI",
            "Aircraft.SpitfireMkI",
            "Aircraft.SpitfireMkI_Heartbreaker",
            "Aircraft.SpitfireMkIa",
            "Aircraft.SpitfireMkIIa", 
            "Aircraft.Bf-109E-1",
            "Aircraft.Bf-109E-3",
            "Aircraft.Bf-109E-3B",
            "Aircraft.Bf-110C-4",
            "Aircraft.Bf-110C-7",
            "Aircraft.CR42",
            "Aircraft.G50",  
        };

                public List<string> bomberAircrafts = new List<string>
        {
            "Aircraft.BlenheimMkI",
            "Aircraft.BlenheimMkIV",
            //"Aircraft.SunderlandMkI",
            //"Aircraft.WalrusMkI",
            "Aircraft.WellingtonMkIc", 
            "Aircraft.Ju-87B-2",
            "Aircraft.Do-17Z-1",
            "Aircraft.Do-17Z-2",
            //"Aircraft.Do-215B-1", NO BOMBER
            "Aircraft.FW-200C-1",
            "Aircraft.He-111H-2",
            "Aircraft.He-111P-2",
            "Aircraft.Ju-88A-1",
            //"Aircraft.He-115B-2"
            "Aircraft.BR-20M", 
        };
            }

            #endregion

            #region AirGroupInfos

            private abstract class AirGroupInfo
            {
                #region Public properties

                public abstract List<string> Aircrafts
                {
                    get;
                }

                public abstract List<string> AirGroupKeys
                {
                    get;
                }

                public abstract int SquadronCount
                {
                    get;
                }

                public abstract int FlightCount
                {
                    get;
                }

                public abstract int FlightSize
                {
                    get;
                }

                public int AircraftMaxCount
                {
                    get { return FlightCount * FlightSize; }
                }

                #endregion
            }

            static private AirGroupInfo[] GetAirGroupInfos(int armyIndex)
            {
                if (armyIndex == 1)
                {
                    return RedAirGroupInfos;
                }
                else if (armyIndex == 2)
                {
                    return BlueAirGroupInfos;
                }
                else
                {
                    return new AirGroupInfo[] { };
                }
            }

            static private AirGroupInfo GetAirGroupInfo(int armyIndex, string airGroupKey)
            {
                foreach (AirGroupInfo airGroupInfo in GetAirGroupInfos(armyIndex))
                {
                    List<string> airGroupKeys = new List<string>(airGroupInfo.AirGroupKeys);
                    if (airGroupKeys.Contains(airGroupKey))
                    {
                        return airGroupInfo;
                    }
                }

                return null;
            }

            static private AirGroupInfo GetAirGroupInfo(string airGroupKey)
            {
                foreach (AirGroupInfo airGroupInfo in RedAirGroupInfos)
                {
                    List<string> airGroupKeys = new List<string>(airGroupInfo.AirGroupKeys);
                    if (airGroupKeys.Contains(airGroupKey))
                    {
                        return airGroupInfo;
                    }
                }
                foreach (AirGroupInfo airGroupInfo in BlueAirGroupInfos)
                {
                    List<string> airGroupKeys = new List<string>(airGroupInfo.AirGroupKeys);
                    if (airGroupKeys.Contains(airGroupKey))
                    {
                        return airGroupInfo;
                    }
                }

                return null;
            }

            static private AirGroupInfo[] RedAirGroupInfos = new AirGroupInfo[]
    {
        new RafFighterCommandEarlyAirGroupInfo(),
        new RafFighterCommandLateAirGroupInfo(),
        new RafBomberCommandAirGroupInfo(),
        new RafFlyingTrainingSchoolEarlyAirGroupInfo(),
        new RafFlyingTrainingSchoolLateAirGroupInfo(),
    };

            static private AirGroupInfo[] BlueAirGroupInfos = new AirGroupInfo[]
    {
        new LwFighterStabAirGroupInfo(),
        new LwFighterAirGroupInfo(),
        new LwZerstoererStabAirGroupInfo(),
        new LwZerstoererAirGroupInfo(),
        new LwStukaStabAirGroupInfo(),
        new LwStukaAirGroupInfo(),
        new LwBomberStabAirGroupInfo(),
        new LwBomberAirGroupInfo(),
        new LwTransportAirGroupInfo(),
        new LwReconAirGroupInfo(),
        new RaFighterAirGroupInfo(),
        new RaBomberAirGroupInfo(),
    };

            private class RafFighterCommandEarlyAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.BeaufighterMkIF",
            "Aircraft.DefiantMkI",
            "Aircraft.GladiatorMkII",
            "Aircraft.HurricaneMkI_dH5-20",
            "Aircraft.HurricaneMkI",
            "Aircraft.SpitfireMkI",
            //"Aircraft.SpitfireMkI_Heartbreaker",
            "Aircraft.SpitfireMkIa",
            "Aircraft.SpitfireMkIIa",            
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"gb01", /* Generic Fighter Command Early */
            "BoB_RAF_F_1Sqn_Early",
            "BoB_RAF_F_1_RCAF_Early",
            "BoB_RAF_F_111Sqn_Early",
            "BoB_RAF_F_141Sqn_Early",
            "BoB_RAF_F_145Sqn_Early",
            "BoB_RAF_F_151Sqn_Early",
            "BoB_RAF_F_152Sqn_Early",
            "BoB_RAF_F_17Sqn_Early",
            "BoB_RAF_F_19Sqn_Early",
            "BoB_RAF_F_213Sqn_Early",
            "BoB_RAF_F_219Sqn_Early",
            "BoB_RAF_F_222Sqn_Early",
            "BoB_RAF_F_229Sqn_Early",
            "BoB_RAF_F_23Sqn_Early",
            "BoB_RAF_F_232Sqn_Early",
            "BoB_RAF_F_234Sqn_Early",
            "BoB_RAF_F_235Sqn_Early",
            "BoB_RAF_F_236Sqn_Early",
            "BoB_RAF_F_238Sqn_Early",
            "BoB_RAF_F_242Sqn_Early",
            "BoB_RAF_F_245Sqn_Early",
            "BoB_RAF_F_247Sqn_Early",
            "BoB_RAF_F_248Sqn_Early",
            "BoB_RAF_F_249Sqn_Early",
            "BoB_RAF_F_25Sqn_Early",
            "BoB_RAF_F_253Sqn_Early",
            "BoB_RAF_F_257Sqn_Early",
            "BoB_RAF_F_263Sqn_Early",
            "BoB_RAF_F_264Sqn_Early",
            "BoB_RAF_F_266Sqn_Early",
            "BoB_RAF_F_29Sqn_Early",
            "BoB_RAF_F_3Sqn_Early",
            "BoB_RAF_F_302_PL_Early",
            "BoB_RAF_F_303_PL_Early",
            "BoB_RAF_F_310_CZ_Early",
            "BoB_RAF_F_312_CZ_Early",
            "BoB_RAF_F_32Sqn_Early",
            "BoB_RAF_F_41Sqn_Early",
            "BoB_RAF_F_43Sqn_Early",
            "BoB_RAF_F_46Sqn_Early",
            "BoB_RAF_F_501Sqn_Early",
            "BoB_RAF_F_504Sqn_Early",
            "BoB_RAF_F_54Sqn_Early",
            "BoB_RAF_F_56Sqn_Early",
            "BoB_RAF_F_600Sqn_Early",
            "BoB_RAF_F_601Sqn_Early",
            "BoB_RAF_F_602Sqn_Early",
            "BoB_RAF_F_603Sqn_Early",
            "BoB_RAF_F_604Sqn_Early",
            "BoB_RAF_F_605Sqn_Early",
            "BoB_RAF_F_607Sqn_Early",
            "BoB_RAF_F_609Sqn_Early",
            "BoB_RAF_F_610Sqn_Early",
            "BoB_RAF_F_611Sqn_Early",
            "BoB_RAF_F_615Sqn_Early",
            "BoB_RAF_F_616Sqn_Early",
            "BoB_RAF_F_64Sqn_Early",
            "BoB_RAF_F_65Sqn_Early",
            "BoB_RAF_F_66Sqn_Early",
            "BoB_RAF_F_72Sqn_Early",
            "BoB_RAF_F_73Sqn_Early",
            "BoB_RAF_F_74Sqn_Early",
            "BoB_RAF_F_79Sqn_Early",
            "BoB_RAF_F_85Sqn_Early",
            "BoB_RAF_F_87Sqn_Early",
            "BoB_RAF_F_92Sqn_Early",
            //"BoB_RAF_F_FatCat_Early", /* Fiction Early */
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 2; }
                }

                public override int FlightSize
                {
                    get { return 6; }
                }

                #endregion
            }

            private class RafFighterCommandLateAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.BeaufighterMkIF",
            "Aircraft.DefiantMkI",
            "Aircraft.GladiatorMkII",
            "Aircraft.HurricaneMkI_dH5-20",
            "Aircraft.HurricaneMkI",
            "Aircraft.SpitfireMkI",
            //"Aircraft.SpitfireMkI_Heartbreaker",
            "Aircraft.SpitfireMkIa",
            "Aircraft.SpitfireMkIIa",            
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"gb01_Late", /* Generic Fighter Command Late */
            "BoB_RAF_F_1Sqn_Late",
            "BoB_RAF_F_1_RCAF_Late",
            "BoB_RAF_F_111Sqn_Late",
            "BoB_RAF_F_141Sqn_Late",
            "BoB_RAF_F_145Sqn_Late",
            "BoB_RAF_F_151Sqn_Late",
            "BoB_RAF_F_152Sqn_Late",
            "BoB_RAF_F_17Sqn_Late",
            "BoB_RAF_F_19Sqn_Late",
            "BoB_RAF_F_213Sqn_Late",
            "BoB_RAF_F_219Sqn_Late",
            "BoB_RAF_F_222Sqn_Late",
            "BoB_RAF_F_229Sqn_Late",
            "BoB_RAF_F_23Sqn_Late",
            "BoB_RAF_F_232Sqn_Late",
            "BoB_RAF_F_234Sqn_Late",
            "BoB_RAF_F_235Sqn_Late",
            "BoB_RAF_F_236Sqn_Late",
            "BoB_RAF_F_238Sqn_Late",
            "BoB_RAF_F_242Sqn_Late",
            "BoB_RAF_F_245Sqn_Late",
            "BoB_RAF_F_247Sqn_Late",
            "BoB_RAF_F_248Sqn_Late",
            "BoB_RAF_F_249Sqn_Late",
            "BoB_RAF_F_25Sqn_Late",
            "BoB_RAF_F_253Sqn_Late",
            "BoB_RAF_F_257Sqn_Late",
            "BoB_RAF_F_263Sqn_Late",
            "BoB_RAF_F_264Sqn_Late",
            "BoB_RAF_F_266Sqn_Late",
            "BoB_RAF_F_29Sqn_Late",
            "BoB_RAF_F_3Sqn_Late",
            "BoB_RAF_F_302_PL_Late",
            "BoB_RAF_F_303_PL_Late",
            "BoB_RAF_F_310_CZ_Late",
            "BoB_RAF_F_312_CZ_Late",
            "BoB_RAF_F_32Sqn_Late",
            "BoB_RAF_F_41Sqn_Late",
            "BoB_RAF_F_43Sqn_Late",
            "BoB_RAF_F_46Sqn_Late",
            "BoB_RAF_F_501Sqn_Late",
            "BoB_RAF_F_504Sqn_Late",
            "BoB_RAF_F_54Sqn_Late",
            "BoB_RAF_F_56Sqn_Late",
            "BoB_RAF_F_600Sqn_Late",
            "BoB_RAF_F_601Sqn_Late",
            "BoB_RAF_F_602Sqn_Late",
            "BoB_RAF_F_603Sqn_Late",
            "BoB_RAF_F_604Sqn_Late",
            "BoB_RAF_F_605Sqn_Late",
            "BoB_RAF_F_607Sqn_Late",
            "BoB_RAF_F_609Sqn_Late",
            "BoB_RAF_F_610Sqn_Late",
            "BoB_RAF_F_611Sqn_Late",
            "BoB_RAF_F_615Sqn_Late",
            "BoB_RAF_F_616Sqn_Late",
            "BoB_RAF_F_64Sqn_Late",
            "BoB_RAF_F_65Sqn_Late",
            "BoB_RAF_F_66Sqn_Late",
            "BoB_RAF_F_72Sqn_Late",
            "BoB_RAF_F_73Sqn_Late",
            "BoB_RAF_F_74Sqn_Late",
            "BoB_RAF_F_79Sqn_Late",
            "BoB_RAF_F_85Sqn_Late",
            "BoB_RAF_F_87Sqn_Late",
            "BoB_RAF_F_92Sqn_Late",
            //"BoB_RAF_F_FatCat_Late", /* Fiction Early */
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 4; }
                }

                #endregion
            }

            private class RafBomberCommandAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.AnsonMkI",
            "Aircraft.BlenheimMkI",
            "Aircraft.BlenheimMkIV",
            //"Aircraft.SunderlandMkI",
            //"Aircraft.WalrusMkI",
            "Aircraft.WellingtonMkIc",           
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"gb02", /* Generic Command Bomber */
            "BoB_RAF_B_10Sqn",
            "BoB_RAF_B_101Sqn",
            "BoB_RAF_B_102Sqn",
            "BoB_RAF_B_103Sqn",
            "BoB_RAF_B_104Sqn",
            "BoB_RAF_B_105Sqn",
            "BoB_RAF_B_106Sqn",
            "BoB_RAF_B_107Sqn",
            "BoB_RAF_B_110Sqn",
            "BoB_RAF_B_114Sqn",
            "BoB_RAF_B_115Sqn",
            "BoB_RAF_B_12Sqn",
            "BoB_RAF_B_139Sqn",
            "BoB_RAF_B_142Sqn",
            "BoB_RAF_B_144Sqn",
            "BoB_RAF_B_148Sqn",
            "BoB_RAF_B_149Sqn",
            "BoB_RAF_B_15Sqn",
            "BoB_RAF_B_150Sqn",
            "BoB_RAF_B_166Sqn",
            "BoB_RAF_B_207Sqn",
            "BoB_RAF_B_21Sqn",
            "BoB_RAF_B_214Sqn",
            "BoB_RAF_B_215Sqn",
            "BoB_RAF_B_218Sqn",
            "BoB_RAF_B_35Sqn",
            "BoB_RAF_B_37Sqn",
            "BoB_RAF_B_38Sqn",
            "BoB_RAF_B_40Sqn",
            "BoB_RAF_B_44Sqn",
            "BoB_RAF_B_48Sqn",
            "BoB_RAF_B_49Sqn",
            "BoB_RAF_B_50Sqn",
            "BoB_RAF_B_51Sqn",
            "BoB_RAF_B_52Sqn",
            "BoB_RAF_B_58Sqn",
            "BoB_RAF_B_61Sqn",
            "BoB_RAF_B_7Sqn",
            "BoB_RAF_B_75Sqn",
            "BoB_RAF_B_76Sqn",
            "BoB_RAF_B_77Sqn",
            "BoB_RAF_B_78Sqn",
            "BoB_RAF_B_82Sqn",
            "BoB_RAF_B_83Sqn",
            "BoB_RAF_B_88Sqn",
            "BoB_RAF_B_9Sqn",
            "BoB_RAF_B_90Sqn",
            "BoB_RAF_B_97Sqn",
            "BoB_RAF_B_98Sqn",
            "BoB_RAF_B_99Sqn",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 2; }
                }

                public override int FlightSize
                {
                    get { return 6; }
                }

                #endregion
            }

            private class RafFlyingTrainingSchoolEarlyAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.AnsonMkI",
            "Aircraft.DH82A",           
        };

                public List<string> airGroupKeys = new List<string>
        {
            "LONDON",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 2; }
                }

                public override int FlightSize
                {
                    get { return 6; }
                }

                #endregion
            }

            private class RafFlyingTrainingSchoolLateAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.AnsonMkI",
            "Aircraft.DH82A",           
        };

                public List<string> airGroupKeys = new List<string>
        {
            "LONDON_Late",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 4; }
                }

                #endregion
            }

            private class LwFighterStabAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Bf-109E-1",
            "Aircraft.Bf-109E-3",
            "Aircraft.Bf-109E-3B",
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_JG2_Stab",
            "BoB_LW_JG27_Stab",
            "BoB_LW_JG3_Stab",
            "BoB_LW_JG51_Stab",
            "BoB_LW_JG52_Stab",
            "BoB_LW_JG53_Stab",
            "BoB_LW_JG54_Stab",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 4; }
                }

                #endregion
            }

            private class LwFighterAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> fighterAircrafts = new List<string>
        {
            "Aircraft.Bf-109E-1",
            "Aircraft.Bf-109E-3",
            "Aircraft.Bf-109E-3B",
        };

                public List<string> fighterAirGroupKeys = new List<string>
        {
            //"g01", /* Generic Fighter */
            //"g04", /* Generic Fighter Bomber */
            "BoB_LW_ErprGr210F",
            "BoB_LW_LG2_I",
            "BoB_LW_JG2_I",
            "BoB_LW_JG26_I",
            "BoB_LW_JG27_I",
            "BoB_LW_JG3_I",
            "BoB_LW_JG51_I",
            "BoB_LW_JG52_I",
            "BoB_LW_JG53_I",
            "BoB_LW_JG54_I",
            "BoB_LW_JG77_I",
            "BoB_LW_JG2_II",
            "BoB_LW_JG26_II",
            "BoB_LW_JG27_II",
            "BoB_LW_JG3_II",
            "BoB_LW_JG51_II",
            "BoB_LW_JG52_II",
            "BoB_LW_JG53_II",
            "BoB_LW_JG54_II",
            "BoB_LW_JG2_III",
            "BoB_LW_JG26_III",
            "BoB_LW_JG27_III",
            "BoB_LW_JG3_III",
            "BoB_LW_JG51_III",
            "BoB_LW_JG52_III",
            "BoB_LW_JG53_III",
            "BoB_LW_JG54_III",
            "BoB_LW_LG1_V"
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return fighterAircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return fighterAirGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 4; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 4; }
                }

                #endregion
            }

            private class LwZerstoererStabAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Bf-110C-4",
            "Aircraft.Bf-110C-7",
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_ZG2_Stab",
            "BoB_LW_ZG26_Stab",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 4; }
                }

                #endregion
            }

            private class LwZerstoererAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Bf-110C-4",
            "Aircraft.Bf-110C-7",
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_ErprGr210",
            "BoB_LW_ZG2_I",
            "BoB_LW_ZG26_I",
            "BoB_LW_ZG2_II",
            "BoB_LW_ZG26_II",
            "BoB_LW_ZG76_II",
            "BoB_LW_ZG26_III",
            "BoB_LW_ZG76_III",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 4; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 4; }
                }

                #endregion
            }

            private class LwStukaStabAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Ju-87B-2",
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_StG1_Stab",
            "BoB_LW_StG2_Stab",
            "BoB_LW_StG3_Stab",
            "BoB_LW_StG77_Stab",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            private class LwStukaAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Ju-87B-2",
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"g03", /* Generic Stuka */
            "BoB_LW_StG1_I",
            "BoB_LW_StG2_I",
            "BoB_LW_StG3_I",
            "BoB_LW_StG77_I",
            "BoB_LW_StG1_II",
            "BoB_LW_StG2_II",
            "BoB_LW_StG3_II",
            "BoB_LW_StG77_II",
            "BoB_LW_StG1_III",
            "BoB_LW_StG3_III",
            "BoB_LW_StG77_III",
            "BoB_LW_LG1_IV",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 4; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            private class LwBomberStabAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Do-17Z-1",
            "Aircraft.Do-17Z-2",
            "Aircraft.Do-215B-1",
            "Aircraft.FW-200C-1",
            "Aircraft.He-111H-2",
            "Aircraft.He-111P-2",
            "Aircraft.Ju-88A-1"
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_KGr_100",
            "BoB_LW_KGr_806",
            "BoB_LW_KuFlGr_106",
            "BoB_LW_KuFlGr_406",
            "BoB_LW_KuFlGr_506",
            "BoB_LW_KuFlGr_606",
            "BoB_LW_KuFlGr_706",
            "BoB_LW_KG26_Stab",
            "BoB_LW_KG27_Stab",
            "BoB_LW_KG3_Stab",
            "BoB_LW_KG30_Stab",
            "BoB_LW_KG40_Stab",
            "BoB_LW_KG51_Stab",
            "BoB_LW_KG54_Stab",
            "BoB_LW_KGzbV1_Stab",
            "BoB_LW_ZG76_Stab",
            "BoB_LW_KG76_Stab",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            private class LwBomberAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Do-17Z-1",
            "Aircraft.Do-17Z-2",
            "Aircraft.Do-215B-1",
            "Aircraft.FW-200C-1",
            "Aircraft.He-111H-2",
            "Aircraft.He-111P-2",
            "Aircraft.Ju-88A-1"
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"g02", /* Generic Bomber */
            //"g05", /* Generic Training */
            //"g06", /* Generic Transport */
            "BoB_LW_KG1_I",
            "BoB_LW_KG2_I",
            "BoB_LW_KG26_I",
            "BoB_LW_KG27_I",
            "BoB_LW_KG3_I",
            "BoB_LW_KG30_I",
            "BoB_LW_KG4_I",
            "BoB_LW_KG40_I",
            "BoB_LW_KG51_I",
            "BoB_LW_KG53_I",
            "BoB_LW_KG54_I",
            "BoB_LW_KG55_I",
            "BoB_LW_KG76_I",
            "BoB_LW_LG1_I",
            "BoB_LW_LG2_II",
            "BoB_LW_KG1_II",
            "BoB_LW_KG2_II",
            "BoB_LW_KG26_II",
            "BoB_LW_KG27_II",
            "BoB_LW_KG3_II",
            "BoB_LW_KG30_II",
            "BoB_LW_KG4_II",
            "BoB_LW_KG51_II",
            "BoB_LW_KG53_II",
            "BoB_LW_KG54_II",
            "BoB_LW_KG55_II",
            "BoB_LW_KG76_II",
            "BoB_LW_LG1_II",
            "BoB_LW_KG1_III",
            "BoB_LW_KG2_III",
            "BoB_LW_KG26_III",
            "BoB_LW_KG27_III",
            "BoB_LW_KG3_III",
            "BoB_LW_KG30_III",
            "BoB_LW_KG4_III",
            "BoB_LW_KG51_III",
            "BoB_LW_KG53_III",
            "BoB_LW_KG55_III",
            "BoB_LW_KG76_III",
            "BoB_LW_LG1_III",
            "BoB_LW_KG1_IV",
            "BoB_LW_KG27_IV",
            "BoB_LW_KG2_IV",
            "BoB_LW_KG3_IV",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 4; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            private class LwTransportAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.FW-200C-1",
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_KGzbV1_I",
            "BoB_LW_KGzbV1_II",
            "BoB_LW_KGzbV1_III",
            "BoB_LW_KGzbV1_IV",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 5; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            private class LwReconAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.Do-17Z-1",
            "Aircraft.Do-17Z-2",
            "Aircraft.Do-215B-1",
            "Aircraft.FW-200C-1",
            "Aircraft.He-111H-2",
            "Aircraft.He-111P-2",
            "Aircraft.Ju-88A-1",
        };

                public List<string> airGroupKeys = new List<string>
        {
            "BoB_LW_AufklGr_120",
            "BoB_LW_AufklGr_121",
            "BoB_LW_AufklGr_122",
            "BoB_LW_AufklGr_123",
            "BoB_LW_AufklGr_22",
            "BoB_LW_AufklGr_11",
            "BoB_LW_AufklGr_14",
            "BoB_LW_AufklGr_31",
            "BoB_LW_AufklGr_ObdL",
            "BoB_LW_AufklGr10",
            "BoB_LW_Wekusta_51",
            "BoB_LW_Wekusta_ObdL",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 4; }
                }

                public override int FlightSize
                {
                    get { return 1; }
                }

                #endregion
            }

            private class RaFighterAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.CR42",
            "Aircraft.G50",            
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"it02", /* Generic Fighter */
            "BoB_RA_56St_18Gruppo_83Sq",
            "BoB_RA_56St_18Gruppo_85Sq",
            "BoB_RA_56St_18Gruppo_95Sq",
            "BoB_RA_56St_20Gruppo_351Sq",
            "BoB_RA_56St_20Gruppo_352Sq",
            "BoB_RA_56St_20Gruppo_353Sq",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            private class RaBomberAirGroupInfo : AirGroupInfo
            {
                #region Private members

                public List<string> aircrafts = new List<string>
        {
            "Aircraft.BR-20M",         
        };

                public List<string> airGroupKeys = new List<string>
        {
            //"it01", /* Generic Bomber */
            "BoB_RA_13St_11Gruppo_1Sq",
            "BoB_RA_13St_11Gruppo_4Sq",
            "BoB_RA_13St_43Gruppo_3Sq",
            "BoB_RA_13St_43Gruppo_5Sq",
            "BoB_RA_43St_98Gruppo_240Sq",
            "BoB_RA_43St_98Gruppo_241Sq",
            "BoB_RA_43St_99Gruppo_242Sq",
            "BoB_RA_43St_99Gruppo_243Sq",
        };

                #endregion

                #region Public properties

                public override List<string> Aircrafts
                {
                    get
                    {
                        return aircrafts;
                    }
                }

                public override List<string> AirGroupKeys
                {
                    get
                    {
                        return airGroupKeys;
                    }
                }

                public override int SquadronCount
                {
                    get { return 1; }
                }

                public override int FlightCount
                {
                    get { return 3; }
                }

                public override int FlightSize
                {
                    get { return 3; }
                }

                #endregion
            }

            #endregion

            #region Radar

            private class Radar
            {
                public Radar(string name, double x, double y)
                {
                    Name = name;
                    X = x;
                    Y = y;
                }

                public string Name;

                public double X;

                public double Y;
            }

            #endregion

            #region Private variables

            static ISectionFile debug = null;

            static bool spawnParked = false;

            static Random rand = new Random();

            static List<AirGroup> availableAirGroups = new List<AirGroup>();

            private int maxRandomSpawn = 1;

            int? playerSquadronIndex = null;
            int? playerFlightIndex = null;
            int? playerAircraftIndex = null;
            string playerAirGroupKey = null;
            AirGroup playerAirGroup = null;


            ISectionFile airGroupsTemplate;
            private List<AirGroup> redAirGroups = new List<AirGroup>();
            private List<AirGroup> blueAirGroups = new List<AirGroup>();

            ISectionFile markersTemplate;
            private List<Point3d> redMarkers = new List<Point3d>();
            private List<Point3d> blueMarkers = new List<Point3d>();
            private List<Point3d> neutralMarkers = new List<Point3d>();

            ISectionFile radarsTemplate;
            private List<Radar> redRadars = new List<Radar>();
            private List<Radar> blueRadars = new List<Radar>();

            #endregion

            #region Private methods

            private List<AirGroup> getAirGroups(int armyIndex)
            {
                if (armyIndex == 1)
                {
                    return redAirGroups;
                }
                else if (armyIndex == 2)
                {
                    return blueAirGroups;
                }
                else
                {
                    return new List<AirGroup>();
                }
            }

            private List<Radar> getRadars(int armyIndex)
            {
                if (armyIndex == 1)
                {
                    return redRadars;
                }
                else if (armyIndex == 2)
                {
                    return blueRadars;
                }
                else
                {
                    return new List<Radar>();
                }
            }

            private List<Point3d> getFriendlyMarkers(int armyIndex)
            {
                if (armyIndex == 1)
                {
                    return redMarkers;
                }
                else if (armyIndex == 2)
                {
                    return blueMarkers;
                }
                else
                {
                    return new List<Point3d>();
                }
            }

            private List<Point3d> getEnemyMarkers(int armyIndex)
            {
                if (armyIndex == 1)
                {
                    return blueMarkers;
                }
                else if (armyIndex == 2)
                {
                    return redMarkers;
                }
                else
                {
                    return new List<Point3d>();
                }
            }

            private double createRandomAltitude(AircraftInfo.MissionType missionType)
            {
                // TODO: Altitude range depends on mission type.
                return (double)rand.Next(500, 6000);
            }

            private AirGroup getRandomInterceptedFlight(AirGroup interceptingAirUnit, AircraftInfo.MissionType missionType)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex != interceptingAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(missionType))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (escortAirGroup == null)
                            //{
                            //    escortAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    Point3d targetAirUnitPos = targetAirUnit.Position;
                            //    if (airGroup.Position.distance(ref targetAirUnitPos) < escortAirGroup.Position.distance(ref targetAirUnitPos))
                            //    {
                            //        escortAirGroup = airGroup;
                            //    }
                            //}
                        }
                    }
                }

                if (airGroups.Count > 0)
                {
                    int escortedAirGroupIndex = rand.Next(airGroups.Count);
                    AirGroup escortedAirGroup = airGroups[escortedAirGroupIndex];

                    return escortedAirGroup;
                }
                else
                {
                    return null;
                }
            }

            private AirGroup getRandomEscortedFlight(AirGroup escortingAirUnit)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex == escortingAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(AircraftInfo.MissionType.GROUND_ATTACK_AREA))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (escortAirGroup == null)
                            //{
                            //    escortAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    Point3d targetAirUnitPos = targetAirUnit.Position;
                            //    if (airGroup.Position.distance(ref targetAirUnitPos) < escortAirGroup.Position.distance(ref targetAirUnitPos))
                            //    {
                            //        escortAirGroup = airGroup;
                            //    }
                            //}
                        }
                    }
                }

                if (airGroups.Count > 0)
                {
                    int escortedAirGroupIndex = rand.Next(airGroups.Count);
                    AirGroup escortedAirGroup = airGroups[escortedAirGroupIndex];

                    return escortedAirGroup;
                }
                else
                {
                    return null;
                }
            }

            private AirGroup getRandomEscortFlight(AirGroup targetAirUnit)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex == targetAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(AircraftInfo.MissionType.ESCORT))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (escortAirGroup == null)
                            //{
                            //    escortAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    Point3d targetAirUnitPos = targetAirUnit.Position;
                            //    if (airGroup.Position.distance(ref targetAirUnitPos) < escortAirGroup.Position.distance(ref targetAirUnitPos))
                            //    {
                            //        escortAirGroup = airGroup;
                            //    }
                            //}
                        }
                    }
                }

                if (airGroups.Count > 0)
                {
                    int escortAirGroupIndex = rand.Next(airGroups.Count);
                    AirGroup escortAirGroup = airGroups[escortAirGroupIndex];

                    return escortAirGroup;
                }
                else
                {
                    return null;
                }
            }

            private void createRandomInterceptFlight(ISectionFile sectionFile, AirGroup targetAirUnit, Point3d targetArea)
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                foreach (AirGroup airGroup in availableAirGroups)
                {
                    if (airGroup.ArmyIndex != targetAirUnit.ArmyIndex)
                    {
                        if (airGroup.AircraftInfo.MissionTypes.Contains(AircraftInfo.MissionType.INTERCEPT))
                        {
                            airGroups.Add(airGroup);

                            // TODO: Check distance between target and escort

                            //if (interceptAirGroup == null)
                            //{
                            //    interceptAirGroup = airGroup;
                            //}
                            //else
                            //{
                            //    if (airGroup.Position.distance(ref targetArea) < interceptAirGroup.Position.distance(ref targetArea))
                            //    {
                            //        interceptAirGroup = airGroup;
                            //    }
                            //}
                        }
                    }
                }

                if (airGroups.Count > 0)
                {
                    int interceptAirGroupIndex = rand.Next(airGroups.Count);
                    AirGroup interceptAirGroup = airGroups[interceptAirGroupIndex];

                    availableAirGroups.Remove(interceptAirGroup);

                    interceptAirGroup.CreateInterceptFlight(sectionFile, targetAirUnit, targetArea);
                    GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, interceptAirGroup.Name + ": Intercept flight(" + targetAirUnit.Name + ")", null);
                }
            }

            private void createRandomFlight(ISectionFile sectionFile, AirGroup airGroup)
            {
                List<AircraftInfo.MissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
                if (missionTypes != null && missionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = rand.Next(missionTypes.Count);
                    AircraftInfo.MissionType randomMissionType = missionTypes[randomMissionTypeIndex];

                    // Bomber mission types
                    if (randomMissionType == AircraftInfo.MissionType.RECON_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                            airGroup.CreateReconFlight(sectionFile, targetArea);

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);

                            GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Recon flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                        }
                    }
                    else if (randomMissionType == AircraftInfo.MissionType.GROUND_ATTACK_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            AirGroup escortAirGroup = getRandomEscortFlight(airGroup);
                            if (escortAirGroup != null)
                            {
                                availableAirGroups.Remove(escortAirGroup);

                                Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                                airGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                                escortAirGroup.CreateEscortFlight(sectionFile, airGroup);

                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ") with " + escortAirGroup.Name + ": Escort flight(" + airGroup.Name + ")", null);
                            }
                            else
                            {
                                airGroup.CreateGroundAttackFlight(sectionFile, targetArea);
                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                            }

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                        }
                    }

                    // Fighter mission types
                    //else if (randomMissionType == AircraftInfo.MissionType.DEFENSIVE_PATROL_AREA)
                    //{
                    //    List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                    //    if (friendlyMarkers.Count > 0)
                    //    {
                    //        int markerIndex = rand.Next(friendlyMarkers.Count);
                    //        Point3d marker = friendlyMarkers[markerIndex];
                    //        Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                    //        airGroup.CreateCoverFlight(sectionFile, targetArea);

                    //        GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Defensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                    //    }
                    //}
                    else if (randomMissionType == AircraftInfo.MissionType.OFFENSIVE_PATROL_AREA)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));
                            airGroup.CreateHuntingFlight(sectionFile, targetArea);

                            GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                            createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                        }
                    }
                    else if (randomMissionType == AircraftInfo.MissionType.ESCORT)
                    {
                        List<Point3d> enemyMarkers = getEnemyMarkers(airGroup.ArmyIndex);
                        if (enemyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(enemyMarkers.Count);
                            Point3d marker = enemyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            AirGroup escortedAirGroup = getRandomEscortedFlight(airGroup);
                            if (escortedAirGroup != null)
                            {
                                availableAirGroups.Remove(escortedAirGroup);

                                Point3d rendevouzPosition = new Point3d(airGroup.Position.x + 0.50 * (escortedAirGroup.Position.x - airGroup.Position.x), airGroup.Position.y + 0.50 * (escortedAirGroup.Position.y - airGroup.Position.y), targetArea.z);

                                escortedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                                airGroup.CreateEscortFlight(sectionFile, escortedAirGroup);

                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Escort flight(" + airGroup.Name + ") for " + escortedAirGroup.Name + ": Ground attack flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                                createRandomInterceptFlight(sectionFile, escortedAirGroup, targetArea);
                            }
                            else
                            {
                                airGroup.CreateHuntingFlight(sectionFile, targetArea);

                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": No escort required. Instead offensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);

                                createRandomInterceptFlight(sectionFile, airGroup, targetArea);
                            }
                        }
                    }
                    else if (randomMissionType == AircraftInfo.MissionType.INTERCEPT)
                    {
                        List<Point3d> friendlyMarkers = getFriendlyMarkers(airGroup.ArmyIndex);
                        if (friendlyMarkers.Count > 0)
                        {
                            int markerIndex = rand.Next(friendlyMarkers.Count);
                            Point3d marker = friendlyMarkers[markerIndex];
                            Point3d targetArea = new Point3d(marker.x, marker.y, createRandomAltitude(randomMissionType));

                            List<AircraftInfo.MissionType> subMissionTypes = new List<AircraftInfo.MissionType>() { AircraftInfo.MissionType.OFFENSIVE_PATROL_AREA, AircraftInfo.MissionType.RECON_AREA, AircraftInfo.MissionType.GROUND_ATTACK_AREA };
                            int randomSubMissionTypeIndex = rand.Next(subMissionTypes.Count);
                            AircraftInfo.MissionType randomSubMissionType = subMissionTypes[randomSubMissionTypeIndex];

                            AirGroup interceptedAirGroup = getRandomInterceptedFlight(airGroup, randomSubMissionType);
                            if (interceptedAirGroup != null)
                            {
                                availableAirGroups.Remove(interceptedAirGroup);

                                if (randomSubMissionType == AircraftInfo.MissionType.OFFENSIVE_PATROL_AREA)
                                {
                                    interceptedAirGroup.CreateHuntingFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == AircraftInfo.MissionType.RECON_AREA)
                                {
                                    interceptedAirGroup.CreateReconFlight(sectionFile, targetArea);
                                }
                                else if (randomSubMissionType == AircraftInfo.MissionType.GROUND_ATTACK_AREA)
                                {
                                    AirGroup escortAirGroup = getRandomEscortFlight(interceptedAirGroup);
                                    if (escortAirGroup != null)
                                    {
                                        availableAirGroups.Remove(escortAirGroup);

                                        Point3d rendevouzPosition = new Point3d(interceptedAirGroup.Position.x + 0.50 * (escortAirGroup.Position.x - interceptedAirGroup.Position.x), interceptedAirGroup.Position.y + 0.50 * (escortAirGroup.Position.y - interceptedAirGroup.Position.y), targetArea.z);

                                        interceptedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea, rendevouzPosition);
                                        escortAirGroup.CreateEscortFlight(sectionFile, interceptedAirGroup);
                                    }
                                    else
                                    {
                                        interceptedAirGroup.CreateGroundAttackFlight(sectionFile, targetArea);
                                    }
                                }

                                airGroup.CreateInterceptFlight(sectionFile, interceptedAirGroup, targetArea);
                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": Intercept flight(" + interceptedAirGroup.Name + ")", null);
                            }
                            else
                            {
                                airGroup.CreateCoverFlight(sectionFile, targetArea);
                                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, airGroup.Name + ": No intercept required. Instead defensive patrol flight(" + targetArea.x + "," + targetArea.y + "," + targetArea.z + ")", null);
                            }
                        }
                    }
                }
            }

            #endregion

            public override void Init(ABattle battle, int missionNumber)
            {
                base.Init(battle, missionNumber);

                debug = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/Main.mis");

                radarsTemplate = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/__Radars.mis");

                for (int i = 0; i < radarsTemplate.lines("Stationary"); i++)
                {
                    string key;
                    string value;
                    radarsTemplate.get("Stationary", i, out key, out value);

                    // Radar
                    string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (valueParts.Length > 4)
                    {
                        if (valueParts[0] == "Stationary.Radar.EnglishRadar1")
                        {
                            double x;
                            double y;
                            double.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x);
                            double.TryParse(valueParts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y);
                            Radar radar = new Radar(key, x, y);
                            redRadars.Add(radar);
                        }
                    }
                }

                markersTemplate = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/__Markers.mis");

                for (int i = 0; i < markersTemplate.lines("FrontMarker"); i++)
                {
                    string key;
                    string value;
                    markersTemplate.get("FrontMarker", i, out key, out value);

                    string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (valueParts.Length == 3)
                    {
                        double x;
                        double y;
                        int army;
                        if (double.TryParse(valueParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x)
                            && double.TryParse(valueParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y)
                            && int.TryParse(valueParts[2], out army))
                        {
                            if (army == 0)
                            {
                                neutralMarkers.Add(new Point3d(x, y, 0.0));
                            }
                            else if (army == 1)
                            {
                                redMarkers.Add(new Point3d(x, y, 0.0));
                            }
                            else if (army == 2)
                            {
                                blueMarkers.Add(new Point3d(x, y, 0.0));
                            }
                        }
                    }
                }

                airGroupsTemplate = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/Campaigns/Prototype/__AirGroups.mis");

                availableAirGroups.Clear();

                for (int i = 0; i < airGroupsTemplate.lines("AirGroups"); i++)
                {
                    string key;
                    string value;
                    airGroupsTemplate.get("AirGroups", i, out key, out value);

                    AirGroup airGroup = new AirGroup(airGroupsTemplate, key);
                    availableAirGroups.Add(airGroup);

                    if (GetAirGroupInfo(1, airGroup.AirGroupKey) != null)
                    {
                        getAirGroups(1).Add(airGroup);
                    }
                    else if (GetAirGroupInfo(2, airGroup.AirGroupKey) != null)
                    {
                        getAirGroups(2).Add(airGroup);
                    }
                }

                if (airGroupsTemplate.exist("MAIN", "player"))
                {
                    string playerAircraftId = airGroupsTemplate.get("MAIN", "player");

                    int result;
                    int.TryParse(playerAircraftId.Substring(playerAircraftId.LastIndexOf(".") + 1, 1), out result);
                    playerSquadronIndex = result;
                    int.TryParse(playerAircraftId.Substring(playerAircraftId.LastIndexOf(".") + 2, 1), out result);
                    playerFlightIndex = result;
                    int.TryParse(playerAircraftId.Substring(playerAircraftId.LastIndexOf(".") + 3, 1), out result);
                    playerAircraftIndex = result;

                    playerAirGroupKey = playerAircraftId.Substring(playerAircraftId.IndexOf(":") + 1, playerAircraftId.LastIndexOf(".") - playerAircraftId.IndexOf(":") - 1);

                    // Find the air group of the player.
                    if (playerAirGroupKey != null && playerSquadronIndex != null && playerFlightIndex != null && playerAircraftIndex != null)
                    {
                        foreach (AirGroup airGroup in getAirGroups(1))
                        {
                            if (airGroup.AirGroupKey == playerAirGroupKey &&
                                airGroup.SquadronIndex == playerSquadronIndex &&
                                airGroup.Flight.Length > playerFlightIndex &&
                                airGroup.Flight[(int)playerFlightIndex] != null &&
                                airGroup.Flight[(int)playerFlightIndex].Length > playerAircraftIndex)
                            {
                                playerAirGroup = airGroup;
                            }
                        }
                        foreach (AirGroup airGroup in getAirGroups(2))
                        {
                            if (airGroup.AirGroupKey == playerAirGroupKey &&
                                airGroup.SquadronIndex == playerSquadronIndex &&
                                airGroup.Flight.Length > playerFlightIndex &&
                                airGroup.Flight[(int)playerFlightIndex] != null &&
                                airGroup.Flight[(int)playerFlightIndex].Length > playerAircraftIndex)
                            {
                                playerAirGroup = airGroup;
                            }
                        }
                    }
                }
            }

            public override void OnBattleStarted()
            {
                base.OnBattleStarted();

                GamePlay.gpPostMissionLoad(radarsTemplate);
                GamePlay.gpPostMissionLoad(markersTemplate);

                // Assign the closest airport to each air unit.
                if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                {
                    foreach (int armyIndex in GamePlay.gpArmies())
                    {
                        foreach (AirGroup airGroup in getAirGroups(armyIndex))
                        {
                            // Check type of waypoint. LANDING = use airport; NORMALFLY = use position
                            if (airGroup.Waypoints.Count > 0 && airGroup.Waypoints[0].Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF)
                            {
                                // Find the closest airport.
                                AiAirport closestAirport = null;
                                foreach (AiAirport airport in GamePlay.gpAirports())
                                {
                                    if (closestAirport == null)
                                    {
                                        closestAirport = airport;
                                    }
                                    else
                                    {
                                        Point3d airportPos = airport.Pos();
                                        Point3d closestAirportPos = closestAirport.Pos();

                                        Point3d p = new Point3d(airGroup.Waypoints[0].X, airGroup.Waypoints[0].Y, airGroup.Waypoints[0].Z);
                                        if (p.distance(ref airportPos) < p.distance(ref closestAirportPos))
                                        {
                                            closestAirport = airport;
                                        }
                                    }
                                }
                                airGroup.Airport = closestAirport;
                            }
                        }
                    }
                }

                ISectionFile sectionFile = GamePlay.gpCreateSectionFile();
                // Generate flights
                if (playerAirGroup != null)
                {
                    availableAirGroups.Remove(playerAirGroup);

                    createRandomFlight(sectionFile, playerAirGroup);
                }

                for (int i = 0; i < maxRandomSpawn; i++)
                {
                    int randomAirGroupIndex = rand.Next(availableAirGroups.Count);
                    AirGroup randomAirGroup = availableAirGroups[randomAirGroupIndex];
                    availableAirGroups.Remove(randomAirGroup);

                    createRandomFlight(sectionFile, randomAirGroup);
                }

                GamePlay.gpPostMissionLoad(sectionFile);

                #if DEBUG

                debug.save("$user/missions/__Debug.mis");

                #endif
            }

            public override void OnMissionLoaded(int missionNumber)
            {
                base.OnMissionLoaded(missionNumber);

                placePlayer();
            }

            public void placePlayer()
            {
                // Place player
                if (GamePlay.gpPlayer().Place() == null && playerAirGroupKey != null && playerSquadronIndex != null && playerFlightIndex != null && playerAircraftIndex != null)
                {
                    if (GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                    {
                        foreach (int armyIndex in GamePlay.gpArmies())
                        {
                            if (GamePlay.gpAirGroups(armyIndex) != null && GamePlay.gpAirGroups(armyIndex).Length > 0)
                            {
                                foreach (AiAirGroup aiAirGroup in GamePlay.gpAirGroups(armyIndex))
                                {
                                    if (aiAirGroup.GetItems() != null && aiAirGroup.GetItems().Length > 0)
                                    {
                                        foreach (AiActor aiActor in aiAirGroup.GetItems())
                                        {
                                            if (aiActor.Name().Remove(0, aiActor.Name().IndexOf(":") + 1) == playerAirGroupKey + "." + playerSquadronIndex.Value.ToString() + playerFlightIndex.Value.ToString() + playerAircraftIndex.Value.ToString())
                                            {
                                                GamePlay.gpPlayer().PlaceEnter(aiActor, 0);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public override void OnTickGame()
            {
                base.OnTickGame();
            }
        }
    }
}