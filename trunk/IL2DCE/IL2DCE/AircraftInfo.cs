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
    public class AircraftInfo : IAircraftInfo
    {
        public AircraftInfo(string aircraft)
        {
            Aircraft = aircraft;
        }

        public bool IsFlyable
        {
            get
            {
                if (flyableAircrafts.Contains(Aircraft))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<EMissionType> MissionTypes
        {
            get
            {
                List<EMissionType> missionTypes = new List<EMissionType>();

                if (reconAircrafts.Contains(Aircraft))
                {
                    if (!missionTypes.Contains(EMissionType.RECON_AREA))
                    {
                        missionTypes.Add(EMissionType.RECON_AREA);
                    }
                }

                if (bomberAircrafts.Contains(Aircraft))
                {
                    //if (!missionTypes.Contains(EMissionType.GROUND_ATTACK_AREA))
                    //{
                    //    missionTypes.Add(EMissionType.GROUND_ATTACK_AREA);
                    //}
                    if (!missionTypes.Contains(EMissionType.GROUND_ATTACK_TARGET))
                    {
                        missionTypes.Add(EMissionType.GROUND_ATTACK_TARGET);
                    }
                }

                if (fighterAircrafts.Contains(Aircraft))
                {
                    if (!missionTypes.Contains(EMissionType.OFFENSIVE_PATROL_AREA))
                    {
                        missionTypes.Add(EMissionType.OFFENSIVE_PATROL_AREA);
                    }
                    //if (!missionTypes.Contains(MissionType.DEFENSIVE_PATROL_AREA))
                    //{
                    //    missionTypes.Add(MissionType.DEFENSIVE_PATROL_AREA);
                    //}
                    if (!missionTypes.Contains(EMissionType.ESCORT))
                    {
                        missionTypes.Add(EMissionType.ESCORT);
                    }
                    if (!missionTypes.Contains(EMissionType.INTERCEPT))
                    {
                        missionTypes.Add(EMissionType.INTERCEPT);
                    }
                }

                return missionTypes;
            }
        }

        public string Aircraft
        {
            get;
            set;
        }

        private List<string> flyableAircrafts = new List<string>
        {
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
            "Aircraft.BlenheimMkIV",
            "Aircraft.Ju-87B-2",
            "Aircraft.He-111H-2",
            "Aircraft.He-111P-2",
            "Aircraft.Ju-88A-1",
            "Aircraft.BR-20M", 
        };

        private List<string> liaisonAircrafts = new List<string>
        {
            "Aircraft.AnsonMkI",
            "Aircraft.DH82A",
            "Aircraft.Bf-108B-2",
        };

        private List<string> reconAircrafts = new List<string>
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

        private List<string> fighterAircrafts = new List<string>
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
            "Aircraft.Bf-110C-4",

            "Aircraft.CR42",
            "Aircraft.G50",  
        };

        private List<string> bomberAircrafts = new List<string>
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
            "Aircraft.Bf-109E-3B",
            "Aircraft.Bf-110C-7",
        };
    }
}