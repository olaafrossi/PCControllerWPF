// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 16
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Data;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;

// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public sealed class WatchdogViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly object _processMonitorLock = new();
        private readonly IProcessMonitor _procMonitor;
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;
        private ObservableCollection<string> _procMonRealTimeCollection = new();

        public WatchdogViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            Log.Info("WatchdogViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            // Setup UI Commands
            RefreshProcLogsCommand = new MvxCommand(GetProcLogs);
            StopProcMonitorCommand = new MvxCommand(StopProcMonitor);
            StartProcMonitorCommand = new MvxCommand(StartProcMonitor);

            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetProcLogs();


            // get singleton and create event handlers
            lock (_processMonitorLock)
            {
                _procMonitor = Mvx.IoCProvider.Resolve<IProcessMonitor>();
                _procMonitor.ProcessEvent += OnProcessEvent;
                _procMonitor.ProcessExited += OnProcessExited;
                _procMonitor.ResourceEvent += OnResourceEvent;
                // Setup the binding and thread safety when msg's come in from _procMonitor
                BindingOperations.EnableCollectionSynchronization(ProcMonRealTimeCollection, _procMonitor);
            }

            // set initial UI Fields
            ProcessName = _procMonitor.ProcessName;
            ExecutionString = _procMonitor.ExecutionString;
            ResourceSnapshotInterval = _procMonitor.ResourceSnapshotInterval;
            UnresponsiveTimeout = _procMonitor.UnresponsiveTimeout;
            ProcMonitorStoppedButtonStatus = true;
            ProcMonitorStartedButtonStatus = true;

            RaisePropertyChanged(() => ProcMonitorStoppedButtonStatus);
            RaisePropertyChanged(() => ProcMonitorStartedButtonStatus);
        }


        public IMvxCommand RefreshProcLogsCommand { get; set; }

        public IMvxCommand StopProcMonitorCommand { get; set; }

        public IMvxCommand StartProcMonitorCommand { get; set; }

        public bool ProcMonitorStoppedButtonStatus { get; set; }

        public bool ProcMonitorStartedButtonStatus { get; set; }

        public string ProcessName { get; set; }

        public string ExecutionString { get; set; }

        public TimeSpan ResourceSnapshotInterval { get; set; }

        public TimeSpan UnresponsiveTimeout { get; set; }

        public string NumberOfProcLogsToFetch { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IList<ProcMonitorModel> ProcGridRows { get; set; }

        public ObservableCollection<string> ProcMonRealTimeCollection
        {
            get { return _procMonRealTimeCollection; }
            set { SetProperty(ref _procMonRealTimeCollection, value); }
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public override void Prepare(WindowChildParam param) => _param = param;

        public void WriteErrorDataToDataBase(string error)
        {
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            ProcMonitorModel procData = new();
            procData.Timestamp = DateTime.Now;
            procData.Message = error;
            sql.InsertProcData(procData);
        }

        public void WriteProcDataToDataBase(ResourceSnapshot snap)
        {
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            ProcMonitorModel procData = new();

            procData.PeakPagedMemorySize = snap.PeakPagedMemorySize;
            procData.PeakWorkingSet = snap.PeakWorkingSet;
            procData.PrivateMemorySize = snap.PrivateMemorySize;
            procData.ThreadCount = snap.ThreadCount;
            procData.HandleCount = snap.HandleCount;
            procData.IsNotResponding = snap.IsNotResponding;
            procData.Timestamp = DateTime.Now;
            procData.Message = "";
            sql.InsertProcData(procData);
        }

        private string DumpToUserFile(string processName, string text)
        {
            try
            {
                //Create a userfile with a timestamp that will be accesible
                string filename = string.Format("{0}_{1}.txt", processName, DateTime.Now.ToString("yyyyMMdd-hhmmss"));
                string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Watchdog", filename);
                string filedir = Path.GetDirectoryName(filepath);

                //Ensure the directory exists
                if (!Directory.Exists(filedir))
                {
                    Directory.CreateDirectory(filedir);
                }

                File.WriteAllText(filepath, text);
                Log.Error("User process file written to: {0}", filepath);
                return filepath;
            }
            catch (Exception ex)
            {
                Log.Error("Could not dump to user file", ex);
                return null;
            }
        }

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

        private void OnProcessEvent(object? sender, ProcessEventArgs e)
        {
            ProcMonRealTimeCollection.Insert(0, e.ToString());
            RaisePropertyChanged(() => ProcMonRealTimeCollection);
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            lock (_processMonitorLock)
            {
                if (_procMonitor != null)
                {
                    //print the last 30 snapshots
                    StringBuilder sb = new();
                    sb.AppendLine("Process Exited");
                    int snapshots = 30; //At most
                    foreach (ResourceSnapshot s in _procMonitor.GetSnapshotHistory())
                    {
                        if (--snapshots < 0)
                        {
                            break;
                        }

                        sb.AppendLine(s.ToString());
                    }

                    ProcMonRealTimeCollection.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} Monitored Process Died {e}");
                    RaisePropertyChanged(() => ProcMonRealTimeCollection);
                    string userfile = DumpToUserFile(ProcessName, sb.ToString());
                    Console.WriteLine(userfile);
                    WriteErrorDataToDataBase("Monitored Process Died");
                    //SetMessage(string.Format("File written to {0}", userfile));
                }
            }
        }

        private void OnResourceEvent(object? sender, ResourceSnapshot e)
        {
            ProcMonRealTimeCollection.Insert(0, e.ToString());
            RaisePropertyChanged(() => ProcMonRealTimeCollection);
            WriteProcDataToDataBase(e);
        }

        private void StartProcMonitor()
        {
        }

        private void StopProcMonitor()
        {
            _procMonitor.Kill();
        }
    }
}