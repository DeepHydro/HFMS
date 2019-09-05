using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace HdyroCloud.Web
{
    [ServiceContract]
    public class Variable
    {
        public Variable()
        {

        }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int ID { get; set; }

    }
}