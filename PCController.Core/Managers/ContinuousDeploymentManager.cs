using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MvvmCross.Logging;
using Octokit;
using PCController.Core.Models;
using PCController.Core.Properties;
using Serilog;

namespace PCController.Core.Managers
{
    public class ContinuousDeploymentManager : IContinuousDeploymentManager
    {
        private readonly ILogger _log = Log.Logger;

        public ContinuousDeploymentManager(ILogger log)
        {
            _log = log;
            _log.Information("ContinuousDeploymentManager has been constructed");

            // setting private variables

        }

        private void GetSetupInfo()
        {

        }
    }
}
