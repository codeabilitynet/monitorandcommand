using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAbility.MonitorAndCommand.Repository
{
    public class Average
    {
        public enum ChartSpans { Last48Hours = 1, Last7Days = 2, Last30Days = 3, Last3Monthes = 4, LastYear = 5 }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public double Value { get; set; }

        public DateTime TimeStamp
        {
            get { return new DateTime(Year, Month, Day, Hour, Minute, 0, 0); }
        }

        public string JsonValue
        {
            get { return Value.ToString("#.##").Replace(",","."); }
        }
    }
}
