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
    public class NetduinoSimulator
    {
        static MessageClient messageClient;

        public static void Start(string ipAddress, int portNumber)
        {
            bool RedLedStatus = false;
            bool GreenLedStatus = false;

            messageClient = new MessageClient(Devices.NETDUINO_PLUS);

            messageClient.DataReceived += client_DataReceived;
            messageClient.CommandReceived += client_CommandReceived;

            Console.WriteLine("Device console.");
            Console.WriteLine("Hit a key to start client, hit [0,2] to send Netduino data, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            bool running = true;

            Console.WriteLine("Running.");

            //Simulating a Netduino device with two LEDs
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_BOARD_LED, Netduino.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_BUTTON, Netduino.DATA_BUTTON_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_SENSOR, Netduino.DATA_SENSOR_RANDOM);

            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_RED_LED, Netduino.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_GREEN_LED, Netduino.DATA_LED_STATUS);

            messageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);

            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.KeyChar.Equals('0'))
                {
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_BUTTON_PRESSED, Netduino.OBJECT_BUTTON, Netduino.CONTENT_BUTTON_PRESSED);
                }
                else if (keyInfo.KeyChar.Equals('1'))
                {
                    RedLedStatus = !RedLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_RED_LED, RedLedStatus ? Netduino.CONTENT_LED_STATUS_ON : Netduino.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.KeyChar.Equals('2'))
                {
                    GreenLedStatus = !GreenLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_GREEN_LED, GreenLedStatus ? Netduino.CONTENT_LED_STATUS_ON : Netduino.CONTENT_LED_STATUS_OFF);
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
