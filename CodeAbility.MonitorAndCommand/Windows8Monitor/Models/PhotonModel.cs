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

using CodeAbility.MonitorAndCommand.Models;
using Windows.UI.Xaml;


namespace CodeAbility.MonitorAndCommand.Windows8Monitor.Models
{
    public class PhotonModel : INotifyPropertyChanged
    {
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

        private string name = String.Empty;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public string ShortName
        {
            get { return Name.Last().ToString(); }
        }

        private string temperature = String.Empty;
        public string Temperature
        {
            get { return temperature; }
            set
            {
                temperature = value;
                OnPropertyChanged("Temperature");
            }
        }

        private string humidity = String.Empty;
        public string Humidity
        {
            get { return humidity; }
            set
            {
                humidity = value;
                OnPropertyChanged("Humidity");
            }
        }

        private bool boardLED = false;
        public bool BoardLED
        {
            get { return boardLED; }
            set
            {
                boardLED = value;
                OnPropertyChanged("BoardLED");
            }
        }

        private bool redLED = false;
        public bool RedLED
        {
            get { return redLED; }
            set
            {
                redLED = value;
                OnPropertyChanged("RedLED");
            }
        }

        private bool greenLED = false;
        public bool GreenLED
        {
            get { return greenLED; }
            set
            {
                greenLED = value;
                OnPropertyChanged("GreenLED");
            }
        }

        public PhotonModel(string name) 
        {
            Name = name; 
        }
    }
}
