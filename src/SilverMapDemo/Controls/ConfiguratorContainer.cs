using System;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Samples
{
	public class ConfiguratorContainer : ContentControl
	{
		public ConfiguratorContainer()
		{
			DefaultStyleKey = typeof(ConfiguratorContainer);
		}

		public ESRI.ArcGIS.Client.Layer Layer { get; set; }

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TitleProperty =
						DependencyProperty.Register("Title", typeof(String), typeof(ConfiguratorContainer), null);
		/// <summary>
		/// Gets or sets Title.
		/// </summary>
		public String Title
		{
			get { return (String)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}
	}
}
