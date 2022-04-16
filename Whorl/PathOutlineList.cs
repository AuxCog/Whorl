using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PathOutlineList
    {
        public class PathInfo
        {
            public PathOutline PathOutline { get; private set; }
            public int StartIndex { get; private set; } = -1;
            public int EndIndex { get; private set; } = -1;

            public void SetPathOutline(PathOutline pathOutline)
            {
                PathOutline = pathOutline;
                StartIndex = -1;
                EndIndex = -1;
            }

        }
        private List<PathInfo> pathInfos { get; } = new List<PathInfo>();
        public IEnumerable<PathInfo> PathInfos => pathInfos;

        public void AddPathOutline(PathOutline pathOutline)
        {
            if (!(pathOutline.UseVertices && pathOutline.VerticesSettings.IsValid))
                throw new ArgumentException("Invalid pathOutline.");
            var pathInfo = new PathInfo();
            pathInfo.SetPathOutline(pathOutline);
            pathInfos.Add(pathInfo);
        }

        public bool RemovePathOutline(int index)
        {
            if (index >= 0 && index < pathInfos.Count)
            {
                pathInfos.RemoveAt(index);
                return true;
            }
            else
                return false;
        }
    }
}
