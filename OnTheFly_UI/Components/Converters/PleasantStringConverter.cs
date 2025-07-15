using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OnTheFly_UI.Components.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PleasantStringConverter : IValueConverter
    {
        private readonly char[] _splitters = new char[] { ' ', ',', '-', '_' };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(string))
                throw new InvalidCastException();


            string text = (string)value;


            string[] text_parts = text.Split(_splitters);

            int spaceCount = text_parts.Length - 1;

            string[] new_parts = new string[text_parts.Length + spaceCount]; // text_parts.Length - 1 for seperation with space


            string new_part = string.Empty;
            int index = 0;
            
            foreach (var part in text_parts)
            {
                if (part.Length > 0 && char.IsUpper(part[0]))
                    new_part = part;
                else
                {
                    new_part = part.Substring(0,1).ToUpper() + part.Substring(1);

                }
                
                new_parts[index] = new_part;
                index++;


                if (index >= new_parts.Length)
                    continue;
               
                new_parts[index] = " ";
                index++;

            }

            return string.Concat(new_parts);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
