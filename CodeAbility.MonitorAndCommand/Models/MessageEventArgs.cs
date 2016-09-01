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

#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT;
#endif

namespace CodeAbility.MonitorAndCommand.Models
{
    public class MessageEventArgs : EventArgs
    {
        public string SendingDevice { get; set; }

        public string FromDevice { get; set; }

        public string ToDevice { get; set; }

        public string ReceivingDevice { get; set; }

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
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
            ReceivingDevice = receivedMessage.ReceivingDevice; 
            ContentType = receivedMessage.ContentType;
            Name = receivedMessage.Name;
            Parameter = receivedMessage.Parameter; 
            Content = receivedMessage.Content;
            Timestamp = receivedMessage.Timestamp;
        }

        public override string ToString()
        {
            const string NOT_AVAILABLE = "N/A";

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
            string parameter = String.IsNullOrEmpty(Parameter.ToString()) ? NOT_AVAILABLE : Parameter.ToString();
            string content = String.IsNullOrEmpty(Content.ToString()) ? NOT_AVAILABLE : Content.ToString();

            return String.Format("{0}, {1}: {2}, {3} - From:{4}, To:{5}, Parameter:{6}, Content:{7}", Timestamp, SendingDevice, ContentType, Name, FromDevice, ToDevice, parameter, content);
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
