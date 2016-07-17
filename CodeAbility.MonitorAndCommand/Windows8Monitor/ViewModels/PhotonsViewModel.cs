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
    public class PhotonsViewModel : INotifyPropertyChanged
    {
        const int LOAD_DATA_PERIOD_IN_SECONDS = 1;
        const int HANDLE_RECEIVED_EVENTS_PERIOD_IN_MILLISECONDS = 100;

        public PhotonModel PhotonAModel { get; set; }
        public PhotonModel PhotonBModel { get; set; }
        public PhotonModel PhotonCModel { get; set; }
        public PibrellaModel PibrellaModel { get; set; }

        public Pages.Photons mainPage;

        public string IpAddress { get; set; }
        public int PortNumber { get; set; }

        DispatcherTimer messagesTimer;
        Queue<MessageEventArgs> messagesReceived = new Queue<MessageEventArgs>();

        List<DeviceModel> deviceModels = new List<DeviceModel>();
        public ObservableCollection<DeviceModel> DeviceModels
        {
            get { return new ObservableCollection<DeviceModel>(deviceModels); }
        }

        public PhotonsViewModel(Pages.Photons page)
        {
            mainPage = page;

            messagesTimer = new DispatcherTimer();
            messagesTimer.Tick += messagesTimer_Tick;
            messagesTimer.Interval = new TimeSpan(0, 0, 0, 0, HANDLE_RECEIVED_EVENTS_PERIOD_IN_MILLISECONDS);

            messagesTimer.Start();

            IpAddress = App.Current.Resources["IpAddress"].ToString();
            PortNumber = 11000;

            PhotonAModel = new PhotonModel(Devices.PHOTON_A);
            PhotonBModel = new PhotonModel(Devices.PHOTON_B);
            PhotonCModel = new PhotonModel(Devices.PHOTON_C);
            PibrellaModel = new PibrellaModel(); 

            deviceModels.Add(new DeviceModel(Devices.PHOTON_A));
            deviceModels.Add(new DeviceModel(Devices.PHOTON_B));
            deviceModels.Add(new DeviceModel(Devices.PHOTON_C));

            deviceModels.Add(new DeviceModel(Devices.RASPBERRY_PI_B));
            deviceModels.Add(new DeviceModel(Devices.ANDROID_PHONE));

            deviceModels.Add(new DeviceModel(Devices.WINDOWS_PHONE));
            deviceModels.Add(new DeviceModel(Devices.WINDOWS_SURFACE));
            
            deviceModels.Add(new DeviceModel(Devices.SERVER));
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

                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);

                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
                messageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);

                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
                messageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);

                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_RED);
                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_GREEN);
                messageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RGB_LED, Photon.DATA_RGB_BLUE);

                messageClient.SubscribeToTraffic(Devices.RASPBERRY_PI_B, Devices.SERVER);
                messageClient.SubscribeToTraffic(Devices.SERVER, Devices.RASPBERRY_PI_B);

                messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.PHOTON_A);
                messageClient.SubscribeToTraffic(Devices.PHOTON_A, Devices.WINDOWS_PHONE); 

                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_PHOTON_A);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_PHOTON_B);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_PHOTON_C);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_RASPBERRY_B);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_ANDROID_PHONE);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_WINDOWS_PHONE);
                messageClient.SubscribeToServerState(ServerStates.STATE_CONNECTION_WINDOWS_SURFACE);
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
            else if (fromDevice.Equals(Devices.PHOTON_A) || fromDevice.Equals(Devices.PHOTON_B) || fromDevice.Equals(Devices.PHOTON_C))
            {
                HandlePhotonsMessage(e);
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
                PibrellaModel.RedLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
            }
            else if (dataName.Equals(Pibrella.OBJECT_YELLOW_LED))
            {
                PibrellaModel.YellowLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
            }
            else if (dataName.Equals(Pibrella.OBJECT_GREEN_LED))
            {
                PibrellaModel.GreenLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
            } 
        }

        private void HandlePhotonsMessage(MessageEventArgs e)
        {
            PhotonModel model = SelectModel(e.FromDevice);             
            string objectName = e.Name;
            string parameter = e.Parameter.ToString();

            if (objectName.Equals(Photon.OBJECT_SENSOR))
            {
                if (parameter.Equals(Photon.DATA_SENSOR_TEMPERATURE))
                {
                    model.Temperature = e.Content.ToString();
                }
                else if (parameter.Equals(Photon.DATA_SENSOR_HUMIDITY))
                {
                    model.Humidity = e.Content.ToString();
                }
            }
            else if (objectName.Equals(Photon.OBJECT_BOARD_LED))
            {
                model.BoardLED = e.Content.ToString().Equals(Photon.CONTENT_LED_STATUS_ON);
            }
            else if (objectName.Equals(Photon.OBJECT_RGB_LED))
            {
                if (parameter.Equals(Photon.DATA_RGB_RED))
                    model.RGBRed = Int32.Parse(e.Content.ToString());
                else if (parameter.Equals(Photon.DATA_RGB_GREEN))
                    model.RGBGreen = Int32.Parse(e.Content.ToString());
                else if (parameter.Equals(Photon.DATA_RGB_BLUE))
                    model.RGBBlue = Int32.Parse(e.Content.ToString());
            }
            else if (objectName.Equals(Photon.OBJECT_GREEN_LED))
            {
                model.GreenLED = e.Content.ToString().Equals(Photon.CONTENT_LED_STATUS_ON);
            }    
            else if (objectName.Equals(Photon.OBJECT_RED_LED)) 
            {
                model.RedLED = e.Content.ToString().Equals(Photon.CONTENT_LED_STATUS_ON);
            }            
        }

        PhotonModel SelectModel(string deviceName)
        {
            switch (deviceName)
            {
                case Devices.PHOTON_A:
                    return PhotonAModel;
                case Devices.PHOTON_B:
                    return PhotonBModel;
                case Devices.PHOTON_C:
                    return PhotonCModel;
            }

            return null;
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
