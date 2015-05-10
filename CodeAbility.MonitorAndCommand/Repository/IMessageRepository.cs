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
        IEnumerable<Message> ListMessages();
        void Purge();
    }
}
