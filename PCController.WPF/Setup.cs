using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MvvmCross.Core;

using Serilog;
using Serilog.Sinks.SystemConsole;
using MvvmCross.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.ViewModels;

namespace PCController.WPF
{
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
