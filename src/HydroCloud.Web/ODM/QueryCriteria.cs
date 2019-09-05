using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace HdyroCloud.Web
{
    [ServiceContract]
    public class QueryCriteria
    {
        public QueryCriteria()
        {
            MaximumRecord = 0;
        }
        [DataMember]
        public DateTime Start { get; set; }
        [DataMember]
        public DateTime End { get; set; }
        [DataMember]
        public int SiteID { get; set; }
        [DataMember]
        public int VariableID { get; set; }
        [DataMember]
        public string VariableName { get; set; }
        [DataMember]
        public BBox BBox { get; set; }
        [DataMember]
        public int MaximumRecord { get; set; }
    }
    [ServiceContract]
    public class BBox
    {
        public BBox()
        {
            West = -180;
            East = 180;
            North = 90;
            South = 90;
        }
        [DataMember]
        public double West { get; set; }
        [DataMember]
        public double East { get; set; }
        [DataMember]
        public double South { get; set; }
        [DataMember]
        public double North { get; set; }
    }
}