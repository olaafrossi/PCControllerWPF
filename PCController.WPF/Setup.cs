// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 12
// by Olaaf Rossi

using MvvmCross.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.ViewModels;
using PCController.Core.Properties;
using Serilog;

namespace PCController.WPF
{
    public class Setup : MvxWpfSetup
    {
        public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.Serilog;

        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
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
                .WriteTo.SQLite(Settings.Default.SQLiteDBPath)
                .WriteTo.File(Settings.Default.LocalLogFolderFile)
                .WriteTo.Console()
                .CreateLogger();

            return base.CreateLogProvider();
        }
    }
}