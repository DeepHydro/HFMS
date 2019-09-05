using System;
using System.Linq;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
//using HUST.WREIS.Geography;

namespace HydroCloud.Spatial
{
    public class GeometryTypeAdaptor
    {

        public static PointCollection ConvertToPointCollection(IList<MapPoint> points)
        {
            PointCollection collection = new PointCollection();

            foreach (var p in points)
            {
                collection.Add(p);
            }
            return collection;
        }

        public static Polygon ConvertToPolygon(IList<MapPoint> points)
        {
            Polygon ep = new Polygon();
            ep.Rings.Add(ConvertToPointCollection(points));
            return ep;
        }


        public static Polyline ConvertToPolyline(IList<MapPoint> points)
        {
            Polyline polyline = new Polyline();
            polyline.Paths.Add( ConvertToPointCollection( points));
            return polyline;
        }

        /// <summary>
        /// 根据几何要素取得对应的渲染符号
        /// </summary>
        /// <param name="ge"></param>
        /// <returns></returns>
        public static ESRI.ArcGIS.Client.Symbols.Symbol GetSymbol(ESRI.ArcGIS.Client.Geometry.Geometry ge, System.Windows.Media.Color color)
        {
            if (ge is MapPoint)
                return new SimpleMarkerSymbol()
                {
                    Color = new System.Windows.Media.SolidColorBrush(color),
                    Size = 10,
                    Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
                };
            else if (ge is ESRI.ArcGIS.Client.Geometry.Polyline)
                return new LineSymbol() { Color = new System.Windows.Media.SolidColorBrush(color), Width = 3 };
            else if (ge is ESRI.ArcGIS.Client.Geometry.Polygon
                     || ge is ESRI.ArcGIS.Client.Geometry.Envelope)
            {
                Symbol MySm = new SimpleFillSymbol()
                {
                    BorderBrush = new System.Windows.Media.SolidColorBrush(color) { },
                    BorderThickness = 1,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White) { Opacity = 0.4 }
                };
                return MySm;
            }
            return null;
        }

        public static void MeasureScreenBoundingBox(ESRI.ArcGIS.Client.Map map, Envelope envelop, out double width, out double height)
        {
            if (envelop != null)
            {
                MapPoint leftCorner = new MapPoint(envelop.XMin, envelop.YMin);
                System.Windows.Point p1 = map.MapToScreen(leftCorner);
                MapPoint rightCorner = new MapPoint(envelop.XMax, envelop.YMax);
                System.Windows.Point p2 = map.MapToScreen(rightCorner);

                width = Math.Abs(p2.X - p1.X);
                height = Math.Abs(p1.Y - p2.Y);
            }
            else
            {
                width = 0;
                height = 0;
            }
        }
    }
}
