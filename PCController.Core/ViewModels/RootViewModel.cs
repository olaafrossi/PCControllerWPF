// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 12
// by Olaaf Rossi

using System;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;

namespace PCController.Core.ViewModels
{
    public class RootViewModel : MvxNavigationViewModel
    {
        private readonly IMvxLog _log;
        private readonly IMvxViewModelLoader _mvxViewModelLoader;

        private int _counter = 2;

        private bool _isVisible;

        private string _welcomeText = "Default welcome";

        public RootViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService, IMvxViewModelLoader mvxViewModelLoader) : base(logProvider, navigationService)
        {
            _mvxViewModelLoader = mvxViewModelLoader;
            _log = logProvider.GetLogFor<RootViewModel>();
            try
            {
                IMvxMessenger messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
                string? str = messenger.ToString();
                _log.InfoFormat("Creating the Root View Model via the MVX IOC provider {messenger}", messenger);
            }
            catch (Exception e)
            {
                _log.ErrorException("could not resolve the Messenger", e);
            }

            //ShowChildCommand = new MvxAsyncCommand(async () =>
            //{
            //    var result = await NavigationService.Navigate<ChildViewModel, SampleModel, SampleModel>(new SampleModel
            //    {
            //        Message = "Hey",
            //        Value = 1.23m
            //    });
            //    var testIfReturn = result;
            //});

            //ShowModalCommand = new MvxAsyncCommand(Navigate);

            //ShowModalNavCommand =
            //    new MvxAsyncCommand(async () => await NavigationService.Navigate<ModalNavViewModel>());

            //ShowTabsCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<TabsRootViewModel>());

            //ShowPagesCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<PagesRootViewModel>());

            //ShowSplitCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<SplitRootViewModel>());

            //ShowNativeCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<NativeViewModel>());

            //ShowOverrideAttributeCommand = new MvxAsyncCommand(async () =>
            //    await NavigationService.Navigate<OverrideAttributeViewModel>());

            //ShowSheetCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<SheetViewModel>());

            ShowWindowCommand = new MvxAsyncCommand(async () => await NavigationService.Navigate<WindowViewModel>());

            //ShowMixedNavigationCommand =
            //    new MvxAsyncCommand(async () => await NavigationService.Navigate<MixedNavFirstViewModel>());

            //ShowDictionaryBindingCommand = new MvxAsyncCommand(async () =>
            //    await NavigationService.Navigate<DictionaryBindingViewModel>());

            //ShowCollectionViewCommand =
            //    new MvxAsyncCommand(async () => await NavigationService.Navigate<CollectionViewModel>());

            //ShowSharedElementsCommand = new MvxAsyncCommand(async () =>
            //    await NavigationService.Navigate<SharedElementRootChildViewModel>());

            //ShowCustomBindingCommand =
            //    new MvxAsyncCommand(async () => await NavigationService.Navigate<CustomBindingViewModel>());

            //ShowFluentBindingCommand =
            //    new MvxAsyncCommand(async () => await NavigationService.Navigate<FluentBindingViewModel>());

            //RegisterAndResolveWithReflectionCommand = new MvxAsyncCommand(RegisterAndResolveWithReflection);
            //RegisterAndResolveWithNoReflectionCommand = new MvxAsyncCommand(RegisterAndResolveWithNoReflection);

            _counter = 3;

            TriggerVisibilityCommand =
                new MvxCommand(() => IsVisible = !IsVisible);

            //FragmentCloseCommand = new MvxAsyncCommand(() => NavigationService.Navigate<FragmentCloseViewModel>());

            //ShowLocationCommand = new MvxAsyncCommand(() => NavigationService.Navigate<LocationViewModel>());
        }

        public IMvxLanguageBinder TextSource
        {
            get { return new MvxLanguageBinder("Playground.Core", "Text"); }
        }

        public MvxNotifyTask MyTask { get; set; }

        public IMvxAsyncCommand ShowChildCommand { get; }

        public IMvxAsyncCommand ShowModalCommand { get; }

        public IMvxAsyncCommand ShowModalNavCommand { get; }

        public IMvxAsyncCommand ShowCustomBindingCommand { get; }

        public IMvxAsyncCommand ShowTabsCommand { get; }

        public IMvxAsyncCommand ShowPagesCommand { get; }

        public IMvxAsyncCommand ShowSplitCommand { get; }

        public IMvxAsyncCommand ShowOverrideAttributeCommand { get; }

        public IMvxAsyncCommand ShowNativeCommand { get; }

        public IMvxAsyncCommand ShowSheetCommand { get; }

        public IMvxAsyncCommand ShowWindowCommand { get; }

