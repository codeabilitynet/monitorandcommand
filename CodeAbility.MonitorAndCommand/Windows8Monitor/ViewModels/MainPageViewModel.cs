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

using CodeAbility.MonitorAndCommand.Windows8Monitor.Models;
using CodeAbility.MonitorAndCommand.W8Client;
using CodeAbility.MonitorAndCommand.Environment;
using System.Collections.ObjectModel;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        const int LOAD_DATA_PERIOD_IN_SECONDS = 60;

        IEnumerable<SerieItem> voltageSerie;
        public IEnumerable<SerieItem> VoltageSerie
        {
            get { return voltageSerie; }
            set
            {
                voltageSerie = value;
                OnPropertyChanged("VoltageSerie");
            }
        }

        string realTimeVoltage;
        public string RealTimeVoltage
        {
            get { return realTimeVoltage; }
            set
            {
                realTimeVoltage = value;

                OnPropertyChanged("RealTimeVoltage");
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

            deviceModels.Add(new DeviceModel(Environment.Devices.NETDUINO_MCP4921));
            deviceModels.Add(new DeviceModel(Environment.Devices.PIBRELLA));
            deviceModels.Add(new DeviceModel(Environment.Devices.WINDOWS_PHONE));
            deviceModels.Add(new DeviceModel(Environment.Devices.WINDOWS_SURFACE));
        }

        public async void Start()
        {        
            MessageClient messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            messageClient.DataReceived += messageClient_DataReceived;

            if (await messageClient.Start(IpAddress, PortNumber))
            {
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
                messageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

                messageClient.SubscribeToServerState(ServerStates.STATE_NETDUINO_ISCONNECTED);
                messageClient.SubscribeToServerState(ServerStates.STATE_PIBRELLA_ISCONNECTED);
                messageClient.SubscribeToServerState(ServerStates.STATE_WINDOWSPHONE_ISCONNECTED);
                messageClient.SubscribeToServerState(ServerStates.STATE_SURFACE_ISCONNECTED);
                messageClient.SubscribeToServerState(ServerStates.STATE_VOLTAGE_CONTROL);

                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
                messageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        void messageClient_DataReceived(object sender, MonitorAndCommand.Models.MessageEventArgs e)
        {
            ReceivedData = e.Parameter.ToString(); 
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
            //voltageSerie = model.LoadVoltageSerie();
    
            //mainPage.SetChartsAxes();

            //VoltageSerie = voltageSerie;

            //if (VoltageSerie.Count() > 0)
            //    FirstVoltageData = String.Format("{0}V", Math.Round(VoltageSerie.First().Value, 2).ToString());
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
