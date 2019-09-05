using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HydroCloud.Spatial
{
    public class SpatialReferenceSystem
    {
        public static double[] ToGeographic(double mercatorX_lon, double mercatorY_lat)
        {
            double[] lonlat = new double[2];
            if (Math.Abs(mercatorX_lon) < 180 && Math.Abs(mercatorY_lat) < 90)
                return null;

            if ((Math.Abs(mercatorX_lon) > 20037508.3427892) || (Math.Abs(mercatorY_lat) > 20037508.3427892))
                return null;

            double x = mercatorX_lon;
            double y = mercatorY_lat;
            double num3 = x / 6378137.0;
            double num4 = num3 * 57.295779513082323;
            double num5 = Math.Floor((double)((num4 + 180.0) / 360.0));
            double num6 = num4 - (num5 * 360.0);
            double num7 = 1.5707963267948966 - (2.0 * Math.Atan(Math.Exp((-1.0 * y) / 6378137.0)));
            lonlat[0] = num6;
            lonlat[1] = num7 * 57.295779513082323;
            return lonlat;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mercatorX_lon"></param>
        /// <param name="mercatorY_lat"></param>
        /// <returns></returns>
        public static double[] ToWebMercator(double mercatorX_lon, double mercatorY_lat)
        {
            double[] lonlat = new double[2];
            if ((Math.Abs(mercatorX_lon) > 180 || Math.Abs(mercatorY_lat) > 90))
                return null;

            double num = mercatorX_lon * 0.017453292519943295;
            double x = 6378137.0 * num;
            double a = mercatorY_lat * 0.017453292519943295;

            lonlat[0] = x;
            lonlat[1] = 3189068.5 * Math.Log((1.0 + Math.Sin(a)) / (1.0 - Math.Sin(a)));
            return lonlat;
        }
    }
}
