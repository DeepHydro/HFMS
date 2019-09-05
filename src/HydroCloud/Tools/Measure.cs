using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Samples.Extensions;

namespace ESRI.ArcGIS.Samples
{
    public class Measure
    {
        private bool isActivated;
        int lineCount = 0;
        int _graphicCount = 0;
        double totalLength = 0;
        double _segmentLength = 0;
        double _tempTotalLength = 0;
        MapPoint originPoint;
        MapPoint endPoint;
        ESRI.ArcGIS.Client.Geometry.PointCollection _points;
        Point lastClick;
        SimpleMarkerSymbol markerSymbol;
        SimpleMarkerSymbol endSymbol;
        bool _isMeasuring = false;
        List<double> _lengths;

		public Measure()
		{
			//Set up defaults
			this.MapUnits = ScaleBarUnit.DecimalDegrees;
			this.NumberDecimals = 2;
			this.DistanceUnits = ScaleBarUnit.Kilometers;
			this.AreaUnits = AreaUnit.SqKm;
			this.Type = MeasureType.Distance;
			GraphicsLayer = new GraphicsLayer();
			LineSymbol = new SimpleLineSymbol()
			{
				Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
				Width = 2,
				Style = SimpleLineSymbol.LineStyle.Solid
			};
			FillSymbol = new SimpleFillSymbol()
			{
				Fill = new SolidColorBrush(Color.FromArgb(0x22, 255, 255, 255)),
				BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
				BorderThickness = 2
			};
			markerSymbol = new SimpleMarkerSymbol()
			{
				Color = new SolidColorBrush(Color.FromArgb(0x66, 255, 0, 0)),
				Size = 5,
				Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
			};
			endSymbol = new SimpleMarkerSymbol()
			{
				Color = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),
				Size = 5,
				Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
			};
			_lengths = new List<double>();
			_points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
		}


		#region Properties

		public ScaleBarUnit MapUnits { get; set; }
		public double NumberDecimals { get; set; }
		public Map Map { get; set; }
		private GraphicsLayer GraphicsLayer { get; set; }
		public MeasureType Type { get; set; }
		public double TotalLength { get; set; }
		public double TotalArea { get; set; }
		public AreaUnit AreaUnits { get; set; }
		public ScaleBarUnit DistanceUnits { get; set; }
		public FillSymbol FillSymbol { get; set; }
		public LineSymbol LineSymbol { get; set; }
        public bool IsActivated
        {
            get { return isActivated; }
            set
            {
                if (isActivated != value)
                {
                    isActivated = value;
                    if (value)
                    {
                        Map.MouseMove += map_MouseMove;
						Map.MouseLeftButtonDown += map_MouseLeftButtonDown;

						Map.Layers.Add(GraphicsLayer);
						Map.Cursor = Cursors.Hand;
                    }
                    else
                    {
						Map.Cursor = Cursors.Arrow;
						Map.MouseMove -= map_MouseMove;
						Map.MouseLeftButtonDown -= map_MouseLeftButtonDown;
						Map.Layers.Remove(GraphicsLayer);
                        ResetValues();
                    }
                }
            }
		}

		#endregion

