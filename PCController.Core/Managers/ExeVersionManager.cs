using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCController.Core.Managers
{
    public class ExeVersionManager
    {
        public string GetVersion()
        {
            // static version--- FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "Notepad.exe"));
            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Environment.SystemDirectory + "\\Notepad.exe");
            string output = $"File: {myFileVersionInfo.FileDescription} {myFileVersionInfo.FileVersion}";
            return output;

            //TODO massive case statement here checking ever possible stamping method of the exe/dll
        }
    }
}
