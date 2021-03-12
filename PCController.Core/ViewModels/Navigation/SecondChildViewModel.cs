using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace PCController.Core.ViewModels.Navigation
{
    public class SecondChildViewModel : MvxNavigationViewModel
    {
        public SecondChildViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            ShowNestedChildCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<NestedChildViewModel>());

            CloseCommand = new MvxAsyncCommand(async () => await NavigationService.Close(this));
        }

        public IMvxAsyncCommand ShowNestedChildCommand { get; private set; }

        public IMvxAsyncCommand CloseCommand { get; private set; }
    }
}
