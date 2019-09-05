using HydroCloud.ServiceReference;
using HydroCloud.SpatialTemporal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Visifire.Charts;

namespace ESRI.ArcGIS.Samples
{
    public partial class Chart : UserControl
    {
        private DoubleTimeSeries _DoubleTimeSeries;
        public Chart()
        {
            InitializeComponent();
            Axis axisx = new Axis();
            // Set axis properties
            // axis.IntervalType = IntervalTypes.;
            // Add axis to AxesX collection
            axisx.ValueFormatString = "yyyy-MM-dd";
            timeseriesChart.AxesX.Add(axisx);

            Axis axisy = new Axis();
            timeseriesChart.AxesY.Add(axisy);

            Title title = new Title();
            timeseriesChart.Titles.Add(title);
        }

         private void btnShowData_Click(object sender, RoutedEventArgs e)
         {
             if (_DoubleTimeSeries != null)
                 datagrid.ItemsSource = DataPair.ToList(_DoubleTimeSeries);
         }
         private void btnDownloadData_Click(object sender, RoutedEventArgs e)
         {
             if (_DoubleTimeSeries != null)
             {
                 SaveFileDialog dilg = new SaveFileDialog();
                 dilg.Filter = "csv file|*.csv";
                 dilg.DefaultFileName = timeseriesChart.Titles[0].Text + ".csv";
                 if (dilg.ShowDialog().Value)
                 {
                     Stream fs = dilg.OpenFile();
                     StreamWriter sw = new StreamWriter(fs);
                     sw.WriteLine("Dates," + "Variable");
                     for (int i = 0; i < _DoubleTimeSeries.DateTimes.Count; i++)
                     {
                         string line = string.Format("{0},{1}", _DoubleTimeSeries.DateTimes[i], _DoubleTimeSeries.Values[i]);
                         sw.WriteLine(line);
                     }
                     sw.Close();
                 }
             }
         }
         public void PlotTimeSeries(DoubleTimeSeries ts, string titletext)
         {
             if (!keepSeries.IsChecked.Value)
                 clearSeries_Click(null, null);

             if (keepSeries.IsChecked.Value)
                 timeseriesChart.Titles[0].Text = "";
             else
                 timeseriesChart.Titles[0].Text = titletext;

             _DoubleTimeSeries = ts;
             var dataSeries = new DataSeries();
             // Set DataSeries properties
             dataSeries.RenderAs = RenderAs.QuickLine;
             dataSeries.XValueType = ChartValueTypes.DateTime;
             dataSeries.LegendText = titletext;

             double max = ts.Values.Max();
             double min = ts.Values.Min();
             double range = max - min;
             var axisy = timeseriesChart.AxesY[0];
             if (range > 0)
             {
                 var maxy = max + range * 0.1;
                 var miny = min - range * 0.1;
                 miny = miny < 0 ? 0 : miny;
                 axisy.AxisMaximum = maxy;
                 axisy.AxisMinimum = miny;
             }

             for (int i = 0; i < ts.DateTimes.Count; i++)
             {
                 DataPoint dataPoint = new DataPoint();
                 dataPoint.XValue = ts.DateTimes[i];
                 dataPoint.YValue = ts.Values[i];
                 dataSeries.DataPoints.Add(dataPoint);
             }
             timeseriesChart.Series.Add(dataSeries);
         }

         public void PlotTimeSeries(DoubleTimeSeries ts, string titletext, Color color, RenderAs linetype)
         {
             if (!keepSeries.IsChecked.Value)
                 clearSeries_Click(null, null);

             if (keepSeries.IsChecked.Value)
                 timeseriesChart.Titles[0].Text = "";
             else
                 timeseriesChart.Titles[0].Text = titletext;

             _DoubleTimeSeries = ts;
             var dataSeries = new DataSeries();
             // Set DataSeries properties
             dataSeries.RenderAs = RenderAs.QuickLine;
             dataSeries.XValueType = ChartValueTypes.DateTime;
             dataSeries.LegendText = titletext;
             dataSeries.LineThickness = 2;

             double max = ts.Values.Max();
             double min = ts.Values.Min();
             double range = max - min;
             var axisy = timeseriesChart.AxesY[0];
             if (range > 0)
             {
                 var maxy = max + range * 0.1;
                 var miny = min - range * 0.1;
                 miny = miny < 0 ? 0 : miny;
                 axisy.AxisMaximum = maxy;
                 axisy.AxisMinimum = miny;
             }

             for (int i = 0; i < ts.DateTimes.Count; i++)
             {
                 DataPoint dataPoint = new DataPoint();
                 dataPoint.XValue = ts.DateTimes[i];
                 dataPoint.YValue = ts.Values[i];
                 dataSeries.DataPoints.Add(dataPoint);
             }
             dataSeries.Color = new SolidColorBrush(color);
             timeseriesChart.Series.Add(dataSeries);
         }

         private void clearSeries_Click(object sender, RoutedEventArgs e)
         {
             timeseriesChart.Series.Clear();
         }

         private void btnOpenCSVData_Click(object sender, RoutedEventArgs e)
         {
             OpenFileDialog dlg = new OpenFileDialog();
             dlg.Multiselect = false;
             dlg.Filter = "csv Files (*.csv)|*.csv|All Files (*.*)|*.*";
             if (!(dlg.ShowDialog() ?? false))
                 return;
             var fs = dlg.File.OpenRead();
             StreamReader sr = new StreamReader(fs);
             var line = sr.ReadLine();
             var splitor = new char[] { ',' };
             var strs = line.Split(splitor);
             var nts = strs.Count() - 1;
             var legends = new string[nts];
             DoubleTimeSeries[] ts = new DoubleTimeSeries[nts];
             double[] vec = new double[nts];
             for (int i = 0; i < nts; i++)
             {
                 ts[i] = new DoubleTimeSeries();
                 ts[i].DateTimes = new System.Collections.ObjectModel.ObservableCollection<DateTime>();
                 ts[i].Values = new System.Collections.ObjectModel.ObservableCollection<double>();
             }
             while (!sr.EndOfStream)
             {
                 line = sr.ReadLine();
                 if (line != "" && line != null)
                 {
                     strs = line.Split(splitor);
                     DateTime date = DateTime.Parse(strs[0]);
                     for (int i = 0; i < nts; i++)
                     {
                         ts[i].DateTimes.Add(date);
                         ts[i].Values.Add(double.Parse(strs[i + 1]));
                     }
                 }
             }

             sr.Close();
             for (int i = 0; i < nts; i++)
             {
                 PlotTimeSeries(ts[i], legends[i]);
             }
         }

         private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {

         }


    }
}
