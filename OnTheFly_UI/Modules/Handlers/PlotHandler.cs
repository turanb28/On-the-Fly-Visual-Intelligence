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
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Modules.Handlers
{
    public static class PlotHandler
    {

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
            
            var ratio = configuration.Width / (float)frame.Width;

            frame = new Bitmap(frame, new System.Drawing.Size((int)(frame.Width * ratio), (int)(frame.Height * ratio)));

            var g = System.Drawing.Graphics.FromImage(frame);

            var pen = new System.Drawing.Pen(System.Drawing.Color.Transparent, 0);
            var rect = new System.Drawing.Rectangle(0, 0, 0, 0);

            foreach (var obj in result)
            {
                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;

                rect.Width  = obj.Bounds.Width;
                rect.Height = obj.Bounds.Height;
                rect.X      = obj.Bounds.X;
                rect.Y      = obj.Bounds.Y;

                string text = $"{obj.Name.Name} %{Math.Round(obj.Confidence * 100, 1)}";


                DrawDetectionRectangle(frame, rect, text, configuration.ObjectColors[obj.Name.Id], configuration.Font, configuration.FontColor, ratio);

                //pen.Width = configuration.BorderThickness;
                //pen.Color = configuration.ObjectColors[obj.Name.Id];

                //g.DrawRectangle(pen, rect);

                //var size = g.MeasureString(text, configuration.Font);

                //var point = new System.Drawing.Point((int)(obj.Bounds.X * ratio), (int)((obj.Bounds.Y - (int)size.Height) * ratio));

                //var backgroundFiller = new System.Drawing.Rectangle(point, size.ToSize());

                //g.FillRectangle(new System.Drawing.SolidBrush(pen.Color), backgroundFiller);

                //g.DrawString(text, configuration.Font, new System.Drawing.SolidBrush(configuration.FontColor), point);
            }

            BitmapSource bitmapSource = BitmapConvertHandler.ToBitmapSourceFast(frame);
            return bitmapSource;
        }

        public static BitmapSource PlotObbDetection(byte[] frame, YoloResult<ObbDetection> result, PlotConfiguration? configuration = null, HashSet<string> hiddenNames = null)
        {
            using (var stream = new MemoryStream(frame))
            {
                var bitmap = new System.Drawing.Bitmap(stream);
                return PlotObbDetection(bitmap, result, configuration, hiddenNames);
            }
        }

        public static BitmapSource PlotObbDetection(Bitmap frame, YoloResult<ObbDetection> result, PlotConfiguration? configuration = null, HashSet<string> hiddenNames = null)
        {
            if (result.Count == 0)
                return BitmapConvertHandler.ToBitmapSourceFast(frame);

            if (configuration == null)
                configuration = new PlotConfiguration();

            var ratio = configuration.Width / (float)frame.Width;
            frame = new Bitmap(frame, new System.Drawing.Size((int)(frame.Width * ratio), (int)(frame.Height * ratio)));

            var g = System.Drawing.Graphics.FromImage(frame);
            var pen = new System.Drawing.Pen(System.Drawing.Color.Cyan, 3);
            var rect = new System.Drawing.Rectangle(50, 50, 1, 100);

            foreach (var obj in result)
            {
                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;
              

                string text = $"{obj.Name.Name} %{Math.Round(obj.Confidence * 100, 1)}";
                pen.Width = configuration.BorderThickness;
                pen.Color = configuration.ObjectColors[obj.Name.Id];

                System.Drawing.Point[] points = GetObbPoints(obj, ratio);

                g.DrawPolygon(pen, points);

                var size = g.MeasureString(text, configuration.Font);
                var point = new System.Drawing.Point((int)(obj.Bounds.X * ratio), (int)((obj.Bounds.Y * ratio) - size.Height));
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



            var ratio = configuration.Width / (float)frame.Width;

            var frameRect = new System.Drawing.Rectangle(0, 0, frame.Width, frame.Height);



            var bitmapData = frame.LockBits(frameRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

            foreach (var obj in result)
            {

                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;

                if (obj.Confidence < configuration.MinimumConfidence) 
                    continue;

                if (obj.Name.Id > configuration.ObjectColors.Count)
                    pen.Color = System.Drawing.Color.Gray;
                else
                    pen.Color = configuration.ObjectColors[obj.Name.Id];

                int pixelSize = 3;
                unsafe
                {
                    byte* sourceRow;

                    for (var x = 0; x < obj.Mask.Width; x++)
                    {
                        for (var y = 0; y < obj.Mask.Height; y++)
                        {
                            if (obj.Mask[y, x] > configuration.MinimumConfidence)
                            {
                                sourceRow = (byte*)bitmapData.Scan0 + ((y + obj.Bounds.Y) * bitmapData.Stride);

                                sourceRow[(x + obj.Bounds.X) * pixelSize + 0] = Convert.ToByte(sourceRow[(x + obj.Bounds.X) * pixelSize + 0] * (1 - alpha) + pen.Color.B * alpha);
                                sourceRow[(x + obj.Bounds.X) * pixelSize + 1] = Convert.ToByte(sourceRow[(x + obj.Bounds.X) * pixelSize + 1] * (1 - alpha) + pen.Color.G * alpha);
                                sourceRow[(x + obj.Bounds.X) * pixelSize + 2] = Convert.ToByte(sourceRow[(x + obj.Bounds.X) * pixelSize + 2] * (1 - alpha) + pen.Color.R * alpha);
                            }

                        }
                    }
                }
            }
            frame.UnlockBits(bitmapData);

            var frameSized = new Bitmap(frame, new System.Drawing.Size((int)(frame.Width * ratio), (int)(frame.Height * ratio)));

            var g = System.Drawing.Graphics.FromImage(frameSized);

            foreach (var obj in result)
            {
                if (obj.Confidence < configuration.MinimumConfidence) 
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

                frameSized = DrawDetectionRectangle(frameSized, rect, text, pen.Color, configuration.Font, configuration.FontColor, ratio);

                //pen.Width = configuration.BorderThickness;
                //g.DrawRectangle(pen, rect);

                //var size = g.MeasureString(text, configuration.Font);
                //var point = new System.Drawing.Point(obj.Bounds.X, obj.Bounds.Y - (int)size.Height);
                //var backgroundFiller = new System.Drawing.Rectangle(point, size.ToSize());

                //g.FillRectangle(new System.Drawing.SolidBrush(pen.Color), backgroundFiller);

                //g.DrawString(text, configuration.Font, new System.Drawing.SolidBrush(configuration.FontColor), point);

            }


            Trace.WriteLine($"drawing segmentation {sw.ElapsedMilliseconds}");
            BitmapSource bitmapSource = BitmapConvertHandler.ToBitmapSourceFast(frameSized);

            return bitmapSource;

        }

        public static BitmapSource PlotPose(byte[] frame, YoloResult<Pose> result, PlotConfiguration? configuration = null, HashSet<string> hiddenNames = null)
        {
            using (var stream = new MemoryStream(frame))
            {
                var bitmap = new System.Drawing.Bitmap(stream);
                return PlotPose(bitmap, result, configuration, hiddenNames);
            }
        }

        public static BitmapSource PlotPose(Bitmap frame, YoloResult<Pose> result, PlotConfiguration? configuration = null, HashSet<string> hiddenNames = null) // Make it adaptive

        { 
            // YOLO11 pose models use the -pose suffix, i.e. yolo11n-pose.pt.
            // These models are trained on the COCO keypoints dataset and are suitable for a variety of pose estimation tasks.
            // In the default YOLO11 pose model, there are 17 keypoints, each representing a different part of the human body:
            // 0: Nose, 1: Left Eye, 2: Right Eye, 3: Left Ear, 4: Right Ear, 5: Left Shoulder, 6: Right Shoulder,
            // 7: Left Elbow, 8: Right Elbow, 9: Left Wrist, 10: Right Wrist, 11: Left Hip, 12: Right Hip,
            // 13: Left Knee, 14: Right Knee, 15: Left Ankle, 16: Right Ankle

            if (result.Count == 0)
                return BitmapConvertHandler.ToBitmapSourceFast(frame);

            if (configuration == null)
                configuration = new PlotConfiguration();


            var pen = new System.Drawing.Pen(System.Drawing.Color.Cyan, 3);
            var rect = new System.Drawing.Rectangle(50, 50, 100, 100);


            var ratio = configuration.Width / (float)frame.Width;
            frame = new Bitmap(frame, new System.Drawing.Size((int)(frame.Width * ratio), (int)(frame.Height * ratio) ) );

            var g = Graphics.FromImage(frame);




            foreach (var obj in result)
            {
                if (hiddenNames != null && hiddenNames.Contains(obj.Name.Name))
                    continue;

                var keypoints = obj;

                foreach (var kp in keypoints)
                {
                    using var brush = new SolidBrush(configuration.KeypointColor);
                    g.FillEllipse(brush, (kp.Point.X* ratio) - 3, (kp.Point.Y* ratio) - 3, 6, 6);
                  
                }


                // Draw lines between keypoints

                var lines = new (int, int)[]
                {
                    (0, 1), (0, 2), (1, 3), (2, 4), // Head
                    (5, 6), (5, 7), (6, 8), // Shoulders and Arms
                    (7, 9), (8, 10), // Elbows and Wrists
                    (11, 12), (11, 13), (12, 14), // Hips and Legs
                    (5,11),(6,12),// Hips and shoulders
                    (13, 15), (14, 16) // Knees and Ankles
                };


                foreach (var line in lines)
                {

                    var start = new PointF(keypoints[line.Item1].Point.X * ratio, keypoints[line.Item1].Point.Y * ratio);
                    var end = new PointF(keypoints[line.Item2].Point.X * ratio, keypoints[line.Item2].Point.Y * ratio);
                    
                    if (start.X * ratio < 0 || start.Y * ratio < 0 || end.X * ratio < 0 || end.Y * ratio < 0)
                        continue; // Skip invalid points

                    pen = new Pen(configuration.LineColor, configuration.BorderThickness);
                    g.DrawLine(pen, start, end);
                }


                if (obj.Name.Id > configuration.ObjectColors.Count)
                    pen.Color = System.Drawing.Color.Gray;
                else
                    pen.Color = configuration.ObjectColors[obj.Name.Id];


                string text = $"{obj.Name.Name} %{Math.Round(obj.Confidence * 100, 1)}";

                var size = g.MeasureString(text, configuration.Font);
                var point = new System.Drawing.Point((int)(obj.Bounds.X * ratio), (int)(obj.Bounds.Y * ratio));
                var backgroundFiller = new System.Drawing.Rectangle(point, size.ToSize());

                g.FillRectangle(new System.Drawing.SolidBrush(pen.Color), backgroundFiller);

                g.DrawString(text, configuration.Font, new System.Drawing.SolidBrush(configuration.FontColor), point);



          
            }

            return BitmapConvertHandler.ToBitmapSourceFast(frame);
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

        private static System.Drawing.Point[] GetObbPoints(ObbDetection obj , float ratio = 1) 
        {
            var _angle = obj.Angle * MathF.PI / 180.0f;

            var b = MathF.Cos(_angle) * .5f;
            var a = MathF.Sin(_angle) * .5f;

            var x = (int)(obj.Bounds.X * ratio);
            var y = (int)(obj.Bounds.Y * ratio);
            var w = (int)(obj.Bounds.Width * ratio);
            var h = (int)(obj.Bounds.Height * ratio);

            var points = new System.Drawing.Point[4];

            points[0].X = (int)MathF.Round(x - a * h - b * w, 0);
            points[0].Y = (int)MathF.Round(y + b * h - a * w, 0);

            points[1].X = (int)MathF.Round(x + a * h - b * w, 0);
            points[1].Y = (int)MathF.Round(y - b * h - a * w, 0);

            points[2].X = (int)MathF.Round(2f * x - points[0].X, 0);
            points[2].Y = (int)MathF.Round(2f * y - points[0].Y, 0);

            points[3].X = (int)MathF.Round(2f * x - points[1].X, 0);
            points[3].Y = (int)MathF.Round(2f * y - points[1].Y, 0);

            // Calculate the distances of each point from the origin (0, 0)
            var distance1 = Math.Sqrt(Math.Pow(points[0].X, 2) + Math.Pow(points[0].Y, 2));
            var distance2 = Math.Sqrt(Math.Pow(points[1].X, 2) + Math.Pow(points[1].Y, 2));

            // Rotate if necessary to ensure pt[0] is the top-left point
            if (distance2 < distance1)
            {
                var temp = points[0];
                points[0] = points[1];
                points[1] = points[2];
                points[2] = points[3];
                points[3] = temp;
            }

            return points;
        }
    
        private static Bitmap DrawDetectionRectangle(Bitmap frame, Rectangle rect, string text, System.Drawing.Color color, Font font, System.Drawing.Color frontColor, float ratio) // Test it
        {
            var g = System.Drawing.Graphics.FromImage(frame);

            var pen = new System.Drawing.Pen(color, 2);
            
            rect.X = (int)(rect.X * ratio);
            rect.Y = (int)(rect.Y * ratio);
            rect.Width = (int)(rect.Width * ratio);
            rect.Height = (int)(rect.Height * ratio);
            g.DrawRectangle(pen, rect);
            var size = g.MeasureString(text, font);
            var point = new System.Drawing.Point(rect.X, rect.Y - (int)size.Height);
            var backgroundFiller = new System.Drawing.Rectangle(point, size.ToSize());
            g.FillRectangle(new System.Drawing.SolidBrush(color), backgroundFiller);
            g.DrawString(text, font, new System.Drawing.SolidBrush(frontColor), point);
            return frame;
        }

        private static Bitmap MergedBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap result = new Bitmap(Math.Max(bmp1.Width, bmp2.Width),
                                       Math.Max(bmp1.Height, bmp2.Height));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp2, System.Drawing.Point.Empty);
                g.DrawImage(bmp1, System.Drawing.Point.Empty);
            }
            return result;
        }
    }
}
