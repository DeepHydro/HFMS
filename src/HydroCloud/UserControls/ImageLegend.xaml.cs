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
    public partial class ImageLegend : UserControl
    {
        public ImageLegend()
        {
            InitializeComponent();
        }

        public ImageSource LegendImageSource
        {
            set
            {
                legendImage.Source = value;
            }
            get
            {
                return legendImage.Source;
            }
        }
    }
}
