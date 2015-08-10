/*
 * Copyright (c) 2015, Paul Gaunard (www.codeability.net)
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
using System.Collections.ObjectModel; 
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

using CodeAbility.MonitorAndCommand.WpfMonitor.Models;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        const int COMPUTATION_PERIOD = 1000;

        MessageClient messageClient;
        
        public PibrellaViewModel PibrellaViewModel { get; set; }

        public NetduinoPlusViewModel NetduinoPlusViewModel { get; set; }
        
        public DataGeneratorViewModel DataGeneratorViewModel { get; set; }
        
        public TemperatureSensorViewModel TemperatureSensorViewModel { get; set; }

        protected List<DeviceData> devicesData = new List<DeviceData>();
        public ObservableCollection<DeviceData> DevicesData
        {
            get { return new ObservableCollection<DeviceData>(devicesData); }
        }

        bool? connected = null;
        public bool? Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                OnPropertyChanged("Connected");
            }
        }

        public MainWindowViewModel()
        {
            messageClient = new MessageClient(Devices.WPF_MONITOR);
            App.Current.Resources.Add("MessageClient", messageClient);

            PibrellaViewModel = new PibrellaViewModel();
            NetduinoPlusViewModel = new NetduinoPlusViewModel();
            DataGeneratorViewModel = new DataGeneratorViewModel();
            TemperatureSensorViewModel = new TemperatureSensorViewModel(); 
        }

        public void Connect()
        {
            string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

            try
            {
                messageClient.Start(ipAddress, portNumber);

                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.PIBRELLA);
                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_PLUS);
                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_3);
                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.DATA_GENERATOR);

                PibrellaViewModel.Subscribe();
                NetduinoPlusViewModel.Subscribe();
                DataGeneratorViewModel.Subscribe();
                TemperatureSensorViewModel.Subscribe();

                Connected = true;
            }
            catch(Exception)
            {
                Connected = false;
            }
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            string toDevice = e.ToDevice;
            string fromDevice = e.FromDevice;                  
        }   
    }
}
