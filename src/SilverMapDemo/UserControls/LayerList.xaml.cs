using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESRI.ArcGIS.Samples
{
	public partial class LayerList : UserControl
	{
		public LayerList()
		{
			InitializeComponent();
		}

		public class Layer
		{
			public Layer(ESRI.ArcGIS.Client.Layer layer, int index)
			{
				Index = index;
				ID = layer.ID;
				Opacity = layer.Opacity;
				Visible = layer.Visible;
			}
			public int Index { get; set; }
			public string ID { get; set; }
			public double Opacity { get; set; }
			public bool Visible { get; set; }
		}

		private void UpdateLayers()
		{
			List<Layer> layers = new List<Layer>();
			if (Map != null)
			{
				int i=0;
				foreach(ESRI.ArcGIS.Client.Layer l in Map.Layers)
				{
					if (i == 0 || l.ID == "Annotations" || string.IsNullOrEmpty(l.ID))
					{
						i++;
						continue; //hide base layer
					}
					layers.Insert(0, new Layer(l, i++));
				}
			}
			list.Visibility = (layers.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
			noLayers.Visibility = (layers.Count != 0) ? Visibility.Collapsed : Visibility.Visible;
			list.ItemsSource = layers;
		}


		/// <summary>
		/// Identifies the <see cref="Map"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MapProperty =
						DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(LayerList),
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
		/// <param name="d">LayersControl that changed its Map.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			LayerList dp = d as LayerList;
			ESRI.ArcGIS.Client.Map newMap = (ESRI.ArcGIS.Client.Map)e.NewValue;
			ESRI.ArcGIS.Client.Map oldMap = (ESRI.ArcGIS.Client.Map)e.OldValue;
			if (oldMap != null)
				newMap.Layers.CollectionChanged -= dp.Layers_CollectionChanged;
			if (newMap != null)
				newMap.Layers.CollectionChanged += dp.Layers_CollectionChanged;
			dp.UpdateLayers();
		}

		private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateLayers();
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Slider slider = sender as Slider;
			Layer l = slider.DataContext as Layer;
			Map.Layers[l.Index].Opacity = slider.Value;
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			CheckBox cb = sender as CheckBox;
			Layer l = cb.DataContext as Layer;
			Map.Layers[l.Index].Visible = cb.IsChecked.Value;
		}

		private void MoveUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement cb = sender as FrameworkElement;
			Layer l = cb.DataContext as Layer;
			Reorder(l.Index, 1);
		}

		private void MoveDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement cb = sender as FrameworkElement;
			Layer l = cb.DataContext as Layer;
			if (l.ID == "Annotations") return;
			Reorder(l.Index, -1);
		}
		private void Reorder(int layerIndex, int direction)
		{
			ESRI.ArcGIS.Client.Layer l = Map.Layers[layerIndex];
			if (l == null || direction == 0) return;
			int newIndex = layerIndex + direction;
			if (newIndex < 1) newIndex = 1;
			if (newIndex >= Map.Layers.Count) newIndex = Map.Layers.Count - 1;
			if (Map.Layers[newIndex].ID == "Annotations") newIndex--;
			if (newIndex == layerIndex) return;
			Map.Layers.Remove(l);
			//if (direction > 0) newIndex--;
			Map.Layers.Insert(newIndex, l);
		}
	}
}
