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
    public interface ICore
    {
        maddox.game.IGamePlay GamePlay
        {
            get;
        }

        ICareer CurrentCareer
        {
            get;
            set;
        }

        List<ICareer> Careers
        {
            get;
        }

        List<ICampaignInfo> CampaignInfos
        {
            get;
        }

        IGenerator Generator
        {
            get;
        }

        maddox.game.ISectionFile GlobalAircraftInfoFile
        {
            get;
        }

        void DeleteCareer(ICareer career);

        void InitCampaign();

        void ResetCampaign(IGame game);

        void Generate(string templateFileName, string missionId, out maddox.game.ISectionFile missionFile, out IBriefingFile briefingFile);

        void AdvanceCampaign(IGame game);

        bool SpawnParked
        {
            get;
            set;
        }

        int Debug
        {
            get;
            set;
        }

        double FlightSize
        {
            get;
        }

        double FlightCount
        {
            get;
        }

        int AdditionalAirOperations
        {
            get;
        }

        int AdditionalGroundOperations
        {
            get;
        }
    }
}