// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 18
// by Olaaf Rossi

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using Serilog;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.Managers
{
    public class UdpShowControlManager
    {
        private readonly IAsyncUdpLink _asyncUdpLink;


        private IDisposable _disposableImplementation;

        public UdpShowControlManager(string IPAddress, int remotePort, int localPort)
        {
            IAsyncUdpLink link = new AsyncUdpLink(IPAddress, remotePort, localPort);
            _asyncUdpLink = link;
            _asyncUdpLink.DataReceived += LinkOnDataReceived;
        }


        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }

        public string RemoteIP { get; set; }

        public string LocalIP { get; set; } = GetLocalIPAddress();

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }

        public string UdpFrameCombined { get; set; }

        public event EventHandler UDPDataReceived;

        public void DisposeUDPLink()
        {
            _asyncUdpLink.Dispose();
            // needs works
        }

        public void AddUdpFrame(string frame)
        {
            try
            {
                string message = frame;

                if (message.Contains("!0D!0A"))
                {
                    message = message.Replace("!0D!0A", "\r\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                }
                else if (message.Contains("!0D"))
                {
                    message = message.Replace("!0D", "\r");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                }
                else if (message.Contains("!0A"))
                {
                    message = message.Replace("!0A", "\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                }
                else
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                }
            }
            catch (Exception e)
            {
                Log.Error("Parsing the FrameToSend has failed {e}", e);
            }

            WriteUDPDataToDataBase(frame, true);
        }

        public void WriteUDPDataToDataBase(string frameToSend, bool sentTypeMessage)
        {
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            UdpSenderModel udpFrame = new UdpSenderModel();

            if (sentTypeMessage is true)
            {
                udpFrame.OutgoingMessage = frameToSend;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.LocalIP = LocalIP;
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                string udpFrameCombine =
                    $"SENT: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Sent Frame: {frameToSend} Remote IP: {_asyncUdpLink.Address} This IP: {LocalIP} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";
                //UDPRealTimeCollection.Insert(0, udpFrameCombine);
                UdpFrameCombined = udpFrameCombine;
                _asyncUdpLink.DataReceived += UDPDataReceived;
            }
            else
            {
                udpFrame.OutgoingMessage = string.Empty;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.LocalIP = LocalIP;
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                string udpFrameCombine =
                    $"RECEIVED: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Received Frame: {IncomingMessage} Remote IP: {_asyncUdpLink.Address} This IP: {LocalIP} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";

                UdpFrameCombined = udpFrameCombine;
                _asyncUdpLink.DataReceived += UDPDataReceived;
            }
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error("No network adapters with an IPv4 address in the system!{e}", e);
            }

            Log.Logger.Error("No network adapters with an IPv4 address in the system!");
            return "No network adapters with an IPv4 address in the system!";
        }

        public void LinkOnDataReceived(object sender, EventArgs e)
        {
            byte[] dataBytes = _asyncUdpLink.GetMessage();
            string messageFromController = Encoding.ASCII.GetString(dataBytes); // new byte array and feed it the input string
            Log.Information("Message from Remote {messageFromController}", messageFromController);
            IncomingMessage = messageFromController;
            WriteUDPDataToDataBase(string.Empty, false);
            IncomingMessage = string.Empty; // clear the prop
        }

        public void Dispose()
        {
            _asyncUdpLink.Enabled = false;
            _asyncUdpLink.Dispose();
            _disposableImplementation?.Dispose();
        }
    }

    public class AsyncUdpLinkEvents : EventArgs
    {
        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }

        public string RemoteIP { get; set; }

        public string LocalIP { get; set; }

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }
    }
}