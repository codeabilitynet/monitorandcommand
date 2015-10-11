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

namespace CodeAbility.MonitorAndCommand.Netduino.MCP4921
{

    public class Process
    {
        const int STARTUP_TIME = 5000;
        const int PERIOD = 50;

        const int BUTTON_PRESSED_DURATION = 500;        
        const int RECONNECTION_TIMER_DURATION = 60000;

        const double BOARD_REFERENCE_VOLTAGE = 3.3;

        const double LED_MINIMUM_FORWARD_VOLTAGE = 1.5; 
        const double LED_MAXIMUM_FORWARD_VOLTAGE = 3.5;

        MessageClient messageClient = null;

        DAConverter converter = null;

        OutputPort boardLed = new OutputPort(Pins.ONBOARD_LED, false);

        AutoResetEvent reconnectEvent = new AutoResetEvent(false);

        public void Start(string ipAddress, int port, bool isLoggingEnabled)
        {
            while (true)
            {
                try
                {
                    reconnectEvent.Reset();

                    messageClient = new MessageClient(Environment.Devices.NETDUINO_MCP4921, isLoggingEnabled);

                    if (messageClient != null)
                    {
                        converter = new DAConverter(BOARD_REFERENCE_VOLTAGE);

                        messageClient.DataReceived += messageClient_DataReceived;
                        messageClient.CommandReceived += messageClient_CommandReceived;

                        messageClient.Start(ipAddress, port);

                        messageClient.PublishData(Environment.Devices.ALL, Environment.MCP4921.OBJECT_ANALOG_DATA, Environment.MCP4921.DATA_ANALOG_VALUE);
                        messageClient.SubscribeToData(Environment.Devices.ALL, Environment.MCP4921.OBJECT_DIGITAL_DATA, Environment.MCP4921.DATA_DIGITAL_VALUE);
                        messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.MCP4921.OBJECT_DIGITAL_DATA, Environment.MCP4921.COMMAND_CONVERT);
                     }

                    //TimerCallback workTimerCallBack = DoWork;
                    //Timer workTimer = new Timer(workTimerCallBack, messageClient, STARTUP_TIME, PERIOD);
                    
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

        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(Environment.Devices.NETDUINO_MCP4921))
                return;

            string objectName = e.Parameter.ToString();
            string commandValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

            if (objectName.Equals(Environment.MCP4921.OBJECT_DIGITAL_DATA))
            {
                //boardLedEvent.Set();
            }
        }

        void messageClient_CommandReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(Environment.Devices.NETDUINO_MCP4921))
                return;

            string objectName = e.Parameter.ToString();
            string dataValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

            if (objectName.Equals(Environment.MCP4921.OBJECT_DIGITAL_DATA))
            {        
                int inputData = Int32.Parse(dataValue);
                Convert(inputData);
            }
        }

        private void Convert(int inputData)
        {
            if (!(inputData >= 0 && inputData <= 100)) 
                return; 
            
            boardLed.Write(true);

            double voltage = converter.Write(ComputeConverterData(inputData));

            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, Environment.MCP4921.OBJECT_ANALOG_DATA, Environment.MCP4921.DATA_ANALOG_VALUE, voltage);

            boardLed.Write(false);  
        }

        private int ComputeConverterData(int inputData)
        {
            int converterData = 0;

            double expectedVoltage = LED_MINIMUM_FORWARD_VOLTAGE + ((double)inputData / 100) * (BOARD_REFERENCE_VOLTAGE - LED_MINIMUM_FORWARD_VOLTAGE);

            converterData = (int)((expectedVoltage / BOARD_REFERENCE_VOLTAGE) * DAConverter.STEPS);

            return converterData; 
        }
    }
}
