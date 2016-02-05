using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Environment
{
    public class ServerStates
    {
        public enum ConnectionStates { Connected, Disconnected };

        public const string STATE_CONNECTION_NETDUINO_3_WIFI = "Connection.Netduino3Wifi";
        public const string STATE_CONNECTION_RASPBERRY_B = "Connection.RaspberryPiB";
        public const string STATE_CONNECTION_WINDOWS_PHONE = "Connection.WindowsPhone";
        public const string STATE_CONNECTION_WINDOWS_SURFACE = "Connection.WindowsSurface";

        public const string STATE_MCP4921_VOLTAGE = "VoltageControl";
    }
}
