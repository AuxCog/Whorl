using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public struct PathPointInfo
    {
        public PointF Point { get; }
        public float PathLength { get; }

        public PathPointInfo(PointF point, float pathLength)
        {
            Point = point;
            PathLength = pathLength;
        }
    }
}
