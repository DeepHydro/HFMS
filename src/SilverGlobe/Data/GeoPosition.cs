// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using SilverGlobe.Math3D;

namespace SilverGlobe.Data
{
    /// <summary>
    /// Represents a geographical position with latitude and longitude.
    /// </summary>
    public struct GeoPosition
    {
        #region Members

        private Double _longitude;
        private  Double _latitude;

        #endregion

        #region Properties

        public Double Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }

        public Double Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        public String LongitudeString
        {
            get
            {
                Double absLong = Math.Abs(_longitude);

                Double deg = Math.Floor(absLong);
                Double min = Math.Floor(60 * (absLong - deg));

                return String.Format("{0:00}°{1:00}' {2}", deg, min, _longitude > 0 ? 'E' : 'W' );
            }
        }

        public String LatitudeString
        {
            get
            {
                Double absLat = Math.Abs(_latitude);

                Double deg = Math.Floor(absLat);
                Double min = Math.Floor(60 * (absLat - deg));

                return String.Format("{0:00}°{1:00}' {2}", deg, min, _latitude > 0 ? 'N' : 'S');
            }
        }

        public Quaternion GetQuaternion()
        {
            Quaternion q1 = Quaternion.ForRotation(Vector3D.XAxis, _latitude);
            Quaternion q2 = Quaternion.ForRotation(Vector3D.YAxis, _longitude - 90);

            return q2 * q1;
        }

        #endregion

        #region Constructor

        public GeoPosition(Double latitude, Double longitude)
        {
            _longitude = longitude;
            _latitude = latitude;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert a geographical position to a point on a 3D sphere with radius 1.
        /// </summary>
        public Point3D ToSphere()
        {
            return ToSphere(1);
        }

        /// <summary>
        /// Convert a geographical position to a point on a 3D sphere with given radius.
        /// </summary>
        public Point3D ToSphere(Double radius)
        {
            Double longitudeRad = Longitude * Math.PI / 180d;
            Double latitudeRad = Latitude * Math.PI / 180d;

            Double y = radius * Math.Sin(latitudeRad);
            Double r2 = radius * Math.Cos(latitudeRad);

            Double x = r2 * Math.Cos(longitudeRad);
            Double z = r2 * Math.Sin(longitudeRad);

            return new Point3D(x, y, z);                                
        }

        #endregion

        #region Equality

        public override Boolean Equals(object obj)
        {
            if (!(obj is GeoPosition)) return false;
            
            return Equals((GeoPosition)obj);
        }

        public Boolean Equals(GeoPosition pos2)
        {
            return _longitude == pos2._longitude && _latitude == pos2._latitude;
        }

        public static Boolean operator ==(GeoPosition a, GeoPosition b)
        {
            return a._latitude == b._latitude && a._longitude == b._longitude;
        }

        public static Boolean operator !=(GeoPosition a, GeoPosition b)
        {
            return !(a == b);
        }

        public override Int32 GetHashCode()
        {
            return Longitude.GetHashCode() ^ Latitude.GetHashCode();
        }

        #endregion
    }
}
