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
    /// Represents a quaternion structure. 
    /// </summary>
    public struct Quaternion
    {
        #region Members

        private Double _w, _x, _y, _z;

        #endregion

        #region Properties

        public Double W 
        {
            get { return _w; }
            set { _w = value; }
        }

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
        /// Get the conjugate of the quaternion.
        /// </summary>
        public Quaternion Conjugate
        {
            get { return new Quaternion(_w, -_x, -_y, -_z); }
        }

        /// <summary>
        /// Get the axis of the quaternion rotation.
        /// </summary>
        public Vector3D Axis
        {
            get
            {
                Vector3D v = new Vector3D(_x, _y, _z).Normalized;

                if (v == Vector3D.Empty) v = Vector3D.YAxis;

                return v;
            }
        }

        #endregion

        #region Constructor

        public Quaternion(Double w, Double x, Double y, Double z) : this()
        {
            _w = w;
            _x = x;
            _y = y;
            _z = z;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a quaternion for an axis rotation.
        /// </summary>
        public static Quaternion ForRotation(Vector3D axis, Double angle)
        {
            Double a = 0.5 * angle * Math.PI / 180d;
            Vector3D v = axis * Math.Sin(a);

            return new Quaternion(Math.Cos(a), v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Calculate the inner product of two quaternions.
        /// </summary>
        public Double Dot(Quaternion b)
        {
            Quaternion a = this;

            return a._w * b._w + a._x * b._x + a._y * b._y + a._z * b._z;
        }

        /// <summary>
        /// Normalize the quaternion.
        /// </summary>
        public void Normalize()
        {
            Double length = Math.Sqrt(_w * _w + _z * _z + _y * _y + _x * _x);

            _w /= length;
            _x /= length;
            _y /= length;
            _z /= length;
        }

        /// <summary>
        /// Convert the quaternion to a matrix.
        /// </summary>
        public Matrix3D ToMatrix()
        {
            Quaternion q = this;

            Double w2 = q.W * q.W;
            Double x2 = q.X * q.X;
            Double y2 = q.Y * q.Y;
            Double z2 = q.Z * q.Z;

            Double xy_2 = 2 * q.X * q.Y;
            Double wz_2 = 2 * q.W * q.Z;
            Double wy_2 = 2 * q.W * q.Y;
            Double wx_2 = 2 * q.W * q.X;
            Double xz_2 = 2 * q.X * q.Z;
            Double yz_2 = 2 * q.Y * q.Z;

            return new Matrix3D
            (
                w2 + x2 - y2 - z2, xy_2 + wz_2, xz_2 - wy_2, 0,
                xy_2 - wz_2, w2 - x2 + y2 - z2, yz_2 + wx_2, 0,
                xz_2 + wy_2, yz_2 - wx_2, w2 - x2 - y2 + z2, 0,
                0, 0, 0, w2 + x2 + y2 + z2
            );
        }

        #endregion

        #region Equality

        public override Boolean Equals(object o)
        {
            if (!(o is Quaternion)) return false;

            return Equals((Quaternion)o);
        }

        public Boolean Equals(Quaternion q)
        {
            return Equals(this, q);
        }

        public static Boolean Equals(Quaternion q1, Quaternion q2)
        {
            return q1._w == q2._w && q1._x == q2._x && q1._y == q2._y && q1._z == q2._z;
        }

        public override Int32 GetHashCode()
        {
            return _w.GetHashCode() ^ _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        #endregion

        #region Operations
        
        public static Quaternion operator *(Quaternion q2, Quaternion q1)
        {
            return new Quaternion
                   (
                        q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z,
                        q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y,
                        q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z,
                        q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X                    
                   );
        }

        public static Quaternion operator *(Quaternion q, Double d)
        {
            return new Quaternion(q._w * d, q._x * d, q._y * d, q._z * d);
        }

        public static Quaternion operator +(Quaternion q1, Quaternion q2)
        {
            return new Quaternion
                   (
                        q1._w + q2._w,
                        q1._x + q2._x,
                        q1._y + q2._y,
                        q1._z + q2._z                    
                   );
        }

        public static Quaternion operator -(Quaternion q1, Quaternion q2)
        {
            return new Quaternion
                   (
                        q1._w - q2._w,
                        q1._x - q2._x,
                        q1._y - q2._y,
                        q1._z - q2._z
                   );
        }

        public static bool operator ==(Quaternion q1, Quaternion q2)
        {
            return q1.X == q2.X && q1.Y == q2.Y && q1.Z == q2.Z && q1.W == q2.W;
        }

        public static bool operator !=(Quaternion q1, Quaternion q2)
        {
            return (q1 != q2);
        }

        #endregion
    }
}
