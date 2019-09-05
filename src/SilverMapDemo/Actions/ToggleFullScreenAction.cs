﻿using System.Windows;
using System.Windows.Interactivity;

namespace ESRI.ArcGIS.Samples
{
	public class ToggleFullScreenAction : TriggerAction<UIElement>
	{
		protected override void Invoke(object parameter)
		{
			Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
		}
	}
}
