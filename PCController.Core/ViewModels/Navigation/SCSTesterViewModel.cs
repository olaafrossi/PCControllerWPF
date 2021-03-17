using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
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
    public class SCSTesterViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;

        public Stopwatch NetStopwatch = new Stopwatch();

        private WindowChildParam _param;

        public SCSTesterViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public override void Prepare(WindowChildParam param) => _param = param;
    }
}
