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
        const double highLimitVoltage = 2.5d;
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
                if (value != state)
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
