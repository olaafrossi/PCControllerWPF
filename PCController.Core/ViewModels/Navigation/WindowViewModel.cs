// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 12
// by Olaaf Rossi

using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Models;
using PCController.Core.Services;

// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public class WindowChildParam
    {
        public int ParentNo { get; set; }
        public int ChildNo { get; set; }
    }

    public sealed class WindowViewModel : MvxNavigationViewModel
    {
        private static int _count;
        private bool _isItem1 = true;
        private bool _isItem2;
        private bool _isItem3;
        private bool _isItemSetting = true;
        private Modes _mode = Modes.Blue;
        private ObservableCollection<string> _realTimeLogCollection = new();
        private readonly IMvxLog _log;


        private void Sinky_LogString(object sender, string e)
        {
            RealTimeLogCollection.Insert(0, e);
        }


        public WindowViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<WindowViewModel>();

            // get singleton and create event handler
            CollectionSink sink = Mvx.IoCProvider.Resolve<CollectionSink>();
            sink.LogString += Sinky_LogString;

            _count++;
            Count = _count;

            _log.Info("WindowViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);
            RealTimeLogCollection.Insert(0, "Hello World");
            RealTimeLogCollection.Insert(0, "Hi");




            //ShowWindowChildCommand = new MvxAsyncCommand<int>(async no =>
            //{
            //    await NavigationService.Navigate<WindowChildViewModel, WindowChildParam>(new WindowChildParam {ParentNo = Count, ChildNo = no});
            //});

            //ShowNavBarCommand = new MvxAsyncCommand<int>(async no =>
            //{
            //    await NavigationService.Navigate<NavBarViewModel, WindowChildParam>(new WindowChildParam {ParentNo = Count, ChildNo = no});
            //});



            ShowHomeCommand = new MvxAsyncCommand<int>(async no =>
                {
                    await NavigationService.Navigate<HomeViewModel, WindowChildParam>(new WindowChildParam { ParentNo = Count, ChildNo = no });
                });


            ShowPCControllerInfoCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<PCControllerInfoViewModel, WindowChildParam>(new WindowChildParam { ParentNo = Count, ChildNo = no });
            });

            ShowWatchdogCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<WatchdogViewModel, WindowChildParam>(new WindowChildParam { ParentNo = Count, ChildNo = no });
            });

            ShowPCNetworkListenerCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<PCNetworkListenerViewModel, WindowChildParam>(new WindowChildParam { ParentNo = Count, ChildNo = no });
            });

            ShowSCSUDPTesterCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<SCSTesterUDPViewModel, WindowChildParam>(new WindowChildParam { ParentNo = Count, ChildNo = no });
            });

            ShowAppDeployerCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<AppDeployViewModel, WindowChildParam>(new WindowChildParam { ParentNo = Count, ChildNo = no });
            });



            


            CloseCommand = new MvxAsyncCommand(async () => await NavigationService.Close(this));

            ToggleSettingCommand = new MvxAsyncCommand(async () =>
            {
                await Task.Run(() =>
                {
                    IsItemSetting = !IsItemSetting;
                });
            });


            //Thread.Sleep(50);
            //OpenHomeView();

        }

        private void OpenHomeView()
        {
            ShowHomeCommand.Execute(2);
        }


        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<int> ShowWindowChildCommand { get; }

        public IMvxAsyncCommand<int> ShowNavBarCommand { get; set; }

        public IMvxAsyncCommand<int> ShowHomeCommand { get; set; }

        public IMvxAsyncCommand<int> ShowAppDeployerCommand { get; set; }

        public IMvxAsyncCommand<int> ShowPCControllerInfoCommand { get; set; }

        public IMvxAsyncCommand<int> ShowPCNetworkListenerCommand { get; set; }

        public IMvxAsyncCommand<int> ShowWatchdogCommand { get; set; }

        public IMvxAsyncCommand<int> ShowSCSUDPTesterCommand { get; set; }

        public IMvxAsyncCommand ToggleSettingCommand { get; }

        public IMvxCommand OpenUdpCommand { get; set; }

        public IMvxCommand CloseUdpCommand { get; set; }

        public string Title
        {
            get
            {
                return $"No.{Count} Window View";
            }
        }

        public Modes Mode
        {
            get { return _mode; }
            set
            {
                if (value == _mode) return;
                _mode = value;
                RaisePropertyChanged(() => Mode);
            }
        }

        public bool IsItem1
        {
            get { return _isItem1; }
            set
            {
                if (value == _isItem1) return;
                _isItem1 = value;
                RaisePropertyChanged(() => IsItem1);
            }
        }

        public bool IsItem2
        {
            get { return _isItem2; }
            set
            {
                if (value == _isItem2) return;
                _isItem2 = value;
                RaisePropertyChanged(() => IsItem2);
            }
        }

        public bool IsItem3
        {
            get { return _isItem3; }
            set
            {
                if (value == _isItem3) return;
                _isItem3 = value;
                RaisePropertyChanged(() => IsItem3);
            }
        }

        public bool IsItemSetting
        {
            get { return _isItemSetting; }
            set
            {
                if (value == _isItemSetting) return;
                _isItemSetting = value;
                RaisePropertyChanged(() => IsItemSetting);
            }
        }

        public int Count { get; set; }

        public ObservableCollection<string> RealTimeLogCollection
        {
            get { return _realTimeLogCollection; }
            set
            {
                SetProperty(ref _realTimeLogCollection, value);
            }
        }
    }
}