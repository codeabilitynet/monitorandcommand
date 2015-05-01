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
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.RemoteConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string DEVICE_NAME = "WindowsPhone";

            string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

            MessageClient client = new MessageClient(DEVICE_NAME, ipAddress, portNumber);

            client.DataReceived += client_DataReceived;

            Console.WriteLine("Remote console.");
            Console.WriteLine("Hit a key to start, hit [0,3] to send Pibrella commands, hit ESC to exit.");
            Console.ReadKey();
            client.Start();

            bool running = true;

            Console.WriteLine("Running.");

            //Pibrella
            client.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
            client.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
            client.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
            client.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

            client.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
            client.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
            client.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
            client.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);

            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.NumPad0)
                {
                    client.SendCommand(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED, null);
                }
				if (keyInfo.Key == ConsoleKey.NumPad1)
				{
                    client.SendCommand(Devices.PIBRELLA, Pibrella.COMMAND_TOGGLE_LED, Pibrella.OBJECT_GREEN_LED,  null);
				}
				else if (keyInfo.Key == ConsoleKey.NumPad2)
				{
                    client.SendCommand(Devices.PIBRELLA, Pibrella.COMMAND_TOGGLE_LED, Pibrella.OBJECT_YELLOW_LED, null);
				}
				else if (keyInfo.Key == ConsoleKey.NumPad3)
				{
                    client.SendCommand(Devices.PIBRELLA, Pibrella.COMMAND_TOGGLE_LED, Pibrella.OBJECT_RED_LED, null);
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

        static void client_DataReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);
        }       
    }
}
