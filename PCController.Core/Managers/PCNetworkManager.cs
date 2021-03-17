using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.Managers
{
    public class PCNetworkManager
    {
        public event EventHandler<int> UDPPortSet;

        public PCNetworkManager(NetworkMessagesEventArgs incomingMsg)
        {
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            NetworkMessageModel netMsg = new NetworkMessageModel();

            if (incomingMsg.IncomingMessage is null)
            {
                netMsg.IncomingMessage = string.Empty;
            }
            else
            {
                netMsg.IncomingMessage = incomingMsg.IncomingMessage;
            }

            if (incomingMsg.OutgoingMessage is null)
            {
                netMsg.IncomingMessage = string.Empty;
            }
            else
            {
                netMsg.OutgoingMessage = incomingMsg.OutgoingMessage;
            }

            if (incomingMsg.RemoteIP is null)
            {
                incomingMsg.RemoteIP = string.Empty;
            }
            else
            {
                netMsg.RemoteIP = incomingMsg.RemoteIP;
            }

            if (incomingMsg.IncomingMessage is null)
            {
                incomingMsg.Timestamp = string.Empty;
            }
            else
            {
                netMsg.Timestamp = incomingMsg.Timestamp;
            }

            if (incomingMsg.RemotePort is null)
            {
                incomingMsg.RemotePort = string.Empty;
            }
            else
            {
                netMsg.RemotePort = incomingMsg.RemotePort;
            }

            netMsg.UDPPort = incomingMsg.UDPPort;

            this.UDPPortSet?.Invoke(this, incomingMsg.UDPPort);

            sql.InsertNetMessage(netMsg);
        }
    }
}
