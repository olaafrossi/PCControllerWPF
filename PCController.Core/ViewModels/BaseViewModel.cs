using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace PCController.Core.ViewModels
{
    public class BaseViewModel : MvxNavigationViewModel
    {
        private readonly IMvxLog _log;

        public BaseViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<BaseViewModel>();
            _log.Info("Base VM constructed");
        }
    }
}
