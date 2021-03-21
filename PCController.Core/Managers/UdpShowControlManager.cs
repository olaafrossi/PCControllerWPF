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
    public class UdpShowControlManager : IDisposable
    {
        private readonly IAsyncUdpLink _asyncUdpLink;

        public UdpShowControlManager(string IPAddress, int remotePort, int localPort)
        {
            IAsyncUdpLink link = new AsyncUdpLink(IPAddress, remotePort, localPort);
            _asyncUdpLink = link;
            _asyncUdpLink.DataReceived += LinkOnDataReceived;
        }

        public string IncomingMessage { get; set; }

        public string UdpFrameCombined { get; set; }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void AddUdpFrame(string frame)
        {
            try
            {
                string message = frame;

                Log.Information("Going to send a message over the UDP driver {message}", message);

                if (message.Contains("!0D!0A"))
                {
                    message = message.Replace("!0D!0A", "\r\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    Log.Information("Message contained <crlf>, replaced the hex with ASCII to sent properly {message}", message);
                }
                else if (message.Contains("!0D"))
                {
                    message = message.Replace("!0D", "\r");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    Log.Information("Message contained <cr>, replaced the hex with ASCII to sent properly {message}", message);
                }
                else if (message.Contains("!0A"))
                {
                    message = message.Replace("!0A", "\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    Log.Information("Message contained <lf>, replaced the hex with ASCII to sent properly {message}", message);
                }
                else
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    Log.Information("Message did not contain any ending characters {message}", message);
                }
            }
            catch (Exception e)
            {
                Log.Error("Parsing the FrameToSend has failed {e}", e);
            }

            WriteUDPDataToDataBase(frame, true);
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

        public event EventHandler UDPDataReceived;

        public void WriteUDPDataToDataBase(string frameToSend, bool sentTypeMessage)
        {
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            UdpSenderModel udpFrame = new();

            if (sentTypeMessage is true)
            {
                udpFrame.OutgoingMessage = frameToSend;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.LocalIP = GetLocalIPAddress();
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                string udpFrameCombine =
                    $"SENT: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Sent Frame: {frameToSend} Remote IP: {_asyncUdpLink.Address} This IP: {GetLocalIPAddress()} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";

                UdpFrameCombined = udpFrameCombine;
            }
            else
            {
                udpFrame.OutgoingMessage = string.Empty;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.LocalIP = GetLocalIPAddress();
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                string udpFrameCombine =
                    $"RECEIVED: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Received Frame: {IncomingMessage} Remote IP: {_asyncUdpLink.Address} This IP: {GetLocalIPAddress()} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";

                UdpFrameCombined = udpFrameCombine;
            }

            Log.Information("created an event handler to propogate messages to the ViewModel");
            _asyncUdpLink.DataReceived += UDPDataReceived;
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                Log.Logger.Information("Asking the base DNS class for IPv4 addresses");
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
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

        private void ReleaseUnmanagedResources()
        {
            Log.Information("Locking the UDP Driver");
            lock (_asyncUdpLink)
            {
                _asyncUdpLink.Enabled = false;
                _asyncUdpLink.Dispose();
                Log.Information("Set UDP Driver Enable to false, and called the dispose method on the library");
            }
        }

        ~UdpShowControlManager()
        {
            ReleaseUnmanagedResources();
        }
    }
}