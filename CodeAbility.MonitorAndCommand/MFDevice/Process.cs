/*
 * Copyright (c) 2015, Paul Gaunard (codeability.net)
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
        const int RECONNECTION_TIMER_DURATION = 60000;

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
                    messageClient.CommandReceived -= socketClient_CommandReceived;
                    button.OnInterrupt -= new NativeEventHandler(button_OnInterrupt);

                    AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                    autoResetEvent.WaitOne(RECONNECTION_TIMER_DURATION, false);

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

        //void BlinkBoardLED()
        //{
        //    try
        //    { 
        //        boardLedEvent.Set();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

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
