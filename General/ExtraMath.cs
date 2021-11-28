using System;
using Microsoft.Xna.Framework;

namespace Boids.General
{
    public class ExtraMath
    {
        public static double PointDistance(Vector2 point1, Vector2 point2)
        {
            double px = point1.X - point2.X;
            double py = point1.Y - point2.Y;
            return Math.Sqrt(px * px + py * py);
        }
        
        public static double AngleDifference(double angle1, double angle2)
        {
            if (angle1 > Math.PI)
            {
                angle1 -= Math.PI*2;
            }
            if (angle2 > Math.PI)
            {
                angle2 -= Math.PI*2;
            }

            double angle3 = angle1 - angle2;

            if (angle3 < -Math.PI)
            {
                angle3 += Math.PI*2;
            }
            
            return angle3;
            // return ((((angle1 - angle2) % Math.PI*2) + Math.PI*3) % Math.PI*2) - Math.PI;
            // return ((((angle1 - angle2) % 360) + 540) % 360) - 180;
            //         same            Math.PI*2 Math.PI*3 Math.PI*2 Math.PI
        }
    }
}