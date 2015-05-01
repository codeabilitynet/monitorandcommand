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
