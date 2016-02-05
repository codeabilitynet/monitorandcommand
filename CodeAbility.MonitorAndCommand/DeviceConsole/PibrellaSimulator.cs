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
    public class PibrellaSimulator
    {
        static MessageClient messageClient;

        public static void Start(string ipAddress, int portNumber)
        {
            bool redLedStatus = false;
            bool greenLedStatus = false;
            bool yellowLedStatus = false;

            messageClient = new MessageClient(Devices.RASPBERRY_PI_B);

            messageClient.DataReceived += client_DataReceived;
            messageClient.CommandReceived += client_CommandReceived;

            Console.WriteLine("Pibrella simulator.");
            Console.WriteLine("Hit a key to start client, hit [0,3] to send Pibrella data, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            bool running = true;

            Console.WriteLine("Running.");

            //Simulating a Pibrella device
            messageClient.PublishData(Devices.ALL, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
            messageClient.PublishData(Devices.ALL, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
            messageClient.PublishData(Devices.ALL, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
            messageClient.PublishData(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

            messageClient.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
            messageClient.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
            messageClient.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
            messageClient.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);

            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.KeyChar.Equals('0'))
                {
                    messageClient.SendData(Devices.ALL,
                                           Environment.Pibrella.OBJECT_BUTTON,
                                           Environment.Pibrella.DATA_BUTTON_STATUS,
                                           Environment.Pibrella.CONTENT_BUTTON_PRESSED);
                }
                else if (keyInfo.KeyChar.Equals('1'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendData(Devices.ALL,
                                           Pibrella.OBJECT_RED_LED,
                                           Pibrella.DATA_LED_STATUS,
                                           redLedStatus ?
                                               Pibrella.CONTENT_LED_STATUS_ON :
                                               Pibrella.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.KeyChar.Equals('2'))
                {
                    yellowLedStatus = !yellowLedStatus;
                    messageClient.SendData(Devices.ALL,
                                           Pibrella.OBJECT_YELLOW_LED,
                                           Pibrella.DATA_LED_STATUS,
                                           yellowLedStatus ?
                                               Pibrella.CONTENT_LED_STATUS_ON :
                                               Pibrella.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.KeyChar.Equals('3'))
                {
                    greenLedStatus = !greenLedStatus;
                    messageClient.SendData(Devices.ALL,
                                           Pibrella.OBJECT_GREEN_LED,
                                           Pibrella.DATA_LED_STATUS,
                                           greenLedStatus ?
                                               Pibrella.CONTENT_LED_STATUS_ON :
                                               Pibrella.CONTENT_LED_STATUS_OFF);
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
