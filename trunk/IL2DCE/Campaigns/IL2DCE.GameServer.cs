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

//$reference parts/IL2DCE/IL2DCE.dll
//$reference parts/core/gamePlay.dll
//$debug

using System;
using maddox.game;
using maddox.game.world;

public class Mission : AMission
{
    public override void OnMissionLoaded(int missionNumber)
    {
        base.OnMissionLoaded(missionNumber);
        
        if (missionNumber == 0)
        {
            maddox.game.GameDef game = GamePlay as maddox.game.GameDef;
            IL2DCE.Core core = new IL2DCE.Core(game);

            core.CurrentCampaign = core.Campaigns[0];

            maddox.game.ISectionFile missionFile = core.StartCampaign();

            GamePlay.gpPostMissionLoad(missionFile);
        }
    }    
}
