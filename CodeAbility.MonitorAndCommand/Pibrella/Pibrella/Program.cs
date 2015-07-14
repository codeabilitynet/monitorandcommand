using System;
using Raspberry.IO;
using Raspberry.IO.GeneralPurpose;

namespace CodeAbility.RaspberryPi.Pibrella
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			int period = args.GetPeriod (); 
			int runningTime = args.GetRunningTime (); 

			Pibrella pibrella = new Pibrella (); 

			Console.WriteLine ("Hit a key to start.");
			Console.ReadKey();

			pibrella.Connection.Open ();

			if (pibrella.Connection.IsOpened) {
				for (int i = 0; i < (runningTime / period); i++) {
				
					if (i % 4 == 0 || (i - 1) % 4 == 0)
						pibrella.Connection.Toggle (pibrella.LedPinGreen);

					if ((i - 1) % 4 == 0 || (i - 2) % 4 == 0)
						pibrella.Connection.Toggle (pibrella.LedPinYellow);

					if ((i - 2) % 4 == 0 || (i - 3) % 4 == 0)
						pibrella.Connection.Toggle (pibrella.LedPinRed);
												
					System.Threading.Thread.Sleep (period);
				}
			}
	
			pibrella.Connection.Close (); 

			Console.WriteLine ("Hit a key to quit.");
			Console.ReadKey(); 

			return; 
		}
	}
}
