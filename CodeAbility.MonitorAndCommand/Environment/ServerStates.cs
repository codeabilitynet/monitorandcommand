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

        public const string STATE_CONNECTION_WINDOWS_PHONE = "Connection.Nokia Lumia 520";
        public const string STATE_CONNECTION_WINDOWS_SURFACE = "Connection.Surface RT";

        public const string STATE_CONNECTION_NETDUINO_3_WIFI = "Connection.Netduino 3 Wifi";

        public const string STATE_CONNECTION_RASPBERRY_B = "Connection.Raspberry Pi B";
        public const string STATE_CONNECTION_ANDROID_PHONE = "Connection.Android Phone";

        public const string STATE_CONNECTION_PHOTON_A = "Connection.Photon A";
        public const string STATE_CONNECTION_PHOTON_B = "Connection.Photon B";
        public const string STATE_CONNECTION_PHOTON_C = "Connection.Photon C";

        public const string STATE_CONNECTION_SERVER = "Connection.Server";
        
        public enum VoltageStates { Low, Standard, High, Danger }
        public enum PhotonsStates { Normal, Warning, Danger }

        public const string STATE_MCP4921_VOLTAGE = "VoltageControl";
    }
}
