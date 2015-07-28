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
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public bool IsConnected { get { return socket != null && socket.Connected; } }

        string DeviceName { get; set; }

        string IpAddress { get; set; }

        int PortNumber { get; set; }

        #endregion 

        private Socket socket = null;

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

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                socket.Connect(remoteEP);

                receiveThread.Start();
                sendThread.Start();
                
                Register();

                Console.WriteLine(String.Format("Device {0} connected to server.", DeviceName));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void Stop()
        {
            Unregister();

            Thread.Sleep(1000);

            receiveThread.Abort();

            if (socket.Connected)
                socket.Shutdown(SocketShutdown.Both);
            
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
            Trace.WriteLine("Starting Sender() thread.");

            while (socket.Connected)
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
                    Trace.WriteLine(exception);
                    throw;
                }
            }
        }

        private void Send(Message message)
        {
            try
            {
                Trace.WriteLine(String.Format("Sending     : {0}", message));

                string serializedMessage = JsonConvert.SerializeObject(message);
                string paddedSerializedData = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);
                byte[] byteData = Encoding.UTF8.GetBytes(paddedSerializedData);

                socket.Send(byteData, 0, Constants.BUFFER_SIZE, 0);

                Trace.WriteLine(String.Format("Sent        : {0}", message));
            }
            catch(Exception exception)
            {
                Trace.WriteLine(String.Format("Send exception : {0}", exception));
            }
        }

        private void Receiver()
        {
            Trace.WriteLine("Starting Receiver() thread.");

            while (socket.Connected)
            {
                try
                {
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];

                    socket.Receive(buffer, 0, Constants.BUFFER_SIZE, 0);
                    string paddedSerializedData = Encoding.UTF8.GetString(buffer, 0, Constants.BUFFER_SIZE);
                    string serializedMessage = JsonHelpers.CleanUpPaddedSerializedData(paddedSerializedData);

                    Message message = JsonConvert.DeserializeObject<Message>(serializedMessage);

                    Trace.WriteLine(String.Format("Received    : {0}", message));

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
                    Trace.WriteLine(e);
                }
            }
        }

        #endregion 
    }
}
