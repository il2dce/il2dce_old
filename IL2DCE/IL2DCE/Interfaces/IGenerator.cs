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
    public interface IGenerator
    {
        void Init(maddox.game.ISectionFile missionFile);

        maddox.game.ISectionFile GenerateRandomAirOperation();

        maddox.game.ISectionFile GenerateRandomGroundOperation();

        void GenerateGroundOperation(GroundGroup groundGroup);
        
        void Created(GroundGroup groundGroup);

        void Destroyed(GroundGroup groundGroup);

        void UpdateWaypoints();

        void Generate(string templateFileName, string missionId, out maddox.game.ISectionFile missionFile, out IBriefingFile briefingFile);
        
        System.Collections.Generic.List<FrontMarker> FrontMarkers
        {
            get;
        }

        System.Collections.Generic.List<FrontMarker> BlueFrontMarkers
        {
            get;
        }

        System.Collections.Generic.List<FrontMarker> RedFrontMarkers
        {
            get;
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

        System.Collections.Generic.IList<IGroundGroup> GroundGroups
        {
            get;
        }

        System.Collections.Generic.IList<IGroundGroup> RedGroundGroups
        {
            get;
        }

        System.Collections.Generic.IList<IGroundGroup> BlueGroundGroups
        {
            get;
        }

        double GetRandomAltitude(IAircraftParametersInfo missionParameters);
    }
}