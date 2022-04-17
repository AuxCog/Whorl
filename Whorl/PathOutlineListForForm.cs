using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PathOutlineListForForm: PathOutlineList
    {
        public class PathInfoForForm: PathInfo
        {
            public RectangleF BoundingRectangle { get; private set; }
            public PointF[] FullCurvePoints { get; private set; }

            private bool _fullPointsAreUpToDate;
            public bool FullPointsAreUpToDate => FullCurvePoints != null && _fullPointsAreUpToDate;

            public PathInfoForForm(Pattern pattern, float sortId, WhorlDesign design) :
                   base(sortId)
            {
                OrigZVector = pattern.ZVector;
                OrigCenter = pattern.Center;
                PathPattern = new PathPattern(pattern, design: design);
                if (PathPattern.FillInfo.FillType != FillInfo.FillTypes.Path)
                {
                    PathPattern.FillInfo = new PathFillInfo(PathPattern);
                }
                //PathPattern.ZVector = pattern.ZVector;
                //PathPattern.Center = pattern.Center;
                PathPattern.BoundaryColor = PathPattern.CenterColor = Color.Red;
                //SetPathOutline(pathOutline);
                PathPattern.ComputeCurvePoints(PathPattern.ZVector);
                BoundingRectangle = Tools.GetBoundingRectangleF(PathPattern.CurvePoints);
            }

            public PathInfoForForm(PathOutlineList.PathInfo pathInfo) : base(pathInfo.SortId)
            {
                OrigCenter = pathInfo.OrigCenter;
                OrigZVector = pathInfo.OrigZVector;
                PathPattern = new PathPattern(pathInfo.PathPattern);
                PathPattern.Center = OrigCenter;
                PathPattern.ZVector = OrigZVector;
                //SetPathOutline(pathInfo.PathOutline);
                PathPattern.ComputeCurvePoints(PathPattern.ZVector);
                BoundingRectangle = Tools.GetBoundingRectangleF(PathPattern.CurvePoints);
                StartIndex = pathInfo.StartIndex;
                EndIndex = pathInfo.EndIndex;
                Clockwise = pathInfo.Clockwise;
            }

            //public override void SetPathOutline(PathOutline pathOutline)
            //{
            //    base.SetPathOutline(pathOutline);
            //    _fullPointsAreUpToDate = false;
            //}

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
                SetForPreview(PathPattern, OrigCenter, OrigZVector, boundingRect, scale, center, panXY);
                _fullPointsAreUpToDate = false;
            }

            public static void SetForPreview(PathPattern pathPattern, PointF origCenter, Complex origZVector,
                               RectangleF boundingRect, double scale, PointF center, Point panXY)
            {
                PointF dc = GetDeltaCenter(boundingRect, origCenter);
                pathPattern.Center = new PointF(center.X + (float)scale * dc.X + panXY.X,
                                                center.Y + (float)scale * dc.Y + panXY.Y);
                pathPattern.ZVector = scale * origZVector;
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

            public int FindClosestIndex(PointF p, out PointF foundPoint)
            {
                if (!FullPointsAreUpToDate)
                    ComputePoints();
                //if (FullCurvePoints.Length != PathOutline.PathPoints.Count())
                //    throw new Exception("PathPoints length not equal to SeedPoints");
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
        }

        private RectangleF boundingRect { get; set; } = RectangleF.Empty;
        private List<PathInfoForForm> pathInfoForForms { get; set; } = new List<PathInfoForForm>();
        public IEnumerable<PathInfoForForm> PathInfoForForms => pathInfoForForms;
        public PathPattern ResultPathPattern { get; private set; }

        public PathOutlineListForForm(WhorlDesign design): base(design)
        {
        }

        public PathOutlineListForForm(PathOutlineList pathOutlineList, WhorlDesign design = null):
               base(design ?? pathOutlineList.Design)
        {
            bool isFirst = true;
            foreach (var pathInfo in pathOutlineList.PathInfos)
            {
                var pathInfoForForm = new PathInfoForForm(pathInfo);
                pathInfoForForms.Add(pathInfoForForm);
                if (isFirst)
                {
                    isFirst = false;
                    boundingRect = pathInfoForForm.BoundingRectangle;
                }
                else
                    boundingRect = Tools.GetBoundingRectangleF(boundingRect, pathInfoForForm.BoundingRectangle);
            }
        }

        public void ClearSettings()
        {
            pathInfoForForms.Clear();
            ResultPathPattern = null;
            boundingRect = RectangleF.Empty;
        }

        public override void SortPathInfos()
        {
            pathInfoForForms = pathInfoForForms.OrderBy(i => i.SortId).ToList();
        }

        //public static IEnumerable<PathOutline> GetPathOutlines(Pattern pattern)
        //{
        //    return pattern.BasicOutlines.Select(o => o as PathOutline)
        //                   .Where(o => o != null && o.UseVertices);
        //}

        public void SetForPreview(Size picSize, Point panXY, double zoom = 1)
        {
            PathInfoForForm.GetPreviewValues(picSize, boundingRect, out double scale, out PointF center);
            scale *= zoom;
            foreach (var pathInfo in pathInfoForForms)
            {
                pathInfo.SetForPreview(boundingRect, scale, center, panXY);
            }
            if (ResultPathPattern != null && pathInfoForForms.Exists(p => p.StartIndex >= 0 && p.EndIndex >= 0))
            {
                SetResultForPreview(picSize, panXY, zoom);
            }
        }

        public void SetResultForPreview(Size picSize, Point panXY, double zoom = 1)
        {
            if (ResultPathPattern == null)
                throw new InvalidOperationException("ResultPathPattern is null.");
            var pathInfo1 = pathInfoForForms.Find(p => p.StartIndex >= 0 && p.EndIndex >= 0);
            if (pathInfo1 == null)
                throw new InvalidOperationException("No valid PathInfo found.");
            PathInfoForForm.GetPreviewValues(picSize, boundingRect, out double scale, out PointF center);
            scale *= zoom;
            PathInfoForForm.SetForPreview(ResultPathPattern, pathInfo1.OrigCenter,
                            pathInfo1.OrigZVector, boundingRect, scale, center, panXY);
        }

        public void SetResultForDesign()
        {
            if (ResultPathPattern == null)
                throw new InvalidOperationException("ResultPathPattern is null.");
            var pathInfo1 = pathInfoForForms.Find(p => p.StartIndex >= 0 && p.EndIndex >= 0);
            if (pathInfo1 == null)
                throw new InvalidOperationException("No valid PathInfo found.");
            ResultPathPattern.Center = pathInfo1.OrigCenter;
            ResultPathPattern.ZVector = pathInfo1.OrigZVector;
        }

        public void AddPathInfos(IEnumerable<Pattern> patterns)
        {
            foreach (var pattern in patterns)
            {
                //PathOutline pathOutline = GetPathOutlines(pattern).FirstOrDefault();
                //if (pathOutline == null)
                //    throw new ArgumentException("Didn't find valid PathOutline in pattern.");
                var pathInfo = new PathInfoForForm(pattern, GetNextSortId(), Design);
                pathInfoForForms.Add(pathInfo);
                if (boundingRect == RectangleF.Empty)
                    boundingRect = pathInfo.BoundingRectangle;
                else
                    boundingRect = Tools.GetBoundingRectangleF(boundingRect, pathInfo.BoundingRectangle);
            }
        }

        public override string Validate()
        {
            return Validate(pathInfoForForms);
        }

        public void SetResultPathPattern()
        {
            string errMessage = Validate();
            if (errMessage != null)
                throw new Exception(errMessage);
            var pathOutlineList = new PathOutlineList(this);
            ResultPathPattern = new PathPattern(Design);
            ResultPathPattern.BasicOutlines.Add(pathOutlineList);
            ResultPathPattern.BoundaryColor = Color.Blue;
        }
    }
}
