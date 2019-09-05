using System;
using System.Data;
using System.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace HydroCloud.Web.RS
{
    public class ImageBaseSource 
    {
        public ImageBaseSource()
        {

        }

        public SensorImageRecord[] GetSensorCatalog(string catalogfile)
        {
            StreamReader sr = new StreamReader(catalogfile);
            List<SensorImageRecord> list = new List<SensorImageRecord>();
            var line = "";
            char[] splitor = new char[] { ',' };
            while(!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line != null && line != "")
                {
                    line = line.Trim();
                    var strs = line.Split(splitor);
                    SensorImageRecord record = new SensorImageRecord()
                    {
                        SerialID = int.Parse(strs[0]),
                        SensorName = strs[1],
                        VariableName = strs[2],
                        ImageName = strs[3],
                        Date = DateTime.Parse(strs[4]),
                        ImageFile = strs[5],
                        LegendFile = strs[6],
                        BBox = new double[4]
                    };
                    record.BBox[0] = double.Parse(strs[7]);
                    record.BBox[1] = double.Parse(strs[8]);
                    record.BBox[2] = double.Parse(strs[9]);
                    record.BBox[3] = double.Parse(strs[10]);
                    list.Add(record);
                }
            }
            return list.ToArray();
        }
    }
}
