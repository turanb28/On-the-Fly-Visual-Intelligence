using Compunet.YoloSharp.Data;
using Emgu.CV;
using OnTheFly_UI.Modules.DTOs;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace OnTheFly_UI.Modules
{
    public class DataAcquisitionModule
    {
        public delegate void DataFlowtHandler();
        public TimeSpan Timeout = TimeSpan.FromMilliseconds(2000);

        public DataFlowtHandler? DataAcquired;

        public ConcurrentQueue<ProcessObject> PreprocessingBuffer = new ConcurrentQueue<ProcessObject>();

        public ObservableCollection<RequestObject> Requests = new ObservableCollection<RequestObject>();
        
        private ConcurrentQueue<Guid> RequestQueue = new ConcurrentQueue<Guid>();

        public int BufferLimit = 5;
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public DataAcquisitionModule() { }
        bool IsInterrupt = false;

        public void RequestWithID(Guid id) 
        {
            IsInterrupt = true;
            RequestQueue.Clear();
            RequestQueue.Enqueue(id);
            StartReading();
            DataAcquired?.Invoke();
        }

        private void AddRequest(RequestObject request)
        {
            Requests.Add(request);
            RequestQueue.Enqueue(request.Id);
            request.Status = RequestStatus.OnWaiting;

        }

        private bool NextItem(out RequestObject? requestObject)
        {
            requestObject = null;
            
            Guid id = Guid.Empty;
            
            var ret = RequestQueue.TryDequeue(out id);

            if(!ret)
                return false;

            requestObject = Requests.Where(x => x.Id == id).FirstOrDefault();

            if (requestObject == null) return false;

            return true;
        }


        public void RequestImage(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            var request = new RequestObject(source: path, sourceType: RequestSourceType.Image);


            AddRequest(request);
            
            if(!isThreadAlive)  //////
                StartReading();
        }

        public void RequestImage(string[] paths)
        {
            foreach (var path in paths)
            {
                if (CancellationTokenSource.IsCancellationRequested)
                    return;

               RequestImage(path);
            }

        }

        public void RequestVideo(string path)
        {
            var request = new RequestObject(source: path, sourceType: RequestSourceType.Video);

            AddRequest(request);

            StartReading();
        }


        bool isThreadAlive = false;
        private void StartReading()
        {

            if (!isThreadAlive)
            {
                isThreadAlive = true;
                Task.Run(Reader).ContinueWith(continuationAction =>
                {
                    isThreadAlive = false;
                });
            }
        }

        private void Reader()
        {
            while (true)
            {
                if (CancellationTokenSource.IsCancellationRequested)
                    break;

                
                RequestObject requestObject = new RequestObject();

                var ret = NextItem(out requestObject);

                if (!ret)
                {
                    if (!SpinWait.SpinUntil(() => { return NextItem(out requestObject) && !CancellationTokenSource.IsCancellationRequested; }, Timeout))
                        break;
                }
                requestObject.Status = RequestStatus.OnLoading;
                IsInterrupt = false;
                switch (requestObject.SourceType)
                {
                    case RequestSourceType.Image:
                        ReadImage(requestObject.Source, requestObject);
                        break;

                    case RequestSourceType.Video:
                        ReadVideo(requestObject.Source, requestObject);
                        break;

                    case RequestSourceType.Stream:
                        //ReadUDPStream(requestObject.Source, requestObject); 
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }


        private void ReadImage(string path, RequestObject requestObject)
        {
            using(var frame = CvInvoke.Imread(path)) 
            {
               
                if (requestObject.Result.Count != 0)
                    Enqueue(frame, requestObject, requestObject.Result[0]);
                else
                    Enqueue(frame,requestObject);
            }
            DataAcquired?.Invoke();

        }

        private void ReadVideo(string path, RequestObject requestObject)
        {
            DataAcquired?.Invoke();
            using (Mat frame = new Mat())
            using (VideoCapture capture = new VideoCapture(path))
            {
                requestObject.FPS = (int)capture.Get(Emgu.CV.CvEnum.CapProp.Fps);

                int count = 0;

                while (capture.IsOpened)
                {
                    if (CancellationTokenSource.IsCancellationRequested)
                        return;

                    if (IsInterrupt)
                        break;

                    var ret = SpinWait.SpinUntil(() => { return capture.Read(frame) && !CancellationTokenSource.IsCancellationRequested; }, TimeSpan.FromMilliseconds(1000));

                    if (ret)
                    {
                        if (requestObject.Result.Count > count)
                            Enqueue(frame, requestObject, requestObject.Result[count]);
                        else
                            Enqueue(frame, requestObject);
                    }
                    else
                        break;

                    count++;
                }
            }
        }


        private void Enqueue(Mat frame,RequestObject requestObject,YoloResult? result = null)
        {
            var ppo = new ProcessObject(frame);
            ppo.Request = requestObject;
            if(result != null)
                ppo.Result = result;

            SpinWait.SpinUntil(() => { return PreprocessingBuffer.Count < BufferLimit; });
            
            PreprocessingBuffer.Enqueue(ppo);

        }

        //private void ReadUDPStream(string url)
        //{
        //    Task.Run(() =>
        //    {
        //        DataAcquired?.Invoke();
        //        using (Mat frame = new Mat())
        //        using (VideoCapture capture = new VideoCapture(url,VideoCapture.API.Ffmpeg))
        //        {
        //            while (capture.IsOpened)
        //            {
        //                var ret = SpinWait.SpinUntil(() => { return capture.Read(frame); }, Timeout);

        //                if (ret)
        //                    EncodeAndEnqueue(frame);
        //                else
        //                    break;
        //            }
        //        }
        //    });
        //}


    }
}
