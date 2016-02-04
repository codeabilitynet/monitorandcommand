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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.DeviceConsole
{
    public class MCP4921Simulator
    {
        static MessageClient messageClient;

        public static void Start(string ipAddress, int portNumber)
        {
            messageClient = new MessageClient(Devices.NETDUINO_MCP4921);

            messageClient.DataReceived += client_DataReceived;
            messageClient.CommandReceived += client_CommandReceived;

            Console.WriteLine("Device console.");
            Console.WriteLine("Hit a key to start client, hit [0,1,2,3] to send MCP4921 voltages values, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            bool running = true;

            Console.WriteLine("Running.");

            //Simulating a Netduino device with two LEDs
            messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE);
            messageClient.SubscribeToData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA, Environment.Objects.MCP4921.DATA_DIGITAL_VALUE);
            messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA, Environment.Objects.MCP4921.COMMAND_CONVERT);

            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE, keyInfo.KeyChar.ToString());

                if (keyInfo.KeyChar.Equals('0') || keyInfo.KeyChar.Equals('1') || keyInfo.KeyChar.Equals('2') || keyInfo.KeyChar.Equals('3'))
                {
                    if (messageClient != null)
                        messageClient.SendData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE, keyInfo.KeyChar.ToString());

                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    running = false;
                    break;
                }
            }

            Console.WriteLine("Stopped.");

            messageClient.Stop();
        }

        static void client_CommandReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);
        }

        static void client_DataReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);
        }
    }
}
