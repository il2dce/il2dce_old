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
    namespace Mission
    {
        public class Mission : AMission
        {
            IGameSingle Game
            {
                get
                {
                    return GamePlay as IGameSingle;
                }
            }

            public override void OnSingleBattleSuccess(bool success)
            {
                if (Game != null)
                {
                    if (success == true)
                    {
                        Game.BattleSuccess = EBattleResult.ALIVE;
                    }
                    else
                    {
                        Game.BattleSuccess = EBattleResult.DEAD;
                    }
                }                
            }

            public override void OnTickGame()
            {
                base.OnTickGame();

                if (Time.tickCounter() % 300 == 0)
                {
                    Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Update waypoint positions.", null);

                    if (Game.gpArmies() != null && Game.gpArmies().Length > 0)
                    {
                        foreach (int armyIndex in Game.gpArmies())
                        {
                            if (Game.gpAirGroups(armyIndex) != null && Game.gpAirGroups(armyIndex).Length > 0)
                            {
                                foreach (AiAirGroup aiAirGroup in Game.gpAirGroups(armyIndex))
                                {
                                    foreach (AiWayPoint aiWayPoint in aiAirGroup.GetWay())
                                    {
                                        if (aiWayPoint is AiAirWayPoint)
                                        {
                                            AiAirWayPoint aiAirWayPoint = aiWayPoint as AiAirWayPoint;
                                            if (aiAirWayPoint.Action == AiAirWayPointType.AATTACK_BOMBERS
                                                || aiAirWayPoint.Action == AiAirWayPointType.AATTACK_BOMBERS
                                                || aiAirWayPoint.Action == AiAirWayPointType.GATTACK_TARG)
                                            {
                                                if (aiAirWayPoint.Target != null && aiAirWayPoint.Target.IsValid() && aiAirWayPoint.Target.IsAlive())
                                                {
                                                    aiAirWayPoint.P = aiAirWayPoint.Target.Pos();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }                    
                }
            }
        }
    }
}