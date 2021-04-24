using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using PCController.Core.ViewModels;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for WindowView.xaml
    /// </summary>
    public partial class WindowView : IMvxOverridePresentationAttribute
    {
        public WindowView()
        {
            InitializeComponent();
        }

        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            MvxViewModelInstanceRequest instanceRequest = request as MvxViewModelInstanceRequest;
            WindowViewModel viewModel = instanceRequest?.ViewModelInstance as WindowViewModel;

            return new MvxWindowPresentationAttribute
            {
                Identifier = $"{nameof(WindowView)}.{viewModel}"
            };
        }
    }
}
