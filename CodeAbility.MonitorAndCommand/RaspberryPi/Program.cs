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
        const int BLINKING_PERIOD_IN_MILLISECONDS = 1000;

        public static void Main()
        {
            //PibrellaBoardLEDsBlinkingProcess process = new PibrellaBoardLEDsBlinkingProcess(BLINKING_PERIOD_IN_MILLISECONDS);
            PibrellaBoardLEDsMonitor process = new PibrellaBoardLEDsMonitor();
            process.Start();
        }
    }
}
