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
        public byte[] Frame { get; set; } = null;
        public int Index { get; set; } = 0;
        public string ModelName { get; set; } = string.Empty;
        public RequestTaskType Task { get => Request.TaskType; }// Think making it a new class to obtain more information about the task
        public RequestObject Request { get; set; }
        public YoloResult Result { get; set; } = null; // make it a new class to obtain more information about the result and add them processing modeule.
        //public List<ResultTableItem> ResultTable { get; set; } = new List<ResultTableItem>();   
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
