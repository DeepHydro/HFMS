// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using SilverGlobe.Math3D;
using System.Collections.ObjectModel;

namespace SilverGlobe.Data
{
    /// <summary>
    /// A continent and it's shapes on the globe.
    /// </summary>
    public class ContinentShape
    {
        private String _name;
        private List<Point3D[]> _shapes;        

        public String Name
        {
            get { return _name; }
        }

        public ReadOnlyCollection<Point3D[]> Shapes
        {
            get { return _shapes.AsReadOnly(); } 
        }

        public ContinentShape(String name, IEnumerable<Point3D[]> shapes)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (shapes == null)
                throw new ArgumentNullException("shapes");

            _name = name;
            _shapes = new List<Point3D[]>(shapes);
        }
    }

    /// <summary>
    /// Static class that contains all shapes of all continents.
    /// </summary>
    public static class ContinentShapes
    {
        public static readonly ContinentShape Africa;
        public static readonly ContinentShape America;
        public static readonly ContinentShape Antarctica;
        public static readonly ContinentShape Australia;
        public static readonly ContinentShape Eurasia;

        static ContinentShapes()
        {
            Africa = new ContinentShape("Africa", new Point3D[][]
            {
                Coordinates.Africa.ToSphere(),
                Coordinates.Madagascar.ToSphere(),
            });

            America = new ContinentShape("America", new Point3D[][]
            {
               Coordinates.Northamerica.ToSphere(),
               Coordinates.Middleamerica.ToSphere(),
               Coordinates.Southamerica.ToSphere(),
               Coordinates.Caribean.ToSphere(),
               Coordinates.NorthCanada.ToSphere(),
            });

            Antarctica = new ContinentShape("Antarctica", new Point3D[][]
            {
                Coordinates.Antarctica.ToSphere(),
            });

            Australia = new ContinentShape("Australia", new Point3D[][]
            {
                Coordinates.Australia.ToSphere(),
                Coordinates.Tasmania.ToSphere(),
                Coordinates.NewZealand.ToSphere(),
                Coordinates.NewGuinea.ToSphere(),
            });

            Eurasia = new ContinentShape("Eurasia", new Point3D[][]
            {
                Coordinates.Greenland.ToSphere(),
                Coordinates.ContinentalEurope.ToSphere(),
                Coordinates.England.ToSphere(),
                Coordinates.Ireland.ToSphere(),
                Coordinates.Mediteranean.ToSphere(),
                Coordinates.ContinentalAsia.ToSphere(),
                Coordinates.Indonesia1.ToSphere(),
                Coordinates.Indonesia2.ToSphere(),
                Coordinates.Taiwan.ToSphere(),
                Coordinates.SriLanka.ToSphere(),
            });            
        }
    }
}
