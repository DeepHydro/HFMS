using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Samples
{
	[TemplatePart(Name = "Progress", Type = typeof(ProgressBar))]
	[TemplatePart(Name = "ValueText", Type = typeof(TextBlock))]
	public class MapProgress : Control
	{
		ProgressBar bar;
		TextBlock text;
		bool isVisible = false;
		public MapProgress()
		{
			DefaultStyleKey = typeof(MapProgress);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			text = GetTemplateChild("ValueText") as TextBlock;
			bar = GetTemplateChild("Progress") as ProgressBar;
			ChangeVisualState(false);
		}

		private void ChangeVisualState(bool useTransitions)
		{
			bool ok = false;
			if (isVisible)
			{
				ok = GoToState(useTransitions, "Show");
			}
			else
			{
				ok = GoToState(useTransitions, "Hide");
			}
		}

		private bool GoToState(bool useTransitions, string stateName)
		{
			return VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		public ESRI.ArcGIS.Client.Map Map
		{
			get { return (ESRI.ArcGIS.Client.Map)GetValue(MapProperty); }
			set { SetValue(MapProperty, value); }
		}

		public static readonly DependencyProperty MapProperty =
			DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(MapProgress), new PropertyMetadata(null, OnMapPropertyChanged));

		private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MapProgress mp = d as MapProgress;
			Map oldMap = e.OldValue as Map;
			Map newMap = e.NewValue as Map;
			if (oldMap != null)
			{
				oldMap.Progress -= mp.newMap_Progress;
			}
			if (newMap != null)
			{
				newMap.Progress += mp.newMap_Progress;
			}
		}

		private void newMap_Progress(object sender, ProgressEventArgs e)
		{
			if (bar != null)
				bar.Value = e.Progress;
			if (text != null)
				text.Text = string.Format("{0}%", e.Progress);
			isVisible = (e.Progress < 99);
			ChangeVisualState(true);
		}
	}
}
