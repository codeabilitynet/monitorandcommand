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
using System.Threading; 
using System.Threading.Tasks;

using Newtonsoft.Json;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.WPClient
{
    public class MessageClient
    {
        #region Event

        // A delegate type for hooking up change notifications.
        public delegate void DataReceivedEventHandler(object sender, MessageEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event DataReceivedEventHandler DataReceived;

        // Invoke the Changed event; called whenever list changes
        protected void OnDataReceived(MessageEventArgs e)
        {
            if (DataReceived != null)
                DataReceived(this, e);
        }

        #endregion 

        #region Properties
        
        string DeviceName { get; set; }

        string IpAddress { get; set; }

        int PortNumber { get; set; }

        #endregion 

        SocketClient client = new SocketClient();

        public MessageClient(string deviceName)
        {
            DeviceName = deviceName;

            client.DataStringReceived += client_DataStringReceived;
        }

        public MessageClient(string deviceName, string ipAddress, int portNumber) : this(deviceName)
        {
            IpAddress = ipAddress;
            PortNumber = portNumber;
        }

        void client_DataStringReceived(object sender, DataStringEventArgs e)
        {
            string serializedData = e.Data;
            int firstBraceIndex = serializedData.IndexOf('{');
            int lastBraceIndex = serializedData.LastIndexOf('}');
            string cleanedUpSerializedData = serializedData.Substring(firstBraceIndex, lastBraceIndex - firstBraceIndex + 1);

            Message message = JsonConvert.DeserializeObject<Message>(cleanedUpSerializedData);
            if (DataReceived != null)
                DataReceived(this, new MessageEventArgs(message));          
        }
        #region Public methods

        public bool Start(string ipAddress, int portNumber)
        {
            client.Connect(IpAddress, PortNumber);

            Register();

            return client.IsConnected; 
        }

        public void Stop()
        {
            client.Close();
        }

        public void Register()
        {
            Message message = Message.InstanciateRegisterMessage(DeviceName);
            Send(message);
        }

        public void Unregister()
        {
            Message message = Message.InstanciateUnregisterMessage(DeviceName);
            Send(message);
        }

        public void PublishCommand(string toDevice, string commandTarget, string commandName)
        {
            Message message = Message.InstanciatePublishMessage(DeviceName, toDevice, commandTarget, commandName);
            Send(message);
        }

        public void PublishData(string toDevice, string dataSource, string dataName)
        {
            Message message = Message.InstanciatePublishMessage(DeviceName, toDevice, dataSource, dataName);
            Send(message);
        }

        public void SubscribeToData(string fromDevice, string dataSource, string dataName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, dataSource, dataName);
            Send(message);
        }

        public void SubscribeToCommand(string fromDevice, string commandName, string commandTarget)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, commandName, commandTarget);
            Send(message);
        }

        public void Unsubscribe(string fromDevice, string element, string publicationName)
        {
            Message message = Message.InstanciateUnsubscribeMessage(DeviceName, fromDevice, DeviceName, element, publicationName);
            Send(message);
        }

        public void SendCommand(string toDevice, string commandName, string commandTarget, object commandContent)
        {
            Message message = Message.InstanciateCommandMessage(DeviceName, toDevice, commandName, commandTarget, commandContent);
            Send(message);
        }

        #endregion 

        protected void Send(Message message)
        {
            string serializedData = JsonConvert.SerializeObject(message);
            client.Send(serializedData);
        }

    }
}
