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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Server;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.WpfServer
{
    internal class ExtendedMessageListener : MessageListener
    {
        public delegate void ReceivedMessageEventHandler(object sender, MessageEventArgs e);

        public event ReceivedMessageEventHandler MessageReceived;

        protected void OnMessageReceived(MessageEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        public delegate void SentMessageEventHandler(object sender, MessageEventArgs e);

        public event SentMessageEventHandler MessageSent;

        protected void OnMessageSent(MessageEventArgs e)
        {
            if (MessageSent != null)
                MessageSent(this, e);
        }

        public ExtendedMessageListener(string ipAddress, int portNumber, bool isMessageServiceActivated) :
            base(ipAddress, portNumber, isMessageServiceActivated)
        {

        }

        protected override void PreProcess(CodeAbility.MonitorAndCommand.Models.Message message)
        {
            base.PreProcess(message);
        }

        protected override void PostProcess(CodeAbility.MonitorAndCommand.Models.Message message)
        {
            base.PostProcess(message);

            try
            {
                ContentTypes messageType = message.ContentType;
                switch(messageType)
                { 
                    case ContentTypes.DATA:
                    case ContentTypes.COMMAND:
                        ProcessPayloadMessage(message);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(String.Format("PostProcessing exception : {0}", exception));
            }
        }

        protected override void PostSend(Message message)
        {
            base.PostSend(message);

            OnMessageSent(new MessageEventArgs(message));
        }

        private void ProcessPayloadMessage(Message message)
        {
            OnMessageReceived(new MessageEventArgs(message));
        }
    }
}
