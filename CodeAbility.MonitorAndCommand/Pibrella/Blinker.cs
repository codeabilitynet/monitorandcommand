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
        const int BUTTON_PRESSED_DURATION = 250;

        public int Period { get; set; }
        public bool Running { get; set; }
		public bool Blinking { get; set; }

        string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
        int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

		private Timer aTimer;

        PibrellaBoard pibrella = new PibrellaBoard();
        MessageClient client = null; 

        public Blinker(int period) 
        {
            Period = period;

            Running = false; 
			Blinking = false;

            client = new MessageClient(Environment.Devices.PIBRELLA, ipAddress, portNumber);

            pibrella.ButtonPressed += HandleButtonPressed;        
			pibrella.Connection.Open();            
        }

        void client_CommandReceived(object sender, MessageEventArgs e)
        {
			string commandName = e.Name; 
			string parameter = e.Parameter.ToString();
            string content = e.Content.ToString();

			if (commandName.Equals(Environment.Pibrella.COMMAND_TOGGLE_LED)) {
				if (parameter.Equals(Environment.Pibrella.OBJECT_GREEN_LED))
					ToggleGreenLed();
                else if (parameter.Equals(Environment.Pibrella.OBJECT_YELLOW_LED))
					ToggleBlueLed();
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
            System.Threading.Thread thread = new System.Threading.Thread(ButtonPressedSimulator);
            thread.Start();
        }

        private void ButtonPressedSimulator()
        {
            client.SendData(Environment.Devices.ALL, Environment.Pibrella.DATA_BUTTON_STATUS, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.CONTENT_BUTTON_ON);
            System.Threading.Thread.Sleep(BUTTON_PRESSED_DURATION);
            client.SendData(Environment.Devices.ALL, Environment.Pibrella.DATA_BUTTON_STATUS, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.CONTENT_BUTTON_OFF);           
        }

        public void Start()
        {
			Running = true;
			Blinking = true;

            client.CommandReceived += client_CommandReceived;
            client.Start();

			if (pibrella.Connection.IsOpened) {

                //Simulating a Pibrella device
                client.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_GREEN_LED, Environment.Pibrella.DATA_LED_STATUS);
                client.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_YELLOW_LED, Environment.Pibrella.DATA_LED_STATUS);
                client.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_RED_LED, Environment.Pibrella.DATA_LED_STATUS);
                client.PublishData(Environment.Devices.ALL, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.DATA_BUTTON_STATUS);

                client.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_GREEN_LED, Environment.Pibrella.COMMAND_TOGGLE_LED);
                client.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_YELLOW_LED, Environment.Pibrella.COMMAND_TOGGLE_LED);
                client.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_RED_LED, Environment.Pibrella.COMMAND_TOGGLE_LED);
                client.SubscribeToCommand(Environment.Devices.ALL, Environment.Pibrella.OBJECT_BUTTON, Environment.Pibrella.COMMAND_BUTTON_PRESSED);

                aTimer = new Timer(500);
				// Hook up the Elapsed event for the timer. 
				aTimer.Elapsed += OnTimedEvent;
				aTimer.Enabled = true;
			}
        }

        int state = 3;
		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			if (Blinking) {

				if ((state) % 3 == 0)
                { 
					ToggleGreenLed ();
                }

				if ((state) % 3 == 1)
                { 
					ToggleBlueLed ();
                }

				if ((state) % 3 == 2)
                { 
					ToggleRedLed ();
                }
			}

            state++;

            if (state > 5)
                state = 3;
		}

        bool greenLedStatus = false;
		protected void ToggleGreenLed()
		{
			pibrella.Connection.Toggle (pibrella.LedPinGreen);
            greenLedStatus = !greenLedStatus;
            client.SendData(Environment.Devices.ALL, 
                            Environment.Pibrella.OBJECT_GREEN_LED, 
                            Environment.Pibrella.DATA_LED_STATUS,
                            greenLedStatus ? 
                                Environment.Pibrella.CONTENT_LED_STATUS_ON : 
                                Environment.Pibrella.CONTENT_LED_STATUS_OFF);
		}

        bool yellowLedStatus = false;
		protected void ToggleBlueLed()
		{
			pibrella.Connection.Toggle (pibrella.LedPinYellow);
            yellowLedStatus = !yellowLedStatus;
            client.SendData(Environment.Devices.ALL, 
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
            client.SendData(Environment.Devices.ALL, 
                            Environment.Pibrella.OBJECT_RED_LED, 
                            Environment.Pibrella.DATA_LED_STATUS,
                            redLedStatus ?
                                Environment.Pibrella.CONTENT_LED_STATUS_ON :
                                Environment.Pibrella.CONTENT_LED_STATUS_OFF);
		}
			
        public void Stop()
        {
			aTimer.Enabled = false;
		
            pibrella.Connection.Close (); 
        }
    }
}
