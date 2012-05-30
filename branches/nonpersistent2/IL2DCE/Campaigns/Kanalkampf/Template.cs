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

//$reference parts/IL2DCE/IL2DCE.Mission.dll
//$debug

// Various emergency & service cars script by naryv posted an sukhoi.ru today.
// http://forum.1cpublishing.eu/showthread.php?p=335269

// exclude AI fix: http://forum.1cpublishing.eu/showthread.php?t=26112&page=4

using System;
using System.Collections.Generic;
using maddox.game;
using maddox.game.world;
using maddox.GP;

//public class Mission : IL2DCE.Mission.MissionSingle
public class Mission : AMission
{
    public override void OnBattleStarted()
    {
        base.OnBattleStarted();
        MissionNumberListener = -1;
    }

    Random rnd = new Random();


    [Flags]
    internal enum ServiceType // ??? ????????????? ??????? 
    {
        NONE = 0,
        EMERGENCY = 1,
        FIRE = 2,
        FUEL = 4,
        AMMO = 8,
        BOMBS = 16,
        PRISONERCAPTURE = 32
    }

    internal class TechCars
    {
        internal AiGroundGroup TechCar { get; set; }
        internal AiAirport BaseAirport { get; set; }
        internal IRecalcPathParams cur_rp { get; set; }
        internal int RouteFlag = 0;
        internal int cartype = 0;
        internal int servPlaneNum = -1;
        internal ServiceType CarType { get { return (ServiceType)cartype; } set { cartype = (int)value; } }

        public TechCars(AiGroundGroup car, AiAirport airoport, IRecalcPathParams rp)
        {
            this.TechCar = car;
            this.BaseAirport = airoport;
            this.cur_rp = rp;
        }

    }

    internal class PlanesQueue
    {
        internal AiAircraft aircraft { get; set; }
        internal AiAirport baseAirport { get; set; }
        internal int state = 0;
        internal ServiceType State { get { return (ServiceType)state; } set { state = (int)value; } }
        internal int Lifetime = 0;
        internal float health = 1;
        public PlanesQueue(AiAircraft aircraft, AiAirport baseAirport, int state)
        {
            this.aircraft = aircraft;
            this.baseAirport = baseAirport;
            this.state = state;
        }
    }

    internal List<TechCars> CurTechCars = new List<TechCars>();
    internal List<PlanesQueue> CurPlanesQueue = new List<PlanesQueue>();
    TechCars TmpCar = null;
    bool MissionLoading = false;

    internal double PseudoRnd(double MinValue, double MaxValue)
    {
        return rnd.NextDouble() * (MaxValue - MinValue) + MinValue;
    }



    public override void OnActorTaskCompleted(int missionNumber, string shortName, AiActor actor)
    {
        base.OnActorTaskCompleted(missionNumber, shortName, actor);


        AiActor ai_actor = actor as AiActor;
        if (ai_actor != null)
        {
            if (ai_actor is AiGroundGroup)
                for (int i = 0; i < CurTechCars.Count; i++) // ???? ????????????? ??????? ??????? ?? ?????????????? ????????, ????????? ?? ????????????
                {
                    if (CurTechCars[i].TechCar == ai_actor as AiGroundGroup)
                        if (CurTechCars[i].RouteFlag == 1)
                            EndPlaneService(i);

                        else
                            CheckNotServicedPlanes(i);
                };
        }
    }

    internal void CheckNotServicedPlanes(int techCarIndex)
    {
        for (int j = 0; j < CurPlanesQueue.Count; j++)
        {
            if (CurTechCars[techCarIndex].TechCar.IsAlive() && (CurPlanesQueue[j].baseAirport == CurTechCars[techCarIndex].BaseAirport) && ((CurTechCars[techCarIndex].CarType & CurPlanesQueue[j].State) != 0) && (CurTechCars[techCarIndex].servPlaneNum == -1))
            {
                if (SetEmrgCarRoute(j, techCarIndex))   // ?????????? ??????? ??????????? ????????? ???????
                {
                    return;
                }
            }
        }
    }

