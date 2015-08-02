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

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Helpers;

namespace CodeAbility.MonitorAndCommand.MFClient
{
    public class MessageClient
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

        #endregion 

        #region Properties

        public string DeviceName { get; protected set; }

        string IpAddress { get; set; }

        int PortNumber { get; set; }

        bool IsLoggingEnabled { get; set; }

        #endregion 

        Socket socket = null;

        private Queue messagesToSend = new Queue();

        private Thread sendThread = null; 
        private Thread receiveThread = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        public MessageClient(string deviceName, bool isLoggingEnabled)
        { 
            DeviceName = deviceName;

            IsLoggingEnabled = isLoggingEnabled;
        }

        #region Public Methods

        public void Start(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            PortNumber = port;

            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(IpAddress);
                IPAddress _ipAddress = ipHostEntry.AddressList[0];

                IPEndPoint endpoint = new IPEndPoint(_ipAddress, PortNumber);
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (IsLoggingEnabled)
                    Logger.Instance.Write("Connecting to " + endpoint.ToString() + ".");

                this.socket.Connect(endpoint);

                sendThread = new Thread(new ThreadStart(Sender));
                receiveThread = new Thread(new ThreadStart(Receiver));

                sendThread.Start();
                receiveThread.Start();
                    
                Register();

                if (IsLoggingEnabled)
                    Logger.Instance.Write("MessageClient started.");
            }
            catch (Exception exception)
            {
                if (IsLoggingEnabled)
                {
                    Logger.Instance.Write("MessageClient Start() exception !");
                    Logger.Instance.Write(exception.ToString());
                }
            }
        }

        public void Stop()
        {
            try
            {
                this.receiveThread.Abort();
                this.sendThread.Abort();

                this.socket.Close();

                if (IsLoggingEnabled)
                    Logger.Instance.Write("MessageClient stopped");
            }
            catch (Exception exception)
            {
                if (IsLoggingEnabled)
                    Logger.Instance.Write(exception.ToString());
            }
        }

        public void Register()
        {
            Message message = Message.InstanciateRegisterMessage(DeviceName);
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

            sendDone.Set();
        }

        private void Sender()
        {
            if (IsLoggingEnabled)
                Logger.Instance.Write("Starting Sender() thread.");

            while (true)
            {
                try
                {
                    sendDone.Reset();

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
                    
                    sendDone.WaitOne();
                }
                catch (Exception exception)
                {
                    if (IsLoggingEnabled)
                        Logger.Instance.Write(exception.ToString());
                }
            }
        }

        private void Send(Message message)
        {
            try 
            {
                if (IsLoggingEnabled)
                    Logger.Instance.Write("Sending   : " + message.ToString());

                string serializedMessage = JsonSerializer.SerializeObject(message);
                string paddedSerializedData = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);

                byte[] byteData = Encoding.UTF8.GetBytes(paddedSerializedData);

                this.socket.Send(byteData, 0, byteData.Length, 0);

                if (IsLoggingEnabled)
                    Logger.Instance.Write("Sent      : " + message.ToString());
            } 
            catch (Exception exception)
            {
                if (IsLoggingEnabled)
                    Logger.Instance.Write("Send()    : " + exception.ToString());
            }
        }

        #region Threads

        private void Receiver()
        {
            Thread.Sleep(1000);

            if (IsLoggingEnabled)
                Logger.Instance.Write("Starting Receiver() thread.");

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
                    string serializedMessage = JsonHelpers.CleanUpPaddedSerializedData(paddedSerializedData.ToString());

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
                        if (IsLoggingEnabled)
                            Logger.Instance.Write("Received: " + message.ToString());

                        if (message.ContentType == ContentTypes.COMMAND)
                            OnCommandReceived(new MessageEventArgs(message));
                    }
                }
                catch (Exception exception)
                {
                    if (IsLoggingEnabled)
                        Logger.Instance.Write("Receiver() : " + exception.ToString());                   
                }
            }
        }

        #endregion 
    }
}
