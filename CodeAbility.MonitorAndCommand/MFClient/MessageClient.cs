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

        #endregion 

        Socket socket = null;

        private Thread receiveThread = null;

        public MessageClient(string deviceName, string ipAddress, int port)
        { 
            DeviceName = deviceName;
            IpAddress = ipAddress;
            PortNumber = port;

            receiveThread = new Thread(new ThreadStart(Receiver));
        }

        #region Public Methods

        public void Start()
        {
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(IpAddress);
                IPAddress ipAddress = ipHostEntry.AddressList[0];

                IPEndPoint endpoint = new IPEndPoint(ipAddress, PortNumber);
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Connect(endpoint);

                receiveThread.Start();

                Register();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                this.receiveThread.Abort();
                this.socket.Close();
            }
            catch (Exception)
            {
                throw;
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

            //TODO : add queuing

            Send(message);
        }

        #endregion 

        private void EnqueueMessage(Message message)
        {          
            //TODO : add queuing

            Send(message);
        }

        private void Send(Message message)
        {
            string serializedMessage = JsonSerializer.SerializeObject(message);
            string paddedSerializedData = JsonHelpers.PadSerializedMessage(serializedMessage, Constants.BUFFER_SIZE);

            this.socket.Send(Encoding.UTF8.GetBytes(paddedSerializedData));
        }

        #region Threads

        private void Receiver()
        {
            Thread.Sleep(1000);

            while (true)
            {
                try
                {
                    // Receive buffer.
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];

                    // Begin receiving the data from the remote device.
                    socket.Receive(buffer, 0, Constants.BUFFER_SIZE, 0);

                    char[] dataChars = Encoding.UTF8.GetChars(buffer, 0, Constants.BUFFER_SIZE);
                    string paddedSerializedData = new string(dataChars);
                    string serializedMessage = JsonHelpers.CleanUpSerializedData(paddedSerializedData.ToString());

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
                        if (message.ContentType == ContentTypes.COMMAND)
                            OnCommandReceived(new MessageEventArgs(message));
                    }
                }
                catch (Exception exception)
                {
                    throw;
                }
            }
        }

        #endregion 
    }
}
