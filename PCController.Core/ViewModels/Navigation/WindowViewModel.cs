﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Models;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.RichTextBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using PCController.Core.ViewModels.Navigation;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.RichTextBox;

namespace PCController.Core.ViewModels
{
    public class WindowChildParam
    {
        public int ParentNo { get; set; }
        public int ChildNo { get; set; }
    }

    public class WindowViewModel : MvxNavigationViewModel
    {
        private static int _count;

        public string Title
        {
            get
            {
                return $"No.{Count} Window View";
            }
        }

        private Modes _mode = Modes.Blue;
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

        private bool _isItem1 = true;
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

        private bool _isItem2 = false;
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

        private bool _isItem3 = false;
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

        private bool _isItemSetting = true;
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

        public WindowViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {

            _count++;
            Count = _count;
            
            Serilog.Log.Information("Hello, world! from serilog");
            Serilog.Log.Logger.Information("hello");


            


            ShowWindowChildCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<WindowChildViewModel, WindowChildParam>(new WindowChildParam
                {
                    ParentNo = Count,
                    ChildNo = no
                });
            });

            ShowNavBarCommand = new MvxAsyncCommand<int>(async no =>
                {
                    await NavigationService.Navigate<NavBarViewModel, WindowChildParam>(new WindowChildParam
                    {
                        ParentNo = Count,
                        ChildNo = no
                    });
                });

            ShowPCControllerInfoCommand = new MvxAsyncCommand<int>(async no =>
                {
                    await NavigationService.Navigate<PCControllerInfoViewModel, WindowChildParam>(new WindowChildParam
                    {
                        ParentNo = Count,
                        ChildNo = no
                    });
                });

            ShowWatchdogCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<WatchdogViewModel, WindowChildParam>(new WindowChildParam
                {
                    ParentNo = Count,
                    ChildNo = no
                });
            });

            ShowPCNetworkListenerCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<PCNetworkListenerViewModel, WindowChildParam>(new WindowChildParam
                {
                    ParentNo = Count,
                    ChildNo = no
                });
            });

            ShowSCSUDPTesterCommand = new MvxAsyncCommand<int>(async no =>
            {
                await NavigationService.Navigate<SCSTesterUDPViewModel, WindowChildParam>(new WindowChildParam
                {
                    ParentNo = Count,
                    ChildNo = no
                });
            });


            CloseCommand = new MvxAsyncCommand(async () => await NavigationService.Close(this));

            ToggleSettingCommand = new MvxAsyncCommand(async () =>
            {
                await Task.Run(() =>
                {
                    IsItemSetting = !IsItemSetting;
                });
            });
        }


        public IMvxAsyncCommand CloseCommand { get; private set; }
        public IMvxAsyncCommand<int> ShowWindowChildCommand { get; private set; }

        public IMvxAsyncCommand<int> ShowNavBarCommand { get; set; }

        public IMvxAsyncCommand<int> ShowPCControllerInfoCommand { get; set; }

        public IMvxAsyncCommand<int> ShowPCNetworkListenerCommand { get; set; }

        public IMvxAsyncCommand<int> ShowWatchdogCommand { get; set; }

        public IMvxAsyncCommand<int> ShowSCSUDPTesterCommand { get; set; }

        public IMvxAsyncCommand ToggleSettingCommand { get; private set; }

        public IMvxCommand OpenUdpCommand { get; set; }

        public IMvxCommand CloseUdpCommand { get; set; }


    }
}
