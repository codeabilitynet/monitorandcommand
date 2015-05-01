// .NET/Mono Monitor and Command Middleware for embedded projects.
// Copyright (C) 2015 Paul Gaunard (codeability.net)

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.ViewModels
{
    class MainPageViewModel : BaseViewModel
    {
        const int BLINK_TIME = 100;
        const string ALL = "*";

        #region Properties

        public bool pibrellaMessageSent = false;
        public bool PibrellaMessageSent
        {
            get { return pibrellaMessageSent; }
            set
            {
                pibrellaMessageSent = value;
                OnPropertyChanged("PibrellaMessageSent");
            }
        }

        public bool netduinoMessageSent = false;
        public bool NetduinoMessageSent
        {
            get { return netduinoMessageSent; }
            set
            {
                netduinoMessageSent = value;
                OnPropertyChanged("NetduinoMessageSent");
            }
        }

        public bool windowsPhoneMessageSent = false;
        public bool WindowsPhoneMessageSent
        {
            get { return windowsPhoneMessageSent; }
            set
            {
                windowsPhoneMessageSent = value;
                OnPropertyChanged("WindowsPhoneMessageSent");
            }
        }

        public bool pibrellaMessageReceived = false;
        public bool PibrellaMessageReceived
        {
            get { return pibrellaMessageReceived; }
            set
            {
                pibrellaMessageReceived = value;
                OnPropertyChanged("PibrellaMessageReceived");
            }
        }

        public bool netduinoMessageReceived = false;
        public bool NetduinoMessageReceived
        {
            get { return netduinoMessageReceived; }
            set
            {
                netduinoMessageReceived = value;
                OnPropertyChanged("NetduinoMessageReceived");
            }
        }

        public bool windowsPhoneMessageReceived = false;
        public bool WindowsPhoneMessageReceived
        {
            get { return windowsPhoneMessageReceived; }
            set
            {
                windowsPhoneMessageReceived = value;
                OnPropertyChanged("WindowsPhoneMessageReceived");
            }
        }

        #endregion 

        public MainPageViewModel()
        {                
            Connect();
        }

        public void Connect()
        {
            string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

            MessageClient messageClient = new MessageClient(Devices.WPF_MONITOR);

            messageClient.HealthInfoReceived += client_HealthInfoReceived;
            messageClient.DataReceived += client_MessageReceived;
            messageClient.CommandReceived += client_MessageReceived;

            messageClient.Start(ipAddress, portNumber);

            System.Threading.Thread.Sleep(10);

            messageClient.SubscribeToTraffic(Devices.PIBRELLA, Devices.WINDOWS_PHONE);
            messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.PIBRELLA);

            messageClient.SubscribeToTraffic(Devices.NETDUINO, Devices.WINDOWS_PHONE);
            messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO);

        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            string toDevice = e.ToDevice;
            string fromDevice = e.FromDevice;

            //From
            if (fromDevice.Equals(Devices.PIBRELLA))
                PibrellaMessageSentHandler();
            
            if (fromDevice.Equals(Devices.NETDUINO))
                NetduinoMessageSentHandler();
            
            if (fromDevice.Equals(Devices.WINDOWS_PHONE))
                WindowsPhoneMessageSentHandler();

            //To 
            if (toDevice.Equals(Devices.PIBRELLA))
                PibrellaMessageReceivedHandler();
            
            if (toDevice.Equals(Devices.NETDUINO))
                NetduinoMessageReceivedHandler();
            
            if (toDevice.Equals(Devices.WINDOWS_PHONE) || toDevice.Equals(ALL))
                WindowsPhoneMessageReceivedHandler();
        }


        void client_HealthInfoReceived(object sender, CodeAbility.MonitorAndCommand.Models.MessageEventArgs e)
        {
 
        }

        void PibrellaMessageSentHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(PibrellaMessageSentFlash);
            thread.Start();
        }

        void PibrellaMessageSentFlash()
        {
            PibrellaMessageSent = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            PibrellaMessageSent = false;
        }

        void PibrellaMessageReceivedHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(PibrellaMessageReceivedFlash);
            thread.Start();
        }

        void PibrellaMessageReceivedFlash()
        {
            PibrellaMessageReceived = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            PibrellaMessageReceived = false;
        }

        void NetduinoMessageSentHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(NetduinoMessageSentFlash);
            thread.Start();
        }

        private void NetduinoMessageSentFlash()
        {
            NetduinoMessageSent = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            NetduinoMessageSent = false;
        }

        void NetduinoMessageReceivedHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(NetduinoMessageReceivedFlash);
            thread.Start();
        }

        private void NetduinoMessageReceivedFlash()
        {
            NetduinoMessageReceived= true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            NetduinoMessageReceived = false;
        }

        void WindowsPhoneMessageSentHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(WindowsPhoneMessageSentFlash);
            thread.Start();
        }

        private void WindowsPhoneMessageSentFlash()
        {
            WindowsPhoneMessageSent = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            WindowsPhoneMessageSent = false;
        }

        void WindowsPhoneMessageReceivedHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(WindowsPhoneMessageReceivedFlash);
            thread.Start();
        }

        private void WindowsPhoneMessageReceivedFlash()
        {
            WindowsPhoneMessageReceived = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            WindowsPhoneMessageReceived = false;
        }

    }
}
