using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ESRI.ArcGIS.Samples
{
	public class VideoController : Control
	{
        DispatcherTimer timer = new DispatcherTimer();

		Button PlayButton;
		Button PauseButton;
		Button StopButton;
		Slider ProgressBar;
		TextBlock CurrentTime;
		TextBlock Status;
		TextBlock TotalTime;
		FrameworkElement DownloadProgress;
		FrameworkElement Position;

		public VideoController()
		{
			DefaultStyleKey = typeof(VideoController);
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
            timer.Tick += (s,e) => { UpdatePlayBackProgress(); };
		}

		public override void OnApplyTemplate()
		{
			PlayButton = GetTemplateChild("PlayButton") as Button;
			PauseButton = GetTemplateChild("PauseButton") as Button;
			StopButton = GetTemplateChild("StopButton") as Button;
			if (PlayButton != null)
				PlayButton.Click += new RoutedEventHandler(PlayButton_Click);
			if (PauseButton != null)
				PauseButton.Click += new RoutedEventHandler(PauseButton_Click);
			if (StopButton != null)
				StopButton.Click += new RoutedEventHandler(StopButton_Click);
			ProgressBar = GetTemplateChild("ProgressBar") as Slider;
			CurrentTime = GetTemplateChild("CurrentTime") as TextBlock;
			Status = GetTemplateChild("Status") as TextBlock;
			TotalTime = GetTemplateChild("TotalTime") as TextBlock;
			DownloadProgress = GetTemplateChild("DownloadProgress") as FrameworkElement;
			if(DownloadProgress != null)
				DownloadProgress.MouseLeftButtonDown += DownloadProgress_MouseLeftButtonDown;
			Position = GetTemplateChild("Position") as FrameworkElement;
			SetTotalTime();
            SetCurrentState();
			SetDownloadProgress();
			base.OnApplyTemplate();
		}

		private void DownloadProgress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement parent = DownloadProgress.Parent as FrameworkElement;
			Point p = e.GetPosition(parent);
			double pos = p.X / parent.ActualWidth;
			if (Media != null)
			{
				Media.Position = TimeSpan.FromMilliseconds(Media.NaturalDuration.TimeSpan.TotalMilliseconds * pos);
				UpdatePlayBackProgress();
			}
		}

		private void PlayButton_Click(object sender, RoutedEventArgs e)
		{
			if (Media != null)
			{
				Media.Play();
			}
            timer.Start();
		}

		private void PauseButton_Click(object sender, RoutedEventArgs e)
		{
			if (Media != null)
			{
				Media.Pause();
			}
            timer.Stop();
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			if (Media != null)
			{
				Media.Stop();
			}
            timer.Stop();
            UpdatePlayBackProgress();
		}

        private void UpdatePlayBackProgress()
        {
			if (Media != null)
			{
				if (CurrentTime != null)
				{
					CurrentTime.Text = FormatTimeSpan(Media.Position);
				}
				if (Position != null)
				{
					double pos = Media.Position.TotalMilliseconds / Media.NaturalDuration.TimeSpan.TotalMilliseconds;
					FrameworkElement parent = Position.Parent as FrameworkElement;
					pos = parent.ActualWidth * pos;
                    //Position.Margin = new Thickness(pos - Position.ActualWidth*.5, 
                    //    Position.Margin.Top, Position.Margin.Right, Position.Margin.Bottom);
				}
			}
        }

		/// <summary>
		/// Identifies the <see cref="Media"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MediaProperty =
						DependencyProperty.Register("Media", typeof(MediaElement), typeof(VideoController),
						new PropertyMetadata(null, OnMediaPropertyChanged));
		/// <summary>
		/// Gets or sets Media.
		/// </summary>
		public MediaElement Media
		{
			get { return (MediaElement)GetValue(MediaProperty); }
			set { SetValue(MediaProperty, value); }
		}
		/// <summary>
		/// MediaProperty property changed handler. 
		/// </summary>
		/// <param name="d">VideoController that changed its Media.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnMediaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			VideoController dp = d as VideoController;
			MediaElement newElement = (MediaElement)e.NewValue;
			MediaElement oldElement = (MediaElement)e.OldValue;
			if (oldElement != null)
			{
				oldElement.BufferingProgressChanged -= dp.MediaBufferingProgressChanged;
				oldElement.CurrentStateChanged -= dp.MediaCurrentStateChanged;
				oldElement.MediaEnded -= dp.MediaEnded;
				oldElement.MediaFailed -= dp.MediaFailed;
				oldElement.MediaOpened -= dp.MediaOpened;
                newElement.DownloadProgressChanged -= dp.MediaDownloadProgressChanged;
			} 
			if (newElement != null)
			{
				newElement.BufferingProgressChanged += dp.MediaBufferingProgressChanged;
				newElement.CurrentStateChanged += dp.MediaCurrentStateChanged;
				newElement.MediaEnded += dp.MediaEnded;
				newElement.MediaFailed += dp.MediaFailed;
				newElement.MediaOpened += dp.MediaOpened;
                newElement.DownloadProgressChanged += dp.MediaDownloadProgressChanged;
			}
		}

        private void MediaDownloadProgressChanged(object sender, RoutedEventArgs e)
        {
			SetDownloadProgress();
        }

		private void SetDownloadProgress()
		{
			if (DownloadProgress != null && Media != null)
			{
				DownloadProgress.RenderTransform = new ScaleTransform()
				{
					CenterX = 0,
					ScaleX = Media.DownloadProgress
				};
				//DownloadProgress.Width =
				//    (DownloadProgress.Parent as FrameworkElement).ActualWidth * Media.DownloadProgress;
			}
		}

		private void MediaOpened(object sender, RoutedEventArgs e)
		{
			if (Status != null)
				Status.Text = "";
			
			SetTotalTime();
		}

		private void SetTotalTime()
		{
			if (TotalTime != null && Media != null)
                TotalTime.Text = FormatTimeSpan(Media.NaturalDuration.TimeSpan);
		}

        private string FormatTimeSpan(TimeSpan t)
        {
            if (Media.NaturalDuration.TimeSpan.Hours > 0)
               return String.Format("{0}:{1:00}:{2:00}",
                       t.Hours,
                        t.Minutes,
                       t.Seconds);
            else
                return String.Format("{0}:{1:00}",
                       t.Minutes,
                       t.Seconds);
        }

		private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			
		}

		private void MediaEnded(object sender, RoutedEventArgs e)
		{
			Media.Stop();
            SetCurrentState();
		}

		private void MediaCurrentStateChanged(object sender, RoutedEventArgs e)
		{
            SetCurrentState();
		}

        private void SetCurrentState()
        {
            if (Status != null && Media != null)
            {
                switch (Media.CurrentState)
                {
                    case MediaElementState.AcquiringLicense:
                        Status.Text = "Acquiring License"; break;
                    case MediaElementState.Buffering:
                        Status.Text = "Buffering..."; break;
                    case MediaElementState.Opening:
                        Status.Text = "Opening..."; break;
                    case MediaElementState.Paused:
                        if (PlayButton != null) PlayButton.IsEnabled = true;
                        if (PauseButton != null) PauseButton.IsEnabled = false;
                        if (StopButton != null) StopButton.IsEnabled = true;
                        Status.Text = "Paused"; break;
                    case MediaElementState.Playing:
                        if (PlayButton != null) PlayButton.IsEnabled = false;
                        if (PauseButton != null) PauseButton.IsEnabled = true;
                        if (StopButton != null) StopButton.IsEnabled = true;
                        Status.Text = "Playing"; break;
                    case MediaElementState.Stopped:
                        if (PlayButton != null) PlayButton.IsEnabled = true;
                        if (PauseButton != null) PauseButton.IsEnabled = false;
                        if (StopButton != null) StopButton.IsEnabled = false;
                        Status.Text = "Stopped"; break;
                    case MediaElementState.Closed:
                        Status.Text = "Closed"; break;
                    default:
                        Status.Text = ""; break;
                }
            }
        }

		private void MediaBufferingProgressChanged(object sender, RoutedEventArgs e)
		{
		}
	}
}