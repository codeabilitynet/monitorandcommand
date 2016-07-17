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
using System.Threading; 
using System.Threading.Tasks;

using Newtonsoft.Json;

using CodeAbility.MonitorAndCommand.Interfaces;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.W8Client
{
    public class MessageClient : IMessageClient
    {
        #region Event

        // A delegate type for hooking up change notifications.
        public delegate void MessageStringReceivedEventHandler(object sender, MessageEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event MessageStringReceivedEventHandler MessageStringReceived;

        // Invoke the Changed event; called whenever list changes
        protected void OnMessageStringReceived(MessageEventArgs e)
        {
            if (MessageStringReceived != null)
                MessageStringReceived(this, e);
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

            client.MessageStringReceived += client_MessageStringReceived;
        }

        //public MessageClient(string deviceName, string ipAddress, int portNumber) : this(deviceName)
        //{
        //    IpAddress = ipAddress;
        //    PortNumber = portNumber;
        //}

        void client_MessageStringReceived(object sender, MessageStringEventArgs e)
        {
            try
            { 
                string paddedSerializedMessage = e.MessageString;

                string cleanedUpSerializedMessage = CodeAbility.MonitorAndCommand.Helpers.JsonHelpers.CleanUpPaddedSerializedMessage(paddedSerializedMessage);
                Message message = JsonConvert.DeserializeObject<Message>(cleanedUpSerializedMessage);

                if (MessageStringReceived != null)
                    MessageStringReceived(this, new MessageEventArgs(message));        
            }
            catch(Exception exception)
            {

            }
        }

        #region Public methods

        public async Task<bool> Start(string ipAddress, int portNumber)
        {
            IpAddress = ipAddress;
            PortNumber = portNumber;

            await client.Connect(IpAddress, PortNumber);

            Register(DeviceName);

            return client.IsConnected; 
        }

        public void Stop()
        {
            Unregister(DeviceName);

            client.Close();
        }

        public void Register(string deviceName)
        {
            Message message = Message.InstanciateRegisterMessage(deviceName);
            Send(message);
        }

        public void Unregister(string deviceName)
        {
            Message message = Message.InstanciateUnregisterMessage(deviceName);
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

        public void SubscribeToCommand(string fromDevice, string commandTarget, string commandName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, commandTarget, commandName);
            Send(message);
        }

        public void SubscribeToTraffic(string fromDevice, string toDevice)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, toDevice);
            Send(message);
        }

        public void SubscribeToServerState(string stateName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, Message.SERVER, DeviceName, Message.SERVER, stateName);
            Send(message);
        }

        public void Unsubscribe(string fromDevice, string element, string publicationName)
        {
            Message message = Message.InstanciateUnsubscribeMessage(DeviceName, fromDevice, DeviceName, element, publicationName);
            Send(message);
        }

        public void SendData(string toDevice, string dataName, string dataSource, object dataValue)
        {
            Message message = Message.InstanciateDataMessage(DeviceName, toDevice, dataName, dataSource, dataValue);
            Send(message);
        }

        public void SendCommand(string toDevice, string commandTarget, string commandName, object commandContent)
        {
            Message message = Message.InstanciateCommandMessage(DeviceName, toDevice, commandTarget, commandName, commandContent);
            Send(message);
        }

        #endregion 

        protected async void Send(Message message)
        {
            string serializedData = JsonConvert.SerializeObject(message);
            await client.Send(serializedData);
        }

    }
}
