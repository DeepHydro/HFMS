using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Samples
{
	public class SimpleRenderer : ESRI.ArcGIS.Client.IRenderer
	{
		#region IRenderer Members

		public ESRI.ArcGIS.Client.Symbols.Symbol GetSymbol(ESRI.ArcGIS.Client.Graphic graphic)
		{
			if (graphic.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon ||
				graphic.Geometry is ESRI.ArcGIS.Client.Geometry.Envelope)
				return new SimpleFillSymbol()
				{
					Fill = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0, 0)),
					BorderThickness = 2,
					BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0))
				};
			else if (graphic.Geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
				return new SimpleLineSymbol()
				{
					Color = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0)),
					Width = 3
				};
			else return new SimpleMarkerSymbol();
		}

		#endregion
	}

	public class Utilities
	{


		public static string GetToggleGroup(DependencyObject obj)
		{
			return (string)obj.GetValue(ToggleGroupProperty);
		}

		public static void SetToggleGroup(DependencyObject obj, string value)
		{
			obj.SetValue(ToggleGroupProperty, value);
		}

		// Using a DependencyProperty as the backing store for ToggleGroup.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ToggleGroupProperty =
			DependencyProperty.RegisterAttached("ToggleGroup", typeof(string), typeof(Utilities), new PropertyMetadata(null, OnToggleGroupPropertyChanged));


		private static void OnToggleGroupPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
		{
			if (!(dp is ToggleButton))
				throw new ArgumentException("Toggle Group can only be set on ToggleButton");
			ToggleButton button = dp as ToggleButton;
			string newID = (string)args.NewValue;
			string oldID = (string)args.OldValue;
			if (oldID != null && groups.ContainsKey(oldID))
			{
				groups[newID].Remove(button);
				button.Checked -= button_Checked;
			}
			if (newID != null)
			{
				if (!groups.ContainsKey(newID))
				{
					groups.Add(newID, new List<ToggleButton>());
				}
				groups[newID].Add(button);
				button.Checked += button_Checked;
			}
		}

		private static void button_Checked(object sender, RoutedEventArgs e)
		{
			ToggleButton button = sender as ToggleButton;
			string ID = GetToggleGroup(button);
			if (ID != null && groups.ContainsKey(ID))
			{
				foreach (ToggleButton b in groups[ID])
				{
					if(b != button)
						b.IsChecked = false;
				}
			}
		}
		private static Dictionary<string, List<ToggleButton>> groups = new Dictionary<string,List<ToggleButton>>();
	}
}
