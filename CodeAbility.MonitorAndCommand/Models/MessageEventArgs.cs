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

#if MF_FRAMEWORK_VERSION_V4_2
using Microsoft.SPOT;
#endif

namespace CodeAbility.MonitorAndCommand.Models
{
    public class MessageEventArgs : EventArgs
    {
        public string SendingDevice { get; set; }

        public string FromDevice { get; set; }

        public string ToDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        public ContentTypes ContentType { get; set; }
#else
        public int ContentType { get; set; }
#endif
        public string Name { get; set; }
        public object Parameter { get; set; }
        public object Content { get; set; }
        public DateTime Timestamp { get; set; }

        public MessageEventArgs(Message receivedMessage)
            : base()
        {
            SendingDevice = receivedMessage.SendingDevice;
            FromDevice = receivedMessage.FromDevice;
            ToDevice = receivedMessage.ToDevice;
            ContentType = receivedMessage.ContentType;
            Name = receivedMessage.Name;
            Parameter = receivedMessage.Parameter; 
            Content = receivedMessage.Content;
            Timestamp = receivedMessage.Timestamp;
        }

        public override string ToString()
        {
            const string NOT_AVAILABLE = "N/A";

#if !MF_FRAMEWORK_VERSION_V4_2
            string parameter = String.IsNullOrEmpty(Parameter.ToString()) ? NOT_AVAILABLE : Parameter.ToString();
            string content = String.IsNullOrEmpty(Parameter.ToString()) ? NOT_AVAILABLE : Parameter.ToString();

            return String.Format("{0}, {1}: {2}, {3} - From:{4}, To:{5}, Parameter:{6}, Content:{7}", Timestamp, SendingDevice, ContentType, Name, FromDevice, ToDevice, parameter, Content);
#else 
            string parameter =  (Parameter != null) ? Parameter.ToString() : NOT_AVAILABLE;
            string content = (Content != null) ? Content.ToString() : NOT_AVAILABLE;

            return Timestamp + ", "
                 + SendingDevice + ": "
                 + ContentType + ", "
                 + Name + " - "
                 + " From:" + FromDevice
                 + ", To:" + ToDevice
                 + ", Parameter: " + parameter
                 + ", Content: " + content;
#endif

        }
    }
}
