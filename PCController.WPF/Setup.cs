namespace PCController.WPF
{
    using System;
    using System.IO;
    using MvvmCross.Logging;
    using MvvmCross.Platforms.Wpf.Core;
    using MvvmCross.ViewModels;
    using Serilog;

    public class Setup : MvxWpfSetup
    {
        public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.Serilog;

        protected override IMvxApplication CreateApp()
        {
            throw new NotImplementedException();
        }

        protected override IMvxLogProvider CreateLogProvider()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{Directory.GetCurrentDirectory()}\\my_log.log")
                .WriteTo.Console()
                .CreateLogger();
            return base.CreateLogProvider();
        }
    }
}
