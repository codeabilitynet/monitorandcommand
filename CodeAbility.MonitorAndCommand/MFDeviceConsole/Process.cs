/*
 * Copyright (c) 2015, Paul Gaunard (www.codeability.net)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the 
 *  documentation and/or other materials provided with the distribution.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL
 * 
 * 
 * , SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections; 
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using CodeAbility.MonitorAndCommand.MFClient;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.MFDeviceConsole
{
    public class Process
    {
        const int STARTUP_TIME = 5000;
        const int PERIOD = 2500;

        const int BUTTON_PRESSED_DURATION = 500;
        const int RECONNECTION_TIMER_DURATION = 60000;

        MessageClient messageClient = null;

        AutoResetEvent reconnectEvent = new AutoResetEvent(false);

        public bool ledState = false;

        public void Start(string ipAddress, int port, int heartbeatPeriod)
        {
            while (true)
            {
                try
                {
                    reconnectEvent.Reset();

                    messageClient = new MessageClient(Environment.Devices.NETDUINO_3_WIFI, heartbeatPeriod, false);

                    if (messageClient != null)
                    { 
                        messageClient.CommandReceived += socketClient_CommandReceived;

                        messageClient.Start(ipAddress, port);

                        messageClient.PublishData(Environment.Devices.ALL, Environment.LEDs.OBJECT_BOARD_LED, Environment.LEDs.DATA_LED_STATUS);
                        messageClient.PublishData(Environment.Devices.ALL, Environment.LEDs.OBJECT_BUTTON, Environment.LEDs.DATA_BUTTON_STATUS);
                        messageClient.PublishData(Environment.Devices.ALL, Environment.LEDs.OBJECT_SENSOR, Environment.LEDs.DATA_SENSOR_RANDOM);

                        messageClient.PublishData(Environment.Devices.ALL, Environment.LEDs.OBJECT_RED_LED, Environment.LEDs.DATA_LED_STATUS);
                        messageClient.PublishData(Environment.Devices.ALL, Environment.LEDs.OBJECT_GREEN_LED, Environment.LEDs.DATA_LED_STATUS);

                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.LEDs.OBJECT_BOARD_LED, Environment.LEDs.COMMAND_TOGGLE_LED);
                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.LEDs.OBJECT_GREEN_LED, Environment.LEDs.COMMAND_TOGGLE_LED);
                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.LEDs.OBJECT_RED_LED, Environment.LEDs.COMMAND_TOGGLE_LED);
                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.LEDs.OBJECT_BUTTON, Environment.LEDs.COMMAND_BUTTON_PRESSED);
                    }

                    TimerCallback workTimerCallBack = DoWork;
                    Timer workTimer = new Timer(workTimerCallBack, messageClient, STARTUP_TIME, PERIOD);

                    reconnectEvent.WaitOne();
                }
                catch (Exception exception)
                {
                    Logger.Instance.Write("Start()   : " + exception.ToString());

                    if (messageClient != null)
                        messageClient.CommandReceived -= socketClient_CommandReceived;

                    AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                    autoResetEvent.WaitOne(RECONNECTION_TIMER_DURATION, false);

                    reconnectEvent.Set();
                }
            }
        }

        void socketClient_CommandReceived(object sender, MessageEventArgs e)
        {
            try
            {
                //Only consider the messages addressed to me
                if (!e.ToDevice.Equals(Environment.Devices.NETDUINO_3_WIFI))
                    return;

                string targetName = e.Name.ToString();
                string commandValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

                if (targetName.Equals(Environment.LEDs.OBJECT_BUTTON))
                {
                    //boardLedEvent.Set();
                }
                else if (targetName.Equals(Environment.LEDs.OBJECT_RED_LED))
                {
                    ToggleRedLed(commandValue == Environment.LEDs.CONTENT_LED_STATUS_ON);
                }
                else if (targetName.Equals(Environment.LEDs.OBJECT_GREEN_LED))
                {
                    ToggleGreenLed(commandValue == Environment.LEDs.CONTENT_LED_STATUS_ON);
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Write("Command  : " + exception.ToString());
                throw;
            }
        }

        Random random = new Random();
        string sensorDataString = String.Empty;

        private void DoWork(object state)
        {
            try
            {
                //if (!messageClient.IsConnected)
                //    throw new Exception();

                if (messageClient != null)
                {
                    //Board LED On
                    messageClient.SendData(Environment.Devices.ALL, Environment.LEDs.OBJECT_BOARD_LED, Environment.LEDs.DATA_LED_STATUS, Environment.LEDs.CONTENT_LED_STATUS_ON);

                    //Sensor data
                    sensorDataString = random.NextDouble().ToString();
                    messageClient.SendData(Environment.Devices.ALL, Environment.LEDs.OBJECT_SENSOR, Environment.LEDs.DATA_SENSOR_RANDOM, sensorDataString);

                    //Board LED Off
                    messageClient.SendData(Environment.Devices.ALL, Environment.LEDs.OBJECT_BOARD_LED, Environment.LEDs.DATA_LED_STATUS, Environment.LEDs.CONTENT_LED_STATUS_OFF);
                }
             }
            catch (Exception exception)
            {
                Logger.Instance.Write("DoWork   : " + exception.ToString());
                throw;
            }
        }

        void ToggleRedLed(bool state)
        {
            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, 
                                       Environment.LEDs.OBJECT_RED_LED, 
                                       Environment.LEDs.DATA_LED_STATUS, 
                                       state ? 
                                        Environment.LEDs.CONTENT_LED_STATUS_ON :
                                        Environment.LEDs.CONTENT_LED_STATUS_OFF);
        }

        void ToggleGreenLed(bool state)
        {
            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL,
                                       Environment.LEDs.OBJECT_GREEN_LED,
                                       Environment.LEDs.DATA_LED_STATUS,
                                       state ?
                                        Environment.LEDs.CONTENT_LED_STATUS_ON :
                                        Environment.LEDs.CONTENT_LED_STATUS_OFF);
        }
 
        #region Interruptions

        //void button_OnInterrupt(uint data1, uint data2, DateTime time)
        //{
        //    if (messageClient != null)
        //        messageClient.SendData(Environment.Devices.ALL, Environment.Netduino.OBJECT_BUTTON, Environment.Netduino.DATA_BUTTON_STATUS, Environment.Netduino.CONTENT_BUTTON_PRESSED);
        //}

        #endregion 
    }
}
