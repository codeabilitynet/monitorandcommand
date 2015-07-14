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
using System.Threading;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.WPClient;
using CodeAbility.MonitorAndCommand.Environment;

using CodeAbility.MonitorAndCommand.WindowsPhoneController.Models;
using CodeAbility.MonitorAndCommand.WindowsPhoneController.Helpers;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        const string DEFAULT_IP_ADDRESS = "192.168.178.26"; 

        public string IpAddress 
        {
            get { return ApplicationSettings.IpAddress; }
            set { ApplicationSettings.IpAddress = value; }
        }

        public int PortNumber
        {
            get { return ApplicationSettings.PortNumber.Value; }
            set { ApplicationSettings.PortNumber = value; }
        }

        ObservableCollection<Device> devices;
        public ObservableCollection<Device> Devices 
        {
            get { return devices; }
            set
            {
                devices = value;
                OnPropertyChanged("Devices");
            }
        }

        Device device;
        public Device Device
        {
            get { return device; }
            set
            {
                device = value;
                OnPropertyChanged("Device");
            }
        }

        public MainPageViewModel()
        {
            IpAddress = !String.IsNullOrEmpty(ApplicationSettings.IpAddress) ? ApplicationSettings.IpAddress : DEFAULT_IP_ADDRESS;
            PortNumber = ApplicationSettings.PortNumber.HasValue ? ApplicationSettings.PortNumber.Value : 11000 ;
        }

        public void Start()
        {
            MessageClient messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            if (messageClient.Start(IpAddress, PortNumber))
            { 
                Devices = new ObservableCollection<Device>() { new Device(Environment.Devices.NETDUINO_PLUS, "Netduino"), 
                                                               new Device(Environment.Devices.PIBRELLA, "Pibrella") };
            }
        }
    }
}
