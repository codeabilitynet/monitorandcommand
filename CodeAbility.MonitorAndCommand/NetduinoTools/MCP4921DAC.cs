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
    public class MCP49231DAC
    {
        public static int STEPS = 4096;

        //Synchronization Input on Pin 5
        OutputPort LDAC = new OutputPort(Pins.GPIO_PIN_D4, false);
        //Serial Data Input on Pint 4
        OutputPort SDI = new OutputPort(Pins.GPIO_PIN_D5, false);
        //Chip Select Input on Pin 2
        OutputPort CS = new OutputPort(Pins.GPIO_PIN_D6, true);
        //Serial Clock Input on Pin 3
        OutputPort SCK = new OutputPort(Pins.GPIO_PIN_D7, true);

        const string INVALID_VALUE = "Invalid value.";

        double MaxVoltage { get; set; }

        public MCP49231DAC(double maxVoltage) 
        {
            MaxVoltage = maxVoltage; 
        }

        public double Write(int data)
        {
            if (data < 0)
                data = 0;

            if (data > (STEPS - 1))
                data = (STEPS - 1);

            int cmd = 0x7000;
            int fword;
            int tmp;

            fword = cmd | data;

            CS.Write(true);

            CS.Write(false);
            LDAC.Write(true);

            //Write 16 bits
            for (int index = 0; index < 16; index++)
            {
                SCK.Write(false);
                tmp = fword & 0x8000;
                SDI.Write(false);

                if (tmp != 0x0)
                    SDI.Write(true);

                SCK.Write(true);

                fword = fword << 1;
            }
            
            CS.Write(true);
            LDAC.Write(false);

            double voltage = ((double)data / (double)STEPS) * MaxVoltage;

            return voltage;
        }   
    }
}
