using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeByteLibrary.Dotnet.NetworkUtils;

namespace PCController.Console
{
    
    public class UDPManager
    {
        private IAsyncUdpLink _asyncUdpLink;

        public IAsyncUdpLink GetUDPManager(string IPAddress, int remotePort, int localPort)
        {
            IAsyncUdpLink link = new AsyncUdpLink(IPAddress, remotePort, localPort);
            _asyncUdpLink = link;
            return _asyncUdpLink;
        }
    }
}
