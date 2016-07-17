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

using CodeAbility.MonitorAndCommand.Netduino.Tools;

namespace CodeAbility.MonitorAndCommand.Netduino.Processes
{
    public class MCP4921Process : ProcessTemplate
    {
        const int STARTUP_TIME = 15000;
        const int PERIOD = 50;

        const int BUTTON_PRESSED_DURATION = 500;
        const int RECONNECTION_TIMER_DURATION = 60000;

        const double BOARD_REFERENCE_VOLTAGE = 3.3;

        const double LED_MINIMUM_FORWARD_VOLTAGE = 1.5;
        const double LED_MAXIMUM_FORWARD_VOLTAGE = 3.5;

        MCP49231DAC converter = new MCP49231DAC(BOARD_REFERENCE_VOLTAGE);

        public MCP4921Process()
            : base(Environment.Devices.NETDUINO_PLUS, 0, 0)
        {
            
        }

        protected override void SendServerMessages()
        {
            messageClient.PublishData(Environment.Devices.WINDOWS_PHONE, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE);
            messageClient.PublishData(Environment.Devices.WINDOWS_SURFACE, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE);
            messageClient.SubscribeToCommand(Environment.Devices.WINDOWS_PHONE, Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA, Environment.Objects.MCP4921.COMMAND_CONVERT);
        }

        protected override void HandleReceivedData(Models.MessageEventArgs e)
        {
            messageClient.Log("Unhandled  : " + e.ToString());             
        }

        protected override void HandleReceivedCommand(Models.MessageEventArgs e)
        {
            string targetName = e.Name.ToString();
            string dataValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

            if (e.FromDevice.Equals(Environment.Devices.WINDOWS_PHONE) && 
                targetName.Equals(Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA))
            {
                int inputData = Int32.Parse(dataValue);
                Convert(inputData);
            }
        }

        protected override void PerformPeriodicWork()
        {
            throw new NotImplementedException();
        }


        private void Convert(int inputData)
        {
            if (!(inputData >= 0 && inputData <= 100))
                return;

            boardLed.Write(true);

            double voltage = converter.Write(ComputeConverterData(inputData));

            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE, voltage);

            boardLed.Write(false);
        }

        private int ComputeConverterData(int inputData)
        {
            try
            { 
                int converterData = 0;

                double expectedVoltage = LED_MINIMUM_FORWARD_VOLTAGE + ((double)inputData / 100) * (BOARD_REFERENCE_VOLTAGE - LED_MINIMUM_FORWARD_VOLTAGE);

                converterData = (int)((expectedVoltage / BOARD_REFERENCE_VOLTAGE) * MCP49231DAC.STEPS);

                return converterData;

            }
            catch(Exception exception)
            {
                messageClient.Log(exception.ToString());
            }

            return 0;
        }
    }
}
