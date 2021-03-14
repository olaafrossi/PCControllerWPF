using System.Globalization;

namespace PCController.WPF
{
    using MvvmCross.Logging;
    using MvvmCross.Platforms.Wpf.Core;
    using MvvmCross.ViewModels;
    using Serilog;
    using System.IO;

    public class Setup : MvxWpfSetup
    {
        public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.Serilog;

        protected override IMvxApplication CreateApp()
        {
            return (IMvxApplication)new Core.App();
        }

        protected override IMvxLogProvider CreateLogProvider()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithAssemblyVersion()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProcessId()
                .WriteTo.SQLite(PCController.Core.Properties.Settings.Default.SQLiteDBPath)
                .WriteTo.File(PCController.Core.Properties.Settings.Default.LocalLogFolderFile)
                .WriteTo.Console()
                .CreateLogger();

            return base.CreateLogProvider();
        }
    }
}
