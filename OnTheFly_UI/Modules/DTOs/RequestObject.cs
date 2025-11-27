using Compunet.YoloSharp.Data;
using OnTheFly_UI.Modules.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Modules.DTOs
{
    public sealed class RequestObject: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        private BitmapSource? _previewImage = null;

        public BitmapSource? PreviewImage
        {
            get { return _previewImage; }
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewImage)));
                _previewImage = value;
            }
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Source {  get; set; } = string.Empty;
        public int FPS { get; set; } = int.MaxValue; 
        public int FrameCount { get; set; } = 1;

        private double _videoPosition = 0;

        public double VideoPosition
        {
            get { return _videoPosition; }
            set { 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoPosition)));
                _videoPosition = value;
            }
        }

        public RequestStatus Status { get; set; } = RequestStatus.Failed; 
        private RequestTaskType _taskType = RequestTaskType.None;


        public RequestTaskType TaskType { get { return _taskType; } 
                                          set { 
                                                if((Result != null) && (_taskType != value))
                                                    Result.Clear();
                                                _taskType = value; } 
                                         }
        public RequestSourceType SourceType {  get; set; } = RequestSourceType.None;
        public List<YoloResult> Result { get; set; } = new  List<YoloResult>(); // Think of making it Span in case of peak memory while adding new items
        public RequestObject() { }
        public RequestObject(string source,RequestSourceType sourceType) 
        {
            Source = source;
            SourceType = sourceType;
        }
    }
    public enum RequestStatus
    {
        Failed = -1,
        OnWaiting,
        OnLoading,
        OnProcessing,
        OnRendering, 
        Sucess
    }
}
