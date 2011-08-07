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

namespace IL2DCE
{
    namespace Mission
    {
        public class MissionServer : Mission
        {
            private IL2DCE.Core _core;

            public override void OnBattleInit()
            {
                base.OnBattleInit();

                maddox.game.ISectionFile confFile = GamePlay.gpLoadSectionFile("$home/parts/IL2DCE/conf.ini");
                _core = new IL2DCE.Core(GamePlay as IL2DCE.IGame, confFile);

                _core.CurrentCampaign = _core.Campaigns[0];

                maddox.game.ISectionFile missionFile = _core.ContinueCampaign();

                string missionFileName = string.Format(string.Format("$user/mission/IL2DCE/IL2DCE_{0}.mis", System.DateTime.Now.ToString("yyyyMMddHHmmssffff")));
                missionFile.save(missionFileName);

                GamePlay.gpPostMissionLoad(missionFileName);
            }
        }
    }
}