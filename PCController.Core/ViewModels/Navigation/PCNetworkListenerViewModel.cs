using System;
using System.Collections.Generic;
using System.Diagnostics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;


namespace PCController.Core.ViewModels
{
    public class PCNetworkListenerViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;

        private WindowChildParam _param;

        



        public PCNetworkListenerViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            var pcNetworkListener = Mvx.IoCProvider.Resolve<IPcNetworkListener>();
            pcNetworkListener.MessageHit += PCNetworkManagerOnMessage;
        }

        private void PCNetworkManagerOnMessage(object sender, NetworkMessagesEventArgs e)
        {
            PCNetworkManager pc = new PCNetworkManager(e);
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public override void Prepare(WindowChildParam param) => _param = param;



    }
}
