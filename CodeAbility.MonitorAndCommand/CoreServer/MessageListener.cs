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

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Helpers; 

namespace CodeAbility.MonitorAndCommand.Server
{
    /// <summary>
    /// Waits for connections from clients, listens for the messages they send and then routes them to other clients that requested them. 
    /// </summary>
    public class MessageListener
    {
        #region Registration Event Handler

        public delegate void RegistrationEventHandler(object sender, RegistrationEventArgs e);

        public event RegistrationEventHandler RegistrationChanged;

        protected void OnRegistrationChanged(RegistrationEventArgs e)
        {
            if (RegistrationChanged != null)
                RegistrationChanged(this, e);
        }

        #endregion 

        #region Constants

        const int PROCESS_PERIOD = 50; 
        const int PROCESS_START_DELAY = 0;
        const int BACKLOG = 10;

        #endregion

        #region Properties 

        private string IpAddressString { get; set; }
        private int PortNumber { get; set; }

        private bool IsMessageServiceActivated { get; set; }

        #endregion 

        #region Private variables

        //Threads
        private Thread sendingThread = null;
        private Thread processingThread = null;
        private Thread messageStoringThread = null;
        private Thread eventStoringThread = null;

        //ManualResetEvent instances signal completion.
        private ManualResetEvent sendEvent = new ManualResetEvent(false);
        private ManualResetEvent processEvent = new ManualResetEvent(false);
        private ManualResetEvent storeMessageEvent = new ManualResetEvent(false);
        private ManualResetEvent storeEventEvent = new ManualResetEvent(false); 

        //private AutoResetEvent processTimerEvent = new AutoResetEvent(false);
        private AutoResetEvent heartbeatEvent = new AutoResetEvent(false);
  
        // Dictionary holding references to the threads receiving messages from connected clients        
        private ConcurrentDictionary<Address, Thread> receiveThreads = new ConcurrentDictionary<Address, Thread>();
        
        // Dictionary holding references to the sockets opened for each connected client        
        private ConcurrentDictionary<Address, Socket> clientsSockets = new ConcurrentDictionary<Address, Socket>();

        // Queue containing messages received from clients through each "message receiving" thread        
        private ConcurrentQueue<Message> messagesReceived = new ConcurrentQueue<Message>();
   
        // Queue containg messages to be sent
        private ConcurrentQueue<Message> messagesToSend = new ConcurrentQueue<Message>();
        
        // Queue containg messages to be "stored"        
        private ConcurrentQueue<Message> messagesToStore = new ConcurrentQueue<Message>();

        // Queue containg messages to be "stored"        
        private ConcurrentQueue<Event> eventsToStore = new ConcurrentQueue<Event>();

        // Object managing references to devices (connected clients)
        DevicesManager devicesManager = new DevicesManager();

        // Object managing the "message routing rules" derived from publish/subscribe messages sent by the clients
        RulesManager rulesManager = new RulesManager();

        private Socket listener = null;

        #endregion 

        public string LocalEndPoint { get { return (listener != null) ? listener.LocalEndPoint.ToString() : String.Empty; } }

        MessageServiceReference.MessageServiceClient messageServiceClient = null;
        EventServiceReference.EventServiceClient eventServiceClient = null;

        public MessageListener(string ipAddress, int portNumber)
            :this(ipAddress, portNumber, false)
        {

        }

        public MessageListener(string ipAddress, int portNumber, bool isMessageServiceActivated)
        {           
            //Parameters
            IpAddressString = ipAddress;
            PortNumber = portNumber;
            IsMessageServiceActivated = isMessageServiceActivated;

            if (IsMessageServiceActivated)
            {
                try
                { 
                    //Activate the message service
                    messageServiceClient = new MessageServiceReference.MessageServiceClient();
                    messageServiceClient.Open();

                    eventServiceClient = new EventServiceReference.EventServiceClient();
                    eventServiceClient.Open();

                    Console.WriteLine("Message service is activated."); 
                }
                catch(Exception exception)
                {
                    Console.WriteLine("Message service cannot be reached");

                    string content = String.Format("Service exception : {0}", exception);
                    LogEvent(content);
                }
            }
        }

        /// <summary>
        /// Start/Stop listening for messages
        /// </summary>
        public void StartListening()
        {
            IPAddress ipAddress = IPAddress.Parse(IpAddressString);
            if (!CodeAbility.MonitorAndCommand.Helpers.NetworkHelpers.CheckAddressValidity(ipAddress))
                throw new Exception("Invalid Ip address for this machine.");

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PortNumber);

