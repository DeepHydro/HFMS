using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ESRI.ArcGIS.Samples
{
	[TemplateVisualState(GroupName = "CommonStates", Name = "Visible")]
	[TemplateVisualState(GroupName = "CommonStates", Name = "Hidden")]
	[TemplatePart(Name = "Popup", Type = typeof(Popup))]
	public class DropDownMenuWindow : ContentControl
	{
		bool isVisible = false;
		
		public DropDownMenuWindow()
		{
			this.DefaultStyleKey = typeof(DropDownMenuWindow);
		}

		public override void OnApplyTemplate()
		{
			ChangeVisualState(false);
			base.OnApplyTemplate();
		}

		private void ChangeVisualState(bool useTransitions)
		{
			if (isVisible)
			{
				GoToState(useTransitions, "Visible");
			}
			else
			{
				GoToState(useTransitions, "Hidden");
			}
		}

		private bool GoToState(bool useTransitions, string stateName)
		{
			return VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		/// <summary>
		/// Gets or sets IsVisible.
		/// </summary>
		public bool IsVisible
		{
			get { return isVisible; }
			set
			{
				isVisible = value;
				ChangeVisualState(true);
			}
		}

		/// <summary>
		/// Identifies the <see cref="VerticalOffset"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty VerticalOffsetProperty =
						DependencyProperty.Register("VerticalOffset", typeof(double), typeof(DropDownMenuWindow),
						new PropertyMetadata(0.0));

		/// <summary>
		/// Gets or sets VerticalOffset.
		/// </summary>
		public double VerticalOffset
		{
			get { return (double)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="HorizontalOffset"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty HorizontalOffsetProperty =
						DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(DropDownMenuWindow),
						new PropertyMetadata(0.0));

		/// <summary>
		/// Gets or sets HorizontalOffset.
		/// </summary>
		public double HorizontalOffset
		{
			get { return (double)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}
	}
}
