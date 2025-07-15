using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Compunet.YoloSharp.Plotting;
using Emgu.CV.Ocl;
using Emgu.CV.Reg;
using OnTheFly_UI.Modules.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Modules.Handlers
{
    public static class PlotHandler
    {

        //public static void Plot<T>(YoloResult<T> result ) where T : IYoloPrediction<T>
        //{
        //    var a = result;
        //}

        public static BitmapSource PlotDetection(byte[] frame, YoloResult<Detection> result,PlotConfiguration? configuration = null,HashSet<string> hiddenNames = null)
        {
            using (var stream = new MemoryStream(frame))
            {
                var bitmap = new System.Drawing.Bitmap(stream);
                return PlotDetection(bitmap, result,configuration,hiddenNames);
            }
        }
        public static BitmapSource PlotDetection(Bitmap frame, YoloResult<Detection> result, PlotConfiguration? configuration = null, HashSet<string> hiddenNames = null)
        {
            if (result.Count == 0)
                return BitmapConvertHandler.ToBitmapSourceFast(frame);

            if (configuration == null)
                configuration = new PlotConfiguration();

            var g = System.Drawing.Graphics.FromImage(frame);
            var pen = new System.Drawing.Pen(System.Drawing.Color.Cyan, 3);
            var rect = new System.Drawing.Rectangle(50, 50, 100, 100);

            foreach (var obj in result)
            {
                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;

                rect.Width = obj.Bounds.Width;
                rect.Height = obj.Bounds.Height;
                rect.X = obj.Bounds.X;
                rect.Y = obj.Bounds.Y;

                string text = $"{obj.Name.Name} %{Math.Round(obj.Confidence * 100, 1)}";
                pen.Width = configuration.BorderThickness;
                pen.Color = configuration.ObjectColors[obj.Name.Id];

                g.DrawRectangle(pen, rect);

                var size = g.MeasureString(text, configuration.Font);
                var point = new System.Drawing.Point(obj.Bounds.X, obj.Bounds.Y - (int)size.Height);
                var backgroundFiller = new System.Drawing.Rectangle(point, size.ToSize());

                g.FillRectangle(new System.Drawing.SolidBrush(pen.Color), backgroundFiller);
                g.DrawString(text, configuration.Font, new System.Drawing.SolidBrush(configuration.FontColor), point);
            }

            BitmapSource bitmapSource = BitmapConvertHandler.ToBitmapSourceFast(frame);
            return bitmapSource;
        }
        public static BitmapSource PlotSegmentatation(byte[] frame, YoloResult<Segmentation> result, PlotConfiguration? configuration = null, double alpha = 0.3, HashSet<string> hiddenNames = null)
        {
            using (var stream = new MemoryStream(frame))
            {
                var bitmap = new System.Drawing.Bitmap(stream);
                return PlotSegmentatation(bitmap, result, configuration, alpha, hiddenNames);
            }
        }
        public static BitmapSource PlotSegmentatation(Bitmap frame, YoloResult<Segmentation> result, PlotConfiguration? configuration = null,double alpha = 0.3, HashSet<string> hiddenNames = null)
        {
            var sw = new Stopwatch();
            sw.Restart();
            if (result.Count == 0)
                return BitmapConvertHandler.ToBitmapSourceFast(frame);

            if (configuration == null)
                configuration = new PlotConfiguration();


            var pen = new System.Drawing.Pen(System.Drawing.Color.Cyan, 3);
            var rect = new System.Drawing.Rectangle(50, 50, 100, 100);

            var frameRect = new System.Drawing.Rectangle(0, 0, frame.Width, frame.Height);


            var g = System.Drawing.Graphics.FromImage(frame);


            var bitmapData = frame.LockBits(frameRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

            foreach (var obj in result)
            {

                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;

                if (obj.Confidence < 0.6)
                    continue;

                if (obj.Name.Id > configuration.ObjectColors.Count)
                    pen.Color = System.Drawing.Color.Gray;
                else
                    pen.Color = configuration.ObjectColors[obj.Name.Id];

                int pixelSize = bitmapData.Reserved;
                unsafe
                {
                    byte* sourceRow;

                    for (var x = 0; x < obj.Mask.Width; x++)
                    {
                        for (var y = 0; y < obj.Mask.Height; y++)
                        {
                            if (obj.Mask[y, x] > 0.2)
                            {
                                sourceRow = (byte*)bitmapData.Scan0 + ((y + obj.Bounds.Y) * bitmapData.Stride);

                                sourceRow[(x + obj.Bounds.X) * pixelSize + 0] = Convert.ToByte((sourceRow[(x + obj.Bounds.X) * pixelSize + 0] * (1 - alpha) + pen.Color.B * alpha));
                                sourceRow[(x + obj.Bounds.X) * pixelSize + 1] = Convert.ToByte((sourceRow[(x + obj.Bounds.X) * pixelSize + 1] * (1 - alpha) + pen.Color.G * alpha));
                                sourceRow[(x + obj.Bounds.X) * pixelSize + 2] = Convert.ToByte((sourceRow[(x + obj.Bounds.X) * pixelSize + 2] * (1 - alpha) + pen.Color.R * alpha));
                            }

                        }
                    }
                }
            }
            frame.UnlockBits(bitmapData);


            foreach (var obj in result) // Why do we need to draw the rectangles again? Indstead of just use plot detection?
            {
                if (obj.Confidence < 0.6)
                    continue;

                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;

                if (obj.Name.Id > configuration.ObjectColors.Count)
                    pen.Color = System.Drawing.Color.Gray;
                else
                    pen.Color = configuration.ObjectColors[obj.Name.Id];


                rect.Width = obj.Bounds.Width;
                rect.Height = obj.Bounds.Height;
                rect.X = obj.Bounds.X;
                rect.Y = obj.Bounds.Y;

                string text = $"{obj.Name.Name} %{Math.Round(obj.Confidence * 100, 1)}";
                pen.Width = configuration.BorderThickness;
                g.DrawRectangle(pen, rect);

                var size = g.MeasureString(text, configuration.Font);
                var point = new System.Drawing.Point(obj.Bounds.X, obj.Bounds.Y - (int)size.Height);
                var backgroundFiller = new System.Drawing.Rectangle(point, size.ToSize());
               
                g.FillRectangle(new System.Drawing.SolidBrush(pen.Color), backgroundFiller);

                g.DrawString(text, configuration.Font, new System.Drawing.SolidBrush(configuration.FontColor), point);

            }


            Trace.WriteLine($"drawing segmentation {sw.ElapsedMilliseconds}");
            BitmapSource bitmapSource = BitmapConvertHandler.ToBitmapSourceFast(frame);

            return bitmapSource;

        }


        public static BitmapSource Plot(byte[] frame)
        {
            using (var stream = new MemoryStream(frame))
            {
                var bitmap = new System.Drawing.Bitmap(stream);
                return Plot(bitmap);
            }
        }

        public static BitmapSource Plot(Bitmap frame)
        {
            var bitmapSource = BitmapConvertHandler.ToBitmapSourceFast(frame);
            return bitmapSource;
        }

    }
}
