// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 16
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
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
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;
        private IProcessMonitor _procMonitor;
        private ObservableCollection<string> _procMonRealTimeCollection = new();

        // chart fields
        private int index;
        private ObservableCollection<ObservablePoint> _threadCount;
        private ObservableCollection<ObservablePoint> _peakPagedMemorySize;
        private ObservableCollection<ObservablePoint> _peakWorkingSet;
        private ObservableCollection<ObservablePoint> _privateMemorySize;

        public WatchdogViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            Log.Info("WatchdogViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            // Setup UI Commands
            RefreshProcLogsCommand = new MvxCommand(GetLogsFromManager);
            StopProcMonitorCommand = new MvxCommand(StopProcMonitor);
            StartProcMonitorCommand = new MvxCommand(StartProcMonitor);
            AddItemCommand = new MvxCommand(AddThreadToChart);
            RemoveItemCommand = new MvxCommand(RemoveThreadFromChart);
            //AddSeriesCommand = new MvxCommand(AddSeries);
            RemoveSeriesCommand = new MvxCommand(RemoveLastSeries);


            // Fetch Initial Data
            _stopwatch = new Stopwatch();
            GetLogsFromManager();

            // start the proc mon
            ResolveAndStartProcMon();


            // set initial UI Fields
            ProcessName = _procMonitor.ProcessName;
            ExecutionString = _procMonitor.ExecutionString;
            ResourceSnapshotInterval = _procMonitor.ResourceSnapshotInterval;
            UnresponsiveTimeout = _procMonitor.UnresponsiveTimeout;
            ProcMonitorStoppedButtonStatus = true;
            ProcMonitorStartedButtonStatus = true;
            AddChart();
            AddMemoryChart();



            RaisePropertyChanged(() => ProcMonitorStoppedButtonStatus);
            RaisePropertyChanged(() => ProcMonitorStartedButtonStatus);
        }

        public IMvxCommand AddItemCommand { get; set; }

        public IMvxCommand RemoveItemCommand { get; set; }

        public IMvxCommand AddSeriesCommand { get; set; }

        public IMvxCommand RemoveSeriesCommand { get; set; }

        public IMvxCommand RefreshProcLogsCommand { get; set; }

        public IMvxCommand StopProcMonitorCommand { get; set; }

        public IMvxCommand StartProcMonitorCommand { get; set; }

        public bool ProcMonitorStoppedButtonStatus { get; set; }

        public bool ProcMonitorStartedButtonStatus { get; set; }

        public ObservableCollection<ISeries> ThreadSeries { get; set; }

        public ObservableCollection<ISeries> MemorySeries { get; set; }

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

        public int ProcessThreadCount { get; set; }

        public long PeakPagedMemorySize { get; set; }

        public long PeakWorkingSet { get; set; }

        public long PrivateMemorySize { get; set; }

        public void AddChart()
        {

            ThreadSeries = new ObservableCollection<ISeries>();

            _threadCount = new ObservableCollection<ObservablePoint> { new(index++, 1), new(index++, 1) };
            ThreadSeries.Add(new LineSeries<ObservablePoint> { Values = _threadCount });
            AddSeries();


        }

        public void AddMemoryChart()
        {

            MemorySeries = new ObservableCollection<ISeries>();

            _peakPagedMemorySize = new ObservableCollection<ObservablePoint> { new(index++, 1), new(index++, 1) };
            MemorySeries.Add(new LineSeries<ObservablePoint> { Values = _peakPagedMemorySize });

            _peakWorkingSet = new ObservableCollection<ObservablePoint> { new(index++, 1), new(index++, 1) };
            MemorySeries.Add(new LineSeries<ObservablePoint> { Values = _peakWorkingSet });

            _privateMemorySize = new ObservableCollection<ObservablePoint> { new(index++, 1), new(index++, 1) };
            MemorySeries.Add(new LineSeries<ObservablePoint> { Values = _privateMemorySize });
            
            AddMemorySeries();


        }

        public void AddThreadToChart()
        {
            if (_threadCount.Count > 120)
            {
                RemoveThreadFromChart();
            }
            else
            {
                _threadCount.Add(new ObservablePoint(index++, ProcessThreadCount));
            }
        }

        public void AddMemToChart()
        {

            if (_peakPagedMemorySize.Count > 120)
            {
                RemoveMemFromChart();
            }
            else
            {
                _peakPagedMemorySize.Add(new ObservablePoint(index++, PeakPagedMemorySize));
                _peakWorkingSet.Add(new ObservablePoint(index++, PeakWorkingSet));
                _privateMemorySize.Add(new ObservablePoint(index++, PrivateMemorySize));
            }

        }

        public void AddSeries()
        {
            if (ThreadSeries.Count == 5)
            {
                return;
            }
            ThreadSeries.Add(new LineSeries<int> { Values = new List<int> { 0 } });
        }

        public void AddMemorySeries()
        {
            if (MemorySeries.Count == 5)
            {
                return;
            }
            MemorySeries.Add(new LineSeries<long> { Values = new List<long> { 0 } });
        }

        public void RemoveThreadFromChart()
        {
            if (_threadCount.Count < 2) return;

            _threadCount.RemoveAt(0);
        }

        public void RemoveMemFromChart()
        {
            if (_peakPagedMemorySize.Count < 2) return;

            _peakPagedMemorySize.RemoveAt(0);
        }

        public void RemoveLastSeries()
        {
            if (ThreadSeries.Count == 1) return;

            ThreadSeries.RemoveAt(ThreadSeries.Count - 1);
        }

        public override void Prepare(WindowChildParam param) => _param = param;

        public void WriteErrorDataToDataBase(string error)
        {
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.PCControllerDB));
            ProcMonitorModel procData = new();
            procData.Timestamp = DateTime.Now;
            procData.Message = error;
            sql.InsertProcData(procData);
        }

        public void WriteProcDataToDataBase(ResourceSnapshot snap)
        {
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.PCControllerDB));
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

        public void GetLogsFromManager()
        {
            _stopwatch.Start();
            
            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.PCControllerDB));
            ProcMonitorModel procData = new();
            ComboBoxSQLParseManager parser = new ComboBoxSQLParseManager();
            
            int numLogs = parser.GetLogs(NumberOfProcLogsToFetch);

            Log.Info("Getting Data Logs from {sql} number: {numOfMsgs}", sql, numLogs);
            IList<ProcMonitorModel> rows = sql.GetSomeProcData(numLogs);
            
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
            ProcMonRealTimeCollection.Insert(0, "I think the process is frozen");
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
            ProcessThreadCount = e.ThreadCount;

            long toMegaBytes = 1024;

            PeakPagedMemorySize = e.PeakPagedMemorySize / toMegaBytes /toMegaBytes;
            PrivateMemorySize = e.PrivateMemorySize / toMegaBytes / toMegaBytes;
            PeakWorkingSet = e.PeakWorkingSet / toMegaBytes / toMegaBytes;



            WriteProcDataToDataBase(e);

            RaisePropertyChanged(() => ProcMonRealTimeCollection);
            RaisePropertyChanged(() => ProcessThreadCount);
            RaisePropertyChanged(() => PeakPagedMemorySize);
            RaisePropertyChanged(() => PrivateMemorySize);
            RaisePropertyChanged(() => PeakWorkingSet);
            RaisePropertyChanged(() => ThreadSeries);
            RaisePropertyChanged(() => MemorySeries);

            Application.Current.Dispatcher.Invoke(() =>
            {
                AddThreadToChart();
                AddMemToChart();
            });
        }

        private void ResolveAndStartProcMon()
        {
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