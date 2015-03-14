using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE
{
    class AirHeadquarters : IHeadquarters
    {
        private int maxAltitude = 4000;

        private Army Army
        {
            get
            {
                return this.army;
            }
        }
        private Army army;

        private List<IStrategicPoint> FriendlyStrategicPoints
        {
            get
            {
                List<IStrategicPoint> strategicPoints = new List<IStrategicPoint>();
                foreach (IStrategicPoint strategicPoint in PersistentWorld.Map.StrategicPoints)
                {
                    if (strategicPoint.Army == Army)
                    {
                        strategicPoints.Add(strategicPoint);
                    }
                }
                return strategicPoints;
            }
        }

        private List<IStrategicPoint> EnemyStrategicPoints
        {
            get
            {
                List<IStrategicPoint> strategicPoints = new List<IStrategicPoint>();
                foreach (IStrategicPoint strategicPoint in PersistentWorld.Map.StrategicPoints)
                {
                    if (strategicPoint.Army != Army)
                    {
                        strategicPoints.Add(strategicPoint);
                    }
                }
                return strategicPoints;
            }
        }

        private List<Radar> EnemyRadars
        {
            get
            {
                List<Radar> radars = new List<Radar>();
                foreach (IBuilding building in PersistentWorld.Buildings.Values)
                {
                    if (building is Radar && building.Army != Army)
                    {
                        radars.Add(building as Radar);
                    }
                }
                return radars;
            }
        }

        private List<StationaryAircraft> EnemyStationaryAircrafts
        {
            get
            {
                List<StationaryAircraft> stationaryAircrafts = new List<StationaryAircraft>();
                foreach (IBuilding building in PersistentWorld.Buildings.Values)
                {
                    if (building is StationaryAircraft && building.Army != Army)
                    {
                        stationaryAircrafts.Add(building as StationaryAircraft);
                    }
                }
                return stationaryAircrafts;
            }
        }

        private List<IUnit> FriendlyUnits
        {
            get
            {
                return this.friendlyUnits;
            }
        }
        private List<IUnit> friendlyUnits = new List<IUnit>();

        private List<IUnit> EnemyUnits
        {
            get
            {
                return this.enemyUnits;
            }
        }
        private List<IUnit> enemyUnits = new List<IUnit>();

        private List<IUnit> IdleUnits
        {
            get
            {
                List<IUnit> idleUnits = new List<IUnit>();
                foreach (IUnit unit in FriendlyUnits)
                {
                    if (unit.State == UnitState.Idle)
                    {
                        idleUnits.Add(unit);
                    }
                }
                return idleUnits;
            }
        }

        private List<IUnit> IdleInterceptors
        {
            get
            {
                List<IUnit> idleInterceptors = new List<IUnit>();
                foreach (IUnit unit in EnemyUnits)
                {
                    if (unit.State == UnitState.Idle && unit is AirGroup && ((unit as AirGroup).AircraftInfo.MissionTypes.Contains(EMissionType.INTERCEPT) || (unit as AirGroup).AircraftInfo.MissionTypes.Contains(EMissionType.COVER)))
                    {
                        idleInterceptors.Add(unit);
                    }
                }
                return idleInterceptors;
            }
        }

        public AirHeadquarters(IPersistentWorld persistentWorld, Army army)
        {
            PersistentWorld = persistentWorld;
            PersistentWorld.MissionSlice += new EventHandler(OnMissionSlice);

            PersistentWorld.UnitDiscovered += new UnitEventHandler(OnUnitDiscovered);
            PersistentWorld.UnitCovered += new UnitEventHandler(OnUnitCovered);

            this.army = army;
        }

        void OnUnitDiscovered(object sender, UnitEventArgs e)
        {
            //if (e.Unit.Army != Army)
            //{
            //    if (!EnemyUnits.ContainsKey(e.Unit))
            //    {
            //        PersistentWorld.Debug(e.Unit.Id + " detected. Intercept!");

            //        if (IdleUnits.Count > 0)
            //        {
            //            List<AirGroup> idleInterceptors = new List<AirGroup>();
            //            foreach(IUnit idleUnit in IdleUnits)
            //            {
            //                if(idleUnit is AirGroup)
            //                {
            //                    if((idleUnit as AirGroup).AircraftInfo.MissionTypes.Contains(EMissionType.INTERCEPT))
            //                    {
            //                        idleInterceptors.Add(idleUnit as AirGroup);
            //                    }
            //                }
            //            }
                        
            //            int unitIndex = PersistentWorld.Random.Next(idleInterceptors.Count);
            //            AirGroup airGroup = idleInterceptors[unitIndex] as AirGroup;
            //            EnemyUnits.Add(e.Unit, airGroup);

            //            AirGroup escortAirGroup = null;
            //            CreateAirOperation(new BriefingFile(), airGroup, EMissionType.INTERCEPT, false, ref escortAirGroup);

            //            PersistentWorld.TakeOrder(new AirOrder(airGroup, escortAirGroup, null));
            //        }
            //    }
            //    else
            //    {
            //        PersistentWorld.Debug(e.Unit.Id + " redetected. Update!");             
            //    }
            //}
        }

        void OnUnitCovered(object sender, UnitEventArgs e)
        {
            //PersistentWorld.Debug(e.Unit.Id + " lost. RTB!");
        }

        public IPersistentWorld PersistentWorld
        {
            set
            {
                this.persistentWorld = value;
            }
            get
            {
                return this.persistentWorld;
            }
        }
        private IPersistentWorld persistentWorld;

        public void Register(IUnit unit)
        {
            if (unit is AirGroup && unit.Army == Army && !FriendlyUnits.Contains(unit))
            {
                FriendlyUnits.Add(unit);
            }
            else if(unit is AirGroup && unit.Army != Army && !EnemyUnits.Contains(unit))
            {
                EnemyUnits.Add(unit);
            }
        }

        public void Deregister(IUnit unit)
        {
            if (FriendlyUnits.Contains(unit))
            {
                FriendlyUnits.Remove(unit);
            }
            else if(EnemyUnits.Contains(unit))
            {
                EnemyUnits.Remove(unit);
            }
        }

        private void OnMissionSlice(object sender, EventArgs e)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup availableAirGroup in IdleUnits)
            {
                foreach (EMissionType missionType in availableAirGroup.AircraftInfo.MissionTypes)
                {
                    if(AircraftInfo.IsMissionTypeOffensive(missionType))
                    {
                        airGroups.Add(availableAirGroup);
                    }
                }
            }

            if (airGroups.Count > 0)
            {
                int unitIndex = PersistentWorld.Random.Next(airGroups.Count);
                AirGroup airGroup = airGroups[unitIndex];
                AirGroup escortAirGroup = null;
                AirGroup interceptAirGroup = null;

                CreateRandomAirOperation(airGroup, ref escortAirGroup, ref interceptAirGroup);

                PersistentWorld.TakeOrder(new AirOrder(airGroup, escortAirGroup, interceptAirGroup, null));
            }
        }

        private bool IsMissionTypeAvailable(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.COVER)
            {
                //List<IStrategicPoint> strategicPoints = FriendlyStrategicPoints;
                //if (strategicPoints.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                //List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Ship });
                //if (groundGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ARMED_RECON)
            {
                //List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                //if (groundGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                //List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Armor });
                //if (groundGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                List<Radar> radars = EnemyRadars;
                if (radars.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_AIRCRAFT)
            {
                List<StationaryAircraft> stationaryAircrafts = EnemyStationaryAircrafts;
                if (stationaryAircrafts.Count > 0)
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
                //List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Ship });
                //if (groundGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                //List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
                //if (groundGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ESCORT)
            {
                //List<AirGroup> airGroups = getAvailableEscortedAirGroups(airGroup.Army);
                //if (airGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.INTERCEPT)
            {
                //List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.Army);
                //if (airGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.MARITIME_RECON)
            {
                //List<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Ship });
                //if (groundGroups.Count > 0)
                //{
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.RECON)
            {
                List<IStrategicPoint> strategicPoints = EnemyStrategicPoints;
                if (strategicPoints.Count > 0)
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

        private void CreateRandomAirOperation(AirGroup airGroup, ref AirGroup escortAirGroup, ref AirGroup interceptAirGroup)
        {
            List<EMissionType> missionTypes = airGroup.AircraftInfo.MissionTypes;
            if (missionTypes != null && missionTypes.Count > 0)
            {
                List<EMissionType> availableMissionTypes = new List<EMissionType>();
                foreach (EMissionType missionType in missionTypes)
                {
                    if (IsMissionTypeAvailable(airGroup, missionType))
                    {
                        availableMissionTypes.Add(missionType);
                        //PersistentWorld.Debug(airGroup.Id + " is available for " + missionType);
                    }
                    //else
                    //{
                    //    PersistentWorld.Debug(airGroup.Id + " is unavailable for " + missionType);
                    //}
                }

                if (availableMissionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = PersistentWorld.Random.Next(availableMissionTypes.Count);
                    EMissionType randomMissionType = availableMissionTypes[randomMissionTypeIndex];

                    CreateAirOperation(new BriefingFile(), airGroup, randomMissionType, true, ref escortAirGroup, ref interceptAirGroup);
                }
                else
                {
                    PersistentWorld.Debug("No mission type available.");
                }
            }
        }

        private void CreateBriefing(BriefingFile briefingFile, AirGroup airGroup, AirGroup escortAirGroup)
        {
            // TODO: Implement
        }

        private void CreateAirOperation(BriefingFile briefingFile, AirGroup airGroup, EMissionType missionType, bool allowIntercept, ref AirGroup escortAirGroup, ref AirGroup interceptAirGroup)
        {
            List<AircraftParametersInfo> aircraftParametersInfos = airGroup.AircraftInfo.GetAircraftParametersInfo(missionType);
            int aircraftParametersInfoIndex = PersistentWorld.Random.Next(aircraftParametersInfos.Count);
            AircraftParametersInfo randomAircraftParametersInfo = aircraftParametersInfos[aircraftParametersInfoIndex];
            AircraftLoadoutInfo aircraftLoadoutInfo = airGroup.AircraftInfo.GetAircraftLoadoutInfo(randomAircraftParametersInfo.LoadoutId);
            airGroup.Weapons = aircraftLoadoutInfo.Weapons;
            airGroup.Detonator = aircraftLoadoutInfo.Detonator;
            airGroup.Formation = airGroup.AirGroupInfo.DefaultFormation;

            bool forcedEscortAirGroup = true;
            if (AircraftInfo.IsMissionTypeEscorted(missionType))
            {
                if (escortAirGroup == null)
                {
                    forcedEscortAirGroup = false;

                    List<AirGroup> escortAirGroups = new List<AirGroup>();
                    foreach (AirGroup availableAirGroup in IdleUnits)
                    {
                        if (availableAirGroup != airGroup && availableAirGroup.AircraftInfo.MissionTypes.Contains(EMissionType.ESCORT))
                        {
                            escortAirGroups.Add(availableAirGroup);
                        }
                    }

                    if (escortAirGroups.Count > 0)
                    {
                        int unitIndex = PersistentWorld.Random.Next(escortAirGroups.Count);
                        escortAirGroup = escortAirGroups[unitIndex];
                    }
                }
            }

            if (missionType == EMissionType.COVER)
            {
                //List<IStrategicPoint> strategicPoints = FriendlyStrategicPoints;
                //if (strategicPoints.Count > 0)
                //{
                //    int strategicPointIndex = PersistentWorld.Random.Next(strategicPoints.Count);
                //    IStrategicPoint strategicPoint = strategicPoints[strategicPointIndex];
                //    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //    airGroup.Cover(missionType, new maddox.GP.Point2d(strategicPoint.Position.Item1, strategicPoint.Position.Item2), altitude);
                //}
            }
            else if (missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                //GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Ship });
                //double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
            }
            else if (missionType == EMissionType.ARMED_RECON)
            {
                //GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle });
                //double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                //GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Armor });
                //double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                List<Radar> radars = EnemyRadars;
                if (radars.Count > 0)
                {
                    int radarIndex = PersistentWorld.Random.Next(radars.Count);
                    Radar radar = radars[radarIndex];
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, new maddox.GP.Point2d(radar.Position.Item1, radar.Position.Item2), altitude, escortAirGroup);
                }
            }
            else if (missionType == EMissionType.ATTACK_AIRCRAFT)
            {
                List<StationaryAircraft> stationaryAircrafts = EnemyStationaryAircrafts;
                if (stationaryAircrafts.Count > 0)
                {
                    int stationaryAircraftIndex = PersistentWorld.Random.Next(stationaryAircrafts.Count);
                    StationaryAircraft stationaryAircraft = stationaryAircrafts[stationaryAircraftIndex];
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.GroundAttack(missionType, new maddox.GP.Point2d(stationaryAircraft.Position.Item1, stationaryAircraft.Position.Item2), altitude, escortAirGroup);
                }
            }
            else if (missionType == EMissionType.ATTACK_SHIP)
            {
                //GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Ship });
                //double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                //GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
                //double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //airGroup.GroundAttack(missionType, groundGroup, altitude, escortAirGroup);
            }
            else if (missionType == EMissionType.ESCORT)
            {
                //AirGroup escortedAirGroup = getAvailableRandomEscortedAirGroup(airGroup.Army);
                //if (escortedAirGroup != null)
                //{
                //    List<EMissionType> availableEscortedMissionTypes = new List<EMissionType>();
                //    foreach (EMissionType targetMissionType in escortedAirGroup.AircraftInfo.MissionTypes)
                //    {
                //        if (IsMissionTypeAvailable(escortedAirGroup, targetMissionType) && AircraftInfo.IsMissionTypeEscorted(targetMissionType))
                //        {
                //            availableEscortedMissionTypes.Add(targetMissionType);
                //        }
                //    }

                //    if (availableEscortedMissionTypes.Count > 0)
                //    {
                //        int escortedMissionTypeIndex = PersistentWorld.Random.Next(availableEscortedMissionTypes.Count);
                //        EMissionType randomEscortedMissionType = availableEscortedMissionTypes[escortedMissionTypeIndex];
                //        CreateAirOperation(briefingFile, escortedAirGroup, randomEscortedMissionType, true, airGroup);

                //        airGroup.Escort(missionType, escortedAirGroup);
                //    }
                //}
            }
            else if (missionType == EMissionType.INTERCEPT)
            {
                //AirGroup interceptedAirGroup = getAvailableRandomOffensiveAirGroup(airGroup.Army);
                //if (interceptedAirGroup != null)
                //{
                //    List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                //    foreach (EMissionType targetMissionType in interceptedAirGroup.AircraftInfo.MissionTypes)
                //    {
                //        if (IsMissionTypeAvailable(interceptedAirGroup, targetMissionType) && AircraftInfo.IsMissionTypeOffensive(targetMissionType))
                //        {
                //            availableOffensiveMissionTypes.Add(targetMissionType);
                //        }
                //    }

                //    if (availableOffensiveMissionTypes.Count > 0)
                //    {
                //        int offensiveMissionTypeIndex = PersistentWorld.Random.Next(availableOffensiveMissionTypes.Count);
                //        EMissionType randomOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];
                //        CreateAirOperation(briefingFile, interceptedAirGroup, randomOffensiveMissionType, false, null);

                //        airGroup.Intercept(missionType, interceptedAirGroup);
                //    }
                //}

                //AirGroup interceptedAirGroup = null;
                //foreach (IUnit enemyUnit in EnemyUnits.Keys)
                //{
                //    if (EnemyUnits[enemyUnit] == airGroup)
                //    {
                //        interceptedAirGroup = enemyUnit as AirGroup;
                //    }
                //}

                //if (interceptedAirGroup != null)
                //{
                //    airGroup.Intercept(missionType, interceptedAirGroup);
                //}
            }
            else if (missionType == EMissionType.MARITIME_RECON)
            {
                //GroundGroup groundGroup = getAvailableRandomEnemyGroundGroup(airGroup.Army, new List<EGroundGroupType> { EGroundGroupType.Ship });
                //double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                //airGroup.Recon(missionType, groundGroup, altitude, escortAirGroup);
            }
            else if (missionType == EMissionType.RECON)
            {
                List<IStrategicPoint> strategicPoints = EnemyStrategicPoints;
                if (strategicPoints.Count > 0)
                {
                    int strategicPointIndex = PersistentWorld.Random.Next(strategicPoints.Count);
                    IStrategicPoint strategicPoint = strategicPoints[strategicPointIndex];
                    double altitude = GetRandomAltitude(randomAircraftParametersInfo);

                    airGroup.Recon(missionType, new maddox.GP.Point2d(strategicPoint.Position.Item1, strategicPoint.Position.Item2), altitude);
                }
            }

            GetRandomFlightSize(airGroup, missionType);
            CreateBriefing(briefingFile, airGroup, escortAirGroup);

            if (forcedEscortAirGroup == false && escortAirGroup != null)
            {
                List<AircraftParametersInfo> escortAircraftParametersInfos = escortAirGroup.AircraftInfo.GetAircraftParametersInfo(EMissionType.ESCORT);
                int escortAircraftParametersInfoIndex = PersistentWorld.Random.Next(escortAircraftParametersInfos.Count);
                AircraftParametersInfo escortRandomAircraftParametersInfo = escortAircraftParametersInfos[escortAircraftParametersInfoIndex];
                AircraftLoadoutInfo escortAircraftLoadoutInfo = escortAirGroup.AircraftInfo.GetAircraftLoadoutInfo(escortRandomAircraftParametersInfo.LoadoutId);
                escortAirGroup.Weapons = escortAircraftLoadoutInfo.Weapons;

                escortAirGroup.Escort(EMissionType.ESCORT, airGroup);

                GetRandomFlightSize(escortAirGroup, EMissionType.ESCORT);
                CreateBriefing(briefingFile, escortAirGroup, null);
            }

            if (AircraftInfo.IsMissionTypeOffensive(missionType))
            {
                if (IdleInterceptors.Count > 0)
                {
                    int unitIndex = PersistentWorld.Random.Next(IdleInterceptors.Count);
                    interceptAirGroup = IdleInterceptors[unitIndex] as AirGroup;

                    List<EMissionType> missionTypes = new List<EMissionType>();
                    if (interceptAirGroup.AircraftInfo.MissionTypes.Contains(EMissionType.INTERCEPT))
                    {
                        missionTypes.Add(EMissionType.INTERCEPT);
                    }
                    if (interceptAirGroup.AircraftInfo.MissionTypes.Contains(EMissionType.COVER))
                    {
                        missionTypes.Add(EMissionType.COVER);
                    }

                    if (missionTypes.Count > 0)
                    {
                        int missionTypeIndex = PersistentWorld.Random.Next(missionTypes.Count);
                        EMissionType interceptMissionType = missionTypes[missionTypeIndex];

                        List<AircraftParametersInfo> interceptAircraftParametersInfos = interceptAirGroup.AircraftInfo.GetAircraftParametersInfo(interceptMissionType);
                        int interceptAircraftParametersInfoIndex = PersistentWorld.Random.Next(interceptAircraftParametersInfos.Count);
                        AircraftParametersInfo interceptRandomAircraftParametersInfo = interceptAircraftParametersInfos[interceptAircraftParametersInfoIndex];
                        AircraftLoadoutInfo interceptAircraftLoadoutInfo = interceptAirGroup.AircraftInfo.GetAircraftLoadoutInfo(interceptRandomAircraftParametersInfo.LoadoutId);
                        interceptAirGroup.Weapons = interceptAircraftLoadoutInfo.Weapons;

                        if (interceptMissionType == EMissionType.INTERCEPT)
                        {
                            interceptAirGroup.Intercept(interceptMissionType, airGroup);
                        }
                        else if (interceptMissionType == EMissionType.COVER)
                        {
                            interceptAirGroup.Cover(interceptMissionType, airGroup.TargetArea.Value, airGroup.Altitude.Value);
                        }

                        GetRandomFlightSize(interceptAirGroup, interceptMissionType);
                        CreateBriefing(briefingFile, interceptAirGroup, null);
                    }
                }
            }
        }

        public double GetRandomAltitude(AircraftParametersInfo missionParameters)
        {
            if (missionParameters.MinAltitude != null && missionParameters.MinAltitude.HasValue && missionParameters.MaxAltitude != null && missionParameters.MaxAltitude.HasValue)
            {
                int maxAltitude = (int)missionParameters.MaxAltitude.Value;
                if(maxAltitude > this.maxAltitude)
                {
                    maxAltitude = this.maxAltitude;
                }

                return (double)PersistentWorld.Random.Next((int)missionParameters.MinAltitude.Value, maxAltitude);
            }
            else
            {
                // Use some default altitudes
                return (double)PersistentWorld.Random.Next(300, this.maxAltitude);
            }
        }

        private void GetRandomFlightSize(AirGroup airGroup, EMissionType missionType)
        {
            airGroup.Flights.Clear();
            int aircraftNumber = 1;

            int flightCount = (int)Math.Ceiling(airGroup.AirGroupInfo.FlightCount * PersistentWorld.FlightCountFactor);
            int flightSize = (int)Math.Ceiling(airGroup.AirGroupInfo.FlightSize * PersistentWorld.FlightSizeFactor);

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

        //private List<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        //{
        //    List<GroundGroup> groundGroups = new List<GroundGroup>();
        //    foreach (GroundGroup groundGroup in getAvailableEnemyGroundGroups(armyIndex))
        //    {
        //        if (groundGroupTypes.Contains(groundGroup.Type))
        //        {
        //            groundGroups.Add(groundGroup);
        //        }
        //    }
        //    return groundGroups;
        //}

        //private List<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex, List<EGroundGroupType> groundGroupTypes)
        //{
        //    List<GroundGroup> groundGroups = new List<GroundGroup>();
        //    foreach (GroundGroup groundGroup in getAvailableFriendlyGroundGroups(armyIndex))
        //    {
        //        if (groundGroupTypes.Contains(groundGroup.Type))
        //        {
        //            groundGroups.Add(groundGroup);
        //        }
        //    }
        //    return groundGroups;
        //}
    }
}
