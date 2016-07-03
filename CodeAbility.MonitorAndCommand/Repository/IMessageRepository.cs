using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.Repository
{
    public interface IMessageRepository
    {
        void Insert(Message message);

        IEnumerable<Message> ListLastMessages(int numberOfMessages);

        IEnumerable<Average> ListHourlyAverages(int numberOfMessages, string deviceName, string objectName, string parameterName);

        IEnumerable<Average> List15MinutesAverages(int numberOfMessages, string deviceName, string objectName, string parameterName);

        void Purge();
    }
}
