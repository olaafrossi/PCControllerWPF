using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Localization;
using MvvmCross.ViewModels;
using PCController.Core.Services;
using PCController.Core.ViewModels;
using Serilog;
using Serilog.Core;
using ThreeByteLibrary.Dotnet;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Core
{
    public class App : MvxApplication
    {
        /// <summary>
        /// Breaking change in v6: This method is called on a background thread. Use
        /// Startup for any UI bound actions
        /// </summary>
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            Mvx.IoCProvider.RegisterSingleton<IMvxTextProvider>(new TextProviderBuilder().TextProvider);

            Mvx.IoCProvider.RegisterSingleton<CollectionSink>(new CollectionSink());

            // setup info for the 3Byte network listener
            int portNum = Properties.Settings.Default.PCListenerUDPPort;
            ILogger netLogger = Log.Logger;
            Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.NetworkUtils.IPcNetworkListener>(new PcNetworkListener(netLogger, portNum));

            // setup info for the 3Byte watchdog/process monitor
            string processName = Properties.Settings.Default.ProcessName;
            string exeString = Properties.Settings.Default.ExecutionString;
            Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.IProcessMonitor>(new ProcessMonitor(processName, exeString));

            // setup info for the 3Byte AsyncUDP Link for SCS Testing

            //string iPAddress = Properties.Settings.Default.AsyncUdpIPAddress;
            //int remotePort = Properties.Settings.Default.AsyncUdpRemotePort;
            //int localPort = Properties.Settings.Default.AsyncUdpLocalPort;
            //Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.NetworkUtils.IAsyncUdpLink>(new AsyncUdpLink(iPAddress, remotePort, localPort));

            // start the app
            RegisterAppStart<RootViewModel>();
        }
        
        //public event EventHandler<NetworkMessagesEventArgs> MessageHit;

        /// <summary>
        /// Do any UI bound startup actions here
        /// </summary>
        public override Task Startup()
        {
            return base.Startup();
        }

        /// <summary>
        /// If the application is restarted (eg primary activity on Android
        /// can be restarted) this method will be called before Startup
        /// is called again
        /// </summary>
        public override void Reset()
        {
            base.Reset();
        }
    }
}
