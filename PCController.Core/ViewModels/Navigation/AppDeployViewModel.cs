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
    public sealed class AppDeployViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly IMvxLog _log;
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;

        public AppDeployViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<AppDeployViewModel>();
            _log.Info("AppDeployViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);
            _stopwatch = new Stopwatch();

            // Setup UI Commands
            CheckForAppUpdateCommand = new MvxCommand(CheckAppUpdate);
            CheckAppVersionCommand = new MvxCommand(CheckAppVersion);
            SaveSettingsCommand = new MvxCommand(SaveCDSettings);
            CheckSettingsCommand = new MvxCommand(CheckCDConnectionSettings);

            // set initial UI Fields

            // Fetch Initial Data

        }

        private void CheckCDConnectionSettings()
        {
            throw new NotImplementedException();
        }

        private void SaveCDSettings()
        {
            throw new NotImplementedException();
        }

        private void CheckAppVersion()
        {
            throw new NotImplementedException();
        }

        private void CheckAppUpdate()
        {
            throw new NotImplementedException();
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            ExeVersionManager versionManager = new ExeVersionManager();
            string version = versionManager.GetVersion();

            ApplicationVersion = version;
            ApplicationName = PCController.Core.Properties.Settings.Default.ProcessName;
            RaisePropertyChanged(() => ApplicationName);
            RaisePropertyChanged(() => ApplicationVersion);

        }

        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";


        public IMvxCommand CheckForAppUpdateCommand { get; set; }
        public IMvxCommand CheckAppVersionCommand { get; set; }
        public IMvxCommand SaveSettingsCommand { get; set; }
        public IMvxCommand CheckSettingsCommand { get; set; }
        public bool SettingHaveChanged { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public string ApplicationVersionCheckMethod { get; set; }
        public string VersionOnCDPlatform { get; set; }
        public bool AutoDownload { get; set; }
        public DateTime LastCheckedDateTime { get; set; }
        public string CDPlatform { get; set; }
        public string CDPlatformConnectionString { get; set; }
        public string CDPlatformPath { get; set; }
        public string CDPlatformUsername { get; set; }
        public string CDPlatformPassword { get; set; }
        public string CDPlatformToken { get; set; }

 


    }
}
