using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Environment;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class DeviceConnection : BaseStateMachine
    {
        const int NOTIFY_STATE_TIMER_PERIOD = 5000;

        private ServerStates.ConnectionStates state = ServerStates.ConnectionStates.Disconnected;
        public ServerStates.ConnectionStates State
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

        public DeviceConnection() : base(NOTIFY_STATE_TIMER_PERIOD)
        {

        }

        public void ChangeState(bool isConnected)
        {
            State = isConnected ? ServerStates.ConnectionStates.Connected : ServerStates.ConnectionStates.Disconnected;
        }
    }
}