    internal void EndPlaneService(int techCarIndex)
    {
        if (CurTechCars[techCarIndex].cur_rp == null) return;
        CurTechCars[techCarIndex].cur_rp = null; // ?????????? ???????                 
        if (CurTechCars[techCarIndex].servPlaneNum >= 0)
        {

            CurPlanesQueue[CurTechCars[techCarIndex].servPlaneNum].State &= ~CurTechCars[techCarIndex].CarType; // ??????? ??? ???????????? ? ?????????????? ????????, ???? ?? ????

            CurTechCars[techCarIndex].servPlaneNum = -1; // ?????????? ????? ?????????????? ????????
            Timeout(5f, () =>
            {
                if (!MoveFromRWay(techCarIndex))// ????????? ?? ????? ?? ?? ???????, ? ??????? ? ??? ???? ???.
                {
                    CurTechCars[techCarIndex].RouteFlag = 0;
                    CheckNotServicedPlanes(techCarIndex);   // ? ???????, ??? ?? ??? ????????????? ?????????
                }
            });

        }
        else Timeout(5f, () =>
        {
            CurTechCars[techCarIndex].RouteFlag = 0;
            CheckNotServicedPlanes(techCarIndex);   // ? ???????, ??? ?? ??? ????????????? ?????????                
        });


    }

    internal bool MoveFromRWay(int carNum)
    {
        bool result = false;
        if ((GamePlay.gpLandType(CurTechCars[carNum].TechCar.Pos().x, CurTechCars[carNum].TechCar.Pos().y) & LandTypes.ROAD) == 0)
            return result;

        Point3d TmpPos = CurTechCars[carNum].TechCar.Pos();
        while (((GamePlay.gpLandType(TmpPos.x, TmpPos.y) & LandTypes.ROAD) != 0))
        {
            TmpPos.x += 10f;
            TmpPos.y += 10f;
        };
        Point2d EmgCarStart, EmgCarFinish;
        EmgCarStart.x = CurTechCars[carNum].TechCar.Pos().x; EmgCarStart.y = CurTechCars[carNum].TechCar.Pos().y;
        EmgCarFinish.x = TmpPos.x; EmgCarFinish.y = TmpPos.y;
        CurTechCars[carNum].servPlaneNum = -1;
        CurTechCars[carNum].RouteFlag = 0;
        CurTechCars[carNum].cur_rp = null;
        CurTechCars[carNum].cur_rp = GamePlay.gpFindPath(EmgCarStart, 10f, EmgCarFinish, 10f, PathType.GROUND, CurTechCars[carNum].TechCar.Army());

        result = true;
        return result;
    }


    public bool SetEmrgCarRoute(int aircraftNumber, int carNum)
    {
        bool result = false;
        if (CurTechCars[carNum].TechCar != null)
        {
            CurTechCars[carNum].servPlaneNum = aircraftNumber; // ????????????? ????? ?????????????? ????????
            if (CurTechCars[carNum].cur_rp == null)
            {
                Point2d EmgCarStart, EmgCarFinish, LandedPos;
                LandedPos.x = CurPlanesQueue[aircraftNumber].aircraft.Pos().x; LandedPos.y = CurPlanesQueue[aircraftNumber].aircraft.Pos().y;
                int Sign = ((carNum % 2) == 0) ? 2 : -2;
                EmgCarStart.x = CurTechCars[carNum].TechCar.Pos().x; EmgCarStart.y = CurTechCars[carNum].TechCar.Pos().y;
                EmgCarFinish.x = LandedPos.x - PseudoRnd(2f, 5f) * ((LandedPos.x - EmgCarStart.x) / (Math.Abs(LandedPos.x - EmgCarStart.x))) - Sign;
                EmgCarFinish.y = LandedPos.y - PseudoRnd(2f, 5f) * ((LandedPos.y - EmgCarStart.y) / (Math.Abs(LandedPos.y - EmgCarStart.y))) - Sign;
                CurTechCars[carNum].cur_rp = GamePlay.gpFindPath(EmgCarStart, 10f, EmgCarFinish, 10f, PathType.GROUND, CurTechCars[carNum].TechCar.Army());
                result = true;
            }
        }
        return result;
    }


