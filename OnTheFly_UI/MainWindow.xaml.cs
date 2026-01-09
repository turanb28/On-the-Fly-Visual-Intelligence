using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using Microsoft.ML.OnnxRuntime;
using Microsoft.Win32;
using OnTheFly_UI.Components;
using OnTheFly_UI.Modules;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Handlers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public DataAcquisitionModule DataAcquisitionModule { get; set; }
        public ProcessingModule ProcessingModule { get; set; }
        public VisualizationModule VisualizationModule { get; set; }
        public bool _testing { get; set; } = false;
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            DataAcquisitionModule = new DataAcquisitionModule();
            ProcessingModule = new ProcessingModule(DataAcquisitionModule.PreprocessingBuffer);
            VisualizationModule = new VisualizationModule(ProcessingModule.PostProcessingBuffer, ProcessingModule.Metadata);

            DataAcquisitionModule.DataAcquired += () => { ProcessingModule.StartProcess(); VisualizationModule.StartProcess(); }; 
            ProcessingModule.ModelLoaded += (string m) => { 
                    UIMessageBox.Show($"{m} is loaded");
                if (DataAcquisitionModule.Requests.Count <= 0)
                    return;
                DataAcquisitionModule.RequestWithID(DataAcquisitionModule.Requests[sidebar.SelectedIndex].Id);
            };
            ProcessingModule.ModelUnloaded += (string m) =>
            {
                UIMessageBox.Show($"{m} is unloaded");
            };
            ProcessingModule.ProcessingException += (e) => { UIMessageBox.Show(e, UIMessageBox.InformationType.Error); };
            Display.DisplayUserInteraction += VisualizationModule.InteractionEventHnadler;

            sidebar.Values = DataAcquisitionModule.Requests;




            var a = RecentFileHandler.GetRecentFiles();

            foreach (var item in a)
            {
                if (!File.Exists(item))
                    continue;
                ProcessingModule.AddModel(item);
                
            }

        }

        private async void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = "C:\\Desktop";
            file.Filter = "Image  | *.png;*.jpg;*.jpeg;*.bmp";
            file.FilterIndex = 1;
            file.Multiselect = true;
            file.ShowDialog();
            DataAcquisitionModule.RequestImage(file.FileNames.Take(2000).ToArray());
        }

        private void AddVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = "C:\\Desktop";
            file.Filter = "Video |*.mp4";
            file.FilterIndex = 0;
            file.Multiselect = false;
            file.ShowDialog();
            if(string.IsNullOrEmpty(file.FileName))
                return;
            DataAcquisitionModule.RequestVideo(file.FileName);
        }


        private void AddStream_Click(object sender, RoutedEventArgs e)
        {
            //UIMessageBox.Show("This feature is coming soon!");

            //DataAcquisitionModule.RequestStream("udp://127.0.0.1:23000"); //@"https://www.pexels.com/download/video/35217602/"); 
            DataAcquisitionModule.RequestStream(@"https://www.pexels.com/download/video/35217602/"); 
        }

        private void AddTestStream_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = "C:\\Desktop";
            file.Filter = "Video |*.mp4";
            file.FilterIndex = 0;
            file.Multiselect = false;
            file.ShowDialog();
            if (string.IsNullOrEmpty(file.FileName))
                return;

            _testing = true;
            var thread = new Thread(() => {
                Process cmd = new Process();
                cmd.StartInfo.FileName = "ffmpeg";
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                cmd.StartInfo.WorkingDirectory = @"C:\Users\PC";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = false;
                cmd.StartInfo.UseShellExecute = false;
                var a = $"""-re -i "{file.FileName}" -v 0 -vcodec mpeg4 -f mpegts udp://127.0.0.1:23000""";
                cmd.StartInfo.Arguments = a;
                //cmd.StartInfo.Arguments = """-re -i "C:\Users\PC\Projects\Visdrone Dataset -Yolo 11\Training\Test Files\cut.mp4" -v 0 -vcodec mpeg4 -f mpegts udp://127.0.0.1:23000""";
                cmd.Start();

                //cmd.Exited += (s, e) => { 
                //    _testing = true; };

                SpinWait.SpinUntil(() => { return !_testing ;  });
                cmd.Kill( );
            });

            thread.IsBackground = true;

            thread.Start();

            AddStream_Click(sender, e);
        }

        private void Add_Model(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = "C:\\Desktop";
            file.Filter = "Model File |*.onnx";
            file.FilterIndex = 0;
            file.Multiselect = false;
            file.ShowDialog();

            if (string.IsNullOrEmpty(file.FileName))
                return;
            RecentFileHandler.AddRecentFile(file.FileName);
            ProcessingModule.AddModel(file.FileName);
        }



        private void sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            VisualizationModule.PostProcessingBuffer.Clear();

            var index = DataAcquisitionModule.Requests.IndexOf(e.AddedItems[0] as RequestObject);
            if (index < 0)
                return;
            var a = DataAcquisitionModule.Requests[index];
            if (a.SourceType == Modules.Enums.RequestSourceType.Video) /// Make it better
            {
                videoProgessBar.Visibility = Visibility.Visible;  
            }
            else
                videoProgessBar.Visibility = Visibility.Hidden;

            DataAcquisitionModule.RequestWithID(a.Id);

        }
    

      

        #region MainWindow Events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Display)
                return;

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            _testing = false;
            this.Close();
        }

        private void MinimizeApp_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var modelPath = ((MenuItem)sender).ToolTip.ToString();

            var model = ProcessingModule.GetModel(modelPath);

            if (model == null)
            {
                UIMessageBox.Show($"The model is not found in path {modelPath}.");
                return;
            }
            else
            {
                if (model.IsSelected)
                    ProcessingModule.UnselectModel(model.Path);
                else
                    ProcessingModule.SelectModel(model.Path);
            }

        }

        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var a = App.Current.Resources["Color_Background_Light"];

        }
    }
}