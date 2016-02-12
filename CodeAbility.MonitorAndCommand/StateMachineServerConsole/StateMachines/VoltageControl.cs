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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class VoltageControl : BaseStateMachine
    {
        const int NOTIFY_STATE_TIMER_PERIOD = 1000;

        const double lowLimitVoltage = 0.01d;
        const double highLimitVoltage = 2.25d;
        const double dangerLimitVoltage = 3d;

        private ServerStates.VoltageStates state = ServerStates.VoltageStates.Low;
        public ServerStates.VoltageStates State
        {
            get
            {
                ShallNotifyState = false;
                return state;
            }
            protected set
            {
                if (state != value)
                {
                    state = value;
                    ShallNotifyState = true;
                }
            }
        }

        public VoltageControl()
            : base(NOTIFY_STATE_TIMER_PERIOD)
        {

        }

        public void ComputeState(string voltageString)
        {
           double voltage = 0;
                
           if (Double.TryParse(voltageString, out voltage))
           {
               if (voltage < lowLimitVoltage)
                   State = ServerStates.VoltageStates.Low;
               else if (voltage > dangerLimitVoltage)
                   State = ServerStates.VoltageStates.Danger;
               else if (voltage > highLimitVoltage)
                   State = ServerStates.VoltageStates.High;
               else
                   State = ServerStates.VoltageStates.Standard;
           }
        }

    }
}
