using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace HdyroCloud.Web.RS
{
    [ServiceContract]
    public class RegularGrid
    {
        public RegularGrid()
        {

        }
        [DataMember]
        public int NCell
        {
            get;
            set;
        }
        [DataMember]
        public double CellSize
        {
            get;
            set;
        }
        [DataMember]
        public double[] CentroidX
        {
            get;
            set;
        }
        [DataMember]
        public double[] CentroidY
        {
            get;
            set;
        }
    }
}