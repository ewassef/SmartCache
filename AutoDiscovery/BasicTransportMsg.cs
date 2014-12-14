using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace AutoDiscovery
{
    [DataContract]
    public class BasicTransportMsg
    {
        [DataMember]
        public string ServiceType { get; set; }
        [DataMember]
        public Direction Request { get; set; }
        [DataMember]
        public IPAddress Ip { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string AdditionalInfo { get; set; }
    }

    [DataContract]
    public enum Direction
    {
        [EnumMember]
        Inbound = 0,
        [EnumMember]
        Outbound =1
    }
}
