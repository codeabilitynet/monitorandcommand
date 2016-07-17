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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment;

using CodeAbility.MonitorAndCommand.WpfMonitor.Models;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.ViewModels
{
    public class MCP4921ViewModel : BaseViewModel
    {
        private string voltage = String.Empty;
        public string Voltage
        {
            get { return voltage; }
            set
            {
                voltage = value;
                OnPropertyChanged("Voltage");
            }
        }

        bool connected = false; 
        public bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
            }
        }

        MessageClient MessageClient { get; set; }

        public MCP4921ViewModel()
        {
            MessageClient = App.Current.Resources["MessageClient"] as MessageClient;

            MessageClient.DataReceived += messageClient_DataReceived;
        }

        public void Subscribe()
        {
            if (MessageClient != null)
            {
                MessageClient.SubscribeToData(Devices.NETDUINO_3_WIFI, MCP4921.OBJECT_ANALOG_DATA, MCP4921.DATA_ANALOG_VALUE);

                MessageClient.PublishCommand(Devices.NETDUINO_3_WIFI, MCP4921.OBJECT_DIGITAL_DATA, MCP4921.COMMAND_CONVERT);
        
                Connected = true; 
            }
        }

        public void Unsubscribe()
        {
            if (MessageClient != null)
            {
                MessageClient.Unsubscribe(Devices.NETDUINO_3_WIFI, MCP4921.OBJECT_ANALOG_DATA, MCP4921.DATA_ANALOG_VALUE);

                MessageClient.Unsubscribe(Devices.NETDUINO_3_WIFI, MCP4921.OBJECT_DIGITAL_DATA, MCP4921.COMMAND_CONVERT);

                Connected = false; 
            }
        }

        public void SendControlValue(double value)
        {
            int data = (int)value;

            if (MessageClient != null)
                MessageClient.SendCommand(Devices.NETDUINO_3_WIFI, MCP4921.OBJECT_DIGITAL_DATA, MCP4921.COMMAND_CONVERT, data.ToString());
        }

        void messageClient_DataReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages from NETDUINO_MCP4921
            if (!e.FromDevice.Equals(Environment.Devices.NETDUINO_3_WIFI))
                return;

            string dataName = e.Name;

            if (dataName.Equals(Environment.MCP4921.OBJECT_ANALOG_DATA))
            {                   
                Voltage = e.Content.ToString().Substring(0, 4); 
            }          
        }
    }
}
