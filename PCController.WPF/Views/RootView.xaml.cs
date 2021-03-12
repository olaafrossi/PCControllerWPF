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

using Serilog;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for RootView.xaml
    /// </summary>

    public partial class RootView
    {
        public RootView()
        {
            InitializeComponent();  
            Log.Logger.Information("hello from Serilog");
            Console.WriteLine("hello from console");
        }
    }
}
