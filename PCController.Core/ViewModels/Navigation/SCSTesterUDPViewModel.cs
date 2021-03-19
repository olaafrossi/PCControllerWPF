// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 17
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Data;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.Core.Properties;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.ViewModels
{
    public class SCSTesterUDPViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;
        private readonly IAsyncUdpLink _asyncUdpLink;

        private WindowChildParam _param;

        private bool carriageReturnTrue;

        private string frameToSend;

        private string iPAddress;

        private bool lineFeedTrue;

        private string messageSent;

        private ObservableCollection<UdpSenderModel> udpSender = new();

        private ObservableCollection<string> _uDPRealTimeCollection = new();


        public SCSTesterUDPViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            // Setup UI Commands
            RefreshUdpMsgCommand = new MvxCommand(GetUdpLogs);
            SendUdpCommand = new MvxCommand(AddUdpFrame);
            IPBoxTextChangeCommand = new MvxCommand(GetIpSuggestionsFromDb);
            OpenUdpCommand = new MvxCommand(CreateUDPAsyncManager);
            CloseUdpCommand = new MvxCommand(DisposeUDPAsyncManager);

            // Fetch Initial Data
            stopwatch = new Stopwatch();
            GetUdpLogs();
            GetIpSuggestionsFromDb();

            // Setup the UDP Singleton
            IAsyncUdpLink asyncUdpLink = Mvx.IoCProvider.Resolve<IAsyncUdpLink>();
            _asyncUdpLink = asyncUdpLink;
            _asyncUdpLink.DataReceived += LinkOnDataReceived;
            
            UDPRealTimeCollection = _uDPRealTimeCollection;
            BindingOperations.EnableCollectionSynchronization(UDPRealTimeCollection, _asyncUdpLink);
        }

        private void DisposeUDPAsyncManager()
        {
            throw new NotImplementedException();
        }

        private void CreateUDPAsyncManager()
        {
            throw new NotImplementedException();
        }

        public int ID { get; set; }

        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }

        public string RemoteIP { get; set; }

        public string MyIP { get; set; } = GetLocalIPAddress();

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }

        public IList<UdpSenderModel> UdpGridRows { get; set; }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public IMvxCommand SendUdpCommand { get; set; }

        public string NumberOfUdpMsgToFetch { get; set; }

        public IMvxCommand RefreshUdpMsgCommand { get; set; }

        //public ObservableCollection<UdpSenderModel> UdpSender
        //{
        //    get { return udpSender; }
        //    set { SetProperty(ref udpSender, value); }
        //}

        public bool CanSendMsg => IPAddress?.Length > 0 && MessageSent?.Length > 0;



        public string MessageSent
        {
            get { return messageSent; }
            set
            {
                SetProperty(ref messageSent, value);
                RaisePropertyChanged(() => CanSendMsg);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        private ObservableCollection<string> uDPRealTimeCollection;

        public ObservableCollection<string> UDPRealTimeCollection
        {
            get { return uDPRealTimeCollection; }
            set
            {
                SetProperty(ref uDPRealTimeCollection, value);
            }
        }
        //public ObservableCollection<string> UDPRealTimeCollection { get; set; }

        public bool CarriageReturnTrue
        {
            get { return carriageReturnTrue; }
            set
            {
                SetProperty(ref carriageReturnTrue, value);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        public bool LineFeedTrue
        {
            get { return lineFeedTrue; }
            set
            {
                SetProperty(ref lineFeedTrue, value);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        // UDP settings
        public string IPAddress { get; set; } = Settings.Default.AsyncUdpIPAddress;
        public int PortNum { get; set; } = Settings.Default.AsyncUdpRemotePort;
        public int LocalPortNum { get; set; } = Settings.Default.AsyncUdpLocalPort;

        public string FrameToSend
        {
            get
            {
                if (CarriageReturnTrue is false && LineFeedTrue is false)
                {
                    FrameToSend = $"{MessageSent}";
                    RaisePropertyChanged(() => MessageSent);
                    return frameToSend;
                }

                if (CarriageReturnTrue is true && LineFeedTrue is false)
                {
                    FrameToSend = $"{MessageSent}!0D";
                    RaisePropertyChanged(() => MessageSent);
                    return frameToSend;
                }

                if (LineFeedTrue is true && CarriageReturnTrue is false)
                {
                    FrameToSend = $"{MessageSent}!0A";
                    RaisePropertyChanged(() => MessageSent);
                    return frameToSend;
                }

                if (CarriageReturnTrue is true && LineFeedTrue is true)
                {
                    FrameToSend = $"{MessageSent}!0D!0A";
                    RaisePropertyChanged(() => MessageSent);
                    return frameToSend;
                }

                RaisePropertyChanged(() => MessageSent);
                return frameToSend;
                FrameToSend = String.Empty;
            }
            set { SetProperty(ref frameToSend, value); }
        }

        public bool UdpDriverClosed { get; set; }

        public IList<string> IpList { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IMvxCommand IPBoxTextChangeCommand { get; set; }

        public IMvxCommand OpenUdpCommand { get; set; }
        public IMvxCommand CloseUdpCommand { get; set; }

        public void AddUdpFrame()
        {
            try
            {
                string message = FrameToSend;

                if (message.Contains("!0D!0A"))
                {
                    message.Replace("!0D!0A", "\r\n");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                }
                else if (message.Contains("!0D"))
                {
                    message.Replace("!0D", "\r");
                    byte[] inputBytes = Encoding.ASCII.GetBytes(message); // new byte array and feed it the input string
                    _asyncUdpLink.SendMessage(inputBytes);
                }
                else if (message.Contains("!0A"))
                {
                    message.Replace("!0D", "\n");
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
                Log.Error("this is bad {e}", e);
            }

            WriteUDPDataToDataBase(true);

        }

        public void WriteUDPDataToDataBase(bool sendMessage)
        {
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            UdpSenderModel udpFrame = new UdpSenderModel();
            string udpFrameCombine = string.Empty;

            if (sendMessage is true)
            {
                udpFrame.OutgoingMessage = FrameToSend;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.MyIP = MyIP;
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                udpFrameCombine = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} Sent Frame: {FrameToSend} Remote IP: {_asyncUdpLink.Address} This IP: {MyIP} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";
                UDPRealTimeCollection.Insert(0, udpFrameCombine);
            }
            else
            {
                udpFrame.OutgoingMessage = string.Empty;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.MyIP = MyIP;
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                // {IncomingMessage}
                udpFrameCombine = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} Received Frame: {IncomingMessage} Remote IP: {_asyncUdpLink.Address} This IP: {MyIP} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";
                UDPRealTimeCollection.Insert(0, udpFrameCombine);
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


        public void LinkOnDataReceived(object sender, EventArgs e)
        {
            Console.WriteLine($"this might be a message from the controlller {e}");

            byte[] dataBytes;
            dataBytes = _asyncUdpLink.GetMessage();
            string messageFromController = Encoding.ASCII.GetString(dataBytes); // new byte array and feed it the input string
            Console.WriteLine(messageFromController);
            IncomingMessage = messageFromController;
            WriteUDPDataToDataBase(false);
            IncomingMessage = string.Empty;
        }

        private void GetUdpLogs()
        {
            stopwatch.Start();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            int numOfMsgs = 20;

            try
            {
                RaisePropertyChanged(() => NumberOfUdpMsgToFetch);
                if (NumberOfUdpMsgToFetch is null)
                {
                    numOfMsgs = 20;
                }
                else if (NumberOfUdpMsgToFetch.Contains("All"))
                {
                    // All
                    numOfMsgs = 100000000;
                }
                else if (NumberOfUdpMsgToFetch.Length == 40)
                {
                    // 20, 50
                    string logComboBoxSelected = NumberOfUdpMsgToFetch.Substring(38, 2);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfUdpMsgToFetch.Length == 41)
                {
                    // hundred
                    string logComboBoxSelected = NumberOfUdpMsgToFetch.Substring(38, 3);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfUdpMsgToFetch.Length == 42)
                {
                    // thousand
                    string logComboBoxSelected = NumberOfUdpMsgToFetch.Substring(38, 4);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("Didn't parse the number in the Net ComboBox {numOfMsgs}", numOfMsgs, e);
            }

            Serilog.Log.Logger.Information("Getting Data Logs{numOfMsgs}", numOfMsgs);
            var rows = sql.GetSomeUdpData(numOfMsgs);
            UdpGridRows = rows;

            stopwatch.Stop();
            string timeToFetchFromDB = $" DB query time: {stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDB;
            RaisePropertyChanged(() => UdpGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        public override void Prepare(WindowChildParam param) => _param = param;


        // Auto SuggestBox

        private void GetIpSuggestionsFromDb()
        {
            stopwatch.Start();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            int numOfSuggestions = 20;

            Serilog.Log.Logger.Information("Getting IP Address Suggestions {numOfSuggestions}", numOfSuggestions);

            var rows = sql.GetUdpUsedIPAddresses(numOfSuggestions);
            IpList = rows;

            stopwatch.Stop();
            string timeToFetchFromDB = $" DB query time: {stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDB;
            RaisePropertyChanged(() => IpList);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        private void IpSuggestionBoxChanged()
        {
        }
    }

    public class AsyncUdpLinkEvents : EventArgs
    {
        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }

        public string RemoteIP { get; set; }

        public string MyIP { get; set; }

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }
    }
}