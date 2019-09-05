// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

#define CLIP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;
using SilverGlobe.Data;
using SilverGlobe.Math3D;
using System.Windows.Input;

namespace SilverGlobe
{
    public partial class Globe : Canvas, IPositionProvider
    {
        #region Members

        private Quaternion _orientation;

        private Double _radius = 0.5;
        private Double _zDistance = 1.5;
        private Double _fieldOfView = 35 * Math.PI / 180d;

        private Double _screenRadius;
        private Double _clipZ;
        private Matrix3D _projection;
        private Matrix3D _rotation;

        private IDictionary<String, PathGeometry> _continentGeometry;
        private ContinentShape[] _continents;

        #endregion

        #region Constructor

        public Globe()
        {
            InitializeComponent();

            Cursor = Cursors.Stylus;

            _orientation = Quaternion.ForRotation(Vector3D.YAxis, 0)
                         * Quaternion.ForRotation(Vector3D.XAxis, 0);

            UpdateMatrix();
            UpdateDisplayRadius();
        }

        #endregion

        #region Properties

        public Quaternion Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public Double ScreenRadius
        {
            get { return _screenRadius; }
        }

        public Double ZDistance
        {
            get { return _zDistance; }
            set
            {
                _zDistance = value;

                UpdateMatrix();
                UpdateDisplayRadius();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the globe with the geographical shapes.
        /// </summary>
        public void InitShapes(params ContinentShape[] continents)
        {
            if (continents == null)
                throw new ArgumentNullException("continents");

            _continents = continents;
            _continentGeometry = new Dictionary<String, PathGeometry>(continents.Length);

            foreach (ContinentShape c in continents)
            {
                PathGeometry pathData = new PathGeometry
                {
                    FillRule = FillRule.Nonzero,
                    Figures = new PathFigureCollection()
                };

                for (int i = 0; i < c.Shapes.Count; i++)
                {
                    PathFigure figure = new PathFigure
                    {
                        IsClosed = true,
                        Segments = new PathSegmentCollection()
                    };
                    figure.Segments.Add(new PolyLineSegment());

                    pathData.Figures.Add(figure);
                }

                Path p = (Path)_shapes.FindName(c.Name);
                p.Data = pathData;

                _continentGeometry.Add(c.Name, pathData);
            }
        }

        /// <summary>
        /// Update the visual display of the globe.
        /// </summary>
        public void Update()
        {
            _rotation = _orientation.ToMatrix();

            for (int i = 0; i < _continents.Length; i++)
            {
                // continent data
                ContinentShape c = _continents[i];

                // xaml element for the continent
                PathGeometry geometry = _continentGeometry[c.Name];
                var o = geometry.Figures;

                // setup rotation matrix
                Matrix3D rotation = Matrix3D.Identity
                                            .Scale(new Vector3D(1, 1, 1) * _radius)
                                            * _rotation;

                // render the continent shapes
                for (int j = 0; j < c.Shapes.Count; j++)
                {
                    Point3D[] shapePoints = c.Shapes[j];


                    // do 3D projection and clipping
                    Point[] renderPoints = shapePoints.Transform(rotation)          // rotate
                                                      .Clip(_radius, _clipZ)        // clip
                                                      .Transform(_projection)       // project
                                                      .Select(p3 => p3.ToPoint())   // convert
                                                      .ToArray();

                    // set the path geometry
                    PathFigure figure = (PathFigure)geometry.Figures[j];
                    PolyLineSegment segment = (PolyLineSegment)figure.Segments[0];

                    PointCollection segmentPoints = new PointCollection();

                    if (renderPoints.Length > 1)
                    {
                        figure.StartPoint = renderPoints[0];

                        for (int k=1; k<renderPoints.Length; k++)
                        {
                            segmentPoints.Add(renderPoints[k]);
                        }

                        segment.Points = segmentPoints;
                    }

                    segment.Points = segmentPoints; 
                }
            }
        }

        /// <summary>
        /// Calculate the position of a geographical position for the current state of the globe.
        /// </summary>
        Point3D IPositionProvider.ProjectPosition(GeoPosition geoPos)
        {
            Point3D p = geoPos.ToSphere(_radius * 1.02)
                              .Transform(_rotation);

            Double z = p.Z;
            p = p.Transform(_projection);

            return new Point3D(p.X, p.Y, z > _clipZ ? 1 : -1);
        }

        #endregion

        #region Implementation

        private void UpdateMatrix()
        {
            Matrix3D projection = new Matrix3D
            {
                M11 = Math.PI * 0.5 - _fieldOfView,
                M22 = Math.PI * 0.5 - _fieldOfView,
                M33 = 1d,
                M34 = 1d,
                OffsetZ = 0
            };

            _projection = Matrix3D.Identity
                                   .Translate(new Vector3D(0, 0, -_zDistance))
                                    * projection
                                   .Translate(new Vector3D(0.5, 0.5, 0))
                                   .Scale(new Vector3D(160, 160, 1));
        }

        private void UpdateDisplayRadius()
        {
            Double alpha = Math.Asin(_radius / _zDistance);
            Point3D p = new Point3D(_radius * Math.Cos(alpha), 0, _radius * Math.Sin(alpha));

            Point3D topLeft = _projection * p;
            _screenRadius = 80d - topLeft.X;
            _clipZ = p.Z;
#if CLIP
            _clip.RadiusX = _screenRadius;
            _clip.RadiusY = _screenRadius;
#endif

            _glossScale.ScaleX = _glossScale.ScaleY = _screenRadius / 80d;
        }

        #endregion

    }
}
