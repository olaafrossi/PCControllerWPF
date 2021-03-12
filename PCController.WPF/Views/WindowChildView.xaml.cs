using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;

using PCController.Core.ViewModels;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for WindowChildView.xaml
    /// </summary>
    public partial class WindowChildView : IMvxOverridePresentationAttribute
    {
        public WindowChildView()
        {
            InitializeComponent();
        }

        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            var instanceRequest = request as MvxViewModelInstanceRequest;
            var viewModel = instanceRequest?.ViewModelInstance as WindowChildViewModel;

            return new MvxContentPresentationAttribute
                       {
                           WindowIdentifier = $"{nameof(WindowView)}.{viewModel?.ParentNo}",
                           StackNavigation = false
                       };
        }
    }
}
