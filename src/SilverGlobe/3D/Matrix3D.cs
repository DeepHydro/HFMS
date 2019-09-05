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
    /// Represents a 4x4 matrix.
    /// </summary>
    public struct Matrix3D
    {
        #region Members

        private Double _m11, _m12, _m13, _m14, _m21, _m22, _m23, _m24, _m31, _m32, _m33, _m34, _offsetX, _offsetY, _offsetZ, _m44;

        #endregion

        #region Properties

        public Double M11 
        { 
            get { return _m11; } 
            set { _m11 = value; } 
        }

        public Double M12
        {
            get { return _m12; }
            set { _m12 = value; }
        }

        public Double M13
        {
            get { return _m13; }
            set { _m13 = value; }
        }

        public Double M14
        {
            get { return _m14; }
            set { _m14 = value; }
        }

        public Double M21
        {
            get { return _m21; }
            set { _m21 = value; }
        }

        public Double M22
        {
            get { return _m22; }
            set { _m22 = value; }
        }

        public Double M23
        {
            get { return _m23; }
            set { _m23 = value; }
        }

        public Double M24
        {
            get { return _m24; }
            set { _m24 = value; }
        }

        public Double M31
        {
            get { return _m31; }
            set { _m31 = value; }
        }

        public Double M32
        {
            get { return _m32; }
            set { _m32 = value; }
        }

        public Double M33
        {
            get { return _m33; }
            set { _m33 = value; }
        }

        public Double M34
        {
            get { return _m34; }
            set { _m34 = value; }
        }

        public Double OffsetX
        {
            get { return _offsetX; }
            set { _offsetX = value; }
        }

        public Double OffsetY
        {
            get { return _offsetY; }
            set { _offsetY = value; }
        }

        public Double OffsetZ
        {
            get { return _offsetZ; }
            set { _offsetZ = value; }
        }

        public Double M44
        {
            get { return _m44; }
            set { _m44 = value; }
        }

        #endregion

        #region Constancts

        /// <summary>
        /// The identity matrix.
        /// </summary>
        public static readonly Matrix3D Identity = new Matrix3D { M11 = 1, M22 = 1, M33 = 1, M44 = 1 };

        #endregion

        #region Constructor

        public Matrix3D(Double m11, Double m12, Double m13, Double m14,
                        Double m21, Double m22, Double m23, Double m24,
                        Double m31, Double m32, Double m33, Double m34,
                        Double offsetX, Double offsetY, Double offsetZ, Double m44)
        {
            _m11 = m11; _m12 = m12; _m13 = m13; _m14 = m14;
            _m21 = m21; _m22 = m22; _m23 = m23; _m24 = m24;
            _m31 = m31; _m32 = m32; _m33 = m33; _m34 = m34;
            _offsetX = offsetX; _offsetY = offsetY; _offsetZ = offsetZ; _m44 = m44;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a scale vector to the matrix.
        /// </summary>
        public Matrix3D Scale(Vector3D scale)
        {
            _m11 *= scale.X;
            _m12 *= scale.Y;
            _m13 *= scale.Z;

            _m21 *= scale.X;
            _m22 *= scale.Y;
            _m23 *= scale.Z;

            _m31 *= scale.X;
            _m32 *= scale.Y;
            _m33 *= scale.Z;

            _offsetX *= scale.X;
            _offsetY *= scale.Y;
            _offsetZ *= scale.Z;

            return this;
        }

        /// <summary>
        /// Add a translation to the matrix.
        /// </summary>
        public Matrix3D Translate(Vector3D offset)
        {
            M11 += M14 * offset.X;
            M12 += M14 * offset.Y;
            M13 += M14 * offset.Z;
            M21 += M24 * offset.X;
            M22 += M24 * offset.Y;
            M23 += M24 * offset.Z;
            M31 += M34 * offset.X;
            M32 += M34 * offset.Y;
            M33 += M34 * offset.Z;

            OffsetX += M44 * offset.X;
            OffsetY += M44 * offset.Y;
            OffsetZ += M44 * offset.Z;

            return this;
        }

        /// <summary>
        /// Multiplies a point array with the matrix. 
        /// </summary>
        public Point3D[] Transform(Point3D[] points)
        {
            Point3D[] outPoints = new Point3D[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                outPoints[i] = this * points[i];
            }

            return outPoints;
        }

        #endregion

        #region Operatiors

        public static Matrix3D operator *(Matrix3D a, Matrix3D b)
        {
            return new Matrix3D
                   (
                       a._m11 * b._m11 + a._m12 * b._m21 + a._m13 * b._m31 + a._m14 * b._offsetX,
                       a._m11 * b._m12 + a._m12 * b._m22 + a._m13 * b._m32 + a._m14 * b._offsetY,
                       a._m11 * b._m13 + a._m12 * b._m23 + a._m13 * b._m33 + a._m14 * b._offsetZ,
                       a._m11 * b._m14 + a._m12 * b._m24 + a._m13 * b._m34 + a._m14 * b._m44,
                       a._m21 * b._m11 + a._m22 * b._m21 + a._m23 * b._m31 + a._m24 * b._offsetX,
                       a._m21 * b._m12 + a._m22 * b._m22 + a._m23 * b._m32 + a._m24 * b._offsetY,
                       a._m21 * b._m13 + a._m22 * b._m23 + a._m23 * b._m33 + a._m24 * b._offsetZ,
                       a._m21 * b._m14 + a._m22 * b._m24 + a._m23 * b._m34 + a._m24 * b._m44,
                       a._m31 * b._m11 + a._m32 * b._m21 + a._m33 * b._m31 + a._m34 * b._offsetX,
                       a._m31 * b._m12 + a._m32 * b._m22 + a._m33 * b._m32 + a._m34 * b._offsetY,
                       a._m31 * b._m13 + a._m32 * b._m23 + a._m33 * b._m33 + a._m34 * b._offsetZ,
                       a._m31 * b._m14 + a._m32 * b._m24 + a._m33 * b._m34 + a._m34 * b._m44,
                       a._offsetX * b._m11 + a._offsetY * b._m21 + a._offsetZ * b._m31 + a._m44 * b._offsetX,
                       a._offsetX * b._m12 + a._offsetY * b._m22 + a._offsetZ * b._m32 + a._m44 * b._offsetY,
                       a._offsetX * b._m13 + a._offsetY * b._m23 + a._offsetZ * b._m33 + a._m44 * b._offsetZ,
                       a._offsetX * b._m14 + a._offsetY * b._m24 + a._offsetZ * b._m34 + a._m44 * b._m44
                   );
        }

        public static Point3D operator *(Matrix3D m, Point3D point)
        {
            Double x = point.X, y = point.Y, z = point.Z;

            Point3D p = new Point3D(x * m._m11 + y * m._m21 + z * m._m31 + m._offsetX,
                                    x * m._m12 + y * m._m22 + z * m._m32 + m._offsetY,
                                    x * m._m13 + y * m._m23 + z * m._m33 + m._offsetZ);

            Double k = x * m._m14 + y * m._m24 + z * m._m34 + m._m44;
            
            return p * (1 / k);
        }

        #endregion
    }
}
