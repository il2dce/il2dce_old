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
    public class AirGroup : IAirGroup
    {
        #region Public constructors

        public AirGroup(ISectionFile sectionFile, string airGroupId)
        {
            // airGroupId = <airGroupKey>.<squadronIndex><flightMask>

            // AirGroupKey
            AirGroupKey = airGroupId.Substring(0, airGroupId.IndexOf("."));

            // SquadronIndex
            int.TryParse(airGroupId.Substring(airGroupId.LastIndexOf(".") + 1, 1), out squadronIndex);

            // Flight
            for (int i = 0; i < 4; i++)
            {
                if (sectionFile.exist(airGroupId, "Flight" + i.ToString()))
                {
                    string acNumberLine = sectionFile.get(airGroupId, "Flight" + i.ToString());
                    string[] acNumberList = acNumberLine.Split(new char[] { ' ' });
                    if (acNumberList != null && acNumberList.Length > 0)
                    {
                        List<string> acNumbers = new List<string>();
                        Flights.Add(i, acNumbers);
                        for (int j = 0; j < acNumberList.Length; j++)
                        {
                            acNumbers.Add(acNumberList[j]);
                        }
                    }
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
                if (Waypoints[0].Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF)
                {
                    Airstart = false;
                }
                else if (Waypoints[0].Type == AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY)
                {
                    Airstart = true;
                }
            }
        }

        #endregion

        #region Public properties

        public string AirGroupKey
        {
            get;
            set;
        }

        public int SquadronIndex
        {
            get
            {
                return squadronIndex;
            }
            set
            {
                squadronIndex = value;
            }
        }
        public int squadronIndex;

        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>> Flights
        {
            get
            {
                return flights;
            }
        }
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>> flights = new Dictionary<int, List<string>>();

        public string Class
        {
            get;
            set;
        }

        public string Formation
        {
            get;
            set;
        }

        public int CallSign;

        public int Fuel;

        public int[] Weapons
        {
            get;
            set;
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

        public double Speed
        {
            get
            {
                // TODO: Use aicraft info to determine speed
                return 300.0;
            }
        }

        public bool Airstart
        {
            get;
            set;
        }

        public bool SetOnParked
        {
            get;
            set;
        }

        public string Briefing
        {
            get;
            set;
        }

        public string Name
        {
            get
            {
                int flightMask = 0x0;

                foreach (int flightIndex in Flights.Keys)
                {
                    if (Flights[flightIndex].Count > 0)
                    {
                        int bit = (0x1 << flightIndex);
                        flightMask = (flightMask | bit);
                    }
                }

                return AirGroupKey + "." + SquadronIndex.ToString() + flightMask.ToString("X");
            }
        }

        public int ArmyIndex
        {
            get
            {
                if (AirGroupInfo.GetAirGroupInfo(1, AirGroupKey) != null)
                {
                    return 1;
                }
                else if (AirGroupInfo.GetAirGroupInfo(2, AirGroupKey) != null)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }

        public IAircraftInfo AircraftInfo
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
            if (Waypoints.Count > 0)
            {
                sectionFile.add("AirGroups", Name, "");

                foreach (int flightIndex in Flights.Keys)
                {
                    if (Flights[flightIndex].Count > 0)
                    {
                        string acNumberLine = "";
                        foreach (string acNumber in Flights[flightIndex])
                        {
                            acNumberLine += acNumber + " ";
                        }
                        sectionFile.add(Name, "Flight" + flightIndex, acNumberLine.TrimEnd());
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

                if (Core._spawnParked == true)
                {
                    sectionFile.add(Name, "SetOnPark", "1");
                }
                else
                {
                    sectionFile.add(Name, "SetOnPark", "0");
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

                if (Briefing != null)
                {
                    sectionFile.add(Name, "Briefing", Briefing);
                    
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
            if (!Airstart)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, Position, 0.0));
            }
            else
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
            }
        }

        private void createEndWaypoints(ISectionFile sectionFile, AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, landingAirport.Pos(), 0.0));
            }
            else
            {
                if (!Airstart)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, Position, 0.0));
                }
                else
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
                }
            }
        }

        private void createInbetweenWaypoints(ISectionFile sectionFile, Point3d a, Point3d b)
        {
            Point3d p1 = new Point3d(a.x + 0.25 * (b.x - a.x), a.y + 0.25 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));
            Point3d p2 = new Point3d(a.x + 0.50 * (b.x - a.x), a.y + 0.50 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));
            Point3d p3 = new Point3d(a.x + 0.75 * (b.x - a.x), a.y + 0.75 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p1, 300.0));
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p2, 300.0));
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p3, 300.0));
        }

        private void createStartInbetweenPoints(ISectionFile sectionFile, Point3d target)
        {
            createInbetweenWaypoints(sectionFile, Position, target);
        }

        private void createEndInbetweenPoints(ISectionFile sectionFile, Point3d target, AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                Point3d point = new Point3d(landingAirport.Pos().x, landingAirport.Pos().y, target.z);
                createInbetweenWaypoints(sectionFile, target, point);
            }
            else
            {
                Point3d point = new Point3d(Position.x, Position.y, target.z);
                createInbetweenWaypoints(sectionFile, target, point);
            }
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return AirGroupKey + "." + SquadronIndex;
        }

        public void CreateTransferFlight(ISectionFile sectionFile, AiAirport landingAirport = null)
        {
            Waypoints.Clear();

            createStartWaypoints(sectionFile);

            createEndInbetweenPoints(sectionFile, Position, landingAirport);

            createEndWaypoints(sectionFile, landingAirport);

            writeTo(sectionFile);
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
        }

        public void CreateGroundAttackTargetMission(ISectionFile sectionFile, GroundGroup groundGroup, double altitude, Point3d? rendevouzPosition = null, AiAirport landingAirport = null)
        {
            Waypoints.Clear();

            createStartWaypoints(sectionFile);

            if (rendevouzPosition == null)
            {
                Point3d pStart = new Point3d(groundGroup.Waypoints[0].Position.x, groundGroup.Waypoints[0].Position.y, altitude);
                createStartInbetweenPoints(sectionFile, pStart);
            }
            else
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                Point3d pStart = new Point3d(groundGroup.Waypoints[0].Position.x, groundGroup.Waypoints[0].Position.y, altitude);
                createInbetweenWaypoints(sectionFile, rendevouzPosition.Value, pStart);
            }
            
            foreach (GroundGroupWaypoint groundGroupWaypoint in groundGroup.Waypoints)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, groundGroupWaypoint.Position.x, groundGroupWaypoint.Position.y, altitude, 300.0, groundGroup.Id + " " + groundGroup.Waypoints.IndexOf(groundGroupWaypoint)));
            }

            Point3d pEnd = new Point3d(groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].Position.x, groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].Position.y, altitude);
            createEndInbetweenPoints(sectionFile, pEnd, landingAirport);
            createEndWaypoints(sectionFile, landingAirport);

            writeTo(sectionFile);
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
        }

        public void CreateInterceptFlight(ISectionFile sectionFile, AirGroup targetAirUnit, AiAirport landingAirport = null)
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

            createEndWaypoints(sectionFile, landingAirport);

            writeTo(sectionFile);
        }
        
        #endregion        
    }
}