using HydroCloud.ServiceReference;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HydroCloud.SpatialTemporal
{
    public class DataPair
    {
        public DataPair(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }
        public DataPair()
        {

        }
        public DateTime Date { get; set; }
        public double Value { get; set; }

        public static List<DataPair> ToList(DoubleTimeSeries ts)
        {
            List<DataPair> list = new List<DataPair>();
            for (int i = 0; i < ts.DateTimes.Count; i++)
            {
                list.Add(new DataPair(ts.DateTimes[i], ts.Values[i]));
            }
            return list;
        }
    }
}
