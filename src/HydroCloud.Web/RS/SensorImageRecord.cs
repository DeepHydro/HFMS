using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace HydroCloud.Web.RS
{
   [ServiceContract]
    public class SensorImageRecord
    {
       public SensorImageRecord()
       {

       }
       [DataMember]
       public int SerialID { get; set; }
       [DataMember]
       public string ImageName { get; set; }
       [DataMember]
       public string SensorName { get; set; }
       [DataMember]
       public string VariableName { get; set; }
       [DataMember]
       public string ImageFile { get; set;}
       [DataMember]
       public string LegendFile { get; set; }
       [DataMember]
       public DateTime Date { get; set; }
       [DataMember]
       public double[] BBox { get; set; }
    }
}