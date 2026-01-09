using Compunet.YoloSharp.Data;
using Emgu.CV;
using Emgu.CV.CvEnum;
using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Enums;
using OnTheFly_UI.Modules.Handlers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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

        public int BufferLimit = 50;
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public DataAcquisitionModule() { }
        bool IsInterrupt = false;

        public void RequestWithID(Guid id) 
        {
            IsInterrupt = true;
            RequestQueue.Clear();
            RequestQueue.Enqueue(id);
            StartReading();
        }

        private void AddRequest(RequestObject request)
        {
            Requests.Add(request);

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

            if (!isThreadAlive)
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

        public void RequestStream(string url)
        {
            var request = new RequestObject(source: url, sourceType: RequestSourceType.Stream);
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
                        ReadStream(requestObject.Source, requestObject); 
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
                requestObject.PreviewImage = BitmapConvertHandler.ToBitmapSourceFast(new Bitmap(requestObject.Source));

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
            int index = 0;
            using (Mat frame = new Mat())
            using (VideoCapture capture = new VideoCapture(path))
            {
                requestObject.FPS = (int)capture.Get(Emgu.CV.CvEnum.CapProp.Fps);

                requestObject.FrameCount = (int)capture.Get(Emgu.CV.CvEnum.CapProp.FrameCount);
                

                while (capture.IsOpened)
                {
                    if (CancellationTokenSource.IsCancellationRequested)
                        return;

                    if (IsInterrupt)
                        break;

                    var ret = SpinWait.SpinUntil(() => { return capture.Read(frame) && !CancellationTokenSource.IsCancellationRequested; }, TimeSpan.FromMilliseconds(2000));

                    if(ret &&  requestObject.PreviewImage == null)
                        requestObject.PreviewImage = BitmapConvertHandler.ToBitmapSourceFast(frame.ToBitmap());

                    if (ret)
                    {
                        if (requestObject.Result.Count > requestObject.VideoPosition)
                            Enqueue(frame, requestObject, requestObject.Result[index],index:index); 
                        else
                            Enqueue(frame, requestObject, index: index);
                        requestObject.VideoPosition = capture.Get(Emgu.CV.CvEnum.CapProp.PosFrames);
                        index++;

                    }
                    else
                        break;

                }
            }
        }


        private void Enqueue(Mat frame,RequestObject requestObject,YoloResult? result = null,int index=0)
        {
            var ppo = new ProcessObject(frame);
            ppo.Request = requestObject;
            ppo.Index = index;
            //if(result != null)
            //    ppo.Result = result;

            //if((requestObject.SourceType == RequestSourceType.Stream) && (PreprocessingBuffer.Count >= BufferLimit))
            //    PreprocessingBuffer.TryDequeue(out var discard);


            SpinWait.SpinUntil(() => { 
                return PreprocessingBuffer.Count < BufferLimit; 
            });
            PreprocessingBuffer.Enqueue(ppo);

        }

        private void ReadStream(string url,RequestObject requestObject)
        {
            DataAcquired?.Invoke();
            var ret = true;
            var index = 0;
            using (Mat frame = new Mat()) {

                var capture = new VideoCapture(url,captureProperties:new Tuple<CapProp, int>(CapProp.ReadTimeoutMsec, 1000));
            
                requestObject.FPS = (int)capture.Get(Emgu.CV.CvEnum.CapProp.Fps);
                
                while (capture.IsOpened && !CancellationTokenSource.IsCancellationRequested) 
                {
                    ret = capture.Grab();
      
                    ret = capture.Retrieve(frame);

                    if (ret && requestObject.PreviewImage == null)
                        requestObject.PreviewImage = BitmapConvertHandler.ToBitmapSourceFast(frame.ToBitmap());

                    
                    SpinWait.SpinUntil(() => { return PreprocessingBuffer.Count < BufferLimit; });

                    if (ret)
                        Enqueue(frame, requestObject,index:index);
                    else
                        break;

                    requestObject.VideoPosition = capture.Get(Emgu.CV.CvEnum.CapProp.PosFrames);
                    index ++;
                }

                capture.Stop();
                capture.Release();

            }

                Trace.WriteLine("frame is finished");


        }

    }

    

}
