﻿/*
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

using CodeAbility.MonitorAndCommand.WpfMonitor.Models;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.ViewModels
{
    public class LEDsViewModel : BaseViewModel
    {
        MessageClient messageClient; 

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

        public LEDsViewModel() { }

        public void Subscribe()
        {
            messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            messageClient.DataReceived += client_MessageReceived;

            messageClient.SubscribeToTraffic(Devices.NETDUINO_3_WIFI, Devices.WINDOWS_PHONE);
            messageClient.SubscribeToTraffic(Devices.WINDOWS_PHONE, Devices.NETDUINO_3_WIFI);
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages from the NETDUINO
            string fromDevice = e.FromDevice;
            if (!fromDevice.Equals(Environment.Devices.NETDUINO_3_WIFI))
                return;

            if (e.Name.Equals(LEDs.OBJECT_BOARD_LED))
            {
                BlueLED = e.Content.Equals(LEDs.CONTENT_LED_STATUS_ON);
            }
            else if (e.Name.Equals(LEDs.OBJECT_RED_LED))
            {
                RedLED = e.Content.Equals(LEDs.CONTENT_LED_STATUS_ON);
            }
            else if (e.Name.Equals(LEDs.OBJECT_GREEN_LED))
            {
                GreenLED = e.Content.Equals(LEDs.CONTENT_LED_STATUS_ON);
            }
            else if (e.Name.Equals(LEDs.OBJECT_SENSOR))
            {
                RandomValue = e.Content.ToString().Substring(0,6);
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
            ButtonPressed = true;

            System.Threading.Thread.Sleep(BUTTON_PRESSED_DURATION);

            ButtonPressed = false;
        }
    }
}
