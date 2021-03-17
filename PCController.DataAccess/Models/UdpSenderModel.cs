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

        public string IPAddress { get; set; }

        public int PortNum { get; set; }

        public string RemoteIP { get; set; }

        public string MessageSent { get; set; }

    }
}
