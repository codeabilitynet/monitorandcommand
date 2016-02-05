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

        private ServerStates.ConnectionStates state = ServerStates.ConnectionStates.Disconnected;
        public ServerStates.ConnectionStates State
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

        public void ChangeState(bool isConnected)
        {
            State = isConnected ? ServerStates.ConnectionStates.Connected : ServerStates.ConnectionStates.Disconnected;
        }
    }
}
