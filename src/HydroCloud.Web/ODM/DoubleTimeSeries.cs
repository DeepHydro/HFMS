using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace HdyroCloud.Web
{
    [ServiceContract]
    public class DoubleTimeSeries
    {
        public DoubleTimeSeries()
        {

        }

        [DataMember]
        public DateTime[] DateTimes { get; set; }
        [DataMember]
        public double[] Values { get; set; }
    }
}