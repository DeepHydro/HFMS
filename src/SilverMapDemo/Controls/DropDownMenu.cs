using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// Resizable and draggable custom window control
	/// </summary>
	[ContentProperty("Content")]
	public class DropDownMenu : ContentControl
	{
		private DropDownMenuWindow popup;

		public DropDownMenu()
		{
			DefaultStyleKey = typeof(DropDownMenu);
			this.MouseEnter += DropDownMenu_MouseEnter;
			this.MouseLeave += DropDownMenu_MouseLeave;
		}


		private void HideMenu()
		{
			if (popup != null)
			{
				popup.IsVisible = false;
			}
		}

		private void ShowMenu()
		{
			if (popup != null)
			{
				popup.IsVisible = true;
			}
		}

		private void DropDownMenu_MouseEnter(object sender, MouseEventArgs e)
		{
			ShowMenu();
		}

		private void DropDownMenu_MouseLeave(object sender, MouseEventArgs e)
		{
			HideMenu();
		}

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code or
		/// internal processes (such as a rebuilding layout pass) call
		/// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			popup = GetTemplateChild("Menu") as DropDownMenuWindow;
			if (popup != null)
			{
				(popup.Content as UIElement).MouseEnter += DropDownMenu_MouseEnter;
				(popup.Content as UIElement).MouseLeave += DropDownMenu_MouseLeave;
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			Size s = base.MeasureOverride(availableSize);
			if (popup != null)
			{
				GeneralTransform t = this.TransformToVisual(Application.Current.RootVisual);
				Point p = t.Transform(new Point(0, s.Height));
                popup.HorizontalOffset = 0;
                popup.VerticalOffset = s.Height;
			}
			return s;
		}

		/// <summary>
		/// Identifies the <see cref="MenuContent"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MenuContentProperty =
						DependencyProperty.Register("MenuContent", typeof(object), typeof(DropDownMenu),null);
		/// <summary>
		/// Gets or sets MenuContent.
		/// </summary>
		public object MenuContent
		{
			get { return (object)GetValue(MenuContentProperty); }
			set { SetValue(MenuContentProperty, value); }
		}
	}
}