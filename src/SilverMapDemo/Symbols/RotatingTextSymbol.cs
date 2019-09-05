using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Samples
{
	public class RotatingTextSymbol : MarkerSymbol
	{

		private const string template = @"<ControlTemplate
					xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
					xmlns:local=""clr-namespace:{0};assembly={1}"">
						<Grid RenderTransformOrigin=""0.5,0.5"" Width=""100""
							  local:RotatingTextSymbol.AngleBinder=""{{Binding Path=Symbol.Angle}}"">
							<TextBlock FontWeight=""Bold"" Foreground=""White"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""
								Text=""{{Binding Symbol.Text}}"" >
								<TextBlock.Effect><BlurEffect Radius=""5"" /></TextBlock.Effect>
							</TextBlock>
							<TextBlock FontWeight=""Bold"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""
								Text=""{{Binding Symbol.Text}}"" />
						</Grid>
					</ControlTemplate>";

		public RotatingTextSymbol()
		{
			Type t = typeof(RotatingTextSymbol);
			Assembly asm = Assembly.GetCallingAssembly();
			string temp = string.Format(template, t.Namespace, t.Assembly.FullName.Split(new string[] { ", "}, StringSplitOptions.RemoveEmptyEntries)[0]);
			ControlTemplate = System.Windows.Markup.XamlReader.Load(temp) as ControlTemplate;
			this.OffsetX = 50;
		}
		

		/// <summary>
		/// Identifies the <see cref="Angle"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AngleProperty =
						DependencyProperty.Register("Angle", typeof(double), typeof(RotatingTextSymbol),
						new PropertyMetadata(0.0, OnAnglePropertyChanged));
		/// <summary>
		/// Gets or sets Angle.
		/// </summary>
		public double Angle
		{
			get { return (double)GetValue(AngleProperty); }
			set { SetValue(AngleProperty, value); }
		}

		/// <summary>
		/// AngleProperty property changed handler. 
		/// </summary>
		/// <param name="d">ownerclass that changed its Angle.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RotatingTextSymbol dp = d as RotatingTextSymbol;
			dp.OnPropertyChanged("Angle");
		}

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty =
						DependencyProperty.Register("Text", typeof(string), typeof(RotatingTextSymbol),
						new PropertyMetadata("", OnTextPropertyChanged));
		/// <summary>
		/// Gets or sets Text.
		/// </summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}


		/// <summary>
		/// AngleProperty property changed handler. 
		/// </summary>
		/// <param name="d">ownerclass that changed its Angle.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RotatingTextSymbol dp = d as RotatingTextSymbol;
			dp.OnPropertyChanged("Text");
		}


		public static readonly DependencyProperty AngleBinderProperty =
			DependencyProperty.RegisterAttached("AngleBinder", typeof(double),
			typeof(RotatingTextSymbol),
			new PropertyMetadata(0.0, OnAngleBinderChanged));

		public static double GetAngleBinder(DependencyObject d)
		{
			return (double)d.GetValue(AngleBinderProperty);
		}

		public static void SetAngleBinder(DependencyObject d, double value)
		{
			d.SetValue(AngleBinderProperty, value);
		}

		private static void OnAngleBinderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

			if (d is UIElement)
			{
				UIElement b = d as UIElement;
				if (e.NewValue is double)
				{
					double c = (double)e.NewValue;
					if (!double.IsNaN(c))
					{
						if (b.RenderTransform is RotateTransform)
							(b.RenderTransform as RotateTransform).Angle = c;
						else
							b.RenderTransform = new RotateTransform() { Angle = c };
					}
				}
			}
		}
	}
}
