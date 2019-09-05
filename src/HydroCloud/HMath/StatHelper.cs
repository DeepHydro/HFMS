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

namespace HdyroCloud.HMath
{
    public struct StatInfo
    {
        public double Max { get; set; }
        public double Min { get; set; }
        public double Mean { get; set; }
        public double Std { get; set; }
    }
    public  class StatHelper
    {
        public static StatInfo Get(double [] vec)
        {
            StatInfo info = new StatInfo();
            var max = vec[0];
            var min = vec[0];
            double sum = vec[0];
            for (int i = 1; i < vec.Length; i++)
            {
                if (vec[i] > max)
                    max = vec[i];
                if (vec[i] < min)
                    min = vec[i];
                sum += vec[i];
            }
            info.Mean = sum / vec.Length;
            info.Max = max;
            info.Min = min;
            info.Std = StandardDeviation(vec);
            return info;
        }
        public static double StandardDeviation(IEnumerable<double> valueList)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in valueList)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return System.Math.Sqrt(S / (k - 2));
        }
    }
}
