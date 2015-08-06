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
        public delegate void DataStringReceivedEventHandler(object sender, DataStringEventArgs e);
        public event DataStringReceivedEventHandler DataStringReceived;
        protected void OnDataStringReceived(DataStringEventArgs e)
        {
            if (DataStringReceived != null)
                DataStringReceived(this, e);
        }

        Socket socket = null;

        static ManualResetEvent clientDone = new ManualResetEvent(false);

        // Define a timeout in milliseconds for each asynchronous call. If a response is not received within this 
        // timeout period, the call is aborted.
        const int TIMEOUT_MILLISECONDS = 5000;

        public bool IsConnected { get; set; }

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
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                socketEventArg.RemoteEndPoint = socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();

                    clientDone.Set();
                });

                string paddedData = CodeAbility.MonitorAndCommand.Helpers.JsonHelpers.PadSerializedMessage(data, Constants.BUFFER_SIZE);
                byte[] payload = Encoding.UTF8.GetBytes(paddedData);
                socketEventArg.SetBuffer(payload, 0, Constants.BUFFER_SIZE);

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
