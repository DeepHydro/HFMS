using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Samples;
using HydroCloud.Actions;
using HydroCloud.RemoteSesnsing;
using HydroCloud.ServiceReference;
using HydroCloud.Spatial;
using HydroCloud.SpatialTemporal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Vishcious.ArcGIS.SLContrib;

namespace HydroCloud
{
    public partial class MainPage : UserControl
    {
    	private const int SRID = 4326; //Spatial ref ID of the map. Note that several controls and layers in this application requires 4326 to work as is.
		private double columnWidth = 150; //Width of layer configuration panel
		private GeodeticDistance geoMeasure;
		private Draw draw;
		private ESRI.ArcGIS.Samples.Editor editor;
        private Measure measure;

        private ServiceClient Client;
        private Graphic _OldPointGraphic;
        private GraphicsLayer _GeneralStationLayer;
        private GraphicsLayer _GridLayer;
        private QueryCriteria _QueryCriteria;
        private DoubleTimeSeries _DoubleTimeSeries;
        private UserAction _CurrentUserAction = UserAction.None;
        private BBox _Box;
        private Storyboard _Timer;
        private Station _SelectedStation;
        private bool _IsDownloadingTimeSeries;
        private GridRender _GridRender;
        private int _selected_cell_id;
        private Dictionary<string,GraphicsLayer> _GraphicsLayers;
        private Dictionary<string, MarkerSymbol> _MarkerSymbols;
        private ImageSeries _ImageSeries;
        private bool _GridLoaded = false;
        public FlareClusterer ClustererOfStation { get; set; }
        public UserAction CurrentAction
        {
            get
            {
                return _CurrentUserAction;
            }
            set
            {
                _CurrentUserAction = value;
                OnUserActionChanged(value);
            }
        }
        public MainPage()
		{
			InitializeComponent();

            Client = new ServiceClient();
            Client.GetKeyWordsCompleted += client_GetKeyWordsCompleted;
            Client.GetAllSitesCompleted += Client_GetAllSitesCompleted;
            Client.GetSitesCompleted += client_GetSitesCompleted;
            Client.GetDoubleTimeSeriesCompleted += client_GetDoubleTimeSeriesCompleted;
            Client.GetGridCompleted += Client_GetGridCompleted;
            Client.GetTimeRangeCompleted += Client_GetTimeRangeCompleted;
            Client.GetPointProfileCompleted += Client_GetPointProfileCompleted;
            Client.DownloadImageCompleted += Client_DownloadImageCompleted;
            Client.DownloadLegendCompleted += Client_DownloadLegendCompleted;
            Client.GetSensorImageRecordCompleted += Client_GetSensorImageRecordCompleted;

            _GraphicsLayers = new Dictionary<string, GraphicsLayer>();
            _MarkerSymbols = new Dictionary<string, MarkerSymbol>();

            _MarkerSymbols.Add("Buoy", BuoyMarkerSymbol);
            _MarkerSymbols.Add("Tide", TideMarkerSymbol);
            _MarkerSymbols.Add("Water Quality", WQMarkerSymbol);
            _MarkerSymbols.Add("Hydrology", HydrologyMarkerSymbol);

            _GraphicsLayers.Add("Buoy", Map.Layers["Buoys"] as GraphicsLayer);
            _GraphicsLayers.Add("Tide", Map.Layers["Tide Stations"] as GraphicsLayer);
            _GraphicsLayers.Add("Water Quality", Map.Layers["Water Quality Stations"] as GraphicsLayer);
            _GraphicsLayers.Add("Hydrology", Map.Layers["Hydrological Stations"] as GraphicsLayer);
            _GraphicsLayers.Add("Weather", Map.Layers["Weather Stations"] as GraphicsLayer);
            _GraphicsLayers.Add("General", Map.Layers["General Stations"] as GraphicsLayer);

            _GeneralStationLayer = Map.Layers["General Stations"] as GraphicsLayer;
            _GridLayer = Map.Layers["Model Grid"] as GraphicsLayer;

            foreach (GraphicsLayer layer in _GraphicsLayers.Values)
            {
                layer.MouseLeftButtonDown += GraphicsLayer_MouseLeftButtonDown;
                layer.Clusterer = null;
            }

            ClustererOfStation = new FlareClusterer()
            {
                FlareBackground = new SolidColorBrush(Colors.Yellow),
                FlareForeground = new SolidColorBrush(Colors.Black),
                MaximumFlareCount = 100,
                Radius = 40,
            };

            _ImageSeries = new ImageSeries();
            _QueryCriteria = new QueryCriteria();
            _Timer = new Storyboard();
            _Timer.Duration = TimeSpan.FromMilliseconds(10000);
            _Timer.Completed += _timer_Completed;
            StatusBar.Visibility = System.Windows.Visibility.Collapsed;
            _Box = new BBox();
            _IsDownloadingTimeSeries = false;
            _GridRender = new GridRender();
            GridRender.Instance = _GridRender;
            animationPlayer.ServiceClient = Client;
            coastalWatch.ServiceClient = Client;
            coastalWatch.Map = this.Map;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
            geoMeasure = new GeodeticDistance() { Map = Map }; //Radius tool
            measure = new Measure() { Map = Map, LineSymbol = shadowLine, FillSymbol = shadowFill }; //Measure tool
            draw = new Draw(Map) //Draw tool
            {
                FillSymbol = shadowFill,
                LineSymbol = shadowLine
            };
            draw.DrawComplete += new EventHandler<DrawEventArgs>(draw_DrawComplete);
            editor = new ESRI.ArcGIS.Samples.Editor(Map, Map.Layers["Annotations"] as GraphicsLayer); //Editor tool
            this.Map.Layers.CollectionChanged += Layers_CollectionChanged;

            Client.GetKeyWordsAsync();
            Client.GetAllSitesAsync();
            Client.GetTimeRangeAsync();
		}
   

