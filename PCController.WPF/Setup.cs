// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 12
// by Olaaf Rossi

using System;
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
            Action<string> handler = null;
            var outputTemplate = "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{Caller}]{NewLine}{Exception}{Message}{NewLine}";
            Serilog.Formatting.Display.MessageTemplateTextFormatter tf = new Serilog.Formatting.Display.MessageTemplateTextFormatter(outputTemplate, CultureInfo.InvariantCulture);

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithAssemblyVersion()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProcessId()
                .WriteTo.SQLite(Settings.Default.SQLiteDBPath)
                .WriteTo.File(Settings.Default.LocalLogFolderFile)
                .WriteTo.Console();

            if (handler != null)
            {
                loggerConfig.WriteTo.LoggerDelegateSink(tf, handler);
            }
            
            Log.Logger = loggerConfig.CreateLogger();

            return base.CreateLogProvider();
        }

    }
    public class LoggerDelegateSink : ILogEventSink
    {
        private readonly ITextFormatter _formatter;
        private readonly Action<string> _handler;

        public LoggerDelegateSink(ITextFormatter formatter, Action<string> handler)
        {
            _formatter = formatter ?? throw new ArgumentNullException("formatter");
            _handler = handler ?? throw new ArgumentNullException("handlers");
        }

        public void Emit(LogEvent logEvent)
        {
            var buffer = new StringWriter(new StringBuilder(256));
            _formatter.Format(logEvent, buffer);
            string message = buffer.ToString();
            _handler(message);
        }
    }

    public static class LoggerDelegateSinkExtension
    {
        public static LoggerConfiguration LoggerDelegateSink(this LoggerSinkConfiguration loggerConfig, ITextFormatter formatter, Action<string> handler)
        {
            return loggerConfig.Sink(new LoggerDelegateSink(formatter, handler));
        }
    }
}