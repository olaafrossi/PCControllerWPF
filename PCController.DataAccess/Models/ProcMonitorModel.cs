using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCController.DataAccess.Models
{
    public class ProcMonitorModel
    {
        public DateTime Timestamp { get; set; }
        public long PeakPagedMemorySize { get; set; }
        public long PeakWorkingSet { get; set; }
        public long PrivateMemorySize { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public bool IsNotResponding { get; set; }
        public string Message { get; set; }
    }
}
