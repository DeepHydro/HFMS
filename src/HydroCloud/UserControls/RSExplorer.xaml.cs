using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService.Symbols;
using ESRI.ArcGIS.Client.Geometry;
using HydroCloud.RemoteSesnsing;
using HydroCloud.ServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Samples
{
    public partial class RSExplorer : UserControl
    {
        public RSExplorer()
        {
            InitializeComponent();
            cmbRensor.DisplayMemberPath = "SensorName";
            this.gridImages.Columns.Add(
                       new DataGridTextColumn
                       {
                           Header = "Name",
                           Binding = new  System.Windows.Data.Binding("ImageName")
                       });
            this.gridImages.Columns.Add(
                        new DataGridTextColumn
                        {
                            Header = "Date",
                            Binding = new  System.Windows.Data.Binding("Date")
                        });
        }

        private ImageSeries _ImageSeries;

        public ServiceClient ServiceClient
        {
            get;
            set;
        }

        public Map Map
        {
            get;
            set;
        }

        public GraphicsLayer SelectedLayer
        {
            get;
            set;
        }

        public SensorImageRecord SelectedSensorImageRecord
        {
            get;
            private set;
        }

        public DataGrid ImageList
        {
            get
            {
                return gridImages;
            }
        }

        public bool KeepSeries
        {
            get
            {
                return keepSeries.IsChecked.Value;
            }
        }

        public void UpdateImageRecord(ImageSeries series)
        {
            _ImageSeries = series;
            cmbRensor.ItemsSource = null;
            cmbRensor.ItemsSource = _ImageSeries.GetSensors();
            cmbRensor.SelectedIndex = 0;
        }

        private void cmbRensor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_ImageSeries != null && cmbRensor.SelectedItem != null)
            {
                cmbVariable.ItemsSource = (cmbRensor.SelectedItem as Sensor).Variables;
                cmbVariable.SelectedIndex = 0;
            }
        }

        private void cmbVariable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ImageSeries != null && cmbRensor.SelectedItem != null && cmbVariable.SelectedItem != null)
            {
                var records = _ImageSeries.GetRecords((cmbRensor.SelectedItem as Sensor).SensorName, cmbVariable.SelectedItem.ToString());
                var dates = (from rec in records select rec.Date);
                startDate.SelectedDate = dates.Min();
                endDate.SelectedDate = dates.Max();
                gridImages.ItemsSource = records;
                gridImages.SelectedIndex = 0;
            }
        }

        private void clearSeries_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLayer != null)
                SelectedLayer.Graphics.Clear();
        }

        private void btnShowData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDownloadData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void gridImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var record = gridImages.SelectedItem as SensorImageRecord;
            SelectedSensorImageRecord = record;
            ImageList.IsEnabled = false;
            ServiceClient.DownloadImageAsync(record.ImageFile);
            if(record.LegendFile != "")
                ServiceClient.DownloadLegendAsync(record.LegendFile);
        }
    }
}
