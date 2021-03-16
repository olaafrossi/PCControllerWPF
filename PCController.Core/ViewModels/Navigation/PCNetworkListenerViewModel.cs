// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 15
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Diagnostics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;

namespace PCController.Core.ViewModels
{
    public class PCNetworkListenerViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;

        private WindowChildParam _param;

        public PCNetworkListenerViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            RefreshNetMsgCommand = new MvxCommand(GetNetLogs);

            Serilog.Log.Logger.Information("PCNetworkListenerViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            var pcNetworkListener = Mvx.IoCProvider.Resolve<IPcNetworkListener>();

            pcNetworkListener.MessageHit += PCNetworkManagerOnMessage;

            stopwatch = new Stopwatch();
            stopwatch.Start();
            GetNetLogs();
        }

        public IList<NetworkMessageModel> NetGridRows { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IMvxCommand RefreshNetMsgCommand { get; set; }

        public string NumberOfNetMsgToFetch { get; set; }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        private void PCNetworkManagerOnMessage(object sender, NetworkMessagesEventArgs e)
        {
            PCNetworkManager pc = new PCNetworkManager(e);
        }

        private void GetNetLogs()
        {

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