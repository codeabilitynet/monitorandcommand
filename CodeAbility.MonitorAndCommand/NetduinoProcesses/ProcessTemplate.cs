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
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

using CodeAbility.MonitorAndCommand.MFClient;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.Netduino.Processes
{

    public abstract class ProcessTemplate
    {
        const int BUTTON_PRESSED_DURATION = 500;        
        const int RECONNECTION_TIMER_DURATION = 60000;

        protected MessageClient messageClient = null;

        protected OutputPort boardLed = new OutputPort(Pins.ONBOARD_LED, false);

        AutoResetEvent reconnectEvent = new AutoResetEvent(false);

        string DeviceName { get; set; }
        int DoWorkPeriod { get; set; }
        int DoWorkStartupTime { get; set; }

        public ProcessTemplate(string deviceName, int doWorkStartupTime, int doWorkPeriod)
        {
            DeviceName = deviceName;
            DoWorkStartupTime = doWorkStartupTime;
            DoWorkPeriod = doWorkPeriod;
        }

        public void Start(string ipAddress, int port, bool isLoggingEnabled)
        {

            while (true)
            {
                try
                {
                    reconnectEvent.Reset();

                    messageClient = new MessageClient(DeviceName, isLoggingEnabled);

                    if (messageClient != null)
                    {
                        messageClient.DataReceived += messageClient_DataReceived;
                        messageClient.CommandReceived += messageClient_CommandReceived;

                        messageClient.Start(ipAddress, port);

                        messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.NetduinoBoard.OBJECT_BOARD_LED, Environment.Objects.NetduinoBoard.DATA_LED_STATUS);
                        messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.NetduinoBoard.OBJECT_BUTTON, Environment.Objects.NetduinoBoard.DATA_BUTTON_STATUS);

                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Objects.NetduinoBoard.OBJECT_BOARD_LED, Environment.Objects.NetduinoBoard.COMMAND_TOGGLE_LED);
                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Objects.NetduinoBoard.OBJECT_BUTTON, Environment.Objects.NetduinoBoard.COMMAND_BUTTON_PRESSED);

                        SendServerMessages();
                     }

                    if (DoWorkPeriod > 0)
                    { 
                        TimerCallback workTimerCallBack = DoWork;
                        Timer workTimer = new Timer(workTimerCallBack, messageClient, DoWorkStartupTime, DoWorkPeriod);
                    }
                    
                    reconnectEvent.WaitOne();
                }
                catch (Exception exception)
                {
                    Logger.Instance.Write("Start()   : " + exception.ToString());

                    if (messageClient != null)
                        messageClient.CommandReceived -= messageClient_CommandReceived;
                }
            }
        }

        protected abstract void SendServerMessages();

        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(DeviceName))
                return;

            HandleReceivedData(e);
        }

        protected abstract void HandleReceivedData(MessageEventArgs e);

        void messageClient_CommandReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(DeviceName))
                return;

            HandleReceivedCommand(e);
        }

        protected abstract void HandleReceivedCommand(MessageEventArgs e);

        private void DoWork(object state)
        {
            try
            {
                PerformPeriodicWork();
            }
            catch (Exception exception)
            {
                messageClient.Log("DoWork   : " + exception.ToString());
                throw;
            }
        }

        protected abstract void PerformPeriodicWork();
    }
}
