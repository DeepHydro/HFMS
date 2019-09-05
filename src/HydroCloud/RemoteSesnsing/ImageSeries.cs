using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using HydroCloud.ServiceReference;

namespace HydroCloud.RemoteSesnsing
{
    public class ImageSeries
    {
        public ImageSeries()
        {

        }

        private ObservableCollection<HydroCloud.ServiceReference.SensorImageRecord> _source;
        public ObservableCollection<HydroCloud.ServiceReference.SensorImageRecord> Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                Init();
            }
        }

        private void Init()
        {

        }

        public Sensor[] GetSensors()
        {
            var buf = (from record in Source select record.SensorName).Distinct();
            Sensor[] sensors = new Sensor[buf.Count()];
            int i = 0;
            foreach (var sen in buf)
            {
                sensors[i] = new Sensor();
                var temp = (from record in Source where record.SensorName== sen select record.VariableName).Distinct();
                sensors[i].SensorName = sen;
                sensors[i].Variables = temp.ToArray();
                i++;
            }
            return sensors;
        }

        public SensorImageRecord[] GetRecords(string sensor, string variable)
        {
            var buf = from record in Source where record.SensorName == sensor && record.VariableName == variable select record;
            var records = buf.OrderBy(r => r.SerialID);
            return records.ToArray();
        }

    }
}
