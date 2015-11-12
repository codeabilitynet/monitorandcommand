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
    public class HomeMonitoringProcess : ProcessTemplate
    {
        DS18B20Sensor temperatureSensor = new DS18B20Sensor(Pins.GPIO_PIN_D2);
        HIH4000Sensor humiditySensor = new HIH4000Sensor(Pins.GPIO_PIN_A0);
        SimpleVoltageSensor voltageSensor = new SimpleVoltageSensor(Pins.GPIO_PIN_A1);


        public HomeMonitoringProcess(int doWorkStartupTime, int doWorkPeriod)
            : base(Environment.Devices.NETDUINO_3_WIFI, doWorkStartupTime, doWorkPeriod)
        {

        }

        protected override void SendServerMessages()
        {
            messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.DS18B20.OBJECT_SENSOR, Environment.Objects.DS18B20.DATA_SENSOR_TEMPERATURE);
            messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.HIH4000.OBJECT_SENSOR, Environment.Objects.HIH4000.DATA_SENSOR_HUMIDITY);
            messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.SimpleVoltageSensor.OBJECT_SENSOR, Environment.Objects.SimpleVoltageSensor.DATA_SENSOR_VOLTAGE);
        }

        protected override void HandleReceivedData(Models.MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void HandleReceivedCommand(Models.MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        int cycle = 0;
        float temperatureAccumulator = 0;
        float humidityAccumulator = 0;
        float voltageAccumulator = 0;

        protected override void PerformPeriodicWork()
        {
            boardLed.Write(true);

            temperatureAccumulator += temperatureSensor.ReadTemperature();
            humidityAccumulator += humiditySensor.ReadHumidity();
            voltageAccumulator += voltageSensor.ReadVoltage();

            boardLed.Write(false);

            if (cycle < 5)
            {
                cycle++;
            }
            else
            {
                cycle = 0;

                float meanTemperature = temperatureAccumulator / 6f;
                float meanHumidity = humidityAccumulator / 6f;
                float meanVoltage = voltageAccumulator / 6f;

                string temperatureDataString = meanTemperature.ToString();
                string humidityDataString = meanHumidity.ToString();
                string voltageDataString = meanVoltage.ToString();

                if (messageClient != null)
                {
                    boardLed.Write(true);

                    messageClient.SendData(Environment.Devices.ALL, Environment.Objects.DS18B20.OBJECT_SENSOR, Environment.Objects.DS18B20.DATA_SENSOR_TEMPERATURE, temperatureDataString);
                    messageClient.SendData(Environment.Devices.ALL, Environment.Objects.HIH4000.OBJECT_SENSOR, Environment.Objects.HIH4000.DATA_SENSOR_HUMIDITY, humidityDataString);
                    messageClient.SendData(Environment.Devices.ALL, Environment.Objects.SimpleVoltageSensor.OBJECT_SENSOR, Environment.Objects.SimpleVoltageSensor.DATA_SENSOR_VOLTAGE, voltageDataString);

                    boardLed.Write(false);
                }

                temperatureAccumulator = 0;
                humidityAccumulator = 0;
                voltageAccumulator = 0;
            }

        }
    }
}

