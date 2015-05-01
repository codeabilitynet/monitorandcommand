using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CodeAbility.MonitorAndCommand.WpfMonitor.Converters
{
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (bool)value ? 1 : 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            //return null; 
            throw new NotImplementedException();
        }
    }
 
}
