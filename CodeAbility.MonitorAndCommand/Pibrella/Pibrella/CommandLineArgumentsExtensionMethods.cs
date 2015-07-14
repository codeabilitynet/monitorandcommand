using System;

namespace CodeAbility.RaspberryPi.Pibrella
{
	internal static class CommandLineArgumentsExtensionMethods
	{
		const int defaultRunningTime = 10000; //in milliseconds
		const int defaultPeriod = 200; //in milliseconds

		public static int GetPeriod(this string[] args)
		{
			int period = defaultPeriod; 

			if (args.Length > 0 && args [0] != null)
				Int32.TryParse(args [0], out period);
				
			return period;
		}

		public static int GetRunningTime(this string[] args)
		{
			int runningTime = defaultRunningTime; 

			if (args.Length > 1 && args [1] != null)
				Int32.TryParse(args [1], out runningTime);

			return runningTime;
		}
	}
}
	