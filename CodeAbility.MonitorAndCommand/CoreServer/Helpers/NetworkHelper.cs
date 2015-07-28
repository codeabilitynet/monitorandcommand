using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Server
{
    class NetworkHelper
    {
        /// <summary>
        /// Get the IP address of the local server.
        /// </summary>
        /// <returns>The local IP address.</returns>
        public static IPAddress GetLocalIPAddress()
        {
            IPHostEntry host;
            IPAddress localIP = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip;
                }
            }
            return localIP;
        }
    }
}
