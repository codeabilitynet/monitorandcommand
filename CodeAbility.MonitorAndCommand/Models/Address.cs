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
