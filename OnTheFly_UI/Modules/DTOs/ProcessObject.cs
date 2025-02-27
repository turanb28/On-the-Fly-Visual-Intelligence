using Compunet.YoloSharp.Data;
using Emgu.CV;
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
        public RequestObject Request { get; set; }
        public YoloResult Result { get; set; } = null;
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
