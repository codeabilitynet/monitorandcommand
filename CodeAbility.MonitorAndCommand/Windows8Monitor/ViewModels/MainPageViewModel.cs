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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.UI.Core; 
using Windows.UI.Xaml;

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Windows8Monitor.Models;
using CodeAbility.MonitorAndCommand.W8Client;
using CodeAbility.MonitorAndCommand.Environment;
using System.Collections.ObjectModel;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        const int LOAD_DATA_PERIOD_IN_SECONDS = 1;

        ObservableCollection<SerieItem> voltageSerie;
        public ObservableCollection<SerieItem> VoltageSerie
        {
            get { return voltageSerie; }
            set
            {
                voltageSerie = value;
                OnPropertyChanged("VoltageSerie");
            }
        }

        string voltage;
        public string Voltage
        {
            get { return voltage; }
            set
            {
                voltage = value;

                OnPropertyChanged("Voltage");
            }
        }

        string receivedData;
        public string ReceivedData
        {
            get { return receivedData; }
            set
            {
                receivedData = value;

                OnPropertyChanged("ReceivedData");
            }
        }

        private bool redLED = false;
        public bool RedLED
        {
            get { return redLED; }
            set
            {
                redLED = value;
                OnPropertyChanged("RedLED");
            }
        }

        private bool yellowLED = false;
        public bool YellowLED
        {
            get { return yellowLED; }
            set
            {
                yellowLED = value;
                OnPropertyChanged("YellowLED");
            }
        }

        private bool greenLED = false;
        public bool GreenLED
        {
            get { return greenLED; }
            set
            {
                greenLED = value;
                OnPropertyChanged("GreenLED");
            }
        }

        VoltageModel voltageModel;

        MainPage mainPage;

        DispatcherTimer dispatcherTimer;

        const string DEFAULT_IP_ADDRESS = "192.168.178.26"; 

        public string IpAddress { get; set; }

        public int PortNumber { get; set; }

        List<DeviceModel> deviceModels = new List<DeviceModel>();
        public ObservableCollection<DeviceModel> DeviceModels
        {
            get { return new ObservableCollection<DeviceModel>(deviceModels); }
        }

        public MainPageViewModel(MainPage page)
        {
            mainPage = page;

            voltageModel = new VoltageModel();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, LOAD_DATA_PERIOD_IN_SECONDS);

            dispatcherTimer.Start();

            IpAddress = DEFAULT_IP_ADDRESS;
            PortNumber = 11000;

            deviceModels.Add(new DeviceModel(Environment.Devices.NETDUINO_3_WIFI));
            deviceModels.Add(new DeviceModel(Environment.Devices.RASPBERRY_PI_B));
            deviceModels.Add(new DeviceModel(Environment.Devices.WINDOWS_PHONE));
            //deviceModels.Add(new DeviceModel(Environment.Devices.WINDOWS_SURFACE));
        }

        public async void Start()
        {        
            MessageClient messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            messageClient.DataReceived += messageClient_DataReceived;

            if (await messageClient.Start(IpAddress, PortNumber))
            {
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

                messageClient.SubscribeToData(Message.ALL, MCP4921.OBJECT_ANALOG_DATA, MCP4921.DATA_ANALOG_VALUE);

                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_NETDUINO_3_WIFI);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_RASPBERRY_B);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_WINDOWS_PHONE);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_WINDOWS_SURFACE);
                messageClient.SubscribeToServerState(ServerStates.STATE_MCP4921_VOLTAGE);
            }
        }

        void messageClient_DataReceived(object sender, MonitorAndCommand.Models.MessageEventArgs e)
        { 
            string fromDevice = e.FromDevice;
            string dataName = e.Name;

            if (e.SendingDevice.Equals(Message.SERVER))
            {
                const string CONNECTION = "Connection";

                if (e.Parameter.ToString().StartsWith(CONNECTION))
                {
                    string stateName = e.Parameter.ToString();
                    string deviceName = stateName.Split('.')[1];

                    DeviceModel deviceModel = deviceModels.FirstOrDefault(x => x.Name == deviceName);
                    deviceModel.IsConnected = e.Content.Equals(ServerStates.ConnectionStates.Connected.ToString());
                }
            }
            
            DeviceModel sendingDeviceModel = deviceModels.FirstOrDefault(x => x.Name == e.SendingDevice);
            if (sendingDeviceModel != null)
                sendingDeviceModel.HandleSentMessageEvent();

            DeviceModel receivingDeviceModel = deviceModels.FirstOrDefault(x => x.Name == e.ReceivingDevice);
            if (receivingDeviceModel != null)
                receivingDeviceModel.HandleReceivedMessageEvent();

            if (fromDevice.Equals(Environment.Devices.RASPBERRY_PI_B))
            {
                if (dataName.Equals(Pibrella.OBJECT_RED_LED))
                {
                    RedLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
                }
                else if (dataName.Equals(Pibrella.OBJECT_YELLOW_LED))
                {
                    YellowLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
                }
                else if (dataName.Equals(Pibrella.OBJECT_GREEN_LED))
                {
                    GreenLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
                }
            }
            else if (e.FromDevice.Equals(Environment.Devices.NETDUINO_3_WIFI) || e.FromDevice.Equals(Message.SERVER))
            {
                if (dataName.Equals(Environment.MCP4921.OBJECT_ANALOG_DATA))
                {
                    Voltage = e.Content.ToString().Substring(0, 4);

                    voltageModel.EnqueueVoltage(Double.Parse(e.Content.ToString()), e.Timestamp);
                }   
            } 
        }


        void dispatcherTimer_Tick(object sender, object e)
        {
            LoadData();
        }

        public void StartDispatcherTimer()
        {

        }

        public void LoadData()
        {
            mainPage.SetChartsAxes();
            VoltageSerie = new ObservableCollection<SerieItem>(voltageModel.LoadVoltageSerie());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));                
            }
        }
    }
}
