using System.Diagnostics;
using System.Windows.Navigation;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using PCController.Core.ViewModels;
using PCController.Core.ViewModels;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for SCSTesterUDPView.xaml
    /// </summary>
    public partial class SCSTesterUDPView : IMvxOverridePresentationAttribute
    {
        public SCSTesterUDPView()
        {
            InitializeComponent();
        }

        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            MvxViewModelInstanceRequest instanceRequest = request as MvxViewModelInstanceRequest;
            SCSTesterUDPViewModel viewModel = instanceRequest?.ViewModelInstance as SCSTesterUDPViewModel;
            return new MvxContentPresentationAttribute
            {
                WindowIdentifier = $"{nameof(RootView)}.{viewModel?.ParentNo}",
                StackNavigation = false
            };
        }
    }
}
