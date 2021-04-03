// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 13
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.Diagnostics;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public sealed class PCControllerInfoViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch _stopwatch;

        private WindowChildParam _param;

        private readonly IMvxLog _log;

        public PCControllerInfoViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<PCControllerInfoViewModel>();
            RefreshLogCommand = new MvxCommand(GetLogsFromManager);

            _log.Info("PCControllerViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            GetAppInfo();
            _stopwatch = new Stopwatch();
            GetLogsFromManager();
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public IMvxCommand RefreshLogCommand { get; set; }

        public IMvxAsyncCommand CloseCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this));

        public IList<LogModel> LogGridRows { get; set; }

        public string DataBaseQueryTime { get; set; }

        public string NumberOfLogsToFetch { get; set; }

        public string AssemblyFileVersion { get; set; }

        public string AssemblyInformationVersion { get; set; }

        public string AssemblyVersion { get; set; }

        public string DotNetInfo { get; set; }

        public string InstallLocation { get; set; }

        public string PackageVersion { get; set; }

        public string AppInstallerUri { get; set; }

        public string PackageChannel { get; set; }

        public string DisplayName { get; set; }

        public string MSIXVersionNumber { get; set; }

        public Uri AppInfoInstallerUri { get; set; }
        public override void Prepare(WindowChildParam param) => _param = param;

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

        private void GetAppInfo()
        {
            AppInfoManager appInfo = new AppInfoManager();
            AssemblyFileVersion = appInfo.GetAssemblyFileVersion();
            AssemblyInformationVersion = appInfo.GetAssemblyInformationVersion();
            AssemblyVersion = appInfo.GetAssemblyVersion();
            DotNetInfo = appInfo.GetDotNetInfo();
            InstallLocation = appInfo.GetInstallLocation();
            PackageVersion = appInfo.GetPackageVersion();
            AppInstallerUri = appInfo.GetAppInstallerUri();
            PackageChannel = appInfo.GetPackageChannel();
            DisplayName = appInfo.GetDisplayName();
            MSIXVersionNumber = appInfo.GetMsixPackageVersion().ToString();
            //AppInfoInstallerUri = appInfo.GetAppInstallerInfoUri()
        }
    }
}