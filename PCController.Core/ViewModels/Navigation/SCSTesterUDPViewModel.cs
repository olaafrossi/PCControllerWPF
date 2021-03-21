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
        private UdpShowControlManager _udpLink;

        private readonly Stopwatch _stopwatch;

        private readonly ObservableCollection<string> _uDPRealTimeCollection = new();

        private string _messageSent;

        private WindowChildParam _param;

        private string iPAddress;

        private ObservableCollection<string> uDPRealTimeCollection;

        private ObservableCollection<UdpSenderModel> udpSender = new();

        public void CreateUDPAsyncManager()
        {
            UdpShowControlManager link = new UdpShowControlManager(IPAddress, PortNum, LocalPortNum);
            _udpLink = link;
        }


        public IMvxCommand OpenUdpCommand { get; set; }

        public IMvxCommand CloseUdpCommand { get; set; }

        public SCSTesterUDPViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            // Setup UI Commands
            RefreshUdpMsgCommand = new MvxCommand(GetUdpLogs);
            SendUdpCommand = new MvxCommand(SendUDPMessage);
            IPBoxTextChangeCommand = new MvxCommand(GetIpSuggestionsFromDb);
            OpenUdpCommand = new MvxCommand(CreateUDPAsyncManager);
            CloseUdpCommand = new MvxCommand(DisposeUDPAsyncManager);

            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetUdpLogs();
            GetIpSuggestionsFromDb();

            // Setup the UDP Singleton
            //IAsyncUdpLink asyncUdpLink = Mvx.IoCProvider.Resolve<IAsyncUdpLink>();
            //_asyncUdpLink = asyncUdpLink;
            //_asyncUdpLink.DataReceived += LinkOnDataReceived;

            // Setup the binding and thread safety when msg's come in from _asyncUdpLink
            UDPRealTimeCollection = _uDPRealTimeCollection;
            //BindingOperations.EnableCollectionSynchronization(UDPRealTimeCollection, _udpLink);
            
        }

        private void DisposeUDPAsyncManager()
        {
            _udpLink.DisposeUDPLink();
        }

        private void SendUDPMessage()
        {
            _udpLink.AddUdpFrame(FrameToSend);
            UDPRealTimeCollection.Insert(0, _udpLink.UdpFrameCombined);
        }


        public int ID { get; set; }

        private bool _lineFeedTrue;

        private string _frameToSend;

        private bool _carriageReturnTrue;

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

            set
            {
                SetProperty(ref _frameToSend, value);
            }
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

        public string IPAddress { get; set; } = Settings.Default.AsyncUdpIPAddress;

        public int PortNum { get; set; } = Settings.Default.AsyncUdpRemotePort;

        public int LocalPortNum { get; set; } = Settings.Default.AsyncUdpLocalPort;

       

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

        public string DataBaseQueryTime { get; set; }

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

        public IList<string> IpList { get; set; }

        public IMvxCommand IPBoxTextChangeCommand { get; set; }



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


}