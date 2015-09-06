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
            Console.WriteLine("Hit a key to start client, hit [0,2] to send Netduino command, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            Console.WriteLine("Running.");

            messageClient.SubscribeToData(Devices.NETDUINO_LEDs, LEDs.OBJECT_BOARD_LED, LEDs.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_LEDs, LEDs.OBJECT_BUTTON, LEDs.DATA_BUTTON_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_LEDs, LEDs.OBJECT_SENSOR, LEDs.DATA_SENSOR_RANDOM);

            messageClient.SubscribeToData(Devices.NETDUINO_LEDs, LEDs.OBJECT_RED_LED, LEDs.DATA_LED_STATUS);
            messageClient.SubscribeToData(Devices.NETDUINO_LEDs, LEDs.OBJECT_GREEN_LED, LEDs.DATA_LED_STATUS);

            messageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            messageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
            messageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);

            bool running = true;
            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.KeyChar.Equals('0'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_LEDs, LEDs.COMMAND_BUTTON_PRESSED, LEDs.OBJECT_BUTTON, LEDs.CONTENT_BUTTON_PRESSED);
                }
                if (keyInfo.KeyChar.Equals('1'))
                {
                    redLedStatus = !redLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_LEDs, LEDs.COMMAND_TOGGLE_LED, LEDs.OBJECT_RED_LED, redLedStatus ? LEDs.CONTENT_LED_STATUS_ON : LEDs.CONTENT_LED_STATUS_OFF);
                }
                else if (keyInfo.KeyChar.Equals('2'))
                {
                    greenLedStatus = !greenLedStatus;
                    messageClient.SendCommand(Devices.NETDUINO_LEDs, LEDs.COMMAND_TOGGLE_LED, LEDs.OBJECT_GREEN_LED, greenLedStatus ? LEDs.CONTENT_LED_STATUS_ON : LEDs.CONTENT_LED_STATUS_OFF);
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
