﻿using Compunet.YoloSharp;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private BitmapSource _selectedImage;
        public BitmapSource SelectedImage
        {
            get => _selectedImage;
            set
            {
                if (_selectedImage != value)
                {
                    _selectedImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));
                }
            }
        }


        private Dictionary<string, int> _CurrentResultTable;

        public Dictionary<string, int> CurrentResultTable
        {
            get { return _CurrentResultTable; }
            set {
                _CurrentResultTable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentResultTable)));
            }
        }



        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        public event PropertyChangedEventHandler? PropertyChanged;

        private string selectedModelPath = string.Empty;
        public DataAcquisitionModule DataAcquisitionModule { get; set; }
        public ProcessingModule ProcessingModule { get; set; }
        public VisualizationModule VisualizationModule;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            DataAcquisitionModule = new DataAcquisitionModule();
            ProcessingModule = new ProcessingModule(DataAcquisitionModule.PreprocessingBuffer);
            VisualizationModule = new VisualizationModule(ProcessingModule.PostProcessingBuffer, ProcessingModule.Metadata);
            DataAcquisitionModule.DataAcquired += () => { ProcessingModule.StartProcess(); VisualizationModule.StartProcess(); }; // VisualizationModule.StartProcess();
            ProcessingModule.ModelLoaded += () => { UIMessageBoxHandler.Show("Idle"); };
            ProcessingModule.ProcessingException += (e) => { UIMessageBoxHandler.Show(e); };
            VisualizationModule.displayFrameFucntion = ShowFrame;
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
            file.Filter = "Image  | *.png;*.jpg;*.jpeg:*.bmp";
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

        public void ShowFrame(BitmapSource bitmap,Dictionary<string,int> ResultTable)
        {
            SelectedImage = bitmap;
            CurrentResultTable = ResultTable;
        }

        private void AddStream_Click(object sender, RoutedEventArgs e)
        {
            
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

            var index = DataAcquisitionModule.Requests.IndexOf(e.AddedItems[0] as RequestObject);
            if (index < 0)
                return;
            var a = DataAcquisitionModule.Requests[index];
            DataAcquisitionModule.RequestWithID(a.Id);
            //ProcessingModule.StartProcess();
            //VisualizationModule.StartProcess();

        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is Display)
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
            var modelPath = ((MenuItem)sender).ToolTip.ToString();

            var model = ProcessingModule.GetModel(modelPath);

            if (model == null)
            {
                UIMessageBoxHandler.Show($"The model is not found in path {modelPath}.");
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

    }
}