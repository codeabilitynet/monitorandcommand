using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class TemperatureSensor
    {
        public enum States { Low, Standard, High, Danger }

        const double lowLimitTemperature = 15d;
        const double highLimitTemperature = 25d;
        const double dangerLimitTemperature = 50d;

        public States State { get; protected set; }

        public void ComputeState(string temperatureString)
        {
           double temperature = 0;
                
           if (Double.TryParse(temperatureString, out temperature))
           {
               if (temperature < lowLimitTemperature)
                   State = States.Low;
               else if (temperature > dangerLimitTemperature)
                   State = States.Danger;
               else if (temperature > highLimitTemperature)
                   State = States.High;
               else
                   State = States.Standard;
           }
        }
    }
}
