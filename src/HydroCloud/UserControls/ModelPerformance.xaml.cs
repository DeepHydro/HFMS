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
using System.IO;
using HydroCloud.ServiceReference;
using HydroCloud.HMath;
using Visifire.Charts;

namespace ESRI.ArcGIS.Samples
{
    public partial class ModelPerformance : UserControl
    {
        private Color[] colors = new Color[3];
        private RenderAs [] linetypes = new RenderAs[3];
        public ModelPerformance()
        {
            InitializeComponent();
            colors[0] = Colors.Blue;
            colors[1] = Colors.Red;
            colors[2] = Colors.Cyan;

            linetypes[0] = RenderAs.QuickLine;
            linetypes[1] = RenderAs.QuickLine;
            linetypes[2] = RenderAs.Point;
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
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
            GoodnessOfFit[] fits = new GoodnessOfFit[nts-1];
            for (int i = 0; i < nts; i++)
            {
                ts[i] = new DoubleTimeSeries();
                ts[i].DateTimes = new System.Collections.ObjectModel.ObservableCollection<DateTime>();
                ts[i].Values = new System.Collections.ObjectModel.ObservableCollection<double>();
                legends[i] = strs[i + 1];
              
            }
            for (int i = 1; i < nts; i++)
            {
                fits[i - 1] = new GoodnessOfFit() { Name = strs[i + 1] };
            }
            double[] vec = new double[nts];
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
                chart.PlotTimeSeries(ts[i],legends[i],colors[i],linetypes[i]);
            }
            datagrid.ItemsSource = fits;
        }
    }
}
