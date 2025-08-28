using Compunet.YoloSharp.Data;
using OnTheFly_UI.Modules.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.DTOs
{
    public sealed class RequestObject
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Source {  get; set; } = string.Empty;
        public int FPS { get; set; } = int.MaxValue;
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
