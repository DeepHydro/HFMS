using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Samples
{
	public class LayerCheckBox : CheckBox
	{
		public LayerCheckBox()
		{
			this.Checked += new RoutedEventHandler(LayerCheckBox_Checked);
			this.Unchecked += new RoutedEventHandler(LayerCheckBox_Unchecked);
		}

		void LayerCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if (Configurator != null && ConfiguratorParent != null)
			{
				ConfiguratorParent.Children.Remove(Configurator);
			}
		}

		void LayerCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			//When a layer is checked on, add configurator to panel if it has one
			if (Configurator != null && ConfiguratorParent != null)
			{
				ConfiguratorParent.Children.Add(Configurator);
			}
		}

		/// <summary>
		/// Identifies the <see cref="Map"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapProperty =
						DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(LayerCheckBox),
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
		/// <param name="d">LayerCheckBox that changed its Map.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			LayerCheckBox dp = d as LayerCheckBox;
			ESRI.ArcGIS.Client.Map newMap = (ESRI.ArcGIS.Client.Map)e.NewValue;
			ESRI.ArcGIS.Client.Map oldMap = (ESRI.ArcGIS.Client.Map)e.OldValue;
			if (oldMap != null && dp.Layer != null)
			{
				oldMap.Layers.Remove(dp.Layer);
			}
			if (newMap != null && dp.Layer != null && dp.IsChecked.Value)
			{
				newMap.Layers.Add(dp.Layer);
			}
		}

		protected override void OnToggle()
		{
			base.OnToggle();
			if (this.Layer!=null && this.Map!=null)
			{
				if (this.IsChecked.Value)
				{
					Map.Layers.Insert(Map.Layers.Count - 1, Layer);
				}
				else
				{
					Map.Layers.Remove(Layer);
				}
			}
		}

		/// <summary>
		/// Identifies the <see cref="Layer"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayerProperty =
						DependencyProperty.Register("Layer", typeof(ESRI.ArcGIS.Client.Layer), typeof(LayerCheckBox),
						new PropertyMetadata(null, OnLayerPropertyChanged));
		/// <summary>
		/// Gets or sets Layer.
		/// </summary>
		public ESRI.ArcGIS.Client.Layer Layer
		{
			get { return (ESRI.ArcGIS.Client.Layer)GetValue(LayerProperty); }
			set { SetValue(LayerProperty, value); }
		}

		/// <summary>
		/// LayerProperty property changed handler. 
		/// </summary>
		/// <param name="d">LayerCheckBox that changed its Layer.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnLayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			LayerCheckBox dp = d as LayerCheckBox;
			ESRI.ArcGIS.Client.Layer newLayer = (ESRI.ArcGIS.Client.Layer)e.NewValue;
			ESRI.ArcGIS.Client.Layer oldLayer = (ESRI.ArcGIS.Client.Layer)e.OldValue;
			if (dp.Map != null)
			{
				if (oldLayer != null)
					dp.Map.Layers.Remove(oldLayer);
				if (newLayer != null && dp.IsChecked.Value)
					dp.Map.Layers.Add(newLayer);
				
			}
			if (dp.Configurator != null)
			{
				dp.Configurator.Layer = newLayer;
				if (newLayer != null)
					dp.Configurator.Title = newLayer.ID;
			}
		}


		/// <summary>
		/// Identifies the <see cref="Configurator"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ConfiguratorProperty =
						DependencyProperty.Register("Configurator", typeof(ConfiguratorContainer), typeof(LayerCheckBox),
						new PropertyMetadata(null, OnConfiguratorPropertyChanged));
		/// <summary>
		/// Gets or sets Configurator.
		/// </summary>
		public ConfiguratorContainer Configurator
		{
			get { return (ConfiguratorContainer)GetValue(ConfiguratorProperty); }
			set { SetValue(ConfiguratorProperty, value); }
		}
		/// <summary>
		/// ConfiguratorProperty property changed handler. 
		/// </summary>
		/// <param name="d">LayerCheckBox that changed its Configurator.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnConfiguratorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			LayerCheckBox dp = d as LayerCheckBox;
			ConfiguratorContainer newPanel = (ConfiguratorContainer)e.NewValue;
			ConfiguratorContainer oldPanel = (ConfiguratorContainer)e.OldValue;
			if (oldPanel != null) oldPanel.Layer = null;
			if (newPanel != null)
			{
				newPanel.Layer = dp.Layer;
				if (dp.Layer != null)
					newPanel.Title = dp.Layer.ID;
			}
		}



		public Panel ConfiguratorParent
		{
			get { return (Panel)GetValue(ConfiguratorParentProperty); }
			set { SetValue(ConfiguratorParentProperty, value); }
		}

		public static readonly DependencyProperty ConfiguratorParentProperty =
			DependencyProperty.Register("ConfiguratorParent", typeof(Panel), typeof(LayerCheckBox), null);

	}
}
