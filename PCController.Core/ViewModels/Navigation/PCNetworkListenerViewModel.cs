// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 15
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet.NetworkUtils;
// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public sealed class PCNetworkListenerViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;
        private readonly IMvxLog _log;

        public PCNetworkListenerViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<PCNetworkListenerViewModel>();
            _log.Info("PCNetworkListenerViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            // Setup UI Commands
            RefreshNetMsgCommand = new MvxCommand(GetLogsFromManager);

            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetLogsFromManager();

            // get singleton and create event handler
            IPcNetworkListener pcNetworkListener = Mvx.IoCProvider.Resolve<IPcNetworkListener>();
            pcNetworkListener.MessageHit += PCNetworkManagerOnMessage;

            // set initial UI Fields
            int listeningPort = pcNetworkListener.GetAppSettingsDataUdpPort();
            ListeningUDPPort = listeningPort.ToString();
            TimeSinceLastStartup = GetDateFromTimeSpan();
            RemoteControlTimeStamp = "";
        }

        public IMvxCommand RefreshNetMsgCommand { get; set; }

        public string ListeningUDPPort { get; set; }

        public string RemoteControlIP { get; set; }

        public string RemoteControlPort { get; set; }

        public string RemoteControlTimeStamp { get; set; }

        public string RemoteControlLastMessage { get; set; }

        public TimeSpan RemoteControlTimeSinceLastMessage { get; set; } = new TimeSpan(0, 0, 0, 0);

        public IList<NetworkMessageModel> NetGridRows { get; set; }

        public string DataBaseQueryTime { get; set; }

        public string NumberOfNetMsgToFetch { get; set; }

        public DateTime TimeSinceLastStartup { get; set; }

        public int ParentNo
        {
            get
            {
                return _param.ParentNo;
            }
        }

        public string Text
        {
            get
            {
                return $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";
            }
        }

        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        private static DateTime GetDateFromTimeSpan()
        {
            long tickCountMs = Environment.TickCount64;
            return DateTime.Now.Subtract(TimeSpan.FromMilliseconds(tickCountMs));
        }

        public void GetLogsFromManager()
        {
            _stopwatch.Start();

            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.PCControllerDB));
            ProcMonitorModel procData = new();
            ComboBoxSQLParseManager parser = new ComboBoxSQLParseManager();

            int numLogs = parser.GetLogs(NumberOfNetMsgToFetch);

            _log.Info("Getting Data Logs from {sql} number: {numOfMsgs}", sql, numLogs);
            IList<NetworkMessageModel> rows = sql.GetSomeNetData(numLogs);

            NetGridRows = rows;

            _stopwatch.Stop();

            string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDb;
            RaisePropertyChanged(() => NetGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        private void PCNetworkManagerOnMessage(object sender, NetworkMessagesEventArgs e)
        {
            
            NetWorkTimer(false);
            Thread.Sleep(10);
            NetWorkTimer(true);

            PCNetworkManager pc = new(e);
            RemoteControlIP = e.RemoteIP;
            RemoteControlPort = e.RemotePort;
            RemoteControlTimeStamp = e.Timestamp;
            RemoteControlLastMessage = e.IncomingMessage;

            RaisePropertyChanged(() => RemoteControlIP);
            RaisePropertyChanged(() => RemoteControlPort);
            RaisePropertyChanged(() => RemoteControlTimeStamp);
            RaisePropertyChanged(() => RemoteControlLastMessage);

            _log.Info("Message from PC network shutdown controller {sender} {e}", sender, e);
        }

        private void NetWorkTimer(bool run)
        {
            DispatcherTimer timer = new (){Interval = TimeSpan.FromSeconds(1)};

            if (run is true)
            {
                timer.Tick += TimerOnTick;
                timer.Start();
            }
            else
            {
                // stop the event handler to reset the property field
                timer.Tick -= TimerOnTick;
                timer.Stop();

                RemoteControlTimeSinceLastMessage = timer.Interval;
                RaisePropertyChanged(() => RemoteControlTimeSinceLastMessage);
            }
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            TimeSpan timeTick = new TimeSpan(0, 0, 0, 1);
            RemoteControlTimeSinceLastMessage = RemoteControlTimeSinceLastMessage.Add(timeTick);
            RaisePropertyChanged(() => RemoteControlTimeSinceLastMessage);
        }
    }
}