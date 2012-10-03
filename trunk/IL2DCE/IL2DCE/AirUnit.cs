﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using maddox.game;

//namespace IL2DCE
//{
//    class AirUnit : IUnit
//    {
//        public event UnitEventHandler Idle;

//        public event UnitEventHandler Pending;

//        public event UnitEventHandler Busy;

//        public event UnitEventHandler Destroyed;

//        public event UnitEventHandler Covered;

//        public event UnitEventHandler Discovered;

//        public string Id
//        {
//            get
//            {
//                return this.id;
//            }
//        }
//        private string id;

//        public string Regiment
//        {
//            get
//            {
//                return this.regiment;
//            }
//        }
//        private string regiment;

//        public string Squadron
//        {
//            get
//            {
//                return this.squadron;
//            }
//        }
//        private string squadron;

//        public string Flight
//        {
//            get
//            {
//                return this.flight;
//            }
//        }
//        private string flight;

//        public string AircraftType
//        {
//            get
//            {
//                return this.aircraftType;
//            }
//        }
//        private string aircraftType;

//        public AircraftInfo AircraftInfo
//        {
//            get
//            {
//                return new AircraftInfo(PersistentWorld.AircraftInfoFile, AircraftType);
//            }
//        }

//        private IPersistentWorld PersistentWorld
//        {
//            get
//            {
//                return this.persistentWorld;
//            }
//        }
//        private IPersistentWorld persistentWorld;
        
//        public Tuple<double, double, double> Position
//        {
//            get
//            {
//                Tuple<double, double, double> position = PersistentWorld.GetPositionOf(this);
//                if (position != null)
//                {
//                    return position;
//                }
//                else
//                {
//                    return Airfield;
//                }
//            }
//        }

//        public Tuple<double, double, double> Airfield
//        {
//            get
//            {
//                return this.airfield;
//            }
//        }
//        private Tuple<double, double, double> airfield;
        
//        public UnitState State
//        {
//            get
//            {
//                return this.state;
//            }
//        }
//        public UnitState state;

//        public AirUnit(IPersistentWorld persistentWorld, string id, ISectionFile missionFile)
//        {
//            this.persistentWorld = persistentWorld;
            
//            this.id = id;
//            this.regiment = id.Substring(0, id.IndexOf("."));
//            this.squadron = id.Substring(id.IndexOf(".") + 1, 1);
//            this.flight = id.Substring(id.IndexOf(".") + 2, 1);

//            this.state = UnitState.Idle;
//            this.aircraftType = missionFile.get(id, "AircraftType");

//            string position = missionFile.get(id, "Position");
//            string[] positionList = position.Split(new char[] { ' ' });
//            double x = double.Parse(positionList[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
//            double y = double.Parse(positionList[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
//            double z = double.Parse(positionList[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
//            this.airfield = new Tuple<double, double, double>(x, y, z);

//            PersistentWorld.Debug(id + " created.");
//        }

//        public AirGroupInfo AirGroupInfo
//        {
//            get
//            {
//                AirGroupInfo airGroupInfo = null;
//                airGroupInfo = IL2DCE.AirGroupInfo.GetAirGroupInfo(1, Regiment);
//                if (airGroupInfo != null)
//                {
//                    return airGroupInfo;
//                }
//                airGroupInfo = IL2DCE.AirGroupInfo.GetAirGroupInfo(2, Regiment);
//                if (airGroupInfo != null)
//                {
//                    return airGroupInfo;
//                }

//                return null;
//            }
//        }

//        public Army Army
//        {
//            get
//            {
//                if (IL2DCE.AirGroupInfo.GetAirGroupInfo(1, Regiment) != null)
//                {
//                    return IL2DCE.Army.Red;
//                }
//                else if (IL2DCE.AirGroupInfo.GetAirGroupInfo(2, Regiment) != null)
//                {
//                    return IL2DCE.Army.Blue;
//                }
//                else
//                {
//                    return IL2DCE.Army.None;
//                }
//            }
//        }
        
//        public void RaiseIdle()
//        {
//            this.state = UnitState.Idle;

//            if (Idle != null)
//            {
//                Idle(this, new UnitEventArgs(this));
//            }

//            PersistentWorld.Debug(Id + " idle.");
//        }

//        public void RaisePending()
//        {
//            this.state = UnitState.Pending;

//            if (Pending != null)
//            {
//                Pending(this, new UnitEventArgs(this));
//            }

//            PersistentWorld.Debug(Id + " pending.");
//        }

//        public void RaiseBusy()
//        {
//            this.state = UnitState.Busy;

//            if (Busy != null)
//            {
//                Busy(this, new UnitEventArgs(this));
//            }

//            PersistentWorld.Debug(Id + " busy.");
//        }

//        public void RaiseDestroyed()
//        {
//            this.state = UnitState.Destroyed;

//            if (Destroyed != null)
//            {
//                Destroyed(this, new UnitEventArgs(this));
//            }

//            PersistentWorld.Debug(Id + " destroyed.");
//        }

//        public void RaiseCovered()
//        {
//            if (Covered != null)
//            {
//                Covered(this, new UnitEventArgs(this));
//            }

//            PersistentWorld.Debug(Id + " covered.");
//        }

//        public void RaiseDiscovered()
//        {
//            if (Discovered != null)
//            {
//                Discovered(this, new UnitEventArgs(this));
//            }

//            PersistentWorld.Debug(Id + " discovered.");
//        }
//    }
//}
