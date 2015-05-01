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
using System.Collections.ObjectModel; 
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using CodeAbility.MonitorAndCommand.WPClient;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.ViewModels
{
    public class PibrellaViewModel : BaseViewModel
    {
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

        public PibrellaViewModel()
        {
            MessageClient = App.Current.Resources["MessageClient"] as MessageClient;

            MessageClient.DataReceived += messageClient_DataReceived;
        }

        public void Subscribe()
        {
            if (MessageClient != null)
            {
                //Pibrella
                MessageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

                MessageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
                MessageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
                MessageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
                MessageClient.PublishCommand(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        public void Unsubscribe()
        {
            if (MessageClient != null)
            {
                //Pibrella
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.DATA_BUTTON_STATUS);

                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_GREEN_LED, Pibrella.COMMAND_TOGGLE_LED);
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_YELLOW_LED, Pibrella.COMMAND_TOGGLE_LED);
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_RED_LED, Pibrella.COMMAND_TOGGLE_LED);
                MessageClient.Unsubscribe(Devices.PIBRELLA, Pibrella.OBJECT_BUTTON, Pibrella.COMMAND_BUTTON_PRESSED);
            }
        }

        public void ButtonPushed()
        {
            if (MessageClient != null)
                MessageClient.SendCommand(Devices.PIBRELLA, Pibrella.COMMAND_BUTTON_PRESSED, Pibrella.OBJECT_BUTTON, Pibrella.CONTENT_BUTTON_ON);
        }

        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            string dataName = e.Name;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (dataName.Equals(Pibrella.OBJECT_RED_LED))
                    RedLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
                else if (dataName.Equals(Pibrella.OBJECT_YELLOW_LED))
                    YellowLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
                else if (dataName.Equals(Pibrella.OBJECT_GREEN_LED))
                    GreenLED = e.Content.Equals(Pibrella.CONTENT_LED_STATUS_ON);
                else if (dataName.Equals(Pibrella.DATA_BUTTON_STATUS))
                {
                    ButtonPressed = e.Content.Equals(Pibrella.CONTENT_BUTTON_ON);
                }
            });
        }
    }
}