        public IMvxAsyncCommand ShowMixedNavigationCommand { get; }

        public IMvxAsyncCommand ShowDictionaryBindingCommand { get; }

        public IMvxAsyncCommand ShowCollectionViewCommand { get; }

        //public IMvxAsyncCommand ShowListViewCommand =>
        //    new MvxAsyncCommand(async () => await NavigationService.Navigate<ListViewModel>());

        public IMvxAsyncCommand ShowBindingsViewCommand =>
            new MvxAsyncCommand(async () => await NavigationService.Navigate<BindingsViewModel>());

        //public IMvxAsyncCommand ShowCodeBehindViewCommand =>
        //    new MvxAsyncCommand(async () => await NavigationService.Navigate<CodeBehindViewModel>());

        //public IMvxAsyncCommand ShowNavigationCloseCommand =>
        //    new MvxAsyncCommand(async () => await NavigationService.Navigate<NavigationCloseViewModel>());

        //public IMvxAsyncCommand ShowContentViewCommand =>
        //    new MvxAsyncCommand(async () => await NavigationService.Navigate<ParentContentViewModel>());

        //public IMvxAsyncCommand ConvertersCommand =>
        //    new MvxAsyncCommand(async () => await NavigationService.Navigate<ConvertersViewModel>());

        public IMvxAsyncCommand ShowSharedElementsCommand { get; }

        public IMvxAsyncCommand ShowFluentBindingCommand { get; }

        //public IMvxAsyncCommand RegisterAndResolveWithReflectionCommand { get; }

        //public IMvxAsyncCommand RegisterAndResolveWithNoReflectionCommand { get; }

        public IMvxCommand TriggerVisibilityCommand { get; }

        public IMvxCommand FragmentCloseCommand { get; }
        public IMvxAsyncCommand ShowLocationCommand { get; }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public string WelcomeText
        {
            get => _welcomeText;
            set
            {
                ShouldLogInpc(true);
                SetProperty(ref _welcomeText, value);
                ShouldLogInpc(false);
            }
        }

        public string TimeToRegister { get; set; }

        public string TimeToResolve { get; set; }

        public string TotalTime { get; set; }

        //public override async Task Initialize()
        //{
        //    Log.Warn(() => "Testing log");

        //    await base.Initialize();

        //    // Uncomment this to demonstrate use of StartAsync for async first navigation
        //    // await Task.Delay(5000);

        //    _mvxViewModelLoader.LoadViewModel(MvxViewModelRequest.GetDefaultRequest(typeof(ChildViewModel)),
        //        new SampleModel
        //        {
        //            Message = "From locator",
        //            Value = 2
        //        },
        //        null);
        //}

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            MyTask = MvxNotifyTask.Create(
                async () =>
                {
                    await Task.Delay(300);

                    WelcomeText = "Welcome to MvvmCross!";

                    throw new Exception("Boom!");
                }, exception => { });
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);

            _counter = int.Parse(state.Data["MyKey"]);
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);

            bundle.Data["MyKey"] = _counter.ToString();
        }

        //private async Task Navigate()
        //{
        //    try
        //    {
        //        await NavigationService.Navigate<ModalViewModel>();
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //private async Task RegisterAndResolveWithReflection()
        //{
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    Mvx.IoCProvider.RegisterTypesWithReflection();
        //    var registered = stopwatch.ElapsedTicks;
        //    for (int i = 0; i < 20; i++)
        //    {
        //        Mvx.IoCProvider.ResolveTypes();
        //    }
        //    stopwatch.Stop();
        //    var total = stopwatch.ElapsedTicks;
        //    var resolved = total - registered;

        //    TimeToRegister = $"Time to register using reflection - {registered}";
        //    TimeToResolve = $"Time to resolve using reflection - {resolved}";
        //    TotalTime = $"Total time using reflection - {total}";
        //    await RaiseAllPropertiesChanged();
        //}

        //private async Task RegisterAndResolveWithNoReflection()
        //{
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    Mvx.IoCProvider.RegisterTypesWithNoReflection();
        //    var registered = stopwatch.ElapsedTicks;
        //    for (int i = 0; i < 20; i++)
        //    {
        //        Mvx.IoCProvider.ResolveTypes();
        //    }
        //    stopwatch.Stop();
        //    var total = stopwatch.ElapsedTicks;
        //    var resolved = total - registered;

        //    TimeToRegister = $"Time to register - NO reflection - {registered}";
        //    TimeToResolve = $"Time to resolve - NO reflection - {resolved}";
        //    TotalTime = $"Total time - NO reflection - {total}";
        //    await RaiseAllPropertiesChanged();
        //}
    }
}