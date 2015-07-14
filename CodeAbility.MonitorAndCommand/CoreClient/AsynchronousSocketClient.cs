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
using CodeAbility.MonitorAndCommand.Helpers;

namespace CodeAbility.MonitorAndCommand.Client
{
    //https://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx

    public class AsynchronousClient
    {
        #region Message Event Handler

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

        // A delegate type for hooking up change notifications.
        public delegate void MessageReceivedEventHandler(object sender, MessageEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event MessageReceivedEventHandler MessageReceived;

        // Invoke the Changed event; called whenever list changes
        protected void OnMessageReceived(MessageEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        #endregion 

        #region Public Properties
        public string IpAddress { get; protected set; }

        public int PortNumber { get; protected set; }

        public bool IsConnected { get { return client != null && client.Connected; } }

        #endregion 

        private Socket client = null;

        //Threads
        private Thread receiveThread = null;
        private Thread sendThread = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        private ConcurrentQueue<Message> messagesToSend = new ConcurrentQueue<Message>();

        #region Public Methods

        public AsynchronousClient(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            PortNumber = port;

            receiveThread = new Thread(new ThreadStart(Receiver));
            sendThread = new Thread(new ThreadStart(Sender));
        }

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.GetHostEntry(IpAddress);
                IPAddress _ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(_ipAddress, PortNumber);

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                receiveThread.Start();
                sendThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendRegisterCommand(string deviceName)
        {
            SendCommand(Commands.REGISTER_DEVICE, deviceName, String.Empty);
        }

        public void SendPublishCommand(string parameterName)
        {
            SendCommand(Commands.PUBLISH_DATA, parameterName, String.Empty);
        }

        public void SendSubscribeCommand(string deviceName, string parameterName)
        {
            SendCommand(Commands.SUBSCRIBE_TO, parameterName, deviceName);
        }

        public void SendUnsubscribeCommand(string deviceName, string parameterName)
        {
            SendCommand(Commands.UNSUBSCRIBE_FROM, parameterName, deviceName);
        }

        public void SendHeartBeatCommand()
        {
            SendCommand(Commands.HEARTBEAT, string.Empty, string.Empty);
        }

        public void SendCustomCommand(string command, string parameter, string deviceName)
        {
            SendCommand(command, parameter, deviceName);
        }

        public void SendData(string name, string value)
        {
            Message message = Message.InstanciateDataMessage(name, value);
            message.SetOrigin(client.LocalEndPoint.ToString());

            messagesToSend.Enqueue(message);

            sendDone.Set();
        }

        #endregion 

        #region Internals

        protected void SendCommand(string command, string parameter, string destination)
        {
            Message message = Message.InstanciateCommandMessage(command, parameter, destination);
            message.SetOrigin(client.LocalEndPoint.ToString());

            messagesToSend.Enqueue(message);

            sendDone.Set();
        }

        private void Sender()
        {
            while (client.Connected)
            {
                try
                {
                    if (messagesToSend.Count > 0)
                    {
                        Message message = null;
                        if (messagesToSend.TryDequeue(out message))
                            Send(message);
                    }
                    else
                        sendDone.WaitOne();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        private void Send(Message message)
        {
            string serializedData = JsonConvert.SerializeObject(message);
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(serializedData);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try 
            {
                // Retrieve the socket from the state object.
                //Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receiver()
        {
            while (client.Connected)
            {
                try
                {
                    // Create the state object.
                    StateObject state = new StateObject();
                    state.workSocket = client;

                    // Begin receiving the data from the remote device.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    receiveDone.WaitOne();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                string serializedData = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                Message message = JsonConvert.DeserializeObject<Message>(serializedData);
                if (message != null)
                {
                    if (message.Type == ContentTypes.COMMAND)
                        OnCommandReceived(new MessageEventArgs(message));
                    else if (message.Type == ContentTypes.DATA)
                        OnMessageReceived(new MessageEventArgs(message));
                }

                receiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Shutdown()
        {
            sendThread.Abort();
            receiveThread.Abort();

            client.Shutdown(SocketShutdown.Both);
        }

        #endregion 
    }
}
