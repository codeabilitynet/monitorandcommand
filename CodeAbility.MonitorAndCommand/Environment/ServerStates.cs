using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Environment
{
    public class ServerStates
    {
        public const string STATE_NETDUINO_ISCONNECTED = "NetduinoIsConnected";
        public const string STATE_PIBRELLA_ISCONNECTED = "RaspberryPiBIsConnected";
        public const string STATE_WINDOWSPHONE_ISCONNECTED = "WindowsPhoneIsConnected";
        public const string STATE_SURFACE_ISCONNECTED = "SurfaceIsConnected";

        public const string STATE_VOLTAGE_CONTROL = "VoltageControl";
    }
}
