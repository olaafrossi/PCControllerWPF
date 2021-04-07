using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.Core.Properties;
using PCController.DataAccess;
using PCController.DataAccess.Models;

// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Core.ViewModels
{
    public sealed class AppDeployViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly IMvxLog _log;
        private readonly Stopwatch _stopwatch;
        private WindowChildParam _param;

        public AppDeployViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _log = logProvider.GetLogFor<AppDeployViewModel>();
            _log.Info("AppDeployViewModel has been constructed {logProvider} {navigationService}", logProvider, navigationService);
            _stopwatch = new Stopwatch();

            // Setup UI Commands

            // set initial UI Fields

            // Fetch Initial Data

        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        public override void Prepare(WindowChildParam param)
        {
            _param = param;
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";


    }
}
