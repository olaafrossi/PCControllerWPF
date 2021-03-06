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
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MvvmCross.Logging;
using MvvmCross.Logging.LogProviders;
using PCController.Core.Managers;
using PCController.Core.Properties;
using Syncfusion;


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
            // Licensing key for chart elements
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDUyMjM0QDMxMzkyZTMxMmUzMFFwc0pqdHQ2cVFmY1Y4NGJaK1NpYzFRV2xlMWcwMktFRHpiL1JkV29vTlU9");

            // is this needed just yet?
            CreatableTypes().EndingWith("Service").AsInterfaces().RegisterAsLazySingleton();

            // will i need this?
            Mvx.IoCProvider.RegisterSingleton<IMvxTextProvider>(new TextProviderBuilder().TextProvider);

            // will i need this?
            Mvx.IoCProvider.RegisterSingleton<CollectionSink>(new CollectionSink());

            //TODO move these settings to SQLite
            // setup info for the 3Byte network listener
            int portNum = Properties.Settings.Default.PCListenerUDPPort;
            ILogger logger = Log.Logger;
            Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.NetworkUtils.IPcNetworkListener>(new PcNetworkListener(logger, portNum));

            // setup info for the 3Byte watchdog/process monitor
            string processName = Properties.Settings.Default.ProcessName;
            string exeString = Properties.Settings.Default.ExecutionString;
            
            Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.IProcessMonitor>(new ProcessMonitor(processName, exeString, 1));

            //IMvxLogProvider log = new MvvmCross.Logging.LogProviders.Logger(new Logger());

            //set up the CD singleton
            Mvx.IoCProvider.RegisterSingleton<PCController.Core.Managers.IContinuousDeploymentManager>(new ContinuousDeploymentManager(logger));


            // setup info for the 3Byte AsyncUDP Link for SCS Testing

            //string iPAddress = Properties.Settings.Default.AsyncUdpIPAddress;
            //int remotePort = Properties.Settings.Default.AsyncUdpRemotePort;
            //int localPort = Properties.Settings.Default.AsyncUdpLocalPort;
            //Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.NetworkUtils.IAsyncUdpLink>(new AsyncUdpLink(iPAddress, remotePort, localPort));

            // start the app
            RegisterAppStart<RootViewModel>();
        }

        private void GetAzureSecrets()
        {
            string keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            string kvUri = "https://" + keyVaultName + ".vault.azure.net";

            SecretClient client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
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
