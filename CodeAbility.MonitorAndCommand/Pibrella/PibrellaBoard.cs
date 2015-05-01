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
using Raspberry.IO;
using Raspberry.IO.GeneralPurpose;

namespace CodeAbility.RaspberryPi.Pibrella
{
	public class PibrellaBoard
	{
		public event EventHandler ButtonPressed;

		//Infrastructure
		IGpioConnectionDriver driver;
		GpioConnectionSettings settings;

		//Connection
		public GpioConnection Connection { get; protected set; }

		//Connectors
		ConnectorPin ledPinGreen = ConnectorPin.P1Pin7; //4
		ConnectorPin ledPinYellow = ConnectorPin.P1Pin11; //17
		ConnectorPin ledPinRed = ConnectorPin.P1Pin13; //21

		ConnectorPin inputPinA = ConnectorPin.P1Pin21; //9
		ConnectorPin inputPinB = ConnectorPin.P1Pin26; //7
		ConnectorPin inputPinC = ConnectorPin.P1Pin24; //8
		ConnectorPin inputPinD = ConnectorPin.P1Pin19; //10

		ConnectorPin outputPinA = ConnectorPin.P1Pin15; //22
		ConnectorPin outputPinB = ConnectorPin.P1Pin16; //23
		ConnectorPin outputPinC = ConnectorPin.P1Pin18; //24
	    ConnectorPin outputPinD = ConnectorPin.P1Pin22; //25
	
		ConnectorPin buttonPin = ConnectorPin.P1Pin23; //11
		ConnectorPin buzzerPin = ConnectorPin.P1Pin12; //18

		//Pin configurations
		public OutputPinConfiguration LedPinGreen { get; protected set; }
        public OutputPinConfiguration LedPinYellow { get; protected set; }
        public OutputPinConfiguration LedPinRed { get; protected set; }

		public InputPinConfiguration InputPinA { get; protected set; }
        public InputPinConfiguration InputPinB { get; protected set; }
        public InputPinConfiguration InputPinC { get; protected set; }
        public InputPinConfiguration InputPinD { get; protected set; }

        public OutputPinConfiguration OutputPinA { get; protected set; }
        public OutputPinConfiguration OutputPinB { get; protected set; }
        public OutputPinConfiguration OutputPinC { get; protected set; }
        public OutputPinConfiguration OutputPinD { get; protected set; }

        public OutputPinConfiguration BuzzerPin { get; protected set; }
		public PinConfiguration ButtonPin { get; protected set; }

		protected PinConfiguration[] Pins { get; set; }

		public PibrellaBoard ()
		{
			Initialize (); 
		}

		private void Initialize()
		{
			driver = new GpioConnectionDriver();
			settings = new GpioConnectionSettings () { Driver = driver };

			//Configure pins
			LedPinGreen = ledPinGreen.Output ();
			LedPinYellow = ledPinYellow.Output ();
			LedPinRed = ledPinRed.Output ();

			InputPinA = inputPinA.Input ();
			InputPinB = inputPinB.Input ();
			InputPinC = inputPinC.Input ();
			InputPinD = inputPinD.Input ();

			OutputPinA = outputPinA.Output ();
			OutputPinB = outputPinB.Output ();
			OutputPinC = outputPinC.Output ();
			OutputPinD = outputPinD.Output ();

			BuzzerPin = buzzerPin.Output ();

            //Declaring a ButtonPressed handler
			ButtonPin = buttonPin.Input().Name("Button").Revert().Switch().Enable().OnStatusChanged(x =>
				{
					OnButtonPressed(new EventArgs());
					Console.WriteLine("Button pressed");
				});

			Pins = new PinConfiguration[] { LedPinGreen, LedPinYellow, LedPinRed, 
											InputPinA, InputPinB, InputPinC, InputPinD,
											OutputPinA, OutputPinB, OutputPinC, OutputPinD,
											BuzzerPin, ButtonPin };

			Connection = new GpioConnection (settings, Pins);	
		}

		public virtual void OnButtonPressed(EventArgs e)
		{
			EventHandler handler = ButtonPressed;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}

