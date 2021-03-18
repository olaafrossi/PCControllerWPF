// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 17
// by Olaaf Rossi

using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.ViewModels
{
    public class SCSTesterViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private IAsyncUdpLink _asyncUdpLink;

        private readonly Stopwatch stopwatch;

        private WindowChildParam _param;

        private bool carriageReturnTrue;

        private string frameToSend;

        private string iPAddress;

        private bool lineFeedTrue;

        private string messageSent;

        public Stopwatch NetStopwatch = new();

        private ObservableCollection<UdpSenderModel> udpSender = new();

        public SCSTesterViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            RefreshUdpMsgCommand = new MvxCommand(GetUdpLogs);

            //this.StartUdpDriverCommand = new MvxCommand(this.StartUdpDriver);
            SendUdpCommand = new MvxCommand(AddUdpFrame);
            IPBoxTextChangeCommand = new MvxCommand(GetIpSuggestionsFromDb);

            stopwatch = new Stopwatch();

            IAsyncUdpLink asyncUdpLink = Mvx.IoCProvider.Resolve<IAsyncUdpLink>();

            _asyncUdpLink = asyncUdpLink;

            _asyncUdpLink.DataReceived += LinkOnDataReceived;

            GetUdpLogs();

            GetIpSuggestionsFromDb();
        }

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
                //Log this
                Log.Error("this is bad {e}", e);
                //throw;
            }

            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            UdpSenderModel udpFrame = new UdpSenderModel();

            udpFrame.OutgoingMessage = FrameToSend;
            udpFrame.IncomingMessage = "some string from SCS";
            udpFrame.RemoteIP = _asyncUdpLink.Address;
            udpFrame.MyIP = MyIP;
            udpFrame.LocalPort = _asyncUdpLink.LocalPort;
            udpFrame.RemotePort = _asyncUdpLink.Port;
            udpFrame.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            sql.InsertUdpSentData(udpFrame);

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

        public int ID { get; set; }

        public string IncomingMessage { get; set; }

        public string OutgoingMessage { get; set; }

        public string RemoteIP { get; set; }

        public string MyIP { get; set; } = GetLocalIPAddress();

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public string Timestamp { get; set; }

        public int UDPPort { get; set; }


        public void LinkOnDataReceived(object sender, EventArgs e)
        {
            Console.WriteLine($"this might be a message from the controlller {e}");
            Console.WriteLine(_asyncUdpLink.GetMessage());
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

        public string IPAddress { get; set; } = Properties.Settings.Default.AsyncUdpIPAddress;

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

        public int PortNum { get; set; } = Properties.Settings.Default.AsyncUdpRemotePort;
        public int LocalPortNum { get; set; } = Properties.Settings.Default.AsyncUdpLocalPort;

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
            }
            set { SetProperty(ref frameToSend, value); }
        }

        public bool UdpDriverClosed { get; set; }

        public IList<string> IpList { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IMvxCommand IPBoxTextChangeCommand { get; set; }

        public IMvxCommand StartUdpDriverCommand { get; set; }

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