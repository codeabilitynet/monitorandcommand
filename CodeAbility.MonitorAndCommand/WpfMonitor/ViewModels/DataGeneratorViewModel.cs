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
    public class DataGeneratorViewModel : BaseViewModel
    {
        MessageClient messageClient; 

        private string randomData;
        public string RandomData
        {
            get { return randomData; }
            set
            {
                randomData = value;
                OnPropertyChanged("RandomData");
            }
        }

        public DataGeneratorViewModel() { }

        public void Subscribe()
        {
            messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            messageClient.DataReceived += client_MessageReceived;
            //messageClient.CommandReceived += client_MessageReceived;

            messageClient.SubscribeToTraffic(Devices.WINDOWS_CONSOLE, Devices.ALL);
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            //Only consider the messages from the NETDUINO
            string fromDevice = e.FromDevice;
            if (!fromDevice.Equals(Environment.Devices.WINDOWS_CONSOLE))
                return;

            if (e.Name.Equals(Environment.DataGenerator.OBJECT_GENERATOR))
            {
                RandomData = e.Content.ToString().Substring(0,8);
            }

        }
    }
}
