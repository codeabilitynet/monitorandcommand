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
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.NetworkInformation;

//using CodeAbility.MonitorAndCommand.Netduino.DS18B20;
//using CodeAbility.MonitorAndCommand.Netduino.LEDs;
//using CodeAbility.MonitorAndCommand.Netduino.MCP4921;
using CodeAbility.MonitorAndCommand.Netduino.Processes;

namespace CodeAbility.MonitorAndCommand.Netduino3Wifi
{
    public class Program
    {
        const string IP_ADDRESS = "192.168.178.26";
        const int PORT = 11000;

        const int HEARTBEAT_PERIOD = 0;

        static AutoResetEvent _networkAvailableEvent = new AutoResetEvent(false);
        static AutoResetEvent _networkAddressChangedEvent = new AutoResetEvent(false);

        public static void Main()
        {
            //Prevent the process from starting before wifi connection is established. Check the following URL for reference
            //http://forums.netduino.com/index.php?/topic/11827-best-practices-how-to-wait-for-a-wi-fi-network-connection/

            // wire up events to wait for network link to connect and address to be acquired
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            // if the network is already up or dhcp address is already set, pre-set those flags.
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                _networkAvailableEvent.Set();
            if (NetworkInterface.GetAllNetworkInterfaces()[0].IsDhcpEnabled && NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress != "0.0.0.0")
                _networkAddressChangedEvent.Set();

            _networkAvailableEvent.WaitOne();
            _networkAddressChangedEvent.WaitOne();

            //Debug.Print(Resources.GetString(Resources.StringResources.String1));

            MCP4921Process process = new MCP4921Process();
            process.Start(IP_ADDRESS, PORT, HEARTBEAT_PERIOD, true);
        }

        static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            _networkAddressChangedEvent.Set();
        }

        static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                _networkAvailableEvent.Set();
            }
        }
    }
}
