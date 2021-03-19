using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCController.Core.ViewModels;
using PCController.Core.ViewModels.Navigation;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.Managers
{
    public class UdpShowControlManager
    {
        public UdpShowControlManager(AsyncUdpLinkEvents incomingMsg)
        {
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            UdpSenderModel udpFrame = new UdpSenderModel();

            if (incomingMsg.IncomingMessage is null)
            {
                udpFrame.IncomingMessage = string.Empty;
            }
            else
            {
                udpFrame.IncomingMessage = incomingMsg.IncomingMessage;
            }

            if (udpFrame.OutgoingMessage is null)
            {
                udpFrame.OutgoingMessage = string.Empty;
            }
            else
            {
                udpFrame.OutgoingMessage = incomingMsg.OutgoingMessage;
            }

            if (udpFrame.RemoteIP is null)
            {
                udpFrame.RemoteIP = string.Empty;
            }
            else
            {
                udpFrame.RemoteIP = incomingMsg.RemoteIP;
            }

            if (udpFrame.MyIP is null)
            {
                udpFrame.MyIP = string.Empty;
            }
            else
            {
                udpFrame.MyIP = incomingMsg.MyIP;
            }

            if (udpFrame.Timestamp is null)
            {
                udpFrame.Timestamp = string.Empty;
            }
            else
            {
                udpFrame.Timestamp = udpFrame.Timestamp;
            }


            sql.InsertUdpSentData(udpFrame);
        }
    }
}
