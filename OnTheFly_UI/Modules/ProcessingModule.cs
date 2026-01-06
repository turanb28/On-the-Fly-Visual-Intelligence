using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using Emgu.CV.Dnn;
using OnTheFly_UI.Components;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Enums;
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
        public TimeSpan Timeout = TimeSpan.FromMilliseconds(20000);
        public YoloMetadata Metadata;
        public int BufferLimit = 5;
        public CancellationToken CancellationToken;
        public RequestTaskType TaskType { 
                get {
                    if (Metadata != null)
                        return YoloTasKToRequestTask(Metadata.Task);
                    else
                        return RequestTaskType.None;
                    }
            }
        public List<string> Names { get; set; } = new List<string>();
        public ObservableCollection<ModelObject> Models { get; set; } = new ObservableCollection<ModelObject>();
  
        public ConcurrentQueue<ProcessObject> PreProcessingBuffer;

        public ConcurrentQueue<ProcessObject> PostProcessingBuffer = new ConcurrentQueue<ProcessObject>();

        #region Events
        public delegate void ModelLoadedHandler(string modelName = "");
        public ModelLoadedHandler? ModelLoaded;

        public delegate void ModelUnloadedHandler(string modelName = "");
        public ModelUnloadedHandler? ModelUnloaded;

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

            b.Configuration.Confidence = 0.01f;

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
                var modelname = Path.GetFileName(modelPath);
                Metadata = Model.Metadata;
                Names.Clear();

                foreach (var yoloname in Model.Metadata.Names)
                    Names.Add(yoloname.Name);

                ModelLoaded?.Invoke(modelname);
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
                ModelUnloaded?.Invoke(model.Name);
            }
        } 


        bool isThreadAlive = false;
        public void StartProcess()
        {
            //if(Model == null)
            //{
            //    return;
            //}

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
            var lastWarnedId = Guid.Empty;

            while (true)
            {

                SpinWait.SpinUntil(() => { return PostProcessingBuffer.Count < BufferLimit; });


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
                sw.Restart();




                if (((processObject.Result != null)) && (processObject.Request.TaskType == TaskType))
                {
                    PostProcessingBuffer.Enqueue(processObject);
                    continue;

                }




                processObject.Request.Status = RequestStatus.OnProcessing;
                processObject.Request.TaskType = TaskType; // Set the task type for the request
                //Trace.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString());

                YoloResult? result = null;


                if (Model == null)
                {
                    if ( processObject.Request.Id != lastWarnedId )
                        ProcessingException?.Invoke("The model is not loaded.");
                    PostProcessingBuffer.Enqueue(processObject);
                    lastWarnedId = processObject.Request.Id;
                    continue;
                }

                lastWarnedId = Guid.Empty;

                var newDict = new List<ResultTableItem>();

                var tempDict = new Dictionary<string, int>();



                foreach (var name in Names)
                    tempDict[name] = 0;

                switch (TaskType)
                {
                    case RequestTaskType.Detect:

                        result = Model.Detect(processObject.Frame);
                        if (result == null)
                            return;

                        ((YoloResult<Detection>)result).GroupBy(x => x.Name.Name).ToList().ForEach(x => { tempDict[x.Key] = x.Count(); });
                        break;
                    case RequestTaskType.Classify:
                        result = Model.Classify(processObject.Frame);
                        if (result == null)
                            return;


                        ((YoloResult<Classification>)result).ToList().ForEach(x => { tempDict[x.Name.Name] = (int)(x.Confidence * 100); });
                      
                        break;
                    case RequestTaskType.Segment:
                        result = Model.Segment(processObject.Frame);
                        if (result == null)
                            return;
                        ((YoloResult<Segmentation>)result).GroupBy(x => x.Name.Name).ToList().ForEach(x => { tempDict[x.Key] = x.Count(); });
                            break;
                    case RequestTaskType.Obb:
                        result = Model.DetectObb(processObject.Frame);
                        if (result == null)
                            return;
                        ((YoloResult<ObbDetection>)result).GroupBy(x => x.Name.Name).ToList().ForEach(x => { tempDict[x.Key] = x.Count(); });
                            break;
                    case RequestTaskType.Pose:
                        result = Model.Pose(processObject.Frame); 
                        if (result == null)
                            return;
                        ((YoloResult<Pose>)result).GroupBy(x => x.Name.Name).ToList().ForEach(x => { tempDict[x.Key] = x.Count(); });
                            break;
                    default:
                        UIMessageBox.Show("Unsupported Task",UIMessageBox.InformationType.Error);
                        return; 

                }


                foreach (var x in tempDict.Keys)
                    newDict.Add(new ResultTableItem(x, tempDict[x]));

                processObject.Request.ResultTables.Add(newDict);

                //processObject.Request.Result.Add(result); 


                processObject.Result = result;
                PostProcessingBuffer.Enqueue(processObject);


            }

        }


        private static RequestTaskType YoloTasKToRequestTask(YoloTask task)
        {
            switch (task)
            {
                case YoloTask.Detect:
                    return RequestTaskType.Detect;
                case YoloTask.Classify:
                    return RequestTaskType.Classify;
                case YoloTask.Segment:
                    return RequestTaskType.Segment;
                case YoloTask.Obb:
                    return RequestTaskType.Obb;
                case YoloTask.Pose:
                    return RequestTaskType.Pose;
                default:
                    return RequestTaskType.None;
            }
        }
    }
}
