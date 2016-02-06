using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.RaspberryPi.Processes;

namespace RaspberryPi
{
    class Program
    {
        const string IP_ADDRESS = "192.168.178.26";
        const int PORT = 11000;

        public static void Main()
        {
            PibrellaBoardLEDsMonitor process = new PibrellaBoardLEDsMonitor();
            process.Start();
        }
    }
}
