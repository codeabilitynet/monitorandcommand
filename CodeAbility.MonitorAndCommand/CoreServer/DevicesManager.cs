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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Server
{
    internal class DevicesManager
    {
        List<Device> devices = new List<Device>();
        public List<Device> Devices { get { return devices; } }

        public DevicesManager() { }

        public void AddDevice(string name, Address address)
        {
            if (!Exists(name))
            {
                devices.Add(new Device(address, name));
            }
        }

        public void RemoveDevice(string name)
        {
            devices.RemoveAll(x => x.Name.Equals(name));
        }

        public Device GetDeviceFromName(string deviceName)
        {
            return devices.FirstOrDefault(x => x.Name == deviceName);
        }

        public Device GetDeviceFromAddress(Address address)
        {
            return devices.FirstOrDefault(x => x.Address.Equals(address));
        }

        public Address GetAddressFromDeviceName(string deviceName)
        {
            Device device = GetDeviceFromName(deviceName);
            return (device != null) ? device.Address : null;
        }

        public string GetDeviceNameFromAddress(Address address)
        {
            Device device = GetDeviceFromAddress(address);
            return (device != null) ? device.Name : null;
        }

        private bool Exists(string deviceName)
        {
            Device device = devices.FirstOrDefault(x => x.Name.Equals(deviceName));
            return (device != null);
        }

    }
}
