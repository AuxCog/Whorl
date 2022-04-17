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
            public RectangleF BoundingRectangle { get; private set; }
            public Complex OrigZVector { get; private set; }
            public PointF OrigCenter { get; private set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public bool Clockwise { get; set; }
            public float SortId { get; set; }
            public PointF[] FullCurvePoints { get; private set; }

            private bool _fullPointsAreUpToDate;
            public bool FullPointsAreUpToDate => FullCurvePoints != null && _fullPointsAreUpToDate;

            public PathInfo(PathOutline pathOutline, float sortId)
            {
                SortId = sortId;
                SetPathOutline(pathOutline);
            }

            public PathInfo(Pattern pattern, PathOutline pathOutline, float sortId, WhorlDesign design)
            {
                OrigZVector = pattern.ZVector;
                OrigCenter = pattern.Center;
                SortId = sortId;
                PathPattern = new PathPattern(design);
                PathPattern.ZVector = pattern.ZVector;
                PathPattern.Center = pattern.Center;
                PathPattern.BoundaryColor = Color.Red;
                SetPathOutline(pathOutline);
                PathPattern.ComputeCurvePoints(PathPattern.ZVector);
                BoundingRectangle = Tools.GetBoundingRectangleF(PathPattern.CurvePoints);
            }

            public PathInfo(XmlNode node, WhorlDesign design)
            {
                FromXml(node, design);
            }

            public PathInfo(PathInfo source)
            {
                Tools.CopyProperties(this, source, excludedPropertyNames:
                                     new string[] { nameof(PathPattern), nameof(PathOutline) });
                PathOutline = new PathOutline(source.PathOutline);
                if (source.PathPattern != null)
                    PathPattern = new PathPattern(source.PathPattern);
            }

            public static void GetPreviewValues(Size picSize, RectangleF boundingRect,
                                                out double scale, out PointF center)
            {
                center = new PointF(0.5F * picSize.Width, 0.5F * picSize.Height);
                double boundsDiag = Math.Sqrt(boundingRect.Width * boundingRect.Width +
                                              boundingRect.Height * boundingRect.Height);
                double picDiag = Math.Sqrt(picSize.Width * picSize.Width + picSize.Height * picSize.Height);
                scale = 0.65 * picDiag / boundsDiag;
            }

            public static PointF GetDeltaCenter(RectangleF boundingRect, PointF origCenter)
            {
                PointF bndCenter = new PointF(0.5F * boundingRect.Width, 0.5F * boundingRect.Height);
                return new PointF(origCenter.X - boundingRect.X - bndCenter.X,
                                  origCenter.Y - boundingRect.Y - bndCenter.Y);
            }

            public void SetForPreview(Size picSize, Point panXY, double zoom = 1)
            {
                GetPreviewValues(picSize, BoundingRectangle, out double scale, out PointF center);
                scale *= zoom;
                SetForPreview(BoundingRectangle, scale, center, panXY);
            }

            public void SetForPreview(RectangleF boundingRect, double scale, PointF center, Point panXY)
            {
                if (PathPattern == null)
                    return;
                PointF dc = GetDeltaCenter(boundingRect, OrigCenter);
                PathPattern.Center = new PointF(center.X + (float)scale * dc.X + panXY.X, 
                                                center.Y + (float)scale * dc.Y + panXY.Y);
                PathPattern.ZVector = scale * OrigZVector;
                _fullPointsAreUpToDate = false;
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

            public void ComputePoints()
            {
                double modulus = PathPattern.ZVector.GetModulus();
                double angle = PathPattern.ZVector.GetArgument();
                var points = PathPattern.SeedPoints.Select(sp => 
                             Pattern.ComputeCurvePoint(angle + sp.Angle, modulus * sp.Modulus, 
                                                       PathPattern.Center)).ToArray();
                PathPattern.TransformCurvePoints(points);
                FullCurvePoints = points;
                _fullPointsAreUpToDate = true;
            }

            public void ClearPoints()
            {
                FullCurvePoints = null;
            }

            public int FindClosestIndex(PointF p, out PointF foundPoint)
            {
                if (!FullPointsAreUpToDate)
                    ComputePoints();
                if (FullCurvePoints.Length != PathOutline.PathPoints.Count())
                    throw new Exception("PathPoints length not equal to SeedPoints");
                int index = Tools.FindClosestIndex(p, FullCurvePoints);
                foundPoint = index >= 0 ? FullCurvePoints[index] : PointF.Empty;
                return index;
            }

            public void GetStartEndPoints(out PointF? startPoint, out PointF? endPoint)
            {
                startPoint = null;
                endPoint = null;
                if (StartIndex >= 0 || EndIndex >= 0)
                {
                    if (StartIndex >= 0)
                        startPoint = GetCurvePoint(StartIndex);
                    if (EndIndex >= 0)
                        endPoint = GetCurvePoint(EndIndex);
                }
            }

            public PointF GetCurvePoint(int index)
            {
                if (!FullPointsAreUpToDate)
                    ComputePoints();
                if (index < 0 || index >= FullCurvePoints.Length)
                    throw new ArgumentOutOfRangeException("index is out of range.");
                return FullCurvePoints[index];
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

            public static void ValidatePathOutline(PathOutline pathOutline)
            {
                if (pathOutline == null)
                    throw new ArgumentNullException("pathOutline cannot be null.");
                if (!pathOutline.UseVertices)
                    throw new ArgumentException("Invalid pathOutline.");
            }

            //public IEnumerable<PointF> GetPathSection()
            //{
            //    string message = Validate();
            //    if (message != null)
            //        throw new CustomException(message);
            //    return PathOutline.PathPoints.Skip(StartIndex).Take(EndIndex - StartIndex);
            //}

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(PathInfo);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(xmlNode, this, 
                         nameof(PathOutline), nameof(PathPattern), 
                         nameof(OrigCenter), nameof(OrigZVector), nameof(BoundingRectangle));
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
        private RectangleF boundingRect { get; set; } = RectangleF.Empty;
        //public Complex PathPatternZVector { get; private set; }
        //public PointF PathPatternCenter { get; private set; }

        public PathOutlineList(WhorlDesign design)
        {
            Design = design;
        }

        //public PathOutlineList(WhorlDesign design, Size pictureBoxSize)
        //{
        //    Design = design;
        //    SetPictureBoxSize(pictureBoxSize);
        //}

        //public void SetPictureBoxSize(Size pictureBoxSize)
        //{
        //    PathPatternCenter = new PointF(0.5F * pictureBoxSize.Width, 0.5F * pictureBoxSize.Height);
        //    int minExtent = Math.Min(pictureBoxSize.Width, pictureBoxSize.Height);
        //    PathPatternZVector = new Complex(0.9 * minExtent, 0);
        //}

        public PathOutlineList(PathOutlineList source, WhorlDesign design = null)
        {
            Design = design ?? source.Design;
            pathInfos.AddRange(source.PathInfos.Select(p => new PathInfo(p)));
            //PathPatternZVector = source.PathPatternZVector;
            //PathPatternCenter = source.PathPatternCenter; 
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
            pathPoints = new List<PointF>();
            foreach (PathInfo pathInfo in pathInfos)
            {
                if (pathInfo.StartIndex < 0 || pathInfo.EndIndex < 0)
                    continue;
                var points = pathInfo.PathOutline.PathPoints.ToArray();
                if (pathInfo.StartIndex >= points.Length)
                    throw new Exception("StartIndex is out of range.");
                if (pathInfo.EndIndex >= points.Length)
                    throw new Exception("EndIndex is out of range.");
                int ind = pathInfo.StartIndex;
                int endInd = pathInfo.EndIndex;
                if (pathPoints.Count == 0 || pathPoints.Last() != points[ind])
                    pathPoints.Add(points[ind]);
                int increment = pathInfo.Clockwise ? -1 : 1;
                do
                {
                    ind += increment;
                    if (ind < 0)
                        ind = points.Length - 1;
                    else if (ind >= points.Length)
                        ind = 0;
                    pathPoints.Add(points[ind]);
                } while (ind != endInd);
            }
            return Complex.DefaultZVector;
        }

        public void ClearPathInfos()
        {
            pathInfos.Clear();
            boundingRect = RectangleF.Empty;
        }

        public static IEnumerable<PathOutline> GetPathOutlines(Pattern pattern)
        {
            return pattern.BasicOutlines.Select(o => o as PathOutline)
                           .Where(o => o != null && o.UseVertices); 
        }

        private float GetNextSortId()
        {
            if (pathInfos.Count == 0)
                return 1;
            else
                return 1F + pathInfos.Select(p => p.SortId).Max();
        }

        public void SetForPreview(Size picSize, Point panXY, double zoom = 1)
        {
            PathInfo.GetPreviewValues(picSize, boundingRect, out double scale, out PointF center);
            scale *= zoom;
            foreach (var pathInfo in pathInfos)
            {
                pathInfo.SetForPreview(boundingRect, scale, center, panXY);
            }
        }

        public PathInfo GetPathInfo(int i)
        {
            return pathInfos[i];
        }

        public void AddPathInfos(IEnumerable<Pattern> patterns)
        {
            foreach (var pattern in patterns)
            {
                PathOutline pathOutline = GetPathOutlines(pattern).FirstOrDefault();
                if (pathOutline == null)
                    throw new ArgumentException("Didn't find valid PathOutline in pattern.");
                var pathInfo = new PathInfo(pattern, pathOutline, GetNextSortId(), Design);
                pathInfos.Add(pathInfo);
                if (boundingRect == RectangleF.Empty)
                    boundingRect = pathInfo.BoundingRectangle;
                else
                    boundingRect = Tools.GetBoundingRectangleF(boundingRect, pathInfo.BoundingRectangle);
            }
        }

        public void AddPathOutline(PathOutline pathOutline)
        {
            PathInfo.ValidatePathOutline(pathOutline);
            var copy = new PathOutline(pathOutline);
            pathInfos.Add(new PathInfo(copy, GetNextSortId()));
        }

        public void AddPathOutlines(IEnumerable<PathOutline> pathOutlines)
        {
            foreach (var pathOutline in pathOutlines)
            {
                AddPathOutline(pathOutline);
            }
        }

        public void SortPathInfos()
        {
            pathInfos = pathInfos.OrderBy(p => p.SortId).ToList();
            for (int i = 0; i < pathInfos.Count; i++)
            {
                pathInfos[i].SortId = i + 1;
            }
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
            //parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(PathPatternZVector), PathPatternZVector));
            //parentNode.AppendChild(xmlTools.CreateXmlNode(nameof(PathPatternCenter), PathPatternCenter));
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
                //case nameof(PathPatternZVector):
                //    PathPatternZVector = Tools.GetComplexFromXml(node);
                //    break;
                //case nameof(PathPatternCenter):
                //    PathPatternCenter = Tools.GetPointFFromXml(node);
                //    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }
    }
}
