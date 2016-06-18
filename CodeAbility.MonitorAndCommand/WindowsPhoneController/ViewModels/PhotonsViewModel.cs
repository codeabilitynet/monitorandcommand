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
    public class PhotonsViewModel : BaseViewModel
    {
        public Models.Photon PhotonAModel { get; set; }
        public Models.Photon PhotonBModel { get; set; }
        public Models.Photon PhotonCModel { get; set; }

        MessageClient MessageClient { get; set; }

        public PhotonsViewModel()
        {
            MessageClient = App.Current.Resources["MessageClient"] as MessageClient;

            MessageClient.MessageStringReceived += messageClient_DataReceived;

            PhotonAModel = new Models.Photon(Devices.PHOTON_A);
            PhotonBModel = new Models.Photon(Devices.PHOTON_B);
            PhotonCModel = new Models.Photon(Devices.PHOTON_C); 
        }

        public void Subscribe()
        {
            if (MessageClient != null)
            {
                MessageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                MessageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                MessageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);

                MessageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                MessageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                MessageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                
                MessageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                MessageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                MessageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                MessageClient.SubscribeToData(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);

                MessageClient.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
                MessageClient.PublishCommand(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED);

                MessageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
                MessageClient.PublishCommand(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED);

                MessageClient.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED);
                MessageClient.PublishCommand(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED); 
            }
        }

        public void Unsubscribe()
        {
            if (MessageClient != null)
            {
                MessageClient.Unsubscribe(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                MessageClient.Unsubscribe(Devices.PHOTON_A, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                MessageClient.Unsubscribe(Devices.PHOTON_A, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PHOTON_A, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PHOTON_A, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);

                MessageClient.Unsubscribe(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                MessageClient.Unsubscribe(Devices.PHOTON_B, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                MessageClient.Unsubscribe(Devices.PHOTON_B, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PHOTON_B, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PHOTON_B, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);

                MessageClient.Unsubscribe(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_HUMIDITY);
                MessageClient.Unsubscribe(Devices.PHOTON_C, Photon.OBJECT_SENSOR, Photon.DATA_SENSOR_TEMPERATURE);
                MessageClient.Unsubscribe(Devices.PHOTON_C, Photon.OBJECT_GREEN_LED, Photon.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PHOTON_C, Photon.OBJECT_RED_LED, Photon.DATA_LED_STATUS);
                MessageClient.Unsubscribe(Devices.PHOTON_C, Photon.OBJECT_BOARD_LED, Photon.DATA_LED_STATUS);
            }
        }

        public void ToggleRedLed(string device)
        {
            if (MessageClient != null)
            {
                MessageClient.SendCommand(device, Photon.OBJECT_RED_LED, Photon.COMMAND_TOGGLE_LED, String.Empty);
            }
        }

        public void ToggleGreenLed(string device)
        {
            if (MessageClient != null)
            {
                MessageClient.SendCommand(device, Photon.OBJECT_GREEN_LED, Photon.COMMAND_TOGGLE_LED, String.Empty);
            }
        }
        
        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages from Photon devices
            if (!(e.FromDevice.Equals(Environment.Devices.PHOTON_A) || 
                  e.FromDevice.Equals(Environment.Devices.PHOTON_B) || 
                  e.FromDevice.Equals(Environment.Devices.PHOTON_C)))
                return;

            Models.Photon model = SelectModel(e.FromDevice);
            string objectName = e.Name;
            string parameter = e.Parameter.ToString();

            if (objectName.Equals(Photon.OBJECT_SENSOR))
            {
                if (parameter.Equals(Photon.DATA_SENSOR_TEMPERATURE))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        model.Temperature = e.Content.ToString();
                    });
                }
                else if (parameter.Equals(Photon.DATA_SENSOR_HUMIDITY))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                         model.Humidity = e.Content.ToString();
                    });
                }
            }
            else if (objectName.Equals(Photon.OBJECT_BOARD_LED))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    model.BoardLED = e.Content.ToString().Equals(Photon.CONTENT_LED_STATUS_ON);
                });
            }
            else if (objectName.Equals(Photon.OBJECT_GREEN_LED))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    model.GreenLED = e.Content.ToString().Equals(Photon.CONTENT_LED_STATUS_ON);
                });
            }
            else if (objectName.Equals(Photon.OBJECT_RED_LED))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    model.RedLED = e.Content.ToString().Equals(Photon.CONTENT_LED_STATUS_ON);
                });
            }   
        }

        Models.Photon SelectModel(string deviceName)
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
    }
}
