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

