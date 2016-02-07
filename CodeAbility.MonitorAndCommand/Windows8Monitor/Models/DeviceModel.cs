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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;

using CodeAbility.MonitorAndCommand.Models;


namespace CodeAbility.MonitorAndCommand.Windows8Monitor.Models
{
    public class DeviceModel : INotifyPropertyChanged
    {
        const int BLINK_TIME = 100;

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion 

        #region Public Properties

        public String name;
        public String Name 
        { 
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }

        public bool messageReceived = false;
        public bool MessageReceived
        {
            get { return messageReceived; }
            set
            {
                messageReceived = value;
                OnPropertyChanged("MessageReceived");
            }
        }

        public bool messageSent = false;
        public bool MessageSent
        {
            get { return messageSent; }
            set
            {
                messageSent = value;
                OnPropertyChanged("MessageSent");
            }
        }

        #endregion 

        public DeviceModel(string deviceName)
        {
            Name = deviceName;
        }


        public void SetConnectionState(RegistrationEventArgs.RegistrationEvents registrationEvent)
        {
            IsConnected = registrationEvent == RegistrationEventArgs.RegistrationEvents.Registered;
        }

        public void HandleReceivedMessageEvent()
        {
            if (IsConnected)
                MessageReceivedHandler();
        }

        public void HandleSentMessageEvent()
        {
            if (IsConnected)
                MessageSentHandler();
        }

        async void MessageSentHandler()
        {
            await ThreadPool.RunAsync((workItem) => MessageSentFlash());
        }

        async void MessageSentFlash()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessageSent = true;
            });

            await Task.Delay(TimeSpan.FromMilliseconds(BLINK_TIME));

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessageSent = false;
            });
        }

        async void MessageReceivedHandler()
        {
            await ThreadPool.RunAsync((workItem) => MessageReceivedFlash());
        }

        async void MessageReceivedFlash()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessageReceived = true;
            });

            await Task.Delay(TimeSpan.FromMilliseconds(BLINK_TIME));

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessageReceived = false;
            });
        }
    }
}
