using System.Globalization;
using System.Windows.Data;

namespace OnTheFly_UI.Components.Converters
{
    public class FindMiddleValue : IMultiValueConverter // Change this name.
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            return ((double)values[0] - (double)values[1]) / 2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