        #region Toolbar

        /// <summary>
        /// Used by the delete tool. Delete feature when clicked
        /// </summary>
        private void annotations_MouseLeftButtonDown(object sender, Graphic graphic, MouseButtonEventArgs args)
        {
            if (toggleDeleteGraphics.IsChecked.Value) //only delete if delete tool is active
            {
                GraphicsLayer annotations = Map.Layers["Annotations"] as GraphicsLayer;
                annotations.Graphics.Remove(graphic);
            }
        }

        /// <summary>
        /// Enable draw if one of the draw buttons is enabled
        /// </summary>
        private void toggleDraw_Checked(object sender, RoutedEventArgs e)
        {
            draw.IsEnabled = true;
            if (sender == toggleDrawPoint) draw.DrawMode = DrawMode.Point;
            else if (sender == toggleDrawPolyline) draw.DrawMode = DrawMode.Polyline;
            else if (sender == toggleDrawPolygon) draw.DrawMode = DrawMode.Polygon;
            else if (sender == toggleDrawRectangle) draw.DrawMode = DrawMode.Rectangle;
            else if (sender == toggleDrawFreehand) draw.DrawMode = DrawMode.Freehand;
            else draw.DrawMode = DrawMode.None;
        }

        /// <summary>
        /// Disable draw if all the draw buttons is unchecked
        /// </summary>
        private void toggleDraw_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == toggleDrawPoint && draw.DrawMode == DrawMode.Point ||
                sender == toggleDrawPolyline && draw.DrawMode == DrawMode.Polyline ||
                sender == toggleDrawPolygon && draw.DrawMode == DrawMode.Polygon ||
                sender == toggleDrawRectangle && draw.DrawMode == DrawMode.Rectangle ||
                sender == toggleDrawFreehand && draw.DrawMode == DrawMode.Freehand)
                draw.IsEnabled = false;
        }

        private void draw_DrawComplete(object sender, DrawEventArgs e)
        {
            GraphicsLayer annotations = Map.Layers["Annotations"] as GraphicsLayer;
            Graphic g = new Graphic();
            g.Geometry = e.Geometry;
            annotations.Graphics.Add(g);
        }

        private void toggleEditGeometry_Checked(object sender, RoutedEventArgs e)
        {
            editor.IsEnabled = (sender as ToggleButton).IsChecked.Value;
        }

        private void Measure_Checked(object sender, RoutedEventArgs e)
        {
            measure.IsActivated = false;
            measure.Type = sender == MeasureDistance ? MeasureType.Distance : MeasureType.Area;
            measure.IsActivated = (sender as ToggleButton).IsChecked.Value;
        }

        private void Measure_Unchecked(object sender, RoutedEventArgs e)
        {
            measure.IsActivated = MeasureArea.IsChecked.Value || MeasureDistance.IsChecked.Value;
        }

        private void GeoMeasureToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            geoMeasure.IsActivated = (sender as ToggleButton).IsChecked.Value;
        }

        #endregion

        #region View
        /// <summary>
        /// Show an initalizing layer message when a layer is added and hasn't been initialized yet
        /// </summary>
        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Layer l = e.NewItems[0] as Layer;
                if (!l.IsInitialized)
                {
                    l.Initialized += layer_Initialized;
                    l.InitializationFailed += layer_InitializationFailed;
                    InitializeLayer.Visibility = Visibility.Visible;
                }
            }
        }		/// <summary>
        /// Hide "Initializing layer" message when adding a layer
        /// </summary>
        private void layer_Initialized(object sender, EventArgs e)
        {
            Layer l = sender as Layer;
            l.Initialized -= layer_Initialized;
            InitializeLayer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// If layer failed to initialize, show a message in the browser status bar
        /// </summary>
        private void layer_InitializationFailed(object sender, EventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.SetProperty("status",
                "Layer failed to initialize: " + (sender as Layer).InitializationFailure.Message);
        }
        /// <summary>
        /// When the first layer is checked, show the configurator panel
        /// </summary>
        private void layerChecked(object sender, RoutedEventArgs e)
        {
            if (LeftMenu.Children.Count > 0 && RightColumnDefinition.Width.Value == 0)  //First configurator. Show panel
            {
                RightColumnSplitterDefinition.Width = new GridLength(9);
                RightColumnDefinition.Width = new GridLength(columnWidth);
            }
        }

        /// <summary>
        /// When the last layer is unchecked, hide the configurator panel
        /// </summary>
        private void layerUnchecked(object sender, RoutedEventArgs e)
        {
            //When a layer is removed, remove configurator to panel if it had one
            if (LeftMenu.Children.Count == 0) //No more configurators. Hide panel
            {
                columnWidth = RightColumnDefinition.Width.Value;
                RightColumnSplitterDefinition.Width = new GridLength(0);
                RightColumnDefinition.Width = new GridLength(0);
            }
        }
        #endregion

        #region Layer

        #endregion

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point screenPoint = e.GetPosition(Map);
            ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = Map.ScreenToMap(screenPoint);
            if (Map.WrapAroundIsActive)
                mapPoint = ESRI.ArcGIS.Client.Geometry.Geometry.NormalizeCentralMeridian(mapPoint) as ESRI.ArcGIS.Client.Geometry.MapPoint;
            if (mapPoint != null)
            {
                var pt = SpatialReferenceSystem.ToGeographic(mapPoint.X, mapPoint.Y);
                if (pt != null)
                {
                    lonLabel.Content = pt[0].ToString("0.0000");
                    latLabel.Content = pt[1].ToString("0.0000");
                }
            }
        }
        private void AnnotationLayer_MouseLeftButtonDown_1(object sender, GraphicMouseButtonEventArgs e)
        {
            if (toggleDeleteGraphics.IsChecked.Value) //only delete if delete tool is active
            {
                GraphicsLayer annotations = Map.Layers["Annotations"] as GraphicsLayer;
                annotations.Graphics.Remove(e.Graphic);
                return;
            }
        }
        private void checkCluster_Checked(object sender, RoutedEventArgs e)
        {
            _GeneralStationLayer.Clusterer = checkCluster.IsChecked.Value ? ClustererOfStation : null;
        }
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (keywordBox.Text == "")
            {
                MessageBox.Show("Keyword can not be null.", "Keyword", MessageBoxButton.OK);
                return;
            }
            double buf = 90;
            double.TryParse(TBNorthbound.Text, out buf);
            if (buf == 0)
                _Box.North = 90;
            else
                _Box.North = buf;
            double.TryParse(this.TBSouthbound.Text, out buf);
            if (buf == 0)
                _Box.South = -90;
            else
                _Box.South = buf;
            double.TryParse(this.TBWestbound.Text, out buf);
            if (buf == 0)
                _Box.West = -180;
            else
                _Box.West = buf;
            double.TryParse(this.TBEastbound.Text, out buf);
            if (buf == 0)
                _Box.East = -180;
            else
                _Box.East = buf;

            _QueryCriteria.BBox = _Box;
            _QueryCriteria.VariableName = keywordBox.Text;
            _QueryCriteria.Start = startDate.SelectedDate.Value;
            _QueryCriteria.End = endDate.SelectedDate.Value;
            GeneralInfo.Content = "Searching sites...";
            CurrentAction = UserAction.SearchStation;
            Client.GetSitesAsync(_QueryCriteria); 
        }
        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            if (_OldPointGraphic != null)
            {
                _OldPointGraphic.Selected = false;
            }
            _OldPointGraphic = e.Graphic;
            e.Graphic.Select();

            if (checkClickToDownload.IsChecked.Value && !_IsDownloadingTimeSeries)
            {
                var item = cmbMaxRecords.SelectedItem as ComboBoxItem;
                if (item.Content.ToString() == "All")
                    _QueryCriteria.MaximumRecord = 0;
                else
                {
                    int max = 0;
                    int.TryParse(item.Content.ToString(), out max);
                    _QueryCriteria.MaximumRecord = max;
                }

                _SelectedStation = e.Graphic.Attributes["Site"] as Station;
                _OldPointGraphic = e.Graphic;
                string info = string.Format("Site Name:\t{0}\nLongitude:\t{1}\nLatitude:\t{2}",
                    _SelectedStation.Name, _SelectedStation.Longitude, _SelectedStation.Latitude);
                _QueryCriteria.SiteID = _SelectedStation.ID;
                CurrentAction = UserAction.DownloadTimeSeries;
                GeneralInfo.Content = "Retrieving time series...";
                _IsDownloadingTimeSeries = true;
                Client.GetDoubleTimeSeriesAsync(_QueryCriteria);
            }
        }
     
        private void GridLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            var id = int.Parse(e.Graphic.Attributes["ID"].ToString());
            CurrentAction = UserAction.DownloadTimeSeries;
            GeneralInfo.Content = "Retrieving time series...";
            _IsDownloadingTimeSeries = true;
            _selected_cell_id = id;
            if (rbtnTemp.IsChecked.Value)
                _GridRender.VariableIndex = 0;
            if (rbtnChla.IsChecked.Value)
                _GridRender.VariableIndex = 1;
            Client.GetPointProfileAsync(_GridRender.VariableIndex, id);
        }
        private void ArcGISDynamicMapServiceLayer_InitializationFailed(object sender, EventArgs e)
        {
            Layer layer = sender as Layer;
            MessageBox.Show("加载图层失败：" + layer.InitializationFailure.Message);
        }
        private void client_GetKeyWordsCompleted(object sender, GetKeyWordsCompletedEventArgs e)
        {
            CurrentAction = UserAction.None;
            if (e.Error != null)
            {
                GeneralInfo.Content = "Failed to connect to database";
            }
            else
            {
                GeneralInfo.Content = "Suucessfull to connect to database";
                keywordBox.ItemsSource = e.Result;
            }
        }
        private void Client_GetGridCompleted(object sender, GetGridCompletedEventArgs e)
        {
            _GridLoaded = true;
            _GridRender.Creat(_GridLayer, e.Result);
        }
        private void Client_GetTimeRangeCompleted(object sender, GetTimeRangeCompletedEventArgs e)
        {
            _GridRender.Start = DateTime.FromOADate(e.Result[0]);
            _GridRender.End = DateTime.FromOADate(e.Result[1]);
            _GridRender.NTime = (_GridRender.End - _GridRender.Start).Days + 1;
            _GridRender.Dates = new System.Collections.ObjectModel.ObservableCollection<DateTime>();
            for (int i = 0; i < _GridRender.NTime; i++)
            {
                _GridRender.Dates.Add(_GridRender.Start.AddDays(i));
            }
            animationPlayer.SetDates(_GridRender.Dates);
            animationPlayer.Slider.Maximum = _GridRender.NTime - 1;
            animationPlayer.Slider.Minimum = 0;
        }
        private void client_GetSitesCompleted(object sender, GetSitesCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CurrentAction = UserAction.Idle;
                GeneralInfo.Content = "Failed to search sites";
                return;
            }
            if (e.Result != null)
            {
                _GeneralStationLayer.Graphics.Clear();
                foreach (var s in e.Result)
                {
                    Graphic graphic = new Graphic();
                    double[] lonlat = SpatialReferenceSystem.ToWebMercator(s.Longitude, s.Latitude);
                    graphic.Geometry = new MapPoint(lonlat[0], lonlat[1]);
                    graphic.Symbol = _MarkerSymbols["Hydrology"];
                    graphic.Attributes.Add("Name", s.Name);
                    graphic.Attributes.Add("Site", s);
                    _GeneralStationLayer.Graphics.Add(graphic);
                }
                CurrentAction = UserAction.Idle;
                GeneralInfo.Content = e.Result.Count + " sites found";
            }
            else
            {
                CurrentAction = UserAction.Idle;
                GeneralInfo.Content = "No sites found";
            }
        }

        private void Client_GetAllSitesCompleted(object sender, GetAllSitesCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CurrentAction = UserAction.Idle;
                GeneralInfo.Content = "Failed to search sites";
                return;
            }
            if (e.Result != null)
            {
                foreach (GraphicsLayer layer in _GraphicsLayers.Values)
                {
                    layer.Graphics.Clear();
                }
                foreach (var s in e.Result)
                {                  
                    Graphic graphic = new Graphic();
                    double[] lonlat = SpatialReferenceSystem.ToWebMercator(s.Longitude, s.Latitude);
                    graphic.Geometry = new MapPoint(lonlat[0], lonlat[1]);
                    graphic.Symbol = _MarkerSymbols[s.SiteType]; ;
                    graphic.Attributes.Add("Name", s.Name);
                    graphic.Attributes.Add("Site", s);
                    _GraphicsLayers[s.SiteType].Graphics.Add(graphic);
                }
                CurrentAction = UserAction.Idle;
                GeneralInfo.Content = e.Result.Count + " sites found";
            }
            else
            {
                CurrentAction = UserAction.Idle;
                GeneralInfo.Content = "No sites found";
            }
        }

        private void client_GetDoubleTimeSeriesCompleted(object sender, GetDoubleTimeSeriesCompletedEventArgs e)
        {
            if (e.Error != null || e.Result == null)
            {
                _IsDownloadingTimeSeries = false;
                CurrentAction = UserAction.None;
                GeneralInfo.Content = "Failed to download time series";
                return;
            }
            _DoubleTimeSeries = e.Result;
            string title = string.Format("{0} at {1}", _QueryCriteria.VariableName, _SelectedStation.Name);
            ChartWindow.Visibility = System.Windows.Visibility.Visible;
            chart.PlotTimeSeries(_DoubleTimeSeries, title);
            _IsDownloadingTimeSeries = false;
            CurrentAction = UserAction.Idle;
            GeneralInfo.Content = _DoubleTimeSeries.DateTimes.Count + " values downloaded";
        }
        private void Client_GetPointProfileCompleted(object sender, GetPointProfileCompletedEventArgs e)
        {
            if (e.Error != null || e.Result == null)
            {
                _IsDownloadingTimeSeries = false;
                CurrentAction = UserAction.None;
                GeneralInfo.Content = "Failed to download time series";
                return;
            }
            DoubleTimeSeries ts = new DoubleTimeSeries();
            ts.DateTimes = _GridRender.Dates;
            ts.Values = e.Result;
            ChartWindow.Visibility = System.Windows.Visibility.Visible;
            string title = string.Format("{0} at {1}", "temp", _selected_cell_id);
            if (rbtnTemp.IsChecked.Value)
                title = rbtnTemp.Content.ToString();
            if (rbtnChla.IsChecked.Value)
                title = rbtnChla.Content.ToString();
            chart.PlotTimeSeries(ts, title);
            _IsDownloadingTimeSeries = false;
            CurrentAction = UserAction.Idle;
            GeneralInfo.Content = ts.DateTimes.Count + " values downloaded";
        }

        private void Client_GetSensorImageRecordCompleted(object sender, GetSensorImageRecordCompletedEventArgs e)
        {
            if(e.Result != null)
            {
                _ImageSeries.Source = e.Result;
                coastalWatch.UpdateImageRecord(_ImageSeries);
            }
        }

        private void Client_DownloadImageCompleted(object sender, DownloadImageCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                var bite = Convert.FromBase64String(e.Result);
                MemoryStream fs = new MemoryStream();  
                fs.Write(bite, 0, bite.Length);
                ShowImage(fs);
                coastalWatch.ImageList.IsEnabled = true;
            }
        }

        private void Client_DownloadLegendCompleted(object sender, DownloadLegendCompletedEventArgs e)
        {
            if (e.Result != null && e.Result != "")
            {
                var bite = Convert.FromBase64String(e.Result);
                MemoryStream fs = new MemoryStream();
                fs.Write(bite, 0, bite.Length);
                var source = new BitmapImage();
                source.SetSource(fs);
                legendControl.LegendImageSource = source;
            }
        }

        private void OnUserActionChanged(UserAction action)
        {
            StatusBar.Visibility = System.Windows.Visibility.Visible;
            switch (action)
            {
                case UserAction.SearchStation:
                    searchButton.IsEnabled = false;
                    break;
                case UserAction.Idle:
                    searchButton.IsEnabled = true;
                    break;
                case UserAction.None:
                    break;
            }
            _Timer.Begin();
        }
        private void _timer_Completed(object sender, EventArgs e)
        {
            GeneralInfo.Content = "";
        }

        private void openShp_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (!(ofd.ShowDialog() ?? false))
                return;
            FileInfo shapeFile = null;
            FileInfo dbfFile = null;
            foreach (FileInfo fi in ofd.Files)
            {
                if (fi.Extension.ToLower() == ".shp")
                {
                    shapeFile = fi;
                }
                if (fi.Extension.ToLower() == ".dbf")
                {
                    dbfFile = fi;
                }
            }           
            //Read the SHP and DBF files into the ShapeFileReader
            ShapeFile shapeFileReader = new ShapeFile();
            if (shapeFile != null && dbfFile != null)
            {
                shapeFileReader.Read(shapeFile, dbfFile);
            }
            else
            {
                HtmlPage.Window.Alert("Please select a SP and a DBF file to proceed.");
                return;
            }
            GraphicsLayer graphicsLayer = Map.Layers["Shenzhen Boundary"] as GraphicsLayer;
            graphicsLayer.Graphics.Clear();
            Symbol MySm = new SimpleFillSymbol()
            {
                BorderBrush = new System.Windows.Media.SolidColorBrush(Colors.White) { },
                BorderThickness = 1,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent) { Opacity = 0 }
            };

            foreach (ShapeFileRecord record in shapeFileReader.Records)
            {
                Graphic graphic = record.ToGraphic();
                if (graphic != null)
                {
                    graphic.Symbol = MySm;
                    graphic.Geometry.SpatialReference = new SpatialReference(4326);
                    graphicsLayer.Graphics.Add(graphic);
                }
            }
            graphicsLayer.Refresh();
        }

        private void openImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Png Files (*.png)|*.png|All Files (*.*)|*.*";
            if (!(dlg.ShowDialog() ?? false))
                return;
            var fs = dlg.File.OpenRead();
            ShowImage(fs);
        }

        public void ShowImage(Stream fs)
        {
            if (fs.Length == 0)
                return;
            var selectedSensorImageRecord = coastalWatch.SelectedSensorImageRecord;
            var selectedLayer = Map.Layers[selectedSensorImageRecord.SensorName] as GraphicsLayer;
            coastalWatch.SelectedLayer = selectedLayer;

            if (!coastalWatch.KeepSeries)
                selectedLayer.Graphics.Clear();

            var source = new BitmapImage();
            source.SetSource(fs);
            ESRI.ArcGIS.Client.Geometry.PointCollection pc = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            MapPoint pt = new MapPoint(selectedSensorImageRecord.BBox[0], selectedSensorImageRecord.BBox[3]);
            pc.Add(pt);
            pt = new MapPoint(selectedSensorImageRecord.BBox[1], selectedSensorImageRecord.BBox[3]);
            pc.Add(pt);
            pt = new MapPoint(selectedSensorImageRecord.BBox[1], selectedSensorImageRecord.BBox[2]);
            pc.Add(pt);
            pt = new MapPoint(selectedSensorImageRecord.BBox[0], selectedSensorImageRecord.BBox[2]);
            pc.Add(pt);
            pt = new MapPoint(selectedSensorImageRecord.BBox[0], selectedSensorImageRecord.BBox[3]);
            pc.Add(pt);

            PictureFillSymbol symbl = new PictureFillSymbol()
            {
                Source = source,
                BorderThickness = 0
            };

            Graphic graphic = new Graphic();
            ESRI.ArcGIS.Client.Geometry.Polygon ep = new Polygon();
            ep.Rings.Add(pc);
            graphic.Geometry = ep;
            graphic.Symbol = symbl;
            graphic.Geometry.SpatialReference = new SpatialReference(4326);
            selectedLayer.Graphics.Add(graphic);
            selectedLayer.Refresh();
        }

        private void coastalWatch_Loaded(object sender, RoutedEventArgs e)
        {
            Client.GetSensorImageRecordAsync();
        }

        private void openWarning_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (!(ofd.ShowDialog() ?? false))
                return;
            FileInfo shapeFile = null;
            FileInfo dbfFile = null;
            foreach (FileInfo fi in ofd.Files)
            {
                if (fi.Extension.ToLower() == ".shp")
                {
                    shapeFile = fi;
                }
                if (fi.Extension.ToLower() == ".dbf")
                {
                    dbfFile = fi;
                }
            }
            //Read the SHP and DBF files into the ShapeFileReader
            ShapeFile shapeFileReader = new ShapeFile();
            if (shapeFile != null && dbfFile != null)
            {
                shapeFileReader.Read(shapeFile, dbfFile);
            }
            else
            {
                HtmlPage.Window.Alert("Please select a SP and a DBF file to proceed.");
                return;
            }

            Color[] color = new Color[4];
            color[0] = Colors.Blue;
            color[1] = Colors.Yellow;
            color[2] = Colors.Orange;
            color[3] = Colors.Red;

            GraphicsLayer graphicsLayer = Map.Layers["Warning Region"] as GraphicsLayer;
            graphicsLayer.Graphics.Clear();


            WarningRegion region1 = new WarningRegion()
            {
                Name = "Daya Bay",
                Level = 3,
                Chla = 15.0
            };
            WarningRegion region2 = new WarningRegion()
            {
                Name = "Mirs Bay",
                Level = 1,
                Chla = 15.0
            };
            WarningRegion region3 = new WarningRegion()
            {
                Name = "Shenzhen Bay",
                Level = 2,
                Chla = 15.0
            };
            WarningRegion region4 = new WarningRegion()
            {
                Name = "Maozhou",
                Level = 2,
                Chla = 15.0
            };
            WarningRegion region5 = new WarningRegion()
            {
                Name = "Shekou",
                Level = 1,
                Chla = 15.0
            };
            WarningRegion region6 = new WarningRegion()
            {
                Name = "Lingding",
                Level = 1,
                Chla = 15.0
            };
            List<WarningRegion> listregion = new List<WarningRegion>();
            listregion.Add(region1);
            listregion.Add(region2);
            listregion.Add(region3);
            listregion.Add(region4);
            listregion.Add(region5);
            listregion.Add(region6);
            int i = 0;
            foreach (ShapeFileRecord record in shapeFileReader.Records)
            {
                Symbol MySm = new SimpleFillSymbol()
                {
                    BorderBrush = new System.Windows.Media.SolidColorBrush(Colors.DarkGray),
                    BorderThickness = 2,
                    Fill = new System.Windows.Media.SolidColorBrush(color[listregion[i].Level - 1]) { Opacity = 0.7 }
                };
                Graphic graphic = record.ToGraphic();
                graphic.Geometry.SpatialReference = new SpatialReference(4326);
                if (graphic != null)
                {
                    graphic.Symbol = MySm;
                    graphicsLayer.Graphics.Add(graphic);
                }
                graphic.Attributes.Add("Name", listregion[i].Name);
                graphic.Attributes.Add("Region", listregion[i]);
                i++;
            }
        }

      
        private void btnGetRSGrid_Click(object sender, RoutedEventArgs e)
        {
            if(!_GridLoaded)
                Client.GetGridAsync();
        }


    }
}
