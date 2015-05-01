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

        /// <summary>
        /// Send data to the target network machine.
        /// </summary>
        /// <param name="destination">The target machine IP.</param>
        /// <param name="data">Data to be sent, in string format.</param>
        /// <param name="sanitizeIp">Determines whether to remove the port from the given IP string.</param>
        public static void SendData(string destination, string data, bool sanitizeIp = true)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                string completeIp = string.Empty;

                if (sanitizeIp)
                    completeIp = destination.Remove(destination.IndexOf(":"), destination.Length - destination.IndexOf(":"));

                client.Connect(completeIp, 6169);
                client.Send(Encoding.UTF8.GetBytes(data));

            }
        }
    }
}