            // Create a TCP/IP socket.
           listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {                           
                //Start main Threads

                //Processing using ManualResetEvent
                processingThread = new Thread(new ThreadStart(SynchronizedProcessor));
                processingThread.Start();

                //Processing using a Timer
                //TimerCallback processCallBack = PeriodicProcessor;
                //processTimer = new Timer(processCallBack, null, PROCESS_START_DELAY, PROCESS_PERIOD);

                sendingThread = new Thread(new ThreadStart(Sender));
                messageStoringThread = new Thread(new ThreadStart(MessageStorer));
                eventStoringThread = new Thread(new ThreadStart(EventStorer));

                //Start listening
                listener.Bind(localEndPoint);
                listener.Listen(BACKLOG);

                sendingThread.Start();

                string listening = "Listening on " + listener.LocalEndPoint;
                Console.WriteLine(listening);
                LogEvent(listening);


                //Start storing thread
                if (IsMessageServiceActivated)
                {
                    messageStoringThread.Start();
                    eventStoringThread.Start();
                }

                while (true)
                {
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");

                    Socket socket = listener.Accept();
                    Address address = new Address(socket.RemoteEndPoint.ToString());
                    string content = String.Format("new connection from {0}", address.ToString());
                    Trace.WriteLine(content);
                    LogEvent(content);

                    //Clean up if a previous socket still exists for that address
                    if (clientsSockets.ContainsKey(address))
                        CleanUp(address);

                    if (clientsSockets.TryAdd(address, socket))
                    {
                        Thread receiveThread = new Thread(new ParameterizedThreadStart(Receiver));
                        if (receiveThreads.TryAdd(address, receiveThread))
                            receiveThread.Start(socket);
                        else
                            Trace.WriteLine(String.Format("Could not add thread {0} to collection !", receiveThread.ToString()));
                    }
                    else
                        Trace.WriteLine(String.Format("Could not add socket {0} to collection !", address.ToString()));
                }
            }
            catch (Exception exception)
            {
                string content = String.Format("Listener error : {0}", exception);
                LogEvent(content);
            }
        }

        public void StopListening()
        {
            rulesManager.RemoveAllRules();

            foreach(Socket socket in clientsSockets.Values)
            {
                Address address = new Address(socket.RemoteEndPoint.ToString());
                CleanUp(address);
            }

            devicesManager.RemoveAllDevices();

            listener.Close();
        }

        #region Receiving messages

        /// <summary>
        /// Message receiving thread method
        /// </summary>
        /// <param name="socketObject">receiving socket</param>
        private void Receiver(object socketObject)
        {
            Socket socket = socketObject as Socket;
            Address address = new Address(socket.RemoteEndPoint.ToString());

            Trace.WriteLine(String.Format("Starting Receiver thread for socket {0}", socket.RemoteEndPoint.ToString()));

            // Receive buffer.
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
                        string paddedSerializedMessage = Encoding.UTF8.GetString(buffer, 0, Message.BUFFER_SIZE);
                        string cleanedUpSerializedMessage = JsonHelpers.CleanUpPaddedSerializedMessage(paddedSerializedMessage);

                        Message receivedMessage = JsonConvert.DeserializeObject<Message>(cleanedUpSerializedMessage);

                    //HACK : we pass the ip:port address in the Property argument
                        if (receivedMessage.Name.Equals(ControlActions.REGISTER))
                            receivedMessage.Content = socket.RemoteEndPoint.ToString();
//#if DEBUG
//                        Trace.WriteLine(String.Format("Received  : {0}", receivedMessage));
//#endif
                        messagesReceived.Enqueue(receivedMessage);
                    
                        offset = 0;
                    }
                    else
                    {
                        offset = receivedBytesLength;
                    }

