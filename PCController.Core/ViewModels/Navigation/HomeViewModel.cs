// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 13
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
    public sealed class HomeViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;
        private readonly IMvxLog _log;

        public HomeViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<HomeViewModel>();
            RefreshLogCommand = new MvxCommand(GetLogsFromManager);

            _log.Info("HomeViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            _stopwatch = new Stopwatch();
            GetLogsFromManager();
        }
        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }


        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public IMvxCommand RefreshLogCommand { get; set; }

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

        public void GetLogsFromManager()
        {
            _stopwatch.Start();

            SQLiteCRUD sql = new(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Logs));
            ProcMonitorModel procData = new();
            ComboBoxSQLParseManager parser = new ComboBoxSQLParseManager();

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
    }
}