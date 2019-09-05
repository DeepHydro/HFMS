﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// Resizable and draggable custom window control
	/// </summary>
	[TemplatePart(Name = "btnClose", Type = typeof(Button))] //Close button
	[TemplatePart(Name = "TopBar", Type = typeof(UIElement))] //top bar / area used for dragging the window
	[TemplatePart(Name = "ResizeWidth", Type = typeof(UIElement))] //top bar / area used for dragging the window
	[TemplatePart(Name = "ResizeHeight", Type = typeof(UIElement))] //top bar / area used for dragging the window
	[TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
	[TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
	[TemplateVisualState(GroupName = "CommonStates", Name = "Dragging")]
	[TemplateVisualState(GroupName = "CommonStates", Name = "Focus")]
	[ContentProperty("Content")]
	public class DraggableWindow : ContentControl
	{
		UIElement topbar;
		UIElement ResizeWidth;
		UIElement ResizeHeight;
		Point lastPoint;
		bool isMouseOver = false;
		bool isDragging = false;
		bool hasFocus = false;

		public DraggableWindow()
		{
			DefaultStyleKey = typeof(DraggableWindow);
			this.MouseEnter += (s, e) => { this.isMouseOver = true; ChangeVisualState(true); };
			this.MouseLeave += (s, e) => { this.isMouseOver = false; ChangeVisualState(true); };
			this.GotFocus += (s, e) => { this.hasFocus = true; ChangeVisualState(true); };
			this.LostFocus += (s, e) => { this.hasFocus = false; ChangeVisualState(true); };
		}

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code or
		/// internal processes (such as a rebuilding layout pass) call
		/// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Button btnClose = GetTemplateChild("btnClose") as Button;
			if (btnClose != null)
			{
				btnClose.Click += (s, e) => { this.IsOpen = false; };
			}

			topbar = GetTemplateChild("TopBar") as UIElement;
			if (topbar != null)
			{
				topbar.MouseLeftButtonDown += topbar_MouseLeftButtonDown;
			}

			this.RenderTransform = new TranslateTransform();

			ResizeWidth = GetTemplateChild("ResizeWidth") as UIElement;
			ResizeHeight = GetTemplateChild("ResizeHeight") as UIElement;
			if (ResizeWidth != null)
			{
				ResizeWidth.MouseLeftButtonDown += Resize_MouseLeftButtonDown;
				ResizeWidth.IsHitTestVisible = IsWidthResizeable;
			}
			if (ResizeHeight != null)
			{
				ResizeHeight.MouseLeftButtonDown += Resize_MouseLeftButtonDown;
				ResizeHeight.IsHitTestVisible = IsHeightResizeable;
			}

			ChangeVisualState(false);
		}
		bool resizingWidth;

		private void Resize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			resizingWidth = (sender == ResizeWidth);
			if (resizingWidth && double.IsNaN(this.Width))
				return; //not supported/disabled
			else if (!resizingWidth && double.IsNaN(this.Height))
				return; //not supported/disabled
			lastPoint = e.GetPosition(this.Parent as UIElement);
			Application.Current.RootVisual.MouseMove += Resize_MouseMove;
			Application.Current.RootVisual.MouseLeftButtonUp += Resize_MouseLeftButtonUp;
			Application.Current.RootVisual.MouseLeave += Resize_MouseLeave;
			e.Handled = true;
		}
		private void Resize_MouseMove(object sender, MouseEventArgs e)
		{
			Point p2 = e.GetPosition(this.Parent as UIElement);
			if (resizingWidth)
			{
				double d =  p2.X - lastPoint.X;
				this.Width += d;
				if (this.HorizontalAlignment == HorizontalAlignment.Center)
					this.HorizontalOffset += d / 2; 
				else if (this.HorizontalAlignment == HorizontalAlignment.Right)
				    this.HorizontalOffset += d;
			}
			else
			{
				double d = p2.Y - lastPoint.Y;
				this.Height += d;
				if(this.VerticalAlignment ==  VerticalAlignment.Bottom)
					this.VerticalOffset += d;
				else if (this.VerticalAlignment == VerticalAlignment.Center)
					this.VerticalOffset += d / 2; 

			}
			lastPoint = p2;
			
		}
		private void Resize_MouseLeave(object sender, MouseEventArgs e)
		{
			StopResize();
		}
		private void Resize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			StopResize();
		}

		private void StopResize()
		{
			Application.Current.RootVisual.MouseMove -= Resize_MouseMove;
			Application.Current.RootVisual.MouseLeftButtonUp -= Resize_MouseLeftButtonUp;
			Application.Current.RootVisual.MouseLeave -= Resize_MouseLeave;
		}

		/// <summary>
		/// Starts tragging window drag
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void topbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!IsDraggable) return;
			lastPoint = e.GetPosition(this.Parent as UIElement);
			Application.Current.RootVisual.MouseMove += RootVisual_MouseMove;
			Application.Current.RootVisual.MouseLeftButtonUp += RootVisual_MouseLeftButtonUp;
			Application.Current.RootVisual.MouseLeave += RootVisual_MouseLeave;
			e.Handled = true;
		}

		private void RootVisual_MouseLeave(object sender, MouseEventArgs e)
		{
			StopDrag();
		}

		private void RootVisual_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			StopDrag();
		}

		/// <summary>
		/// Stops tracking window drag.
		/// </summary>
		private void StopDrag()
		{
			isDragging = false;
			Application.Current.RootVisual.MouseMove -= RootVisual_MouseMove;
			Application.Current.RootVisual.MouseLeftButtonUp -= RootVisual_MouseLeftButtonUp;
			Application.Current.RootVisual.MouseLeave -= RootVisual_MouseLeave;
			ChangeVisualState(true);
		}

		private void RootVisual_MouseMove(object sender, MouseEventArgs e)
		{
			isDragging = true;
			ChangeVisualState(true);
			TranslateTransform t = this.RenderTransform as TranslateTransform;
			Point p2 = e.GetPosition(this.Parent as UIElement);
			double dX = p2.X - lastPoint.X;
			double dY = p2.Y - lastPoint.Y;
			HorizontalOffset += dX;
			VerticalOffset += dY;
			lastPoint = p2;
		}
		
		private void ChangeVisualState(bool useTransitions)
		{
			if (isDragging)
			{
				GoToState(useTransitions, "Dragging");
			}
			else if (hasFocus)
			{
				GoToState(useTransitions, "Focus");
			}
			else if (isMouseOver)
			{
				GoToState(useTransitions, "MouseOver");
			}
			else
			{
				GoToState(useTransitions, "Normal");
			}
		}

		private bool GoToState(bool useTransitions, string stateName)
		{
			return VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		#region Dependency Properties

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TitleProperty =
						DependencyProperty.Register("Title", typeof(string), typeof(DraggableWindow), null);
		/// <summary>
		/// Gets or sets Title.
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="IsOpen"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsOpenProperty =
						DependencyProperty.Register("IsOpen", typeof(bool), typeof(DraggableWindow),
						new PropertyMetadata(true, OnIsOpenPropertyChanged));
		/// <summary>
		/// Gets or sets IsOpen.
		/// </summary>
		public bool IsOpen
		{
			get { return (bool)GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}
		/// <summary>
		/// IsOpenProperty property changed handler. 
		/// </summary>
		/// <param name="d">Window that changed its IsOpen.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DraggableWindow dp = d as DraggableWindow;
			bool isOpen = (bool)e.NewValue;
			dp.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
			if (isOpen)
				dp.OnOpened();
			else
				dp.OnClosed();
		}

		/// <summary>
		/// Identifies the <see cref="HorizontalOffset"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty HorizontalOffsetProperty =
						DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(DraggableWindow),
						new PropertyMetadata(0.0, OnHorizontalOffsetPropertyChanged));
		/// <summary>
		/// Gets or sets HorisontalOffset.
		/// </summary>
		public double HorizontalOffset
		{
			get { return (double)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}
		/// <summary>
		/// HorisontalOffsetProperty property changed handler. 
		/// </summary>
		/// <param name="d">Window that changed its HorisontalOffset.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnHorizontalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DraggableWindow dp = d as DraggableWindow;
			if (dp.RenderTransform is TranslateTransform)
				(dp.RenderTransform as TranslateTransform).X = (double)e.NewValue;
		}

		/// <summary>
		/// Identifies the <see cref="VerticalOffset"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty VerticalOffsetProperty =
						DependencyProperty.Register("VerticalOffset", typeof(double), typeof(DraggableWindow),
						new PropertyMetadata(0.0, OnVerticalOffsetPropertyChanged));
		/// <summary>
		/// Gets or sets VerticalOffset.
		/// </summary>
		public double VerticalOffset
		{
			get { return (double)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}
		/// <summary>
		/// VerticalOffsetProperty property changed handler. 
		/// </summary>
		/// <param name="d">Window that changed its VerticalOffset.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DraggableWindow dp = d as DraggableWindow;
			if (dp.RenderTransform is TranslateTransform)
				(dp.RenderTransform as TranslateTransform).Y = (double)e.NewValue;
		}

		/// <summary>
		/// Identifies the <see cref="IsDraggable"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsDraggableProperty =
						DependencyProperty.Register("IsDraggable", typeof(bool), typeof(DraggableWindow),
						new PropertyMetadata(true));
		/// <summary>
		/// Gets or sets IsDraggable.
		/// </summary>
		public bool IsDraggable
		{
			get { return (bool)GetValue(IsDraggableProperty); }
			set { SetValue(IsDraggableProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="IsWidthResizeable"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsWidthResizeableProperty =
						DependencyProperty.Register("IsWidthResizeable", typeof(bool), typeof(DraggableWindow),
						new PropertyMetadata(true));
		/// <summary>
		/// Gets or sets IsWidthResizeable.
		/// </summary>
		public bool IsWidthResizeable
		{
			get { return (bool)GetValue(IsWidthResizeableProperty); }
			set { SetValue(IsWidthResizeableProperty, value); }
		}

		/// <summary>
		/// IsWidthResizeableProperty property changed handler. 
		/// </summary>
		/// <param name="d">Window that changed its IsWidthResizeable.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnIsWidthResizeablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DraggableWindow dp = d as DraggableWindow;
			if (dp.ResizeWidth != null)
			{
				dp.ResizeWidth.IsHitTestVisible = (bool)e.NewValue;
			}
		}

		/// <summary>
		/// Identifies the <see cref="IsHeightResizeable"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsHeightResizeableProperty =
						DependencyProperty.Register("IsHeightResizeable", typeof(bool), typeof(DraggableWindow),
						new PropertyMetadata(true, OnIsHeightResizeablePropertyChanged));
		/// <summary>
		/// Gets or sets IsHeightResizeable.
		/// </summary>
		public bool IsHeightResizeable
		{
			get { return (bool)GetValue(IsHeightResizeableProperty); }
			set { SetValue(IsHeightResizeableProperty, value); }
		}

		/// <summary>
		/// IsHeightResizeableProperty property changed handler. 
		/// </summary>
		/// <param name="d">Window that changed its VerticalOffset.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnIsHeightResizeablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DraggableWindow dp = d as DraggableWindow;
			if (dp.ResizeHeight != null)
			{
				dp.ResizeHeight.IsHitTestVisible = (bool)e.NewValue;
			}
		}

		#endregion

		#region Events

		public event System.EventHandler Closed;
		public event System.EventHandler Opened;

		protected void OnClosed()
		{
			if(Closed!=null)
			{
				Closed(this, new System.EventArgs());
			}
		}
		protected void OnOpened()
		{
			if (Opened != null)
			{
				Opened(this, new System.EventArgs());
			}
		}

		#endregion
	}
}