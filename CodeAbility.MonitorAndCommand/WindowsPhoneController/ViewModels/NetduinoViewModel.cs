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

        private bool buttonPressed = false;
        public bool ButtonPressed
        {
            get { return buttonPressed; }
            set
            {
                buttonPressed = value;
                OnPropertyChanged("ButtonPressed");
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
                MessageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_BOARD_LED, Netduino.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_BUTTON, Netduino.DATA_BUTTON_STATUS);
                MessageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_SENSOR, Netduino.DATA_SENSOR_RANDOM);

                MessageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_RED_LED, Netduino.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.NETDUINO_PLUS, Netduino.OBJECT_GREEN_LED, Netduino.DATA_LED_STATUS);

                MessageClient.PublishCommand(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        public void Unsubscribe()
        {
            if (MessageClient != null)
            {
                MessageClient.Unsubscribe(Devices.NETDUINO_PLUS, Netduino.OBJECT_BOARD_LED, Netduino.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.NETDUINO_PLUS, Netduino.OBJECT_BUTTON, Netduino.DATA_BUTTON_STATUS);
                MessageClient.Unsubscribe(Devices.NETDUINO_PLUS, Netduino.OBJECT_SENSOR, Netduino.DATA_SENSOR_RANDOM);

                MessageClient.Unsubscribe(Devices.NETDUINO_PLUS, Netduino.OBJECT_RED_LED, Netduino.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.NETDUINO_PLUS, Netduino.OBJECT_GREEN_LED, Netduino.DATA_LED_STATUS);

                MessageClient.Unsubscribe(Devices.ALL, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        public void ButtonPushed()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_BUTTON_PRESSED, Netduino.OBJECT_BUTTON, Netduino.CONTENT_BUTTON_PRESSED);
        }

        public void TurnRedLedOn()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_RED_LED, Netduino.CONTENT_LED_STATUS_ON);
        }

        public void TurnRedLedOff()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_RED_LED, Netduino.CONTENT_LED_STATUS_OFF);
        }

        public void TurnGreenLedOn()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_GREEN_LED, Netduino.CONTENT_LED_STATUS_ON);
        }

        public void TurnGreenLedOff()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO_PLUS, Netduino.COMMAND_TOGGLE_LED, Netduino.OBJECT_GREEN_LED, Netduino.CONTENT_LED_STATUS_OFF);
        }

        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages from the NETDUINO
            if (!e.FromDevice.Equals(Environment.Devices.NETDUINO_PLUS))
                return;

            if (e.Name.Equals(Netduino.OBJECT_BOARD_LED))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    BlueLED = e.Content.Equals(Netduino.CONTENT_LED_STATUS_ON);
                });
            }  
            else if (e.Name.Equals(Netduino.OBJECT_RED_LED))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    RedLED = e.Content.Equals(Netduino.CONTENT_LED_STATUS_ON);
                });
            }
            else if (e.Name.Equals(Netduino.OBJECT_GREEN_LED))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    GreenLED = e.Content.Equals(Netduino.CONTENT_LED_STATUS_ON);
                });
            }
            else if (e.Name.Equals(Netduino.OBJECT_SENSOR))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    RandomValue = e.Content.ToString();
                });
            }
            else if (e.Name.Equals(Pibrella.OBJECT_BUTTON))
            {
                SimulatorButtonPressure();
            }
        }

        void SimulatorButtonPressure()
        {
            System.Threading.Thread thread = new System.Threading.Thread(ButtonPressedSimulator);
            thread.Start();
        }

        const int BUTTON_PRESSED_DURATION = 250;
        void ButtonPressedSimulator()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ButtonPressed = true;
            });

            System.Threading.Thread.Sleep(BUTTON_PRESSED_DURATION);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ButtonPressed = false;
            });
        }
    }
}
