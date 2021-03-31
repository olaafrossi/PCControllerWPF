using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCController.DataAccess.Models
{
    public class UdpSenderModel
    {
        public int ID { get; set; }
        public string IncomingMessage { get; set; }
        public string OutgoingMessage { get; set; }
        public string RemoteIP { get; set; }
        public string LocalIP { get; set; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public string Timestamp { get; set; }
    }
}
