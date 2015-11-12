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
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.Hardware.Netduino;

using CodeAbility.MonitorAndCommand.Netduino.Tools;

namespace CodeAbility.MonitorAndCommand.Netduino.Processes
{
    public class DS18B20Process : ProcessTemplate
    {
        DS18B20Sensor temperatureSensor = new DS18B20Sensor(Pins.GPIO_PIN_D2);

        public DS18B20Process(int doWorkStartupTime, int doWorkPeriod)
            : base(Environment.Devices.NETDUINO_3_WIFI, doWorkStartupTime, doWorkPeriod)
        {

        }

        protected override void SendServerMessages()
        {
            messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.DS18B20.OBJECT_SENSOR, Environment.Objects.DS18B20.DATA_SENSOR_TEMPERATURE);
        }

        protected override void HandleReceivedData(Models.MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void HandleReceivedCommand(Models.MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void PerformPeriodicWork()
        {
            string sensorDataString = temperatureSensor.ReadTemperature().ToString();
            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, Environment.Objects.DS18B20.OBJECT_SENSOR, Environment.Objects.DS18B20.DATA_SENSOR_TEMPERATURE, sensorDataString);
        }
    }
}
