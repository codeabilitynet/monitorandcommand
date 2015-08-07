using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Server
{
    class NetworkHelper
    {
        /// <summary>
        /// Get the IP address of the local server.
        /// 
        /// </summary>
        /// <returns>The local IP address.</returns>
        //public static IPAddress GetLocalIPAddress()
        //{
        //    string hostName = Dns.GetHostName();
        //    IPHostEntry host = Dns.GetHostEntry(hostName);
        //    IPAddress localIP = host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

        //    return localIP;
        //}

        public static bool CheckAddressValidity(IPAddress ipAddress)
        {
            string hostName = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostName);

            return host.AddressList.Contains(ipAddress);
        }
    }
}
