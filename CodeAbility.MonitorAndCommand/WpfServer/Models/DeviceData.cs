using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models; 

namespace CodeAbility.MonitorAndCommand.WpfServer.Models
{
    public class DeviceData : INotifyPropertyChanged
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

        public int incomingMessagesCount = 0;
        public int IncomingMessagesCount
        {
            get { return incomingMessagesCount; }
            set
            {
                incomingMessagesCount = value;
                OnPropertyChanged("IncomingMessagesCount");
            }
        }

        public int outgoingMessagesCount = 0;
        public int OutgoingMessagesCount
        {
            get { return outgoingMessagesCount; }
            set
            {
                outgoingMessagesCount = value;
                OnPropertyChanged("OutgoingMessagesCount");
            }
        }

        public double incomingMessagesAverage = 0;
        public double IncomingMessagesAverage
        {
            get { return incomingMessagesAverage; }
            set
            {
                incomingMessagesAverage = value;
                OnPropertyChanged("IncomingMessagesAverage");
            }
        }

        public double outgoingMessagesAverage = 0;
        public double OutgoingMessagesAverage
        {
            get { return outgoingMessagesAverage; }
            set
            {
                outgoingMessagesAverage = value;
                OnPropertyChanged("OutgoingMessagesAverage");
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

        #region Private attributes 

       public int CountingPeriodInSeconds { get; set; }

        int numberOfPeriods = 0;

        int messagesReceivedDuringCurrentPeriod = 0;
        int messagesSentDuringCurrentPeriod = 0;

        int?[] messagesReceived;
        int?[] messagesSent;

        int period = 0;

        #endregion 

        public DeviceData(string deviceName, int countingPeriodInSeconds)
        {
            Name = deviceName;
            CountingPeriodInSeconds = countingPeriodInSeconds;

            numberOfPeriods = (int)(60 / CountingPeriodInSeconds);

            messagesReceived = new int?[numberOfPeriods];
            messagesSent = new int?[numberOfPeriods];
        }


        public void SetConnectionState(RegistrationEventArgs.RegistrationEvents registrationEvent)
        {
            IsConnected = registrationEvent == RegistrationEventArgs.RegistrationEvents.Registered;
        }

        public void HandleReceivedMessageEvent()
        {
            IncomingMessagesCount++;
            messagesReceivedDuringCurrentPeriod++;
            MessageReceivedHandler();
        }

        public void CountMessagesOverElaspedMinute()
        {
            messagesReceived[period] = messagesReceivedDuringCurrentPeriod;
            messagesSent[period] = messagesSentDuringCurrentPeriod;

            IncomingMessagesAverage = CountMessagesOverElapsedMinute(messagesReceived);
            OutgoingMessagesAverage = CountMessagesOverElapsedMinute(messagesSent);

            messagesReceivedDuringCurrentPeriod = 0;
            messagesSentDuringCurrentPeriod = 0;

            period++;
            if (period >= numberOfPeriods)
                period = 0;
        }

        public void HandleSentMessageEvent()
        {
            OutgoingMessagesCount++;
            messagesSentDuringCurrentPeriod++;
            MessageSentHandler();
        }

        double CountMessagesOverElapsedMinute(int?[] counters)
        {
            int messages = 0;

            foreach (int? counter in counters)
            {
                if (counter.HasValue)
                {
                    messages += counter.Value;                   
                }          
            }

            return (double)messages;
        }

        void MessageSentHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(MessageSentFlash);
            thread.Start();
        }

        void MessageSentFlash()
        {
            MessageSent = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            MessageSent = false;
        }

        void MessageReceivedHandler()
        {
            System.Threading.Thread thread = new System.Threading.Thread(MessageReceivedFlash);
            thread.Start();
        }

        void MessageReceivedFlash()
        {
            MessageReceived = true;
            System.Threading.Thread.Sleep(BLINK_TIME);
            MessageReceived = false;
        }
    }
}
