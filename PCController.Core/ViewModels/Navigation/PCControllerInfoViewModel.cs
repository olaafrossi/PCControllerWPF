using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using System;

namespace PCController.Core.ViewModels
{
    public class PCControllerInfoViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private WindowChildParam _param;
        public override void Prepare(WindowChildParam param) => _param = param;

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public IMvxAsyncCommand CloseCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this));

        public PCControllerInfoViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            this.GetAppInfo();
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
