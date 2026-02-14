using Emgu.CV;
using OnTheFly_UI.Modules;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Enums;
using OnTheFly_UI.Modules.Handlers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Components.Converters
{
    [ValueConversion(typeof(RequestObject), typeof(BitmapSource))]
    public partial class PreviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is not RequestObject) )
                return new BitmapImage();

            RequestObject? requestObject = value as RequestObject;

            if ( requestObject == null )
                return new BitmapImage();

            if (requestObject.SourceType == RequestSourceType.Image)
                return BitmapConvertHandler.ToBitmapSourceFast(new Bitmap(requestObject.Source));
         
            else if (requestObject.SourceType == RequestSourceType.Video)
            {
                using (Mat frame = new Mat())
                using (VideoCapture capture = new VideoCapture(requestObject.Source))
                {
                    while (capture.IsOpened)
                    {
                        if (!SpinWait.SpinUntil(() => { return capture.Read(frame); }, 20))
                            return 0;

                        return BitmapConvertHandler.ToBitmapSourceFast(frame.ToBitmap());
                    }
                }
            }

            return 0;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
