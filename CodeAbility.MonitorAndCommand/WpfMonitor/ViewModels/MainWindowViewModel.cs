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

using CodeAbility.MonitorAndCommand.Interfaces;
using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

using CodeAbility.MonitorAndCommand.WpfMonitor.Models;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.ViewModels
{
    class MainWindowViewModel : BaseViewModel, IDisposable
    {
        const int HEARTBEAT_PERIOD_IN_MILLESECONDS = 10000; 

        const int COMPUTATION_PERIOD = 1000;

        MessageClient messageClient;
        
        public PibrellaViewModel PibrellaViewModel { get; set; }

        public LEDsViewModel LEDsViewModel { get; set; }
        
        public DataGeneratorViewModel DataGeneratorViewModel { get; set; }
        
        public DS18B20ViewModel DS18B20ViewModel { get; set; }

        public MCP4921ViewModel MCP4921ViewModel { get; set; }

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
            messageClient = new MessageClient(Devices.WPF_MONITOR, HEARTBEAT_PERIOD_IN_MILLESECONDS);
            App.Current.Resources.Add("MessageClient", messageClient);

            PibrellaViewModel = new PibrellaViewModel();
            LEDsViewModel = new LEDsViewModel();
            DataGeneratorViewModel = new DataGeneratorViewModel();
            DS18B20ViewModel = new DS18B20ViewModel();
            MCP4921ViewModel = new MCP4921ViewModel();
        }

        public void Connect()
        {
            string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

            try
            {
                messageClient.Start(ipAddress, portNumber);

                //messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.RASPBERRY_B);
                //messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_3_WIFI);
                //messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_3_WIFI);
                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_3_WIFI);
                //messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.WINDOWS_CONSOLE);

                //PibrellaViewModel.Subscribe();
                //LEDsViewModel.Subscribe();
                //DataGeneratorViewModel.Subscribe();
                //DS18B20ViewModel.Subscribe();
                MCP4921ViewModel.Subscribe();

                Connected = true;
            }
            catch(Exception)
            {
                Connected = false;
            }
        }

        public void Close()
        {
            //TODO : implement Unsubscribe methods
            //PibrellaViewModel.Unsubscribe();
            //LEDsViewModel.Unsubscribe();
            //DataGeneratorViewModel.Unsubscribe();
            //DS18B20ViewModel.Unsubscribe();
            //MCP4921ViewModel.Unsubscribe();

            messageClient.Stop();
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            string toDevice = e.ToDevice;
            string fromDevice = e.FromDevice;                  
        }

        public void Dispose()
        {
            messageClient = null;
        }
    }
}
