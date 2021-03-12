using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace PCController.Core.ViewModels
{
    public class BaseViewModel : MvxNavigationViewModel
    {
        public BaseViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }
    }
}
