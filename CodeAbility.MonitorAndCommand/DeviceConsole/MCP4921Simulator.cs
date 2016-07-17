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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Client;
using CodeAbility.MonitorAndCommand.Environment;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.DeviceConsole
{
    public class MCP4921Simulator
    {
        const int HEARTBEAT_PERIOD_IN_MILLESECONDS = 10000;

        const double BOARD_REFERENCE_VOLTAGE = 3.3;

        const double LED_MINIMUM_FORWARD_VOLTAGE = 1.5;
        const double LED_MAXIMUM_FORWARD_VOLTAGE = 3.5;

        public static int MCP49231DAC_STEPS = 4096;

        static MessageClient messageClient;

        public static void Start(string ipAddress, int portNumber)
        {
            messageClient = new MessageClient(Devices.NETDUINO_3_WIFI, HEARTBEAT_PERIOD_IN_MILLESECONDS);

            messageClient.DataReceived += client_DataReceived;
            messageClient.CommandReceived += client_CommandReceived;

            Console.WriteLine("Device console.");
            Console.WriteLine("Hit a key to start client, hit [0,1,2,3] to send MCP4921 voltages values, hit ESC to exit.");
            Console.ReadKey();

            messageClient.Start(ipAddress, portNumber);

            bool running = true;

            Console.WriteLine("Running.");

            //Simulating a Netduino device with two LEDs
            messageClient.PublishData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE);
            messageClient.SubscribeToData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA, Environment.Objects.MCP4921.DATA_DIGITAL_VALUE);
            messageClient.SubscribeToCommand(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA, Environment.Objects.MCP4921.COMMAND_CONVERT);

            while (running)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (messageClient != null)
                {
                    if (keyInfo.KeyChar.Equals('0') || keyInfo.KeyChar.Equals('1') || keyInfo.KeyChar.Equals('2') || keyInfo.KeyChar.Equals('3'))
                    {
                        if (messageClient != null)

                            messageClient.SendData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE, keyInfo.KeyChar.ToString());
                    }
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        running = false;
                        break;
                    }
                }
            }

            Console.WriteLine("Stopped.");

            messageClient.Stop();
        }

        static void client_CommandReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);

            string objectName = e.Name.ToString();
            string dataValue = (e.Content != null) ? e.Content.ToString() : String.Empty;

            if (e.FromDevice.Equals(Environment.Devices.WINDOWS_PHONE) &&
                objectName.Equals(Environment.Objects.MCP4921.OBJECT_DIGITAL_DATA))
            {
                int inputData = Int32.Parse(dataValue);
                Convert(inputData);
            }
        }

        static void client_DataReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);
        }

        private static void Convert(int inputData)
        {
            if (!(inputData >= 0 && inputData <= 100))
                return;

            double voltage = ((double)ComputeConverterData(inputData) / (double)MCP49231DAC_STEPS) * BOARD_REFERENCE_VOLTAGE;

            if (messageClient != null)
                messageClient.SendData(Environment.Devices.ALL, Environment.Objects.MCP4921.OBJECT_ANALOG_DATA, Environment.Objects.MCP4921.DATA_ANALOG_VALUE, voltage);
        }

        private static int ComputeConverterData(int inputData)
        {
            int converterData = 0;

            double expectedVoltage = LED_MINIMUM_FORWARD_VOLTAGE + ((double)inputData / 100) * (BOARD_REFERENCE_VOLTAGE - LED_MINIMUM_FORWARD_VOLTAGE);

            converterData = (int)((expectedVoltage / BOARD_REFERENCE_VOLTAGE) * MCP49231DAC_STEPS);

            return converterData;
        }
    }
}
