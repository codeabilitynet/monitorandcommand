using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CodeAbility.MonitorAndCommand.Models; 

namespace MvcApplication.ViewModels
{
    public class ChartViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }

        public bool IsCommaNeeded { get; set; }

        public IEnumerable<Message> Messages { get; protected set; }

        public ChartViewModel(string name, string title, string subtitle, IEnumerable<Message> messages)
        {
            Name = name;
            Title = title;
            SubTitle = subtitle; 

            Messages = messages;

            IsCommaNeeded = false; 
        }
    }
}