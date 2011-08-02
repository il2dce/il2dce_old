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

namespace IL2DCE
{
    public interface ICore
    {
        IGame Game
        {
            get;
        }

        void Init(string templateFileName);

        maddox.game.ISectionFile Generate(string missionFileName);

        bool SpawnParked
        {
            get;
            set;
        }

        System.Collections.Generic.IList<IAirGroup> AirGroups
        {
            get;
        }

        System.Collections.Generic.IList<IAirGroup> RedAirGroups
        {
            get;
        }

        System.Collections.Generic.IList<IAirGroup> BlueAirGroups
        {
            get;
        }

        int? PlayerSquadronIndex
        {
            get;
            set;
        }

        int? PlayerFlightIndex
        {
            get;
            set;
        }

        int? PlayerAircraftIndex
        {
            get;
            set;
        }

        string PlayerAirGroupKey
        {
            get;
            set;
        }

        IAirGroup PlayerAirGroup
        {
            get;
            set;
        }
    }
}