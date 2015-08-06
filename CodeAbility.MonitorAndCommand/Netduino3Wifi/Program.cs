/*
 * Copyright (c) 2015, Paul Gaunard (www.codeability.net)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the 
 *  documentation and/or other materials provided with the distribution.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.Net;

namespace CodeAbility.MonitorAndCommand.Netduino3Wifi
{
    public class Program
    {
        const string IP_ADDRESS = "192.168.178.26";
        const int PORT = 11000;

        static System.Threading.AutoResetEvent _networkAvailableEvent = new System.Threading.AutoResetEvent(false);
        static System.Threading.AutoResetEvent _networkAddressChangedEvent = new System.Threading.AutoResetEvent(false);

        public static void Main()
        {
            //Prevent the process from starting before wifi connection is established. Check the following URL for reference
            //http://forums.netduino.com/index.php?/topic/11827-best-practices-how-to-wait-for-a-wi-fi-network-connection/

            // wire up events to wait for network link to connect and address to be acquired
            Microsoft.SPOT.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            Microsoft.SPOT.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            // if the network is already up or dhcp address is already set, pre-set those flags.
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                _networkAvailableEvent.Set();
            if (Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IsDhcpEnabled && Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress != "0.0.0.0")
                _networkAddressChangedEvent.Set();

            _networkAvailableEvent.WaitOne();
            _networkAddressChangedEvent.WaitOne();

            //Debug.Print(Resources.GetString(Resources.StringResources.String1));

            Process process = new Process();
            process.Start(IP_ADDRESS, PORT, false);
        }

        static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            _networkAddressChangedEvent.Set();
        }

        static void NetworkChange_NetworkAvailabilityChanged(object sender, Microsoft.SPOT.Net.NetworkInformation.NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                _networkAvailableEvent.Set();
            }
        }
    }
}
