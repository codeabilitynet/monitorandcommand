using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Server.Models
{
    public class DeviceData : INotifyPropertyChanged
    {
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

        public double messagesPerMinute = 0;
        public double MessagesPerMinute
        {
            get { return messagesPerMinute; }
            set
            {
                messagesPerMinute = value;
                OnPropertyChanged("MessagesPerMinute");
            }
        }

        public DeviceData(string deviceName)
        {
            Name = deviceName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
