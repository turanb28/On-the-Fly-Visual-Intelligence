using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using Emgu.CV.Rapid;
using OnTheFly_UI.Modules.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace OnTheFly_UI.Components
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class Display : UserControl
    {

        private TransformGroup displayImageTransformGroup = new TransformGroup();
        private TranslateTransform displayImageTranslateTransform = new TranslateTransform();
        private ScaleTransform displayImageScaleTransform = new ScaleTransform();

        private System.Windows.Point panStart;
        private System.Windows.Point currentTransform;
        private System.Windows.Point maxBorderPan = new System.Windows.Point(900, 300);

        //public List<CVObject> Objects { get; private set; } = new List<CVObject> { };
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { 
                SetValue(SourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Display));



        public ObservableCollection<ResultTableItem> ResultTable 
        {
            get { return (ObservableCollection<ResultTableItem>)GetValue(ResultTableProperty); }
            set { 
                SetValue(ResultTableProperty, value); 
            }
        }

        // Using a DependencyProperty as the backing store for ResultTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResultTableProperty =
            DependencyProperty.Register("ResultTable", typeof(ObservableCollection<ResultTableItem>), typeof(Display));



        public Display()
        {
            InitializeComponent();
            displayImageTransformGroup.Children.Add(displayImageTranslateTransform);
            displayImageTransformGroup.Children.Add(displayImageScaleTransform);
            objectCanvas.RenderTransform = displayImageTransformGroup;


           
        }

        private void main_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source.GetType() == typeof(ListBox) )
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(border);

                var change = pos - panStart;

                displayImageTranslateTransform.X = currentTransform.X + (change.X / displayImageScaleTransform.ScaleX);
                displayImageTranslateTransform.Y = currentTransform.Y + (change.Y / displayImageScaleTransform.ScaleY);

                if (displayImageTranslateTransform.X >= maxBorderPan.X)
                    displayImageTranslateTransform.X = maxBorderPan.X;
                else if (displayImageTranslateTransform.X <= -1 * maxBorderPan.X)
                    displayImageTranslateTransform.X = -1 * maxBorderPan.X;

                if (displayImageTranslateTransform.Y >= maxBorderPan.Y)
                    displayImageTranslateTransform.Y = maxBorderPan.Y;
                else if (displayImageTranslateTransform.Y <= -1 * maxBorderPan.Y)
                    displayImageTranslateTransform.Y = -1 * maxBorderPan.Y;
            }

        }
        

        private void main_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.Source.GetType() == typeof(ListBox))
                return;

            var pos = e.GetPosition(border);
            panStart = pos;
            currentTransform.X = displayImageTranslateTransform.X;
            currentTransform.Y = displayImageTranslateTransform.Y;

            Mouse.Capture(border);

        }

        private void main_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleRatio = 1.1;
            double maxScaleRate = 8;
            double minScaleRate = 0.75;


            if (e.Delta > 0)
            {
                if (displayImageScaleTransform.ScaleX >= maxScaleRate)
                {
                    displayImageScaleTransform.ScaleX = maxScaleRate;
                    displayImageScaleTransform.ScaleY = maxScaleRate;
                    return;
                }

                displayImageScaleTransform.ScaleX *= scaleRatio;
                displayImageScaleTransform.ScaleY *= scaleRatio;
            }
            else if (e.Delta < 0)
            {
                if (displayImageScaleTransform.ScaleX <= minScaleRate)
                {
                    displayImageScaleTransform.ScaleX = minScaleRate;
                    displayImageScaleTransform.ScaleY = minScaleRate;
                    return;
                }

                displayImageScaleTransform.ScaleX /= scaleRatio;
                displayImageScaleTransform.ScaleY /= scaleRatio;
            }
        }
        public void SetDisplayImageCenter()
        {

            displayImageScaleTransform.ScaleX = 1;
            displayImageScaleTransform.ScaleY = 1;

            displayImageTranslateTransform.X = 0;
            displayImageTranslateTransform.Y = 0;

        }

        private void border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            border.ReleaseMouseCapture();
        }

        private void border_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetDisplayImageCenter();
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

                //bitmapSource.Freeze();
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
                    return System.Windows.Media.PixelFormats.Bgr32;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = sender as Button;

            var item = a.DataContext;
            string key = "";
            var b = item.GetType().GetProperty("Name").GetValue(item,null).ToString();
            MessageBox.Show(b);
        }
    }
}
