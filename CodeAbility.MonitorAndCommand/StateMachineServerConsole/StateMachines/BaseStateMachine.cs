using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class BaseStateMachine
    {
        public bool HasChangedSinceLastGet { get; protected set; }
    }
}
