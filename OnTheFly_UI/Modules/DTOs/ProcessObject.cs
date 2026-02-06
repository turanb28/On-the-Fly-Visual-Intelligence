using Compunet.YoloSharp.Data;
using Compunet.YoloSharp.Metadata;
using Emgu.CV;
using OnTheFly_UI.Modules.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.DTOs
{
    public sealed class ProcessObject
    {
        public byte[] Frame { get; set; } = [0];
        public int Index { get; set; } = 0;
        public string ModelName { get; set; } = string.Empty;
        public RequestTaskType Task { get => Request.TaskType; }
        public RequestObject Request { get; set; } = new RequestObject();
        public YoloResult? Result { get; set; } = null; 
        public ProcessObject() { }
        public ProcessObject(byte[] frame)
        {
            Frame = frame;
        }
        public ProcessObject(Mat frame)
        {
            Frame = CvInvoke.Imencode(".bmp",frame);
        }
    }
}
