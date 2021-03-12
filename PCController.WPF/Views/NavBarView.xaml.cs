﻿
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;

using PCController.Core.ViewModels;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for NavBarView.xaml
    /// </summary>

    public partial class NavBarView : IMvxOverridePresentationAttribute
    {
        public NavBarView()
        {
            InitializeComponent();
        }

        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            var instanceRequest = request as MvxViewModelInstanceRequest;
            var viewModel = instanceRequest?.ViewModelInstance as NavBarViewModel;
            return new MvxContentPresentationAttribute
                       {
                WindowIdentifier = $"{nameof(RootView)}.{viewModel?.ParentNo}",
                StackNavigation = false
            };
        }
    }
}
