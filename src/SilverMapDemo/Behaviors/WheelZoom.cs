using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// Adds mouse wheel zoom to the map control when running out of browser, full-screen or 
	/// DOM bridge is disabled.
	/// </summary>
	public class WheelZoom : Behavior<Map>
	{
		private double res = double.NaN;

		/// <summary>
		/// Initializes a new instance of the <see cref="WheelZoom"/> class.
		/// </summary>
		public WheelZoom()
		{
		}

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.MouseWheel += AssociatedObject_MouseWheel;
			this.AssociatedObject.ExtentChanged += AssociatedObject_ExtentChanged;
		}

		/// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, 
		/// but before it has actually occurred.
		/// </summary>
		/// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
		protected override void OnDetaching()
		{
			base.OnDetaching();
			this.AssociatedObject.MouseWheel -= AssociatedObject_MouseWheel;
			this.AssociatedObject.ExtentChanged -= AssociatedObject_ExtentChanged;
		}

		private void AssociatedObject_ExtentChanged(object sender, ExtentEventArgs e)
		{
			res = double.NaN;
		}

		private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			//Only handle zoom event if the map control cannot
			if (!System.Windows.Browser.HtmlPage.IsEnabled ||
				Application.Current.IsRunningOutOfBrowser ||
				Application.Current.Host.Content.IsFullScreen)
			{
				double factor = e.Delta < 1 ? this.AssociatedObject.ZoomFactor : 1 / this.AssociatedObject.ZoomFactor;
				MapPoint center = AssociatedObject.ScreenToMap(e.GetPosition(AssociatedObject));
				if (double.IsNaN(res))
					res = this.AssociatedObject.Resolution;
				res = res * factor; //accumulate zoom-to resolution when doing multiple zooms before the previous is complete
				this.AssociatedObject.ZoomToResolution(res, center);
				this.AssociatedObject.Focus();
			}
		}
	}
}
