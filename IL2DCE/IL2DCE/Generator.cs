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
using maddox.game.play;
using maddox.game.page;
using maddox.GP;

namespace IL2DCE
{
    public class Generator : IGenerator
    {
        private Random rand = new Random();

        private List<AirGroup> availableAirGroups = new List<AirGroup>();
        private List<AirGroup> operatingAirGroups = new List<AirGroup>();

        private List<GroundGroup> availableGroundGroups = new List<GroundGroup>();
        private List<GroundGroup> operatingGroundGroups = new List<GroundGroup>();
        
        private List<Stationary> redStationaries = new List<Stationary>();
        private List<Stationary> blueStationaries = new List<Stationary>();

        private List<Point2d> factories = new List<Point2d>();

        private ICore Core
        {
            get
            {
                return this.core;
            }
        }
        private ICore core;


        public Generator(ICore core)
        {
            this.core = core;
        }

        public System.Collections.Generic.List<FrontMarker> RedFrontMarkers
        {
            get
            {
                List<FrontMarker> redFrontMarkers = new List<FrontMarker>();
                foreach (FrontMarker frontMarker in FrontMarkers)
                {
                    if (frontMarker.Army == 1)
                    {
                        redFrontMarkers.Add(frontMarker);
                    }
                }
                return redFrontMarkers;
            }
        }

        public System.Collections.Generic.List<FrontMarker> BlueFrontMarkers
        {
            get
            {
                List<FrontMarker> blueFrontMarkers = new List<FrontMarker>();
                foreach (FrontMarker frontMarker in FrontMarkers)
                {
                    if (frontMarker.Army == 2)
                    {
                        blueFrontMarkers.Add(frontMarker);
                    }
                }
                return blueFrontMarkers;
            }
        }

        public System.Collections.Generic.List<FrontMarker> FrontMarkers
        {
            get
            {
                return this.frontMarkers;
            }
        }
        private List<FrontMarker> frontMarkers = new List<FrontMarker>();

        public System.Collections.Generic.IList<IAirGroup> AirGroups
        {
            get
            {
                List<IAirGroup> airGroups = new List<IAirGroup>();
                airGroups.AddRange(redAirGroups);
                airGroups.AddRange(blueAirGroups);
                return airGroups;
            }
        }

        public System.Collections.Generic.IList<IAirGroup> RedAirGroups
        {
            get
            {
                List<IAirGroup> airGroups = new List<IAirGroup>();
                airGroups.AddRange(redAirGroups);
                return airGroups;
            }
        }

        public System.Collections.Generic.IList<IAirGroup> BlueAirGroups
        {
            get
            {
                List<IAirGroup> airGroups = new List<IAirGroup>();
                airGroups.AddRange(blueAirGroups);
                return airGroups;
            }
        }

        private List<AirGroup> redAirGroups = new List<AirGroup>();
        private List<AirGroup> blueAirGroups = new List<AirGroup>();

        public System.Collections.Generic.IList<IGroundGroup> GroundGroups
        {
            get
            {
                List<IGroundGroup> groundGroups = new List<IGroundGroup>();
                groundGroups.AddRange(redGroundGroups);
                groundGroups.AddRange(blueGroundGroups);
                return groundGroups;
            }
        }

        public System.Collections.Generic.IList<IGroundGroup> RedGroundGroups
        {
            get
            {
                List<IGroundGroup> groundGroups = new List<IGroundGroup>();
                groundGroups.AddRange(redGroundGroups);
                return groundGroups;
            }
        }

        public System.Collections.Generic.IList<IGroundGroup> BlueGroundGroups
        {
            get
            {
                List<IGroundGroup> groundGroups = new List<IGroundGroup>();
                groundGroups.AddRange(blueGroundGroups);
                return groundGroups;
            }
        }

        private List<GroundGroup> redGroundGroups = new List<GroundGroup>();
        private List<GroundGroup> blueGroundGroups = new List<GroundGroup>();

