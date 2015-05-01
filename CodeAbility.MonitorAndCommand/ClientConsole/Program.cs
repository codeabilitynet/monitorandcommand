// .NET/Mono Monitor and Command Middleware for embedded projects.
// Copyright (C) 2015 Paul Gaunard (codeability.net)

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
    class Program
    {
        static MessageClient client; 

        static void Main(string[] args)
        {
            string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

            bool RedLedStatus = false;
            bool GreenLedStatus = false;
            bool YellowLedStatus = false;

            client = new MessageClient(Devices.PIBRELLA, ipAddress, portNumber);

            client.DataReceived += client_DataReceived;
            client.CommandReceived += client_CommandReceived;

            Console.WriteLine("Device console.");
            Console.WriteLine("Hit a key to start, hit [0,3] to send Pibrella data, hit ESC to exit.");
            Console.ReadKey();
            client.Start();

            bool running = true;

            Console.WriteLine("Running.");

            //Simulating a Pibrella device
            client.PublishData(Devices.ALL, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
            client.PublishData(Devices.ALL, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
            client.PublishData(Devices.ALL, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
            client.PublishData(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

            client.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
            client.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
            client.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
            client.SubscribeToCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);

            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.NumPad0)
                {
                    Thread thread = new Thread(PibrellaButtonPressedSimulator);
                    thread.Start();
                }
                else if (keyInfo.Key == ConsoleKey.NumPad1)
                {
                    RedLedStatus = !RedLedStatus;
                    client.SendData(Devices.ALL, 
                                    Pibrella.OBJECT_RED_LED, 
                                    Pibrella.DATA_LED_STATUS, RedLedStatus ? 
                                        Pibrella.CONTENT_LED_STATUS_ON :
                                        Pibrella.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.Key == ConsoleKey.NumPad2)
                {
                    YellowLedStatus = !YellowLedStatus;
                    client.SendData(Devices.ALL, 
                                    Pibrella.OBJECT_YELLOW_LED, 
                                    Pibrella.DATA_LED_STATUS, YellowLedStatus ? 
                                        Pibrella.CONTENT_LED_STATUS_ON :
                                        Pibrella.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.Key == ConsoleKey.NumPad3)
                {
                    GreenLedStatus = !GreenLedStatus;
                    client.SendData(Devices.ALL, 
                                    Pibrella.OBJECT_GREEN_LED, 
                                    Pibrella.DATA_LED_STATUS, 
                                    GreenLedStatus ? 
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

            client.Stop(); 
        }

        static void PibrellaButtonPressedSimulator()
        {
            client.SendData(Devices.ALL, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.DATA_BUTTON_STATUS, Environment.Pibrella.CONTENT_BUTTON_ON);
            Thread.Sleep(500);
            client.SendData(Devices.ALL, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.DATA_BUTTON_STATUS, Environment.Pibrella.CONTENT_BUTTON_OFF);
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
