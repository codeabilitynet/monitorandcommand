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

namespace CodeAbility.MonitorAndCommand.Server
{
    /// <summary>
    /// Waits for connections from clients, listens for the messages they send and then routes them to other clients that requested them. 
    /// </summary>
    public class MessageListener
    {
        #region 

        public delegate void RegistrationEventHandler(object sender, RegistrationEventArgs e);

        public event RegistrationEventHandler RegistrationChanged;

        protected void OnRegistrationChanged(RegistrationEventArgs e)
        {
            if (RegistrationChanged != null)
                RegistrationChanged(this, e);
        }

        #endregion 

        const string ALL = "*";

        //Threads
        private Thread sendingThread = null;
        private Thread processingThread = null;
        private Thread storingThread = null;
        private Timer heartbeatTimer = null;

        //ManualResetEvent instances signal completion.
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent processDone = new ManualResetEvent(false);
        private ManualResetEvent storeDone = new ManualResetEvent(false);
        private AutoResetEvent heartbeatEvent = new AutoResetEvent(false);

        /// <summary>
        /// Dictionary holding references to the threads receiving messages from connected clients
        /// </summary>
        private ConcurrentDictionary<Address, Thread> receiveThreads = new ConcurrentDictionary<Address, Thread>();

        /// <summary>
        /// Dictionary holding references to the sockets opened for each connected client
        /// </summary>
        private ConcurrentDictionary<Address, Socket> clientsSockets = new ConcurrentDictionary<Address, Socket>();

        /// <summary>
        /// Queue containing messages received from clients through each "message receiving" thread
        /// </summary>
        private ConcurrentQueue<Message> messagesReceived = new ConcurrentQueue<Message>();

        /// <summary>
        /// Queue containg messages to be sent
        /// </summary>
        private ConcurrentQueue<Message> messagesToSend = new ConcurrentQueue<Message>();

        /// <summary>
        /// Queue containg messages to be "stored"
        /// </summary>
        private ConcurrentQueue<Message> messagesToStore = new ConcurrentQueue<Message>();
       
        /// <summary>
        /// Object managing references to devices (connected clients)
        /// </summary>
        DevicesManager devicesManager = new DevicesManager();

        /// <summary>
        /// Object managing the "message routing rules" derived from publish/subscribe messages sent by the clients
        /// </summary>
        RulesManager rulesManager = new RulesManager();

        string IpAddressString { get; set; }
        int PortNumber { get; set; }
        int HeartbeatPeriod { get; set; }

        bool IsMessageServiceActivated { get; set; }

        Socket listener = null;

        public string LocalEndPoint { get { return (listener != null) ? listener.LocalEndPoint.ToString() : String.Empty; } }

        MessageServiceReference.MessageServiceClient messageServiceClient = null;

