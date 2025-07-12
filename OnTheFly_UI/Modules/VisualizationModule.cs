using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using Emgu.CV.Reg;
using Emgu.CV.Util;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnTheFly_UI.Modules
{
    public class VisualizationModule: INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000);
        
        public YoloMetadata Metadata = null;
        
        public ConcurrentQueue<ProcessObject> PostProcessingBuffer;

        public ObservableCollection<string> HiddenClassNames { get; set; } = new ObservableCollection<string>(); 

        private ObservableCollection<ResultTableItem> _CurrentResultTable = new ObservableCollection<ResultTableItem>();

        public ObservableCollection<ResultTableItem> CurrentResultTable
        {
            get { return _CurrentResultTable; }
            set
            {
                _CurrentResultTable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentResultTable)));
            }
        }

        private BitmapSource _currentImage = null;
        public BitmapSource CurrentImage
        {
            get => _currentImage;
            set
            {
                if (_currentImage != value)
                {
                    _currentImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentImage)));
                }
            }
        }

        private ProcessObject LastProcessObject;


        public VisualizationModule(ConcurrentQueue<ProcessObject> ProcessingBuffer, YoloMetadata metadata)
        {
            if (ProcessingBuffer == null)
                throw new Exception("Processing buffer cannot be null");
            PostProcessingBuffer = ProcessingBuffer;

            Metadata = metadata;
        }


        bool isThreadAlive = false;
        
        public void StartProcess()
        {
            if (!isThreadAlive)
            {
                isThreadAlive = true;
                Task.Run(Plot).ContinueWith(continuationAction =>
                {
                    isThreadAlive = false;
                });
            }
           
        }


        private void Plot()
        {
            var sw = new Stopwatch();
            ProcessObject processObject = new ProcessObject();
            var ret = true;
            while (true)
            {
                
                if(ret)
                    sw.Restart();

                ret = PostProcessingBuffer.TryDequeue(out processObject);

                if (!ret)
                {
                    if (!SpinWait.SpinUntil(() => { return PostProcessingBuffer.Count > 0; }, Timeout))
                        break;
                    else
                        continue;
                }
                processObject.Request.Status = RequestStatus.OnRendering; // Think of making it a property of the process object

                LastProcessObject = processObject; // Save the last process object for re-showing just for image type requests

                BitmapSource bitmapSource = null;

                var hiddenNames = CurrentResultTable.Where(x => x.IsHidden).Select(x => x.Name).ToHashSet();

                if (processObject.Task == YoloTask.Detect)
                    bitmapSource = PlotHandler.PlotDetection(processObject.Frame, (YoloResult<Detection>)processObject.Result,hiddenNames: hiddenNames);
                else if (processObject.Task == YoloTask.Segment)
                    bitmapSource = PlotHandler.PlotSegmentatation(processObject.Frame, (YoloResult<Segmentation>)processObject.Result, hiddenNames: hiddenNames);
                else
                    bitmapSource = PlotHandler.Plot(processObject.Frame);




                if (bitmapSource == null)
                {
                    processObject.Request.Status = RequestStatus.Failed;
                    continue;
                }

                int waitTime = (1000 / processObject.Request.FPS) - ((int)sw.ElapsedMilliseconds);
                
                if (waitTime < 0) 
                    waitTime = 0;

                Thread.Sleep(waitTime);
                Trace.WriteLine("FPS= " + (1000/sw.ElapsedMilliseconds).ToString());




                ShowFrame(bitmapSource,processObject.Request.ResultTable);
                
                
                Trace.WriteLine($"Visualization Module = {sw.ElapsedMilliseconds}");
                processObject.Request.Status = RequestStatus.Sucess;

            }
        }

        public void ReShow()
        {
            if (LastProcessObject.Request.SourceType != RequestSourceType.Image)
                return;

            PostProcessingBuffer.Enqueue(LastProcessObject);
            StartProcess();
        }




        public void ShowFrame(BitmapSource bitmap, Dictionary<string, int> ResultTable)
        {
            CurrentImage = bitmap;

            if (CurrentResultTable == null)
                CurrentResultTable = new ObservableCollection<ResultTableItem>();


            foreach (var item in ResultTable)
            {

                var existingItem = CurrentResultTable.FirstOrDefault(x => x.Name == item.Key);
                if (existingItem != null)
                {
                    existingItem.Count = item.Value;
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentResultTable.Add(new ResultTableItem() { Name = item.Key, Count = item.Value });
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }


        }


    }
}
