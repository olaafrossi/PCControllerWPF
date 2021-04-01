// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 12
// by Olaaf Rossi

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Extensions.Configuration;
using MvvmCross.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.ViewModels;
using PCController.Core.Properties;
using PCController.Core.Services;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;


namespace PCController.WPF
{

    public class Setup : MvxWpfSetup
    {
        public override MvxLogProviderType GetDefaultLogProviderType()
        {
            return MvxLogProviderType.Serilog;
        }

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

            Log.Information("Created the Logger");

            return base.CreateLogProvider();
        }
    }
}