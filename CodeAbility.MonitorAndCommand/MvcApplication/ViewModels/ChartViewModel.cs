using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Web;

using CodeAbility.MonitorAndCommand.Models; 

namespace MvcApplication.ViewModels
{
    public class ChartViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
       
        public IEnumerable<Message> Messages { get; protected set; }

        public ChartViewModel(string name, string title, string subtitle, IEnumerable<Message> messages)
        {
            Name = name;
            Title = title;
            SubTitle = subtitle; 

            Messages = messages;
        }

        public string BuildJsonArray()
        {
            const string DATE_FORMAT_STRING = "\"date\":{0}";
            const string VALUE_FORMAT_STRING = "\"value\":{0}";
            const string TOKEN_FORMAT_STRING = "{0},{1}";

            StringBuilder builder = new StringBuilder();

            builder.Append("[");

            foreach (Message message in Messages)
            {
                if (builder.Length > 1)
                    builder.Append(",");

                string dateString = String.Format(DATE_FORMAT_STRING, JsonConvert.SerializeObject(message.Timestamp.ToUniversalTime()));
                string valueString = String.Format(VALUE_FORMAT_STRING, message.Content.ToString());
                string fullString = String.Format(TOKEN_FORMAT_STRING, dateString, valueString);

                builder.Append("{");
                builder.Append(fullString);
                builder.Append("}");
            }

            builder.Append("]");

            return builder.ToString();
        }
    }
}