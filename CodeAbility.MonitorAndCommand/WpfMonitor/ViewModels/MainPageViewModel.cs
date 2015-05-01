/*
 * Copyright (c) 2015, Paul Gaunard (codeability.net)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the 
 *  documentation and/or other materials provided with the distribution.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


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
