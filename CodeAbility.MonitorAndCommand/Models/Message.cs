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
using System.Net;

#if !MF_FRAMEWORK_VERSION_V4_2
using System.Runtime.Serialization;
#if !PORTABLE
using System.ServiceModel;
#endif 
#endif

namespace CodeAbility.MonitorAndCommand.Models
{

#if !MF_FRAMEWORK_VERSION_V4_2
    [DataContract]
#endif
    public class Message
    {
        #region Properties

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public string SendingDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public string ReceivingDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public string FromDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public string ToDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
        public ContentTypes ContentType { get; set; }
#else
        public int ContentType { get; set; }
#endif

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public string Name { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public object Parameter { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public object Content { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2
        [DataMember]
#endif
        public DateTime Timestamp { get; set; }

        #endregion 
        
        const string SERVER = "SERVER";
        const string HEARTBEAT = "HEARTBEAT";
        const string ALL = "*";

        const string MESSAGE_COUNT = "MESSAGE_COUNT";

        #region Static Helpers

        public static Message InstanciateRegisterMessage(string sendingDevice)
        {
            return new Message(sendingDevice, sendingDevice, SERVER, ContentTypes.CONTROL, ControlActions.REGISTER, String.Empty, String.Empty);
        }

        public static Message InstanciateUnregisterMessage(string sendingDevice)
        {
            return new Message(sendingDevice, String.Empty, SERVER, ContentTypes.CONTROL, ControlActions.UNREGISTER, String.Empty, String.Empty);
        }

#if !MF_FRAMEWORK_VERSION_V4_2
        public static Message InstanciatePublishMessage(string sendingDevice, string toDevice, string publicationSource, string publicationName)
#else
        public static Message InstanciatePublishMessage(string sendingDevice, string toDevice, string publicationSource, string publicationName)
#endif
        {
            return new Message(sendingDevice, sendingDevice, toDevice, ContentTypes.CONTROL, ControlActions.PUBLISH, publicationSource, publicationName);
        }

        public static Message InstanciateUnpublishMessage(string sendingDevice, string toDevice, string publicationSource, string publicationName)
        {
            return new Message(sendingDevice, sendingDevice, toDevice, ContentTypes.CONTROL, ControlActions.UNPUBLISH, publicationSource, publicationName);
        }

        public static Message InstanciateSubscribeMessage(string sendingDevice, string fromDevice, string toDevice, string publicationSource, string publicationName)
        {
            return new Message(sendingDevice, fromDevice, toDevice, ContentTypes.CONTROL, ControlActions.SUBSCRIBE, publicationSource, publicationName);
        }

        public static Message InstanciateSubscribeMessage(string sendingDevice, string fromDevice, string toDevice)
        {
            return new Message(sendingDevice, fromDevice, toDevice, ContentTypes.CONTROL, ControlActions.SUBSCRIBE, ALL, ALL);
        }

        public static Message InstanciateUnsubscribeMessage(string sendingDevice, string fromDevice, string toDevice, string publicationSource, string publicationName)
        {
            return new Message(sendingDevice, fromDevice, toDevice, ContentTypes.CONTROL, ControlActions.UNSUBSCRIBE, publicationSource, publicationName);
        }

        public static Message InstanciateCommandMessage(string sendingDevice, string toDevice, string commandName, string commandTarget, object commandContent)
        {
            return new Message(sendingDevice, sendingDevice, toDevice, ContentTypes.COMMAND, commandName, commandTarget, commandContent);
        }

        public static Message InstanciateDataMessage(string sendingDevice, string toDevice, string dataSource, string dataName, object dataContent)
        {
            return new Message(sendingDevice, sendingDevice, toDevice, ContentTypes.DATA, dataSource, dataName, dataContent);
        }

        public static Message InstanciateHealthInfoMessage(string ofDevice, string toDevice, string healthEventCategory, string healthEventName, object healthEventContent)
        {
            return new Message(SERVER, ofDevice, toDevice, ContentTypes.HEALTH, healthEventCategory, healthEventName, healthEventContent);
        }

        public static Message InstanciateHeartbeatMessage(string toDevice)
        {
            return new Message(SERVER, SERVER, toDevice, ContentTypes.HEARTBEAT, MESSAGE_COUNT, String.Empty, DateTime.Now);
        }

        #endregion 

        public Message() { }

#if !MF_FRAMEWORK_VERSION_V4_2
        protected Message(string sendingDevice, string fromDevice, string toDevice, ContentTypes contentType, string name, object parameter, object content)
#else
        protected Message(string sendingDevice, string fromDevice, string toDevice, int contentType, string name, object parameter, object content)
#endif
        {
            SendingDevice = sendingDevice;
            ReceivingDevice = toDevice; //receivingDevice is equal to toDevice unless the server rerouted the message to a device listening for the to/from traffic
            FromDevice = fromDevice;
            ContentType = contentType;
            Name = name;
            Parameter = parameter;
            Content = content;
            Timestamp = DateTime.Now;
            ToDevice = toDevice;
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
