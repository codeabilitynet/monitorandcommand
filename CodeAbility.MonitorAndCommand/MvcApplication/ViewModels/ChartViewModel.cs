using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Web;

using CodeAbility.MonitorAndCommand.Models;
using CodeAbility.MonitorAndCommand.Repository;

namespace MvcApplication.ViewModels
{
    public class ChartViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
       
        public IEnumerable<Average> Averages { get; protected set; }

        double Average { get; set; }

        public ChartViewModel(string name, string title, string subtitle, IEnumerable<Average> averages)
        {
            Name = name;
            Title = title;
            SubTitle = subtitle; 

            Averages = averages;
        }

        public string BuildJsonArray()
        {
            const string DATE_FORMAT_STRING = "\"date\":{0}";
            const string VALUE_FORMAT_STRING = "\"value\":{0}";
            const string TOKEN_FORMAT_STRING = "{0},{1}";

            StringBuilder builder = new StringBuilder();

            builder.Append("[");

            foreach (Average average in Averages)
            {
                if (builder.Length > 1)
                    builder.Append(",");

                string dateString = String.Format(DATE_FORMAT_STRING, JsonConvert.SerializeObject(average.TimeStamp.ToUniversalTime()));
                string valueString = String.Format(VALUE_FORMAT_STRING, average.JsonValue);
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