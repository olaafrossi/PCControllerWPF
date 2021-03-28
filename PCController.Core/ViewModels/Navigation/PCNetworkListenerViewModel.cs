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
        public Stopwatch NetStopwatch = new();

        public PCNetworkListenerViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            Log.Info("PCNetworkListenerViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            // Setup UI Commands
            RefreshNetMsgCommand = new MvxCommand(GetNetLogs);

            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetNetLogs();

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

        private void GetNetLogs()
        {
            _stopwatch.Start();
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            int numOfMsgs = 20;

            try
            {
                RaisePropertyChanged(() => NumberOfNetMsgToFetch);
                if (NumberOfNetMsgToFetch is null)
                {
                    numOfMsgs = 20;
                }
                else if (NumberOfNetMsgToFetch.Contains("All"))
                {
                    // All
                    numOfMsgs = 100000000;
                }
                else if (NumberOfNetMsgToFetch.Length == 40)
                {
                    // 20, 50
                    string logComboBoxSelected = NumberOfNetMsgToFetch.Substring(38, 2);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfNetMsgToFetch.Length == 41)
                {
                    // hundred
                    string logComboBoxSelected = NumberOfNetMsgToFetch.Substring(38, 3);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfNetMsgToFetch.Length == 42)
                {
                    // thousand
                    string logComboBoxSelected = NumberOfNetMsgToFetch.Substring(38, 4);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("Didn't parse the number in the Net ComboBox {numOfMsgs}", numOfMsgs, e);
            }

            Log.Info("Getting Data Logs{numOfMsgs}", numOfMsgs);
            IList<NetworkMessageModel> rows = sql.GetSomeNetData(numOfMsgs);
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

            Log.Info("Message from PC network shutdown controller {sender} {e}", sender, e);
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