		public void ResetValues()
        {
            _isMeasuring = false;
            originPoint = null;
            endPoint = null;
            lineCount = 0;
            _points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            _lengths = new List<double>();
            totalLength = 0;
            _tempTotalLength = 0;
            _segmentLength = 0;
        }

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Point pt = e.GetPosition(null);
            if (Math.Abs(pt.X - lastClick.X) < 2 && Math.Abs(pt.Y - lastClick.Y) < 2)
            {
                int lastone = GraphicsLayer.Graphics.Count - 1;
                GraphicsLayer.Graphics.RemoveAt(lastone);
                if (Type == MeasureType.Area)
                {
                    ESRI.ArcGIS.Client.Geometry.Polygon poly1 = GraphicsLayer.Graphics[0].Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                    MapPoint firstpoint = poly1.Rings[0][0];
                    poly1.Rings[0].Add(new MapPoint(firstpoint.X, firstpoint.Y));
                    GraphicsLayer.Graphics[0].Geometry = poly1;
                }
                ResetValues();
            }
            else
            {
                if (_points.Count == 0)
                {
                    GraphicsLayer.Graphics.Clear();
                    if (Type == MeasureType.Area)
                    {
                        Graphic areaGraphic = new Graphic()
                        {
                            Symbol = FillSymbol
                        };

                        GraphicsLayer.Graphics.Add(areaGraphic);
                        Graphic areaTotalGraphic = new Graphic()
                        {
                            Symbol = new RotatingTextSymbol()
                        };
                        GraphicsLayer.Graphics.Add(areaTotalGraphic);
                    }
                }
				originPoint = Map.ScreenToMap(e.GetPosition(Map));
				endPoint = Map.ScreenToMap(e.GetPosition(Map));
                ESRI.ArcGIS.Client.Geometry.Polyline line = new ESRI.ArcGIS.Client.Geometry.Polyline();
                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                points.Add(originPoint);
                points.Add(endPoint);
                line.Paths.Add(points);
                _points.Add(endPoint);
                if (_points.Count == 2)
                    _points.Add(endPoint);
                lineCount++;
				if (Type == MeasureType.Area && _points.Count > 2)
                {
                    ESRI.ArcGIS.Client.Geometry.Polygon poly = new ESRI.ArcGIS.Client.Geometry.Polygon();
                    poly.Rings.Add(_points);
                    GraphicsLayer.Graphics[0].Geometry = poly;
                }
				if (Type == MeasureType.Distance)
                {
                    Graphic totalTextGraphic = new Graphic()
                    {
                        Geometry = originPoint,
                        Symbol = new RotatingTextSymbol()
                    };
                    GraphicsLayer.Graphics.Add(totalTextGraphic);

                }
                Graphic marker = new Graphic()
                {
                    Geometry = endPoint,
                    Symbol = markerSymbol
                };
                GraphicsLayer.Graphics.Add(marker);
                Graphic lineGraphic = new Graphic()
                {
                    Geometry = line,
                    Symbol = LineSymbol
                };
                GraphicsLayer.Graphics.Add(lineGraphic);
                Graphic textGraphic = new Graphic()
                {
                    Geometry = endPoint,
                    Symbol = new RotatingTextSymbol()
                };
				textGraphic.SetZIndex(1);
                GraphicsLayer.Graphics.Add(textGraphic);
                totalLength += _segmentLength;
                _lengths.Add(_segmentLength);
                _segmentLength = 0;
                _isMeasuring = true;
            }
            lastClick = pt;
        }


        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            Map.Cursor = Cursors.Hand; 
            if (originPoint != null && _isMeasuring)
            {
                _graphicCount = GraphicsLayer.Graphics.Count;
                int g = _graphicCount - 1;
				MapPoint p = Map.ScreenToMap(e.GetPosition(Map));
                MapPoint midpoint = new MapPoint((p.X + originPoint.X) / 2, (p.Y + originPoint.Y) / 2);
                ESRI.ArcGIS.Client.Geometry.PointCollection polypoints = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                ESRI.ArcGIS.Client.Geometry.Polygon poly = new ESRI.ArcGIS.Client.Geometry.Polygon();
				if (Type == MeasureType.Area && _points.Count > 2)
                {
                    Graphic graphic = GraphicsLayer.Graphics[0];
                    poly = graphic.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                    polypoints = poly.Rings[0];
                    int lastPt = polypoints.Count - 1;
                    polypoints[lastPt] = p;
                }
                GraphicsLayer.Graphics[g - 2].Geometry = midpoint;
                (GraphicsLayer.Graphics[g - 1].Geometry as ESRI.ArcGIS.Client.Geometry.Polyline).Paths[0][1] = p;
                GraphicsLayer.Graphics[g].Geometry = midpoint;
                double angle = Math.Atan2((p.X - originPoint.X), (p.Y - originPoint.Y)) / Math.PI * 180 - 90;
                if (angle > 90 || angle < -90) angle -= 180;
                RotatingTextSymbol symb = GraphicsLayer.Graphics[g].Symbol as RotatingTextSymbol;
                symb.Angle = angle;

                double dist = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetSphericalDistance(originPoint, p);
                double distRound = RoundToSignificantDigit(dist);
                symb.Text = Convert.ToString(RoundToSignificantDigit(ConvertDistance(dist,DistanceUnits)));
                GraphicsLayer.Graphics[g].Symbol = symb;
                _segmentLength = distRound;
                _tempTotalLength = totalLength + distRound;
                RotatingTextSymbol totSym;
				if (Type == MeasureType.Distance)
                {
                    totSym = GraphicsLayer.Graphics[0].Symbol as RotatingTextSymbol;
                    totSym.Text = string.Format("Length Total:\n{0} {1}", RoundToSignificantDigit(ConvertDistance(_tempTotalLength, DistanceUnits)), DistanceUnits.ToString());
                    GraphicsLayer.Graphics[0].Symbol = totSym;
                }
                else
                {
                    totSym = GraphicsLayer.Graphics[1].Symbol as RotatingTextSymbol;
                    if (polypoints != null && polypoints.Count > 2)
                    {
                        double lastLen = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetSphericalDistance(polypoints[0], polypoints[polypoints.Count - 1]);
                        poly = GraphicsLayer.Graphics[0].Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                        MapPoint anchor = poly.Extent.GetCenter();
                        ESRI.ArcGIS.Client.Geometry.PointCollection temppoints = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                        foreach (MapPoint point in poly.Rings[0])
                        {
                            temppoints.Add(point);
                        }
                        temppoints.Add(poly.Rings[0][0]);
                        ESRI.ArcGIS.Client.Geometry.PointCollection pts = ToKmMapPoint(temppoints);
                        double area = GetArea(pts);
                        //double area = GetArea(ToKMMapPoint(polypoints));
                        totSym.Text = string.Format("Area:\n{0} {1}", RoundToSignificantDigit(area), AreaUnits.ToString());
                        GraphicsLayer.Graphics[1].Geometry = anchor;
                        GraphicsLayer.Graphics[1].Symbol = totSym;
                    }
                }
            }

        }
        private double RoundToSignificantDigit(double value)
        {
            double round = Math.Floor(-Math.Log(Map.Resolution * 111.1));
            if (round > 0)
            {
                round = Math.Pow(10, round);
                return Math.Round(value * round) / round;
            }
            else { return Math.Round(value); }

        }

        public double ConvertDistance(double distance, ScaleBarUnit toUnits)
        {
            double mDistance = distance;
            if (toUnits == ScaleBarUnit.Miles)
            {
                mDistance = distance / 1.60934;
            }
            else if (toUnits == ScaleBarUnit.Feet)
            {
                mDistance = distance * 3000.280839895;
            }
            else if (toUnits == ScaleBarUnit.Meters)
            {
                mDistance = distance * 1000;
            }
            return mDistance;
        }

        private double ConvertAreaUnits(double area, AreaUnit toUnits)
        {
            double mArea = area;
            if (toUnits == AreaUnit.Acres)
                mArea = area * 247.1054;
            else if (toUnits == AreaUnit.SqMi)
                mArea = area * 0.3861;
            else if (toUnits == AreaUnit.SqM)
                mArea = area * 1000000;
            else if (toUnits == AreaUnit.SqFt)
                mArea = area * 10763910.41671;
            else if (toUnits == AreaUnit.Hect)
                mArea = area * 0.01;

            return mArea;
        }

        private ESRI.ArcGIS.Client.Geometry.PointCollection ToKmMapPoint(ESRI.ArcGIS.Client.Geometry.PointCollection points)
        {
            ESRI.ArcGIS.Client.Geometry.PointCollection pts = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            MapPoint pt = points[0];
            if (MapUnits == ScaleBarUnit.DecimalDegrees)
            {
                foreach (MapPoint point in points)
                {
                    double x = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetSphericalDistance(point, new MapPoint(pt.X, point.Y));
                    double y = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetSphericalDistance(point, new MapPoint(point.X, pt.Y));
                    pts.Add(new MapPoint(x, y));
                }
                return pts;
            }
            else
                return points;
        }

        private double GetArea(ESRI.ArcGIS.Client.Geometry.PointCollection points)
        {
            double tempArea = 0;
            double xDiff, yDiff, x1, x2, y1, y2;
            for (int i = 0; i < points.Count - 1; i++)
            {
                x1 = points[i].X;
                x2 = points[i + 1].X;
                y1 = points[i].Y;
                y2 = points[i + 1].Y;
                xDiff = x2 - x1;
                yDiff = y2 - y1;
                //tempArea += x1 * yDiff - y1 * xDiff;
                tempArea += (x1 + x2) * (y1 - y2);

            }

            return Math.Abs(tempArea) / 2;

        }
    }

    public enum AreaUnit : int
    {
        Undefined = -1,
        SqMi = 1,
        Acres = 2,
        SqKm = 3,
        SqFt = 4,
        SqM = 5,
        Hect = 6
    }

    public enum MeasureType : int
    {
        Distance = 1,
        Area = 2
    }
}
