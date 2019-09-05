// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;

namespace SilverGlobe.Math3D
{
    /// <summary>
    /// Extension methods for dealing with Point arrays inline.
    /// </summary>
    public static class Math3dExtensions
    {
        public static Point3D[] Transform(this Point3D[] points, Matrix3D matrix)
        {
            return matrix.Transform(points);
        }

        public static Point3D Transform(this Point3D point, Matrix3D matrix)
        {
            return matrix * point;
        }

        public static Point3D[] Clip(this Point3D[] points, Double sphereRadius, Double z)
        {
            return Clipping.ClipPolygon(points, sphereRadius, z);
        }

        public static Double Clamp(this Double d, Double min, Double max)
        {
            return d < min ? min : (d > max ? max : d);
        }        
    }
}
