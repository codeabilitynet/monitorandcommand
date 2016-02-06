using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class BaseStateMachine
    {
        Timer notifyStateTimer; 

        public bool ShallNotifyState { get; protected set; }

        public BaseStateMachine(int notificationPeriodInMilliseconds)
        {
            TimerCallback notifyStateTimerCallBack = DoNotifyState;
            notifyStateTimer = new Timer(notifyStateTimerCallBack, null, 0, notificationPeriodInMilliseconds); 
        }

        protected virtual void DoNotifyState(object state)
        {
            ShallNotifyState = true;
        }
    }
}
