using System;
using Raspberry.IO;
using Raspberry.IO.GeneralPurpose;

namespace CodeAbility.RaspberryPi.Pibrella
{
	public class Pibrella
	{
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
		public PinConfiguration LedPinGreen { get; protected set; }
		public PinConfiguration LedPinYellow { get; protected set; }
		public PinConfiguration LedPinRed { get; protected set; }

		public PinConfiguration InputPinA { get; protected set; }
		public PinConfiguration InputPinB { get; protected set; }
		public PinConfiguration InputPinC { get; protected set; }
		public PinConfiguration InputPinD { get; protected set; }

		public PinConfiguration OutputPinA { get; protected set; }
		public PinConfiguration OutputPinB { get; protected set; }
		public PinConfiguration OutputPinC { get; protected set; }
		public PinConfiguration OutputPinD { get; protected set; }

		public PinConfiguration BuzzerPin { get; protected set; }
		public PinConfiguration ButtonPin { get; protected set; }

		protected PinConfiguration[] Pins { get; set; }

		public Pibrella ()
		{
			Initialize (); 
		}

		private void Initialize()
		{
			driver = new GpioConnectionDriver();
			settings = new GpioConnectionSettings () { Driver = driver };

			//Configure pins
			LedPinGreen = ledPinGreen.Output ();//.Name ("LedGreen");
			LedPinYellow = ledPinYellow.Output ();//.Name ("LedYellow");
			LedPinRed = ledPinRed.Output ();//.Name ("LedRed");

			InputPinA = inputPinA.Input ();//.Name ("InputPinA");
			InputPinB = inputPinB.Input ();//.Name ("InputPinB");
			InputPinC = inputPinC.Input ();//.Name ("InputPinC");
			InputPinD = inputPinD.Input ();//.Name ("InputPinD");

			OutputPinA = outputPinA.Output ();//.Name ("OutputPinA");
			OutputPinB = outputPinB.Output ();//.Name ("OutputPinB");
			OutputPinC = outputPinC.Output ();//.Name ("OutputPinC");
			OutputPinD = outputPinD.Output ();//.Name ("OutputPinD");

			BuzzerPin = buzzerPin.Output ();//.Name ("Buzzer");
			ButtonPin = buttonPin.Output ();//.Name ("Button");

			Pins = new PinConfiguration[] { LedPinGreen, LedPinYellow, LedPinRed, 
											InputPinA, InputPinB, InputPinC, InputPinD,
											OutputPinA, OutputPinB, OutputPinC, OutputPinD,
											BuzzerPin, ButtonPin };

			Connection = new GpioConnection (settings, Pins);	
		}
	}
}

