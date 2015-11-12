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
using System.Net;

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
using System.Runtime.Serialization;
#if !PORTABLE
using System.ServiceModel;
#endif 
#endif

namespace CodeAbility.MonitorAndCommand.Models
{

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
    [DataContract]
#endif
    public class Message
    {
        #region Properties

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public string SendingDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public string ReceivingDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public string FromDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public string ToDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
        public ContentTypes ContentType { get; set; }
#else
        public int ContentType { get; set; }
#endif

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public string Name { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public object Parameter { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public object Content { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        [DataMember]
#endif
        public DateTime Timestamp { get; set; }

        #endregion 
        
        public const string SERVER = "SERVER";
        public const string HEARTBEAT = "HEARTBEAT";
        public const string ALL = "*";

        #region Static Helpers

        public static Message InstanciateRegisterMessage(string sendingDevice)
        {
            return new Message(sendingDevice, sendingDevice, SERVER, ContentTypes.CONTROL, ControlActions.REGISTER, String.Empty, String.Empty);
        }

        public static Message InstanciateUnregisterMessage(string sendingDevice)
        {
            return new Message(sendingDevice, String.Empty, SERVER, ContentTypes.CONTROL, ControlActions.UNREGISTER, String.Empty, String.Empty);
        }

        public static Message InstanciatePublishMessage(string sendingDevice, string toDevice, string publicationSource, string publicationName)
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

        public static Message InstanciateHeartbeatMessage(string ofDevice)
        {
            return new Message(ofDevice, ofDevice, SERVER, ContentTypes.HEARTBEAT, HEARTBEAT, String.Empty, String.Empty);
        }

        //public static Message InstanciateHeartbeatRequestMessage(string fromDevice)
        //{
        //    return new Message(fromDevice, fromDevice, ALL, ContentTypes.HEARTBEAT, HEARTBEAT, String.Empty, DateTime.Now);
        //}

        #endregion 

        /// <summary>
        /// Empty constructor, only there because it's needed by the serialization/deserialization process. Do not use in your code.
        /// </summary>
        public Message() { }

        public Message(Message message) :
            this(message.SendingDevice, message.FromDevice, message.ToDevice, message.ContentType, message.Name, message.Parameter, message.Content) { }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
        public Message(string sendingDevice, string fromDevice, string toDevice, ContentTypes contentType, string name, object parameter, object content)
#else
        public Message(string sendingDevice, string fromDevice, string toDevice, int contentType, string name, object parameter, object content)
#endif
        {
            SendingDevice = sendingDevice;
            ReceivingDevice = toDevice; //receivingDevice is equal to toDevice unless the server rerouted the message to a device listening for the to/from traffic
            FromDevice = fromDevice;
            ToDevice = toDevice;

            ContentType = contentType;
            Name = name;
            Parameter = parameter;
            Content = content;

            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            const string NOT_AVAILABLE = "[NA]";

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
            string parameter = String.IsNullOrEmpty(Parameter.ToString()) ? NOT_AVAILABLE : Parameter.ToString();
            string content = String.IsNullOrEmpty(Parameter.ToString()) ? NOT_AVAILABLE : Parameter.ToString();

            return String.Format("{0}, {1}: {2}, {3} - From:{4}, To:{5}, Parameter:{6}, Content:{7}", Timestamp, SendingDevice, ContentType, Name, FromDevice, ToDevice, parameter, Content);
#else 
            string parameter =  (Parameter != null) ? Parameter.ToString() : NOT_AVAILABLE;
            string content = (Content != null) ? Content.ToString() : NOT_AVAILABLE;

            return SendingDevice + ": "
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
