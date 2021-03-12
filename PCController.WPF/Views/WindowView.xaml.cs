using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
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
            var instanceRequest = request as MvxViewModelInstanceRequest;
            var viewModel = instanceRequest?.ViewModelInstance as NavBarViewModel;

            return new MvxWindowPresentationAttribute
                       {
                           Identifier = $"{nameof(WindowView)}.{viewModel}"
                       };
        }
    }
}
