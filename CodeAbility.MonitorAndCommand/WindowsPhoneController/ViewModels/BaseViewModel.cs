using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
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
