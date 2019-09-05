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
    /// Represents a vector in 2D space.
    /// </summary>
    public struct Vector : IEquatable<Vector>
    {
        #region Members

        private Double _x, _y;

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

        /// <summary>
        /// The length of the vector.
        /// </summary>
        public Double Length
        {
            get { return Math.Sqrt(_x * _x + _y * _y); }
        }

        /// <summary>
        /// The normal vector of the given vector.
        /// </summary>
        public Vector Normalized
        {
            get
            {
                Double length = Length;
                return length > 0 ? this / length : this;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// The null vector.
        /// </summary>
        public static readonly Vector Empty = new Vector();

        #endregion

        #region Constructors

        public Vector(Point p) : this(p.X,p.Y)
        {            
        }

        public Vector(Double x, Double y)
        {
            _x = x;
            _y = y;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rotate the vector by a given angle.
        /// </summary>
        public Vector Rotate(Double angle)
        {
            return new Vector( Math.Cos(angle)*_x - Math.Sin(angle)*_y,
                               Math.Sin(angle)*_x + Math.Cos(angle)*_y );
        }

        #endregion
       
        #region Equality

        public override Boolean Equals(object o)
        {
            if (!(o is Vector)) return false;

            return Equals((Vector)o);
        }

        public Boolean Equals(Vector v)
        {
            return Equals(this, v);
        }

        public static Boolean Equals(Vector a, Vector b)
        {
            return a._x == b._x && a._y == b._y;
        }

        public override Int32 GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode();
        }

        #endregion
   
        #region Operators

        public static Vector operator /(Vector vector, Double scalar)
        {
            return vector * (1d / scalar);
        }

        public static Vector operator *(Vector vector, Double scalar)
        {
            return new Vector(vector._x * scalar, vector._y * scalar);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Boolean operator ==(Vector a, Vector b)
        {
            return a._x == b._x && a._y == b._y;
        }

        public static Boolean operator !=(Vector a, Vector b)
        {
            return a._x != b._x || a._y != b._y;
        }

        #endregion

     }
    
}
