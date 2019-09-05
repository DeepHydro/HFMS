using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SilverGlobe.Data;
using System.Windows.Threading;
using ESRI.ArcGIS.Client.Geometry;

namespace SilverGlobe
{
	/// <summary>
	/// Globe linked to map
	/// </summary>
	public partial class WorldGlobe : UserControl
	{
		private DispatcherTimer _timer;
		private RotationController _rotationController;
		private SlerpController _slerpController;
		
		public WorldGlobe()
		{
			InitializeComponent();
			InitShapes();
			this.Loaded += new RoutedEventHandler(WorldGlobe_Loaded);
		}

		private void WorldGlobe_Loaded(object sender, RoutedEventArgs e)
		{
			//SubscribeEvents();
			_timer = new DispatcherTimer();
			_timer.Tick += Timer_Tick;
			_timer.Interval = TimeSpan.FromMilliseconds(10);

			_rotationController = new RotationController(_globe, _timer);
			_slerpController = new SlerpController(_globe);
		}

		/// <summary>
		///  Internal handler for setting orientation of globe when map extent changes
		/// </summary>
		private void Map_ExtentChanged(object sender, ESRI.ArcGIS.Client.ExtentEventArgs args)
		{
			MapPoint center = Map.Extent.GetCenter();
			if (center.Y < -90) center.Y = -90;
			else if (center.Y > 90) center.Y = 90;
			SetViewCenter(new GeoPosition(center.Y, center.X));
		}

		public void SetViewCenter(GeoPosition position)
		{
			if (!_timer.IsEnabled)
				_timer.Start();
			_rotationController.AnimateTo(position);
		}

		private void Timer_Tick(Object sender, EventArgs e)
		{
			_globe.Update();
			_rotationController.Update();
			
		}

		private void InitShapes()
		{
			_globe.InitShapes(ContinentShapes.Africa,
							  ContinentShapes.America,
							  ContinentShapes.Australia,
							  ContinentShapes.Eurasia,
							  ContinentShapes.Antarctica
							  );
		}

		/// <summary>
		/// Identifies the <see cref="Map"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapProperty =
						DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(WorldGlobe),
						new PropertyMetadata(null, OnMapPropertyChanged));
		/// <summary>
		/// Gets or sets Map.
		/// </summary>
		public ESRI.ArcGIS.Client.Map Map
		{
			get { return (ESRI.ArcGIS.Client.Map)GetValue(MapProperty); }
			set { SetValue(MapProperty, value); }
		}
		/// <summary>
		/// MapProperty property changed handler. 
		/// </summary>
		/// <param name="d">WorldGlobe that changed its Map.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WorldGlobe dp = d as WorldGlobe;
			ESRI.ArcGIS.Client.Map newMap = (ESRI.ArcGIS.Client.Map)e.NewValue;
			ESRI.ArcGIS.Client.Map oldMap = (ESRI.ArcGIS.Client.Map)e.OldValue;
			if (oldMap != null)
			{
				oldMap.ExtentChanged -= dp.Map_ExtentChanged;
			} 
			if (newMap != null)
			{
				newMap.ExtentChanged += dp.Map_ExtentChanged;
			}
		}
	}
}
