using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class VoltageControl : BaseStateMachine
    {
        public enum States { Low, Standard, High, Danger }

        const double lowLimitVoltage = 0.01d;
        const double highLimitVoltage = 2.5d;
        const double dangerLimitVoltage = 3d;

        private States state = States.Low;
        public States State
        {
            get
            {
                HasChangedSinceLastGet = false;
                return state;
            }
            protected set
            {
                if (value != state)
                {
                    state = value;
                    HasChangedSinceLastGet = true;
                }
            }
        }

        public void ComputeState(string voltageString)
        {
           double voltage = 0;
                
           if (Double.TryParse(voltageString, out voltage))
           {
               if (voltage < lowLimitVoltage)
                   State = States.Low;
               else if (voltage > dangerLimitVoltage)
                   State = States.Danger;
               else if (voltage > highLimitVoltage)
                   State = States.High;
               else
                   State = States.Standard;
           }
        }
    }
}
