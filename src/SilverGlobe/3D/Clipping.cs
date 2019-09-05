// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using System.Linq;
using System.Collections.Generic;

namespace SilverGlobe.Math3D
{   
    public static class Clipping
    {
        /// <summary>
        /// Clip a sequence of 3D points against the x/y plane, so that hidden points on the backside
        /// are hidden and lines between visible and invisible points appear correct.
        /// </summary>
        public static Point3D[] ClipPolygon(Point3D[] inputPoints, Double radius, Double clipZ)        
        {
            List<Point3D> clippedPoints = new List<Point3D>();
            
            Int32 len = inputPoints.Length;
            if (len < 3) return new Point3D[0];

            Point3D point, prevPoint;
            Boolean isPointInFront, isPrevPointInFront; 
            
            // find first point that in front of the clipping plane
            point = new Point3D();
            Corner outCorner = Corner.TopLeft, inCorner = Corner.TopLeft;
            int startIndex = -1;
            
            int i = 0;
            foreach (Point3D p in inputPoints)
            {
                if (p.Z >= clipZ)
                {
                    startIndex = i;

                    point = p;
                    prevPoint = inputPoints[(i + len -1) % len];                    
                    
                    inCorner = outCorner = p.ToCorner();
                    break;
                }

                i++;
            }

            // no point found => return empty list
            if (startIndex == -1) return new Point3D[0];

            // ---------------------------------------------------------------
            
            // set initial values
            isPointInFront = true;
            clippedPoints.Add(point);

            Double sliceRadius = Math.Sqrt(radius * radius - clipZ * clipZ);

            for (i = 1; i <= len; i++)
            {
                prevPoint = point;
                isPrevPointInFront = isPointInFront;

                int index = (startIndex + i) % len;
                point = inputPoints[index];

                isPointInFront = point.Z >= clipZ;

                if (isPointInFront)
                {
                    if (isPrevPointInFront)
                    {
                        clippedPoints.Add(point);
                    }
                    else
                    {
                        // out -> in 
                        
                        // add intersection point
                        Double f = (clipZ - prevPoint.Z) / (point.Z - prevPoint.Z);
                        Point3D p = prevPoint + (point - prevPoint) * f;
                        p = p.AlignToRadius(sliceRadius);

                        inCorner = p.ToCorner();
                        AddCornerSequence(clippedPoints, outCorner, inCorner, clipZ);

                        clippedPoints.Add(p);
                        clippedPoints.Add(point);                        
                    }
                }
                else
                {                    
                    if (isPrevPointInFront)
                    {
                        // in -> out

                        // add intersection point	
                        Double f = (clipZ - prevPoint.Z) / (point.Z - prevPoint.Z);
                        Point3D p = prevPoint + (point - prevPoint) * f;
                        p = p.AlignToRadius(sliceRadius);
                        
                        clippedPoints.Add(p);

                        outCorner = p.ToCorner();
                    }
                }
            }

            return clippedPoints.ToArray();
        }

        /// <summary>
        /// Project a point to the edge of a slice through the sphere.
        /// </summary>
        private static Point3D AlignToRadius(this Point3D p, Double radius)
        {
            Double angle = Math.Atan2(p.Y, p.X);

            return new Point3D(radius * Math.Cos(angle), radius * Math.Sin(angle), p.Z);
        }

        /// <summary>
        /// Find the shortest connection between two points that are clipped by viewplane.
        /// </summary>
        private static void AddCornerSequence(this ICollection<Point3D> clippedPoints, 
                                              Corner startCorner, Corner endCorner, 
                                              Double z)
        {
            byte a = (byte)startCorner;
            byte b = (byte)endCorner;

            if (b < a) b += 4;
            int d = b - a;

            Corner c;

            switch (d)
            {
                case 3:
                    clippedPoints.Add(startCorner.ToPoint(z));
                    c = (Corner)((a + 3) % 4);
                    clippedPoints.Add(c.ToPoint(z));
                    break;

                case 2:
                    // start
                    clippedPoints.Add(startCorner.ToPoint(z));
                    // middle
                    c = b > a ? (Corner)((a + 1) % 4) : (Corner)((a + 3) % 4);
                    clippedPoints.Add(c.ToPoint(z));
                    // end
                    clippedPoints.Add(endCorner.ToPoint(z));
                    break;

                case 1:
                    clippedPoints.Add(startCorner.ToPoint(z));
                    clippedPoints.Add(endCorner.ToPoint(z));
                    break;

                case 0:
                    clippedPoints.Add(startCorner.ToPoint(z));
                    break;
            }
        }

    }
}

