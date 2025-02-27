using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Modules.Handlers
{
    public static class BitmapConvertHandler
    {
        public static BitmapSource FromByteArray(byte[] frame)
        {
            using(var stream = new MemoryStream(frame))
            {
                var bitmap = new System.Drawing.Bitmap(stream);
                return ToBitmapSourceFast(bitmap);
            }
        }
        public static BitmapSource ToBitmapSourceFast(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                var pixelFormats = ConvertPixelFormat(bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width,
                    bitmapData.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    pixelFormats,
                    null,
                    bitmapData.Scan0,
                    bitmapData.Stride * bitmapData.Height,
                    bitmapData.Stride);

                bitmapSource.Freeze();
                return bitmapSource;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return System.Windows.Media.PixelFormats.Bgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr32;
                default:
                    return System.Windows.Media.PixelFormats.Bgr24;
            }
        }
    }
}
