using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Samples.Extensions;

namespace ESRI.ArcGIS.Samples
{
	public class GeodeticDistance : DependencyObject
	{
		private Cursor mapCursor;
		MapPoint origin;
		GraphicsLayer layer;
		RotatingTextSymbol textSymb = new RotatingTextSymbol();
		Graphic midMarker = new Graphic() { Symbol = new SimpleMarkerSymbol() { Color = new SolidColorBrush(Color.FromArgb(0x66, 255, 0, 0)), Size=5 } };
		Graphic straightLine = new Graphic() { Symbol = new SimpleLineSymbol(Color.FromArgb(0x99,0,0,0), 1), };
		Graphic textPoint;
		Graphic greatCircleLine = new Graphic() { Symbol = new SimpleLineSymbol(Color.FromArgb(0x99, 255, 0, 0), 4) };
		Graphic radiusLine = new Graphic() { Symbol = new SimpleLineSymbol(Color.FromArgb(0xff, 255, 255, 0), 1) };
		Graphic radiusFill = new Graphic() { Symbol = new SimpleFillSymbol() { Fill = new SolidColorBrush(Color.FromArgb(0x22, 255, 255, 255)), BorderThickness = 0 } };

		public GeodeticDistance()
		{
			

		}

		public Map Map
		{
			get { return (Map)GetValue(MapProperty); }
			set { SetValue(MapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MapProperty =
			DependencyProperty.Register("Map", typeof(Map), typeof(GeodeticDistance), new PropertyMetadata(null, OnMapPropertyChanged));

		private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GeodeticDistance gd = d as GeodeticDistance;
			Map oldMap = e.OldValue as Map;
			bool isActive = gd.IsActivated;
			if (isActive)
				gd.IsActivated = false;
			if (oldMap != null)
			{
				gd.layer.ClearGraphics();
				
			}
			Map map = e.NewValue as Map;
			if (map != null)
			{
				//Create graphics layer and populate with the needed necessary graphics
				gd.layer = new GraphicsLayer();
				gd.layer.Graphics.Add(gd.radiusFill);

				gd.layer.Graphics.Add(gd.greatCircleLine);

				gd.layer.Graphics.Add(gd.radiusLine);

				Polyline line = new Polyline();
				ESRI.ArcGIS.Client.Geometry.PointCollection pnts = new ESRI.ArcGIS.Client.Geometry.PointCollection();
				pnts.Add(new MapPoint());
				pnts.Add(new MapPoint());
				line.Paths.Add(pnts);
				gd.straightLine.Geometry = line;

				gd.layer.Graphics.Add(gd.straightLine);

				gd.layer.Graphics.Add(gd.midMarker);

				gd.textPoint = new Graphic() { Symbol = gd.textSymb };
				gd.textPoint.SetZIndex(2);
				gd.layer.Graphics.Add(gd.textPoint);
			}
			gd.IsActivated = isActive;
		}

		public bool IsActivated
		{
			get { return (bool)GetValue(IsActivatedProperty); }
			set { SetValue(IsActivatedProperty, value); }
		}

		public static readonly DependencyProperty IsActivatedProperty =
			DependencyProperty.Register("IsActivated", typeof(bool), typeof(GeodeticDistance), new PropertyMetadata(false, OnIsActivatedPropertyChanged));

		private static void OnIsActivatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GeodeticDistance gd = d as GeodeticDistance;
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;
			if (newValue != oldValue)
			{
				if (gd.Map != null)
				{
					if (newValue)
					{
						gd.mapCursor = gd.Map.Cursor;
						gd.Map.MouseMove += gd.map_MouseMove;
						gd.Map.MouseClick += gd.map_MouseClick;
						gd.Map.Layers.Add(gd.layer);
						gd.Map.Cursor = Cursors.Hand;
					}
					else
					{
						gd.Map.MouseMove -= gd.map_MouseMove;
						gd.Map.MouseClick -= gd.map_MouseClick;
						gd.Map.Layers.Remove(gd.layer);
						gd.Map.Cursor = gd.mapCursor; //restore cursor
					}
				}
			}
		}

		private void map_MouseClick(object sender, Map.MouseEventArgs e)
		{
			origin = e.MapPoint;
			midMarker.Geometry = origin;
			(straightLine.Geometry as Polyline).Paths[0][0] = origin;
			(straightLine.Geometry as Polyline).Paths[0][1] = origin;
			textSymb.Text = "";
			greatCircleLine.Geometry = null;
			radiusLine.Geometry = null;
			radiusFill.Geometry = null;
		}

		private void map_MouseMove(object sender, MouseEventArgs e)
		{
			if (origin == null) return;
			Map map = sender as Map;
			MapPoint p = map.ScreenToMap(e.GetPosition(map));
			if (p == null || p.X < -180 || p.X > 180 || p.Y < -90 || p.Y > 90) return;
			MapPoint midpoint = new MapPoint((p.X + origin.X) / 2, (p.Y + origin.Y) / 2);
			textPoint.Geometry = midpoint;
			midMarker.Geometry = midpoint;
			(straightLine.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline).Paths[0][1] = p;
			//layer.Graphics[2].Geometry = midpoint;
			double angle = Math.Atan2((p.X - origin.X), (p.Y - origin.Y)) / Math.PI * 180 - 90;
			if (angle > 90 || angle < -90) angle -= 180;
			textSymb.Angle = angle;

			double dist = Math.Pow(p.X - origin.X, 2) + Math.Pow(p.Y - origin.Y, 2);
			dist = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetSphericalDistance(origin, p);
			textSymb.Text = string.Format("{0} km", RoundToSignificantDigit(dist, map.Resolution));

			greatCircleLine.Geometry = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetGeodesicLine(origin, p);
			radiusLine.Geometry = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetRadius(origin, dist);
			radiusFill.Geometry = ESRI.ArcGIS.Samples.Extensions.Geodesic.GetRadiusAsPolygon(origin, dist);

		}

		private static double RoundToSignificantDigit(double value, double resolution)
		{
			double round = Math.Floor(-Math.Log(resolution * 111.1));
			if (round > 0)
			{
				round = Math.Pow(10, round);
				return Math.Round(value * round) / round;
			}
			else { return Math.Round(value); }

		}
	}
}
