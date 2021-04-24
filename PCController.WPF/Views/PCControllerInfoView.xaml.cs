using System.Diagnostics;
using System.Windows.Navigation;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using Serilog;
// TODO nothing should be in this view, and remove serilog

using PCController.Core.ViewModels;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for PCControllerView.xaml
    /// </summary>
    public partial class PCControllerInfoView : IMvxOverridePresentationAttribute
    {
        public PCControllerInfoView()
        {
            InitializeComponent();
        }
        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            MvxViewModelInstanceRequest instanceRequest = request as MvxViewModelInstanceRequest;
            PCControllerInfoViewModel viewModel = instanceRequest?.ViewModelInstance as PCControllerInfoViewModel;
            return new MvxContentPresentationAttribute
            {
                WindowIdentifier = $"{nameof(RootView)}.{viewModel?.ParentNo}",
                StackNavigation = false
            };
        }

        private void LinkTo3ByteOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Log.Logger.Information("User clicked {sender} {e}", sender, e);
            string destURL = "https://www.3-byte.com.com/";
            ProcessStartInfo sInfo = new ProcessStartInfo(destURL) { UseShellExecute = true };
            Process.Start(sInfo);
        }

        private void LinkToGitHubProjectOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Log.Logger.Information("User clicked {sender} {e}", sender, e);
            string destURL = "https://www.google.com/";
            ProcessStartInfo sInfo = new ProcessStartInfo(destURL) { UseShellExecute = true };
            Process.Start(sInfo);
        }

        private void LinkToProjectInstallerOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Log.Logger.Information("User clicked {sender} {e}", sender, e);
            string destURL = "https://www.github.com/";
            ProcessStartInfo sInfo = new ProcessStartInfo(destURL) { UseShellExecute = true };
            Process.Start(sInfo);
        }
    }
}
