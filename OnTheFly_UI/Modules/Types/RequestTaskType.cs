using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.Enums
{
    public enum RequestTaskType
    {
        None = -1,
        Obb,
        Detect,
        Segment,
        Pose,
        Classify
    }
}
