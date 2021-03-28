// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 16
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
using ThreeByteLibrary.Dotnet;
using ThreeByteLibrary.Dotnet.NetworkUtils;
// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public sealed class WatchdogViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;
        private IProcessMonitor _procMonitor;

        public WatchdogViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            Log.Info("WatchdogViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            // Setup UI Commands
            RefreshProcLogsCommand = new MvxCommand(GetProcLogs);

            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetProcLogs();


            // get singleton and create event handlers
            _procMonitor = Mvx.IoCProvider.Resolve<IProcessMonitor>();
            _procMonitor.ProcessEvent += OnProcessEvent;
            _procMonitor.ProcessExited += OnProcessExited;

            // set initial UI Fields
            ProcessName = _procMonitor.ProcessName;
            ExecutionString = _procMonitor.ExecutionString;
            ResourceSnapshotInterval = _procMonitor.ResourceSnapshotInterval;
            UnresponsiveTimeout = _procMonitor.UnresponsiveTimeout;
        }


        public IMvxCommand RefreshProcLogsCommand { get; set; }

        public string ProcessName { get; set; }

        public string ExecutionString { get; set; }

        public TimeSpan ResourceSnapshotInterval { get; set; }

        public TimeSpan UnresponsiveTimeout { get; set; }

        public string ResourceSnaphotCombined { get; set; }

        public string NumberOfProcLogsToFetch { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IList<ProcMonitorModel> ProcGridRows { get; set; }


        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";



        private void GetProcLogs()
        {
            _stopwatch.Start();
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            int numOfMsgs = 20;

            try
            {
                RaisePropertyChanged(() => NumberOfProcLogsToFetch);
                if (NumberOfProcLogsToFetch is null)
                {
                    numOfMsgs = 20;
                }
                else if (NumberOfProcLogsToFetch.Contains("All"))
                {
                    // All
                    numOfMsgs = 100000000;
                }
                else if (NumberOfProcLogsToFetch.Length == 40)
                {
                    // 20, 50
                    string logComboBoxSelected = NumberOfProcLogsToFetch.Substring(38, 2);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfProcLogsToFetch.Length == 41)
                {
                    // hundred
                    string logComboBoxSelected = NumberOfProcLogsToFetch.Substring(38, 3);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfProcLogsToFetch.Length == 42)
                {
                    // thousand
                    string logComboBoxSelected = NumberOfProcLogsToFetch.Substring(38, 4);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
            }
            catch (Exception e)
            {
                Log.Error("Didn't parse the number in the Net ComboBox {numOfMsgs}", numOfMsgs, e);
            }

            Log.Info("Getting Data Logs{numOfMsgs}", numOfMsgs);
            IList<ProcMonitorModel> rows = sql.GetSomeProcData(numOfMsgs);
            ProcGridRows = rows;

            _stopwatch.Stop();
            string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDb;
            RaisePropertyChanged(() => ProcGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnProcessEvent(object? sender, ProcessEventArgs e)
        {
            //WriteProcDataToDataBase();
        }

        //public void WriteProcDataToDataBase()
        //{
        //    SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
        //    ProcMonitorModel procData = new();

        //    procData = _procMonitor.GetSnapshotHistory{
        //        _procMonitor.GetSnapshotHistory
        //    }
            

        //    procData.PeakPagedMemorySize = _procMonitor.PeakPagedMemorySize;
        //    procData.PeakWorkingSet = PeakWorkingSet;
        //    procData.RemoteIP = _asyncUdpLink.Address;
        //    procData.LocalIP = GetLocalIPAddress();
        //    procData.LocalPort = _asyncUdpLink.LocalPort;
        //    procData.RemotePort = _asyncUdpLink.Port;
        //    procData.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //            sql.InsertProcData(procData);

        //            string procDataCombine =
        //                $"SENT: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Sent Frame: {frameToSend} Remote IP: {_asyncUdpLink.Address} This IP: {GetLocalIPAddress()} Remote Port: {_asyncUdpLink.LocalPort} Local Port: {_asyncUdpLink.Port}";

        //    ResourceSnaphotCombined = procDataCombine;
        //}

        public override void Prepare(WindowChildParam param) => _param = param;
    }
}