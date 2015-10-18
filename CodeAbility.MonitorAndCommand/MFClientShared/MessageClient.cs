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
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;

using Json.NETMF;

using CodeAbility.MonitorAndCommand.Interfaces;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Helpers;

namespace CodeAbility.MonitorAndCommand.MFClient
{
    public class MessageClient : IMessageClient
    {
        #region Event

        // A delegate type for hooking up change notifications.
        public delegate void CommandReceivedEventHandler(object sender, MessageEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event CommandReceivedEventHandler CommandReceived;

        // Invoke the Changed event; called whenever list changes
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

        public string DeviceName { get; protected set; }

        string ServerIpAddress { get; set; }

        int PortNumber { get; set; }

        bool IsLoggingEnabled { get; set; }

        public bool IsConnected { get; set; }

        #endregion 

        Socket socket = null;

        private Queue messagesToSend = new Queue();

        private Thread sendThread = null; 
        private Thread receiveThread = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent sendEvent = new ManualResetEvent(false);

        public MessageClient(string deviceName, bool isLoggingEnabled)
        { 
            DeviceName = deviceName;

            IsLoggingEnabled = isLoggingEnabled;

            IsConnected = false;
        }

        #region Public Methods

        public void Start(string serverIpAddress, int port)
        {
            ServerIpAddress = serverIpAddress;
            PortNumber = port;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(ServerIpAddress);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PortNumber);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, Constants.BUFFER_SIZE * 8);
                //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, Constants.BUFFER_SIZE * 8);

                Log("Connecting to " + ipAddress + ".");

                socket.Connect(remoteEP);

                IsConnected = true;

                Log("Connected to " + remoteEP.ToString() + ".");

                sendThread = new Thread(new ThreadStart(Sender));
                receiveThread = new Thread(new ThreadStart(Receiver));

                sendThread.Start();
                receiveThread.Start();
                    
                Register(DeviceName);

                Log("MessageClient started.");
            }
            catch (Exception exception)
            {
                Log("MessageClient Start() exception !");
                Log(exception.ToString());

                Stop(); 
            }
        }

        public void Stop()
        {
            try
            {
                Unregister(DeviceName);

                Thread.Sleep(1000);

                this.receiveThread.Abort();
                this.sendThread.Abort();

                if (socket != null)
                    socket.Close();

                IsConnected = false;

                Log("MessageClient stopped");
            }
            catch (Exception exception)
            {
                Log(exception.ToString());
            }
        }

        public void Register(string deviceName)
        {
            Message message = Message.InstanciateRegisterMessage(deviceName);
            EnqueueMessage(message);
        }

        public void Unregister(string deviceName)
        {
            Message message = Message.InstanciateUnregisterMessage(deviceName);
            EnqueueMessage(message);
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

        public void SubscribeToCommand(string fromDevice, string commandName, string commandTarget)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, commandName, commandTarget);
            EnqueueMessage(message);
        }

        public void SubscribeToData(string fromDevice, string dataSource, string dataName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, dataSource, dataName);
            EnqueueMessage(message);
        }

        public void SendCommand(string toDevice, string commandName, string commandTarget, object commandValue)
        {
            Message message = Message.InstanciateCommandMessage(DeviceName, toDevice, commandName, commandTarget, commandValue);
            EnqueueMessage(message);
        }

        public void SendData(string toDevice, string dataSource, string dataName, object dataValue)
        {
            Message message = Message.InstanciateDataMessage(DeviceName, toDevice, dataSource, dataName, dataValue);
            EnqueueMessage(message);
        }

        #endregion 

        private void EnqueueMessage(Message message)
        {
            lock (messagesToSend)
            {
                messagesToSend.Enqueue(message);
            }

            sendEvent.Set();
        }

        private void Sender()
        {
            Log("Starting Sender() thread.");

            while (true)
            {
                try
                {
                    sendEvent.Reset();

                    while(messagesToSend.Count > 0)
                    {
                        Message message = null; 

                        lock (messagesToSend)
                        {
                            message = messagesToSend.Dequeue() as Message;
                        }

                        if (message != null)
                            Send(message);
                    }
                    
                    sendEvent.WaitOne();
                }
                catch (Exception exception)
                {
                    Log(exception.ToString());
                    //throw;
                }
            }
        }

        private void Send(Message message)
        {
            try 
            {
                Log("Sending   : " + message.ToString());

                string serializedMessage = JsonSerializer.SerializeObject(message);
                string paddedSerializedData = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);

                byte[] buffer = Encoding.UTF8.GetBytes(paddedSerializedData);

                this.socket.Send(buffer, 0, Constants.BUFFER_SIZE, 0);

                Log("Sent      : " + message.ToString());
            } 
            catch (Exception exception)
            {
                Log("Send()    : " + exception.ToString());
                //throw;
            }
        }

        #region Threads

        private void Receiver()
        {
            Thread.Sleep(1000);

            Log("Starting Receiver() thread.");

            while (true)
            {
                try
                {
                    // Receive buffer.
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];

                    // Begin receiving the data from the remote device.
                    socket.Receive(buffer, 0, Constants.BUFFER_SIZE, SocketFlags.None);

                    char[] dataChars = Encoding.UTF8.GetChars(buffer, 0, Constants.BUFFER_SIZE);
                    string paddedSerializedData = new string(dataChars);

                    if (paddedSerializedData != null)
                    { 
                        string serializedMessage = JsonHelpers.CleanUpPaddedSerializedData(paddedSerializedData);
                        object deserializedObject = JsonSerializer.DeserializeString(serializedMessage);
                        Hashtable hashTable = deserializedObject as Hashtable;
                        Message message = new Message()
                        {
                            SendingDevice = (string)hashTable["SendingDevice"],
                            ReceivingDevice = (string)hashTable["ReceivingDevice"],
                            FromDevice = (string)hashTable["FromDevice"],
                            ToDevice = (string)hashTable["ToDevice"],
                            ContentType = Convert.ToInt32(hashTable["ContentType"].ToString()),
                            Name = (string)hashTable["Name"],
                            Parameter = (string)hashTable["Parameter"],
                            Content = hashTable.Contains("Content") ? (string)hashTable["Content"] : String.Empty
                        };
    
                        if (message != null)
                        {
                            Log("Received  :" + message.ToString());

                            if (message.ContentType.Equals(ContentTypes.DATA))
                                OnDataReceived(new MessageEventArgs(message));
                            else if (message.ContentType == ContentTypes.COMMAND)
                                OnCommandReceived(new MessageEventArgs(message));
                            else if (message.ContentType.Equals(ContentTypes.HEALTH))
                                OnHealthInfoReceived(new MessageEventArgs(message));
                        }

                    }
                }
                catch (Exception exception)
                {
                    Log("Receiver() : " + exception.ToString());
                    //throw;
                }
            }
        }

        #endregion 

        public void Log(string message)
        {
            if (IsLoggingEnabled)
                Logger.Instance.Write(message);
        }

    }
}