        public MessageListener(string ipAddress, int portNumber, int heartbeatPeriod, bool isMessageServiceActivated)
        {
            //Parameters
            IpAddressString = ipAddress;
            PortNumber = portNumber;
            HeartbeatPeriod = heartbeatPeriod;
            IsMessageServiceActivated = isMessageServiceActivated;

            if (IsMessageServiceActivated)
            {
                try
                { 
                    //Activate the message service
                    messageServiceClient = new MessageServiceReference.MessageServiceClient();
                    messageServiceClient.Open();

                    Console.WriteLine("Message service is activated."); 
                }
                catch(Exception exception)
                {
                    Console.WriteLine("Message service cannot be reached");
                    Trace.WriteLine(String.Format("Service exception : {0}", exception));
                }
            }

            //Threads
            processingThread = new Thread(new ThreadStart(Processor));
            sendingThread = new Thread(new ThreadStart(Sender));
            storingThread = new Thread(new ThreadStart(Storer));

            //Heart beat Timer
            if (HeartbeatPeriod > 0)
            { 
                TimerCallback tcb = Heartbeat;
                heartbeatTimer = new Timer(tcb, heartbeatEvent, HeartbeatPeriod, HeartbeatPeriod);
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
                processingThread.Start();

                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Listening on " + listener.LocalEndPoint); 

                sendingThread.Start();

                if (IsMessageServiceActivated)
                    storingThread.Start();

                while (true)
                {
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    Socket socket = listener.Accept();

                    Address address = new Address(socket.RemoteEndPoint.ToString());

                    clientsSockets.TryAdd(address, socket);

                    Thread receiveThread = new Thread(new ParameterizedThreadStart(Receiver));

                    receiveThreads.TryAdd(address, receiveThread);
                    receiveThread.Start(socket);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
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

            while (socket.Connected)
            {
                try
                {
                    // Receive buffer.
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];
                    socket.Receive(buffer, 0, Constants.BUFFER_SIZE, 0);

                    string paddedSerializedData = Encoding.UTF8.GetString(buffer, 0, Constants.BUFFER_SIZE);
                    string cleanedUpSerializedData = JsonHelpers.CleanUpPaddedSerializedData(paddedSerializedData);

#if DEBUG
                    // Objects serialized with json.netmf's 4.2 .dll cannot be deserialized when the server is running on Mono
                    Trace.WriteLine(String.Format("JSON      : {0}", cleanedUpSerializedData));
#endif

                    Message receivedMessage = JsonConvert.DeserializeObject<Message>(cleanedUpSerializedData);

                    //HACK : we pass the ip:port address in the Property argument
                    if (receivedMessage.Name.Equals(ControlActions.REGISTER))
                        receivedMessage.Content = socket.RemoteEndPoint.ToString();

                    messagesReceived.Enqueue(receivedMessage);

                    //Console.WriteLine(String.Format("Message     : {0}", receivedMessage));
                    Trace.WriteLine(String.Format("Message   : {0}", receivedMessage));

                    if (IsMessageServiceActivated)       
                        StoreMessage(receivedMessage);

                    processDone.Set();
                }
                catch(Exception exception)
                {
                    Trace.WriteLine(exception);

                    CleanUp(address);
                    break;
                }
            }
        }

        private void CleanUp(Address address)
        {
            string deviceName = devicesManager.GetDeviceNameFromAddress(address);

            Console.WriteLine(String.Format("Device {0} disconnected.", deviceName));

            //Close socket and abort thread
            Thread thread = null;

            Unregister(deviceName);

            if (receiveThreads.TryRemove(address, out thread))
            {
                Socket _socket = null;
                if (clientsSockets.TryRemove(address, out _socket))
                { 
                    if (_socket.Connected)
                        _socket.Close();

                    _socket.Dispose();
                }

                Trace.WriteLine(String.Format("Device {0} socket closed.", deviceName));

                thread.Abort();

                Trace.WriteLine(String.Format("Device {0} thread aborted.", deviceName));
            }
        }

        #endregion 

        #region Processing received messages

        /// <summary>
        /// Message processing thread method
        /// </summary>
        private void Processor()
        {
            Trace.WriteLine("Starting Processor() thread"); 

            while (true)
            {
                processDone.Reset();

                while (messagesReceived.Count > 0)
                {
                    Message message = null;
                    if (messagesReceived.TryDequeue(out message))
                    {
                        PreProcess(message);
                        Process(message);
                        PostProcess(message);
                    }
                }

                processDone.WaitOne();
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
                        SendToAuthorizedDevices(message);
                        break;
                    case ContentTypes.HEARTBEAT:
                        ReturnHeartbeatRequest(message);
                        break;
                    case ContentTypes.HEALTH:
                    case ContentTypes.RESPONSE:
                        throw new NotImplementedException();
                }

                Trace.WriteLine(String.Format("Processed : {0}", message));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(String.Format("Processing exception : {0}", exception));
            }
        }

