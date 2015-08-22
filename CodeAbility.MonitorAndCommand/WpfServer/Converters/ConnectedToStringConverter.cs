using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CodeAbility.MonitorAndCommand.WpfServer.Converters
{
    public class ConnectedToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool? connected = (bool?)value;

            if (!connected.HasValue)
                return "Connect"; 
            else
            {
                return connected.Value ? "Connected" : "Error"; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            //return null; 
            throw new NotImplementedException();
        }
    }

}