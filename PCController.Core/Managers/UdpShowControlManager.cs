// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 18
// by Olaaf Rossi

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using Serilog;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.Managers
{
    public class UdpShowControlManager
    {
        private readonly IAsyncUdpLink _asyncUdpLink;
        private readonly IMvxLog _log;

        public UdpShowControlManager(string ipAddress, int remotePort, int localPort, IMvxLogProvider logProvider)
        {
            _log = logProvider.GetLogFor<UdpShowControlManager>();
            IAsyncUdpLink link = new AsyncUdpLink(ipAddress, remotePort, localPort);
            _asyncUdpLink = link;
            _asyncUdpLink.DataReceived += LinkOnDataReceived;
            _asyncUdpLink.DataReceived += UDPDataReceived;
        }

        public string IncomingMessage { get; set; }

        public string UdpFrameCombined { get; set; }

        public void AddUdpFrame(string frame)
        {
            try
            {
                string message = frame;

                _log.Info("Going to send a message over the UDP driver {message}", message);

                if (message.Contains("!0D!0A"))
                {
                    message = message.Replace("!0D!0A", "\r\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    _log.Info("Message contained <crlf>, replaced the hex with ASCII to sent properly {message}", message);
                }
                else if (message.Contains("!0D"))
                {
                    message = message.Replace("!0D", "\r");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    _log.Info("Message contained <cr>, replaced the hex with ASCII to sent properly {message}", message);
                }
                else if (message.Contains("!0A"))
                {
                    message = message.Replace("!0A", "\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    _log.Info("Message contained <lf>, replaced the hex with ASCII to sent properly {message}", message);
                }
                else
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                    _log.Info("Message did not contain any ending characters {message}", message);
                }
            }
            catch (Exception e)
            {
                _log.ErrorException("Parsing the FrameToSend has failed {e}", e);
            }

            WriteUDPDataToDataBase(frame, true);
        }

        public void LinkOnDataReceived(object sender, EventArgs e)
        {
            byte[] dataBytes = _asyncUdpLink.GetMessage();
            string messageFromController = Encoding.ASCII.GetString(dataBytes); // new byte array and feed it the input string
            _log.Info("Message from Remote {messageFromController}", messageFromController);
            IncomingMessage = messageFromController;
            WriteUDPDataToDataBase(string.Empty, false);
            IncomingMessage = string.Empty; // clear the prop
            UDPDataReceived?.Invoke(this, e);
        }

        public event EventHandler UDPDataReceived;

        public void WriteUDPDataToDataBase(string frameToSend, bool sentTypeMessage)
        {
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.PCControllerDB));
            UdpSenderModel udpFrame = new();
            string incomingMessage = IncomingMessage;

            if (incomingMessage is null)
            {
                _log.Error("Incoming Message is null");
            }
            else
            {
                if (incomingMessage.Contains("\r\n"))
                {
                    incomingMessage = incomingMessage.Replace("\r\n", "!0A!0D");
                }
                else if (incomingMessage.Contains("\r"))
                {
                    incomingMessage = incomingMessage.Replace("\r", "!0D");
                }
                else if (incomingMessage.Contains("\n"))
                {
                    incomingMessage = incomingMessage.Replace("\n", "!0A");
                }

                if (sentTypeMessage is true)
                {
                    udpFrame.OutgoingMessage = frameToSend;
                    udpFrame.IncomingMessage = incomingMessage;
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
                    udpFrame.IncomingMessage = incomingMessage;
                    udpFrame.RemoteIP = _asyncUdpLink.Address;
                    udpFrame.LocalIP = GetLocalIPAddress();
                    udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                    udpFrame.RemotePort = _asyncUdpLink.Port;
                    udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    sql.InsertUdpSentData(udpFrame);

                    string udpFrameCombine =
                        $"RECEIVED: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Received Frame: {incomingMessage} Remote IP: {_asyncUdpLink.Address} This IP: {GetLocalIPAddress()} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";

                    UdpFrameCombined = udpFrameCombine;
                }
            }
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
    }
}