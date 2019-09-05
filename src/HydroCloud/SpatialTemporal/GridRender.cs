using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using HdyroCloud.HMath;
using HydroCloud.ServiceReference;
using HydroCloud.Spatial;
using System;
using System.Windows;
using System.Windows.Media;

namespace HydroCloud.SpatialTemporal
{
    public class GridRender
    {
        public GridRender()
        {
            RampID = 14;
            Opacity = 0.5;
            BorderThickness = 1;
        }
        private Ramp _ColorRamp;
        private int _RampID;
        private int _ColourRampCount = 5;
        private GraphicsLayer _GraphicsLayer;
        private Color []_ClassColor;

        public static GridRender Instance { get; set; }
        public int VariableIndex { get; set; }
        public int TimeIndex { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<System.DateTime> Dates { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int NTime { get; set; }
        public int RampID 
        { 
            get
            {
                return _RampID;
            }
            set 
            {
                _RampID = value;
                _ColorRamp = new Ramp(_RampID);
                UpdateColors();
            } 
        }
        public int ColourRampCount
        {
            get
            {
                return _ColourRampCount;
            }
            set
            {
                _ColourRampCount = value;
                if (_ColourRampCount > 0)
                {
                    _ColourRampCount = 5;
                    UpdateColors();
                }
            }
        }
        public double Opacity
        {
            get;
            set;
        }
        public double BorderThickness
        {
            get;
            set;
        }

        public void Creat(GraphicsLayer layer, RegularGrid grid)
        {
            _GraphicsLayer = layer;
            var cellsize= grid.CellSize*0.5;
            var polygon = new Polygon();
         
            for(int i=0;i<grid.NCell;i++)
            {
                var symbol = GetDefaultSymbol(polygon, System.Windows.Media.Colors.White);
                ESRI.ArcGIS.Client.Geometry.PointCollection pc = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                var buf = SpatialReferenceSystem.ToWebMercator(grid.CentroidX[i] - cellsize, grid.CentroidY[i] + cellsize);
                MapPoint p1 = new MapPoint(buf[0], buf[1]);
                buf = SpatialReferenceSystem.ToWebMercator(grid.CentroidX[i] + cellsize, grid.CentroidY[i] + cellsize);
                MapPoint p2 = new MapPoint(buf[0], buf[1]);
                buf = SpatialReferenceSystem.ToWebMercator(grid.CentroidX[i] + cellsize, grid.CentroidY[i] - cellsize);
                MapPoint p3 = new MapPoint(buf[0], buf[1]);
                buf = SpatialReferenceSystem.ToWebMercator(grid.CentroidX[i] - cellsize, grid.CentroidY[i] - cellsize);
                MapPoint p4 = new MapPoint(buf[0], buf[1]);
                buf = SpatialReferenceSystem.ToWebMercator(grid.CentroidX[i] - cellsize, grid.CentroidY[i] + cellsize);
                MapPoint p5 = new MapPoint(buf[0], buf[1]);
                pc.Add(p1);
                pc.Add(p2);
                pc.Add(p3);
                pc.Add(p4);
                pc.Add(p5);
                Graphic graphic = new Graphic();
                Polygon ep = new Polygon();
                ep.Rings.Add(pc);
                graphic.Geometry = ep;
                graphic.Symbol = symbol;
                graphic.Attributes.Add("ID", i);
                layer.Graphics.Add(graphic);
            }
        }
        private void UpdateColors()
        {
            int originLen = _ColorRamp.Colors.Length;
            int deltaL = (int)Math.Floor((double)originLen / _ColourRampCount);
            _ClassColor = new Color[_ColourRampCount];
            _ClassColor[0] = _ColorRamp.Colors[0];
            for (int i = 1; i < _ColourRampCount - 1; i++)
            {
                _ClassColor[i] = _ColorRamp.Colors[i * deltaL];
            }
            _ClassColor[_ColourRampCount - 1] = _ColorRamp.Colors[originLen - 1];
        }
        public void Render(double[] vec)
        {
            if (vec == null)
                return;
          //  var statinfo = StatHelper.Get(vec);
            var breaks = JenksFisher.CreateJenksFisherBreaksArray(vec, ColourRampCount + 1);
            if (breaks.Count <= 1)
                return;
            //MessageBox.Show(breaks.Count.ToString());
            //Console.WriteLine(breaks.Count);
            Console.WriteLine(breaks);
            for (int i = 0; i < _GraphicsLayer.Graphics.Count; i++)
            {
                var color = _ClassColor[0];
                for (int j = 0; j < ColourRampCount; j++)
                {
                    if (vec[i] >= breaks[j] && vec[i] < breaks[j + 1])
                    {
                        color = _ClassColor[j];
                        break;
                    }
                }
                var symbol=_GraphicsLayer.Graphics[i].Symbol as SimpleFillSymbol;
                symbol.Fill = new System.Windows.Media.SolidColorBrush(color) { Opacity = Opacity };
                symbol.BorderThickness = BorderThickness;
            }
            _GraphicsLayer.Refresh();
        }
        public int [] GetColorLevel(double [] vec)
        {
            int len=vec.Length;
            int[] levels = new int[len];
            var sinfo = StatHelper.Get(vec);
            if (sinfo.Max == sinfo.Min)
            {
                return levels;
            }
            else
            {
                double[] dividers = new double[ColourRampCount + 1];

                return levels;
            }
        }
        public  Color GetColor(double max, double min, double averagedValue)
        {
            if (max == min)
            {
                return _ClassColor[0];
            }
            else
            {
                double level = (averagedValue - min) / (max - min) * _ClassColor.Length - 1;
                Color color = Colors.White;
                if (level < 0)
                {
                    color = _ClassColor[0];
                }
                else
                {
                    if (level < _ClassColor.Length)
                        color = _ClassColor[(int)level];
                    else
                    {
                        color = _ClassColor[_ColorRamp.Colors.Length - 1];
                    }
                }
                return color;
            }
        }
        /// <summary>
        /// 根据几何要素取得对应的渲染符号
        /// </summary>
        /// <param name="ge"></param>
        /// <returns></returns>
        public  ESRI.ArcGIS.Client.Symbols.Symbol GetDefaultSymbol(ESRI.ArcGIS.Client.Geometry.Geometry ge, System.Windows.Media.Color color)
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
                    BorderThickness = BorderThickness,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White) { Opacity = Opacity }
                };
                return MySm;
            }
            return null;
        }
    }
}
