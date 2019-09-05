using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// Extracts the magnitude from the GeoRss feed title.
	/// </summary>
	public sealed class MagnitudeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Dictionary<string, object> attributes = value as Dictionary<string, object>;
			if (attributes == null || !attributes.ContainsKey("Title")) return 0.0;
			string title = (string)attributes["Title"];
			string[] parts = title.Split(new char[] { ' ' });
			double scale = 1;
			string val = parts[1].Replace(",", "");
			if (parameter != null)
			{
				scale = Double.Parse((string)parameter, CultureInfo.InvariantCulture);
			}
			return Double.Parse(val, CultureInfo.InvariantCulture) * scale;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
