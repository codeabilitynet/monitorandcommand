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
            IpAddress = !String.IsNullOrEmpty(ApplicationSettings.IpAddress) ? ApplicationSettings.IpAddress : "192.168.178.26";
            PortNumber = ApplicationSettings.PortNumber.HasValue ? ApplicationSettings.PortNumber.Value : 11000 ;
        }

        public void Connect()
        {
            MessageClient messageClient = App.Current.Resources["MessageClient"] as MessageClient;

            if (messageClient.Start(IpAddress, PortNumber))
            { 
                Devices = new ObservableCollection<Device>() { new Device(Environment.Devices.NETDUINO, "Netduino"), 
                                                               new Device(Environment.Devices.PIBRELLA, "Pibrella") };
            }
        }
    }
}
