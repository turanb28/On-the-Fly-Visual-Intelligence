using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using Emgu.CV.Dnn;
using Emgu.CV.Reg;
using Emgu.CV.Util;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Enums;
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
using System.Windows.Threading;
using System.Xml.Linq;

namespace OnTheFly_UI.Modules
{
    public class VisualizationModule: INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000);

        public required YoloMetadata? Metadata = null;
        
        public required ConcurrentQueue<ProcessObject> PostProcessingBuffer;

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

        private BitmapSource? _currentImage = null;
        public BitmapSource? CurrentImage
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

        private RequestObject _currentRequest = new RequestObject();
        public RequestObject CurrentRequest
        {
            get => _currentRequest;
            set
            {
                if (_currentRequest != value)
                {
                    _currentRequest = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRequest)));
                }
            }
        }

        private double _index = 0;

        public double Index { get { return _index; } set { _index = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
            }
        }

        private ProcessObject? LastProcessObject=null;
        public float Confidence { get; set; } = 0.0f;
        public VisualizationModule()
        {

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
            ProcessObject? processObject = new ProcessObject();
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

                if(processObject == null)
                    continue;

                CurrentRequest = processObject.Request;


                LastProcessObject = processObject; // Save the last process object for re-showing just for image type requests

                Index = processObject.Index;

                BitmapSource? bitmapSource = null;

                var hiddenNames = CurrentResultTable.Where(x => x.IsHidden).Select(x => x.Name).ToHashSet();

                var configuration = new PlotConfiguration
                {
                    MinimumConfidence = Confidence,
                };

                if (processObject.Result == null)
                    bitmapSource = PlotHandler.Plot(processObject.Frame);
                else if (processObject.Result.GetType() == typeof(YoloResult<Detection>))
                        bitmapSource = PlotHandler.PlotDetection(processObject.Frame, (YoloResult<Detection>)processObject.Result,configuration: configuration, hiddenNames: hiddenNames);
                else if (processObject.Result.GetType() == typeof(YoloResult<Segmentation>))
                    bitmapSource = PlotHandler.PlotSegmentatation(processObject.Frame, (YoloResult<Segmentation>)processObject.Result, configuration: configuration, hiddenNames: hiddenNames);
                else if (processObject.Result.GetType() == typeof(YoloResult<ObbDetection>))
                    bitmapSource = PlotHandler.PlotObbDetection(processObject.Frame, (YoloResult<ObbDetection>)processObject.Result, configuration: configuration, hiddenNames: hiddenNames);
                else if (processObject.Result.GetType() == typeof(YoloResult<Pose>))
                    bitmapSource = PlotHandler.PlotPose(processObject.Frame, (YoloResult<Pose>)processObject.Result,configuration: configuration, hiddenNames: hiddenNames);
                else
                    bitmapSource = PlotHandler.Plot(processObject.Frame);


                if (bitmapSource == null)
                    continue;

                int waitTime = 0;
                
                if (processObject.Request.FPS !=0 )
                    waitTime = (1000 / processObject.Request.FPS) - ((int)sw.ElapsedMilliseconds);
                
                if (waitTime < 0) 
                    waitTime = 0;

                Thread.Sleep(waitTime);

                if(processObject.Request.ResultTables.Count > processObject.Index)
                    ShowFrame(bitmapSource, processObject.Request.ResultTables[processObject.Index]);
                else
                    ShowFrame(bitmapSource, new List<ResultTableItem>());
            }
        }


        public void InteractionEventHnadler()
        {
            if (LastProcessObject == null)
                return;

            if (LastProcessObject.Request.SourceType != RequestSourceType.Image)
                return;

            PostProcessingBuffer.Enqueue(LastProcessObject);
            StartProcess();
        }




        public void ShowFrame(BitmapSource bitmap, List<ResultTableItem> ResultTable)
        {
            CurrentImage = bitmap;

            var currentItemsDict = CurrentResultTable.ToDictionary(i => i.Name);

            var namesInNewResult = new HashSet<string>();
            var itemsToAdd = new List<ResultTableItem>();
            var itemsToRemove = new List<ResultTableItem>();

            foreach (var newItem in ResultTable)
            {
                namesInNewResult.Add(newItem.Name);

                if (currentItemsDict.TryGetValue(newItem.Name, out var existingItem))
                    existingItem.Count = newItem.Count;
                else
                    itemsToAdd.Add(newItem);
            }

            foreach (var currentItem in CurrentResultTable)
            {
                if (!namesInNewResult.Contains(currentItem.Name))
                    itemsToRemove.Add(currentItem);
            }

            if (itemsToAdd.Any() || itemsToRemove.Any())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in itemsToRemove)
                        CurrentResultTable.Remove(item);

                    foreach (var item in itemsToAdd)
                        CurrentResultTable.Add(item);
                },DispatcherPriority.Render);
            }

        }


    }
}
