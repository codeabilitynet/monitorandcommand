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

namespace CodeAbility.MonitorAndCommand.Models
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
