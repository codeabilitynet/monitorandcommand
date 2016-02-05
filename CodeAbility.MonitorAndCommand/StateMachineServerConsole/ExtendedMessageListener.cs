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

using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole
{
    internal class ExtendedMessageListener : MessageListener
    {
        VoltageControl voltageControlStateMachine = new VoltageControl();

        DeviceConnection netduinoConnection = new DeviceConnection();
        DeviceConnection pibrellaConnection = new DeviceConnection();
        DeviceConnection windowsPhoneConnection = new DeviceConnection();
        DeviceConnection surfaceConnection = new DeviceConnection();

        public ExtendedMessageListener(string ipAddress, int portNumber, int heartbeatPeriod, bool isMessageServiceActivated) :
            base(ipAddress, portNumber, heartbeatPeriod, isMessageServiceActivated)
        {
            this.RegistrationChanged += ExtendedMessageListener_RegistrationChanged;
        }

        void ExtendedMessageListener_RegistrationChanged(object sender, RegistrationEventArgs e)
        {
            switch(e.DeviceName)
            {
                case Environment.Devices.NETDUINO_3_WIFI:
                    netduinoConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_NETDUINO_3_WIFI, netduinoConnection.State.ToString()));
                    break;
                case Environment.Devices.RASPBERRY_PI_B:
                    pibrellaConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_RASPBERRY_B, pibrellaConnection.State.ToString()));
                    break;
                case Environment.Devices.WINDOWS_PHONE:
                    windowsPhoneConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_WINDOWS_PHONE, windowsPhoneConnection.State.ToString()));
                    break;
                case Environment.Devices.WINDOWS_SURFACE:
                    surfaceConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_WINDOWS_SURFACE, surfaceConnection.State.ToString()));
                    break;
            }
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

            if (voltageControlStateMachine.HasChangedSinceLastGet)
                SendToRegisteredDevices(InstantiateServerStateDataMessage(Environment.ServerStates.STATE_MCP4921_VOLTAGE, voltageControlStateMachine.State.ToString()));   
        }

        private void ProcessPayloadMessage(Message message)
        {
            switch (message.Parameter.ToString())
            {
                case Environment.MCP4921.OBJECT_ANALOG_DATA :
                    voltageControlStateMachine.ComputeState(message.Content.ToString());
                    break;
                default:
                    break;
            }
        }
    }
}
