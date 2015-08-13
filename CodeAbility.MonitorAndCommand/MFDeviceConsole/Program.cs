using System;
using Microsoft.SPOT;

namespace CodeAbility.MonitorAndCommand.MFDeviceConsole
{
    public class Program
    {
        const string IP_ADDRESS = "192.168.178.26";
        const int PORT = 11000;

        public static void Main()
        {
            Process process = new Process();
            process.Start(IP_ADDRESS, PORT);
        }
    }
}
