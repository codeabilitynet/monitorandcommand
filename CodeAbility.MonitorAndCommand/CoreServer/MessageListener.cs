﻿/*
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

namespace CodeAbility.MonitorAndCommand.Server
{
    //Inspired from : https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx

    public class MessageListener
    {
        const string ALL = "*";

        //Threads
        private Thread sendingThread = null;
        private Thread processingThread = null;
        private Timer heartbeatTimer = null;

        //ManualResetEvent instances signal completion.
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent processDone = new ManualResetEvent(false);
        private AutoResetEvent heartbeatEvent = new AutoResetEvent(false);

        //
        private ConcurrentDictionary<Address, Thread> receiveThreads = new ConcurrentDictionary<Address, Thread>();
        private ConcurrentDictionary<Address, Socket> clientsSockets = new ConcurrentDictionary<Address, Socket>();

        private ConcurrentDictionary<string, int> messageHeartbeatCounters = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, Queue<int>> messageMinutesCounters = new ConcurrentDictionary<string, Queue<int>>();
        //
        private ConcurrentQueue<Message> messagesReceived = new ConcurrentQueue<Message>();
        private ConcurrentQueue<Message> messagesToSend = new ConcurrentQueue<Message>();
        
        //
        DevicesManager devicesManager = new DevicesManager();
        RulesManager rulesManager = new RulesManager(); 

        int PortNumber { get; set; }
        int HeartbeatPeriod { get; set; }

        Socket listener = null;

        public string LocalEndPoint { get { return (listener != null) ? listener.LocalEndPoint.ToString() : String.Empty; } }

        public MessageListener(int portNumber, int heartbeatPeriod)
        {
            //Parameters
            PortNumber = portNumber;
            HeartbeatPeriod = heartbeatPeriod;

            //Threads
            processingThread = new Thread(new ThreadStart(Processor));
            sendingThread = new Thread(new ThreadStart(Sender));

            //Heart beat Timer
            if (HeartbeatPeriod > 0)
            { 
                TimerCallback tcb = Heartbeat;
                heartbeatTimer = new Timer(tcb, heartbeatEvent, HeartbeatPeriod, HeartbeatPeriod);
            }
        }

        public void StartListening()
        {
            IPAddress ipAddress = CodeAbility.MonitorAndCommand.Server.NetworkHelper.GetLocalIPAddress();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PortNumber);

            // Create a TCP/IP socket.
           listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                processingThread.Start();

                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Listening on " + listener.LocalEndPoint); 

                sendingThread.Start();

                while (true)
                {
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    Socket socket = listener.Accept(/*new AsyncCallback(AcceptCallback), listener*/);

                    Address address = new Address(socket.RemoteEndPoint.ToString());

                    clientsSockets.TryAdd(address, socket);

                    Thread receiveThread = new Thread(new ParameterizedThreadStart(Receiver));

                    receiveThreads.TryAdd(address, receiveThread);
                    receiveThread.Start(socket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        #region Threads Methods

        public void Receiver(object socketObject)
        {
            Socket socket = socketObject as Socket;

            while (socket.Connected)
            {
                try
                {
                    // Receive buffer.
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];

                    socket.Receive(buffer, 0, Constants.BUFFER_SIZE, 0);

                    string serializedData = Encoding.UTF8.GetString(buffer, 0, Constants.BUFFER_SIZE);                    
                    int firstBraceIndex = serializedData.IndexOf('{');
                    int lastBraceIndex = serializedData.LastIndexOf('}');
                    string cleanedUpSerializedData = serializedData.Substring(firstBraceIndex, lastBraceIndex - firstBraceIndex + 1);

                    Message receivedMessage = JsonConvert.DeserializeObject<Message>(cleanedUpSerializedData);

                    //HACK : we pass the ip:port address in the Property argument
                    if (receivedMessage.Name.Equals(ControlActions.REGISTER))
                        receivedMessage.Content = new Address(socket.RemoteEndPoint.ToString());

                    Console.WriteLine(receivedMessage);

                    messagesReceived.Enqueue(receivedMessage);

                    processDone.Set();
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception);

                    //Close socket and abort thread
                    Thread thread = null;
                    Address address = new Address(socket.RemoteEndPoint.ToString());
                    if (receiveThreads.TryRemove(address, out thread))
                    {
                        Socket _socket = null;
                        if (clientsSockets.TryRemove(address, out _socket))
                            _socket.Close();

                        thread.Abort();
                    }

                    break;
                }
            }
        }

        private void Processor()
        {
            while(true)
            {
                processDone.Reset();

                while (messagesReceived.Count > 0)
                {
                    Message message = null;
                    if (messagesReceived.TryDequeue(out message))
                        Process(message);
                }

                processDone.WaitOne();
            }
        }

        private void Sender()
        {
            while (true)
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
        }

        public void Heartbeat(Object stateInfo)
        {
            foreach (Device device in devicesManager.Devices)
            {
                //double deviceMessageCount = GetDeviceMessageCount(device.Name);
                Message heartbeat = Message.InstanciateHeartbeatMessage(device.Name);
                SendToSpecificDevice(heartbeat);
            }

            heartbeatEvent.Set();
        }

        #endregion 

        #region Server Actions

        protected void Process(Message message)
        {
            ContentTypes messageType = message.ContentType;
            if (messageType.Equals(ContentTypes.CONTROL))
            {
                HandleServerAction(message);
            }
            else if (messageType.Equals(ContentTypes.DATA) || messageType.Equals(ContentTypes.COMMAND))
            {
                SendToAuthorizedDevices(message);
            }
            else if (messageType.Equals(ContentTypes.HEARTBEAT))
            {
                HandleReturnedHeartbeat(message);
            }
        }

        private void HandleServerAction(Message message)
        {
            try
            {
                switch (message.Name)
                {
                    case ControlActions.REGISTER:
                        //HACK : we pass the ip:port address in the Content argument
                        Address deviceAddress = message.Content as Address;
                        Register(deviceAddress, message.FromDevice);
                        break;
                    case ControlActions.UNREGISTER:
                        Unregister(message.FromDevice);
                        break;
                    case ControlActions.PUBLISH:
                        Publish(message);
                        break;
                    case ControlActions.UNPUBLISH:
                        Unpublish(message);
                        break;
                    case ControlActions.SUBSCRIBE:
                        Subscribe(message);
                        break;
                    case ControlActions.UNSUBSCRIBE:
                        Unsubscribe(message);
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected void Register(Address origin, string deviceName)
        {
            devicesManager.AddDevice(deviceName, origin);
        }

        protected void Unregister(string deviceName)
        {
            devicesManager.RemoveDevice(deviceName);
        }

        protected void Publish(Message message)
        {
            rulesManager.AddRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        protected void Unpublish(Message message)
        {
            rulesManager.RemoveRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        protected void Subscribe(Message message) 
        {
            rulesManager.AddRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        protected void Unsubscribe(Message message)
        {
            rulesManager.RemoveRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        protected void HandleReturnedHeartbeat(Message message)
        {
            DateTime sentOn = (DateTime)message.Timestamp;
            DateTime now = DateTime.Now;
            TimeSpan span = now - sentOn;
        }

        private void SendToAuthorizedDevices(Message message)
        {
            IEnumerable<string> authorizedDeviceNames = rulesManager.GetAuthorizedDeviceNames(message.ContentType, message.FromDevice, message.ToDevice, message.Name, message.Parameter.ToString());
            foreach (string deviceName in authorizedDeviceNames)
            {
                message.ReceivingDevice = deviceName;

                Send(message);
            }
        }

        private void SendToSpecificDevice(Message message)
        {
            messagesToSend.Enqueue(message);
            sendDone.Set();
        }

        private void Send(Message message)
        {
            string serializedMessage = JsonConvert.SerializeObject(message);
            serializedMessage = serializedMessage.PadRight(Constants.BUFFER_SIZE, '.');
            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(serializedMessage);

            Address destinationAddress = devicesManager.GetAddressFromDeviceName(message.ReceivingDevice);
            Socket socket = clientsSockets.First(x => x.Key.Equals(destinationAddress)).Value as Socket;

            if (socket != null && socket.Connected)
                socket.Send(byteData, 0, byteData.Length, 0);
            else
            {
                clientsSockets.TryRemove(destinationAddress, out socket);
                Thread receiveThread = null;
                if (receiveThreads.TryGetValue(destinationAddress, out receiveThread))
                {
                    receiveThread.Abort();
                    receiveThreads.TryRemove(destinationAddress, out receiveThread);
                }
            }
        }

        #endregion 
    }
}
