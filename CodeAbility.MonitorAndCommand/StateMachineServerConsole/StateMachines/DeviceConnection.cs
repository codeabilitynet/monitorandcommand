using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class DeviceConnection : BaseStateMachine
    {
        public enum States { Connected, Disconnected }

        private States state = States.Disconnected;
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

        public void ChangeState(bool isConnected)
        {
            State = isConnected ? States.Connected : States.Disconnected;
        }
    }
}
