using Compunet.YoloSharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.DTOs
{
    public class ResultDTO // This is a class to an alternative to the YoloResult<T> class with additional properties for the UI
    {
        public string ModelName { get; set; } = string.Empty;
        public ResultType Type { get; set; } = ResultType.Undefined;

        public YoloResult YoloResult { get; set; } // This is the result of the YoloSharp processing, it can be OBB, Detection, Segmentetion, Pose or Classification


    }


    public enum ResultType 
    {
        Undefined = -1,
        OBB,
        Detection,
        Segmentetion,
        Pose,
        Classification
    }
}
