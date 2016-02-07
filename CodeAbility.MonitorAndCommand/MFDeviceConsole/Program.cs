using System;
using Microsoft.SPOT;

namespace CodeAbility.MonitorAndCommand.MFDeviceConsole
{
    public class Program
    {
        const string IP_ADDRESS = "192.168.178.26";
        const int PORT = 11000;

        const int HEARTBEAT_PERIOD = 0;

        public static void Main()
        {
            Process process = new Process();
            process.Start(IP_ADDRESS, PORT, HEARTBEAT_PERIOD);
        }
    }
}
