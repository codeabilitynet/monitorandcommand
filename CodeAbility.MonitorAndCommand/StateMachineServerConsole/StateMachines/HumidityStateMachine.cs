using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.StateMachineServerConsole.StateMachines
{
    public class HumiditySensor
    {
        public enum States { Low, Standard, High, Danger }

        const double lowLimitHumidity = 35d;
        const double highLimitHumidity = 70d;
        const double dangerLimitHumidity = 90d;
        public States State { get; protected set; }

        public void ComputeState(string humidityString)
        {
            double humidity = 0;

            if (Double.TryParse(humidityString, out humidity))
            {
                if (humidity < lowLimitHumidity)
                    State = States.Low;
                else if (humidity > dangerLimitHumidity)
                    State = States.Danger;
                else if (humidity > highLimitHumidity)
                    State = States.High;
                else
                    State = States.Standard;
            }
        }
    }
}
