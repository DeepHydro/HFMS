using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Samples.Extensions;

namespace ESRI.ArcGIS.Samples.SilverMapDemo
{
	public partial class Page : UserControl
	{
		private const int SRID = 4326; //Spatial ref ID of the map. Note that several controls and layers in this application requires 4326 to work as is.
		private double columnWidth = 150; //Width of layer configuration panel
		private GeodeticDistance geoMeasure;
		private Draw draw;
		private Editor editor;
        private Measure measure;

		public Page()
		{
			InitializeComponent();
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
			editor = new Editor(Map, Map.Layers["Annotations"] as GraphicsLayer); //Editor tool

			//Load points for the heatmap
            //ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader loader = new ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader();
            //loader.LoadCompleted += heatmap_LoadCompleted;
            //loader.LoadRss((layerFeeds1.Layer as GeoRssLayer).Source, null);

			this.Map.Layers.CollectionChanged += Layers_CollectionChanged;

            ////Load the US states for the pie markers layers
            //ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            //{
            //    Geometry = new ESRI.ArcGIS.Client.Geometry.Envelope(-180, 0, 0, 90)
            //};
            //query.OutFields.Add("*");

            //QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            //queryTask.ExecuteCompleted += new EventHandler<QueryEventArgs>(MarkerSymbols_QueryTask_ExecuteCompleted);

            //queryTask.ExecuteAsync(query);

		}

		private void MarkerSymbols_QueryTask_ExecuteCompleted(object sender, QueryEventArgs e)
		{
			//Create centroid for all US States
			FeatureSet featureSet = e.FeatureSet;

            GraphicsLayer graphicsLayer = null; // layerDemo6.Layer as GraphicsLayer;
			
			if (featureSet != null && featureSet.Features.Count > 0)
			{
				foreach (Graphic feature in featureSet.Features)
				{
					ESRI.ArcGIS.Client.Geometry.Polygon polygon = feature.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                    if (polygon == null)
                        return;
					polygon.SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326);

					Graphic graphic;
					//Getting the center point of the polygon MBR:
					ESRI.ArcGIS.Client.Geometry.MapPoint featureCentroid = feature.Geometry.Extent.GetCenter();
					bool pointInPolygon = featureCentroid.IsWithin(polygon);
					if (pointInPolygon)
					{
						graphic = new Graphic()
						{
							Geometry = featureCentroid,
					
						};
					}
					else
					{
						graphic = new Graphic()
						{
							Geometry = polygon,
							Symbol = null
						};
					}

					//Assigning attributes from the feature to the graphic:
					foreach (string key in feature.Attributes.Keys)
					{
                        //for (int i = 0; i < pieChartMarkerSymbol.Fields.Count; i++)
                        //{
                        //    if (pieChartMarkerSymbol.Fields[i].FieldName == key)
                        //        graphic.Attributes.Add(key, feature.Attributes[key]);
                        //}
					}
					graphicsLayer.Graphics.Add(graphic);

					//Using the geometry service to find a new centroid in the case the
					//calculated centroid is not inside of the polygon:
					if (!pointInPolygon)
					{
						GeometryService geometryService = new GeometryService("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
						List<Graphic> graphicsList = new List<Graphic>();
						graphicsList.Add(graphic);
						geometryService.LabelPointsCompleted += MarkerSymbols_GeometryService_LabelPointsCompleted;
						geometryService.LabelPointsAsync(graphicsList);
					}
				}
			}
		}

		private void MarkerSymbols_GeometryService_LabelPointsCompleted(object sender, GraphicsEventArgs args)
		{
        //    GraphicsLayer graphicsLayer = layerDemo6.Layer as GraphicsLayer;
        //    int idx = 0;
        //    foreach (Graphic graphic in args.Results)
        //    {
        ////		graphic.Symbol = pieChartMarkerSymbol;
        //        ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = graphic.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
        //        graphic.Attributes.Add("X", mapPoint.X);
        //        graphic.Attributes.Add("Y", mapPoint.Y);

        //        foreach (string key in graphicsLayer.Graphics[idx].Attributes.Keys)
        //        {
        //            graphic.Attributes.Add(key, graphicsLayer.Graphics[idx].Attributes[key]);
        //        }

        //        graphicsLayer.Graphics.Add(graphic);
        //        idx++;
        //    }
		}

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
		}

		/// <summary>
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
		/// This is called when the earthquake georss feed has loaded and parsed all points. The
		/// same points are then added to the heatmap.
		/// </summary>
		private void heatmap_LoadCompleted(object sender, ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader.RssLoadedEventArgs e)
		{
            //HeatMapLayer layer = (layerDemo1.Layer as HeatMapLayer);
            //layer.HeatMapPoints.Clear();
            //foreach (Graphic g in e.Graphics)
            //{
            //    double magnitude = (double)new MagnitudeConverter().Convert(g.Attributes, typeof(double), null, null);
            //    for (int i = 0; i < magnitude; i++)
            //    {
            //        layer.HeatMapPoints.Add(g.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint);
            //    }
            //}
		}

		/// <summary>
		/// Called a radio buttons in the base layer menu is selected.
		/// Each radiobutton has a string tag that contains the URL to the service url
		/// </summary>
		private void BaseLayer_Changed(object sender, RoutedEventArgs e)
		{
			if (Map == null) return;
			ArcGISTiledMapServiceLayer layer = Map.Layers[0] as ArcGISTiledMapServiceLayer;
			layer.Url = (sender as FrameworkElement).Tag as string;
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

		#region Control Panel logic


		private void wmsLayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            if (layerWeather2 != null)
            {
                ((WMSMapServiceLayer)layerWeather2.Layer).Layers = new string[] 
                { 
                    (e.AddedItems[0] as WMSMapServiceLayer.LayerInfo).Name 
                };
            }
		}

		private void globeLayer_Initialized(object sender, EventArgs e)
		{
            layerWeather2.Configurator.Visibility = Visibility.Visible;
            if (wmsLayerList.Items.Count > 43)
                wmsLayerList.SelectedIndex = 43;
		}

		#endregion Control Panel logic

		/// <summary>
		/// Collapse topbar when the ESRI logo is clicked.
		/// </summary>
		private void EsriLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TopBarRowDefinition.Height = new GridLength(0);
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
			if(sender == toggleDrawPoint) draw.DrawMode = DrawMode.Point;
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

		private void pieCharts_MouseEnter(object sender, Graphic graphic, MouseEventArgs args)
		{
			graphic.SetZIndex(1);
		}

		private void pieCharts_MouseLeave(object sender, Graphic graphic, MouseEventArgs args)
		{
			graphic.SetZIndex(0);
		}
	}
}
