using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.Models
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

        public DeviceData(string deviceName)
        {
            Name = deviceName;
        }

        public void HandleReceivedMessageEvent()
        {
            MessageReceivedHandler();
        }

        public void HandleSentMessageEvent()
        {
            MessageSentHandler();
        }

        #region Helpers 

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

        #endregion 
    }
}
