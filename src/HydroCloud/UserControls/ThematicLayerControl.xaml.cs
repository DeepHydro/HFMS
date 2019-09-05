using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Samples
{
	public partial class ThematicLayerControl : UserControl
	{
		public GraphicsLayer Layer
		{
			get { return (GraphicsLayer)GetValue(LayerProperty); }
			set { SetValue(LayerProperty, value); }
		}

		public static readonly DependencyProperty LayerProperty =
			DependencyProperty.Register("Layer", typeof(GraphicsLayer), typeof(ThematicLayerControl), null);


		List<ThematicItem> ThematicItemList = new List<ThematicItem>();
		List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();
		FeatureSet _featureSet = null;
		int _lastGeneratedClassCount = 0;
		
		public struct ThematicItem
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public double Min { get; set; }
			public double Max { get; set; }
			public List<double> RangeStarts { get; set; }
		}

		public class ThemeColor
		{
			public ThemeColor(string name, Color color)
			{
				Name = name; Background = new SolidColorBrush(color);
			}
			public string Name { get; set; }
			public Brush Background { get; set; }
		}

		public ThematicLayerControl()
		{
			InitializeComponent();
			this.Loaded += new RoutedEventHandler(ThematicLayerControl_Loaded);
		}

		void ThematicLayerControl_Loaded(object sender, RoutedEventArgs e)
		{
			ClassCountCombo.ItemsSource = new int[] { 3, 4, 5, 6 };
			ClassTypeCombo.ItemsSource = new string[] { "Quantile", "Equal Interval" };
			ColorBlendCombo.SelectedIndex = ClassTypeCombo.SelectedIndex = ClassCountCombo.SelectedIndex = 0;
			// Get start value for number of classifications in XAML.
			_lastGeneratedClassCount = 3;

			// Set query where clause to include features with an area greater than 70 square miles.  This 
			// will effectively exclude the District of Columbia from attributes to avoid skewing classifications.
			ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
			{
				Where = "SQMI > 70"
			};
			query.OutFields.Add("*");

			QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
				"Demographics/ESRI_Census_USA/MapServer/5");

			queryTask.ExecuteCompleted += (evtsender, args) =>
			{
				if (args.FeatureSet == null)
					return;
				_featureSet = args.FeatureSet;


				if (_featureSet != null && _featureSet.Features.Count > 0)
				{
					// Clear previous graphic features
					Layer.ClearGraphics();

					for (int i = 0; i < _featureSet.Features.Count; i++)
					{
						Layer.Graphics.Add(_featureSet.Features[i]);
					}
					SetRangeValues();
					//					RenderButton.IsEnabled = true;
				}
			};
			queryTask.ExecuteAsync(query);

			CreateColorList();
			CreateThematicList();
		}
		private void CreateColorList()
		{
			ColorList = new List<List<SolidColorBrush>>();

			List<SolidColorBrush> BlueShades = new List<SolidColorBrush>();
			List<SolidColorBrush> RedShades = new List<SolidColorBrush>();
			List<SolidColorBrush> GreenShades = new List<SolidColorBrush>();
			List<SolidColorBrush> YellowShades = new List<SolidColorBrush>();
			List<SolidColorBrush> MagentaShades = new List<SolidColorBrush>();
			List<SolidColorBrush> CyanShades = new List<SolidColorBrush>();

			int _classCount = (int)ClassCountCombo.SelectedItem;
			int rgbFactor = 255 / _classCount;

			for (int j = 0; j < 256; j = j + rgbFactor)
			{
				BlueShades.Add(new SolidColorBrush(Color.FromArgb(255, (byte)j, (byte)j, 255)));
				RedShades.Add(new SolidColorBrush(Color.FromArgb(255, 255, (byte)j, (byte)j)));
				GreenShades.Add(new SolidColorBrush(Color.FromArgb(255, (byte)j, 255, (byte)j)));
				YellowShades.Add(new SolidColorBrush(Color.FromArgb(255, 255, 255, (byte)j)));
				MagentaShades.Add(new SolidColorBrush(Color.FromArgb(255, 255, (byte)j, 255)));
				CyanShades.Add(new SolidColorBrush(Color.FromArgb(255, (byte)j, 255, 255)));
			}

			ColorList.Add(BlueShades);
			ColorList.Add(RedShades);
			ColorList.Add(GreenShades);
			ColorList.Add(YellowShades);
			ColorList.Add(MagentaShades);
			ColorList.Add(CyanShades);

			foreach (List<SolidColorBrush> brushList in ColorList)
			{
				brushList.Reverse();
			}

			List<SolidColorBrush> MixedShades = new List<SolidColorBrush>();
			if (_classCount > 5) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 255, 255)));
			if (_classCount > 4) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 0, 255)));
			if (_classCount > 3) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 255, 0)));
			MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 255, 0)));
			MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 0, 255)));
			MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 0, 0)));
			ColorList.Add(MixedShades);

			_lastGeneratedClassCount = _classCount;
		}

		private void CreateThematicList()
		{
			ThematicItemList.Add(new ThematicItem() { Name = "POP2007", Description = "2007 Population " });
			ThematicItemList.Add(new ThematicItem() { Name = "POP07_SQMI", Description = "2007 Pop per Sq Mi" });
			ThematicItemList.Add(new ThematicItem() { Name = "MED_AGE", Description = "Median age" });
			ThematicItemList.Add(new ThematicItem() { Name = "SQMI", Description = "Total SqMi" });
			FieldCombo.ItemsSource = ThematicItemList;
			FieldCombo.SelectedIndex = 0;
		}

		private void SetRangeValues()
		{
			if (this.Layer == null) return;
			
			// if necessary, update ColorList based on current number of classes.
			int _classCount = (int)ClassCountCombo.SelectedItem;
			if (_lastGeneratedClassCount != _classCount) CreateColorList();

			// Field on which to generate a classification scheme.  
			ThematicItem thematicItem = ThematicItemList[FieldCombo.SelectedIndex];

			// Store a list of values to classify
			List<double> valueList = new List<double>();

			// Get range, min, max, etc. from features
			for (int i = 0; i < _featureSet.Features.Count; i++)
			{
				Graphic graphicFeature = _featureSet.Features[i];

				double graphicValue = Convert.ToDouble(graphicFeature.Attributes[thematicItem.Name]);

				if (i == 0)
				{
					thematicItem.Min = graphicValue;
					thematicItem.Max = graphicValue;
				}
				else
				{
					if (graphicValue < thematicItem.Min) { thematicItem.Min = graphicValue; }
					if (graphicValue > thematicItem.Max) { thematicItem.Max = graphicValue; }
				}

				valueList.Add(graphicValue);
			}

			// Set up range start values
			thematicItem.RangeStarts = new List<double>();

			double totalRange = thematicItem.Max - thematicItem.Min;
			double portion = totalRange / _classCount;

			thematicItem.RangeStarts.Add(thematicItem.Min);
			double startRangeValue = thematicItem.Min;

			// Equal Interval
			if (ClassTypeCombo.SelectedIndex == 1)
			{
				for (int i = 1; i < _classCount; i++)
				{
					startRangeValue += portion;
					thematicItem.RangeStarts.Add(startRangeValue);
				}
			}
			// Quantile
			else
			{
				// Enumerator of all values in ascending order
				IEnumerable<double> valueEnumerator =
					from aValue in valueList
					orderby aValue //"ascending" is default
					select aValue;

				int increment = Convert.ToInt32(Math.Ceiling(_featureSet.Features.Count / _classCount));
				for (int i = increment; i < valueList.Count; i += increment)
				{
					double value = valueEnumerator.ElementAt(i);
					thematicItem.RangeStarts.Add(value);
				}
			}

			// Create graphic features and set symbol using the class range which contains the value 
			List<SolidColorBrush> brushList = ColorList[ColorBlendCombo.SelectedIndex];
			if (_featureSet != null && _featureSet.Features.Count > 0)
			{
				ClassBreaksRenderer renderer = new ClassBreaksRenderer()
				{
					// Attribute = thematicItem.Name
				};
				for (int i = 1; i < thematicItem.RangeStarts.Count; i++)
				{
					renderer.Classes.Add(new ClassBreakInfo()
					{
						MinimumValue = thematicItem.RangeStarts[i - 1],
						MaximumValue = thematicItem.RangeStarts[i],
						Symbol = new SimpleFillSymbol()
						{
							Fill = brushList[i],
							BorderThickness = 0
						}
					});
				}
				renderer.Classes[0].MinimumValue = double.MinValue;
				renderer.Classes[renderer.Classes.Count - 1].MaximumValue = double.MaxValue;
				Layer.Renderer = renderer;
				Layer.Refresh();
			}
		}

		private void OnComboBoxChanged_Render(object sender, SelectionChangedEventArgs e)
		{
			SetRangeValues();
		}
	}
}
