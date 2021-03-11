using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MvvmCross;
using MvvmCross.Core;
using MvvmCross.Platforms.Wpf;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Views;

namespace PCController.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : MvxApplication
    {
        protected override void RegisterSetup()
        {
            this.RegisterSetupType<MvxWpfSetup<PCController.Core.App>>();
        }
    }
}
