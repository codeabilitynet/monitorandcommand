using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Server;
using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.Server.Helpers
{
    public class CommandHelper
    {
        /// <summary>
        /// Interpret a command that was captured from an inbound device.
        /// </summary>
        /// <param name="rawCommand">The string content of the command.</param>
        /// <param name="sourceLocation">The location (IP) of the inbound device.</param>
        public static void InterpretCommand(string rawCommand, string sourceLocation)
        {
            // Make sure that we are not attempting to process a dummy command, therefore
            // causing an exception.
            if (!string.IsNullOrWhiteSpace(rawCommand))
            {
                KeyValuePair<string, string> result = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(rawCommand.Remove(0, rawCommand.IndexOf('{')));

                // Get the initial list of sets on the target server
                if (result.Key == Commands.LIST_DEVICES)
                {

                }
                // Create a new set on the target server
                else if (result.Key.Contains(Commands.REGISTER_DEVICE))
                {

                }
            }
        }
    }
}
