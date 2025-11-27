using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
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
using System.Xml.Linq;

namespace OnTheFly_UI.Modules
{
    public class VisualizationModule: INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000); //Change messagebox it is block tihs thread

        public YoloMetadata Metadata = null;
        
        public ConcurrentQueue<ProcessObject> PostProcessingBuffer;

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

        private RequestObject _currentRequest = null;
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
                CurrentRequest = processObject.Request;

                processObject.Request.Status = RequestStatus.OnRendering; // Think of making it a property of the process object

                LastProcessObject = processObject; // Save the last process object for re-showing just for image type requests


                BitmapSource bitmapSource = null;

                var hiddenNames = CurrentResultTable.Where(x => x.IsHidden).Select(x => x.Name).ToHashSet();

                if (processObject.Result == null)
                    bitmapSource = PlotHandler.Plot(processObject.Frame);
                else if (processObject.Result.GetType() == typeof(YoloResult<Detection>))
                        bitmapSource = PlotHandler.PlotDetection(processObject.Frame, (YoloResult<Detection>)processObject.Result,hiddenNames: hiddenNames);
                else if (processObject.Result.GetType() == typeof(YoloResult<Segmentation>))
                    bitmapSource = PlotHandler.PlotSegmentatation(processObject.Frame, (YoloResult<Segmentation>)processObject.Result, hiddenNames: hiddenNames);
                else if (processObject.Result.GetType() == typeof(YoloResult<ObbDetection>))
                    bitmapSource = PlotHandler.PlotObbDetection(processObject.Frame, (YoloResult<ObbDetection>)processObject.Result, hiddenNames: hiddenNames);
                else if (processObject.Result.GetType() == typeof(YoloResult<Pose>))
                    bitmapSource = PlotHandler.PlotPose(processObject.Frame, (YoloResult<Pose>)processObject.Result, hiddenNames: hiddenNames);
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
                try
                {
                    //Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
                    //Trace.WriteLine("FPS= " + (1000 / sw.ElapsedMilliseconds).ToString());

                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error in Visualization Module: " + ex.Message);
                    processObject.Request.Status = RequestStatus.Failed;
                }




                ShowFrame(bitmapSource,processObject.ResultTable);
                
                
                //Trace.WriteLine($"Visualization Module = {sw.ElapsedMilliseconds}");
                processObject.Request.Status = RequestStatus.Sucess;

            }
        }


        public void InteractionEventHnadler()
        {
            if (LastProcessObject.Request.SourceType != RequestSourceType.Image)
                return;

            PostProcessingBuffer.Enqueue(LastProcessObject);
            StartProcess();
        }




        public void ShowFrame(BitmapSource bitmap, List<ResultTableItem> ResultTable)
        {
            CurrentImage = bitmap;
            CurrentResultTable = new ObservableCollection<ResultTableItem>(ResultTable);
        }


    }
}
