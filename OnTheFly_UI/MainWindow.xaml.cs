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
        private string modelPath = "C:\\Users\\PC\\Projects\\Visdrone Dataset -Yolo 11\\Training";
        private BitmapSource _selectedImage { get; set; } = null;
        public BitmapSource SelectedImage { get { return _selectedImage; } set { _selectedImage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedImage")); } }

        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        public event PropertyChangedEventHandler? PropertyChanged;

        private string selectedModelPath = string.Empty;

        public DataAcquisitionModule DataAcquisitionModule { get; set; }
        public ProcessingModule ProcessingModule;
        public VisualizationModule VisualizationModule;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            selectedModelPath = @"C:\Users\PC\Projects\Visdrone Dataset -Yolo 11\Training\yolo11m-seg.onnx";
            selectedModelPath = @"C:\Users\PC\Projects\Visdrone Dataset -Yolo 11\Training\runs\detect\train\weights\best_full.onnx";

            DataAcquisitionModule = new DataAcquisitionModule();

            ProcessingModule = new ProcessingModule(selectedModelPath, DataAcquisitionModule.PreprocessingBuffer);


            VisualizationModule = new VisualizationModule(ProcessingModule.PostProcessingBuffer, ProcessingModule.Metadata);

            DataAcquisitionModule.DataAcquired += () => { ProcessingModule.StartProcess(); VisualizationModule.StartProcess(); };

            VisualizationModule.displayFrameFucntion = ShowFrame;

            sidebar.Values = DataAcquisitionModule.Requests;
            
        }




        private async void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var a = SelectedImageControl.Source;
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
            var a = SelectedImageControl.Source;
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

        public void ShowFrame(BitmapSource bitmap)
        {
            SelectedImage = bitmap;
        }

        private void AddStream_Click(object sender, RoutedEventArgs e)
        {
            //DataAcquisitionModule.ReadUDPStream("udp://127.0.0.1:6666");

            //TestList.Add(new RequestObject(@"C:\Users\PC\Projects\Visdrone Dataset -Yolo 11\Training\VisDrone2019-DET-test-dev\images\0000006_05168_d_0000013.jpg", RequestSourceType.Image));

            ProcessingModule.SelectModel(@"C:\Users\PC\Projects\Visdrone Dataset -Yolo 11\Training\runs\detect\train\weights\best_full.onnx");
            //CancellationTokenSource.Cancel();
            //MessageBox.Show(DataAcquisitionModule.PreprocessingBuffer.Count.ToString());

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
    }
}