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
using System.Collections.ObjectModel; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

using CodeAbility.MonitorAndCommand.WPClient;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.ViewModels
{
    public class NetduinoViewModel : BaseViewModel
    {
        public bool blueLED = false;
        public bool BlueLED
        {
            get { return blueLED; }
            set
            {
                blueLED = value;
                OnPropertyChanged("BlueLED");
            }
        }

        public bool greenLED = false;
        public bool GreenLED
        {
            get { return greenLED; }
            set
            {
                greenLED = value;
                OnPropertyChanged("GreenLED");
            }
        }

        public bool redLED = false;
        public bool RedLED
        {
            get { return redLED; }
            set
            {
                redLED = value;
                OnPropertyChanged("RedLED");
            }
        }

        public string randomValue = String.Empty;
        public string RandomValue
        {
            get { return randomValue; }
            set
            {
                randomValue = value;
                OnPropertyChanged("RandomValue");
            }
        }


        MessageClient MessageClient { get; set; }

        public NetduinoViewModel()
        {
            MessageClient = App.Current.Resources["MessageClient"] as MessageClient;

            MessageClient.DataReceived += messageClient_DataReceived;
        }

        public void Subscribe()
        {
            if (MessageClient != null)
            { 
                MessageClient.SubscribeToData(Devices.NETDUINO, Netduino.OBJECT_BOARD_LED, Netduino.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.NETDUINO, Netduino.OBJECT_BUTTON, Netduino.DATA_BUTTON_STATUS);
                MessageClient.SubscribeToData(Devices.NETDUINO, Netduino.OBJECT_SENSOR, Netduino.DATA_SENSOR_RANDOM);

                MessageClient.SubscribeToData(Devices.NETDUINO, Netduino.OBJECT_RED_LED, Netduino.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.NETDUINO, Netduino.OBJECT_GREEN_LED, Netduino.DATA_LED_STATUS);

                MessageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }


        public void Unsubscribe()
        {
            if (MessageClient != null)
            {
                MessageClient.Unsubscribe(Devices.NETDUINO, Netduino.OBJECT_BOARD_LED, Netduino.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.NETDUINO, Netduino.OBJECT_BUTTON, Netduino.DATA_BUTTON_STATUS);
                MessageClient.Unsubscribe(Devices.NETDUINO, Netduino.OBJECT_SENSOR, Netduino.DATA_SENSOR_RANDOM);

                MessageClient.Unsubscribe(Devices.NETDUINO, Netduino.OBJECT_RED_LED, Netduino.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.NETDUINO, Netduino.OBJECT_GREEN_LED, Netduino.DATA_LED_STATUS);

                MessageClient.Unsubscribe(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        public void ButtonPushed()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO, Netduino.COMMAND_BUTTON_PRESSED, Netduino.OBJECT_BUTTON, Netduino.CONTENT_BUTTON_ON);
        }

        public void TurnRedLedOn()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_RED_LED, Netduino.CONTENT_LED_STATUS_ON);
        }

        public void TurnRedLedOff()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_RED_LED, Netduino.CONTENT_LED_STATUS_OFF);
        }

        public void TurnGreenLedOn()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_GREEN_LED, Netduino.CONTENT_LED_STATUS_ON);
        }

        public void TurnGreenLedOff()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_GREEN_LED, Netduino.CONTENT_LED_STATUS_OFF);
        }

        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.Name.Equals(Netduino.OBJECT_BOARD_LED))
                    BlueLED = e.Content.Equals(Netduino.CONTENT_LED_STATUS_ON);
                else if (e.Name.Equals(Netduino.OBJECT_RED_LED))
                    RedLED = e.Content.Equals(Netduino.CONTENT_LED_STATUS_ON);
                else if (e.Name.Equals(Netduino.OBJECT_GREEN_LED))
                    GreenLED = e.Content.Equals(Netduino.CONTENT_LED_STATUS_ON);
                else if (e.Name.Equals(Netduino.OBJECT_SENSOR))
                    RandomValue = e.Content.ToString();
            });
        }
    }
}
