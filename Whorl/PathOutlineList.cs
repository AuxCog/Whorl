using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class PathOutlineList: PathOutline
    {
        public class PathInfo
        {
            public PathOutline PathOutline { get; private set; }
            public PathPattern PathPattern { get; private set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public float SortId { get; set; }

            public PathInfo(PathOutline pathOutline, float sortId)
            {
                SortId = sortId;
                SetPathOutline(pathOutline);
            }

            public PathInfo(PathOutline pathOutline, float sortId, WhorlDesign design, Complex zVector, PointF center)
            {
                SortId = sortId;
                PathPattern = new PathPattern(design);
                PathPattern.ZVector = zVector;
                PathPattern.Center = center;
                SetPathOutline(pathOutline);
            }

            public PathInfo(XmlNode node, WhorlDesign design)
            {
                FromXml(node, design);
            }

            public PathInfo(PathInfo source)
            {
                PathOutline = new PathOutline(source.PathOutline);
                if (source.PathPattern != null)
                    PathPattern = new PathPattern(source.PathPattern);
                StartIndex = source.StartIndex;
                EndIndex = source.EndIndex;
            }

            public void SetPathOutline(PathOutline pathOutline)
            {
                if (pathOutline == null)
                    throw new ArgumentNullException("pathOutline cannot be null.");
                pathOutline.ComputePathPoints();
                if (pathOutline.PathPoints == null)
                    throw new Exception("PathPoints is null for pathOutline");
                PathOutline = pathOutline;
                StartIndex = -1;
                EndIndex = -1;
                if (PathPattern != null)
                {
                    if (PathPattern.BasicOutlines.Count == 0)
                        PathPattern.BasicOutlines.Add(pathOutline);
                    else
                        PathPattern.BasicOutlines[0] = pathOutline;
                }
            }

            public int FindClosestIndex(PointF p)
            {
                if (PathPattern == null)
                    throw new NullReferenceException("PathPattern is null.");
                if (!PathPattern.ComputeCurvePoints(PathPattern.ZVector))
                    throw new Exception("ComputeCurvePoints failed.");
                if (PathPattern.CurvePoints.Length != PathOutline.PathPoints.Count())
                    throw new Exception("PathPoints length not equal to CurvePoints");
                return Tools.FindClosestIndex(p, PathPattern.CurvePoints);
            }

            public string Validate()
            {
                int pathLength = PathOutline.PathPoints.Count();
                if (StartIndex < 0 || StartIndex >= pathLength)
                    return "StartIndex is out of bounds.";
                else if (EndIndex < 0 || EndIndex >= pathLength)
                    return "EndIndex is out of bounds.";
                else
                    return null;
            }

            public IEnumerable<PointF> GetPathSection()
            {
                string message = Validate();
                if (message != null)
                    throw new CustomException(message);
                return PathOutline.PathPoints.Skip(StartIndex).Take(EndIndex - StartIndex);
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(PathInfo);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(xmlNode, this, nameof(PathOutline), nameof(PathPattern));
                PathOutline.ToXml(parentNode, xmlTools);
                if (PathPattern != null)
                    PathPattern.ToXml(parentNode, xmlTools);
                return xmlTools.AppendToParent(parentNode, xmlNode);
            }

            private void FromXml(XmlNode node, WhorlDesign design)
            {
                Tools.GetAllXmlAttributes(this, node);
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case nameof(PathOutline):
                            PathOutline = new PathOutline();
                            PathOutline.FromXml(childNode);
                            break;
                        case nameof(PathPattern):
                            PathPattern = (PathPattern)Pattern.CreatePatternFromXml(design, childNode, throwOnError: true);
                            break;
                    }
                }
            }
        }

        private List<PathInfo> pathInfos { get; set; } = new List<PathInfo>();
        public IEnumerable<PathInfo> PathInfos => pathInfos;
        public WhorlDesign Design { get; }
        public Complex PathPatternZVector { get; private set; }
        public PointF PathPatternCenter { get; private set; }

        public PathOutlineList(WhorlDesign design)
        {
            Design = design;
        }

        public PathOutlineList(WhorlDesign design, Size pictureBoxSize)
        {
            Design = design;
            SetPictureBoxSize(pictureBoxSize);
        }

        public void SetPictureBoxSize(Size pictureBoxSize)
        {
            PathPatternCenter = new PointF(0.5F * pictureBoxSize.Width, 0.5F * pictureBoxSize.Height);
            int minExtent = Math.Min(pictureBoxSize.Width, pictureBoxSize.Height);
            PathPatternZVector = new Complex(0.9 * minExtent, 0);
        }

        public PathOutlineList(PathOutlineList source, WhorlDesign design = null)
        {
            Design = design ?? source.Design;
            pathInfos.AddRange(source.PathInfos.Select(p => new PathInfo(p)));
            PathPatternZVector = source.PathPatternZVector;
            PathPatternCenter = source.PathPatternCenter; 
        }

        public override object Clone()
        {
            return new PathOutlineList(this);
        }

        public string Validate()
        {
            return pathInfos.Select(p => p.Validate()).FirstOrDefault(msg => msg != null);
        }

        public override Complex ComputePathPoints()
        {
            var points = pathInfos.SelectMany(pi => pi.GetPathSection());
            SetPathPoints(points);
            return Complex.DefaultZVector;
        }

        public void AddPathOutline(PathOutline pathOutline, bool createPathPattern = true)
        {
            if (pathOutline == null)
                throw new ArgumentNullException("pathOutline cannot be null.");
            if (!pathOutline.UseVertices)
                throw new ArgumentException("Invalid pathOutline.");
            var copy = new PathOutline(pathOutline);
            float sortId;
            if (pathInfos.Count == 0)
                sortId = 1;
            else
                sortId = 1F + pathInfos.Select(p => p.SortId).Max();
            if (createPathPattern)
                pathInfos.Add(new PathInfo(copy, sortId, Design, PathPatternZVector, PathPatternCenter));
            else
                pathInfos.Add(new PathInfo(copy, sortId));
        }

        public void SortPathInfos()
        {
            pathInfos = pathInfos.OrderBy(p => p.SortId).ToList();
        }

        public void RemovePathOutline(int index)
        {
            if (index >= 0 && index < pathInfos.Count)
                pathInfos.RemoveAt(index);
            else
                throw new ArgumentOutOfRangeException("index is out of range.");
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PathOutlineList);
            return base.ToXml(parentNode, xmlTools, xmlNodeName);
        }

        protected override void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.AppendExtraXml(parentNode, xmlTools);
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(PathPatternZVector), PathPatternZVector));
            parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(PathPatternCenter), PathPatternCenter));
            foreach (var pathInfo in pathInfos)
            {
                pathInfo.ToXml(parentNode, xmlTools);
            }
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            switch (node.Name)
            {
                case nameof(PathInfo):
                    pathInfos.Add(new PathInfo(node, Design));
                    break;
                case nameof(PathPatternZVector):
                    PathPatternZVector = Tools.GetComplexFromXml(node);
                    break;
                case nameof(PathPatternCenter):
                    PathPatternCenter = Tools.GetPointFFromXml(node);
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }
    }
}
