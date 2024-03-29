﻿// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
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

namespace IL2DCE
{
    public enum EMissionType
    {
        //LIASON,

        RECON,
        MARITIME_RECON,
        ARMED_RECON,
        ARMED_MARITIME_RECON,
        
        ATTACK_ARMOR,
        ATTACK_VEHICLE,
        //ATTACK_ARTILLERY,
        ATTACK_RADAR,
        ATTACK_SHIP,

        INTERCEPT,
        //MARITIME_INTERCEPT,
        //NIGHT_INTERCEPT,
        ESCORT,
        COVER,
        //MARITIME_COVER,

        //INTRUDER,
        //NIGHT_INTRUDER,
    };

    public interface IAircraftInfo
    {
        bool IsFlyable
        {
            get;
        }

        List<EMissionType> MissionTypes
        {
            get;
        }

        string Aircraft
        {
            get;
        }

        List<IAircraftParametersInfo> GetAircraftParametersInfo(EMissionType missionType);

        IAircraftLoadoutInfo GetAircraftLoadoutInfo(string loadoutId);
    }
}