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
    public class AirGroup : IUnit
    {
        #region Events

        public event UnitEventHandler Idle;

        public event UnitEventHandler Pending;

        public event UnitEventHandler Busy;

        public event UnitEventHandler Destroyed;

        public event UnitEventHandler Covered;

        public event UnitEventHandler Discovered;

        public void RaiseIdle()
        {
            this.state = UnitState.Idle;

            if (Idle != null)
            {
                Idle(this, new UnitEventArgs(this));
            }

            PersistentWorld.Debug(Id + " idle.");
        }

        public void RaisePending()
        {
            this.state = UnitState.Pending;

            if (Pending != null)
            {
                Pending(this, new UnitEventArgs(this));
            }

            PersistentWorld.Debug(Id + " pending.");
        }

        public void RaiseBusy()
        {
            this.state = UnitState.Busy;

            if (Busy != null)
            {
                Busy(this, new UnitEventArgs(this));
            }

            PersistentWorld.Debug(Id + " busy.");
        }

        public void RaiseDestroyed()
        {
            this.state = UnitState.Destroyed;

            if (Destroyed != null)
            {
                Destroyed(this, new UnitEventArgs(this));
            }

            PersistentWorld.Debug(Id + " destroyed.");
        }

        public void RaiseCovered()
        {
            if (Covered != null)
            {
                Covered(this, new UnitEventArgs(this));
            }

            //PersistentWorld.Debug(Id + " covered.");
        }

        public void RaiseDiscovered()
        {
            if (Discovered != null)
            {
                Discovered(this, new UnitEventArgs(this));
            }

            //PersistentWorld.Debug(Id + " discovered.");
        }

        #endregion
        
        #region Properties

        private IPersistentWorld PersistentWorld
        {
            get
            {
                return this.persistentWorld;
            }
        }
        private IPersistentWorld persistentWorld;

        public string Id
        {
            get
            {
                return this.id;
            }
        }
        private string id;

        public UnitState State
        {
            get
            {
                return this.state;
            }
        }
        public UnitState state;
        
        public Tuple<double, double, double> Position
        {
            get
            {
                Tuple<double, double, double> position = PersistentWorld.GetPositionOf(this);
                if (position != null)
                {
                    return position;
                }
                else
                {
                    return new Tuple<double,double,double>(Aerodrome.x, Aerodrome.y, Aerodrome.z);
                }
            }
        }

        public string Regiment
        {
            get
            {
                return this.regiment;
            }
        }
        private string regiment;

        public int SquadronIndex
        {
            get
            {
                return this.squadronIndex;
            }
        }
        private int squadronIndex;

        public int FlightIndex
        {
            get
            {
                return this.flightIndex;
            }
        }
        private int flightIndex;
        
        public string Class
        {
            get
            {
                return this.aircraftClass;
            }
        }
        private string aircraftClass;

        public string Formation
        {
            get
            {
                return this.formation;
            }
            set
            {
                this.formation = value;
            }
        }
        private string formation;

        public int CallSign
        {
            get
            {
                return this.callSign;
            }
        }
        private int callSign;


        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>> Flights
        {
            get
            {
                return this.flights;
            }
        }
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>> flights = new Dictionary<int, List<string>>();


        public int Fuel
        {
            get
            {
                return this.fuel;
            }
        }
        private int fuel;

        public int[] Weapons
        {
            get
            {
                return this.weapons;
            }
            set
            {
                this.weapons = value;
            }
        }
        private int[] weapons;

        public List<string> Detonator
        {
            get
            {
                return this.detonator;
            }
            set
            {
                this.detonator = value;
            }
        }
        private List<string> detonator = new List<string>();

        public AirGroupInfo AirGroupInfo
        {
            get
            {
                return IL2DCE.AirGroupInfo.GetAirGroupInfo(Regiment);
            }
        }

        public AircraftInfo AircraftInfo
        {
            get
            {
                return new AircraftInfo(PersistentWorld.AircraftInfoFile, Class);
            }
        }

        public Army Army
        {
            get
            {
                if (IL2DCE.AirGroupInfo.GetAirGroupInfo(1, Regiment) != null)
                {
                    return IL2DCE.Army.Red;
                }
                else if (IL2DCE.AirGroupInfo.GetAirGroupInfo(2, Regiment) != null)
                {
                    return IL2DCE.Army.Blue;
                }
                else
                {
                    return IL2DCE.Army.None;
                }
            }
        }

        public List<AirGroupWaypoint> Waypoints
        {
            get
            {
                return this.waypoints;
            }
        }
        private List<AirGroupWaypoint> waypoints = new List<AirGroupWaypoint>();

        public List<Aircraft> Aircrafts
        {
            get
            {
                return this.aircrafts;
            }
        }
        private List<Aircraft> aircrafts = new List<Aircraft>();

        #endregion

        public AirGroup(IPersistentWorld persistentWorld, string id, ISectionFile missionFile)
        {
            this.persistentWorld = persistentWorld;

            // Id
            this.id = id;

            // Regiment
            this.regiment = id.Substring(0, id.IndexOf("."));

            // SquadronIndex
            this.squadronIndex = int.Parse(id.Substring(id.IndexOf(".") + 1, 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            // FlightIndex
            this.flightIndex = int.Parse(id.Substring(id.IndexOf(".") + 2, 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            // Flight
            for (int i = 0; i < 4; i++)
            {
                if (missionFile.exist(id, "Flight" + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)))
                {
                    string acNumberLine = missionFile.get(id, "Flight" + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
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
            this.aircraftClass = missionFile.get(id, "Class");

            // Formation
            this.formation = missionFile.get(id, "Formation");

            // CallSign
            this.callSign = int.Parse(missionFile.get(id, "CallSign"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            // Fuel
            this.fuel = int.Parse(missionFile.get(id, "Fuel"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            // Weapons
            string weaponsLine = missionFile.get(id, "Weapons");
            string[] weaponsList = weaponsLine.Split(new char[] { ' ' });
            if (weaponsList != null && weaponsList.Length > 0)
            {
                Weapons = new int[weaponsList.Length];
                for (int i = 0; i < weaponsList.Length; i++)
                {
                    int.TryParse(weaponsList[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Weapons[i]);
                }
            }

            // Detonator
            for (int i = 0; i < missionFile.lines(id); i++)
            {
                string key;
                string value;
                missionFile.get(id, i, out key, out value);
                if (key == "Detonator")
                {
                    this.detonator.Add(value);
                }
            }

            // Belt
            // TODO: Parse belt

            // Skill
            // TODO: Parse skill

            // Waypoints
            for (int i = 0; i < missionFile.lines(Id + "_Way"); i++)
            {
                AirGroupWaypoint waypoint = new AirGroupWaypoint(missionFile, Id, i);
                Waypoints.Add(waypoint);
            }

            if (Waypoints.Count > 0)
            {
                this.aerodrome = new Point3d(Waypoints[0].X, Waypoints[0].Y, Waypoints[0].Z);
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

        public Point3d Aerodrome
        {
            get
            {
                return this.aerodrome;
            }            
        }
        private Point3d aerodrome;

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

        #region Private methods

        private double distanceBetween(AirGroupWaypoint start, AirGroupWaypoint end)
        {
            double distanceStart = distanceTo(start);
            double distanceEnd = distanceTo(end);
            return distanceEnd - distanceStart;
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

        private void createStartWaypoints()
        {
            if (!Airstart)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, Aerodrome, 0.0));
            }
            else
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Aerodrome, Speed));
            }
        }

        private void createEndWaypoints(AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, landingAirport.Pos(), 0.0));
            }
            else
            {
                if (!Airstart)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, Aerodrome, 0.0));
                }
                else
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Aerodrome, Speed));
                }
            }
        }

        private void createInbetweenWaypoints(Point3d a, Point3d b)
        {
            Point3d p1 = new Point3d(a.x + 0.25 * (b.x - a.x), a.y + 0.25 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));
            Point3d p2 = new Point3d(a.x + 0.50 * (b.x - a.x), a.y + 0.50 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));
            Point3d p3 = new Point3d(a.x + 0.75 * (b.x - a.x), a.y + 0.75 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p1, 300.0));
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p2, 300.0));
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p3, 300.0));
        }

        private void createStartInbetweenPoints(Point3d target)
        {
            createInbetweenWaypoints(Aerodrome, target);
        }

        private void createEndInbetweenPoints(Point3d target, AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                Point3d point = new Point3d(landingAirport.Pos().x, landingAirport.Pos().y, target.z);
                createInbetweenWaypoints(target, point);
            }
            else
            {
                Point3d point = new Point3d(Aerodrome.x, Aerodrome.y, target.z);
                createInbetweenWaypoints(target, point);
            }
        }

        private void reset()
        {
            Waypoints.Clear();

            this.missionType = null;
            this.altitude = null;
            this.EscortAirGroup = null;
            this.TargetAirGroup = null;
            this.TargetGroundGroup = null;
            this.TargetStationary = null;
            this.TargetArea = null;
        }

        #endregion

        #region Public methods

        public void WriteTo(ISectionFile sectionFile)
        {
            if (Waypoints.Count > 0)
            {
                sectionFile.add("AirGroups", Id, "");

                foreach (int flightIndex in Flights.Keys)
                {
                    if (Flights[flightIndex].Count > 0)
                    {
                        string acNumberLine = "";
                        foreach (string acNumber in Flights[flightIndex])
                        {
                            acNumberLine += acNumber + " ";
                        }
                        sectionFile.add(Id, "Flight" + flightIndex, acNumberLine.TrimEnd());
                    }
                }

                sectionFile.add(Id, "Class", Class);
                sectionFile.add(Id, "Formation", Formation);
                sectionFile.add(Id, "CallSign", CallSign.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                sectionFile.add(Id, "Fuel", Fuel.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

                if (Weapons != null && Weapons.Length > 0)
                {
                    string weaponsLine = "";
                    foreach (int weapon in Weapons)
                    {
                        weaponsLine += weapon.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " ";
                    }
                    sectionFile.add(Id, "Weapons", weaponsLine.TrimEnd());
                }

                if (Detonator != null && Detonator.Count > 0)
                {
                    foreach (string detonator in Detonator)
                    {
                        sectionFile.add(Id, "Detonator", detonator);
                    }
                }

                if (SetOnParked == true)
                {
                    sectionFile.add(Id, "SetOnPark", "1");
                }
                else
                {
                    sectionFile.add(Id, "Scramble", "1");
                }
                                
                sectionFile.add(Id, "Skill", "0.3 0.3 0.3 0.3 0.3 0.3 0.3 0.3");

                foreach (AirGroupWaypoint waypoint in Waypoints)
                {
                    if (waypoint.Target == null)
                    {
                        sectionFile.add(Id + "_Way", waypoint.Type.ToString(), waypoint.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                    }
                    else
                    {
                        sectionFile.add(Id + "_Way", waypoint.Type.ToString(), waypoint.X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Target);
                    }
                }

                sectionFile.add(Id, "Briefing", Id);

                //if (AircraftInfo.IsFlyable)
                //{
                //    string birthPlaceKey = "BirthPlace" + sectionFile.lines("BirthPlace").ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                //    string birthPlaceValue = ((int)Army).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Math.Round(Waypoints[0].X).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Math.Round(Waypoints[0].Y).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Math.Round(Waypoints[0].Z).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " ";
                //    birthPlaceValue += "12 1 1 ";
                //    if (Army == Army.Red && Regiment.StartsWith("BoB_RAF_"))
                //    {
                //        birthPlaceValue += "gb ";
                //        if (Regiment.StartsWith("BoB_RAF_F_") && Regiment.EndsWith("_Early"))
                //        {
                //            birthPlaceValue += "RAF_Fighter_1939 ";
                //        }
                //        else if (Regiment.StartsWith("BoB_RAF_F_") && Regiment.EndsWith("_Late"))
                //        {
                //            birthPlaceValue += "RAF_Fighter_1940 ";
                //        }
                //        else if (Regiment.StartsWith("BoB_RAF_B_"))
                //        {
                //            birthPlaceValue += "RAF_Bomber_1940 ";
                //        }
                //        birthPlaceValue += Regiment;
                //    }
                //    else if (Army == Army.Blue && Regiment.StartsWith("BoB_LW_"))
                //    {
                //        birthPlaceValue += "de . .";
                //        // TODO: Limit available regiments
                //    }

                //    sectionFile.add("BirthPlace", Id, birthPlaceValue);
                //    sectionFile.add(birthPlaceKey, Class, "");
                //}
            }
        }

        public override string ToString()
        {
            return Regiment + "." + SquadronIndex;
        }

        public void Transfer(EMissionType missionType, double altitude, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;

            createStartWaypoints();

            Point3d target = new Point3d(Aerodrome.x, Aerodrome.y, altitude);
            createEndInbetweenPoints(target, landingAirport);

            createEndWaypoints(landingAirport);
        }

        public void Cover(EMissionType missionType, GroundGroup targetGroundGroup, double altitude, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetGroundGroup = targetGroundGroup;

            if (targetGroundGroup.Waypoints.Count > 0)
            {
                createStartWaypoints();

                Point3d p = new Point3d(targetGroundGroup.Waypoints[0].Position.x, targetGroundGroup.Waypoints[0].Position.y, altitude);

                createStartInbetweenPoints(p);

                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, p.x, p.y, altitude, 300.0));
                AirGroupWaypoint start = Waypoints[Waypoints.Count - 1];

                while (distanceBetween(start, Waypoints[Waypoints.Count - 1]) < 200000.0)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, p.x + 5000.0, p.y + 5000.0, altitude, 300.0));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, p.x + 5000.0, p.y - 5000.0, altitude, 300.0));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, p.x - 5000.0, p.y - 5000.0, altitude, 300.0));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, p.x - 5000.0, p.y + 5000.0, altitude, 300.0));
                }

                createEndInbetweenPoints(p, landingAirport);

                createEndWaypoints(landingAirport);
            }
        }

        public void Cover(EMissionType missionType, Stationary targetStationary, double altitude, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetStationary = targetStationary;

            createStartWaypoints();

            Point3d p = new Point3d(targetStationary.Position.x, targetStationary.Position.y, altitude);
            createStartInbetweenPoints(p);

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetStationary.Position.x, targetStationary.Position.y, altitude, 300.0));
            AirGroupWaypoint start = Waypoints[Waypoints.Count - 1];

            while (distanceBetween(start, Waypoints[Waypoints.Count - 1]) < 200000.0)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetStationary.Position.x + 5000.0, targetStationary.Position.y + 5000.0, altitude, 300.0));
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetStationary.Position.x + 5000.0, targetStationary.Position.y - 5000.0, altitude, 300.0));
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetStationary.Position.x - 5000.0, targetStationary.Position.y - 5000.0, altitude, 300.0));
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetStationary.Position.x - 5000.0, targetStationary.Position.y + 5000.0, altitude, 300.0));
            }

            createEndInbetweenPoints(p, landingAirport);

            createEndWaypoints(landingAirport);
        }

        public void Cover(EMissionType missionType, Point2d targetArea, double altitude, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetArea = targetArea;

            createStartWaypoints();

            Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);
            createStartInbetweenPoints(p);

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetArea.x, targetArea.y, altitude, 300.0));
            AirGroupWaypoint start = Waypoints[Waypoints.Count - 1];

            while (distanceBetween(start, Waypoints[Waypoints.Count - 1]) < 200000.0)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetArea.x + 5000.0, targetArea.y + 5000.0, altitude, 300.0));
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetArea.x + 5000.0, targetArea.y - 5000.0, altitude, 300.0));
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetArea.x - 5000.0, targetArea.y - 5000.0, altitude, 300.0));
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, targetArea.x - 5000.0, targetArea.y + 5000.0, altitude, 300.0));
            }

            createEndInbetweenPoints(p, landingAirport);

            createEndWaypoints(landingAirport);
        }

        public void Hunting(EMissionType missionType, Point2d targetArea, double altitude, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetArea = targetArea;

            createStartWaypoints();

            Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);
            createStartInbetweenPoints(p);

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.HUNTING, targetArea.x, targetArea.y, altitude, 300.0));

            createEndInbetweenPoints(p, landingAirport);
            createEndWaypoints(landingAirport);
        }
        
        public void GroundAttack(EMissionType missionType, AiGroup aiGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.escortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (escortAirGroup != null)
            {
                rendevouzPosition = new Point3d(Aerodrome.x + 0.50 * (escortAirGroup.Aerodrome.x - Aerodrome.x), Aerodrome.y + 0.50 * (escortAirGroup.Aerodrome.y - Aerodrome.y), altitude);
            }

            createStartWaypoints();

            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                Point3d pStart = new Point3d(aiGroup.Pos().x, aiGroup.Pos().y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(aiGroup.Pos().x, aiGroup.Pos().y, altitude);
                createStartInbetweenPoints(pStart);
            }

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, aiGroup.Pos().x, aiGroup.Pos().y, altitude, 300.0));

            AiWayPoint[] aiWaypoints = aiGroup.GetWay();
            if (aiWaypoints != null && aiWaypoints.Length > 0)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, aiWaypoints[aiWaypoints.Length - 1].P.x, aiWaypoints[aiWaypoints.Length - 1].P.y, altitude, 300.0));
            }

            Point3d pEnd = new Point3d(aiGroup.Pos().x, aiGroup.Pos().y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);

            createEndWaypoints(landingAirport);
        }

        public void GroundAttack(EMissionType missionType, Point2d targetArea, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetArea = targetArea;
            this.escortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (escortAirGroup != null)
            {
                rendevouzPosition = new Point3d(Aerodrome.x + 0.50 * (escortAirGroup.Aerodrome.x - Aerodrome.x), Aerodrome.y + 0.50 * (escortAirGroup.Aerodrome.y - Aerodrome.y), altitude);
            }

            Waypoints.Clear();

            createStartWaypoints();

            Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);
            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                
                createInbetweenWaypoints(rendevouzPosition.Value, p);
            }
            else
            {
                createStartInbetweenPoints(p);
            }

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_POINT, targetArea.x, targetArea.y, altitude, 300.0));

            createEndInbetweenPoints(p, landingAirport);
            createEndWaypoints(landingAirport);
        }

        public void GroundAttack(EMissionType missionType, Stationary targetStationary, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetStationary = targetStationary;
            this.escortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (escortAirGroup != null)
            {
                rendevouzPosition = new Point3d(Aerodrome.x + 0.50 * (escortAirGroup.Aerodrome.x - Aerodrome.x), Aerodrome.y + 0.50 * (escortAirGroup.Aerodrome.y - Aerodrome.y), altitude);
            }
            
            createStartWaypoints();

            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart);
            }

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, targetStationary.X, targetStationary.Y, altitude, 300.0, targetStationary.Id));
            
            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);
            
            createEndWaypoints(landingAirport);
        }

        public void GroundAttack(EMissionType missionType, GroundGroup targetGroundGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            if (targetGroundGroup.AiGroup != null)
            {
                GroundAttack(missionType, targetGroundGroup.AiGroup, altitude, escortAirGroup, landingAirport);
            }
            else
            {
                this.reset();
                this.MissionType = missionType;
                this.Altitude = altitude;
                this.TargetGroundGroup = targetGroundGroup;
                this.escortAirGroup = escortAirGroup;

                Point3d? rendevouzPosition = null;
                if (escortAirGroup != null)
                {
                    rendevouzPosition = new Point3d(Aerodrome.x + 0.50 * (escortAirGroup.Aerodrome.x - Aerodrome.x), Aerodrome.y + 0.50 * (escortAirGroup.Aerodrome.y - Aerodrome.y), altitude);
                }

                if (targetGroundGroup.Waypoints.Count > 0)
                {
                    createStartWaypoints();

                    if (rendevouzPosition != null && rendevouzPosition.HasValue)
                    {
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                        Point3d pStart = new Point3d(targetGroundGroup.Waypoints[0].Position.x, targetGroundGroup.Waypoints[0].Position.y, altitude);
                        createInbetweenWaypoints(rendevouzPosition.Value, pStart);
                    }
                    else
                    {
                        Point3d pStart = new Point3d(targetGroundGroup.Waypoints[0].Position.x, targetGroundGroup.Waypoints[0].Position.y, altitude);
                        createStartInbetweenPoints(pStart);
                    }

                    GroundGroupWaypoint lastGroundGroupWaypoint = null;
                    AirGroupWaypoint start = null;
                    foreach (GroundGroupWaypoint groundGroupWaypoint in targetGroundGroup.Waypoints)
                    {
                        lastGroundGroupWaypoint = groundGroupWaypoint;
                        Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, groundGroupWaypoint.Position.x, groundGroupWaypoint.Position.y, altitude, 300.0, targetGroundGroup.Id + " " + targetGroundGroup.Waypoints.IndexOf(groundGroupWaypoint)));
                        if (start == null)
                        {
                            start = Waypoints[Waypoints.Count - 1];
                        }
                        else
                        {
                            if (distanceBetween(start, Waypoints[Waypoints.Count - 1]) > 20000.0)
                            {
                                break;
                            }
                        }
                    }

                    if (lastGroundGroupWaypoint != null)
                    {
                        Point3d pEnd = new Point3d(lastGroundGroupWaypoint.Position.x, lastGroundGroupWaypoint.Position.y, altitude);
                        createEndInbetweenPoints(pEnd, landingAirport);
                    }

                    createEndWaypoints(landingAirport);
                }
            }
        }

        public void Recon(EMissionType missionType, Point2d targetArea, double altitude, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetArea = targetArea;

            Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);

            createStartWaypoints();
            createStartInbetweenPoints(p);

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, targetArea.x, targetArea.y, altitude, 300.0));

            createEndInbetweenPoints(p, landingAirport);
            createEndWaypoints(landingAirport);
        }

        public void Recon(EMissionType missionType, GroundGroup targetGroundGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = altitude;
            this.TargetGroundGroup = targetGroundGroup;
            this.escortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (escortAirGroup != null)
            {
                rendevouzPosition = new Point3d(Aerodrome.x + 0.50 * (escortAirGroup.Aerodrome.x - Aerodrome.x), Aerodrome.y + 0.50 * (escortAirGroup.Aerodrome.y - Aerodrome.y), altitude);
            }

            Waypoints.Clear();

            if (targetGroundGroup.Waypoints.Count > 0)
            {
                createStartWaypoints();

                if (rendevouzPosition != null && rendevouzPosition.HasValue)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                    Point3d pStart = new Point3d(targetGroundGroup.Waypoints[0].Position.x, targetGroundGroup.Waypoints[0].Position.y, altitude);
                    createInbetweenWaypoints(rendevouzPosition.Value, pStart);                    
                }
                else
                {
                    Point3d pStart = new Point3d(targetGroundGroup.Waypoints[0].Position.x, targetGroundGroup.Waypoints[0].Position.y, altitude);
                    createStartInbetweenPoints(pStart);
                }

                GroundGroupWaypoint lastGroundGroupWaypoint = null;
                AirGroupWaypoint start = null;
                foreach (GroundGroupWaypoint groundGroupWaypoint in targetGroundGroup.Waypoints)
                {
                    lastGroundGroupWaypoint = groundGroupWaypoint;
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, groundGroupWaypoint.Position.x, groundGroupWaypoint.Position.y, altitude, 300.0, targetGroundGroup.Id + " " + targetGroundGroup.Waypoints.IndexOf(groundGroupWaypoint)));
                    if (start == null)
                    {
                        start = Waypoints[Waypoints.Count - 1];
                    }
                    else
                    {
                        if (distanceBetween(start, Waypoints[Waypoints.Count - 1]) > 20000.0)
                        {
                            break;
                        }
                    }
                }

                if (lastGroundGroupWaypoint != null)
                {
                    Point3d pEnd = new Point3d(lastGroundGroupWaypoint.Position.x, lastGroundGroupWaypoint.Position.y, altitude);
                    createEndInbetweenPoints(pEnd, landingAirport);
                }

                createEndWaypoints(landingAirport);
            }
        }

        public void Escort(EMissionType missionType, AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = targetAirGroup.Altitude;
            this.TargetAirGroup = targetAirGroup;

            createStartWaypoints();

            foreach (AirGroupWaypoint waypoint in targetAirGroup.Waypoints)
            {
                if (waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.ESCORT, waypoint.X, waypoint.Y, waypoint.Z, 300.0, targetAirGroup.Id + " " + targetAirGroup.Waypoints.IndexOf(waypoint)));
                }
            }

            createEndWaypoints(landingAirport);
        }

        public void Intercept(EMissionType missionType, AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            this.reset();
            this.MissionType = missionType;
            this.Altitude = targetAirGroup.Altitude;
            this.TargetAirGroup = targetAirGroup;

            createStartWaypoints();

            AirGroupWaypoint interceptWaypoint = null;
            AirGroupWaypoint closestInterceptWaypoint = null;
            foreach (AirGroupWaypoint waypoint in targetAirGroup.Waypoints)
            {
                Point3d p = Aerodrome;
                if (targetAirGroup.distanceTo(waypoint) > waypoint.Position.distance(ref p))
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
                        if (targetAirGroup.distanceTo(waypoint) < closestInterceptWaypoint.Position.distance(ref p))
                        {
                            closestInterceptWaypoint = waypoint;
                        }
                    }
                }
            }

            if (interceptWaypoint != null)
            {
                createStartInbetweenPoints(interceptWaypoint.Position);
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, interceptWaypoint.X, interceptWaypoint.Y, interceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetAirGroup.Waypoints.IndexOf(interceptWaypoint)));


                if (targetAirGroup.Waypoints.IndexOf(interceptWaypoint) - 1 >= 0)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetAirGroup.Waypoints[targetAirGroup.Waypoints.IndexOf(interceptWaypoint) - 1];
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetAirGroup.Waypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport);
                }
                else
                {
                    createEndInbetweenPoints(interceptWaypoint.Position, landingAirport);
                }
            }
            else if (closestInterceptWaypoint != null)
            {
                createStartInbetweenPoints(closestInterceptWaypoint.Position);
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, closestInterceptWaypoint.X, closestInterceptWaypoint.Y, closestInterceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetAirGroup.Waypoints.IndexOf(closestInterceptWaypoint)));


                if (targetAirGroup.Waypoints.IndexOf(closestInterceptWaypoint) + 1 < targetAirGroup.Waypoints.Count)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetAirGroup.Waypoints[targetAirGroup.Waypoints.IndexOf(interceptWaypoint) + 1];
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetAirGroup.Waypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport);
                }
                else
                {
                    createEndInbetweenPoints(closestInterceptWaypoint.Position, landingAirport);
                }
            }

            createEndWaypoints(landingAirport);
        }
        
        #endregion        

        public EMissionType? MissionType
        {
            get
            {
                return this.missionType;
            }
            set
            {
                this.missionType = value;
            }
        }
        private EMissionType? missionType = null;

        public double? Altitude
        {
            get
            {
                return this.altitude;
            }
            set
            {
                this.altitude = value;
            }
        }
        private double? altitude = null;

        public AirGroup EscortAirGroup
        {
            get
            {
                return this.escortAirGroup;
            }
            set
            {
                this.escortAirGroup = value;
            }
        }
        private AirGroup escortAirGroup = null;

        public Stationary TargetStationary
        {
            get
            {
                return this.targetStationary;
            }
            set
            {
                this.targetStationary = value;
            }
        }
        private Stationary targetStationary = null;

        public GroundGroup TargetGroundGroup
        {
            get
            {
                return this.targetGroundGroup;
            }
            set
            {
                this.targetGroundGroup = value;
            }
        }
        private GroundGroup targetGroundGroup = null;

        public AirGroup TargetAirGroup
        {
            get
            {
                return this.targetAirGroup;
            }
            set
            {
                this.targetAirGroup = value;
            }
        }
        private AirGroup targetAirGroup = null;

        public Point2d? TargetArea
        {
            get
            {
                return this.targetArea;
            }
            set
            {
                this.targetArea = value;
            }
        }
        private Point2d? targetArea = null;
    }
}