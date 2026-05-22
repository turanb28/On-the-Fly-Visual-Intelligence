using Compunet.YoloSharp.Data;
using Emgu.CV;
using OnTheFly_UI.Modules.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OnTheFly_UI.Modules.DTOs
{
    public sealed class RequestObject: INotifyPropertyChanged
    {
        public delegate void VideoPositionJumpedEventHandler(object sender, double newPosition);
        public event PropertyChangedEventHandler? PropertyChanged;

        // This event is only triggered when the user jumps to a new position in the video, not when the video position is updated by the video player. 
        public event VideoPositionJumpedEventHandler? VideoPositionJumped;
        private BitmapSource? _previewImage = null;
        [JsonIgnore]
        public BitmapSource? PreviewImage
        {
            get { return _previewImage; }
            set
            {
                _previewImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewImage)));
            }
        }

        public List<Mat> Frames { get; set; } = new List<Mat>();

        public List<ResultTable> ResultTables { get; set; } = new List<ResultTable>(); // You dont clear the result tables when you change the task type, 
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Source {  get; set; } = string.Empty;
        public int FPS { get; set; } = int.MaxValue; 
        public int FrameWidth { get; set; } = 0;
        public int FrameHeight { get; set; } = 0;
        public int FrameCount { get; set; } = 1;

        private double _videoPosition = 0;
        public double VideoPosition
        {
            get { return _videoPosition; }
            set { 
                _videoPosition = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoPosition)));
            }
        }

        public RequestTaskType _taskType = RequestTaskType.None;

        //[JsonIgnore]
        public RequestTaskType TaskType { get { return _taskType; } 
                                          set { 
                                                if((Result != null) && (_taskType != value))
                                                    Result.Clear();
                                                _taskType = value;
                                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskType)));

                                                }
                                            }
        public RequestSourceType SourceType {  get; set; } = RequestSourceType.None;
        [JsonConverter(typeof(Handlers.YoloresultJsonConverter))]
        public List<YoloResult> Result { get; set; } = [];
        public RequestObject() { }
        public RequestObject(string source,RequestSourceType sourceType) 
        {
            Source = source;
            SourceType = sourceType;
        }

        public void JumpVideoToPosition(double position)
        {
            VideoPosition = position;
            VideoPositionJumped?.Invoke(this, position);
        }


    }
  
}
