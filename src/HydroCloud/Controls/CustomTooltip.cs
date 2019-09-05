using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcGIS.Samples
{
	public class CustomTooltip : DependencyObject
	{
		public static object GetToolTip(DependencyObject obj)
		{
			return (object)obj.GetValue(ToolTipProperty);
		}

		public static void SetToolTip(DependencyObject obj, object value)
		{
			obj.SetValue(ToolTipProperty, value);
		}

		public static readonly DependencyProperty ToolTipProperty =
			DependencyProperty.RegisterAttached("ToolTip", typeof(string), typeof(CustomTooltip), new PropertyMetadata("", OnTooltipPropertyChanged));

		private static void OnTooltipPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
		{
			string value = args.NewValue as string;
			TextBlock tb = new TextBlock() { TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
			tb.Text = value.Replace("\\n", "\n");
			ToolTipService.SetToolTip(dp, tb);
		}
	}
}
