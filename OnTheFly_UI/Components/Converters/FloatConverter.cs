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
    [ValueConversion(typeof(double), typeof(float))]
    public class FloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return (float)value;

            //return Binding.DoNothing; // Ensures safe failure instead of throwing an exception.
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return (double)value;
        }
    }
}
