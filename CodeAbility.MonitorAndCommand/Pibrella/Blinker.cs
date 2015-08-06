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

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using Environment = CodeAbility.MonitorAndCommand.Environment; 

using CodeAbility.RaspberryPi.Pibrella;

namespace CodeAbility.RaspberryPi.Pibrella
{
    public class Blinker
    {
        const int BLINKING_PERIOD = 500;

        const int BUTTON_PRESSED_DURATION = 250;
        public int Period { get; set; }
		public bool Blinking { get; set; }

        string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
        int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

		private Timer aTimer;

        PibrellaBoard pibrella = new PibrellaBoard();
        MessageClient messageClient = null; 

        public Blinker(int period) 
        {
            Period = period;

			Blinking = false;

            messageClient = new MessageClient(Environment.Devices.PIBRELLA);

            pibrella.ButtonPressed += HandleButtonPressed;        
			pibrella.Connection.Open();            
        }

        void client_CommandReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages addressed to me
            if (!e.ToDevice.Equals(Environment.Devices.PIBRELLA))
                return;

			string commandName = e.Name; 
			string parameter = e.Parameter.ToString();
            string content = e.Content.ToString();

			if (commandName.Equals(Environment.Pibrella.COMMAND_TOGGLE_LED))
            {
				if (parameter.Equals(Environment.Pibrella.OBJECT_GREEN_LED))
					ToggleGreenLed();
                else if (parameter.Equals(Environment.Pibrella.OBJECT_YELLOW_LED))
					ToggleYellowLed();
                else if (parameter.Equals(Environment.Pibrella.OBJECT_RED_LED))
					ToggleRedLed();
            }
            else if (commandName.Equals(Environment.Pibrella.COMMAND_BUTTON_PRESSED))
            {
				ToggleRunningState();
			}
        }

        void HandleButtonPressed (object sender, EventArgs e)
        {
            ToggleRunningState(); 
        }

        void ToggleRunningState()
        {
            Blinking = !Blinking;
            messageClient.SendData(Environment.Devices.ALL, Environment.Pibrella.DATA_BUTTON_STATUS, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.CONTENT_BUTTON_PRESSED);
        }

        public void Start()
        {
            Blinking = false;

            messageClient.CommandReceived += client_CommandReceived;
            messageClient.Start(ipAddress, portNumber);

			if (pibrella.Connection.IsOpened)
            {
                messageClient.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_GREEN_LED, Environment.Pibrella.DATA_LED_STATUS);
                messageClient.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_YELLOW_LED, Environment.Pibrella.DATA_LED_STATUS);
                messageClient.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_RED_LED, Environment.Pibrella.DATA_LED_STATUS);
                messageClient.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.DATA_BUTTON_STATUS);

                messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_GREEN_LED, Environment.Pibrella.COMMAND_TOGGLE_LED);
                messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_YELLOW_LED, Environment.Pibrella.COMMAND_TOGGLE_LED);
                messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_RED_LED, Environment.Pibrella.COMMAND_TOGGLE_LED);
                messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.COMMAND_BUTTON_PRESSED);

                aTimer = new Timer(BLINKING_PERIOD);
				// Hook up the Elapsed event for the timer. 
				aTimer.Elapsed += OnTimedEvent;
				aTimer.Enabled = true;
			}
        }

        const int RESET_LED_INDEX = 0;
        const int GREEN_LED_INDEX = 1;
        const int YELLOW_LED_INDEX = 2;
        const int RED_LED_INDEX = 3;

        int ledIndex = RESET_LED_INDEX;
		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
            ledIndex++;

			if (Blinking) 
            {
                if (ledIndex == GREEN_LED_INDEX)
                { 
					ToggleGreenLed ();
                }

                if (ledIndex == YELLOW_LED_INDEX)
                { 
					ToggleYellowLed ();
                }

                if (ledIndex == RED_LED_INDEX)
                { 
					ToggleRedLed ();
                }
			}

            if (ledIndex == RED_LED_INDEX)
                ledIndex = RESET_LED_INDEX;
		}

        bool greenLedStatus = false;
		protected void ToggleGreenLed()
		{
			pibrella.Connection.Toggle (pibrella.LedPinGreen);
            greenLedStatus = !greenLedStatus;
            messageClient.SendData(Environment.Devices.ALL, 
                            Environment.Pibrella.OBJECT_GREEN_LED, 
                            Environment.Pibrella.DATA_LED_STATUS,
                            greenLedStatus ? 
                                Environment.Pibrella.CONTENT_LED_STATUS_ON : 
                                Environment.Pibrella.CONTENT_LED_STATUS_OFF);
		}

        bool yellowLedStatus = false;
		protected void ToggleYellowLed()
		{
			pibrella.Connection.Toggle (pibrella.LedPinYellow);
            yellowLedStatus = !yellowLedStatus;
            messageClient.SendData(Environment.Devices.ALL, 
                            Environment.Pibrella.OBJECT_YELLOW_LED, 
                            Environment.Pibrella.DATA_LED_STATUS,
                            yellowLedStatus ?
                                Environment.Pibrella.CONTENT_LED_STATUS_ON :
                                Environment.Pibrella.CONTENT_LED_STATUS_OFF);		
		}

        bool redLedStatus = false;
		protected void ToggleRedLed()
		{
			pibrella.Connection.Toggle (pibrella.LedPinRed);            
            redLedStatus = !redLedStatus;
            messageClient.SendData(Environment.Devices.ALL, 
                            Environment.Pibrella.OBJECT_RED_LED, 
                            Environment.Pibrella.DATA_LED_STATUS,
                            redLedStatus ?
                                Environment.Pibrella.CONTENT_LED_STATUS_ON :
                                Environment.Pibrella.CONTENT_LED_STATUS_OFF);
		}
			
        public void Stop()
        {
			aTimer.Enabled = false;
		
            pibrella.Shutdown(); 
        }
    }
}