                    //While there are bytes in the socket's buffer, we get them, read the messages and add them to the queue
                    //This usually only happens during the registration/subscription phase
                    if (socket.Available < Message.BUFFER_SIZE)
                        processEvent.Set();
                }
                catch(Exception exception)
                {
                    string content = String.Format("Receiver error : {0}", exception);
                    LogEvent(content);

                    CleanUp(address);
                    break;
                }
            }
        }

        private void CleanUp(Address address)
        {
            string deviceName = devicesManager.GetDeviceNameFromAddress(address);

            string content = String.Format("Device {0} disconnected for {1}.", deviceName, address);
            Console.WriteLine(content);
            LogEvent(content);

            //Close socket and abort thread
            Thread thread = null;
 
            Unregister(deviceName);

            try
            {
                if (receiveThreads.TryRemove(address, out thread))
                {
                    //Stop the receive thread before closing the socket
                    thread.Abort();
                    Trace.WriteLine(String.Format("Device {0} thread aborted.", deviceName));

                    Socket _socket = null;
                    if (clientsSockets.TryRemove(address, out _socket))
                    {
                        if (_socket.Connected)
                        {
                            _socket.Shutdown(SocketShutdown.Both);
                            _socket.Close();
                        }

                        _socket.Dispose();
                    }

                    Trace.WriteLine(String.Format("Device {0} socket for {1} closed.", deviceName, address));
                }
            }
            catch(Exception exception)
            {
                Trace.WriteLine(String.Format("Cleanup exception for {0} : {1}", deviceName, exception.ToString()));
            }

            content = String.Format("Device {0} socket and thread cleaned up for {1}.", deviceName, address);
            Console.WriteLine(content);
            LogEvent(content);
        }

        #endregion 

        #region Processing received messages

        /// <summary>
        /// Message processing thread method
        /// </summary>
        private void SynchronizedProcessor()
        {
            Trace.WriteLine("Starting Processor() thread"); 

            while (true)
            {
                processEvent.Reset();

                DequeueAndProcessMessages(); 
                
                //Wait for a signal from any "Receiver" thread
                processEvent.WaitOne();
            }
        }

        /// <summary>
        /// Message processing thread method
        /// </summary>
        private void PeriodicProcessor(Object stateInfo)
        {
            Trace.WriteLine("PeriodicProcessor()"); 

            DequeueAndProcessMessages(); 
        }

        private void DequeueAndProcessMessages()
        {
            while (messagesReceived.Count > 0)
            {
                Message message = null;
                if (messagesReceived.TryDequeue(out message))
                {
                    //HACK : the Timestamp produced by some devices is not reliable, so we redefine it with a Timestamp produced by the server.
                    message.Timestamp = DateTime.Now;

                    if (IsMessageServiceActivated)
                        StoreMessage(message);

                    PreProcess(message);
                    Process(message);
                    PostProcess(message);
                }
            }
        }

        protected virtual void PreProcess(Message message)
        {

        }

        protected void Process(Message message)
        {
            try
            {
                ContentTypes messageType = message.ContentType;
                switch(messageType)
                { 
                    case ContentTypes.CONTROL:
                        ProcessServerAction(message);
                        break;
                    case ContentTypes.DATA:
                    case ContentTypes.COMMAND:
                        SendToRegisteredDevices(message);
                        break;
                    case ContentTypes.HEARTBEAT:
                        HandleHeartbeatMessage(message);
                        break;
                    case ContentTypes.RESPONSE:
                        throw new NotImplementedException();
                }
//#if DEBUG
//                Trace.WriteLine(String.Format("Processed : {0}", message));
//#endif 
            }
            catch (Exception exception)
            {
                string content = String.Format("Processing exception : {0}", exception);

                Trace.WriteLine(content);
                StoreEvent(new Event(Message.SERVER, content));
            }
        }

        protected virtual void PostProcess(Message message)
        {

        }

        protected void PublishServerState(string stateName)
        {
            Message fakeMessage = Message.InstanciatePublishMessage(Message.SERVER, Message.ALL, Message.SERVER, stateName);

            Publish(fakeMessage);
        }

        protected Message InstantiateServerStateDataMessage(string stateName, object content)
        {
            Message message = Message.InstanciateDataMessage(Message.SERVER, Message.ALL, Message.SERVER, stateName, content);
            return message;
        }

        #region Server actions

        private void ProcessServerAction(Message message)
        {
            switch (message.Name)
            {
                case ControlActions.REGISTER:
                    //HACK : we pass the ip:port address in the Content argument
                    Address deviceAddress = new Address(message.Content.ToString());
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
                default:
                    throw new NotSupportedException(String.Format("HandleServerAction, {0} is not a supported action", message.Name));
            }
        }

        private void Register(Address origin, string deviceName)
        {
            //Clean up old connection data if any
            Address previousAddress = devicesManager.GetAddressFromDeviceName(deviceName);
            if (previousAddress != null)
                CleanUp(previousAddress);

            devicesManager.AddDevice(deviceName, origin);

            string content = String.Format("Device {0} registered.", deviceName);
            Console.WriteLine(content);
            LogEvent(content);

            OnRegistrationChanged(new RegistrationEventArgs(deviceName, RegistrationEventArgs.RegistrationEvents.Registered)); 
        }

        private void Unregister(string deviceName)
        {
            rulesManager.RemoveAllRules(deviceName);
            devicesManager.RemoveDevice(deviceName);

            string content = String.Format("Device {0} unregistered.", deviceName);
            Console.WriteLine(content);
            LogEvent(content);

            OnRegistrationChanged(new RegistrationEventArgs(deviceName, RegistrationEventArgs.RegistrationEvents.Unregistered)); 
        }

        private void Publish(Message message)
        {
            rulesManager.AddRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        private void Unpublish(Message message)
        {
            rulesManager.RemoveRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        private void Subscribe(Message message) 
        {
            rulesManager.AddRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        private void Unsubscribe(Message message)
        {
            rulesManager.RemoveRule(message.SendingDevice, message.FromDevice, message.ToDevice, message.Parameter.ToString(), message.Content.ToString());
        }

        #endregion 

        #endregion

        #region Sending messages

        protected void SendToRegisteredDevices(Message message)
        {
            IEnumerable<string> authorizedDeviceNames = rulesManager.GetAuthorizedDeviceNames(message.ContentType, message.FromDevice, message.ToDevice, message.Name, message.Parameter.ToString());
            foreach (string deviceName in authorizedDeviceNames)
            {
                Message sendToMessage = new Message(message);
                sendToMessage.ReceivingDevice = deviceName;
                messagesToSend.Enqueue(sendToMessage);
            }

            sendEvent.Set();
        }

        protected void SendToAllDevices(Message message)
        {
            IEnumerable<string> deviceNames = devicesManager.GetAllDeviceNames();
            foreach (string deviceName in deviceNames)
            {
                Message sendToMessage = new Message(message);
                sendToMessage.ReceivingDevice = deviceName;
                messagesToSend.Enqueue(sendToMessage);
            }

            sendEvent.Set();
        }

        protected void SendDirectlyToDevice(string deviceName, Message message)
        {
            Message sendToMessage = new Message(message);
            sendToMessage.ReceivingDevice = deviceName;
            messagesToSend.Enqueue(sendToMessage);
            
            sendEvent.Set();
        }


        private void Sender()
        {
            Trace.WriteLine("Starting Sender() thread"); 

            while (true)
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
        }

        protected virtual void PostSend(Message message)
        {

        }

        private void Send(Message message)
        {
            Address destinationAddress = null;

            try
            {
                string serializedMessage = JsonConvert.SerializeObject(message);
                serializedMessage = JsonHelpers.PadSerializedMessage(serializedMessage, Message.BUFFER_SIZE);

                // Convert the string data to byte data using UTF8 encoding.
                byte[] byteData = Encoding.UTF8.GetBytes(serializedMessage);

                destinationAddress = devicesManager.GetAddressFromDeviceName(message.ReceivingDevice);
                if (destinationAddress != null)
                {
                    Socket socket = clientsSockets.FirstOrDefault(x => x.Key.Equals(destinationAddress)).Value as Socket;
                    if (socket != null && socket.Connected)
                    {
                        socket.Send(byteData, 0, Message.BUFFER_SIZE, 0);
                        PostSend(message);
//#if DEBUG
//                        Trace.WriteLine(String.Format("Sent      : {0}", message));
//#endif
                    }
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
            }
            catch (Exception exception)
            {
                string content = String.Format("Send socket   : {0}", exception);
                LogEvent(content);
                //CleanUp(destinationAddress);
            }
        }

        #endregion 
        
        #region Heartbeat messages

        private void HandleHeartbeatMessage(Message heartbeatMessage)
        {
            if (!heartbeatMessage.FromDevice.Equals(Message.SERVER))
            {
                SendDirectlyToDevice(heartbeatMessage.SendingDevice, heartbeatMessage);
            }
            else
            {
                //Handle heartbeats initiated by the server
            }
        }

        #endregion 

        #region Message Service

        private void StoreMessage(Message message)
        {
            messagesToStore.Enqueue(message);
            storeMessageEvent.Set();
        }

        private void MessageStorer()
        {
            Trace.WriteLine("Starting MessageStorer() thread");

            while (true)
            {
                storeMessageEvent.Reset();

                while (messagesToStore.Count > 0)
                {
                    Message message = null;
                    if (messagesToStore.TryDequeue(out message))
                        Store(message);
                }

                storeMessageEvent.WaitOne();
            }
        }

        private void Store(Message message)
        {
            try
            { 
                if (messageServiceClient != null && messageServiceClient.State == System.ServiceModel.CommunicationState.Opened)
                {
                    messageServiceClient.StoreMessage(message);
                }
            }
            catch(Exception exception)
            {
                Trace.WriteLine(exception);
            }
        }

        #endregion 

        #region Event Service

        private void StoreEvent(Event _event)
        {
            //HACK : the Timestamp produced by some devices being unreliable, we override it with a Timestamp produced by the server.
            eventsToStore.Enqueue(_event);
            storeEventEvent.Set();
        }

        private void EventStorer()
        {
            Trace.WriteLine("Starting EventStorer() thread");

            while (true)
            {
                storeEventEvent.Reset();

                while (eventsToStore.Count > 0)
                {
                    Event _event = null;
                    if (eventsToStore.TryDequeue(out _event))
                        Store(_event);
                }

                storeEventEvent.WaitOne();
            }
        }

        private void Store(Event _event)
        {
            try
            {
                if (eventServiceClient != null && eventServiceClient.State == System.ServiceModel.CommunicationState.Opened)
                {
                    eventServiceClient.StoreEvent(_event);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
            }
        }

        #endregion 

        private void LogEvent(string content)
        {
            Trace.WriteLine(content);
            StoreEvent(new Event(Message.SERVER, content));
        }
    }
}
