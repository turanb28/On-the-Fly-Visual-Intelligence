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
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Emgu.CV.Structure.MCvMatND;

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

        private void AddImage_Click(object sender, RoutedEventArgs e)
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


        private async void AddStream_Click(object sender, RoutedEventArgs e)
        {
            var link = await UIUserEntry.Show("Enter the live stream or video URL to start processing");

            if (string.IsNullOrEmpty(link))
                return;

            // https://www.pexels.com/download/video/35217602/ 
            DataAcquisitionModule.RequestStream(link);
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
            DataAcquisitionModule.PreprocessingBuffer.Clear();
            ProcessingModule.PostProcessingBuffer.Clear();
            VisualizationModule.PostProcessingBuffer.Clear();

          
            var request = e.AddedItems[0] as RequestObject;

            if (request == null)
                return;

            var index = DataAcquisitionModule.Requests.IndexOf(request);
            if (index < 0)
                return;
            var a = DataAcquisitionModule.Requests[index];
            if (a.SourceType == Modules.Enums.RequestSourceType.Video) /// Make it better
            {
                videoProgessBar.Visibility = Visibility.Visible;
                videoDurationProgessBar.Visibility = Visibility.Visible;
            }
            else
            {
                videoProgessBar.Visibility = Visibility.Hidden;
                videoDurationProgessBar.Visibility = Visibility.Hidden;
            }    

            DataAcquisitionModule.RequestWithID(a.Id);

        }
    

      

        #region MainWindow Events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.Source is Display) || (e.Source is ProgressBar))
                return;

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeApp_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {

            if (sender is not MenuItem)
                return;

            var modelPath = ((MenuItem)sender).ToolTip.ToString();


            if (string.IsNullOrEmpty(modelPath))
                return;

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
            

        }

       
    }
}