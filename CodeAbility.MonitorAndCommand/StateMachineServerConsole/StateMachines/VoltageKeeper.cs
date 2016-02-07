using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class VoltageKeeper : BaseStateMachine
    {
        const int NOTIFY_STATE_TIMER_PERIOD = 250;

        double lastReceivedVoltage = 0;
        double lastRecordedVoltage = 0;

        public VoltageKeeper()
            : base(NOTIFY_STATE_TIMER_PERIOD)
        {

        }

        object locker = new object();

        public void StoreVoltage(string voltageString)
        {
           lock (locker)
           { 
               double voltage = 0;
                
               if (Double.TryParse(voltageString, out voltage))
               {
                   lastReceivedVoltage = voltage;
               }
           }
        }

        protected override void DoNotifyState(object state)
        {
            lock (locker)
            {
                if (ShallNotifyState = false && lastReceivedVoltage == 0)
                    ShallNotifyState = true;
                else
                {
                    lastRecordedVoltage = lastReceivedVoltage;
                    lastReceivedVoltage = 0;
                }
            }
        }

        public double GetLastRecoredVoltage()
        {
            return lastRecordedVoltage;
        }
    }
}
