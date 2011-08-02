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
    public enum MissionType
    {
        RECON_AREA,
        GROUND_ATTACK_AREA,
        OFFENSIVE_PATROL_AREA,
        DEFENSIVE_PATROL_AREA,
        ESCORT,
        INTERCEPT,
    };

    public interface IAircraftInfo
    {
        bool IsFlyable
        {
            get;
        }

        List<MissionType> MissionTypes
        {
            get;
        }

        string Aircraft
        {
            get;
        }
    }
}