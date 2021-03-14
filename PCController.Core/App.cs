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
            
            // setup info for the network listener
            int portNum = Properties.Settings.Default.PCListenerUDPPort;
            ILogger netLogger = Log.Logger;

            Mvx.IoCProvider.RegisterSingleton<ThreeByteLibrary.Dotnet.IPcNetworkListener>(new PcNetworkListener(netLogger, portNum));
            
            // start the app
            RegisterAppStart<RootViewModel>();
        }



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
