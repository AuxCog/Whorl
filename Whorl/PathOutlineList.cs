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
            //public PathOutline PathOutline { get; private set; }
            public PathPattern PathPattern { get; protected set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public bool Clockwise { get; set; }
            public float SortId { get; set; }
            public Complex OrigZVector { get; protected set; }
            public PointF OrigCenter { get; protected set; }

            protected PathInfo(float sortId)
            {
                SortId = sortId;
            }

            public PathInfo(float sortId, PathPattern pathPattern)
            {
                SortId = sortId;
                PathPattern = pathPattern;
                //SetPathOutline(pathOutline);
            }

            public PathInfo(XmlNode node, WhorlDesign design)
            {
                FromXml(node, design);
            }

            public PathInfo(PathInfo source)
            {
                //SetPathOutline(new PathOutline(source.PathOutline));
                PathPattern = (PathPattern)source.PathPattern.GetCopy();
                Tools.CopyProperties(this, source, excludedPropertyNames:
                                     new string[] { nameof(PathOutline), nameof(PathPattern) });
            }

            public PathInfo(PathOutlineListForForm.PathInfoForForm pathInfoForForm)
            {
                //SetPathOutline(new PathOutline(pathInfoForForm.PathOutline));
                PathPattern = (PathPattern)pathInfoForForm.PathPattern.GetCopy();
                StartIndex = pathInfoForForm.StartIndex;
                EndIndex = pathInfoForForm.EndIndex;
                Clockwise = pathInfoForForm.Clockwise;
                SortId  = pathInfoForForm.SortId;
                OrigCenter = pathInfoForForm.OrigCenter;
                OrigZVector = pathInfoForForm.OrigZVector;
            }

            //public virtual void SetPathOutline(PathOutline pathOutline)
            //{
            //    if (pathOutline == null)
            //        throw new ArgumentNullException("pathOutline cannot be null.");
            //    pathOutline.ComputePathPoints();
            //    if (pathOutline.PathPoints == null)
            //        throw new Exception("PathPoints is null for pathOutline");
            //    PathOutline = pathOutline;
            //    StartIndex = -1;
            //    EndIndex = -1;
            //    if (PathPattern != null)
            //    {
            //        if (PathPattern.BasicOutlines.Count == 0)
            //            PathPattern.BasicOutlines.Add(pathOutline);
            //        else
            //            PathPattern.BasicOutlines[0] = pathOutline;
            //    }
            //}

            public PointF[] ComputePathCurvePoints()
            {
                double modulus = PathPattern.ZVector.GetModulus();
                double angle = PathPattern.ZVector.GetArgument();
                if (PathPattern.SeedPoints == null)
                    PathPattern.ComputeSeedPoints();
                var points = PathPattern.SeedPoints.Select(sp =>
                             Pattern.ComputeCurvePoint(angle + sp.Angle, modulus * sp.Modulus,
                                                       PathPattern.Center)).ToArray();
                PathPattern.TransformCurvePoints(points);
                return points;
            }

            public string Validate()
            {
                if (StartIndex < 0 || EndIndex < 0)
                    return null;
                if (PathPattern.SeedPoints == null)
                    PathPattern.ComputeSeedPoints();
                int pathLength = PathPattern.SeedPoints.Length;
                if (StartIndex >= pathLength)
                    return "StartIndex is out of bounds.";
                else if (EndIndex >= pathLength)
                    return "EndIndex is out of bounds.";
                else
                    return null;
            }

            //public static void ValidatePathOutline(PathOutline pathOutline)
            //{
            //    if (pathOutline == null)
            //        throw new ArgumentNullException("pathOutline cannot be null.");
            //    if (!pathOutline.UseVertices)
            //        throw new ArgumentException("Invalid pathOutline.");
            //}

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(PathInfo);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendXmlAttributesExcept(xmlNode, this, nameof(PathOutline));
                //PathOutline.ToXml(xmlNode, xmlTools);
                PathPattern.ToXml(xmlNode, xmlTools);
                xmlNode.AppendChild(xmlTools.CreateXmlNode(nameof(OrigCenter), OrigCenter));
                xmlNode.AppendChild(xmlTools.CreateXmlNode(nameof(OrigZVector), OrigZVector));
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
                            //PathOutline = new PathOutline();
                            //PathOutline.FromXml(childNode);
                            break;
                        case nameof(OrigCenter):
                            OrigCenter = Tools.GetPointFFromXml(childNode);
                            break;
                        case nameof(OrigZVector):
                            OrigZVector = Tools.GetComplexFromXml(childNode);
                            break;
                        case nameof(PathPattern):
                            PathPattern = (PathPattern)Pattern.CreatePatternFromXml(design, childNode, throwOnError: true);
                            break;
                    }
                }
                //Legacy code:
                if (OrigCenter == PointF.Empty)
                    OrigCenter = PathPattern.Center;
                if (OrigZVector == Complex.Zero)
                    OrigZVector = PathPattern.ZVector;
            }
        }

        private List<PathInfo> pathInfos { get; set; } = new List<PathInfo>();
        public IEnumerable<PathInfo> PathInfos => pathInfos;
        public WhorlDesign Design { get; }
        public override bool NoCustomOutline => true;

        private void Init()
        {
            UseVertices = true;
            DrawType = DrawTypes.Custom;
            HasClosedPath = false;
        }

        public PathOutlineList(WhorlDesign design): base()
        {
            Design = design;
            Init();
        }

        public PathOutlineList(PathOutlineList source, WhorlDesign design = null): base(source)
        {
            Design = design ?? source.Design;
            pathInfos.AddRange(source.PathInfos.Select(p => new PathInfo(p)));
            Init();
        }

        public PathOutlineList(PathOutlineListForForm source, WhorlDesign design = null): base()
        {
            Design = design ?? source.Design;
            pathInfos.AddRange(source.PathInfoForForms.Select(p => new PathInfo(p)));
            Init();
        }

        public override object Clone()
        {
            return new PathOutlineList(this);
        }

        protected string Validate(IEnumerable<PathInfo> infos)
        {
            string errMessage = infos.Select(p => p.Validate()).FirstOrDefault(msg => msg != null);
            if (errMessage == null)
            {
                if (!infos.Any(i => i.StartIndex >= 0 && i.EndIndex >= 0))
                    errMessage = "No valid PathInfos were found.";
            }
            return errMessage;
        }

        public virtual string Validate()
        {
            return Validate(pathInfos);
        }

        public override Complex ComputePathPoints()
        {
            pathPoints = new List<PointF>();
            var infos = pathInfos.Where(i => i.StartIndex >= 0 && i.EndIndex >= 0);
            if (!infos.Any())
                return Complex.DefaultZVector;
            SegmentVerticesCenter = infos.First().PathPattern.Center;
            foreach (PathInfo pathInfo in infos)
            {
                var points = pathInfo.ComputePathCurvePoints();
                if (pathInfo.StartIndex >= points.Length)
                    throw new Exception("StartIndex is out of range.");
                if (pathInfo.EndIndex >= points.Length)
                    throw new Exception("EndIndex is out of range.");
                int ind = pathInfo.StartIndex;
                int endInd = pathInfo.EndIndex;
                if (pathPoints.Count == 0 || pathPoints.Last() != points[ind])
                    pathPoints.Add(points[ind]);
                int increment = pathInfo.Clockwise ? 1 : -1;
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
            return NormalizePathVertices();
        }

        //public virtual void ClearPathInfos()
        //{
        //    pathInfos.Clear();
        //}

        protected float GetNextSortId()
        {
            if (pathInfos.Count == 0)
                return 1;
            else
                return 1F + pathInfos.Select(p => p.SortId).Max();
        }

        //protected void AddPathOutline(PathOutline pathOutline, PathPattern pathPattern)
        //{
        //    PathInfo.ValidatePathOutline(pathOutline);
        //    var copy = new PathOutline(pathOutline);
        //    pathInfos.Add(new PathInfo(GetNextSortId(), copy, pathPattern));
        //}

        public virtual void SortPathInfos()
        {
            pathInfos = pathInfos.OrderBy(p => p.SortId).ToList();
            for (int i = 0; i < pathInfos.Count; i++)
            {
                pathInfos[i].SortId = i + 1;
            }
        }

        //public void RemovePathOutline(int index)
        //{
        //    if (index >= 0 && index < pathInfos.Count)
        //        pathInfos.RemoveAt(index);
        //    else
        //        throw new ArgumentOutOfRangeException("index is out of range.");
        //}

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PathOutlineList);
            return base.ToXml(parentNode, xmlTools, xmlNodeName);
        }

        protected override void AppendExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.AppendExtraXml(parentNode, xmlTools);
            foreach (var pathInfo in pathInfos)
            {
                if (pathInfo.StartIndex >= 0 && pathInfo.EndIndex >= 0)
                {
                    pathInfo.ToXml(parentNode, xmlTools);
                }
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
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }
    }
}
