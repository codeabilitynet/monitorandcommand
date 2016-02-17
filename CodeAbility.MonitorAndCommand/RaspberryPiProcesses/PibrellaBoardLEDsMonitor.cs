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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Timers;

using Raspberry.IO;
using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.GeneralPurpose.Behaviors;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using Environment = CodeAbility.MonitorAndCommand.Environment; 

using CodeAbility.MonitorAndCommand.RaspberryPi.Tools;

namespace CodeAbility.MonitorAndCommand.RaspberryPi.Processes
{
    public class PibrellaBoardLEDsMonitor
    {
        const int HEARTBEAT_PERIOD_IN_MILLESECONDS = 10000; 

        string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
        int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

        PibrellaBoard pibrella = new PibrellaBoard();
        MessageClient messageClient = null;

        public PibrellaBoardLEDsMonitor() 
        {
            messageClient = new MessageClient(Environment.Devices.RASPBERRY_PI_B, HEARTBEAT_PERIOD_IN_MILLESECONDS);

            pibrella.ButtonPressed += HandleButtonPressed;        
            pibrella.Connection.Open();            
        }

        void client_CommandReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(Environment.Devices.RASPBERRY_PI_B))
                return;

            string commandName = e.Name; 
            string parameter = e.Parameter.ToString();
            string content = e.Content.ToString();

            if (commandName.Equals(Environment.Objects.PibrellaBoard.COMMAND_TOGGLE_LED))
            {
                if (parameter.Equals(Environment.Objects.PibrellaBoard.OBJECT_GREEN_LED))
	                ToggleGreenLed(content);
                else if (parameter.Equals(Environment.Objects.PibrellaBoard.OBJECT_YELLOW_LED))
	                ToggleYellowLed(content);
                else if (parameter.Equals(Environment.Objects.PibrellaBoard.OBJECT_RED_LED))
	                ToggleRedLed(content);
            }
        }

        void HandleButtonPressed (object sender, EventArgs e)
        {

        }

        public void Start()
        {
            messageClient.CommandReceived += client_CommandReceived;
            messageClient.Start(ipAddress, portNumber);

            if (pibrella.Connection.IsOpened)
            {
                messageClient.PublishData(Environment.Devices.WINDOWS_SURFACE, Environment.Objects.PibrellaBoard.OBJECT_GREEN_LED, Environment.Objects.PibrellaBoard.DATA_LED_STATUS);
                messageClient.PublishData(Environment.Devices.WINDOWS_SURFACE, Environment.Objects.PibrellaBoard.OBJECT_YELLOW_LED, Environment.Objects.PibrellaBoard.DATA_LED_STATUS);
                messageClient.PublishData(Environment.Devices.WINDOWS_SURFACE, Environment.Objects.PibrellaBoard.OBJECT_RED_LED, Environment.Objects.PibrellaBoard.DATA_LED_STATUS);
                messageClient.PublishData(Environment.Devices.WINDOWS_SURFACE, Environment.Objects.PibrellaBoard.OBJECT_BUTTON, Environment.Objects.PibrellaBoard.DATA_BUTTON_STATUS);

                messageClient.SubscribeToCommand(Environment.Devices.SERVER, Environment.Objects.PibrellaBoard.OBJECT_GREEN_LED, Environment.Objects.PibrellaBoard.COMMAND_TOGGLE_LED);
                messageClient.SubscribeToCommand(Environment.Devices.SERVER, Environment.Objects.PibrellaBoard.OBJECT_YELLOW_LED, Environment.Objects.PibrellaBoard.COMMAND_TOGGLE_LED);
                messageClient.SubscribeToCommand(Environment.Devices.SERVER, Environment.Objects.PibrellaBoard.OBJECT_RED_LED, Environment.Objects.PibrellaBoard.COMMAND_TOGGLE_LED);
            }
        }

        bool IsOn(string content)
        {
            if (content == Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_ON)
                return true;
            else if (content == Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_OFF)
                return false;
            else
                return false;
        }

        bool greenLedStatus = false;
		protected void ToggleGreenLed(string content)
		{
            if (IsOn(content) != greenLedStatus)
            { 
                greenLedStatus = IsOn(content);
                pibrella.Connection.Toggle(pibrella.LedPinGreen);
            }

            messageClient.SendData(Environment.Devices.ALL,
                                    Environment.Objects.PibrellaBoard.OBJECT_GREEN_LED,
                                    Environment.Objects.PibrellaBoard.DATA_LED_STATUS,
                                    greenLedStatus ?
                                        Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_ON :
                                        Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_OFF);
		}

        bool yellowLedStatus = false;
        protected void ToggleYellowLed(string content)
		{
            if (IsOn(content) != yellowLedStatus)
            {
                yellowLedStatus = IsOn(content);
                pibrella.Connection.Toggle(pibrella.LedPinYellow);
            }

            messageClient.SendData(Environment.Devices.ALL,
                                    Environment.Objects.PibrellaBoard.OBJECT_YELLOW_LED,
                                    Environment.Objects.PibrellaBoard.DATA_LED_STATUS,
                                    yellowLedStatus ?
                                        Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_ON :
                                        Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_OFF);		
		}

        bool redLedStatus = false;
        protected void ToggleRedLed(string content)
		{
            if (IsOn(content) != redLedStatus)
            {
                redLedStatus = IsOn(content);
                pibrella.Connection.Toggle(pibrella.LedPinRed);
            }

            messageClient.SendData(Environment.Devices.ALL,
                                    Environment.Objects.PibrellaBoard.OBJECT_RED_LED,
                                    Environment.Objects.PibrellaBoard.DATA_LED_STATUS,
                                    redLedStatus ?
                                        Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_ON :
                                        Environment.Objects.PibrellaBoard.CONTENT_LED_STATUS_OFF);
		}
			
        public void Stop()
        {		
            pibrella.Shutdown(); 
        }
    }
}
