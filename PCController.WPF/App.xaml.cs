using MvvmCross.Core;
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
            //MvxWpfSetupSingleton.EnsureSingletonAvailable(Dispatcher, MainWindow);
            this.RegisterSetupType<MvxWpfSetup<Core.App>>();

        }
    }
}
