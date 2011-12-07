// Copyright 2010 Giovanni Botta

// This file is part of GeomClone.

// GeomClone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// GeomClone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeomClone.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameEngine.Helpers
{
    static class VectorHelper
    {
        public static Vector2 PointToWorldSpace(Vector2 point, Vector2 AgentHeading, Vector2 AgentSide, Vector2 AgentPosition)
        {
            //make a copy of the point            
            //Vector3 TransPoint = new Vector3(point, 0);

            //Matrix matTransform = Matrix.Identity;

            //if (AgentHeading.LengthSquared() > 0)
            //    matTransform = GetRotation(AgentHeading, AgentSide);

            ////and translate
            //matTransform *= Matrix.CreateTranslation(AgentPosition.X, AgentPosition.Y, 0);

            ////now transform the vertices
            //TransPoint = Vector3.Transform(TransPoint, matTransform);

            //return new Vector2(TransPoint.X, TransPoint.Y);

            return AgentPosition + AgentHeading * point.X + AgentSide * point.Y;
        }

        //public static Matrix GetRotation(Vector2 fwd,Vector2 side){
        //    Matrix rot = Matrix.Identity;

        //    rot.M11 = fwd.X;
        //    rot.M12 = fwd.Y;
        //    rot.M21 = side.X;
        //    rot.M22 = side.Y;

        //    return rot;
        //}

        // returns 1 if v2 is clockwise to v1, -1 otherwise
        public static float Sign(Vector2 v1, Vector2 v2)
        {
            return Math.Sign(v1.Y * v2.X - v1.X * v2.Y);
        }
        public static Vector2 Perpendicular(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }
        public static Vector2 Rotate(Vector2 v,float angleDegrees)
        {
            double angleRad = Math.PI / 180.0 * angleDegrees;
            return new Vector2((float)(Math.Cos(angleRad) * v.X - Math.Sin(angleRad) * v.Y), (float)(Math.Sin(angleRad) * v.X + Math.Cos(angleRad) * v.Y));

        }


        internal static bool LineIntersection2D(Vector2 vector2, Vector2 f, Vector2 vector2_3, Vector2 vector2_4, out float distToThisIP, out Vector2 point)
        {
            throw new NotImplementedException();
        }
    }
}
