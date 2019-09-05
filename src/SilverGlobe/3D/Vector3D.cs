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
    /// Represents a vector in 3D space.
    /// </summary>
    public struct Vector3D : IEquatable<Vector3D>
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

        /// <summary>
        /// The length of the vector.
        /// </summary>
        public Double Length
        {
            get { return Math.Sqrt(_x * _x + _y * _y + _z * _z); }
        }

        /// <summary>
        /// The normal vector of the given vector.
        /// </summary>
        public Vector3D Normalized
        {
            get
            {
                if (this == Vector3D.Empty) return Vector3D.Empty;

                return this / Length;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// The null vector.
        /// </summary>
        public static readonly Vector3D Empty = new Vector3D();

        /// <summary>
        /// The x-axis base vector.
        /// </summary>
        public static readonly Vector3D XAxis = new Vector3D(1, 0, 0);

        /// <summary>
        /// The y-axis base vector.
        /// </summary>
        public static readonly Vector3D YAxis = new Vector3D(0, 1, 0);

        /// <summary>
        /// The z-axis base vector.
        /// </summary>
        public static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);

        #endregion

        #region Constructor

        public Vector3D(Double x, Double y, Double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        #endregion

        #region Methods 

        /// <summary>
        /// The dot product with another vector
        /// </summary>
        public Double Dot(Vector3D b)
        {
            Vector3D a = this;
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        #endregion

        #region Equality

        public override Boolean Equals(object o)
        {
            if (!(o is Vector3D)) return false;

            return Equals((Vector3D)o);
        }

        public Boolean Equals(Vector3D v)
        {
            return Equals(this, v);
        }

        public static Boolean Equals(Vector3D a, Vector3D b)
        {
            return a._x == b._x && a._y == b._y && a._z == b._z;
        }

        public override Int32 GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        #endregion

        #region Operators

        public static Vector3D operator *(Vector3D vector, Double scalar)
        {
            return new Vector3D(vector._x * scalar, vector._y * scalar, vector._z * scalar);
        }

        public static Vector3D operator /(Vector3D vector, Double scalar)
        {
            return vector * (1d / scalar);
        }

        public static Boolean operator ==(Vector3D a, Vector3D b)
        {
            return a._x == b._x && a._y == b._y && a._z == b._z;
        }

        public static Boolean operator !=(Vector3D a, Vector3D b)
        {
            return a._x != b._x || a._y != b._y || a._z != b._z;
        }

        #endregion
    }
}
