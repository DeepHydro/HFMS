using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// ESRI logo
	/// </summary>
	public partial class EsriLogo : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EsriLogo"/> class.
		/// </summary>
		public EsriLogo()
		{
			// Required to initialize variables
			InitializeComponent();
			this.SizeChanged += new SizeChangedEventHandler(EsriLogo_SizeChanged);
		}

		private void EsriLogo_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			//this.RenderTransformOrigin = new Point(0.5, 0.5);
			double scale = Math.Min(e.NewSize.Width / 400, e.NewSize.Height / 475);
			this.Logo.RenderTransform = new ScaleTransform()
			{ 
				ScaleX = scale,
				ScaleY = scale
			};
		}

		/// <summary>
		/// Begins the show animation.
		/// </summary>
		public void BeginShowAnimation()
		{
			Storyboard show = this.Resources["ShowKey"] as Storyboard;

			show.Begin();
			show.Completed += new EventHandler(show_Completed);
		}

		/// <summary>
		/// Begins the hide animation.
		/// </summary>
		public void BeginHideAnimation()
		{
			Storyboard show = this.Resources["HideKey"] as Storyboard;
			show.Begin();
			show.Completed += new EventHandler(hide_Completed);
		}

		private void show_Completed(object sender, EventArgs e)
		{
			if (ShowCompleted != null) ShowCompleted(this, new EventArgs());
		}

		private void hide_Completed(object sender, EventArgs e)
		{
			if (HideCompleted != null) HideCompleted(this, new EventArgs());
		}

		/// <summary>
		/// Occurs when the show animation has completed.
		/// </summary>
		public event EventHandler ShowCompleted;
		/// <summary>
		/// Occurs when the hide animation has completed.
		/// </summary>
		public event EventHandler HideCompleted;

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			BeginShowAnimation();
		}
	}
}