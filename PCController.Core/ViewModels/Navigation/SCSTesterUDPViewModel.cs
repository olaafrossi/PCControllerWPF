// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 17
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public sealed class SCSTesterUDPViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch _stopwatch;
        private ObservableCollection<string> _udpRealTimeCollection = new ();
        private bool _carriageReturnTrue;
        private string _frameToSend;
        private bool _lineFeedTrue;
        private string _messageSent;
        private WindowChildParam _param;
        private static UdpShowControlManager _udpLink; //TODO how to dispose of a static field?
        private IMvxNavigationService _navigationService;
        private readonly IMvxLog _log;

        public SCSTesterUDPViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<SCSTesterUDPViewModel>();
            _navigationService = navigationService;
            _log.Info("SCSTesterUDPViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            // Setup UI Commands
            RefreshUdpMsgCommand = new MvxCommand(GetLogsFromManager);
            SendUdpCommand = new MvxCommand(SendUDPMessage);
            OpenUdpCommand = new MvxCommand(CreateUDPAsyncManager);
            CloseUdpCommand = new MvxCommand(DisposeUDPAsyncManager);

            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetLogsFromManager();

            // set initial UI Fields
            IPAddress = Settings.Default.AsyncUdpIPAddress;
            PortNum = Settings.Default.AsyncUdpRemotePort;
            LocalPortNum = Settings.Default.AsyncUdpLocalPort;
        }

        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        public IMvxCommand OpenUdpCommand { get; set; }

        public IMvxCommand CloseUdpCommand { get; set; }

        public IMvxCommand RefreshUdpMsgCommand { get; set; }

        public IMvxCommand SendUdpCommand { get; set; }

        public IList<UdpSenderModel> UdpGridRows { get; set; }

        public int ID { get; set; }

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

        public string IPAddress { get; set; }

        private int _portNum;

        public int PortNum
        {
            get { return _portNum; }
            set
            {
                SetProperty(ref _portNum, value);
                RaisePropertyChanged(() => UDPDriverOpenButtonStatus);
            }
        }

        private int _localPortNum;

        public int LocalPortNum
        {
            get { return _localPortNum; }
            set
            {
                SetProperty(ref _localPortNum, value);
                RaisePropertyChanged(() => UDPDriverOpenButtonStatus);
            }
        }

        public int ParentNo
        {
            get { return _param.ParentNo; }
        }

        public string Text
        {
            get { return $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}"; }
        }

        public string NumberOfUdpMsgToFetch { get; set; }

        public string DataBaseQueryTime { get; set; }

        public bool UDPLinkAlive { get; set; }

        private bool _udpDriverOpenButtonStatus;

        public bool UDPDriverOpenButtonStatus
        {
            get { return ValidPortNum is true && ValidLocalPortNum is true && UDPLinkAlive is false; } // TODO work on this UI logic. 
            set
            {
                SetProperty(ref _udpDriverOpenButtonStatus, value);
            }
        }

        public bool UDPDriverClosedButtonStatus { get; set; }

        public bool ValidPortNum
        {
            get { return PortNum > 0 && PortNum < 65535; }
        }

        public bool ValidLocalPortNum
        {
            get { return LocalPortNum > 0 && LocalPortNum < 65535; }
        }

        public bool CanSendMsg
        {
            get { return IPAddress?.Length > 0 && MessageSent?.Length > 0 && UDPLinkAlive is true; }
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
            get { return _udpRealTimeCollection; }
            set { SetProperty(ref _udpRealTimeCollection, value); }
        }

        public void CreateUDPAsyncManager()
        {
            Settings.Default.AsyncUdpIPAddress = IPAddress;
            Settings.Default.AsyncUdpRemotePort = PortNum;
            Settings.Default.AsyncUdpLocalPort = LocalPortNum;
            Settings.Default.Save();
            _log.Info("UDP settings have been saved");

            try
            {
                UdpShowControlManager link = new(IPAddress, PortNum, LocalPortNum, LogProvider);

                //Mvx.IoCProvider.Resolve<UdpShowControlManager>() = _udpLink;

                

                _udpLink = link;
                _log.Info("UDP has been created {IPAddress} {PortNum} {LocalPortNum}", IPAddress, PortNum, LocalPortNum);

                // Setup the binding and thread safety when msg's come in from _asyncUdpLink
                BindingOperations.EnableCollectionSynchronization(UDPRealTimeCollection, _udpLink);
                _udpLink.UDPDataReceived += UDPOnDataReceived;

                // set the UI
                UDPDriverOpenButtonStatus = false;
                UDPDriverClosedButtonStatus = true;
                UDPLinkAlive = true;
                RaisePropertyChanged(() => UDPDriverOpenButtonStatus);
                RaisePropertyChanged(() => UDPDriverClosedButtonStatus);
                RaisePropertyChanged(() => CanSendMsg);
                RaisePropertyChanged(() => UDPLinkAlive);
            }
            catch (Exception e)
            {
                _log.ErrorException("UDP driver was already open {e} but we are going to use the existing link", e);
                UDPRealTimeCollection.Insert(0, $"UDP driver was already open {e} but we are going to use the existing link");
                UDPDriverOpenButtonStatus = false;
                UDPDriverClosedButtonStatus = true;
                UDPLinkAlive = true;
                
                RaisePropertyChanged(() => UDPDriverOpenButtonStatus);
                RaisePropertyChanged(() => UDPDriverClosedButtonStatus);
                RaisePropertyChanged(() => CanSendMsg);
                RaisePropertyChanged(() => UDPLinkAlive);
            }
        }

        public void UDPOnDataReceived(object sender, EventArgs e)
        {
            _log.Info("Message from Remote {e}", e);
            string message = _udpLink.UdpFrameCombined;
            UDPRealTimeCollection.Insert(0, message);
        }

        private void DisposeUDPAsyncManager()
        {
            _log.Info("Disposed UDP Manager (WIP)");
            UDPDriverOpenButtonStatus = true;
            UDPDriverClosedButtonStatus = false;
            UDPLinkAlive = false;
            RaisePropertyChanged(() => UDPDriverOpenButtonStatus);
            RaisePropertyChanged(() => UDPDriverClosedButtonStatus);
            RaisePropertyChanged(() => CanSendMsg);
            RaisePropertyChanged(() => UDPLinkAlive);
        }

        public void GetLogsFromManager()
        {
            _stopwatch.Start();

            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.PCControllerDB));
            ComboBoxSQLParseManager parser = new ComboBoxSQLParseManager();

            int numLogs = parser.GetLogs(NumberOfUdpMsgToFetch);

            _log.Info("Getting Data Logs from {sql} number: {numOfMsgs}", sql, numLogs);

            _log.Info("Getting Data Logs{numOfMsgs}", numLogs);
            IList<UdpSenderModel> rows = sql.GetSomeUdpData(numLogs);
            
            UdpGridRows = rows;

            _stopwatch.Stop();
            string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDb;
            RaisePropertyChanged(() => UdpGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        private void SendUDPMessage()
        {
            if (_udpLink is null)
            {
                _log.Error("UDP driver null so we are going to create a new one");
                UDPRealTimeCollection.Insert(0, "UDP driver null so we are going to create a new one");
                CreateUDPAsyncManager();
            }
            _udpLink?.AddUdpFrame(FrameToSend);
            UDPRealTimeCollection.Insert(0, _udpLink?.UdpFrameCombined);
        }
    }
}