        private void findPath(GroundGroup groundGroup, Point2d start, Point2d end)
        {
            IRecalcPathParams pathParams = null;
            if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
            {
                pathParams = this.Core.GamePlay.gpFindPath(start, 10.0, end, 20.0, PathType.GROUND, groundGroup.Army);
            }
            else if (groundGroup.Type == EGroundGroupType.Ship)
            {
                pathParams = this.Core.GamePlay.gpFindPath(start, 10.0, end, 20.0, PathType.WATER, groundGroup.Army);
            }

            if (pathParams != null)
            {
                while (pathParams.State == RecalcPathState.WAIT)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Wait for path.", null);
                    System.Threading.Thread.Sleep(100);
                }

                if (pathParams.State == RecalcPathState.SUCCESS)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path found (" + pathParams.Path.Length.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ").", null);

                    GroundGroupWaypoint lastGroundGroupWaypoint = null;
                    foreach (maddox.game.world.AiWayPoint aiWayPoint in pathParams.Path)
                    {
                        if (aiWayPoint is maddox.game.world.AiGroundWayPoint)
                        {
                            maddox.game.world.AiGroundWayPoint aiGroundWayPoint = aiWayPoint as maddox.game.world.AiGroundWayPoint;

                            if (aiGroundWayPoint.P.z == -1)
                            {
                                GroundGroupWaypoint groundGroupWaypoint = new GroundGroupWaypoint(aiGroundWayPoint.P.x, aiGroundWayPoint.P.y, aiGroundWayPoint.roadWidth, aiGroundWayPoint.Speed);
                                lastGroundGroupWaypoint = groundGroupWaypoint;
                                groundGroup.Waypoints.Add(groundGroupWaypoint);
                            }
                            else if (lastGroundGroupWaypoint != null)
                            {
                                string s = aiGroundWayPoint.P.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + aiGroundWayPoint.P.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + aiGroundWayPoint.P.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + aiGroundWayPoint.roadWidth.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                                GroundGroupSubWaypoint groundGroupSubWaypoint = new GroundGroupSubWaypoint(s, null);
                                lastGroundGroupWaypoint.SubWaypoints.Add(groundGroupSubWaypoint);
                            }
                        }
                    }
                }
                else if (pathParams.State == RecalcPathState.FAILED)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path not found.", null);
                }
            }
        }

        private void findRoad(GroundGroup groundGroup, Point2d start, Point2d end, IList<Waterway> roads)
        {
            if (roads != null && roads.Count > 0)
            {
                Waterway closestRoad = null;
                double closestRoadDistance = 0.0;
                foreach (Waterway road in roads)
                {
                    if (road.Start != null && road.End != null)
                    {
                        Point2d roadStart = new Point2d(road.Start.Position.x, road.Start.Position.y);
                        double distanceStart = start.distance(ref roadStart);
                        Point2d roadEnd = new Point2d(road.End.Position.x, road.End.Position.y);
                        double distanceEnd = end.distance(ref roadEnd);

                        Point2d p = new Point2d(end.x, end.y);
                        if (distanceEnd < start.distance(ref p))
                        {
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
                }

                if (closestRoad != null)
                {
                    //findPath(groundGroup, start, new Point2d(closestRoad.Start.X, closestRoad.Start.Y));

                    groundGroup.Waypoints.AddRange(closestRoad.Waypoints);

                    List<Waterway> availableRoads = new List<Waterway>(roads);
                    availableRoads.Remove(closestRoad);

                    findRoad(groundGroup, new Point2d(closestRoad.End.Position.x, closestRoad.End.Position.y), end, availableRoads);
                }
            }
        }

        private void createRandomGroundOperation(ISectionFile missionFile, GroundGroup groundGroup)
        {
            return;

            availableGroundGroups.Remove(groundGroup);

            List<FrontMarker> friendlyMarkers = getFriendlyMarkers(groundGroup.Army);
            if (friendlyMarkers.Count > 0)
            {
                List<FrontMarker> availableFriendlyMarkers = new List<FrontMarker>(friendlyMarkers);

                // Find closest friendly marker
                FrontMarker closestMarker = null;
                foreach (FrontMarker marker in availableFriendlyMarkers)
                {
                    if (closestMarker == null)
                    {
                        closestMarker = marker;
                    }
                    else if (closestMarker != null)
                    {
                        Point3d p1 = new Point3d(marker.Position.x, marker.Position.y, 0.0);
                        Point3d p2 = new Point3d(closestMarker.Position.x, closestMarker.Position.y, 0.0);
                        if (groundGroup.Position.distance(ref p1) < groundGroup.Position.distance(ref p2))
                        {
                            closestMarker = marker;
                        }
                    }
                }

                if (closestMarker != null)
                {
                    availableFriendlyMarkers.Remove(closestMarker);

                    if (availableFriendlyMarkers.Count > 0)
                    {
                        int markerIndex = rand.Next(availableFriendlyMarkers.Count);

                        groundGroup.Waypoints.Clear();

                        Point2d start = new Point2d(groundGroup.Position.x, groundGroup.Position.y);
                        Point2d end = new Point2d(availableFriendlyMarkers[markerIndex].Position.x, availableFriendlyMarkers[markerIndex].Position.y);

                        if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
                        {
                            findPath(groundGroup, new Point2d(closestMarker.Position.x, closestMarker.Position.y), new Point2d(availableFriendlyMarkers[markerIndex].Position.x, availableFriendlyMarkers[markerIndex].Position.y));
                            groundGroup.writeTo(missionFile);
                        }
                        else
                        {
                            findPath(groundGroup, start, end);

                            groundGroup.writeTo(missionFile);

                            string groundGroupId = groundGroup.Id;
                            for (int i = 1; i < 3; i++)
                            {
                                double xOffset = -1.0;
                                double yOffset = -1.0;

                                bool subWaypointUsed = false;
                                Point2d p1 = new Point2d(groundGroup.Waypoints[0].X, groundGroup.Waypoints[0].Y);
                                if (groundGroup.Waypoints[0].SubWaypoints.Count > 0)
                                {
                                    foreach (GroundGroupSubWaypoint subWaypoint in groundGroup.Waypoints[0].SubWaypoints)
                                    {
                                        if (subWaypoint.P.HasValue)
                                        {
                                            Point2d p2 = new Point2d(subWaypoint.P.Value.x, subWaypoint.P.Value.y);
                                            double distance = p1.distance(ref p2);
                                            xOffset = 500 * ((p2.x - p1.x) / distance);
                                            yOffset = 500 * ((p2.y - p1.y) / distance);
                                            subWaypointUsed = true;
                                            break;
                                        }
                                    }
                                }
                                if (subWaypointUsed == false)
                                {
                                    Point2d p2 = new Point2d(groundGroup.Waypoints[1].X, groundGroup.Waypoints[1].Y);
                                    double distance = p1.distance(ref p2);
                                    xOffset = 500 * ((p2.x - p1.x) / distance);
                                    yOffset = 500 * ((p2.y - p1.y) / distance);
                                }

                                groundGroup.Waypoints[0].X += xOffset;
                                groundGroup.Waypoints[0].Y += yOffset;

                                subWaypointUsed = false;
                                p1 = new Point2d(groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].X, groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].Y);
                                if (groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].SubWaypoints.Count > 0)
                                {
                                    for (int j = groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].SubWaypoints.Count - 1; j >= 0; j--)
                                    {
                                        GroundGroupSubWaypoint subWaypoint = groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].SubWaypoints[j];
                                        if (subWaypoint.P.HasValue)
                                        {

                                            Point2d p2 = new Point2d(subWaypoint.P.Value.x, subWaypoint.P.Value.y);
                                            double distance = p1.distance(ref p2);
                                            xOffset = 500 * ((p2.x - p1.x) / distance);
                                            yOffset = 500 * ((p2.y - p1.y) / distance);
                                            subWaypointUsed = true;
                                            break;
                                        }
                                    }
                                }
                                if (subWaypointUsed == false)
                                {
                                    Point2d p2 = new Point2d(groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].X, groundGroup.Waypoints[groundGroup.Waypoints.Count - 2].Y);
                                    double distance = p1.distance(ref p2);
                                    xOffset = 500 * ((p2.x - p1.x) / distance);
                                    yOffset = 500 * ((p2.y - p1.y) / distance);
                                }

                                groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].X -= xOffset;
                                groundGroup.Waypoints[groundGroup.Waypoints.Count - 1].Y -= yOffset;

                                groundGroup._id = groundGroupId + "." + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                                groundGroup.writeTo(missionFile);
                            }
                        }
                    }
                }
            }
        }

        private List<GroundGroup> getGroundGroups(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redGroundGroups;
            }
            else if (armyIndex == 2)
            {
                return blueGroundGroups;
            }
            else
            {
                return new List<GroundGroup>();
            }
        }

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

        private List<Stationary> getFriendlyRadars(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return redStationaries;
            }
            else if (armyIndex == 2)
            {
                return blueStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        private List<Stationary> getEnemyRadars(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return blueStationaries;
            }
            else if (armyIndex == 2)
            {
                return redStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        private List<FrontMarker> getFriendlyMarkers(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return RedFrontMarkers;
            }
            else if (armyIndex == 2)
            {
                return BlueFrontMarkers;
            }
            else
            {
                return new List<FrontMarker>();
            }
        }

        private List<FrontMarker> getEnemyMarkers(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return BlueFrontMarkers;
            }
            else if (armyIndex == 2)
            {
                return RedFrontMarkers;
            }
            else
            {
                return new List<FrontMarker>();
            }
        }

        private List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in operatingGroundGroups)
            {
                if (groundGroup.Army != armyIndex)
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        private List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in operatingGroundGroups)
            {
                if (groundGroup.Army == armyIndex)
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        private List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in getAvailableEnemyGroundGroups(armyIndex))
            {
                if (groundGroupTypes.Contains(groundGroup.Type))
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        private List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (GroundGroup groundGroup in getAvailableFriendlyGroundGroups(armyIndex))
            {
                if (groundGroupTypes.Contains(groundGroup.Type))
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        public double GetRandomAltitude(IAircraftParametersInfo missionParameters)
        {
            if (missionParameters.MinAltitude != null && missionParameters.MinAltitude.HasValue && missionParameters.MaxAltitude != null && missionParameters.MaxAltitude.HasValue)
            {
                return (double)rand.Next((int)missionParameters.MinAltitude.Value, (int)missionParameters.MaxAltitude.Value);
            }
            else
            {
                // Use some default altitudes
                return (double)rand.Next(300, 7000);
            }
        }

        private void getRandomFlightSize(IAirGroup airGroup, EMissionType missionType)
        {
            airGroup.Flights.Clear();
            int aircraftNumber = 1;

            int flightCount = (int)Math.Ceiling(airGroup.AirGroupInfo.FlightCount * this.Core.FlightCount);
            int flightSize = (int)Math.Ceiling(airGroup.AirGroupInfo.FlightSize * this.Core.FlightSize);

            if (missionType == EMissionType.RECON || missionType == EMissionType.MARITIME_RECON)
            {
                for (int i = 0; i < 1; i++)
                {
                    List<string> aircraftNumbers = new List<string>();
                    for (int j = 0; j < 1; j++)
                    {
                        aircraftNumbers.Add(aircraftNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        aircraftNumber++;
                    }
                    airGroup.Flights[i] = aircraftNumbers;
                }
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                for (int i = 0; i < 1; i++)
                {
                    List<string> aircraftNumbers = new List<string>();
                    for (int j = 0; j < flightSize; j++)
                    {
                        aircraftNumbers.Add(aircraftNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        aircraftNumber++;
                    }
                    airGroup.Flights[i] = aircraftNumbers;
                }
            }
            else if (missionType == EMissionType.ESCORT || missionType == EMissionType.INTERCEPT)
            {
                if (airGroup.TargetAirGroup != null)
                {
                    if (airGroup.TargetAirGroup.Flights.Count < flightCount)
                    {
                        flightCount = airGroup.TargetAirGroup.Flights.Count;
                    }

                    for (int i = 0; i < flightCount; i++)
                    {
                        List<string> aircraftNumbers = new List<string>();
                        for (int j = 0; j < flightSize; j++)
                        {
                            aircraftNumbers.Add(aircraftNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                            aircraftNumber++;
                        }
                        airGroup.Flights[i] = aircraftNumbers;
                    }
                }
            }
            else
            {
                for (int i = 0; i < flightCount; i++)
                {
                    List<string> aircraftNumbers = new List<string>();
                    for (int j = 0; j < flightSize; j++)
                    {
                        aircraftNumbers.Add(aircraftNumber.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                        aircraftNumber++;
                    }
                    airGroup.Flights[i] = aircraftNumbers;
                }
            }
        }

        private List<AirGroup> getAvailableOffensiveAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex != armyIndex)
                {
                    foreach (EMissionType missionType in airGroup.AircraftInfo.MissionTypes)
                    {
                        if (isMissionTypeOffensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private AirGroup getAvailableRandomOffensiveAirGroup(int armyIndex)
        {
            List<AirGroup> airGroups = getAvailableOffensiveAirGroups(armyIndex);

            if (airGroups.Count > 0)
            {
                int interceptedAirGroupIndex = rand.Next(airGroups.Count);
                AirGroup interceptedAirGroup = airGroups[interceptedAirGroupIndex];

                return interceptedAirGroup;
            }
            else
            {
                return null;
            }
        }

        private List<AirGroup> getAvailableEscortedAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex == armyIndex)
                {
                    foreach (EMissionType missionType in airGroup.AircraftInfo.MissionTypes)
                    {
                        if (isMissionTypeEscorted(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private AirGroup getAvailableRandomEscortedAirGroup(int armyIndex)
        {
            List<AirGroup> airGroups = getAvailableEscortedAirGroups(armyIndex);
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

        private AirGroup getAvailableRandomEscortAirGroup(AirGroup targetAirUnit)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex == targetAirUnit.ArmyIndex)
                {
                    if (airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.ESCORT))
                    {
                        airGroups.Add(airGroup);
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

        private GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex)
        {
            List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex, groundGroupTypes);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomFriendlyGroundGroup(int armyIndex)
        {
            List<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(armyIndex);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private GroundGroup getAvailableRandomFriendlyGroundGroup(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        {
            List<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(armyIndex, groundGroupTypes);
            if (groundGroups.Count > 0)
            {
                int groundGroupIndex = rand.Next(groundGroups.Count);
                GroundGroup targetGroundGroup = groundGroups[groundGroupIndex];

                return targetGroundGroup;
            }
            else
            {
                return null;
            }
        }

        private AirGroup getAvailableRandomInterceptAirGroup(AirGroup targetAirUnit)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in availableAirGroups)
            {
                if (airGroup.ArmyIndex != targetAirUnit.ArmyIndex)
                {
                    if (airGroup.AircraftInfo.MissionTypes.Contains(EMissionType.INTERCEPT))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

            if (airGroups.Count > 0)
            {
                int interceptAirGroupIndex = rand.Next(airGroups.Count);
                AirGroup interceptAirGroup = airGroups[interceptAirGroupIndex];

                return interceptAirGroup;
            }
            else
            {
                return null;
            }
        }

        private bool isMissionTypeEscorted(EMissionType missionType)
        {
            if (missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isMissionTypeOffensive(EMissionType missionType)
        {
            if (missionType == EMissionType.ARMED_MARITIME_RECON
                || missionType == EMissionType.ARMED_RECON
                || missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.MARITIME_RECON
                || missionType == EMissionType.RECON)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isMissionTypeAvailable(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.COVER)
            {
                List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ARMED_RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                List<Stationary> radars = getEnemyRadars(airGroup.ArmyIndex);
                if (radars.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_SHIP)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ESCORT)
            {
                List<AirGroup> airGroups = getAvailableEscortedAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.INTERCEPT)
            {
                List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.MARITIME_RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.RECON)
            {
                List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void createBriefing(IBriefingFile briefingFile, AirGroup airGroup, AirGroup escortAirGroup)
        {
            briefingFile.Name[airGroup.Id] = airGroup.Id;
            briefingFile.Description[airGroup.Id] = new BriefingFile.Text();

            briefingFile.Description[airGroup.Id].Sections.Add("descriptionSection", briefingFile.MissionDescription);

            if (airGroup.MissionType != null && airGroup.MissionType.HasValue)
            {
                EMissionType missionType = airGroup.MissionType.Value;
                string mainSection = missionType.ToString();
                briefingFile.Description[airGroup.Id].Sections.Add("mainSection", mainSection);
            }

            if (airGroup.Altitude != null && airGroup.Altitude.HasValue)
            {
                string altitudeSection = "Altitude: " + airGroup.Altitude + "m";

                briefingFile.Description[airGroup.Id].Sections.Add("altitudeSection", altitudeSection);
            }

            if (airGroup.Waypoints.Count > 0)
            {
                string waypointSection = "Waypoints:\n";

                int i = 1;
                foreach (AirGroupWaypoint waypoint in airGroup.Waypoints)
                {
                    if (waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY)
                    {
                        waypointSection += i + ". " + this.Core.GamePlay.gpSectorName(waypoint.X, waypoint.Y) + "\n";
                    }
                    else
                    {
                        waypointSection += i + ". " + this.Core.GamePlay.gpSectorName(waypoint.X, waypoint.Y) + " " + waypoint.Type.ToString() + "\n";
                    }
                    i++;
                }

                briefingFile.Description[airGroup.Id].Sections.Add("waypointSection", waypointSection);
            }

            if (airGroup.EscortAirGroup != null)
            {
                string escortSection = "Escorted by: " + escortAirGroup.Id;

                briefingFile.Description[airGroup.Id].Sections.Add("escortSection", escortSection);
            }
        }

        void checkPendingAirGroups()
        {
            List<AirGroup> airGroupsToRemove = new List<AirGroup>();
            foreach (AirGroup airGroup in operatingAirGroups)
            {
                if (isAvailable(airGroup))
                {
                    airGroupsToRemove.Add(airGroup);
                }
            }

            foreach (AirGroup airGroup in airGroupsToRemove)
            {
                availableAirGroups.Add(airGroup);
                operatingAirGroups.Remove(airGroup);
                this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, airGroup.Id + " is available.", null);
            }
        }
                
        bool isAvailable(AirGroup airGroup)
        {
            List<string> aiAirGroupNames = new List<string>();
            if (this.Core.GamePlay.gpAirGroups(airGroup.ArmyIndex) != null && this.Core.GamePlay.gpAirGroups(airGroup.ArmyIndex).Length > 0)
            {
                foreach (maddox.game.world.AiAirGroup aiAirGroup in this.Core.GamePlay.gpAirGroups(airGroup.ArmyIndex))
                {
                    string aiAirGroupName = aiAirGroup.Name();
                    aiAirGroupName = aiAirGroupName.Remove(0, aiAirGroupName.IndexOf(":") + 1);
                    aiAirGroupNames.Add(aiAirGroupName);
                }
            }

            if (aiAirGroupNames.Contains(airGroup.Id))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        private void createAirOperation(ISectionFile sectionFile, IBriefingFile briefingFile, AirGroup airGroup, EMissionType missionType, bool allowIntercept, AirGroup forcedEscortAirGroup)
        {
            if (isMissionTypeAvailable(airGroup, missionType))
            {
                availableAirGroups.Remove(airGroup);
                operatingAirGroups.Add(airGroup);

                this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, airGroup.Id + " is operating.", null);

                List<IAircraftParametersInfo> aircraftParametersInfos = airGroup.AircraftInfo.GetAircraftParametersInfo(missionType);
                int aircraftParametersInfoIndex = rand.Next(aircraftParametersInfos.Count);
                IAircraftParametersInfo randomAircraftParametersInfo = aircraftParametersInfos[aircraftParametersInfoIndex];
                IAircraftLoadoutInfo aircraftLoadoutInfo = airGroup.AircraftInfo.GetAircraftLoadoutInfo(randomAircraftParametersInfo.LoadoutId);
                airGroup.Weapons = aircraftLoadoutInfo.Weapons;
                airGroup.Detonator = aircraftLoadoutInfo.Detonator;

                AirGroup escortAirGroup = null;
                if (isMissionTypeEscorted(missionType))
                {
                    if (forcedEscortAirGroup == null)
                    {
                        escortAirGroup = getAvailableRandomEscortAirGroup(airGroup);
                    }
                    else
                    {
                        escortAirGroup = forcedEscortAirGroup;
                    }
                }

                if (missionType == EMissionType.COVER)
                {
                    AirGroup offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup.ArmyIndex);
                    if (offensiveAirGroup != null)
                    {
                        List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in offensiveAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(offensiveAirGroup, targetMissionType) && isMissionTypeOffensive(targetMissionType))
                            {
                                availableOffensiveMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableOffensiveMissionTypes.Count > 0)
                        {
                            int offensiveMissionTypeIndex = rand.Next(availableOffensiveMissionTypes.Count);
                            EMissionType randomOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];
                            createAirOperation(sectionFile, briefingFile, offensiveAirGroup, randomOffensiveMissionType, false, null);

                            if (offensiveAirGroup.Altitude != null && offensiveAirGroup.Altitude.HasValue && offensiveAirGroup.TargetGroundGroup != null)
                            {
                                airGroup.Cover(missionType, offensiveAirGroup.TargetGroundGroup, offensiveAirGroup.Altitude.Value);
                            }
                            else if (offensiveAirGroup.Altitude != null && offensiveAirGroup.Altitude.HasValue && offensiveAirGroup.TargetStationary != null)
                            {
                                airGroup.Cover(missionType, offensiveAirGroup.TargetStationary, offensiveAirGroup.Altitude.Value);
                            }
                            else if (offensiveAirGroup.Altitude != null && offensiveAirGroup.Altitude.HasValue && offensiveAirGroup.TargetArea != null && offensiveAirGroup.TargetArea.HasValue)
                            {
                                airGroup.Cover(missionType, offensiveAirGroup.TargetArea.Value, offensiveAirGroup.Altitude.Value);
                            }
                        }
                    }
                }
                else if (missionType == EMissionType.ARMED_MARITIME_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
                }
                else if (missionType == EMissionType.ARMED_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_ARMOR)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_RADAR)
                {
                    List<Stationary> radars = getEnemyRadars(airGroup.ArmyIndex);
                    if (radars.Count > 0)
                    {
                        int radarIndex = rand.Next(radars.Count);
                        Stationary radar = radars[radarIndex];
                        double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                        airGroup.GroundAttack(missionType, radar, altitude, escortAirGroup);
                    }
                }
                else if (missionType == EMissionType.ATTACK_SHIP)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_VEHICLE)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
                }
                else if (missionType == EMissionType.ESCORT)
                {
                    AirGroup escortedAirGroup = getAvailableRandomEscortedAirGroup(airGroup.ArmyIndex);
                    if (escortedAirGroup != null)
                    {
                        List<EMissionType> availableEscortedMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in escortedAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(escortedAirGroup, targetMissionType) && isMissionTypeEscorted(targetMissionType))
                            {
                                availableEscortedMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableEscortedMissionTypes.Count > 0)
                        {
                            int escortedMissionTypeIndex = rand.Next(availableEscortedMissionTypes.Count);
                            EMissionType randomEscortedMissionType = availableEscortedMissionTypes[escortedMissionTypeIndex];
                            createAirOperation(sectionFile, briefingFile, escortedAirGroup, randomEscortedMissionType, true, airGroup);

                            airGroup.Escort(missionType, escortedAirGroup);
                        }
                    }
                }
                else if (missionType == EMissionType.INTERCEPT)
                {
                    AirGroup interceptedAirGroup = getAvailableRandomOffensiveAirGroup(airGroup.ArmyIndex);
                    if (interceptedAirGroup != null)
                    {
                        List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                        foreach (EMissionType targetMissionType in interceptedAirGroup.AircraftInfo.MissionTypes)
                        {
                            if (isMissionTypeAvailable(interceptedAirGroup, targetMissionType) && isMissionTypeOffensive(targetMissionType))
                            {
                                availableOffensiveMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableOffensiveMissionTypes.Count > 0)
                        {
                            int offensiveMissionTypeIndex = rand.Next(availableOffensiveMissionTypes.Count);
                            EMissionType randomOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];
                            createAirOperation(sectionFile, briefingFile, interceptedAirGroup, randomOffensiveMissionType, false, null);

                            airGroup.Intercept(missionType, interceptedAirGroup);
                        }
                    }
                }
                else if (missionType == EMissionType.MARITIME_RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.Recon(missionType, groundGroup, altitude, escortAirGroup);
                }
                else if (missionType == EMissionType.RECON)
                {
                    GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                    createRandomGroundOperation(sectionFile, groundGroup);
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.Recon(missionType, groundGroup, altitude, escortAirGroup);
                }

                getRandomFlightSize(airGroup, missionType);
                createBriefing(briefingFile, airGroup, escortAirGroup);
                airGroup.writeTo(sectionFile);

                if (forcedEscortAirGroup == null && escortAirGroup != null)
                {
                    availableAirGroups.Remove(escortAirGroup);
                    operatingAirGroups.Add(escortAirGroup);
                    this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, escortAirGroup.Id + " is operating.", null);

                    List<IAircraftParametersInfo> escortAircraftParametersInfos = escortAirGroup.AircraftInfo.GetAircraftParametersInfo(EMissionType.ESCORT);
                    int escortAircraftParametersInfoIndex = rand.Next(escortAircraftParametersInfos.Count);
                    IAircraftParametersInfo escortRandomAircraftParametersInfo = escortAircraftParametersInfos[escortAircraftParametersInfoIndex];
                    IAircraftLoadoutInfo escortAircraftLoadoutInfo = escortAirGroup.AircraftInfo.GetAircraftLoadoutInfo(escortRandomAircraftParametersInfo.LoadoutId);
                    escortAirGroup.Weapons = escortAircraftLoadoutInfo.Weapons;

                    escortAirGroup.Escort(EMissionType.ESCORT, airGroup);

                    getRandomFlightSize(escortAirGroup, EMissionType.ESCORT);
                    createBriefing(briefingFile, escortAirGroup, null);
                    escortAirGroup.writeTo(sectionFile);
                }

                if (isMissionTypeOffensive(missionType))
                {
                    if (allowIntercept)
                    {
                        AirGroup interceptAirGroup = getAvailableRandomInterceptAirGroup(airGroup);
                        if (interceptAirGroup != null)
                        {
                            availableAirGroups.Remove(interceptAirGroup);
                            operatingAirGroups.Add(interceptAirGroup);
                            this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, interceptAirGroup.Id + " is operating.", null);

                            List<IAircraftParametersInfo> interceptAircraftParametersInfos = interceptAirGroup.AircraftInfo.GetAircraftParametersInfo(EMissionType.INTERCEPT);
                            int interceptAircraftParametersInfoIndex = rand.Next(interceptAircraftParametersInfos.Count);
                            IAircraftParametersInfo interceptRandomAircraftParametersInfo = interceptAircraftParametersInfos[interceptAircraftParametersInfoIndex];
                            IAircraftLoadoutInfo interceptAircraftLoadoutInfo = interceptAirGroup.AircraftInfo.GetAircraftLoadoutInfo(interceptRandomAircraftParametersInfo.LoadoutId);
                            interceptAirGroup.Weapons = interceptAircraftLoadoutInfo.Weapons;

                            interceptAirGroup.Intercept(EMissionType.INTERCEPT, airGroup);

                            getRandomFlightSize(interceptAirGroup, EMissionType.INTERCEPT);
                            createBriefing(briefingFile, interceptAirGroup, null);
                            interceptAirGroup.writeTo(sectionFile);
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException(missionType.ToString());
            }
        }

        private void createRandomAirOperation(ISectionFile sectionFile, IBriefingFile briefingFile, AirGroup airGroup)
        {
            List<EMissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
            if (missionTypes != null && missionTypes.Count > 0)
            {
                List<EMissionType> availableMissionTypes = new List<EMissionType>();
                foreach (EMissionType missionType in missionTypes)
                {
                    if (isMissionTypeAvailable(airGroup, missionType))
                    {
                        availableMissionTypes.Add(missionType);

                        this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, airGroup.Id + " is available for " + missionType, null);
                    }
                    else
                    {
                        this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, airGroup.Id + " is unavailable for " + missionType, null);
                    }
                }

                if (availableMissionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = rand.Next(availableMissionTypes.Count);
                    EMissionType randomMissionType = availableMissionTypes[randomMissionTypeIndex];

                    createAirOperation(sectionFile, briefingFile, airGroup, randomMissionType, true, null);
                }
                else
                {
                    this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, "No mission type available.", null);
                }
            }
        }

        public void Init(ISectionFile missionFile)
        {
            frontMarkers.Clear();
            redStationaries.Clear();
            blueStationaries.Clear();
            redAirGroups.Clear();
            blueAirGroups.Clear();
            redGroundGroups.Clear();
            blueGroundGroups.Clear();

            operatingAirGroups.Clear();
            availableAirGroups.Clear();

            availableGroundGroups.Clear();
            operatingGroundGroups.Clear();

            for (int i = 0; i < missionFile.lines("Buildings"); i++)
            {
                string key;
                string value;
                missionFile.get("Buildings", i, out key, out value);

                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (valueParts.Length > 4)
                {
                    if (valueParts[0] == "buildings.House$GermanyMotorPool01")
                    {
                        double x;
                        double y;
                        double.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x);
                        double.TryParse(valueParts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y);
                        Point2d position = new Point2d(x, y);
                        factories.Add(position);
                    }
                }
            }

            for (int i = 0; i < missionFile.lines("Stationary"); i++)
            {
                string key;
                string value;
                missionFile.get("Stationary", i, out key, out value);

                // Radar
                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length > 4)
                {
                    if (valueParts[0] == "Stationary.Radar.EnglishRadar1")
                    {
                        double x;
                        double y;
                        string army = valueParts[1];
                        double.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x);
                        double.TryParse(valueParts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y);
                        Stationary radar = new Stationary(key, x, y);

                        if (army == "gb")
                        {
                            redStationaries.Add(radar);
                        }
                        else if (army == "de")
                        {
                            blueStationaries.Add(radar);
                        }
                    }
                }
            }

            for (int i = 0; i < missionFile.lines("FrontMarker"); i++)
            {
                string key;
                string value;
                missionFile.get("FrontMarker", i, out key, out value);

                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length == 3)
                {
                    double x;
                    double y;
                    int army;
                    if (double.TryParse(valueParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x)
                        && double.TryParse(valueParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y)
                        && int.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out army))
                    {
                        frontMarkers.Add(new FrontMarker(new Point2d(x, y), army));
                    }
                }
            }

            for (int i = 0; i < missionFile.lines("AirGroups"); i++)
            {
                string key;
                string value;
                missionFile.get("AirGroups", i, out key, out value);

                AirGroup airGroup = new AirGroup(this.Core, missionFile, key);
                availableAirGroups.Add(airGroup);

                if (AirGroupInfo.GetAirGroupInfo(1, airGroup.AirGroupKey) != null)
                {
                    getAirGroups(1).Add(airGroup);
                }
                else if (AirGroupInfo.GetAirGroupInfo(2, airGroup.AirGroupKey) != null)
                {
                    getAirGroups(2).Add(airGroup);
                }
            }


            for (int i = 0; i < missionFile.lines("Chiefs"); i++)
            {
                string key;
                string value;
                missionFile.get("Chiefs", i, out key, out value);

                GroundGroup groundGroup = new GroundGroup(missionFile, key);
                availableGroundGroups.Add(groundGroup);

                if (groundGroup.Army == 1)
                {
                    redGroundGroups.Add(groundGroup);
                }
                else if (groundGroup.Army == 2)
                {
                    blueGroundGroups.Add(groundGroup);
                }
            }
        }

        public ISectionFile GenerateRandomAirOperation()
        {
            checkPendingAirGroups();

            ISectionFile missionFile = null;

            if (availableAirGroups != null && availableAirGroups.Count > 0
                && operatingAirGroups.Count < this.Core.AdditionalAirOperations)
            {
                missionFile = this.Core.GamePlay.gpCreateSectionFile();
                IBriefingFile briefingFile = new BriefingFile();

                int randomAirGroupIndex = rand.Next(availableAirGroups.Count);
                AirGroup airGroup = availableAirGroups[randomAirGroupIndex];
                createRandomAirOperation(missionFile, briefingFile, airGroup);
            }

            return missionFile;
        }

        public void GenerateGroundOperation(GroundGroup groundGroup)
        {
            Point2d start = new Point2d(groundGroup.AiGroup.Pos().x, groundGroup.AiGroup.Pos().y);;
            
            if(groundGroup.Target == null)
            {
                if (groundGroup.Type == EGroundGroupType.Armor)
                {
                    // Offensive operation.
                    FrontMarker closestFrontMarker = null;
                    foreach (FrontMarker frontMarker in this.Core.Generator.FrontMarkers)
                    {
                        if (frontMarker.Army != groundGroup.Army)
                        {
                            if (closestFrontMarker == null)
                            {
                                closestFrontMarker = frontMarker;
                            }
                            else
                            {
                                if (frontMarker.Position.distance(ref start) < closestFrontMarker.Position.distance(ref start))
                                {
                                    closestFrontMarker = frontMarker;
                                }
                            }
                        }
                    }

                    if (closestFrontMarker != null)
                    {
                        Point2d end = new Point2d(closestFrontMarker.Position.x, closestFrontMarker.Position.y);
                        groundGroup.Target = end;
                    }               
                }
                else if (groundGroup.Type == EGroundGroupType.Ship)
                {
                    FrontMarker randomFrontMarker = null;
                    
                    // Do not use closest front marker.
                    List<FrontMarker> frontMarkers = new List<FrontMarker>();
                    foreach (FrontMarker frontMarker in getFriendlyMarkers(groundGroup.Army))
                    {
                        if (frontMarker.Position.distance(ref start) > 10)
                        {
                            frontMarkers.Add(frontMarker);
                        }
                    }

                    if (frontMarkers.Count > 0)
                    {
                        int randomMarkerIndex = rand.Next(frontMarkers.Count);
                        randomFrontMarker = frontMarkers[randomMarkerIndex];

                        Point2d end = new Point2d(randomFrontMarker.Position.x, randomFrontMarker.Position.y);
                        groundGroup.Target = end;
                    }
                }
                else if (groundGroup.Type == EGroundGroupType.Vehicle)
                {
                    //if (groundGroup.SubType == EGroundGroupSubType.Supply)
                    {
                        // Supply operation.
                        FrontMarker randomFrontMarker = null;

                        // Do not use closest front marker.
                        List<FrontMarker> frontMarkers = new List<FrontMarker>();
                        foreach (FrontMarker frontMarker in getFriendlyMarkers(groundGroup.Army))
                        {
                            if (frontMarker.Position.distance(ref start) > 10)
                            {
                                frontMarkers.Add(frontMarker);
                            }
                        }

                        if (frontMarkers.Count > 0)
                        {
                            int randomMarkerIndex = rand.Next(frontMarkers.Count);
                            randomFrontMarker = frontMarkers[randomMarkerIndex];

                            Point2d end = new Point2d(randomFrontMarker.Position.x, randomFrontMarker.Position.y);
                            groundGroup.Target = end;
                            groundGroup.PathParams = this.Core.GamePlay.gpFindPath(start, 10 * (groundGroup.Fails + 1), end, 10 * (groundGroup.Fails + 1), PathType.GROUND, groundGroup.Army);
                        }
                    }
                    //else if (groundGroup.SubType == EGroundGroupSubType.Artillery)
                    //{
                    //    // Defensive operation.
                    //    FrontMarker firstLineFrontMarker = null;
                    //    if (getFriendlyMarkers(groundGroup.Army).Count > 0)
                    //    {
                    //        List<FrontMarker> firstLineFrontMarkers = new List<FrontMarker>();
                    //        foreach (FrontMarker x in getFriendlyMarkers(groundGroup.Army))
                    //        {
                    //            bool firstline = true;
                    //            foreach (FrontMarker y in getFriendlyMarkers(groundGroup.Army))
                    //            {
                    //                if (x != y)
                    //                {
                    //                    if (x.distanceToFront() > x.distanceTo(y))
                    //                    {
                    //                        if (y.distanceToFront() < x.distanceTo(y))
                    //                        {
                    //                            firstline = false;
                    //                            break;
                    //                        }
                    //                    }
                    //                }
                    //            }

                    //            if (firstline)
                    //            {
                    //                firstLineFrontMarkers.Add(x);
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
    
            if (groundGroup.Target != null && groundGroup.Target.HasValue)
            {
                Point2d end = groundGroup.Target.Value;
                
                if (groundGroup.Stuck == true)
                {
                    Point2d p = new Point2d(start.x + 0.01 * (end.x - start.x), start.y + 0.01 * (end.y - start.y));
                    start = p;
                    groundGroup.Stuck = false;
                }

                //if (groundGroup.Fails < 10)
                {
                    // Failes 1 to 9 times. Move end closer to the start.
                    //if (groundGroup.Fails > 0)
                    //{
                    //    {
                    //        Point2d p = new Point2d(start.x + (1 - (groundGroup.Fails / 10)) * (end.x - start.x), start.y + (1 - (groundGroup.Fails / 10)) * (end.y - start.y));
                    //        end = p;
                    //    }
                    //}

                    if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
                    {
                        groundGroup.PathParams = this.Core.GamePlay.gpFindPath(start, 10, end, 10, PathType.GROUND, groundGroup.Army);
                    }
                    else if (groundGroup.Type == EGroundGroupType.Ship)
                    {
                        groundGroup.PathParams = this.Core.GamePlay.gpFindPath(start, 100, end, 100, PathType.WATER, groundGroup.Army);
                    }
                }
                //else
                //{
                //    // Failed 10 times. Increase radius of gpFindPath.
                //    if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle)
                //    {
                //        groundGroup.PathParams = this.Core.GamePlay.gpFindPath(start, 10 * (groundGroup.Fails - 9), end, 10 * (groundGroup.Fails - 9), PathType.GROUND, groundGroup.Army);
                //    }
                //    else if (groundGroup.Type == EGroundGroupType.Ship)
                //    {
                //        groundGroup.PathParams = this.Core.GamePlay.gpFindPath(start, 100 * (groundGroup.Fails - 9), end, 100 * (groundGroup.Fails - 9), PathType.WATER, groundGroup.Army);
                //    }

                //    this.Core.GamePlay.gpLogServer(new Player[] { this.Core.GamePlay.gpPlayer() }, "Radius: " + (groundGroup.Fails - 9).ToString(), null);
                //}
            }
        }

        public ISectionFile GenerateRandomGroundOperation()
        {
            ISectionFile missionFile = null;

            if (availableGroundGroups != null && availableGroundGroups.Count > 0)
            {
                missionFile = this.Core.GamePlay.gpCreateSectionFile();

                int randomGroundGroupIndex = rand.Next(availableGroundGroups.Count);
                GroundGroup groundGroup = availableGroundGroups[randomGroundGroupIndex];

                List<Point2d> friendlyFactories = new List<Point2d>();
                foreach (Point2d factory in factories)
                {
                    if (this.Core.GamePlay.gpFrontArmy(factory.x, factory.y) == groundGroup.Army)
                    {
                        friendlyFactories.Add(factory);
                    }
                }

                if (friendlyFactories.Count > 0)
                {
                    int randomFactoryIndex = rand.Next(friendlyFactories.Count);
                    Point2d factory = friendlyFactories[randomFactoryIndex];

                    groundGroup.Waypoints.Clear();
                    groundGroup.Waypoints.Add(new GroundGroupWaypoint(factory.x + 50, factory.y + 50, 38.40, 10.0));
                    groundGroup.Waypoints.Add(new GroundGroupWaypoint(factory.x + 55, factory.y + 55, 38.40, 10.0));

                    groundGroup.writeTo(missionFile);
                }
            }

            return missionFile;
        }

        public void Generate(string templateFileName, string missionId, out ISectionFile missionFile, out IBriefingFile briefingFile)
        {
            availableAirGroups.Clear();
            operatingAirGroups.Clear();
            availableGroundGroups.Clear();
            operatingAirGroups.Clear();

            foreach (AirGroup airGroup in AirGroups)
            {
                availableAirGroups.Add(airGroup);
            }

            foreach (GroundGroup groundGroup in GroundGroups)
            {
                availableGroundGroups.Add(groundGroup);
            }

            missionFile = this.Core.GamePlay.gpLoadSectionFile(templateFileName);
            briefingFile = new BriefingFile();

            briefingFile.MissionName = missionId;
            briefingFile.MissionDescription = "";

            // Delete everything from the template file.
            if (missionFile.exist("AirGroups"))
            {

                for (int i = 0; i < missionFile.lines("AirGroups"); i++)
                {
                    string key;
                    string value;
                    missionFile.get("AirGroups", i, out key, out value);
                    missionFile.delete(key);
                    missionFile.delete(key + "_Way");
                }
                missionFile.delete("AirGroups");
            }

            if (missionFile.exist("Chiefs"))
            {
                // Delete all ground groups from the template file.
                for (int i = 0; i < missionFile.lines("Chiefs"); i++)
                {
                    string key;
                    string value;
                    missionFile.get("Chiefs", i, out key, out value);
                    missionFile.delete(key + "_Road");
                }
                missionFile.delete("Chiefs");
            }

            for (int i = 0; i < missionFile.lines("MAIN"); i++)
            {
                string key;
                string value;
                missionFile.get("MAIN", i, out key, out value);
                if (key == "player")
                {
                    missionFile.delete("MAIN", i);
                    break;
                }
            }

            // Add things to the template file.

            int randomTime = rand.Next(5, 21);
            missionFile.set("MAIN", "TIME", randomTime.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            int randomWeatherIndex = rand.Next(0, 3);
            missionFile.set("MAIN", "WeatherIndex", randomWeatherIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            int randomCloudsHeight = rand.Next(5, 15);
            missionFile.set("MAIN", "CloudsHeight", (randomCloudsHeight * 100).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

            string weatherString = "";
            if (randomWeatherIndex == 0)
            {
                weatherString = "Clear";
            }
            else if (randomWeatherIndex == 1)
            {
                weatherString = "Light clouds at " + randomCloudsHeight * 100 + "m";
            }
            else if (randomWeatherIndex == 2)
            {
                weatherString = "Medium clouds at " + randomCloudsHeight * 100 + "m";
            }

            briefingFile.MissionDescription += this.Core.CurrentCareer.CampaignInfo.Name + "\n";
            briefingFile.MissionDescription += "Date: " + this.Core.CurrentCareer.Date.Value.ToShortDateString() + "\n";
            briefingFile.MissionDescription += "Time: " + randomTime + ":00\n";
            briefingFile.MissionDescription += "Weather: " + weatherString + "\n";

            foreach (AirGroup airGroup in getAirGroups(this.Core.CurrentCareer.ArmyIndex))
            {
                if ((airGroup.AirGroupKey + "." + airGroup.SquadronIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)) == this.Core.CurrentCareer.AirGroup)
                {
                    createRandomAirOperation(missionFile, briefingFile, airGroup);

                    List<string> aircraftOrder = new List<string>();
                    if (airGroup.AirGroupInfo.FlightSize % 3 == 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i)
                                {
                                    aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                                }
                            }

                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i + 3)
                                {
                                    aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + (i + 3).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                                }
                            }
                        }
                    }
                    else if (airGroup.AirGroupInfo.FlightSize % 2 == 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i)
                                {
                                    aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + i.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                                }
                            }

                            foreach (int key in airGroup.Flights.Keys)
                            {
                                if (airGroup.Flights[key].Count > i + 2)
                                {
                                    aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + (i + 2).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                                }
                            }
                        }
                    }
                    else if (airGroup.AirGroupInfo.FlightSize % 1 == 0)
                    {
                        foreach (int key in airGroup.Flights.Keys)
                        {
                            if (airGroup.Flights[key].Count == 1)
                            {
                                aircraftOrder.Add(key.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "0");
                            }
                        }
                    }


                    string playerAirGroupKey = airGroup.AirGroupKey;
                    int playerSquadronIndex = airGroup.SquadronIndex;
                    if (aircraftOrder.Count > 0)
                    {
                        string playerPosition = aircraftOrder[aircraftOrder.Count - 1];

                        double factor = aircraftOrder.Count / 6;
                        int playerPositionIndex = (int)(Math.Floor(this.Core.CurrentCareer.RankIndex * factor));

                        playerPosition = aircraftOrder[aircraftOrder.Count - 1 - playerPositionIndex];

                        if (missionFile.exist("MAIN", "player"))
                        {
                            missionFile.set("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + playerPosition);
                        }
                        else
                        {
                            missionFile.add("MAIN", "player", playerAirGroupKey + "." + playerSquadronIndex.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + playerPosition);
                        }
                    }
                    break;
                }
            }

            if (availableAirGroups != null && availableAirGroups.Count > 0)
            {
                for (int i = 0; i < this.Core.AdditionalAirOperations; i++)
                {
                    if (availableAirGroups.Count > 0)
                    {
                        int randomAirGroupIndex = rand.Next(availableAirGroups.Count);
                        AirGroup randomAirGroup = availableAirGroups[randomAirGroupIndex];
                        createRandomAirOperation(missionFile, briefingFile, randomAirGroup);
                    }
                }
            }

            if (availableGroundGroups != null && availableGroundGroups.Count > 0)
            {
                for (int i = 0; i < this.Core.AdditionalGroundOperations; i++)
                {
                    if (availableGroundGroups.Count > 0)
                    {
                        int randomGroundGroupIndex = rand.Next(availableGroundGroups.Count);
                        GroundGroup randomGroundGroup = availableGroundGroups[randomGroundGroupIndex];
                        availableGroundGroups.Remove(randomGroundGroup);

                        createRandomGroundOperation(missionFile, randomGroundGroup);
                    }
                }
            }
        }

        public void Created(GroundGroup groundGroup)
        {
            availableGroundGroups.Remove(groundGroup);
            operatingGroundGroups.Add(groundGroup);
        }

        public void Destroyed(GroundGroup groundGroup)
        {
            operatingGroundGroups.Remove(groundGroup);
            availableGroundGroups.Add(groundGroup);
        }

        public void UpdateWaypoints()
        {
            // TODO:
        }
    }
}