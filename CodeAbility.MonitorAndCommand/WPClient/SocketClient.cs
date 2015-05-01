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
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;

using Newtonsoft.Json;

using CodeAbility.MonitorAndCommand.Models;


namespace CodeAbility.MonitorAndCommand.WPClient
{
    internal class SocketClient
    {
        // A delegate type for hooking up change notifications.
        public delegate void DataStringReceivedEventHandler(object sender, DataStringEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event DataStringReceivedEventHandler DataStringReceived;

        // Invoke the Changed event; called whenever list changes
        protected void OnDataStringReceived(DataStringEventArgs e)
        {
            if (DataStringReceived != null)
                DataStringReceived(this, e);
        }

        Socket socket = null;

        // Signaling object used to notify when an asynchronous operation is completed
        static ManualResetEvent clientDone = new ManualResetEvent(false);

        // Define a timeout in milliseconds for each asynchronous call. If a response is not received within this 
        // timeout period, the call is aborted.
        const int TIMEOUT_MILLISECONDS = 5000;

        public bool IsConnected { get; set; }

        //StreamSocket _socket;

        private Queue<Message> messagesToSend = new Queue<Message>();

        public string LocalEndPoint { get { return socket.LocalEndPoint.ToString(); } }

        //protected Thread receivingThread;

        public SocketClient()
        {
            //receivingThread = new Thread(Receiver);
        }

        public void Cancel()
        {
            socket.Dispose();
            socket = null;
        }

        public string Connect(string hostName, int portNumber)
        {
            string result = string.Empty;

            DnsEndPoint hostEntry = new DnsEndPoint(hostName, portNumber);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = hostEntry;

            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
            {
                // Retrieve the result of this request
                result = e.SocketError.ToString();

                IsConnected = result == "Success";

                //if (IsConnected)
                //    receivingThread.Start(); 

                Receive();

                clientDone.Set();
            });

            clientDone.Reset();

            socket.ConnectAsync(socketEventArg);

            clientDone.WaitOne(TIMEOUT_MILLISECONDS);

            return result;
        }

        public string Send(string data)
        {
            string response = "Operation Timeout";

            if (socket != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                socketEventArg.RemoteEndPoint = socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();

                    clientDone.Set();
                });

                data = data.PadRight(256, '.');
                byte[] payload = Encoding.UTF8.GetBytes(data);
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                clientDone.Reset();

                socket.SendAsync(socketEventArg);

                clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            else
            {
                response = "Socket is not initialized";
            }

            return response;
        }

        public void Receive()
        {
            while(IsConnected)
            { 
                if (socket != null)
                {
                    SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                    socketEventArg.RemoteEndPoint = socket.RemoteEndPoint;

                    socketEventArg.SetBuffer(new Byte[Constants.BUFFER_SIZE], 0, Constants.BUFFER_SIZE);

                    socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                    {
                        string response = string.Empty;
                        if (e.SocketError == SocketError.Success)
                        {
                            string receivedData = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                            OnDataStringReceived(new DataStringEventArgs(receivedData));
                        }
                        else
                        {
                            response = e.SocketError.ToString();
                        }

                        clientDone.Set();
                    });

                    clientDone.Reset();

                    socket.ReceiveAsync(socketEventArg);

                    clientDone.WaitOne();
                }
            }
        }

        public void Close()
        {
            if (socket != null)
            {
                socket.Close();
            }
        }

    }
}
