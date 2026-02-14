using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace OnTheFly_UI.Components.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number && parameter is string paramStr && double.TryParse(paramStr, NumberStyles.Any, culture, out double factor))
            {
                return number * factor;
            }
            return Binding.DoNothing;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
