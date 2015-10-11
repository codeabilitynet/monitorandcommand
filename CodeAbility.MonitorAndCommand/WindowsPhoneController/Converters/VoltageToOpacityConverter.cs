using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CodeAbility.MonitorAndCommand.WindowsPhoneController.Converters
{
    public class VoltageToOpacityConverter : IValueConverter
    {
        const double BOARD_REFERENCE_VOLTAGE = 3.3;

        const double LED_MINIMUM_FORWARD_VOLTAGE = 1.5;
        const double LED_MAXIMUM_FORWARD_VOLTAGE = 3.5;

        const double MINIMUM_OPACITY = 0.1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            double result;
            if (Double.TryParse(value.ToString(), out result))
            {
                return MINIMUM_OPACITY + 0.9 * ((result - LED_MINIMUM_FORWARD_VOLTAGE) / (BOARD_REFERENCE_VOLTAGE - LED_MINIMUM_FORWARD_VOLTAGE));
            }
            else
                return MINIMUM_OPACITY;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            //return null; 
            throw new NotImplementedException();
        }
    }
 
}
