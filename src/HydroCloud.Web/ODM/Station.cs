using System;
using System.Runtime.Serialization;
using System.ServiceModel;
namespace HdyroCloud.Web
{
    [ServiceContract]
    public class Station
    {
        public Station()
        {
            SiteType = "Hydrology";
        }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public double Latitude { get; set; }
        [DataMember]
        public double Longitude { get; set; }
        [DataMember]
        public string SiteType { get; set; }
    }
}