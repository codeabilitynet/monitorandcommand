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

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace CodeAbility.MonitorAndCommand.Netduino.Tools
{
    public class HIH4000Sensor
    {
        private const int MAXIMUM_VALUE_INT = 1024;
        private const float VOLTAGE_REFERENCE_FLOAT = 3.3f;
        private const float VOLTS_PER_COUNT_FLOAT = VOLTAGE_REFERENCE_FLOAT / MAXIMUM_VALUE_INT;

        private const float VOLTAGE_SUPPLY_FLOAT = 5.0f; 

        private const float VOLTAGE_DIVIDER_RATIO = 0.666f;

        SecretLabs.NETMF.Hardware.AnalogInput analogInput;

        public HIH4000Sensor(Cpu.Pin pin)
        {
            analogInput = new SecretLabs.NETMF.Hardware.AnalogInput(pin);
        }

        public float ReadHumidity()
        {
            int intValue = analogInput.Read();

            float readVoltage = ((float)intValue) * VOLTS_PER_COUNT_FLOAT ;

            float sensorVoltage = readVoltage * (1 / VOLTAGE_DIVIDER_RATIO);

            //VOUT=(VSUPPLY)(0.0062(sensor RH) + 0.16), typical at 25 ºC
            //True RH = (Sensor RH)/(1.0546 – 0.00216T), T in ºC
            float sensorRH = (sensorVoltage - (0.16f * VOLTAGE_SUPPLY_FLOAT)) / (0.0062f * VOLTAGE_SUPPLY_FLOAT); 
			
            return sensorRH;
        }
    }
}
