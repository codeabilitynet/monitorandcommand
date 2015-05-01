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
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Helpers;

namespace CodeAbility.MonitorAndCommand.Client
{
    //https://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx

    public class MessageClient
    {
        #region Events

        public delegate void CommandReceivedEventHandler(object sender, MessageEventArgs e);

        public event CommandReceivedEventHandler CommandReceived;

        protected void OnCommandReceived(MessageEventArgs e)
        {
            if (CommandReceived != null)
                CommandReceived(this, e);
        }

        public delegate void DataReceivedEventHandler(object sender, MessageEventArgs e);

        public event DataReceivedEventHandler DataReceived;

        protected void OnDataReceived(MessageEventArgs e)
        {
            if (DataReceived != null)
                DataReceived(this, e);
        }

        public delegate void HealthInfoReceivedEventHandler(object sender, MessageEventArgs e);

        public event HealthInfoReceivedEventHandler HealthInfoReceived;

        protected void OnHealthInfoReceived(MessageEventArgs e)
        {
            if (HealthInfoReceived != null)
                HealthInfoReceived(this, e);
        }

        #endregion 

        #region Properties

        public bool IsConnected { get { return client != null && client.Connected; } }

        string DeviceName { get; set; }

        string IpAddress { get; set; }

        int PortNumber { get; set; }

        #endregion 

        private Socket client = null;

        private ConcurrentQueue<Message> messagesToSend = new ConcurrentQueue<Message>();

        //Threads
        private Thread receiveThread = null;
        private Thread sendThread = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        public MessageClient(string deviceName)
        {
            DeviceName = deviceName;

            receiveThread = new Thread(new ThreadStart(Receiver));
            sendThread = new Thread(new ThreadStart(Sender));
        }

        #region Public Methods

        public void Start(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            PortNumber = port;

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.GetHostEntry(IpAddress);
                IPAddress _ipAddress = ipHostInfo.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
                IPEndPoint remoteEP = new IPEndPoint(_ipAddress, PortNumber);

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.Connect(remoteEP);

                receiveThread.Start();
                sendThread.Start();
                
                Register();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Stop()
        {
            Unregister();

            Thread.Sleep(1000);

            receiveThread.Abort();
            client.Shutdown(SocketShutdown.Both);
            sendThread.Abort();
        }

        public void PublishCommand(string toDevice, string commandTarget, string commandName)
        {
            Message message = Message.InstanciatePublishMessage(DeviceName, toDevice, commandTarget, commandName);
            EnqueueMessage(message);
        }

        public void PublishData(string toDevice, string dataSource, string dataName)
        {
            Message message = Message.InstanciatePublishMessage(DeviceName, toDevice, dataSource, dataName);
            EnqueueMessage(message);
        }

        public void SubscribeToData(string fromDevice, string dataSource, string dataName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, dataSource, dataName);
            EnqueueMessage(message);
        }

        public void SubscribeToCommand(string fromDevice, string commandName, string commandTarget)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, commandName, commandTarget);
            EnqueueMessage(message);
        }

        public void SubscribeToTraffic(string fromDevice, string toDevice)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, toDevice);
            EnqueueMessage(message);
        }

        public void Unsubscribe(string fromDevice, string publicationElement, string publicationName)
        {
            Message message = Message.InstanciateUnsubscribeMessage(DeviceName, fromDevice, DeviceName, publicationElement, publicationName);
            EnqueueMessage(message);
        }

        public void SendCommand(string toDevice, string commandName, string commandTarget, object commandValue)
        {
            Message message = Message.InstanciateCommandMessage(DeviceName, toDevice, commandName, commandTarget, commandValue);
            EnqueueMessage(message);
        }

        public void SendData(string toDevice, string dataName, string dataSource, object dataValue)
        {
            Message message = Message.InstanciateDataMessage(DeviceName, toDevice, dataName, dataSource, dataValue);
            messagesToSend.Enqueue(message);

            sendDone.Set();
        }

        #endregion 

        #region Internals

        public void Register()
        {
            Message message = Message.InstanciateRegisterMessage(DeviceName);
            EnqueueMessage(message);
        }

        public void Unregister()
        {
            Message message = Message.InstanciateUnregisterMessage(DeviceName);
            EnqueueMessage(message);
        }

        protected void EnqueueMessage(Message message)
        {
            messagesToSend.Enqueue(message);

            sendDone.Set();
        }

        private void Sender()
        {
            while (client.Connected)
            {
                try
                {
                    sendDone.Reset();

                    if (messagesToSend.Count > 0)
                    {
                        Message message = null;
                        if (messagesToSend.TryDequeue(out message))
                            Send(message);
                    }
                    else
                        sendDone.WaitOne();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        private void Send(Message message)
        {
            string serializedMessage = JsonConvert.SerializeObject(message);
            string paddedSerializedData = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);
            byte[] byteData = Encoding.UTF8.GetBytes(paddedSerializedData);

            client.Send(byteData, 0, byteData.Length, 0);

            Console.WriteLine(message);
        }

        private void Receiver()
        {
            while (client.Connected)
            {
                try
                {
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];

                    client.Receive(buffer, 0, Constants.BUFFER_SIZE, 0);
                    string paddedSerializedData = Encoding.UTF8.GetString(buffer, 0, Constants.BUFFER_SIZE);
                    string serializedMessage = JsonHelpers.CleanUpSerializedData(paddedSerializedData);

                    Message message = JsonConvert.DeserializeObject<Message>(serializedMessage);
                    if (message != null)
                    {
                        if (message.ContentType.Equals(ContentTypes.COMMAND))
                            OnCommandReceived(new MessageEventArgs(message));
                        else if (message.ContentType.Equals(ContentTypes.DATA))
                            OnDataReceived(new MessageEventArgs(message));
                        else if (message.ContentType.Equals(ContentTypes.HEALTH))
                            OnHealthInfoReceived(new MessageEventArgs(message));
                        else if (message.ContentType.Equals(ContentTypes.HEARTBEAT))
                            Send(message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion 
    }
}
