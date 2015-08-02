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
    public class NetduinoRemote
    {
        public static void Start(string ipAddress, int portNumber)
        {
            bool redLedStatus = false;
            bool greenLedStatus = false;

            MessageClient messageClient = new MessageClient(Devices.WINDOWS_PHONE);

            messageClient.DataReceived += client_DataReceived;

            Console.WriteLine("Remote console");
            Console.WriteLine("Hit a key to start client, hit [0,2] to send Pibrella commands, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            Console.WriteLine("Running.");

            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_BOARD_LED, Netduino.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_BUTTON, Netduino.DATA_BUTTON_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_SENSOR, Netduino.DATA_SENSOR_RANDOM);

            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_RED_LED, Netduino.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_GREEN_LED, Netduino.DATA_LED_STATUS);

            messageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);

            bool running = true;
            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.KeyChar.Equals('0'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_BUTTON_PRESSED, Netduino.OBJECT_BUTTON, Netduino.CONTENT_BUTTON_PRESSED);
                }
                if (keyInfo.KeyChar.Equals('1'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_RED_LED, redLedStatus ? Netduino.CONTENT_LED_STATUS_ON : Netduino.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.KeyChar.Equals('2'))
                {
                    greenLedStatus = !greenLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_GREEN_LED, greenLedStatus ? Netduino.CONTENT_LED_STATUS_ON : Netduino.CONTENT_LED_STATUS_OFF);
                }
                if (keyInfo.Key == ConsoleKey.Escape)
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
