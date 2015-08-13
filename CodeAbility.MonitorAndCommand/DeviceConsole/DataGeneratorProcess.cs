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
    public class DataGeneratorProcess
    {
        const int STARTUP_TIME = 2500;
        const int PERIOD = 500;

        static MessageClient messageClient;

        public static void Start(string ipAddress, int portNumber)
        {
            messageClient = new MessageClient(Devices.DATA_GENERATOR);

            messageClient.CommandReceived += client_CommandReceived;
            messageClient.Start(ipAddress, portNumber);

            Console.WriteLine("Hit a key to start data generation.");
            Console.ReadKey();

            Console.WriteLine("Data generator.");
            Console.WriteLine("Running.");

            messageClient.PublishData(Devices.ALL, DataGenerator.OBJECT_GENERATOR, DataGenerator.DATA_GENERATOR_DATA);
            messageClient.SubscribeToCommand(Devices.ALL, DataGenerator.OBJECT_GENERATOR, DataGenerator.COMMAND_TOGGLE_GENERATION);

            TimerCallback workTimerCallBack = DoWork;
            Timer workTimer = new Timer(workTimerCallBack, messageClient, STARTUP_TIME, PERIOD);

            Console.WriteLine("Hit a key to stop data generation.");
            Console.ReadKey();

            Console.WriteLine("Stopped.");

            messageClient.Stop();
        }

        private static void DoWork(object state)
        {
            try
            {
                //Sensor data
                string sensorDataString = new Random().NextDouble().ToString();
                if (messageClient != null)
                    messageClient.SendData(Environment.Devices.ALL, Environment.NetduinoPlus.OBJECT_SENSOR, Environment.NetduinoPlus.DATA_SENSOR_RANDOM, sensorDataString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        static void client_CommandReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e);
        }
    }
}
