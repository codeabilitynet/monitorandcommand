using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.Converters
{
    public class BooleanToConnectionStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            SolidColorBrush greenBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            SolidColorBrush redBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

            return (bool)value ? redBrush : greenBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            //return null; 
            throw new NotImplementedException();
        }
    }
 
}
