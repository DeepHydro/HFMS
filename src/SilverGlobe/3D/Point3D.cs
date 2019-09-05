// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using System.Windows;

namespace SilverGlobe.Math3D
{
    /// <summary>
    /// Represents a point in 3D space.
    /// </summary>
    public struct Point3D : IEquatable<Point3D>
    {
        #region Members

        private Double _x, _y, _z;

        #endregion

        #region Properties

        public Double X 
        {
            get { return _x; }
            set { _x = value; }
        }

        public Double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public Double Z
        {
            get { return _z; }
            set { _z = value; }
        }

        #endregion

        #region Constructor

        public Point3D(Double x, Double y, Double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the 2D part of the 3D point.
        /// </summary>
        public Point ToPoint()
        {
            return new Point(_x, _y);
        }

        #endregion

        #region Equality

        public override Boolean Equals(object o)
        {
            if (!(o is Point3D)) return false;

            return Equals((Point3D)o);
        }

        public Boolean Equals(Point3D p)
        {
            return Equals(this, p);
        }

        public static Boolean Equals(Point3D a, Point3D b)
        {
            return a._x == b._x && a._y == b._y && a._z == b._z;
        }

        public override Int32 GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        #endregion

        #region Operators

        public static Vector3D operator -(Point3D a, Point3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3D operator +(Point3D p, Vector3D v)
        {
            return new Point3D(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        }

        public static Point3D operator *(Point3D p, Double k)
        {
            return new Point3D(p._x * k, p._y * k, p._z * k);
        }

        public static Boolean operator ==(Point3D a, Point3D b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static Boolean operator !=(Point3D a, Point3D b)
        {
            return !(a == b);
        }

        #endregion

    }
}
