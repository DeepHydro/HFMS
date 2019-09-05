using HydroCloud.ServiceReference;
using HydroCloud.SpatialTemporal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Samples
{
    public partial class AnimationPlayer : UserControl
    {
        private ServiceClient _ServiceClient;
        private int _step = 0;
        public AnimationPlayer()
        {
            InitializeComponent();
            int[] id = new int[23];
            int[] cla = new int[25];
            for (int i = 0; i < 23; i++)
            {
                id[i] = i + 1;
            }
            for (int i = 0; i < 25; i++)
            {
                cla[i] = i + 1;
            }
            cmbColorID.ItemsSource = id;
            cmbColorCount.ItemsSource = cla;
            cmbColorCount.SelectionChanged -= this.cmbColorCount_SelectionChanged;
            cmbColorID.SelectionChanged -= this.cmbColorID_SelectionChanged;
            cmbColorID.SelectedIndex = 13;
            cmbColorCount.SelectedIndex = 5;
            cmbColorCount.SelectionChanged += this.cmbColorCount_SelectionChanged;
            cmbColorID.SelectionChanged += this.cmbColorID_SelectionChanged;
        }

        public ServiceClient ServiceClient
        {
            get
            {
                return _ServiceClient;
            }
            set
            {
                _ServiceClient = value;
                _ServiceClient.GetSliceCompleted += _ServiceClient_GetSliceCompleted;
            }
        }
        public Slider Slider
        {
            get
            {
                return slider;
            }
        }

        public int CurrentStep
        {
            get
            {
                return _step;
            }
            set
            {
                if (value < 0)
                    return;
                else if (value >= GridRender.Instance.NTime)
                    return;
                _step = value;
                slider.ValueChanged -= this.slider_ValueChanged;
                slider.Value = _step;
                slider.ValueChanged += this.slider_ValueChanged;
                cmbDates.SelectionChanged -= cmbDates_SelectionChanged;
                cmbDates.SelectedIndex = _step;
                cmbDates.SelectionChanged += cmbDates_SelectionChanged;
                SetControlState(true);
                _ServiceClient.GetSliceAsync(GridRender.Instance.VariableIndex, _step);
            }
        }

        public void SetDates(System.Collections.ObjectModel.ObservableCollection<System.DateTime> dates)
        {
            cmbDates.ItemsSource = dates;
            cmbDates.SelectionChanged -= this.cmbDates_SelectionChanged;
            cmbDates.SelectedIndex = 0;
            cmbDates.SelectionChanged += this.cmbDates_SelectionChanged;
        }

      private  void _ServiceClient_GetSliceCompleted(object sender, GetSliceCompletedEventArgs e)
        {
            GridRender.Instance.Render(e.Result.ToArray());
            SetControlState(false);
        }
      private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
          CurrentStep  = (int) e.NewValue;
         
      }
        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            CurrentStep--;
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            CurrentStep++;
        }

        private void cmbColorID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridRender.Instance.RampID = (int)cmbColorID.SelectedItem;
        }

        private void cmbColorCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridRender.Instance.ColourRampCount = (int)cmbColorCount.SelectedItem;
        }
        private void cmbDates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentStep = cmbDates.SelectedIndex;
        }
        private void slider_transp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(GridRender.Instance != null)
                GridRender.Instance.Opacity = e.NewValue;
        }
        private void slider_borderthick_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (GridRender.Instance != null)
                GridRender.Instance.BorderThickness = e.NewValue;
        }
        private void SetControlState(bool isbusy)
        {
            btnBackward.IsEnabled = !isbusy;
            btnForward.IsEnabled = !isbusy;
            cmbColorCount.IsEnabled = !isbusy;
            cmbColorID.IsEnabled = !isbusy;
            cmbDates.IsEnabled = !isbusy;
            slider.IsEnabled = !isbusy;
        }
    }
}
