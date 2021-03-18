// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 15
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core.ViewModels
{
    public class PCNetworkListenerViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;

        public Stopwatch NetStopwatch = new Stopwatch();

        private WindowChildParam _param;

        public PCNetworkListenerViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            RefreshNetMsgCommand = new MvxCommand(GetNetLogs);

            Serilog.Log.Logger.Information("PCNetworkListenerViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            IPcNetworkListener pcNetworkListener = Mvx.IoCProvider.Resolve<IPcNetworkListener>();
            
            int listeningPort = pcNetworkListener.GetAppSettingsDataUdpPort();
            ListeningUDPPort = listeningPort.ToString();

            pcNetworkListener.MessageHit += PCNetworkManagerOnMessage;

            stopwatch = new Stopwatch();
            GetNetLogs();
        }

        public string ListeningUDPPort { get; set; }

        public string RemoteControlIP { get; set; }

        public string RemoteControlPort { get; set; }

        public string RemoteControlTimeStamp { get; set; }

        public string RemoteControlLastMessage { get; set; }

        public string RemoteControlTimeSinceLastMessage { get; set; }

        public IList<NetworkMessageModel> NetGridRows { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IMvxCommand RefreshNetMsgCommand { get; set; }

        public string NumberOfNetMsgToFetch { get; set; }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        private void PCNetworkManagerOnMessage(object sender, NetworkMessagesEventArgs e)
        {
            NetStopwatch.Start();
            
            PCNetworkManager pc = new PCNetworkManager(e);

            RemoteControlIP = e.RemoteIP;
            RaisePropertyChanged(() => RemoteControlIP);
            
            RemoteControlPort = e.RemotePort;
            RaisePropertyChanged(() => RemoteControlPort);
           
            RemoteControlTimeStamp = e.Timestamp;
            RaisePropertyChanged(() => RemoteControlTimeStamp);
            
            RemoteControlLastMessage = e.IncomingMessage;
            RaisePropertyChanged(() => RemoteControlLastMessage);
            
            string t = $"{NetStopwatch.ElapsedMilliseconds.ToString()} ms";
            RemoteControlTimeSinceLastMessage = t;
            RaisePropertyChanged(() => RemoteControlTimeSinceLastMessage);
            
            NetStopwatch.Stop();
            NetStopwatch.Reset();

        }

        private async Task StartNetWorkTimer()
        {
            
            do
            {

            } 
            while (NetStopwatch.IsRunning);

        }

        private void GetNetLogs()
        {
            stopwatch.Start();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
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

            Serilog.Log.Logger.Information("Getting Data Logs{numOfMsgs}", numOfMsgs);
            var rows = sql.GetSomeNetData(numOfMsgs);
            NetGridRows = rows;

            stopwatch.Stop();
            string timeToFetchFromDB = $" DB query time: {stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDB;
            RaisePropertyChanged(() => NetGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        public override void Prepare(WindowChildParam param) => _param = param;
    }
}