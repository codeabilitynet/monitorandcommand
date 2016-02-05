using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAbility.MonitorAndCommand.Models;

namespace CodeAbility.MonitorAndCommand.Repository
{
    public interface IEventRepository
    {
        void Insert(Event _event);

        IEnumerable<Event> ListLastEvents(int numberOfEvents);

        void Purge();
    }
}
