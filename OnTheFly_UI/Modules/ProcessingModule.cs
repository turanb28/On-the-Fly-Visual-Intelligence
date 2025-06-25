using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using Emgu.CV.Dnn;
using OnTheFly_UI.Modules.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public YoloPredictor Model = null;
        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000);
        public YoloMetadata Metadata;
        public int BufferLimit = 5;
        public CancellationToken CancellationToken;

        public ObservableCollection<ModelObject> Models { get; set; } = new ObservableCollection<ModelObject>();
  
        public ConcurrentQueue<ProcessObject> PreProcessingBuffer;

        public ConcurrentQueue<ProcessObject> PostProcessingBuffer = new ConcurrentQueue<ProcessObject>();

        #region Events
        public delegate void ModelLoadedHandler();
        public ModelLoadedHandler? ModelLoaded;

        public delegate void ProcessingExceptionHandler(string? message);
        public ProcessingExceptionHandler? ProcessingException;

        #endregion

        public ProcessingModule( ConcurrentQueue<ProcessObject> ProcessingBuffer)
        {
           
            if (ProcessingBuffer == null)
                throw new Exception("Processing buffer cannot be null");

            PreProcessingBuffer = ProcessingBuffer;
           
           
        }

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

            

            if(ProcessingBuffer == null) // make it first
                throw new Exception("Processing buffer cannot be null");

            PreProcessingBuffer = ProcessingBuffer;
        
        }

        public void AddModel(string model)
        {
            if (Models.Where(x => x.Path == model).Count() > 0)
            {
                ProcessingException?.Invoke("The model is already added.");
                return;
            }

            var modelObject = new ModelObject()
            {
                Name = Path.GetFileName(model),
                Path = model,
            };
            Models.Add(modelObject);
        }
        
        public ModelObject? GetModel(string modelPath)
        {
            return Models.Where(x => x.Path == modelPath).FirstOrDefault();
        }

        public void SelectModel(string modelPath)
        {
            if (Models.Where(x => x.Path == modelPath).Count() < 0)
            {
                ProcessingException?.Invoke("The model is not loaded.");
                return;
            }

            Task.Run(() =>
            {
                Model = new YoloPredictor(modelPath);
                Metadata = Model.Metadata;
                ModelLoaded?.Invoke();
                foreach(var item in Models)
                {
                    if (item.Path == modelPath)
                       item.IsSelected = true;
                    else
                        item.IsSelected = false;
                }
            });
           
        }

        public void UnselectModel(string modelPath)
        {
            if (Models.Where(x => x.Path == modelPath).Count() > 0)
            {
                var model = Models.Where(x => x.Path == modelPath).First();
                model.IsSelected = false;
                Model = null;
            }
        } // Fix this part. The unselected model runs after adding new model


        bool isThreadAlive = false;
        public void StartProcess()
        {
            if(Model == null)
                ProcessingException?.Invoke("The model is not loaded.");

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
                    if (!SpinWait.SpinUntil(() => { return PreProcessingBuffer.Count > 0 && !CancellationToken.IsCancellationRequested; }, Timeout)) // Tp keep the thread alive while there is no data
                        break;
                    else
                        continue;
                }

                processObject.Request.Status = RequestStatus.OnProcessing;

                //if (processObject.Result != null)
                //{
                //    PostProcessingBuffer.Enqueue(processObject);
                //    continue;
                //}


                YoloResult? result = null;

                if(Model == null)
                {
                    PostProcessingBuffer.Enqueue(processObject);
                    continue;
                }

                switch (Metadata.Task)
                {
                    case YoloTask.Detect:
                        result = Model.Detect(processObject.Frame);
                        if (result == null)
                            return;
                        processObject.Request.ResultTable = ((YoloResult<Detection>)result).GroupBy(x => x.Name.Name).ToDictionary(x => x.Key, x => x.Count());

                        break;
                    case YoloTask.Classify:
                        if (result == null)
                            return;
                        result = Model.Classify(processObject.Frame);
                        processObject.Request.ResultTable = ((YoloResult<Classification>)result).GroupBy(x => x.Name.Name).ToDictionary(x => x.Key, x => x.Count());
                        break;
                    case YoloTask.Segment:
                        if (result == null)
                            return;
                        result = Model.Segment(processObject.Frame);
                        processObject.Request.ResultTable = ((YoloResult<Segmentation>)result).GroupBy(x => x.Name.Name).ToDictionary(x => x.Key, x => x.Count());

                        break;
                    case YoloTask.Obb:
                        if (result == null)
                            return;
                        result = Model.DetectObb(processObject.Frame);
                        processObject.Request.ResultTable = ((YoloResult<ObbDetection>)result).GroupBy(x => x.Name.Name).ToDictionary(x => x.Key, x => x.Count());
                        break;
                    case YoloTask.Pose:
                        if (result == null)
                            return;
                        result = Model.Pose(processObject.Frame);
                        processObject.Request.ResultTable = ((YoloResult<Pose>)result).GroupBy(x => x.Name.Name).ToDictionary(x => x.Key, x => x.Count());
                        break;
                    default:
                        return; // Unsupported task
                }

                //var result = Model.Detect(processObject.Frame);

                processObject.Task = Metadata.Task; // Think of making it again

                processObject.Request.Result.Add(result);
                processObject.Result = result;
                PostProcessingBuffer.Enqueue(processObject);

                
                Trace.WriteLine($"processing segmentation {sw.ElapsedMilliseconds}");

            }

        }

    }
}
