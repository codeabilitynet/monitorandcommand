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
        const string ALL = "*";

        //Threads
        private Thread sendingThread = null;
        private Thread processingThread = null;
        private Timer heartbeatTimer = null;

        //ManualResetEvent instances signal completion.
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent processDone = new ManualResetEvent(false);
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
        /// Object managing references to devices (connected clients)
        /// </summary>
        DevicesManager devicesManager = new DevicesManager();

        /// <summary>
        /// Object managing the "message routing rules" derived from publish/subscribe messages sent by the clients
        /// </summary>
        RulesManager rulesManager = new RulesManager(); 

        int PortNumber { get; set; }
        int HeartbeatPeriod { get; set; }

        bool IsMessageServiceActivated { get; set; }

        Socket listener = null;

        public string LocalEndPoint { get { return (listener != null) ? listener.LocalEndPoint.ToString() : String.Empty; } }

        MessageServiceReference.MessageServiceClient messageServiceClient = null;

        public MessageListener(int portNumber, int heartbeatPeriod, bool isMessageServiceActivated)
        {
            //Parameters
            PortNumber = portNumber;
            HeartbeatPeriod = heartbeatPeriod;
            IsMessageServiceActivated = isMessageServiceActivated;

            if (IsMessageServiceActivated)
            {
                //Activate the message service
                messageServiceClient = new MessageServiceReference.MessageServiceClient();
                messageServiceClient.Open();

                Console.WriteLine("Message service is activated."); 
            }

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

        /// <summary>
        /// Start listening for messages
        /// </summary>
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

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        #region Receiving messages

        /// <summary>
        /// Message receiving thread method
        /// </summary>
        /// <param name="socketObject">receiving socket</param>
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

                    string paddedSerializedData = Encoding.UTF8.GetString(buffer, 0, Constants.BUFFER_SIZE);
                    string cleanedUpSerializedData = JsonHelpers.CleanUpPaddedSerializedData(paddedSerializedData);
                    Message receivedMessage = JsonConvert.DeserializeObject<Message>(cleanedUpSerializedData);

                    //HACK : we pass the ip:port address in the Property argument
                    if (receivedMessage.Name.Equals(ControlActions.REGISTER))
                        receivedMessage.Content = socket.RemoteEndPoint.ToString();

                    messagesReceived.Enqueue(receivedMessage);

                    if (IsMessageServiceActivated)       
                        StoreMessage(receivedMessage);

                    processDone.Set();
                }
                catch(Exception)
                {
                    Address address = new Address(socket.RemoteEndPoint.ToString());
                    string deviceName = devicesManager.GetDeviceNameFromAddress(address);

                    Console.WriteLine(String.Format("Device {0} disconnected.", deviceName));

                    //Close socket and abort thread
                    Thread thread = null;
                    devicesManager.RemoveDevice(devicesManager.GetDeviceNameFromAddress(address));

                    if (receiveThreads.TryRemove(address, out thread))
                    {
                        Socket _socket = null;
                        if (clientsSockets.TryRemove(address, out _socket))
                            _socket.Close();

                        thread.Abort();

                        Trace.WriteLine(String.Format("Device {0} thread & socket cleaned up.", deviceName));
                    }

                    break;
                }
            }
        }

        #endregion 

        #region Processing received messages

        /// <summary>
        /// Message processing thread method
        /// </summary>
        private void Processor()
        {
            while (true)
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

        protected void Process(Message message)
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
                    HandleReturnedHeartbeat(message);
                    break;
                case ContentTypes.HEALTH:
                case ContentTypes.RESPONSE:
                    throw new NotImplementedException();
            }
        }

        #region Server actions

        private void ProcessServerAction(Message message)
        {
            try
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
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
            }
        }

        protected void Register(Address origin, string deviceName)
        {
            devicesManager.AddDevice(deviceName, origin);
            Console.WriteLine(String.Format("Device {0} registered.", deviceName)); 
        }

        protected void Unregister(string deviceName)
        {
            devicesManager.RemoveDevice(deviceName);
            Console.WriteLine(String.Format("Device {0} unregistered.", deviceName)); 
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

            //Health monitoring is not implemented yet
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

        private void Send(Message message)
        {
            string serializedMessage = JsonConvert.SerializeObject(message);
            serializedMessage = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);

            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(serializedMessage);

            Address destinationAddress = devicesManager.GetAddressFromDeviceName(message.ReceivingDevice);
            if (destinationAddress != null)
            {
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
            try
            { 
                if (messageServiceClient.State == System.ServiceModel.CommunicationState.Opened)
                    messageServiceClient.StoreMessageAsync(message);
            }
            catch(Exception exception)
            {
                Trace.WriteLine(exception);
            }
        }

        #endregion 
    }
}
