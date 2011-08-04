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
            public override void OnMissionLoaded(int missionNumber)
            {
                base.OnMissionLoaded(missionNumber);

                //if(GamePlay.gpArmies() != null && GamePlay.gpArmies().Length > 0)
                //{
                //    foreach (int armyIndex in GamePlay.gpArmies())
                //    {
                //        if (GamePlay.gpGroundGroups(armyIndex) != null && GamePlay.gpGroundGroups(armyIndex).Length > 0)
                //        {
                //            foreach (AiGroundGroup aiGroundGroup in GamePlay.gpGroundGroups(armyIndex))
                //            {
                //                if (aiGroundGroup.GetWay() != null && aiGroundGroup.GetWay().Length > 0)
                //                {
                //                    Point3d startPoint;
                //                    Point3d endPoint;
                //                    aiGroundGroup.GetPos(out startPoint);
                //                    AiWayPoint waypoint = aiGroundGroup.GetWay()[aiGroundGroup.GetWay().Length - 1];
                //                    endPoint = waypoint.P;

                //                    IRecalcPathParams pathParams = GamePlay.gpFindPath(new Point2d(startPoint.x, startPoint.y), 1.0, new Point2d(endPoint.x, endPoint.y), 1.0, PathType.GROUND, armyIndex);
                //                    aiGroundGroup.SetWay(pathParams.Path);
                //                }
                //            }
                //        }
                //    }
                //}                
            }
        }
    }
}