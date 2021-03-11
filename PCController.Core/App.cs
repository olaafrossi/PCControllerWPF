using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MvvmCross.ViewModels;

using PCController.Core.ViewModels;

namespace PCController.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<NavBarViewModel>();
        }
    }
}
