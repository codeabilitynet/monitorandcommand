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
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.WpfServer.Models;
using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Environment; 

namespace CodeAbility.MonitorAndCommand.WpfServer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        const int COMPUTATION_PERIOD_IN_MILLISECONDS = 50; 
        const int COMPUTATION_PERIOD_IN_SECONDS = 1; 

        protected List<DeviceData> devicesData = new List<DeviceData>();
        public ObservableCollection<DeviceData> DevicesData
        {
            get { return new ObservableCollection<DeviceData>(devicesData); }
        }

        private ConcurrentQueue<RegistrationEventArgs> registrationEventArgs = new ConcurrentQueue<RegistrationEventArgs>();
        private ConcurrentQueue<MessageEventArgs> receivedEventArgs = new ConcurrentQueue<MessageEventArgs>();
        private ConcurrentQueue<MessageEventArgs> sentEventArgs = new ConcurrentQueue<MessageEventArgs>();

        Timer uiNotifyTimer; 
        Timer computationTimer;

        public MainWindowViewModel()
            : base()
        {
            devicesData.Add(new DeviceData(Environment.Devices.PIBRELLA, COMPUTATION_PERIOD_IN_SECONDS));
            devicesData.Add(new DeviceData(Environment.Devices.NETDUINO_PLUS, COMPUTATION_PERIOD_IN_SECONDS));
            devicesData.Add(new DeviceData(Environment.Devices.NETDUINO_3, COMPUTATION_PERIOD_IN_SECONDS));
            devicesData.Add(new DeviceData(Environment.Devices.DATA_GENERATOR, COMPUTATION_PERIOD_IN_SECONDS));
            devicesData.Add(new DeviceData(Environment.Devices.WINDOWS_PHONE, COMPUTATION_PERIOD_IN_SECONDS));
            devicesData.Add(new DeviceData(Environment.Devices.WPF_MONITOR, COMPUTATION_PERIOD_IN_SECONDS));

            new Thread(StartServer).Start();
        }

        void StartServer()
        {
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);
            int heartbeatPeriod = Int32.Parse(ConfigurationManager.AppSettings["HeartbeatPeriod"]);
            bool isMessageServiceActivated = ConfigurationManager.AppSettings["IsMessageServiceActivated"].Equals("true");
           
            ExtendedMessageListener messageListener = new ExtendedMessageListener(portNumber, heartbeatPeriod, isMessageServiceActivated);
            messageListener.RegistrationChanged += messageListener_RegistrationChanged;
            messageListener.MessageReceived += messageListener_MessageReceived;
            messageListener.MessageSent += messageListener_MessageSent;

            TimerCallback uiNotifyTimerCallBack = DoNotify;
            uiNotifyTimer = new Timer(uiNotifyTimerCallBack, null, 0, COMPUTATION_PERIOD_IN_MILLISECONDS); 

            TimerCallback computationTimerCallBack = DoCompute;
            computationTimer = new Timer(computationTimerCallBack, null, 0, COMPUTATION_PERIOD_IN_SECONDS * 1000);

            messageListener.StartListening();
        }

        #region Timer methods 

        private void DoNotify(object state)
        {
            while (registrationEventArgs.Count > 0)
            {
                RegistrationEventArgs eventArgs = null;
                if (registrationEventArgs.TryDequeue(out eventArgs))
                    NotifyRegistrationChanged(eventArgs);
            }

            while (receivedEventArgs.Count > 0)
            {
                MessageEventArgs args = null;
                if (receivedEventArgs.TryDequeue(out args))
                    NotifyMessageReceived(args);
            }

            while (sentEventArgs.Count > 0)
            {
                MessageEventArgs args = null;
                if (sentEventArgs.TryDequeue(out args))
                    NotifyMessageSent(args);
            }
        }

        private void DoCompute(object state)
        {
            foreach (DeviceData deviceData in devicesData)
            {
                deviceData.CountMessagesOverElaspedMinute();
            }
        }

        #endregion 

        #region Server events 

        void messageListener_RegistrationChanged(object sender, RegistrationEventArgs e)
        {
            registrationEventArgs.Enqueue(e); 
        }

        void messageListener_MessageReceived(object sender, MessageEventArgs e)
        {
            receivedEventArgs.Enqueue(e); 
        }

        void messageListener_MessageSent(object sender, MessageEventArgs e)
        {
            sentEventArgs.Enqueue(e); 
        }

        #endregion 

        #region Notification Methods

        void NotifyRegistrationChanged(RegistrationEventArgs args)
        {
            DeviceData deviceData = devicesData.FirstOrDefault(x => x.Name == args.DeviceName);
            if (deviceData != null)
                deviceData.SetConnectionState(args.RegistrationEvent);
        }

        void NotifyMessageReceived(MessageEventArgs args)
        {
            DeviceData deviceData = devicesData.FirstOrDefault(x => x.Name == args.SendingDevice);
            if (deviceData != null)
                deviceData.HandleSentMessageEvent(); //Received by the server, but sent by the device
        }

        void NotifyMessageSent(MessageEventArgs args)
        {
            DeviceData deviceData = devicesData.FirstOrDefault(x => x.Name == args.ReceivingDevice);
            if (deviceData != null)
                deviceData.HandleReceivedMessageEvent(); //Sent by the server, but received by the device
        }

        #endregion 
    }
}
