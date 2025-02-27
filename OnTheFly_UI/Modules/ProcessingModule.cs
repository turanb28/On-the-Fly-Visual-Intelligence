using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using OnTheFly_UI.Modules.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OnTheFly_UI.Modules
{
    public class ProcessingModule
    {
        public YoloPredictor Model;
        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000);

        public YoloMetadata Metadata;
        public int BufferLimit = 5;
        public ConcurrentQueue<ProcessObject> PreProcessingBuffer;

        public ConcurrentQueue<ProcessObject> PostProcessingBuffer = new ConcurrentQueue<ProcessObject>();
        public CancellationToken CancellationToken;

        public ProcessingModule(string model, ConcurrentQueue<ProcessObject> ProcessingBuffer) 
        {
            if (!File.Exists(model))
                throw new Exception($"The model, {model}, does not exist.");


            var b = new YoloPredictorOptions()
            {
                Configuration = new YoloConfiguration(),
            };

            b.Configuration.Confidence = 0.1f;

            Model = new YoloPredictor(model,b);
            Metadata = Model.Metadata;

            if(ProcessingBuffer == null)
                throw new Exception("Processing buffer cannot be null");

            PreProcessingBuffer = ProcessingBuffer;
        
        }

        public void SelectModel(string model)
        {
            if (!File.Exists(model))
                throw new Exception($"The model, {model}, does not exist.");
            Model = new YoloPredictor(model);
            Metadata = Model.Metadata;
        }

        bool isThreadAlive = false;
        public void StartProcess()
        {

            if (!isThreadAlive)
            {
                isThreadAlive = true;
                Task.Run(Process).ContinueWith(continuationAction => {
                    isThreadAlive = false; });
            }
        }

        private void Process()
        {
            ProcessObject processObject;
            var sw = new Stopwatch();
            while (true)
            {

                SpinWait.SpinUntil(() => { return PostProcessingBuffer.Count < BufferLimit; });
                sw.Restart();


                if (CancellationToken.IsCancellationRequested)
                    break;

                var ret = PreProcessingBuffer.TryDequeue(out processObject);

                if (!ret)
                {
                    if (!SpinWait.SpinUntil(() => { return PreProcessingBuffer.Count > 0 && !CancellationToken.IsCancellationRequested; }, Timeout))
                        break;
                    else
                        continue;
                }

                processObject.Request.Status = RequestStatus.OnProcessing;

                if (processObject.Result != null)
                {
                    PostProcessingBuffer.Enqueue(processObject);
                    continue;
                }


                YoloResult? result = null;

                switch(Metadata.Task)
                {
                    case YoloTask.Detect:
                        result = Model.Detect(processObject.Frame);
                        break;
                    case YoloTask.Classify:
                        result = Model.Classify(processObject.Frame);
                        break;
                    case YoloTask.Segment:
                        result = Model.Segment(processObject.Frame);
                        break;
                    case YoloTask.Obb:
                        result = Model.DetectObb(processObject.Frame);
                        break;
                    case YoloTask.Pose:
                        result = Model.Pose(processObject.Frame);
                        break;
                    default:
                        break;
                }

                //var result = Model.Detect(processObject.Frame);

                

                if(result == null)
                    return;

                processObject.Request.Result.Add(result);
                processObject.Result = result;
                PostProcessingBuffer.Enqueue(processObject);

                
                Trace.WriteLine($"processing segmentation {sw.ElapsedMilliseconds}");

            }

        }

    }
}
