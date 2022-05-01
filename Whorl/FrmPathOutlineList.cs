using ParserEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class FrmPathOutlineList : Form
    {
        private enum DisplayModes
        {
            One = 0,
            All = 1,
            Result = 2
        }
        private enum EditModes
        {
            Default,
            Pan
        }
        private class OutlineInfo
        {
            public int Index { get; }
            public PictureBox PictureBox { get; set; }
            public TextBox TxtSortId { get; set; }
            public CheckBox ChkClockwise { get; set; }

            public OutlineInfo(int index)
            {
                Index = index;
            }
        }
        private class PointInfo
        {
            public PointF Point { get; }
            public int PatternIndex { get; }
            public bool IsStartPoint { get; }
            public PathOutlineListForForm.PathDetailForForm PathDetailForForm { get; }

            public PointInfo(PointF point, int index, bool isStartPoint, PathOutlineListForForm.PathDetailForForm pathDetail)
            {
                Point = point;
                PatternIndex = index;
                IsStartPoint = isStartPoint;
                PathDetailForForm = pathDetail;
            }
        }
        private class IntersectionInfo
        {
            public PointF Point { get; }
            public int PointIndex { get; }
            public int PatternIndex { get; }

            public IntersectionInfo(PointF point, int pointIndex, int patternIndex)
            {
                Point = point;
                PointIndex = pointIndex;
                PatternIndex = patternIndex;
            }
        }
        public FrmPathOutlineList()
        {
            InitializeComponent();
        }

        private static readonly Size pictureBoxSize = new Size(75, 75);

        private PathOutlineListForForm pathOutlineListForForm { get; set; }
        private PathOutlineListForForm.PathInfoForForm[] pathInfos { get; set; }
        private List<OutlineInfo> outlineInfos { get; } = new List<OutlineInfo>();
        private List<PointInfo> pointInfos { get; } = new List<PointInfo>();
        //private Size designSize { get; set; }
        private int selectedPathInfoIndex { get; set; } = -1;
        private List<PathOutlineListForForm.PathDetailForForm> allSections { get; set; }
        private PathOutlineListForForm.PathDetailForForm displayedSection { get; set; }
        private float squaresFactor { get; set; } = 1F;
        private PointF squaresOffset { get; set; }
        private Size squaresArraySize { get; set; }
        private List<IntersectionInfo>[,] squaresArray { get; set; }
        private double zoomFactor { get; set; } = 1;
        private Point[] panXYs { get; } = new Point[3];
        private Point startPanXY { get; set; }
        private Point dragStart { get; set; }
        private Point dragEnd { get; set; }
        private bool dragging { get; set; }
        private string zoomText { get; set; }


        private void FrmPathOutlineList_Load(object sender, EventArgs e)
        {
            try
            {
                cboDisplayMode.DataSource = Enum.GetValues(typeof(DisplayModes));
                cboDisplayMode.SelectedItem = DisplayModes.One;
                cboMode.DataSource = Enum.GetValues(typeof(EditModes));
                cboMode.SelectedItem = EditModes.Default;
                PopulatePanel();
                zoomText = txtZoomPercent.Text;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private DisplayModes GetDisplayMode()
        {
            return (DisplayModes)cboDisplayMode.SelectedItem;
        }

        private EditModes GetEditMode()
        {
            return (EditModes)cboMode.SelectedItem;
        }

        public void Initialize(PathOutlineListForForm pathOutlineList)
        {
            try
            {
                if (!pathOutlineList.PathInfoForForms.Any())
                {
                    throw new Exception("No PathInfos were found.");
                }
                pathOutlineListForForm = pathOutlineList;
                pathOutlineListForForm.SortPathInfos();
                pathInfos = pathOutlineList.PathInfoForForms.ToArray();
                allSections = pathInfos.SelectMany(pi => pi.PathDetailsForForm).OrderBy(d => d.SortId).ToList();
                InitializeSections();
                displayedSection = allSections.First();
                for (int i = 0; i < panXYs.Length; i++)
                    panXYs[i] = new Point(0, 0);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void InitializeSections()
        {
            if (allSections.Count == 0)
            {
                var pathInfo = pathInfos[0];
                var firstSection = new PathOutlineListForForm.PathDetailForForm(pathInfo);
                allSections.Add(firstSection);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CreateResultOutline())
                    return;
                DialogResult = DialogResult.OK;
                Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Image GetPatternImage(Pattern pattern, Size size)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            if (pattern.CurvePoints == null)
                pattern.ComputeCurvePoints(pattern.ZVector);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                pattern.DrawFilled(g, null);
            }
            return bitmap;
        }

        private void UpdateInfos()
        {
            for (int i = 0; i < outlineInfos.Count; i++)
            {
                var info = outlineInfos[i];
                pathInfos[i].Clockwise = info.ChkClockwise.Checked;
            }
        }

        //private bool SortInfos()
        //{
        //    bool changed = false;
        //    for (int i = 0; i < outlineInfos.Count; i++)
        //    {
        //        var info = outlineInfos[i];
        //        if (float.TryParse(info.TxtSortId.Text, out float x))
        //        {
        //            if (pathInfos[i].SortId != x)
        //            {
        //                pathInfos[i].SortId = x;
        //                changed = true;
        //            }
        //        }
        //        else
        //        {
        //            throw new CustomException("Please enter a number for Sort.");
        //        }
        //    }
        //    if (changed)
        //    {
        //        pathOutlineListForForm.SortPathInfos();
        //        pathInfos = pathOutlineListForForm.PathInfoForForms.ToArray();
        //        PopulatePanel();
        //    }
        //    return changed;
        //}

        private void BtnSort_Click(object sender, EventArgs e)
        {
            try
            {
                //SortInfos();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void Pic1Outline_Click(object sender, EventArgs e)
        {
            try
            {
                PictureBox pic = (PictureBox)sender;
                int newIndex = (int)pic.Tag;
                if (!SelectPathPattern(newIndex))
                    return;
                //if (newIndex == selectedPathInfoIndex)
                //    return;
                //if (selectedPathInfoIndex >= 0)
                //{
                //    outlineInfos[selectedPathInfoIndex].PictureBox.BorderStyle = BorderStyle.None;
                //}
                //selectedPathInfoIndex = newIndex;
                //pic.BorderStyle = BorderStyle.Fixed3D;
                if (GetDisplayMode() != DisplayModes.Result)
                    picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool SelectPathPattern(int index)
        {
            if (selectedPathInfoIndex == index) return false;
            if (selectedPathInfoIndex >= 0)
            {
                outlineInfos[selectedPathInfoIndex].PictureBox.BorderStyle = BorderStyle.None;
            }
            selectedPathInfoIndex = index;
            outlineInfos[selectedPathInfoIndex].PictureBox.BorderStyle = BorderStyle.Fixed3D;
            return true;
        }

        private void ChkClockwise_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var chkBox = (CheckBox)sender;
                int index = (int)chkBox.Tag;
                pathInfos[index].Clockwise = chkBox.Checked;
                picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void AddIntersectionLinkLabel_Click(object sender, EventArgs e)
        {
            try
            {
                var lnkLabel = (LinkLabel)sender;
                int patternIndex = (int)lnkLabel.Tag;
                AddIntersectingPoint(patternIndex);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void cboDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (GetDisplayMode() == DisplayModes.Result)
                {
                    if (pathOutlineListForForm.ResultPathPattern == null)
                    {
                        if (!CreateResultOutline())
                            return;
                    }
                }
                picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetZoom()
        {
            if (txtZoomPercent.Text == zoomText)
                return;
            if (double.TryParse(txtZoomPercent.Text, out double value))
            {
                zoomFactor = 0.01 * value;
                zoomText = txtZoomPercent.Text;
                picOutline.Refresh();
            }
        }

        private bool CreateResultOutline()
        {
            UpdateInfos();
            for (int i = 0; i < allSections.Count; i++)
            {
                allSections[i].SortId = i + 1;
            }
            foreach (var pathInfo in pathInfos)
            {
                pathInfo.SetPathDetails(allSections.Where(d => d.GetParent() == pathInfo));
            }
            string errMessage = pathOutlineListForForm.Validate();
            if (errMessage != null)
            {
                MessageBox.Show(errMessage);
                return false;
            }
            pathOutlineListForForm.SetResultPathPattern();
            return true;
        }

        private void computeResultOutlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (CreateResultOutline())
                    picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private int GetSquaresXIndex(PointF p)
        {
            return (int)Math.Round(squaresFactor * (p.X - squaresOffset.X));
        }

        private int GetSquaresYIndex(PointF p)
        {
            return (int)Math.Round(squaresFactor * (p.Y - squaresOffset.Y));
        }

        private List<IntersectionInfo> GetSquaresIntersectionInfos(PointF p)
        {
            var intersectionInfos = new List<IntersectionInfo>();
            int xi = GetSquaresXIndex(p);
            int yi = GetSquaresYIndex(p);
            int xiMin = Math.Max(0, xi - 1);
            int yiMin = Math.Max(0, yi - 1);
            int xiMax = Math.Min(squaresArraySize.Width - 1, xi + 1);
            int yiMax = Math.Min(squaresArraySize.Height - 1, yi + 1);
            for (int i = xiMin; i <= xiMax; i++)
            {
                for (int j = yiMin; j <= yiMax; j++)
                {
                    List<IntersectionInfo> infos = squaresArray[i, j];
                    if (infos != null)
                        intersectionInfos.AddRange(infos);
                }
            }
            return intersectionInfos;
        }

        private void CreateSquaresArray(params PathOutlineListForForm.PathInfoForForm[] pathInfoArray)
        {
            List<PointF[]> pointsList = new List<PointF[]>();
            var patternIndices = new List<int>();
            foreach (var pathInfo in pathInfoArray)
            {
                pathInfo.ComputePointsIfNeeded();
                if (pathInfo.FullCurvePoints.Length < 2)
                    throw new Exception("FullCurvePoints length is < 2.");
                pointsList.Add(pathInfo.FullCurvePoints);
                patternIndices.Add(Array.IndexOf(pathInfos, pathInfo));
            }
            double maxSegLen = Math.Sqrt(pointsList.Select(ps => Tools.SegmentLengthsSquared(ps).Max()).Max());
            squaresFactor = (float)(0.1 / maxSegLen);
            RectangleF boundingRect = Tools.GetBoundingRectangleF(pointsList.SelectMany(ps => ps));
            squaresOffset = boundingRect.Location;
            int width = 1 + (int)Math.Ceiling(squaresFactor * boundingRect.Width);
            int height = 1 + (int)Math.Ceiling(squaresFactor * boundingRect.Height);
            squaresArraySize = new Size(width, height);
            squaresArray = new List<IntersectionInfo>[width, height];
            for (int ptnI = 0; ptnI < pointsList.Count; ptnI++)
            {
                PointF[] points = pointsList[ptnI];
                for (int i = 0; i < points.Length; i++)
                {
                    PointF p = points[i];
                    int xi = GetSquaresXIndex(p);
                    int yi = GetSquaresYIndex(p);
                    if (xi < 0 || yi < 0 || xi >= width || yi >= height)
                    {
                        throw new ArgumentOutOfRangeException("Index.");
                    }
                    List<IntersectionInfo> list = squaresArray[xi, yi];
                    if (list == null)
                    {
                        list = new List<IntersectionInfo>();
                        squaresArray[xi, yi] = list;
                    }
                    list.Add(new IntersectionInfo(p, i, patternIndices[ptnI]));
                }
            }
        }

        private void AddIntersectingPoint(int patternIndex)
        {
            var lastSection = allSections.LastOrDefault();
            if (lastSection == null || lastSection.StartIndex < 0)
            {
                MessageBox.Show("Please set the first start point.");
                return;
            }
            var pathInfo = pathInfos[patternIndex];
            var lastPathInfo = (PathOutlineListForForm.PathInfoForForm)lastSection.Parent;
            if (pathInfo == lastPathInfo)
            {
                MessageBox.Show("Please select a different path pattern.");
                return;
            }
            CreateSquaresArray(lastPathInfo, pathInfo);
            PointF[] points = lastPathInfo.FullCurvePoints;
            int increment = lastPathInfo.GetIncrement();
            int i = lastSection.StartIndex;
            float firstDist = 0f;
            PointF firstP = points[i];
            IntersectionInfo intersectionInfo = null;
            while (true)
            {
                i += increment;
                if (i < 0)
                    i = points.Length - 1;
                else if (i >= points.Length)
                    i = 0;
                if (i == lastSection.StartIndex)
                    break;
                PointF p = points[i];
                if (firstDist < 30F)
                {
                    firstDist = Math.Max(firstDist, Tools.DistanceSquared(firstP, p));
                    if (firstDist < 30F)
                        continue;
                }
                var infos = GetSquaresIntersectionInfos(p).Where(ii => ii.PatternIndex == patternIndex);
                if (infos.Any())
                {
                    var infoArray = infos.ToArray();
                    int ind = Tools.FindClosestIndex(p, infoArray.Select(ii => ii.Point).ToArray(), bufferSize: 5F);
                    if (ind >= 0)
                    {
                        intersectionInfo = infoArray[ind];
                        int ind2 = Tools.FindClosestIndex(intersectionInfo.Point, points);
                        if (ind2 >= 0)
                        {
                            lastSection.EndIndex = ind2;
                            break;
                        }
                        else
                            intersectionInfo = null;
                    }
                }
            }
            squaresArray = null;
            if (intersectionInfo != null)
            {
                var newSection = new PathOutlineListForForm.PathDetailForForm(pathInfo, 0);
                newSection.StartIndex = intersectionInfo.PointIndex;
                allSections.Add(newSection);
                SelectPathPattern(patternIndex);
                displayedSection = newSection;
                picOutline.Refresh();
            }
            else
            {
                MessageBox.Show("Didn't find an intersecting point.");
            }
        }

        private void overlayResultOutlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (overlayResultOutlineToolStripMenuItem.Checked)
                {
                    if (pathOutlineListForForm.ResultPathPattern == null)
                    {
                        if (!CreateResultOutline())
                        {
                            overlayResultOutlineToolStripMenuItem.Checked = false;
                            return;
                        }
                    }
                }
                picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtZoomPercent_Leave(object sender, EventArgs e)
        {
            try
            {
                SetZoom();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtZoomPercent_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == '\r')
                    SetZoom();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void UpdateAllCurvePoints()
        {
            foreach (var pathInfo in pathInfos)
            {
                if (!pathInfo.FullPointsAreUpToDate)
                    pathInfo.ComputePoints();
            }
        }

        private PathOutlineListForForm.PathDetailForForm GetClosestSection(PointF p)
        {
            var pointsList = allSections.Select(d => d.GetCurveSection().AsEnumerable()).ToList();
            int index = Tools.FindClosestIndex(p, pointsList, out _);
            return index < 0 ? null : allSections[index];
        }

        //private void SetAllStartEndPoints()
        //{
        //    pointInfos.Clear();
        //    for (int i = 0; i < pathInfos.Length; i++)
        //    {
        //        var pathInfo = pathInfos[i];
        //        foreach (var detail in pathInfo.PathDetailsForForm)
        //        {
        //            pathInfo.GetStartEndPoints(detail, out PointF? startP, out PointF? endP);
        //            if (startP != null)
        //                pointInfos.Add(new PointInfo((PointF)startP, i, isStartPoint: true, detail));
        //            if (endP != null)
        //                pointInfos.Add(new PointInfo((PointF)endP, i, isStartPoint: false, detail));
        //        }
        //    }
        //}

        private void SetNextPointIndex(PointF p, bool isStart, bool lockToPoints = false)
        {
            if (selectedPathInfoIndex < 0 || GetDisplayMode() != DisplayModes.All)
                return;
            var section = allSections.Last();
            if (allSections.Count == 1 && section.EndIndex < 0 && selectedPathInfoIndex != 0)
            {
                MessageBox.Show("Please select the first path pattern.");
                return;
            }
            if (section.EndIndex >= 0)
            {
                if (!AddNewSection())
                    return;
                section = allSections.Last();
            }
            if (section.StartIndex < 0 && allSections.Count > 1)
                throw new Exception("The last section is invalid.");
            var pathInfo = section.GetParent();
            int index = -1;
            //if (lockToPoints)
            //{
            //    SetAllStartEndPoints();
            //    PointF[] joinPoints = pointInfos.Select(pi => pi.Point).ToArray();
            //    int pointInfosIndex = Tools.FindClosestIndex(p, joinPoints);
            //    if (pointInfosIndex >= 0)
            //    {
            //        PointF lockPoint = joinPoints[pointInfosIndex];
            //        index = pathInfo.FindClosestIndex(lockPoint, out PointF p2);
            //    }
            //}
            if (index < 0)
            {
                index = pathInfo.FindClosestIndex(p, out _);
                if (index < 0)
                {
                    MessageBox.Show("Didn't find the point on the path outline.");
                    return;
                }
            }
            if (section.StartIndex < 0)
                section.StartIndex = index;
            else
                section.EndIndex = index;
            picOutline.Refresh();
        }

        private void selectCurveSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var section = GetClosestSection(dragStart);
                if (section == null)
                {
                    MessageBox.Show("Didn't find the closest Curve Section.");
                }
                else
                {
                    displayedSection = section;
                    picOutline.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SelectNextSection(int increment)
        {
            if (displayedSection == null || allSections.Count <= 1)
                return;
            int index = allSections.IndexOf(displayedSection) + increment;
            index = Tools.GetIndexInRange(index, allSections.Count);
            displayedSection = allSections[index];
            picOutline.Refresh();
        }

        private void selectNextSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SelectNextSection(increment: 1);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void selectPreviousSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SelectNextSection(increment: -1);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool AddNewSection()
        {
            string errMessage = _AddNewSection();
            if (errMessage != null)
                MessageBox.Show(errMessage);
            return errMessage == null;
        }

        private string _AddNewSection()
        {
            var section = allSections.Last();
            if (section.EndIndex < 0)
                return "Please set the end point first.";
            var pathInfo = pathInfos[selectedPathInfoIndex];
            if (pathInfo == section.GetParent())
                return "Please select a different path pattern.";
            if (!SetIntersectionIndex(section, pathInfo, out int index2))
                return "No intersecting point was found.";
            var newSection = new PathOutlineListForForm.PathDetailForForm(pathInfo, 0);
            newSection.StartIndex = index2;
            allSections.Add(newSection);
            return null;
        }

        private bool SetIntersectionIndex(PathOutlineListForForm.PathDetailForForm section,
                                          PathOutlineListForForm.PathInfoForForm pathInfo,
                                          out int index2)
        {
            PointF endPoint = section.GetParent().GetCurvePoint(section.EndIndex);
            index2 = pathInfo.FindClosestIndex(endPoint, out float distSquared, bufferSize: 30F);
            bool isValid = index2 >= 0;
            if (isValid && distSquared > 5F)
            {
                int index1 = section.EndIndex;
                distSquared = FindIntersectionIndex(section.GetParent().FullCurvePoints,
                                                    pathInfo.FullCurvePoints,
                                                    ref index1, ref index2);
                isValid = distSquared <= 5F;
                if (isValid)
                    section.EndIndex = index1;
            }
            return isValid;
        }

        private int FindBoundIndex(PointF[] points, int index, int increment, float distSquared)
        {
            int foundIndex = -1;
            PointF p = points[index];
            int startInd = index;
            float curDist = float.MinValue;
            do
            {
                float dist = Tools.DistanceSquared(p, points[index]);
                if (dist > curDist)
                {
                    curDist = dist;
                    foundIndex = index;
                    if (dist >= distSquared)
                        break;
                }
                index = Tools.GetIndexInRange(index + increment, points.Length);
            } while (index != startInd);
            return foundIndex;
        }

        private List<PointF> GetSectionPoints(PointF[] points, int index, float maxSegLen = 0.5F)
        {
            const float maxDistSquared = 100F;
            var sectionPoints = new List<PointF>();
            int iMin = FindBoundIndex(points, index, -1, maxDistSquared);
            int iMax = FindBoundIndex(points, index, 1, maxDistSquared);
            PointF prevP = points[iMin];
            sectionPoints.Add(prevP);
            for (int i = iMin; i <= iMax; i++)
            {
                int ind = Tools.GetIndexInRange(i, points.Length);
                PointF p = points[ind];
                if (Tools.DistanceSquared(prevP, p) >= maxSegLen)
                {
                    sectionPoints.Add(p);
                    prevP = p;
                }
            }
            return sectionPoints;
        }

        private float FindIntersectionIndex(PointF[] curvePoints1,
                                            PointF[] curvePoints2,
                                            ref int ind1, ref int ind2)
        {
            var sect1 = GetSectionPoints(curvePoints1, ind1);
            var sect2 = GetSectionPoints(curvePoints2, ind2);
            int si1 = Tools.FindClosestIndex(sect1, sect2, out float distSq, bufferSize: 5F);
            if (si1 >= 0)
            {
                PointF pt = sect1[si1];
                ind1 = Tools.FindClosestIndex(pt, curvePoints1);
                ind2 = Tools.FindClosestIndex(pt, curvePoints2);
            }
            else
                distSq = float.MaxValue;
            return distSq;
        }

        //private void addCurveSectionToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string errMessage = AddNewSection();
        //        if (errMessage != null)
        //            MessageBox.Show(errMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private void deleteCurveSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var section = allSections.Last();
                allSections.Remove(section);
                if (allSections.Count == 0)
                    InitializeSections();
                if (displayedSection == section)
                {
                    displayedSection = allSections.First();
                    picOutline.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setNextPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetNextPointIndex(dragStart, isStart: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void closeCurvePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var firstSection = allSections.First();
                var section = allSections.Last();
                bool isFirstPattern = section.GetParent() == pathInfos[0];
                if (selectedPathInfoIndex != 0 && !isFirstPattern)
                {
                    MessageBox.Show("Please select the first path pattern.");
                    return;
                }
                if (!isFirstPattern)
                {
                    if (!AddNewSection())
                        return;
                    section = allSections.Last();
                    isFirstPattern = section.GetParent() == pathInfos[0];
                }
                int index2 = firstSection.StartIndex;
                if (isFirstPattern)
                    section.EndIndex = firstSection.StartIndex;
                else
                {
                    PointF pt = firstSection.GetParent().GetCurvePoint(index2);
                    section.EndIndex = section.GetParent().FindClosestIndex(pt, out _);
                    if (section.EndIndex >= 0 &&
                        SetIntersectionIndex(section, firstSection.GetParent(), out index2))
                    {
                        firstSection.StartIndex = index2;
                    }
                    else
                    {
                        MessageBox.Show("Didn't find closing intersection point.");
                        allSections.Remove(section);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void setEndPointToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SetPointIndex(dragStart, isStart: false);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void setLockedStartPointToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SetPointIndex(dragStart, isStart: true, lockToPoints: true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void setLockedEndPointToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SetPointIndex(dragStart, isStart: false, lockToPoints: true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private int GetDisplayIndex()
        {
            return (int)GetDisplayMode();
        }

        private void SetPan(Point p)
        {
            int ind = GetDisplayIndex();
            panXYs[ind] = new Point(startPanXY.X + p.X - dragStart.X, 
                                    startPanXY.Y + p.Y - dragStart.Y);
        }

        private void resetPanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < panXYs.Length; i++)
                    panXYs[i] = new Point(0, 0);
                picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picOutline_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                dragStart = e.Location;
                if (e.Button == MouseButtons.Left)
                    startPanXY = panXYs[GetDisplayIndex()];
                else if (e.Button == MouseButtons.Right)
                {
                    if (GetDisplayMode() == DisplayModes.All)
                        contextMenuStrip1.Show(picOutline, e.Location);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picOutline_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (GetEditMode() == EditModes.Pan)
                    {
                        dragging = true;
                        SetPan(e.Location);
                        picOutline.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picOutline_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                dragEnd = e.Location;
                if (e.Button == MouseButtons.Left)
                {
                    if (dragging && GetEditMode() == EditModes.Pan)
                    {
                        SetPan(e.Location);
                        picOutline.Refresh();
                    }
                }
                dragging = false;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ShowPoint(Graphics g, PathOutlineListForForm.PathInfoForForm pathInfo, int index, 
                               bool isStart, bool isSelected)
        {
            if (index >= 0)
            {
                PointF pt = pathInfo.GetCurvePoint(index);
                Color color;
                if (isSelected)
                    color = isStart ? Color.Blue : Color.LimeGreen;
                else
                    color = Color.Yellow;
                Tools.DrawSquare(g, color, pt, size: isSelected ? 3 : 1);
            }
        }

        private void ShowSection(Graphics g, PathOutlineListForForm.PathDetailForForm section)
        {
            if (section.StartIndex < 0 || section.EndIndex < 0)
                return;
            PointF[] points = section.GetCurveSection().ToArray();
            if (points.Length > 1)
                g.DrawLines(Pens.LightBlue, points);
        }

        private void picOutline_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DisplayModes displayMode = GetDisplayMode();
                if (displayMode == DisplayModes.One)
                {
                    if (selectedPathInfoIndex < 0)
                        return;
                    var pathInfo = pathInfos[selectedPathInfoIndex];
                    pathInfo.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                    pathInfo.PathPattern.DrawFilled(e.Graphics, null);
                }
                else if (displayMode == DisplayModes.All)
                {
                    pathOutlineListForForm.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                    for (int i = 0; i < pathInfos.Length; i++)
                    {
                        var pathInfo = pathInfos[i];
                        Color color = pathInfo.PathPattern.BoundaryColor;
                        if (i == selectedPathInfoIndex)
                            pathInfo.PathPattern.BoundaryColor = Color.Red;
                        else
                            pathInfo.PathPattern.BoundaryColor = Color.Black;
                        pathInfo.PathPattern.DrawFilled(e.Graphics, null);
                        pathInfo.PathPattern.BoundaryColor = color;
                    }
                    PathOutlineListForForm.PathInfoForForm selPathInfo = null;
                    if (selectedPathInfoIndex >= 0)
                    {
                        selPathInfo = pathInfos[selectedPathInfoIndex];
                        foreach (var detail in allSections)
                        {
                            if (detail.GetParent() == selPathInfo)
                            {
                                ShowPoint(e.Graphics, selPathInfo, detail.StartIndex, isStart: true, isSelected: true);
                                ShowPoint(e.Graphics, selPathInfo, detail.EndIndex, isStart: false, isSelected: true);
                            }
                        }
                    }
                    foreach (var detail in allSections)
                    {
                        var pathInfo = detail.GetParent();
                        if (pathInfo != selPathInfo)
                        {
                            ShowPoint(e.Graphics, pathInfo, detail.StartIndex, isStart: true, isSelected: false);
                            ShowPoint(e.Graphics, pathInfo, detail.EndIndex, isStart: false, isSelected: false);
                        }
                    }
                }
                if (displayMode == DisplayModes.Result || overlayResultOutlineToolStripMenuItem.Checked)
                {
                    var pattern = pathOutlineListForForm.ResultPathPattern;
                    if (pattern != null)
                    {
                        if (pattern.ZVector == Complex.Zero)
                        {
                            pathOutlineListForForm.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                        }
                        pattern.DrawFilled(e.Graphics, null);
                    }
                }
                if (displayMode == DisplayModes.All && displayedSection != null)
                {
                    ShowSection(e.Graphics, displayedSection);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PopulatePanel()
        {
            outlineInfos.Clear();
            pnlOutlines.Controls.Clear();
            selectedPathInfoIndex = -1;
            int left = 5, top = 5;
            for (int index = 0; index < pathInfos.Length; index++)
            {
                var pathInfo = pathInfos[index];
                var rowOutlineInfo = new OutlineInfo(index);

                var pic1Outline = new PictureBox();
                pic1Outline.Tag = index;
                pic1Outline.Size = pictureBoxSize;
                pic1Outline.BackColor = Color.White;
                pic1Outline.Location = new Point(left, top);
                pathInfo.SetForPreview(pic1Outline.ClientSize, Point.Empty);
                pic1Outline.Image = GetPatternImage(pathInfo.PathPattern, pic1Outline.ClientSize);
                pic1Outline.Click += Pic1Outline_Click;
                rowOutlineInfo.PictureBox = pic1Outline;
                pnlOutlines.Controls.Add(pic1Outline);
                left += pictureBoxSize.Width + 5;

                Point saveLoc = new Point(left, top);

                //Label lbl = new Label();
                //lbl.AutoSize = true;
                //lbl.Text = "Sort:";
                //lbl.Location = new Point(left, top);
                //pnlOutlines.Controls.Add(lbl);
                //left += 35;

                //var txtSort = new TextBox();
                //txtSort.Tag = index;
                //txtSort.Width = 40;
                //txtSort.TextAlign = HorizontalAlignment.Right;
                //txtSort.Text = pathInfo.SortId.ToString("0.##");
                //txtSort.Location = new Point(left, top);
                //rowOutlineInfo.TxtSortId = txtSort;
                //pnlOutlines.Controls.Add(txtSort);

                var chkClockwise = new CheckBox();
                chkClockwise.Tag = index;
                chkClockwise.Text = "Clockwise";
                chkClockwise.Location = new Point(left, top);
                chkClockwise.Checked = pathInfo.Clockwise;
                chkClockwise.CheckedChanged += ChkClockwise_CheckedChanged;
                rowOutlineInfo.ChkClockwise = chkClockwise;
                pnlOutlines.Controls.Add(chkClockwise);

                top += 20;
                left = saveLoc.X;

                var addIntersectionLinkLabel = new LinkLabel();
                addIntersectionLinkLabel.Tag = index;
                addIntersectionLinkLabel.Text = "Add Intersecting Point";
                addIntersectionLinkLabel.Location = new Point(left, top);
                addIntersectionLinkLabel.Click += AddIntersectionLinkLabel_Click;
                pnlOutlines.Controls.Add(addIntersectionLinkLabel);

                outlineInfos.Add(rowOutlineInfo);

                top = saveLoc.Y + pictureBoxSize.Height + 5;
                left = 5;
            }
        }
    }
}
