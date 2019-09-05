// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;

namespace SilverGlobe.Math3D
{
    public enum Corner : byte
    {
        TopLeft = 0,
        TopRight = 1,
        BottomRight = 2,
        BottomLeft = 3
    }

    /// <summary>
    /// Helper class that adds methods for dealing with Corner enums
    /// </summary>
    public static class CornerExtensions
    {
        /// <summary>
        /// Maps a corner to a point.
        /// </summary>
        public static Point3D ToPoint(this Corner corner, Double z)
        {
            switch (corner)
            {
                case Corner.TopLeft:
                    return new Point3D(-1d, -1d, z);

                case Corner.BottomLeft:
                    return new Point3D(-1d, 1d, z);

                case Corner.TopRight:
                    return new Point3D(1d, -1d, z);

                case Corner.BottomRight:
                    return new Point3D(1d, 1d, z);
            }

            throw new ArgumentException(String.Format("Corner value not defined: {0}.", corner));
        }

        /// <summary>
        /// Finds the corner where a point lies in.
        /// </summary>
        public static Corner ToCorner(this Point3D point)
        {
            if (point.Y > 0)
            {
                return point.X > 0 ? Corner.BottomRight : Corner.BottomLeft;
            }
            else
            {
                return point.X > 0 ? Corner.TopRight : Corner.TopLeft;
            }
        }
    }
}
