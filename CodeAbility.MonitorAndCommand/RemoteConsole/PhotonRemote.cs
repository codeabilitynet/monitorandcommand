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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.RemoteConsole
{
    public class PhotonRemote
    {
        public static void Start(string ipAddress, int portNumber)
        {
            
            MessageClient messageClient = new MessageClient(Devices.WINDOWS_PHONE);

            messageClient.DataReceived += client_DataReceived;

            Console.WriteLine("Remote console");

            Console.WriteLine("Hit a key to start client, hit [0,3] to send Photon commands, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            Console.WriteLine("Running.");

            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);

            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
            messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

            messageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
            messageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED);

            messageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED);
            messageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN);
            messageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE);

            bool running = true;
            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                double random = new Random().NextDouble();

                if (keyInfo.KeyChar.Equals('0'))
                {
                    messageClient.SendCommand(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.COMMAND_TOGGLE_LED, String.Empty);
                }
                if (keyInfo.KeyChar.Equals('1'))
                {
                    int rgbRed = (int)(random * 255);
                    messageClient.SendCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_RED, rgbRed.ToString());
                }
                if (keyInfo.KeyChar.Equals('2'))
                {
                    int rgbGreen = (int)(random * 255);
                    messageClient.SendCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_GREEN, rgbGreen.ToString());
                }
                if (keyInfo.KeyChar.Equals('3'))
                {
                    int rgbBlue = (int)(random * 255);
                    messageClient.SendCommand(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.COMMAND_SET_RGB_BLUE, rgbBlue.ToString());
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

        static void client_DataReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);
        }
    }
}

