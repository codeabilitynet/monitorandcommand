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
//using Windows.Networking;
//using Windows.Networking.Sockets;
//using Windows.Storage.Streams;

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

        /// <summary>
        /// Attempt a TCP socket connection to the given host over the given port
        /// </summary>
        /// <param name="hostName">The name of the host</param>
        /// <param name="portNumber">The port number to connect</param>
        /// <returns>A string representing the result of this connection attempt</returns>
        public string Connect(string hostName, int portNumber)
        {
            string result = string.Empty;

            // Create DnsEndPoint. The hostName and port are passed in to this method.
            DnsEndPoint hostEntry = new DnsEndPoint(hostName, portNumber);

            // Create a stream-based, TCP socket using the InterNetwork Address Family. 
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Create a SocketAsyncEventArgs object to be used in the connection request
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = hostEntry;

            // Inline event handler for the Completed event.
            // Note: This event handler was implemented inline in order to make this method self-contained.
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
            {
                // Retrieve the result of this request
                result = e.SocketError.ToString();

                IsConnected = result == "Success";

                //if (IsConnected)
                //    receivingThread.Start(); 

                Receive();

                // Signal that the request is complete, unblocking the UI thread
                clientDone.Set();
            });

            // Sets the state of the event to nonsignaled, causing threads to block
            clientDone.Reset();

            // Make an asynchronous Connect request over the socket
            socket.ConnectAsync(socketEventArg);

            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            clientDone.WaitOne(TIMEOUT_MILLISECONDS);

            return result;
        }

        /// <summary>
        /// Send the given data to the server using the established connection
        /// </summary>
        /// <param name="data">The data to send to the server</param>
        /// <returns>The result of the Send request</returns>
        public string Send(string data)
        {
            string response = "Operation Timeout";

            // We are re-using the _socket object initialized in the Connect method
            if (socket != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                // Set properties on context object
                socketEventArg.RemoteEndPoint = socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                // Inline event handler for the Completed event.
                // Note: This event handler was implemented inline in order 
                // to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();

                    // Unblock the UI thread
                    clientDone.Set();
                });

                data = data.PadRight(256, '.');
                byte[] payload = Encoding.UTF8.GetBytes(data);
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                // Sets the state of the event to nonsignaled, causing threads to block
                clientDone.Reset();

                // Make an asynchronous Send request over the socket
                socket.SendAsync(socketEventArg);

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            else
            {
                response = "Socket is not initialized";
            }

            return response;
        }


        /// <summary>
        /// Receive data from the server using the established socket connection
        /// </summary>
        /// <returns>The data received from the server</returns>
        public void Receive()
        {
            while(IsConnected)
            { 
                // We are receiving over an established socket connection
                if (socket != null)
                {
                    // Create SocketAsyncEventArgs context object
                    SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                    socketEventArg.RemoteEndPoint = socket.RemoteEndPoint;

                    // Setup the buffer to receive the data
                    socketEventArg.SetBuffer(new Byte[Constants.BUFFER_SIZE], 0, Constants.BUFFER_SIZE);

                    // Inline event handler for the Completed event.
                    // Note: This even handler was implemented inline in order to make 
                    // this method self-contained.
                    socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                    {
                        string response = string.Empty;
                        if (e.SocketError == SocketError.Success)
                        {
                            // Retrieve the data from the buffer
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

                    // Make an asynchronous Receive request over the socket
                    socket.ReceiveAsync(socketEventArg);

                    // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                    // If no response comes back within this time then proceed
                    clientDone.WaitOne();
                }
            }
        }

        /// <summary>
        /// Closes the Socket connection and releases all associated resources
        /// </summary>
        public void Close()
        {
            if (socket != null)
            {
                socket.Close();
            }
        }

    }
}
