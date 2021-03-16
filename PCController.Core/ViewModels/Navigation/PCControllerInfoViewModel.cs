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

namespace PCController.Core.ViewModels
{
    public class PCControllerInfoViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;

        private WindowChildParam _param;

        public PCControllerInfoViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            RefreshLogCommand = new MvxCommand(GetDataLogs);

            Serilog.Log.Logger.Information("PCControllerViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);

            GetAppInfo();
            stopwatch = new Stopwatch();
            GetDataLogs();
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

        private void GetDataLogs()
        {
            stopwatch.Start();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Logs));
            int numOfLogs = 20;

            try
            {
                RaisePropertyChanged(() => NumberOfLogsToFetch);
                if (NumberOfLogsToFetch is null)
                {
                    numOfLogs = 20;
                }
                else if (NumberOfLogsToFetch.Contains("All"))
                {
                    // All
                    numOfLogs = 100000000;
                }
                else if (NumberOfLogsToFetch.Length == 40)
                {
                    // 20, 50
                    string logComboBoxSelected = NumberOfLogsToFetch.Substring(38, 2);
                    numOfLogs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfLogsToFetch.Length == 41)
                {
                    // hundred
                    string logComboBoxSelected = NumberOfLogsToFetch.Substring(38, 3);
                    numOfLogs = int.Parse(logComboBoxSelected);
                }
                else if (NumberOfLogsToFetch.Length == 42)
                {
                    // thousand
                    string logComboBoxSelected = NumberOfLogsToFetch.Substring(38, 4);
                    numOfLogs = int.Parse(logComboBoxSelected);
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("Didn't parse the number in the Log ComboBox {numOfLogs}", numOfLogs, e);
            }

            Serilog.Log.Logger.Information("Getting Data Logs{numOfLogs}", numOfLogs);
            var rows = sql.GetSomeLogs(numOfLogs);
            LogGridRows = rows;

            stopwatch.Stop();
            string timeToFetchFromDB = $" DB query time: {stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDB;
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