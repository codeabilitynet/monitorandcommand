using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.Helpers
{
    public static class ApplicationSettings
    {

        private static string IP_ADDRESS = "IpAddress";
        public static string IpAddress 
        {
            get { return ReadSetting(IP_ADDRESS) != null ? ReadSetting(IP_ADDRESS).ToString() : String.Empty; }
            set { SaveSetting(IP_ADDRESS, value); }
        }

        private static string PORT_NUMBER = "PortNumber";
        public static int? PortNumber
        {
            get { return ReadSetting(PORT_NUMBER) != null ? Int32.Parse(ReadSetting(PORT_NUMBER).ToString()) : (int?)null; }
            set { SaveSetting(PORT_NUMBER, value); }
        }

        private static object ReadSetting(string name)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(name) ? IsolatedStorageSettings.ApplicationSettings[name] : null;
        }

        private static void SaveSetting(string name, object value)
        {
            IsolatedStorageSettings.ApplicationSettings[name] = value;
        }
    }
}
