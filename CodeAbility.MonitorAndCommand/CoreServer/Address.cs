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

namespace CodeAbility.MonitorAndCommand.Server
{
    public class Address
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public Address() { }

        public Address (string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public Address(string endpointString)
        {
            string[] tokens = endpointString.Split(':');
            Ip = tokens[0];
            Port = Int32.Parse(tokens[1]);
        }

        public override string ToString()
        {
            return Ip + ":" + Port.ToString();
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to Point return false.
            Address address = obj as Address;
            if ((System.Object)address == null)
                return false;

            // Return true if the fields match:
            return (this.Ip.Equals(address.Ip) && this.Port.Equals(address.Port));
        }

        public override int GetHashCode()
        {
            return Port;
        }
    }
}
