// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

// Updated to Silverlight 2.0
// 1/2009
// Added Description


using System;
using System.Windows;

namespace SilverGlobe.Data
{
    /// <summary>
    /// Represents a geographical location with position, name and an arbitrary data object.
    /// </summary>
    public class Location
    {
        #region Members

        private String _name;

        private GeoPosition _position;

        private String _description;

        #endregion

        #region Properties

        public String Name
        {
            get { return _name; }
        }

        public GeoPosition Position
        {
            get { return _position; }
        }

        public Object Data
        {
            get;
            set;
        }

        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        #endregion

        #region Constructors

        public Location(String name, GeoPosition geoPosition) : this(name, geoPosition, null)
        {
        }

        public Location(String name, GeoPosition geoPosition, String description)
        {
            _name = name;
            _position = geoPosition;
            _description = Description;
        }

        public Location(String name, GeoPosition geoPosition, String description, Object data)
        {
            _name = name;
            _position = geoPosition;
            _description = Description;
            Data = data;
        }

        #endregion
    }
}
