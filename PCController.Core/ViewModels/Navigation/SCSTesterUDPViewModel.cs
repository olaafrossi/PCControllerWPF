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
        private readonly IAsyncUdpLink _asyncUdpLink;

        private readonly Stopwatch _stopwatch;

        private readonly ObservableCollection<string> _uDPRealTimeCollection = new();

        private bool _carriageReturnTrue;

        private string _frameToSend;

        private bool _lineFeedTrue;

        private string _messageSent;

        private WindowChildParam _param;

        private string iPAddress;

        private ObservableCollection<string> uDPRealTimeCollection;

        private ObservableCollection<UdpSenderModel> udpSender = new();


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
            _stopwatch = new Stopwatch();
            GetUdpLogs();
            GetIpSuggestionsFromDb();

            // Setup the UDP Singleton
            IAsyncUdpLink asyncUdpLink = Mvx.IoCProvider.Resolve<IAsyncUdpLink>();
            _asyncUdpLink = asyncUdpLink;
            _asyncUdpLink.DataReceived += LinkOnDataReceived;

            // Setup the binding and thread safety when msg's come in from _asyncUdpLink
            UDPRealTimeCollection = _uDPRealTimeCollection;
            BindingOperations.EnableCollectionSynchronization(UDPRealTimeCollection, _asyncUdpLink);
        }

        public int ID { get; set; }

        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }

        public string RemoteIP { get; set; }

        public string LocalIP { get; set; } = GetLocalIPAddress();

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }

        public IList<UdpSenderModel> UdpGridRows { get; set; }

        public int ParentNo
        {
            get { return _param.ParentNo; }
        }

        public string Text
        {
            get { return $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}"; }
        }

        public IMvxCommand SendUdpCommand { get; set; }

        public string NumberOfUdpMsgToFetch { get; set; }

        public IMvxCommand RefreshUdpMsgCommand { get; set; }

        public bool CanSendMsg
        {
            get { return IPAddress?.Length > 0 && MessageSent?.Length > 0; }
        }

        public string MessageSent
        {
            get { return _messageSent; }

            set
            {
                SetProperty(ref _messageSent, value);
                RaisePropertyChanged(() => CanSendMsg);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        public ObservableCollection<string> UDPRealTimeCollection
        {
            get { return uDPRealTimeCollection; }
            set { SetProperty(ref uDPRealTimeCollection, value); }
        }

        public bool CarriageReturnTrue
        {
            get { return _carriageReturnTrue; }

            set
            {
                SetProperty(ref _carriageReturnTrue, value);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        public bool LineFeedTrue
        {
            get { return _lineFeedTrue; }

            set
            {
                SetProperty(ref _lineFeedTrue, value);
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
                    FrameToSend = MessageSent;
                    RaisePropertyChanged(() => MessageSent);
                    return _frameToSend;
                }

                if (CarriageReturnTrue is true && LineFeedTrue is false)
                {
                    FrameToSend = $"{MessageSent}!0D";
                    RaisePropertyChanged(() => MessageSent);
                    return _frameToSend;
                }

                if (LineFeedTrue is true && CarriageReturnTrue is false)
                {
                    FrameToSend = $"{MessageSent}!0A";
                    RaisePropertyChanged(() => MessageSent);
                    return _frameToSend;
                }

                if (CarriageReturnTrue is true && LineFeedTrue is true)
                {
                    FrameToSend = $"{MessageSent}!0D!0A";
                    RaisePropertyChanged(() => MessageSent);
                    return _frameToSend;
                }

                RaisePropertyChanged(() => MessageSent);
                return _frameToSend;
            }

            set { SetProperty(ref _frameToSend, value); }
        }

        public IList<string> IpList { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IMvxCommand IPBoxTextChangeCommand { get; set; }

        public IMvxCommand OpenUdpCommand { get; set; }

        public IMvxCommand CloseUdpCommand { get; set; }

        private void DisposeUDPAsyncManager()
        {
            //throw new NotImplementedException();
            //TODO move much of this logic into the UdpManager class
        }

        private void CreateUDPAsyncManager()
        {
            //throw new NotImplementedException();
            //TODO move much of this logic into the UdpManager class
        }

        public void AddUdpFrame()
        {
            try
            {
                string message = FrameToSend;

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

            WriteUDPDataToDataBase(true);
        }

        public void WriteUDPDataToDataBase(bool sentTypeMessage)
        {
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            UdpSenderModel udpFrame = new UdpSenderModel();

            if (sentTypeMessage is true)
            {
                udpFrame.OutgoingMessage = FrameToSend;
                udpFrame.IncomingMessage = IncomingMessage;
                udpFrame.RemoteIP = _asyncUdpLink.Address;
                udpFrame.LocalIP = LocalIP;
                udpFrame.LocalPort = _asyncUdpLink.LocalPort;
                udpFrame.RemotePort = _asyncUdpLink.Port;
                udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql.InsertUdpSentData(udpFrame);

                string udpFrameCombine =
                    $"SENT: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Sent Frame: {FrameToSend} Remote IP: {_asyncUdpLink.Address} This IP: {LocalIP} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";
                UDPRealTimeCollection.Insert(0, udpFrameCombine);
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
                UDPRealTimeCollection.Insert(0, udpFrameCombine);
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
                Serilog.Log.Logger.Error("No network adapters with an IPv4 address in the system!{e}", e);
            }

            Serilog.Log.Logger.Error("No network adapters with an IPv4 address in the system!");
            return "No network adapters with an IPv4 address in the system!";
        }


        public void LinkOnDataReceived(object sender, EventArgs e)
        {
            byte[] dataBytes = _asyncUdpLink.GetMessage();
            string messageFromController = Encoding.ASCII.GetString(dataBytes); // new byte array and feed it the input string
            Log.Info("Message from Remote {messageFromController}", messageFromController);
            IncomingMessage = messageFromController;
            WriteUDPDataToDataBase(false);
            IncomingMessage = string.Empty; // clear the prop
        }

        private void GetUdpLogs()
        {
            _stopwatch.Start();
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
                Log.Error("Didn't parse the number in the Net ComboBox {numOfMsgs}", numOfMsgs, e);
            }

            Log.Info("Getting Data Logs{numOfMsgs}", numOfMsgs);
            var rows = sql.GetSomeUdpData(numOfMsgs);
            UdpGridRows = rows;

            _stopwatch.Stop();
            string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDb;
            RaisePropertyChanged(() => UdpGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        private void GetIpSuggestionsFromDb()
        {
            _stopwatch.Start();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            int numOfSuggestions = 20;

            Log.Info("Getting IP Address Suggestions {numOfSuggestions}", numOfSuggestions);

            var rows = sql.GetUdpUsedIPAddresses(numOfSuggestions);
            IpList = rows;

            _stopwatch.Stop();
            string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDb;
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

        public string LocalIP { get; set; }

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }
    }
}