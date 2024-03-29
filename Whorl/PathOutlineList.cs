﻿using ParserEngine;
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
        public class PathDetail: IXml
        {
            public PathInfo Parent { get; }
            public int StartIndex { get; set; } = -1;
            public int EndIndex { get; set; } = -1;
            public float SortId { get; set; }
            public PointF StartPoint { get; set; }
            public PointF EndPoint { get; set; }

            public PathDetail(PathInfo pathInfo, float sortId)
            {
                Parent = pathInfo;
                SortId = sortId;
            }

            public PathDetail(PathInfo pathInfo, XmlNode xmlNode)
            {
                Parent = pathInfo;
                FromXml(xmlNode);
            }

            public PathDetail(PathInfo pathInfo, PathDetail source)
            {
                Parent = pathInfo;
                StartIndex = source.StartIndex;
                EndIndex = source.EndIndex;
                SortId = source.SortId;
            }

            public string Validate(int pathLength)
            {
                if (StartIndex < 0 || EndIndex < 0)
                    return null;
                if (StartIndex >= pathLength)
                    return "StartIndex is out of bounds.";
                else if (EndIndex >= pathLength)
                    return "EndIndex is out of bounds.";
                else
                    return null;
            }

            public List<PointF> GetCurveSection()
            {
                var section = new List<PointF>();
                AppendCurveSection(section);
                return section;
            }

            public bool AppendCurveSection(List<PointF> points)
            {
                if (StartIndex < 0 || EndIndex < 0)
                    return false;
                if (!Parent.FullPointsAreUpToDate)
                    Parent.ComputePoints();
                if (StartIndex >= Parent.FullCurvePoints.Length)
                    throw new Exception("StartIndex is out of range.");
                if (EndIndex >= Parent.FullCurvePoints.Length)
                    throw new Exception("EndIndex is out of range.");
                int ind = StartIndex;
                int increment = Parent.GetIncrement();
                while (ind != EndIndex)
                {
                    points.Add(Parent.FullCurvePoints[ind]);
                    ind += increment;
                    if (ind == 0)
                        ind = Parent.FullCurvePoints.Length - 1;
                    else if (ind == Parent.FullCurvePoints.Length - 1)
                        ind = 0;
                }
                return true;
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(PathDetail);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendAllXmlAttributes(xmlNode, this);
                return xmlTools.AppendToParent(parentNode, xmlNode);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetAllXmlAttributes(this, node);
            }
        }
        public class PathInfo
        {
            //public PathOutline PathOutline { get; private set; }
            public PathPattern PathPattern { get; protected set; }
            public bool Clockwise { get; set; } = true;
            public Complex OrigZVector { get; protected set; }
            public PointF OrigCenter { get; protected set; }
            private List<PathDetail> pathDetails { get; } = new List<PathDetail>();
            public IEnumerable<PathDetail> PathDetails => pathDetails;
            public PointF[] FullCurvePoints { get; protected set; }
            protected bool _fullPointsAreUpToDate { get; set; }
            public bool FullPointsAreUpToDate => FullCurvePoints != null && _fullPointsAreUpToDate;

            //Legacy properties:
            public int StartIndex { get; set; } = -1;
            public int EndIndex { get; set; } = -1;
            public float SortId { get; set; }

            protected PathInfo()
            {
            }

            public PathInfo(PathPattern pathPattern)
            {
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
                                     new string[] { nameof(PathPattern) });
                pathDetails.AddRange(source.pathDetails.Select(d => new PathDetail(this, d)));
            }

            public PathInfo(PathOutlineListForForm.PathInfoForForm pathInfoForForm)
            {
                //SetPathOutline(new PathOutline(pathInfoForForm.PathOutline));
                PathPattern = (PathPattern)pathInfoForForm.PathPattern.GetCopy();
                pathDetails.AddRange(pathInfoForForm.PathDetailsForForm.Select(d => new PathDetail(this, d)));
                Clockwise = pathInfoForForm.Clockwise;
                OrigCenter = pathInfoForForm.OrigCenter;
                OrigZVector = pathInfoForForm.OrigZVector;
            }

            public virtual PathDetail AddDetail(float sortId)
            {
                var detail = new PathDetail(this, sortId);
                pathDetails.Add(detail);
                return detail;
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

            public int GetIncrement()
            {
                return Clockwise ? 1 : -1;
            }

            public void ComputePointsIfNeeded()
            {
                if (!FullPointsAreUpToDate)
                    ComputePoints();
            }

            public void ComputePoints()
            {
                FullCurvePoints = ComputePathCurvePoints();
                _fullPointsAreUpToDate = true;
            }

            public void ClearPoints()
            {
                FullCurvePoints = null;
            }

            private PointF[] ComputePathCurvePoints()
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
                if (PathPattern.SeedPoints == null)
                    PathPattern.ComputeSeedPoints();
                int pathLength = PathPattern.SeedPoints.Length;
                return pathDetails.Select(d => d.Validate(pathLength)).FirstOrDefault(s => s != null);
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
                xmlTools.AppendXmlAttributesExcept(xmlNode, this, nameof(PathPattern),
                                                   nameof(SortId), nameof(StartIndex), nameof(EndIndex));
                foreach (PathDetail pathDetail in pathDetails)
                {
                    if (pathDetail.StartIndex >= 0 && pathDetail.EndIndex >= 0)
                    {
                        pathDetail.ToXml(xmlNode, xmlTools);
                    }
                }
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
                        case nameof(PathDetail):
                            pathDetails.Add(new PathDetail(this, childNode));
                            break;
                        case nameof(PathOutline):
                            //Legacy case.
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
                if (node.Attributes[nameof(SortId)] != null)
                {
                    var detail = AddDetail(SortId);
                    detail.StartIndex = StartIndex;
                    detail.EndIndex = EndIndex;
                }
                if (OrigCenter == PointF.Empty)
                    OrigCenter = PathPattern.Center;
                if (OrigZVector == Complex.Zero)
                    OrigZVector = PathPattern.ZVector;
            }
        }

        private List<PathInfo> pathInfos { get; set; } = new List<PathInfo>();
        public IEnumerable<PathInfo> PathInfos => pathInfos;
        public WhorlDesign Design { get; }
        public bool InitializedFromForm { get; private set; }
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
            Tools.CopyProperties(this, source);
            pathInfos.AddRange(source.PathInfoForForms.Select(p => new PathInfo(p)));
            Init();
            InitializedFromForm = true;
        }

        public override object Clone()
        {
            return new PathOutlineList(this);
        }

        public List<PathDetail> GetOrderedPathDetails()
        {
            if (InitializedFromForm)
            {
                return pathInfos.SelectMany(p => p.PathDetails).OrderBy(d => d.SortId).ToList();
            }
            var details = new List<PathDetail>();
            var allDetails = pathInfos.SelectMany(p => p.PathDetails)
                             .Where(d => d.StartIndex >= 0 && d.EndIndex >= 0).ToList();
            if (!allDetails.Any())
                return details;
            foreach (var detail in allDetails)
            {
                PathInfo pathInfo = detail.Parent;
                if (pathInfo.FullCurvePoints == null)
                    pathInfo.ComputePoints();
                var points = pathInfo.FullCurvePoints;
                if (detail.StartIndex >= points.Length)
                    throw new Exception("StartIndex is out of range.");
                if (detail.EndIndex >= points.Length)
                    throw new Exception("EndIndex is out of range.");
                detail.StartPoint = points[detail.StartIndex];
                detail.EndPoint = points[detail.EndIndex];
            }
            PathDetail currDetail = allDetails.First();
            while (true)
            {
                details.Add(currDetail);
                allDetails.Remove(currDetail);
                if (allDetails.Count == 0)
                    break;
                int nextInd = Tools.FindClosestIndex(currDetail.EndPoint, 
                                                     allDetails.Select(d => d.StartPoint).ToArray(),
                                                     out float distSquared,
                                                     bufferSize: 1F);
                if (nextInd == -1)
                    break;
                currDetail = allDetails[nextInd];
            }
            return details;
        }

        public override Complex ComputePathPoints()
        {
            pathPoints = new List<PointF>();
            var details = GetOrderedPathDetails();
            if (!details.Any())
                return Complex.DefaultZVector;
            SegmentVerticesCenter = details.First().Parent.PathPattern.Center;
            foreach (PathDetail detail in details)
            {
                detail.AppendCurveSection(pathPoints);
                //PathInfo pathInfo = detail.Parent;
                //if (pathInfo.FullCurvePoints == null)
                //    pathInfo.ComputePoints();
                //var points = pathInfo.FullCurvePoints;
                //int ind = detail.StartIndex;
                //int endInd = detail.EndIndex;
                //if (pathPoints.Count == 0 || pathPoints.Last() != points[ind])
                //    pathPoints.Add(points[ind]);
                //int increment = pathInfo.Clockwise ? 1 : -1;
                //do
                //{
                //    ind += increment;
                //    if (ind < 0)
                //        ind = points.Length - 1;
                //    else if (ind >= points.Length)
                //        ind = 0;
                //    pathPoints.Add(points[ind]);
                //} while (ind != endInd);
            }
            return NormalizePathVertices();
        }

        private string Validate(IEnumerable<PathInfo> infos)
        {
            string errMessage = infos.Select(p => p.Validate()).FirstOrDefault(msg => msg != null);
            if (errMessage == null)
            {
                if (!infos.Any(i => i.PathDetails.Any(d => d.StartIndex >= 0 && d.EndIndex >= 0)))
                    errMessage = "No valid PathInfos were found.";
            }
            return errMessage;
        }

        public virtual string Validate()
        {
            return Validate(pathInfos);
        }

        //public virtual void ClearPathInfos()
        //{
        //    pathInfos.Clear();
        //}

        //protected virtual float GetNextSortId()
        //{
        //    if (pathInfos.Count == 0)
        //        return 1;
        //    else
        //        return 1F + pathInfos.Select(p => p.SortId).Max();
        //}

        //protected void AddPathOutline(PathOutline pathOutline, PathPattern pathPattern)
        //{
        //    PathInfo.ValidatePathOutline(pathOutline);
        //    var copy = new PathOutline(pathOutline);
        //    pathInfos.Add(new PathInfo(GetNextSortId(), copy, pathPattern));
        //}

        //public virtual void SortPathInfos()
        //{
        //    pathInfos = pathInfos.OrderBy(p => p.SortId).ToList();
        //    for (int i = 0; i < pathInfos.Count; i++)
        //    {
        //        pathInfos[i].SortId = i + 1;
        //    }
        //}

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
                if (pathInfo.PathDetails.Any(d => d.StartIndex >= 0 && d.EndIndex >= 0))
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
