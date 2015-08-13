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
    class Program
    {
        static void Main(string[] args)
        {
            string ipAddress = ConfigurationManager.AppSettings["IpAddress"];
            int portNumber = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

            Console.WriteLine("Device console.");
            Console.WriteLine("Hit [1] to start a Data Generator");
            Console.WriteLine("Hit [2] to start a Pibrella simulator");
            Console.WriteLine("Hit [3] to start a Netduino Plus simulator");
            
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey();
            }
            while (!(keyInfo.KeyChar.Equals('1') || keyInfo.KeyChar.Equals('2') || keyInfo.KeyChar.Equals('3')));

            if (keyInfo.KeyChar.Equals('1'))
                DataGeneratorProcess.Start(ipAddress, portNumber);
            else if (keyInfo.KeyChar.Equals('2'))
                PibrellaSimulator.Start(ipAddress, portNumber);
            else if (keyInfo.KeyChar.Equals('3'))
                NetduinoSimulator.Start(ipAddress, portNumber);
        }
    }
}