        protected virtual void PostProcess(Message message)
        {

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

        protected void Register(Address origin, string deviceName)
        {
            devicesManager.AddDevice(deviceName, origin);
            Console.WriteLine(String.Format("Device {0} registered.", deviceName));
            OnRegistrationChanged(new RegistrationEventArgs(deviceName, RegistrationEventArgs.RegistrationEvents.Registered)); 
        }

        protected void Unregister(string deviceName)
        {
            rulesManager.RemoveAllRules(deviceName);
            devicesManager.RemoveDevice(deviceName);
            Console.WriteLine(String.Format("Device {0} unregistered.", deviceName));
            OnRegistrationChanged(new RegistrationEventArgs(deviceName, RegistrationEventArgs.RegistrationEvents.Unregistered)); 
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

        protected void ReturnHeartbeatRequest(Message heartbeatMessage)
        {
            messagesToSend.Enqueue(heartbeatMessage);
        }

        #endregion 

        #endregion

        #region Sending messages

        private void SendToAuthorizedDevices(Message message)
        {
            IEnumerable<string> authorizedDeviceNames = rulesManager.GetAuthorizedDeviceNames(message.ContentType, message.FromDevice, message.ToDevice, message.Name, message.Parameter.ToString());
            foreach (string deviceName in authorizedDeviceNames)
            {
                Message sendToMessage = new Message(message);
                sendToMessage.ReceivingDevice = deviceName;
                messagesToSend.Enqueue(sendToMessage);
            }

            sendDone.Set();
        }

        private void SendToAllDevices(Message message)
        {
            IEnumerable<string> deviceNames = devicesManager.GetAllDeviceNames();
            foreach (string deviceName in deviceNames)
            {
                Message sendToMessage = new Message(message);
                sendToMessage.ReceivingDevice = deviceName;
                messagesToSend.Enqueue(sendToMessage);
            }

            sendDone.Set();
        }

        private void Sender()
        {
            Trace.WriteLine("Starting Sender() thread"); 

            while (true)
            {
                sendDone.Reset();

                while (messagesToSend.Count > 0)
                {
                    Message message = null;
                    if (messagesToSend.TryDequeue(out message))
                        Send(message);
                }
                
                sendDone.WaitOne();
            }
        }

        protected virtual void PostSend(Message message)
        {

        }

        private void Send(Message message)
        {
            try
            {
                string serializedMessage = JsonConvert.SerializeObject(message);
                serializedMessage = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);

                // Convert the string data to byte data using UTF8 encoding.
                byte[] byteData = Encoding.UTF8.GetBytes(serializedMessage);

                Address destinationAddress = devicesManager.GetAddressFromDeviceName(message.ReceivingDevice);
                if (destinationAddress != null)
                {
                    Socket socket = clientsSockets.FirstOrDefault(x => x.Key.Equals(destinationAddress)).Value as Socket;
                    if (socket != null && socket.Connected)
                    {
                        socket.Send(byteData, 0, Constants.BUFFER_SIZE, 0);

                        PostSend(message);

                        Trace.WriteLine(String.Format("Sent        : {0}", message));
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
                Trace.WriteLine(String.Format("Send exception : {0}", exception));
            }
        }

        #endregion 
        
        #region Heartbeat messages

        public void Heartbeat(Object stateInfo)
        {
            foreach (Device device in devicesManager.Devices)
            {
                Message heartbeat = Message.InstanciateHeartbeatMessage();
                SendToAllDevices(heartbeat);
            }

            heartbeatEvent.Set();
        }

        #endregion 

        #region Message Service

        private void StoreMessage(Message message)
        {
            //HACK : the Timestamp produced by some devices being unreliable, we override it with a Timestamp produced by the server.
            message.Timestamp = DateTime.Now;
            messagesToStore.Enqueue(message);
            storeDone.Set();
        }

        private void Storer()
        {
            Trace.WriteLine("Starting Sender() thread");

            while (true)
            {
                storeDone.Reset();

                while (messagesToStore.Count > 0)
                {
                    Message message = null;
                    if (messagesToStore.TryDequeue(out message))
                        Store(message);
                }

                storeDone.WaitOne();
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
    }
}
