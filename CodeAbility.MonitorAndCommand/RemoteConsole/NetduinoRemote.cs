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

            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, NetduinoPlus.OBJECT_BOARD_LED, NetduinoPlus.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, NetduinoPlus.OBJECT_BUTTON, NetduinoPlus.DATA_BUTTON_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, NetduinoPlus.OBJECT_SENSOR, NetduinoPlus.DATA_SENSOR_RANDOM);

            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, NetduinoPlus.OBJECT_RED_LED, NetduinoPlus.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_PLUS, NetduinoPlus.OBJECT_GREEN_LED, NetduinoPlus.DATA_LED_STATUS);

            messageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);

            bool running = true;
            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.KeyChar.Equals('0'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, NetduinoPlus.COMMAND_BUTTON_PRESSED, NetduinoPlus.OBJECT_BUTTON, NetduinoPlus.CONTENT_BUTTON_PRESSED);
                }
                if (keyInfo.KeyChar.Equals('1'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, NetduinoPlus.COMMAND_TOGGLE_LED, NetduinoPlus.OBJECT_RED_LED, redLedStatus ? NetduinoPlus.CONTENT_LED_STATUS_ON : NetduinoPlus.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.KeyChar.Equals('2'))
                {
                    greenLedStatus = !greenLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_PLUS, NetduinoPlus.COMMAND_TOGGLE_LED, NetduinoPlus.OBJECT_GREEN_LED, greenLedStatus ? NetduinoPlus.CONTENT_LED_STATUS_ON : NetduinoPlus.CONTENT_LED_STATUS_OFF);
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
