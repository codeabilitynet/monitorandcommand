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
        const int HANDLE_RECEIVED_EVENTS_PERIOD_IN_MILLISECONDS = 100;

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

        string voltage = "0.0";
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

        DispatcherTimer chartTimer;

        DispatcherTimer messagesTimer;

        Queue<MessageEventArgs> messagesReceived = new Queue<MessageEventArgs>();

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

            chartTimer = new DispatcherTimer();
            chartTimer.Tick += chartTimer_Tick;
            chartTimer.Interval = new TimeSpan(0, 0, LOAD_DATA_PERIOD_IN_SECONDS);

            chartTimer.Start();

            messagesTimer = new DispatcherTimer();
            messagesTimer.Tick += messagesTimer_Tick;
            messagesTimer.Interval = new TimeSpan(0, 0, 0, 0, HANDLE_RECEIVED_EVENTS_PERIOD_IN_MILLISECONDS);

            messagesTimer.Start();

            IpAddress = DEFAULT_IP_ADDRESS;
            PortNumber = 11000;

            deviceModels.Add(new DeviceModel(Environment.Devices.NETDUINO_3_WIFI));
            deviceModels.Add(new DeviceModel(Environment.Devices.RASPBERRY_PI_B));
            deviceModels.Add(new DeviceModel(Environment.Devices.WINDOWS_PHONE));
            deviceModels.Add(new DeviceModel(Environment.Devices.WINDOWS_SURFACE));
            deviceModels.Add(new DeviceModel(Environment.Devices.SERVER));
        }

        public async void Start()
        {        
            MessageClient messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            messageClient.MessageStringReceived += messageClient_MessageStringReceived;

            if (await messageClient.Start(IpAddress, PortNumber))
            {
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.RASPBERRY_PI_B, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);

                messageClient.SubscribeToData(Devices.NETDUINO_3_WIFI, MCP4921.OBJECT_ANALOG_DATA, MCP4921.DATA_ANALOG_VALUE);

                messageClient.SubscribeToTraffic(Devices.NETDUINO_3_WIFI, Devices.WINDOWS_PHONE);
                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_3_WIFI);

                messageClient.SubscribeToTraffic(Devices.RASPBERRY_PI_B, Devices.SERVER);
                messageClient.SubscribeToTraffic(Devices.SERVER, Devices.RASPBERRY_PI_B); 

                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_NETDUINO_3_WIFI);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_RASPBERRY_B);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_WINDOWS_PHONE);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_WINDOWS_SURFACE);
                messageClient.SubscribeToServerState(ServerStates.STATE_MCP4921_VOLTAGE);
            }

            DeviceModel serverModel = deviceModels.FirstOrDefault(x => x.Name == Devices.SERVER);
            serverModel.IsConnected = true;

            DeviceModel surfaceModel = deviceModels.FirstOrDefault(x => x.Name == Devices.WINDOWS_SURFACE);
            surfaceModel.IsConnected = true;
        }

        void messageClient_MessageStringReceived(object sender, MonitorAndCommand.Models.MessageEventArgs e)
        {
            lock(messagesReceived)
            { 
                messagesReceived.Enqueue(e);
            }
        }


        void chartTimer_Tick(object sender, object e)
        {
            LoadData();
        }

        public void LoadData()
        {
            mainPage.SetChartsAxes();
            VoltageSerie = new ObservableCollection<SerieItem>(voltageModel.LoadVoltageSerie());
        }

        void messagesTimer_Tick(object sender, object e)
        {
            while (messagesReceived.Count > 0)
            {
                MessageEventArgs message = null;
                lock (messagesReceived)
                {
                    message = messagesReceived.Dequeue();
                }
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(MessageEventArgs e)
        {
            string fromDevice = e.FromDevice;
            string dataName = e.Name;

            const string CONNECTION = "Connection";

            if (e.SendingDevice.Equals(Devices.SERVER) && e.Parameter.ToString().StartsWith(CONNECTION))
            {
                HandleConnectionMessage(e);
            }

            DeviceModel sendingDeviceModel = deviceModels.FirstOrDefault(x => x.Name == e.SendingDevice);
            if (sendingDeviceModel != null)
                sendingDeviceModel.HandleSentMessageEvent();

            DeviceModel receivingDeviceModel = deviceModels.FirstOrDefault(x => x.Name == e.ReceivingDevice || x.Name == e.ToDevice);
            if (receivingDeviceModel != null)
                receivingDeviceModel.HandleReceivedMessageEvent();

            if (fromDevice.Equals(Devices.RASPBERRY_PI_B))
            {
                HandleRaspberryPiBMessage(e);
            }
            else if (e.FromDevice.Equals(Devices.NETDUINO_3_WIFI) || e.FromDevice.Equals(Devices.SERVER))
            {
                HandleNetduino3WifiMessage(e);
            } 
        }

        private void HandleConnectionMessage(MessageEventArgs e)
        {
            string stateName = e.Parameter.ToString();
            string deviceName = stateName.Split('.')[1];

            DeviceModel deviceModel = deviceModels.FirstOrDefault(x => x.Name == deviceName);
            deviceModel.IsConnected = e.Content.Equals(ServerStates.ConnectionStates.Connected.ToString());
        }

        private void HandleRaspberryPiBMessage(MessageEventArgs e)
        {
            string dataName = e.Name;

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

        private void HandleNetduino3WifiMessage(MessageEventArgs e)
        {
            string dataName = e.Name;

            if (dataName.Equals(Environment.MCP4921.OBJECT_ANALOG_DATA))
            {
                const int MAX_LENGTH = 4;

                string voltageString = e.Content.ToString();
                int stringLength = voltageString.Length;
                int maxLength = (stringLength < MAX_LENGTH) ? stringLength : MAX_LENGTH;

                Voltage = e.Content.ToString().Substring(0, maxLength).PadRight(MAX_LENGTH, '0');

                voltageModel.EnqueueVoltage(Double.Parse(e.Content.ToString()), e.Timestamp);
            }
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
