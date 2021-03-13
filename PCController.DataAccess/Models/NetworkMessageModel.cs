namespace PCController.DataAccess.Models
{
    public class NetworkMessageModel
    {
        public int ID { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }

        public string RemoteIP { get; set; }

        public string RemotePort { get; set; }

        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }
    }
}
