﻿using System.Diagnostics;
using System.Windows.Navigation;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using PCController.Core.ViewModels;
using Serilog;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for PCNetworkListenerView.xaml
    /// </summary>
    public partial class PCNetworkListenerView : IMvxOverridePresentationAttribute
    {
        public PCNetworkListenerView()
        {
            InitializeComponent();
        }
        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            var instanceRequest = request as MvxViewModelInstanceRequest;
            var viewModel = instanceRequest?.ViewModelInstance as PCNetworkListenerViewModel;
            return new MvxContentPresentationAttribute
            {
                WindowIdentifier = $"{nameof(RootView)}.{viewModel?.ParentNo}",
                StackNavigation = false
            };
        }
    }
}
