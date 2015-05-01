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
using System.Collections; 
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

using CodeAbility.MonitorAndCommand.MFClient;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.Netduino
{
    public class Process
    {
        const int STARTUP_TIME = 5000;
        const int PERIOD = 5000;

        const int BUTTON_PRESSED_DURATION = 500;

        MessageClient messageClient = null;

        OutputPort boardLed = new OutputPort(Pins.ONBOARD_LED, false);
        OutputPort redLed = new OutputPort(Pins.GPIO_PIN_D0, false);
        OutputPort greenLed = new OutputPort(Pins.GPIO_PIN_D1, false);

        InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

        AutoResetEvent autoEvent = new AutoResetEvent(false);

        //Thread boardLedThread = null;
        ManualResetEvent boardLedEvent = new ManualResetEvent(false);

        public bool ledState = false;

        public void Start(string ipAddress, int port)
        {
            while (true)
            {
                try
                {
                    autoEvent.Reset();

                    messageClient = new MessageClient(Environment.Devices.NETDUINO);

                    messageClient.CommandReceived += socketClient_CommandReceived;
                    messageClient.Start(ipAddress, port);

                    messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino.OBJECT_BOARD_LED, Environment.Netduino.DATA_LED_STATUS);
                    messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino.OBJECT_BUTTON, Environment.Netduino.DATA_BUTTON_STATUS);
                    messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino.OBJECT_SENSOR, Environment.Netduino.DATA_SENSOR_RANDOM);

                    messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino.OBJECT_RED_LED, Environment.Netduino.DATA_LED_STATUS);
                    messageClient.PublishData(Environment.Devices.ALL, Environment.Netduino.OBJECT_GREEN_LED, Environment.Netduino.DATA_LED_STATUS);

                    messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Netduino.OBJECT_BOARD_LED, Environment.Netduino.COMMAND_TOGGLE_LED);
                    messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Netduino.OBJECT_GREEN_LED, Environment.Netduino.COMMAND_TOGGLE_LED);
                    messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Netduino.OBJECT_RED_LED, Environment.Netduino.COMMAND_TOGGLE_LED);
                    messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Netduino.OBJECT_BUTTON, Environment.Netduino.COMMAND_BUTTON_PRESSED);

                    button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

                    TimerCallback workTimerCallBack = DoWork;
                    Timer workTimer = new Timer(workTimerCallBack, messageClient, STARTUP_TIME, PERIOD);

                    //boardLedThread = new Thread(BoardLedBlinker);
                    //boardLedThread.Start();

                    autoEvent.WaitOne();
                }
                catch (Exception)
                {
                    autoEvent.Set();
                }
            }
        }

        void socketClient_CommandReceived(object sender, MessageEventArgs e)
        {
            string objectName = e.Parameter.ToString();
            string commandValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

            if (objectName.Equals(Environment.Netduino.OBJECT_BUTTON))
            {
                boardLedEvent.Set();
            }
            else if (objectName.Equals(Environment.Netduino.OBJECT_RED_LED))
            {
                ToggleRedLed(commandValue == Environment.Netduino.CONTENT_LED_STATUS_ON);
            }
            else if (objectName.Equals(Environment.Netduino.OBJECT_GREEN_LED))
            {
                ToggleGreenLed(commandValue == Environment.Netduino.CONTENT_LED_STATUS_ON);
            }
        }

        private void DoWork(object state)
        {
            try
            {
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Netduino.OBJECT_SENSOR, Environment.Netduino.DATA_SENSOR_RANDOM, new Random().NextDouble().ToString());

                //BlinkBoardLED();
            }
            catch (Exception)
            {
                throw;
            }
        }

        void BlinkBoardLED()
        {
            try
            { 
                boardLedEvent.Set();
            }
            catch (Exception)
            {
                throw;
            }
        }

        void ToggleRedLed(bool state)
        {
            redLed.Write(state);

            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, 
                                       Environment.Netduino.OBJECT_RED_LED, 
                                       Environment.Netduino.DATA_LED_STATUS, 
                                       state ? 
                                        Environment.Netduino.CONTENT_LED_STATUS_ON :
                                        Environment.Netduino.CONTENT_LED_STATUS_OFF);
        }

        void ToggleGreenLed(bool state)
        {
            greenLed.Write(state);

            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL,
                                       Environment.Netduino.OBJECT_GREEN_LED,
                                       Environment.Netduino.DATA_LED_STATUS,
                                       state ?
                                        Environment.Netduino.CONTENT_LED_STATUS_ON :
                                        Environment.Netduino.CONTENT_LED_STATUS_OFF);
        }

        private void BoardLedBlinker()
        {
            //while(true)
            //{
                //boardLedEvent.Reset();

                boardLed.Write(true);
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Netduino.OBJECT_BOARD_LED, Environment.Netduino.DATA_LED_STATUS, Environment.Netduino.CONTENT_LED_STATUS_ON);

                Thread.Sleep(BUTTON_PRESSED_DURATION);
            
                boardLed.Write(false);
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.Netduino.OBJECT_BOARD_LED, Environment.Netduino.DATA_LED_STATUS, Environment.Netduino.CONTENT_LED_STATUS_OFF);

                //boardLedEvent.WaitOne();
            //}
        }
 
        #region Interruptions

        void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, Environment.Netduino.OBJECT_BUTTON, Environment.Netduino.DATA_BUTTON_STATUS, Environment.Netduino.CONTENT_BUTTON_ON);
        }

        #endregion 
    }
}
