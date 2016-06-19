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
using System.Threading;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Server;
using CodeAbility.MonitorAndCommand.Models;

using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole
{
    internal class PhotonDemoExtendedMessageListener : MessageListener
    {
        const int CHECK_STATE_TIMER_PERIOD = 1000;

        PhotonsControl photonsControl = new PhotonsControl();

        DeviceConnection photonAConnection = new DeviceConnection();
        DeviceConnection photonBConnection = new DeviceConnection();
        DeviceConnection photonCConnection = new DeviceConnection();

        DeviceConnection surfaceConnection = new DeviceConnection();
        DeviceConnection windowsPhoneConnection = new DeviceConnection();
        DeviceConnection androidConnection = new DeviceConnection();
        DeviceConnection pibrellaConnection = new DeviceConnection();

        Timer checkStatesTimer;

        public PhotonDemoExtendedMessageListener(string ipAddress, int portNumber, bool isMessageServiceActivated) :
            base(ipAddress, portNumber, isMessageServiceActivated)
        {
            this.RegistrationChanged += ExtendedMessageListener_RegistrationChanged;

            TimerCallback checkStatesTimerCallBack = CheckStates;
            checkStatesTimer = new Timer(checkStatesTimerCallBack, null, 0, CHECK_STATE_TIMER_PERIOD); 
        }

        void ExtendedMessageListener_RegistrationChanged(object sender, RegistrationEventArgs e)
        {
            switch(e.DeviceName)
            {
                case Devices.PHOTON_A:
                    photonAConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_PHOTON_A, photonAConnection.State.ToString()));
                    break;
                case Devices.PHOTON_B:
                    photonBConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_PHOTON_B, photonBConnection.State.ToString()));
                    break;
                case Devices.PHOTON_C:
                    photonCConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_PHOTON_C, photonCConnection.State.ToString()));
                    break;
                case Devices.RASPBERRY_PI_B:
                    pibrellaConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_RASPBERRY_B, pibrellaConnection.State.ToString()));
                    break;
                case Devices.WINDOWS_PHONE:
                    windowsPhoneConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_WINDOWS_PHONE, windowsPhoneConnection.State.ToString()));
                    break;
                case Devices.WINDOWS_SURFACE:
                    surfaceConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_WINDOWS_SURFACE, surfaceConnection.State.ToString()));
                    break;
                case Devices.ANDROID_PHONE:
                    androidConnection.ChangeState(e.RegistrationEvent == RegistrationEventArgs.RegistrationEvents.Registered);
                    SendToRegisteredDevices(InstantiateServerStateDataMessage(ServerStates.STATE_CONNECTION_ANDROID_PHONE, androidConnection.State.ToString()));
                    break;
            }
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

        private void ProcessPayloadMessage(Message message)
        {
            switch (message.Parameter.ToString())
            {
                case MCP4921.DATA_ANALOG_VALUE :
                    //voltageKeeper.StoreVoltage(message.Content.ToString());
                    photonsControl.ComputeState(message.SendingDevice, message.Parameter.ToString(), message.Content.ToString());                    
                    CheckLEDsStates();
                    break;
                default:
                    break;
            }
        }

        object locker = new object();

        protected virtual void CheckStates(object state)
        {
            CheckLEDsStates();
        }

        private void CheckStates()
        {
            //CheckVoltageState();
        }

        private void CheckLEDsStates()
        {
            if (photonsControl.ShallNotifyState)
            {
                SendToRegisteredDevices(Message.InstanciateCommandMessage(Devices.SERVER, Devices.RASPBERRY_PI_B, Pibrella.COMMAND_TOGGLE_LED, Pibrella.OBJECT_GREEN_LED,
                                                                       photonsControl.State == ServerStates.PhotonsStates.Normal ?
                                                                            Pibrella.CONTENT_LED_STATUS_ON :
                                                                            Pibrella.CONTENT_LED_STATUS_OFF));
                SendToRegisteredDevices(Message.InstanciateCommandMessage(Devices.SERVER, Devices.RASPBERRY_PI_B, Pibrella.COMMAND_TOGGLE_LED, Pibrella.OBJECT_YELLOW_LED,
                                                                       photonsControl.State == ServerStates.PhotonsStates.Warning ?
                                                                            Pibrella.CONTENT_LED_STATUS_ON :
                                                                            Pibrella.CONTENT_LED_STATUS_OFF));
                SendToRegisteredDevices(Message.InstanciateCommandMessage(Devices.SERVER, Devices.RASPBERRY_PI_B, Pibrella.COMMAND_TOGGLE_LED, Pibrella.OBJECT_RED_LED,
                                                                       photonsControl.State == ServerStates.PhotonsStates.Danger ?
                                                                            Pibrella.CONTENT_LED_STATUS_ON :
                                                                            Pibrella.CONTENT_LED_STATUS_OFF));
            }
        }
    }
}
