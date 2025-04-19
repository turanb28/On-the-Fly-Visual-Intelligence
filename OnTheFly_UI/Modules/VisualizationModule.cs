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
    public class VisualizationModule
    {
        public delegate void DisplayFrameDelegate(BitmapSource frame);
        public DisplayFrameDelegate displayFrameFucntion;
        
        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000);
        
        public YoloMetadata Metadata = null;
        public ConcurrentQueue<ProcessObject> PostProcessingBuffer;

    


        //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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



                BitmapSource bitmapSource = null;

                if (processObject.Task == YoloTask.Detect)
                    bitmapSource = PlotHandler.PlotDetection(processObject.Frame, (YoloResult<Detection>)processObject.Result);
                else if (processObject.Task == YoloTask.Segment)
                    bitmapSource = PlotHandler.PlotSegmentatation(processObject.Frame, (YoloResult<Segmentation>)processObject.Result);
                //PlotHandler.Plot<typeof(>( processObject.Result);
                //BitmapSource bitmapSource = PlotHandler.PlotSegmentatation(processObject.Frame, (YoloResult<Segmentation>)processObject.Result);

                if(bitmapSource == null)
                {
                    processObject.Request.Status = RequestStatus.Failed;
                    continue;
                }

                int waitTime = (1000 / processObject.Request.FPS) - ((int)sw.ElapsedMilliseconds);
                
                if (waitTime < 0) 
                    waitTime = 0;

                Thread.Sleep(waitTime);
                Trace.WriteLine("FPS= " + (1000/sw.ElapsedMilliseconds).ToString());
                displayFrameFucntion?.Invoke(bitmapSource);
                processObject.Request.Status = RequestStatus.Sucess;

            }
        }


    



    }
}
