﻿/*
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

using CodeAbility.MonitorAndCommand.Interfaces;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Helpers;

namespace CodeAbility.MonitorAndCommand.Client
{
    public class MessageClient : IMessageClient
    {
        const int HEARTBEAT_START_DELAY = 15000;

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

        #endregion 

        #region Properties

        public bool IsConnected { get { return socket != null && socket.Connected; } }
        string DeviceName { get; set; }
        string ServerIpAddress { get; set; }
        int PortNumber { get; set; }
        int HeartbeatPeriod { get; set; }

        #endregion 

        private Socket socket = null;

        private ConcurrentQueue<Message> messagesToSend = new ConcurrentQueue<Message>();

        //Threads
        private Thread receiveThread = null;
        private Thread sendThread = null;

        private Timer heartbeatTimer = null;

        //Events
        private ManualResetEvent sendEvent = new ManualResetEvent(false);
        private AutoResetEvent heartbeatEvent = new AutoResetEvent(false);

        private HeartbeatStatus heartbeatStatus = HeartbeatStatus.OK;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        public MessageClient(string deviceName) : 
            this(deviceName, 0)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="heartbeatPeriod"></param>
        public MessageClient(string deviceName, int heartbeatPeriod)
        {
            DeviceName = deviceName;
            HeartbeatPeriod = heartbeatPeriod; 

            receiveThread = new Thread(new ThreadStart(Receiver));
            sendThread = new Thread(new ThreadStart(Sender));
        }

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverIpAddress"></param>
        /// <param name="port"></param>
        public void Start(string serverIpAddress, int port)
        {
            ServerIpAddress = serverIpAddress;
            PortNumber = port;

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPAddress ipAddress = IPAddress.Parse(ServerIpAddress);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PortNumber);

                Console.WriteLine(String.Format("Device {0} connecting to server {1}.", DeviceName, remoteEP.ToString()));

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                socket.Connect(remoteEP);

                receiveThread.Start();
                sendThread.Start();
                
                Register(DeviceName);

                //Start, if period is set, the Heartbeat Timer
                if (HeartbeatPeriod > 0)
                {
                    TimerCallback heartbeatCallBack = Heartbeat;
                    heartbeatTimer = new Timer(heartbeatCallBack, heartbeatEvent, HEARTBEAT_START_DELAY, HeartbeatPeriod);
                }

                Console.WriteLine(String.Format("Device {0} connected to server.", DeviceName));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
                throw;
            }            
        }

        public void Stop()
        {
            Unregister(DeviceName);

            Thread.Sleep(1000);

            receiveThread.Abort();
            sendThread.Abort();

            if (socket != null && socket.Connected)
                socket.Shutdown(SocketShutdown.Both);
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

        public void SubscribeToCommand(string fromDevice, string commandTarget, string commandName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, fromDevice, DeviceName, commandTarget, commandName);
            EnqueueMessage(message);
        }

        public void SubscribeToServerState(string stateName)
        {
            Message message = Message.InstanciateSubscribeMessage(DeviceName, Message.SERVER, DeviceName, Message.SERVER, stateName);
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

        public void SendCommand(string toDevice, string commandTarget, string commandName, object commandValue)
        {
            Message message = Message.InstanciateCommandMessage(DeviceName, toDevice, commandTarget, commandName, commandValue);
            EnqueueMessage(message);
        }

        public void SendData(string toDevice, string dataName, string dataSource, object dataValue)
        {
            Message message = Message.InstanciateDataMessage(DeviceName, toDevice, dataName, dataSource, dataValue);
            EnqueueMessage(message);
        }

        #endregion 

        #region Internals

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

        protected void EnqueueMessage(Message message)
        {
            messagesToSend.Enqueue(message);
            sendEvent.Set();
        }

        private void Sender()
        {
            //Trace.WriteLine("Starting Sender() thread.");

            while (socket.Connected)
            {
                try
                {
                    sendEvent.Reset();

                    while (messagesToSend.Count > 0)
                    {
                        Message message = null;
                        if (messagesToSend.TryDequeue(out message))
                            Send(message);
                    }

                    sendEvent.WaitOne();
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception);
                }
            }
        }

        private void Send(Message message)
        {
            try
            {
                //Trace.WriteLine(String.Format("Sending     : {0}", message));

                string serializedMessage = JsonConvert.SerializeObject(message);
//#if DEBUG
//                Trace.WriteLine(String.Format("JSON        : {0}", serializedMessage));
//#endif
                string paddedSerializedData = JsonHelpers.PadSerializedMessage(serializedMessage, Message.BUFFER_SIZE);
//#if DEBUG
//                Trace.WriteLine(String.Format("Padded      : {0}", paddedSerializedData));
//#endif
                byte[] byteData = Encoding.UTF8.GetBytes(paddedSerializedData);

                socket.Send(byteData, 0, Message.BUFFER_SIZE, 0);
//#if DEBUG
//                Trace.WriteLine(String.Format("Sent        : {0}", message));
//                Console.WriteLine(String.Format("Sent        : {0}", paddedSerializedData));
//#endif
            }
            catch(Exception exception)
            {
                Trace.WriteLine(String.Format("Send exception : {0}", exception));
            }
        }

        private void Receiver()
        {
            //Trace.WriteLine("Starting Receiver() thread.");

            byte[] buffer = new byte[Message.BUFFER_SIZE];
            int offset = 0;

            while (socket.Connected)
            {
                try
                {
                    int missingBytesLength = Message.BUFFER_SIZE - offset;
                    int length = offset > 0 ? missingBytesLength : Message.BUFFER_SIZE;

                    int receivedBytesLength = socket.Receive(buffer, offset, length, 0);

                    if (receivedBytesLength == Message.BUFFER_SIZE || receivedBytesLength + offset == Message.BUFFER_SIZE)
                    {
                        string paddedSerializedData = Encoding.UTF8.GetString(buffer, 0, Message.BUFFER_SIZE);
                        string serializedMessage = JsonHelpers.CleanUpPaddedSerializedMessage(paddedSerializedData);

                        Message message = JsonConvert.DeserializeObject<Message>(serializedMessage);
//#if DEBUG
//                        Trace.WriteLine(String.Format("Received    : {0}", message));
//                        Console.WriteLine(String.Format("Received    : {0}", message));
//#endif
                        if (message != null)
                        {
                            if (message.ContentType.Equals(ContentTypes.COMMAND))
                                OnCommandReceived(new MessageEventArgs(message));
                            else if (message.ContentType.Equals(ContentTypes.DATA))
                                OnDataReceived(new MessageEventArgs(message));
                            else if (message.ContentType.Equals(ContentTypes.HEARTBEAT))
                                HandleHeartbeatMessage(message);
                        }

                        offset = 0;
                    }
                    else
                    {
                        offset = receivedBytesLength;
                    }

                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
        }

        #endregion 
        public void Heartbeat(Object stateInfo)
        {
            if (heartbeatStatus == HeartbeatStatus.NOK)
                throw new Exception();

            if (heartbeatStatus == HeartbeatStatus.Waiting)
                heartbeatStatus = HeartbeatStatus.NOK;

            Message heartbeatMessage = Message.InstanciateHeartbeatMessage(DeviceName);
            EnqueueMessage(heartbeatMessage);
            heartbeatStatus = HeartbeatStatus.Waiting;
        }

        private void HandleHeartbeatMessage(Message heartbeatMessage)
        {
            if (heartbeatMessage.FromDevice.Equals(Message.SERVER))
            {
                //Send back to sender
                EnqueueMessage(heartbeatMessage);
            }
            else if (heartbeatMessage.FromDevice.Equals(DeviceName))
            {
                heartbeatStatus = HeartbeatStatus.OK;
            }
        }
    }
}
