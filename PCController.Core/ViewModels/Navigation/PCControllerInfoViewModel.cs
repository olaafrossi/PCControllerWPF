using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using System;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Serilog;

using PCController.DataAccess;
using PCController.DataAccess.Models;

namespace PCController.Core.ViewModels
{
    public class PCControllerInfoViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private WindowChildParam _param;
        public override void Prepare(WindowChildParam param) => _param = param;

        private readonly Stopwatch stopwatch;

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public IMvxAsyncCommand CloseCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this));

        public PCControllerInfoViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            this.GetAppInfo();
            this.GetDataLogs();
        }

        private void GetDataLogs()
        {
            //this.stopwatch.Restart();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Logs));
            string logComboBoxSelection = string.Empty;

            // get the number of logs from the user
            //this.Dispatcher.Invoke(() => { return logComboBoxSelection = this.LogSelectComboBox.SelectionBoxItem.ToString(); });
            int numOfLogs = 20;
            try
            {
                //numOfLogs = int.Parse(logComboBoxSelection);
                //Log.Logger.Information("Getting Data Logs{numOfLogs}", numOfLogs);
            }
            catch (Exception e)
            {
                //Log.Logger.Error("Didn't parse the number in the Log ComboBox (this is impossible) {numOfLogs}", numOfLogs);
            }

            var rows = sql.GetSomeLogs(20);

            LogGridRows = rows;

            // insert the rows into the LogGrid
            //this.Dispatcher.Invoke(() => { this.LogGrid.ItemsSource = rows; }, DispatcherPriority.DataBind);

            //_ = this.Dispatcher.BeginInvoke(() => { this.LoadTimeTextBlock.Text = $" DB query time: {this.stopwatch.ElapsedMilliseconds} ms"; }, DispatcherPriority.DataBind);
            //Log.Logger.Information("Inserted DB rows into the LogGrid in {this.stopwatch.ElapsedMilliseconds}", this.stopwatch.ElapsedMilliseconds);
            //this.stopwatch.Stop();
        }

        public IList<LogModel> LogGridRows { get; set; }











































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
    }
}
