// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 04 03
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
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
    public sealed class HomeViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly IMvxLog _log;
        private readonly Stopwatch _stopwatch;
        private int _logRefreshInterval;
        private WindowChildParam _param;
        private bool _runLogLoop = true;

        public HomeViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<HomeViewModel>();
            _log.Info("HomeViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);
            _stopwatch = new Stopwatch();

            // Setup UI Commands
            RefreshLogCommand = new MvxCommand(GetLogsFromManager);
            StartAutoLogRefreshCommand = new MvxCommand(StartLogLoop);
            StopAutoLogRefreshCommand = new MvxCommand(StopLogLoop);
            OpenScriptsFolderCommand = new MvxCommand(OpenFolder);

            // set initial UI Fields
            LogRefreshInterval = Settings.Default.AutoRefreshLogInterval;
            AutoLogRefreshTrueButtonStatus = false;
            AutoLogRefreshFalseButtonStatus = true;
            RaisePropertyChanged(() => AutoLogRefreshTrueButtonStatus);
            RaisePropertyChanged(() => AutoLogRefreshFalseButtonStatus);

            // Fetch Initial Data
            StartLogLoop();
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public IMvxCommand RefreshLogCommand { get; set; }

        public IMvxCommand OpenScriptsFolderCommand { get; set; }

        public IMvxCommand StartAutoLogRefreshCommand { get; set; }

        public IMvxCommand StopAutoLogRefreshCommand { get; set; }

        public int LogRefreshInterval
        {
            get
            {
                return _logRefreshInterval;
            }
            set
            {
                SetProperty(ref _logRefreshInterval, value);
            }
        }

        public IMvxAsyncCommand CloseCommand
        {
            get
            {
                return new MvxAsyncCommand(async () => await NavigationService.Close(this));
            }
        }

        public IList<LogModel> LogGridRows { get; set; }

        public string DataBaseQueryTime { get; set; }

        public string NumberOfLogsToFetch { get; set; }

        public bool AutoLogRefreshTrueButtonStatus { get; set; }

        public bool AutoLogRefreshFalseButtonStatus { get; set; }

        public TimeSpan CountDownToRefresh { get; set; } = new(0, 0, 0, 0);

        private void GetLogsFromManager()
        {
            _stopwatch.Start();

            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Logs));
            ProcMonitorModel procData = new();
            ComboBoxSQLParseManager parser = new();

            int numLogs = parser.GetLogs(NumberOfLogsToFetch);

            _log.Info("Getting Data Logs from {sql} number: {numOfMsgs}", sql, numLogs);

            var rows = sql.GetSomeLogs(numLogs);
            LogGridRows = rows;

            _stopwatch.Stop();

            string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDb;
            RaisePropertyChanged(() => LogGridRows);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        private void RunLoop(object state)
        {
            LogRefreshInterval = _logRefreshInterval;
            //Properties.Settings.Default.Save();

            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Logs));
            ProcMonitorModel procData = new();

            while (_runLogLoop is true)
            {
                RefreshTimer(true);
                _stopwatch.Start();

                int numLogs = 20;
                var rows = sql.GetSomeLogs(numLogs);
                LogGridRows = rows;

                _stopwatch.Stop();

                string timeToFetchFromDb = $" DB query time: {_stopwatch.ElapsedMilliseconds} ms";
                DataBaseQueryTime = timeToFetchFromDb;
                RaisePropertyChanged(() => LogGridRows);
                RaisePropertyChanged(() => DataBaseQueryTime);

                if (_logRefreshInterval is < 999)
                {
                    _log.Info("The log interval is set to {_logRefreshInterval} and that is too fast for DB calls, setting to 2 seconds", _logRefreshInterval);
                    Thread.Sleep(2000);
                }
                else if (_logRefreshInterval is > 120000)
                {
                    _log.Info("The log interval is set to {_logRefreshInterval} and that is slow for an auto-refresh function, setting to 2 seconds", _logRefreshInterval);
                    Thread.Sleep(2000);
                }
                else
                {
                    Thread.Sleep(_logRefreshInterval);
                }
            }
        }

        private void OpenFolder()
        {
            string path = Settings.Default.LocalScriptsFolder;
            Process newProcess = new();
            Process.Start("explorer.exe", path);
        }

        private void RefreshTimer(bool run)
        {
            DispatcherTimer timer = new() {Interval = TimeSpan.FromSeconds(1)};

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

                CountDownToRefresh = timer.Interval;
                RaisePropertyChanged(() => CountDownToRefresh);
            }
        }

        private void StartLogLoop()
        {
            _runLogLoop = true;
            ThreadPool.QueueUserWorkItem(RunLoop);
            _log.Info("The Auto Log Refresh Loop has started");

            // ui stuff
            AutoLogRefreshTrueButtonStatus = false;
            AutoLogRefreshFalseButtonStatus = true;
            RaisePropertyChanged(() => AutoLogRefreshTrueButtonStatus);
            RaisePropertyChanged(() => AutoLogRefreshFalseButtonStatus);
        }

        private void StopLogLoop()
        {
            _runLogLoop = false;
            RefreshTimer(false);
            _log.Info("The Auto Log Refresh Loop been stopped");

            // ui stuff
            AutoLogRefreshTrueButtonStatus = true;
            AutoLogRefreshFalseButtonStatus = false;
            RaisePropertyChanged(() => AutoLogRefreshTrueButtonStatus);
            RaisePropertyChanged(() => AutoLogRefreshFalseButtonStatus);
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            TimeSpan timeTick = new(0, 0, 0, 1);
            CountDownToRefresh = CountDownToRefresh.Add(timeTick);
            RaisePropertyChanged(() => CountDownToRefresh);
        }
    }
}