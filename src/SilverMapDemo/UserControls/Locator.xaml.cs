using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ESRI.ArcGIS.Samples
{
	public partial class LocatorControl : UserControl
	{
		public LocatorControl()
		{
			InitializeComponent();
		}

		#region Search Location

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			tb.GotFocus -= TextBox_GotFocus;
			tb.Text = "";
			tb.Foreground = new SolidColorBrush(Colors.White);
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (string.IsNullOrEmpty(tb.Text))
			{
				tb.Text = "Enter a location...";
				tb.Foreground = new SolidColorBrush(Colors.Gray);
				tb.GotFocus += TextBox_GotFocus;
			}
		}

		private void SearchLocation_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;
			TextBox tb = sender as TextBox;

			Locator locatorTask = new Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/ESRI_Places_World/GeocodeServer");
			locatorTask.AddressToLocationsCompleted += LocatorTask_AddressToLocationsCompleted;
			//locatorTask.Failed += LocatorTask_Failed;
			AddressToLocationsParameters addressParams = new AddressToLocationsParameters();
			Dictionary<string, string> address = addressParams.Address;
			addressParams.OutFields.Add("North_Lat");
			addressParams.OutFields.Add("South_Lat");
			addressParams.OutFields.Add("West_Lon");
			addressParams.OutFields.Add("East_Lon");
			addressParams.OutFields.Add("Descr");

			if (!string.IsNullOrEmpty(tb.Text))
				address.Add("PlaceName", tb.Text);
			else return;

			locatorTask.AddressToLocationsAsync(addressParams);
		}

		private void LocatorTask_AddressToLocationsCompleted(object sender, AddressToLocationsEventArgs args)
		{
			List<AddressCandidate> returnedCandidates = args.Results;
			if (returnedCandidates.Count == 1)
			{
				AddressCandidate best = returnedCandidates[0];
				Envelope env = new Envelope()
				{
					XMin = double.Parse(best.Attributes["West_Lon"].ToString(), CultureInfo.InvariantCulture),
					YMin = double.Parse(best.Attributes["South_Lat"].ToString(), CultureInfo.InvariantCulture),
					XMax = double.Parse(best.Attributes["East_Lon"].ToString(), CultureInfo.InvariantCulture),
					YMax = double.Parse(best.Attributes["North_Lat"].ToString(), CultureInfo.InvariantCulture)
				};

				Map.ZoomTo(env);
			}
			else if (returnedCandidates.Count > 1)
			{
				locationResults.Visibility = Visibility.Visible;
				locationResults.ItemsSource = returnedCandidates;
				Dispatcher.BeginInvoke(() =>
				{
					locationResults.Focus();
				});
			}
		}

		private void locationResults_LostFocus(object sender, RoutedEventArgs e)
		{
			locationResults.Visibility = Visibility.Collapsed;
			locationResults.ItemsSource = null;
		}

		private void locationResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			AddressCandidate best = locationResults.SelectedItem as AddressCandidate;
			if (best == null) return;
			Envelope env = new Envelope()
			{
				XMin = double.Parse(best.Attributes["West_Lon"].ToString(), CultureInfo.InvariantCulture),
				YMin = double.Parse(best.Attributes["South_Lat"].ToString(), CultureInfo.InvariantCulture),
				XMax = double.Parse(best.Attributes["East_Lon"].ToString(), CultureInfo.InvariantCulture),
				YMax = double.Parse(best.Attributes["North_Lat"].ToString(), CultureInfo.InvariantCulture)
			};

			Map.ZoomTo(env);
			locationResults.Visibility = Visibility.Collapsed;
			locationResults.ItemsSource = null;
		}

		private void locationResults_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width > 0)
			{
					RearrangeLocationDropdown(e.NewSize);
			}
		}

		private void RearrangeLocationDropdown(Size size)
		{
			double offset = 0;
			//Get placement of popup so it pops up under textbox
			Point p = locationTextBox.TransformToVisual(locationResultsPopUp.Parent as UIElement).Transform(new Point(0, locationTextBox.ActualHeight));
			//If dropdown gets clipped to the right, move it a bit to the left
			Point p2 = locationResultsPopUp.TransformToVisual(Application.Current.RootVisual).Transform(new Point(size.Width+p.X, 0));
			if ((Application.Current.RootVisual as FrameworkElement).ActualWidth < p2.X)
				offset = (Application.Current.RootVisual as FrameworkElement).ActualWidth - p2.X;
			locationResultsPopUp.HorizontalOffset = p.X + offset;
			locationResultsPopUp.VerticalOffset = p.Y;
		}

		#endregion Search Location



		public ESRI.ArcGIS.Client.Map Map
		{
			get { return (ESRI.ArcGIS.Client.Map)GetValue(MapProperty); }
			set { SetValue(MapProperty, value); }
		}

		public static readonly DependencyProperty MapProperty =
			DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(LocatorControl), null);
	}
}