    public override void OnMissionLoaded(int missionNumber)
    {
        base.OnMissionLoaded(missionNumber);
        if (missionNumber > 0)
        {
            List<string> CarTypes = new List<string>();
            CarTypes.Add(":0_Chief_Emrg_");
            CarTypes.Add(":0_Chief_Fire_");
            CarTypes.Add(":0_Chief_Fuel_");
            CarTypes.Add(":0_Chief_Ammo_");
            CarTypes.Add(":0_Chief_Bomb_");
            CarTypes.Add(":0_Chief_Prisoner_");

            AiGroundGroup MyCar = null;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < CarTypes.Count; j++)
                {
                    MyCar = GamePlay.gpActorByName(missionNumber.ToString() + CarTypes[j] + i.ToString()) as AiGroundGroup;
                    if (MyCar != null)
                    {
                        TmpCar = new TechCars(MyCar, FindNearestAirport(MyCar), null);
                        TmpCar.CarType = (ServiceType)(1 << j);
                        TmpCar.cur_rp = null;
                        if (!CurTechCars.Contains(TmpCar))
                            CurTechCars.Add(TmpCar);
                        MissionLoading = false;
                    };
                }
            }
        }
    }


    public override void OnTickGame()
    {
        base.OnTickGame();
        try
        {
            if (Time.tickCounter() % 64 == 0)
            {

                for (int i = 0; i < CurPlanesQueue.Count; i++)
                {
                    CurPlanesQueue[i].Lifetime++;
                    if ((CurPlanesQueue[i].State == ServiceType.NONE) || (CurPlanesQueue[i].aircraft == null) || (CurPlanesQueue[i].Lifetime > 200))
                    {
                        for (int j = 0; j < CurTechCars.Count; j++)
                            if (CurTechCars[j].servPlaneNum == i)
                                EndPlaneService(j);
                        CurPlanesQueue.RemoveAt(i);
                    }
                };

                for (int i = 0; i < CurTechCars.Count; i++)
                {
                    TechCars car = CurTechCars[i];
                    if ((car.TechCar != null && car.cur_rp != null) && (car.cur_rp.State == RecalcPathState.SUCCESS))
                    {
                        if (car.TechCar.IsAlive() && (car.RouteFlag == 0)/* && (car.servPlaneNum != -1)*/)
                        {
                            car.RouteFlag = 1;
                            car.cur_rp.Path[0].P.x = car.TechCar.Pos().x; car.cur_rp.Path[0].P.y = car.TechCar.Pos().y;
                            car.TechCar.SetWay(car.cur_rp.Path);
                            //if (car.servPlaneNum != -1) car.RouteFlag = 0;
                        }

                        double Dist = Math.Sqrt((car.cur_rp.Path[car.cur_rp.Path.Length - 1].P.x - car.TechCar.Pos().x) * (car.cur_rp.Path[car.cur_rp.Path.Length - 1].P.x - car.TechCar.Pos().x) + (car.cur_rp.Path[car.cur_rp.Path.Length - 1].P.y - car.TechCar.Pos().y) * (car.cur_rp.Path[car.cur_rp.Path.Length - 1].P.y - car.TechCar.Pos().y));
                        if (car.servPlaneNum != -1)
                        {
                            if (Dist < ((CurPlanesQueue[car.servPlaneNum].aircraft.Type() == AircraftType.Bomber) ? 20f : 10f))
                                EndPlaneService(i);
                        }
                        else if (Dist < 15f)
                        {
                            EndPlaneService(i);
                        }
                    }
                    if ((car.cur_rp == null) && (car.RouteFlag == 0) && (car.servPlaneNum != -1))
                    {
                        EndPlaneService(i);
                    };
                };
            }

        }
        catch (Exception e) { }
    }




    internal AiAirport FindNearestAirport(AiActor actor)
    {
        AiAirport aMin = null;
        double d2Min = 0;
        Point3d pd = actor.Pos();
        int n = GamePlay.gpAirports().Length;
        for (int i = 0; i < n; i++)
        {
            AiAirport a = (AiAirport)GamePlay.gpAirports()[i];

            if (!a.IsAlive())
                continue;

            Point3d pp;
            pp = a.Pos();
            pd.z = pp.z;
            double d2 = pd.distanceSquared(ref pp);
            if ((aMin == null) || (d2 < d2Min))
            {
                aMin = a;
                d2Min = d2;
            }
        }
        if (d2Min > 2250000.0)
            aMin = null;

        return aMin;
    }

    internal ISectionFile CreateEmrgCarMission(Point3d startPos, double fRadius, int portArmy, int planeArmy, AircraftType type, float health, Point3d aircraftPos)
    {
        ISectionFile f = GamePlay.gpCreateSectionFile();
        string sect;
        string key;
        string value;
        string ChiefName1 = "0_Chief_" + (health < 1f ? "Fire_" : "Fuel_");
        string ChiefName2 = "0_Chief_" + (health < 1f ? "Emrg_" : "Ammo_");
        string ChiefName3 = "0_Chief_" + (health < 1f ? "Bomb_" : "Bomb_");


        if (portArmy == planeArmy) //???? ????????
        {
            switch (portArmy)
            {
                case 1:
                    if (health < 1f)
                    {
                        sect = "CustomChiefs";
                        key = "";
                        value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma"; //???????
                        f.add(sect, key, value);
                        value = "Vehicle.custom_chief_emrg_1 $core/icons/tank.mma";//??????
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.Austin_K2_ATV";
                        value = "";
                        f.add(sect, key, value);
                        key = "TrailerUnit.Fire_pump_UK2_Transport";
                        value = "1";
                        f.add(sect, key, value);
                        sect = "Vehicle.custom_chief_emrg_1";
                        key = "Car.Austin_K2_Ambulance";
                        value = "";
                        f.add(sect, key, value);

                        sect = "Chiefs";
                        key = "0_Chief_Fire_0";
                        value = "Vehicle.custom_chief_emrg_0 gb /skin0 materialsSummer_RAF";
                        f.add(sect, key, value);
                        key = "0_Chief_Emrg_1";
                        value = "Vehicle.custom_chief_emrg_1 gb /skin0 materialsSummer_RAF";
                        f.add(sect, key, value);
                    }
                    else
                    {
                        sect = "CustomChiefs";
                        key = "";
                        value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma";
                        f.add(sect, key, value);
                        value = "Vehicle.custom_chief_emrg_1 $core/icons/tank.mma";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_0"; // ??????????
                        key = "Car.Albion_AM463";
                        value = "";
                        f.add(sect, key, value);
                        if (type == AircraftType.Bomber)  // ??? ???????? ?????? ??????? ? ????? ????????
                        {
                            key = "Car.Fordson_N";
                            value = "";
                            f.add(sect, key, value);
                            key = "TrailerUnit.Towed_Bowser_UK1_Transport";
                            value = "1";
                            f.add(sect, key, value);
                        }

                        sect = "Vehicle.custom_chief_emrg_1"; // ??????
                        value = "";
                        key = "Car.Bedford_MW_open";
                        f.add(sect, key, value);


                        if (type == AircraftType.Bomber)  // ??? ???????? ????? ????????
                        {
                            sect = "CustomChiefs";
                            key = "";
                            value = "Vehicle.custom_chief_emrg_2 $core/icons/tank.mma";
                            f.add(sect, key, value);
                            sect = "Vehicle.custom_chief_emrg_2";
                            value = "";
                            key = "Car.Fordson_N";
                            value = "";
                            f.add(sect, key, value);
                            key = "TrailerUnit.BombLoadingCart_UK1_Transport";
                            value = "1";
                            f.add(sect, key, value);
                            key = "TrailerUnit.BombLoadingCart_UK1_Transport";
                            f.add(sect, key, value);
                        };

                        sect = "Chiefs";
                        key = "0_Chief_Fuel_0";
                        value = "Vehicle.custom_chief_emrg_0 gb /skin0 materialsSummer_RAF";
                        f.add(sect, key, value);

                        key = "0_Chief_Ammo_1";
                        value = "Vehicle.custom_chief_emrg_1 gb /skin0 materialsSummer_RAF/tow00_00 1_Static";
                        f.add(sect, key, value);

                        if (type == AircraftType.Bomber)
                        {
                            key = "0_Chief_Bomb_2";
                            value = "Vehicle.custom_chief_emrg_2 gb /tow01_00 2_Static/tow01_01 3_Static/tow01_02 4_Static/tow01_03 5_Static/tow02_00 6_Static/tow02_01 7_Static";
                            f.add(sect, key, value);
                        }
                        sect = "Stationary";
                        key = "1_Static";
                        value = "Stationary.Morris_CS8-Bedford_MW_CargoAmmo3 gb 0.00 0.00 0.00";
                        f.add(sect, key, value);
                        if (type == AircraftType.Bomber) // ????? ??????
                        {
                            key = "2_Static";
                            value = "Stationary.Weapons_.Bomb_B_GP_250lb_MkIV gb 0.00 0.00 0.00";
                            f.add(sect, key, value);
                            key = "3_Static";
                            value = "Stationary.Weapons_.Bomb_B_GP_250lb_MkIV gb 0.00 0.00 0.00";
                            f.add(sect, key, value);
                            key = "4_Static";
                            value = "Stationary.Weapons_.Bomb_B_GP_250lb_MkIV gb 0.00 0.00 0.00";
                            f.add(sect, key, value);
                            key = "5_Static";
                            value = "Stationary.Weapons_.Bomb_B_GP_250lb_MkIV gb 0.00 0.00 0.00";
                            f.add(sect, key, value);
                            key = "6_Static";
                            value = "Stationary.Weapons_.Bomb_B_GP_500lb_MkIV gb 0.00 0.00 0.00";
                            f.add(sect, key, value);
                            key = "7_Static";
                            value = "Stationary.Weapons_.Bomb_B_GP_500lb_MkIV gb 0.00 0.00 0.00";
                            f.add(sect, key, value);
                        };
                    };
                    break;
                case 2:
                    sect = "CustomChiefs";          //???????
                    key = "";
                    value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma";
                    f.add(sect, key, value);
                    value = "Vehicle.custom_chief_emrg_1 $core/icons/tank.mma";//??????
                    f.add(sect, key, value);
                    if (health < 1f)
                    {
                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.Renault_UE";
                        value = "";
                        f.add(sect, key, value);
                        key = "TrailerUnit.Foam_Extinguisher_GER1_Transport";
                        value = "1";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_1";
                        if (PseudoRnd(0f, 1f) < 0.5f)
                        {
                            key = "Car.Opel_Blitz_med-tent";
                        }
                        else { key = "Car.Opel_Blitz_cargo_med"; };
                        value = "";
                        f.add(sect, key, value);
                        sect = "Chiefs";
                        key = "0_Chief_Fire_0";// "0_Chief_emrg";
                        value = "Vehicle.custom_chief_emrg_0 de ";
                        f.add(sect, key, value);
                        key = "0_Chief_Emrg_1";// "0_Chief_emrg";
                        value = "Vehicle.custom_chief_emrg_1 de ";
                        f.add(sect, key, value);
                    }
                    else
                    {
                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.Opel_Blitz_fuel";
                        value = "";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_1";
                        key = "Car.Renault_UE";
                        f.add(sect, key, value);
                        key = "TrailerUnit.Oil_Cart_GER1_Transport";
                        value = "1";
                        f.add(sect, key, value);
                        key = "Car.Renault_UE";
                        value = "";
                        f.add(sect, key, value);
                        key = "TrailerUnit.Anlasswagen_(starter)_GER1_Transport";
                        value = "1";
                        f.add(sect, key, value);

                        if (type == AircraftType.Bomber) // ????? ??????
                        {
                            sect = "CustomChiefs";
                            key = "";
                            value = "Vehicle.custom_chief_emrg_2 $core/icons/tank.mma";
                            f.add(sect, key, value);
                            sect = "Vehicle.custom_chief_emrg_2";
                            key = "Car.Renault_UE";
                            value = "";
                            f.add(sect, key, value);
                            key = "TrailerUnit.HydraulicBombLoader_GER1_Transport";
                            value = "1";
                            f.add(sect, key, value);
                            key = "Car.Renault_UE";
                            value = "";
                            f.add(sect, key, value);
                            key = "TrailerUnit.BombSled_GER1_Transport";
                            value = "1";
                            f.add(sect, key, value);
                        }

                        sect = "Chiefs";
                        key = "0_Chief_Fuel_0";
                        value = "Vehicle.custom_chief_emrg_0 de";
                        f.add(sect, key, value);
                        key = "0_Chief_Ammo_1";
                        value = "Vehicle.custom_chief_emrg_1 de";
                        f.add(sect, key, value);
                        if (type == AircraftType.Bomber)
                        {
                            key = "0_Chief_Bomb_2";
                            value = "Vehicle.custom_chief_emrg_2 de /tow01_00 1_Static/tow03_00 2_Static";
                            f.add(sect, key, value);
                            sect = "Stationary";
                            key = "1_Static";
                            value = "Stationary.Weapons_.Bomb_B_SC-250_Type2_J de 0.00 0.00 0.00";
                            f.add(sect, key, value);
                            key = "2_Static";
                            value = "Stationary.Weapons_.Bomb_B_SC-1000_C de 0.00 0.00 0.00";
                            f.add(sect, key, value);
                        };
                    };
                    break;
                default:
                    break;

            }
        }
        else
        {
            switch (portArmy)
            {
                case 1:
                    if (health < 1f)
                    {
                        sect = "CustomChiefs";
                        key = "";
                        value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma"; //???????
                        f.add(sect, key, value);
                        value = "Vehicle.custom_chief_emrg_1 $core/icons/tank.mma";//??????
                        f.add(sect, key, value);
                        value = "Vehicle.custom_chief_emrg_2 $core/icons/tank.mma";//????????
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.Austin_K2_ATV";
                        value = "";
                        f.add(sect, key, value);
                        key = "TrailerUnit.Fire_pump_UK2_Transport";
                        value = "1";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_1";
                        key = "Car.Austin_K2_Ambulance";
                        value = "";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_2";
                        key = "Car.Beaverette_III";
                        value = "";
                        f.add(sect, key, value);

                        sect = "Chiefs";
                        key = "0_Chief_Fire_0";
                        value = "Vehicle.custom_chief_emrg_0 gb /skin0 materialsSummer_RAF";
                        f.add(sect, key, value);
                        key = "0_Chief_Emrg_1";
                        value = "Vehicle.custom_chief_emrg_1 gb /skin0 materialsSummer_RAF";
                        f.add(sect, key, value);
                        key = "0_Chief_Prisoner_2";
                        value = "Vehicle.custom_chief_emrg_2 gb ";
                        f.add(sect, key, value);
                        ChiefName3 = "0_Chief_Prisoner_";

                    }
                    else
                    {
                        sect = "CustomChiefs";
                        key = "";
                        value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma"; //???????
                        f.add(sect, key, value);
                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.Beaverette_III";
                        value = "";
                        f.add(sect, key, value);
                        sect = "Chiefs";
                        key = "0_Chief_Prisoner_0";
                        value = "Vehicle.custom_chief_emrg_0 gb ";
                        f.add(sect, key, value);
                        ChiefName1 = "0_Chief_Prisoner_";
                    };
                    break;
                case 2:
                    if (health < 1f)
                    {
                        sect = "CustomChiefs";
                        key = "";
                        value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma"; //???????
                        f.add(sect, key, value);
                        value = "Vehicle.custom_chief_emrg_1 $core/icons/tank.mma";//??????
                        f.add(sect, key, value);
                        value = "Vehicle.custom_chief_emrg_2 $core/icons/tank.mma";//????????
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.Renault_UE";
                        value = "";
                        f.add(sect, key, value);
                        key = "TrailerUnit.Foam_Extinguisher_GER1_Transport";
                        value = "1";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_1";
                        key = "Car.Opel_Blitz_cargo_med";
                        value = "";
                        f.add(sect, key, value);

                        sect = "Vehicle.custom_chief_emrg_2";
                        key = "Car.SdKfz_231_6Rad";
                        value = "";
                        f.add(sect, key, value);

                        sect = "Chiefs";
                        key = "0_Chief_Fire_0";
                        value = "Vehicle.custom_chief_emrg_0 de";
                        f.add(sect, key, value);
                        key = "0_Chief_Emrg_1";
                        value = "Vehicle.custom_chief_emrg_1 de";
                        f.add(sect, key, value);
                        key = "0_Chief_Prisoner_2";
                        value = "Vehicle.custom_chief_emrg_2 de /marker0 1940-42_var1";
                        f.add(sect, key, value);
                        ChiefName3 = "0_Chief_Prisoner_";

                    }
                    else
                    {
                        sect = "CustomChiefs";
                        key = "";
                        value = "Vehicle.custom_chief_emrg_0 $core/icons/tank.mma"; //???????
                        f.add(sect, key, value);
                        sect = "Vehicle.custom_chief_emrg_0";
                        key = "Car.SdKfz_231_6Rad";
                        value = "";
                        f.add(sect, key, value);
                        sect = "Chiefs";
                        key = "0_Chief_Prisoner_0";
                        value = "Vehicle.custom_chief_emrg_0 de /marker0 1940-42_var1";
                        f.add(sect, key, value);
                        ChiefName1 = "0_Chief_Prisoner_";
                    };
                    break;
                default:
                    break;
            };
        }


        Point3d TmpStartPos = startPos;
        TmpStartPos.x += PseudoRnd(-30f, 30f) + fRadius; TmpStartPos.y += PseudoRnd(-30f, 30f) + fRadius;
        Point3d BirthPos = EmrgVehicleStartPos(TmpStartPos, startPos);

        sect = ChiefName1 + "0" + "_Road";
        key = "";
        value = BirthPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "  0 92 5 ";
        f.add(sect, key, value);
        BirthPos.x -= 50f * ((BirthPos.x - aircraftPos.x) / Math.Abs(BirthPos.x - aircraftPos.x)); BirthPos.y -= 50f * ((BirthPos.y - aircraftPos.y) / Math.Abs(BirthPos.y - aircraftPos.y));
        value = BirthPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        f.add(sect, key, value);

        TmpStartPos = startPos;
        TmpStartPos.x += PseudoRnd(-30f, 30f) - fRadius; TmpStartPos.y += PseudoRnd(-30f, 30f) + fRadius;
        BirthPos = EmrgVehicleStartPos(TmpStartPos, startPos);
        //BirthPos = TmpStartPos;
        sect = ChiefName2 + "1" + "_Road";
        key = "";
        value = BirthPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "  0 92 5 ";
        f.add(sect, key, value);
        BirthPos.x -= 50f * ((BirthPos.x - aircraftPos.x) / Math.Abs(BirthPos.x - aircraftPos.x)); BirthPos.y -= 50f * ((BirthPos.y - aircraftPos.y) / Math.Abs(BirthPos.y - aircraftPos.y));
        value = BirthPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        f.add(sect, key, value);

        TmpStartPos = startPos;
        TmpStartPos.x += PseudoRnd(-30f, 30f) + fRadius; TmpStartPos.y += PseudoRnd(-30f, 30f) - fRadius;
        BirthPos = EmrgVehicleStartPos(TmpStartPos, startPos);

        sect = ChiefName3 + "2" + "_Road";
        key = "";
        value = BirthPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "  0 92 5 ";
        f.add(sect, key, value);
        BirthPos.x -= 50f * ((BirthPos.x - aircraftPos.x) / Math.Abs(BirthPos.x - aircraftPos.x)); BirthPos.y -= 50f * ((BirthPos.y - aircraftPos.y) / Math.Abs(BirthPos.y - aircraftPos.y));
        value = BirthPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + BirthPos.z.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        f.add(sect, key, value);

        return f;
    }


    internal Point3d EmrgVehicleStartPos(Point3d startPos, Point3d endPos)
    {
        Point3d TmpPos = startPos;

        while (((GamePlay.gpLandType(TmpPos.x, TmpPos.y) & LandTypes.WATER) != 0))
        {
            TmpPos.x -= (TmpPos.x - endPos.x) / 10f;
            TmpPos.y -= (TmpPos.y - endPos.y) / 10f;
        };

        return TmpPos;
    }

    internal void CheckEmrgCarOnAirport(int aircraftNumber)
    {
        // ????????? ???? ?? ??????? ? ?????????         
        AiGroundGroup MyCar = null;
        for (int i = 0; i < CurTechCars.Count; i++)
        {
            if (CurTechCars[i].TechCar != null)
            {
                if (CurTechCars[i].TechCar.IsAlive() && CurTechCars[i].BaseAirport == CurPlanesQueue[aircraftNumber].baseAirport && (CurTechCars[i].CarType & CurPlanesQueue[aircraftNumber].State) != 0)
                {
                    MissionLoading = false;
                    MyCar = CurTechCars[i].TechCar;
                    if ((CurTechCars[i].cur_rp == null) && (CurTechCars[i].RouteFlag == 0) && (CurTechCars[i].servPlaneNum == -1)) // ???? ????? ??? ???? - ???????? ????????
                        SetEmrgCarRoute(aircraftNumber, i);
                }
            }
        };
        if ((MyCar == null) && !MissionLoading)
        {
            MissionLoading = true;
            int ArmyPos = 0;
            if (GamePlay.gpFrontExist())
            {
                ArmyPos = GamePlay.gpFrontArmy(CurPlanesQueue[aircraftNumber].baseAirport.Pos().x, CurPlanesQueue[aircraftNumber].baseAirport.Pos().y);
            }
            else { ArmyPos = CurPlanesQueue[aircraftNumber].aircraft.Army(); };
            // ??????? ?????? ? ?????????                                     
            GamePlay.gpPostMissionLoad(CreateEmrgCarMission(CurPlanesQueue[aircraftNumber].baseAirport.Pos(), (CurPlanesQueue[aircraftNumber].baseAirport.FieldR() / 4), ArmyPos, CurPlanesQueue[aircraftNumber].aircraft.Army(), CurPlanesQueue[aircraftNumber].aircraft.Type(), CurPlanesQueue[aircraftNumber].health, CurPlanesQueue[aircraftNumber].aircraft.Pos()));
        }
        return;
    }


    public override void OnAircraftCrashLanded(int missionNumber, string shortName, AiAircraft aircraft)
    {
        base.OnAircraftCrashLanded(missionNumber, shortName, aircraft);
        Timeout(5, () =>
        {
            aircraft.Destroy();
        });
    }

    public override void OnAircraftLanded(int missionNumber, string shortName, AiAircraft aircraft)
    {
        base.OnAircraftLanded(missionNumber, shortName, aircraft);

        AiAirport NearestAirport = FindNearestAirport(aircraft);
        //if (NearestAirport != null)

        if ((NearestAirport != null) && (!isAiControlledPlane(aircraft)))       // here is the fix 
        {
            PlanesQueue CurPlane = new PlanesQueue(aircraft, NearestAirport, 0);
            int ArmyPos = 0;
            CurPlane.health = (float)aircraft.getParameter(part.ParameterTypes.M_Health, -1);
            if (GamePlay.gpFrontExist())
            {
                ArmyPos = GamePlay.gpFrontArmy(NearestAirport.Pos().x, NearestAirport.Pos().y);
            }
            else { ArmyPos = aircraft.Army(); };
            if (CurPlane.health < 1f)
            {
                CurPlane.State |= ServiceType.EMERGENCY;
                CurPlane.State |= ServiceType.FIRE;
            }
            else if (aircraft.Army() == ArmyPos)
            {
                CurPlane.State |= ServiceType.FUEL;
                CurPlane.State |= ServiceType.AMMO;
                if (aircraft.Type() == AircraftType.Bomber) CurPlane.State |= ServiceType.BOMBS;
            };
            if (!(aircraft.Army() == ArmyPos)) CurPlane.State |= ServiceType.PRISONERCAPTURE;
            if (!CurPlanesQueue.Contains(CurPlane))
            {
                CurPlanesQueue.Add(CurPlane);
                CheckEmrgCarOnAirport(CurPlanesQueue.Count - 1);
            }
            else
            {
                for (int i = 0; i < CurPlanesQueue.Count; i++)
                    if (CurPlanesQueue[i] == CurPlane)
                    {
                        CheckEmrgCarOnAirport(i);
                        break;
                    }
            }
            CurPlane = null;
        };

    }

}
