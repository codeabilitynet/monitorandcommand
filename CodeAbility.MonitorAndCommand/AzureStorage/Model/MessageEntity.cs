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
using Microsoft.WindowsAzure.Storage.Table;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.AzureStorage.Model
{
    public class MessageEntity : TableEntity
    {
        public MessageEntity() { }

        public MessageEntity(string partitionKey, string rowKey, Message message)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;

            SendingDevice = message.SendingDevice;
            ReceivingDevice = message.ToDevice; 
            FromDevice = message.FromDevice;
            ToDevice = message.ToDevice;

            ContentType = message.ContentType;
            Name = message.Name;
            Parameter = message.Parameter;
            Content = message.Content;

            Timestamp = message.Timestamp;
        }

        public string SendingDevice { get; set; }

        public string ReceivingDevice { get; set; }

        public string FromDevice { get; set; }

        public string ToDevice { get; set; }

        public ContentTypes ContentType { get; set; }

        public string Name { get; set; }

        public object Parameter { get; set; }

        public object Content { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
