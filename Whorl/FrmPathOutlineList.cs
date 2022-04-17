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
            public int Index { get; }
            public bool IsStartPoint { get; }

            public PointInfo(PointF point, int index, bool isStartPoint)
            {
                Point = point;
                Index = index;
                IsStartPoint = isStartPoint;
            }
        }
        public FrmPathOutlineList()
        {
            InitializeComponent();
        }

        private static readonly Size pictureBoxSize = new Size(75, 75);

        private PathOutlineList pathOutlineList { get; set; }
        private PathOutlineList.PathInfo[] pathInfos { get; set; }
        private List<OutlineInfo> outlineInfos { get; } = new List<OutlineInfo>();
        private List<PointInfo> pointInfos { get; } = new List<PointInfo>();
        private Size designSize { get; set; }
        private int selectedRowIndex { get; set; } = -1;
        private double zoomFactor { get; set; } = 1;
        private Point[] panXYs { get; } = new Point[3];
        private Point startPanXY { get; set; }
        private Point dragStart { get; set; }
        private Point dragEnd { get; set; }
        private bool dragging { get; set; }

        private void FrmPathOutlineList_Load(object sender, EventArgs e)
        {
            try
            {
                cboDisplayMode.DataSource = Enum.GetValues(typeof(DisplayModes));
                cboDisplayMode.SelectedItem = DisplayModes.One;
                cboMode.DataSource = Enum.GetValues(typeof(EditModes));
                cboMode.SelectedItem = EditModes.Default;
                PopulatePanel();
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

        public void Initialize(PathOutlineList pathOutlineList, Size designSize)
        {
            try
            {
                this.pathOutlineList = pathOutlineList;
                pathInfos = pathOutlineList.PathInfos.Where(p => p.PathPattern != null).ToArray();
                this.designSize = designSize;
                for (int i = 0; i < panXYs.Length; i++)
                    panXYs[i] = new Point(0, 0);
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

        private void BtnSort_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < outlineInfos.Count; i++)
                {
                    var info = outlineInfos[i];
                    if (float.TryParse(info.TxtSortId.Text, out float x))
                    {
                        pathInfos[i].SortId = x;
                    }
                    else
                    {
                        MessageBox.Show("Please enter a number for Sort.");
                        return;
                    }
                }
                pathOutlineList.SortPathInfos();
                pathInfos = pathOutlineList.PathInfos.Where(p => p.PathPattern != null).ToArray();
                PopulatePanel();
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
                if (newIndex == selectedRowIndex)
                    return;
                if (selectedRowIndex >= 0)
                {
                    outlineInfos[selectedRowIndex].PictureBox.BorderStyle = BorderStyle.None;
                }
                selectedRowIndex = newIndex;
                pic.BorderStyle = BorderStyle.Fixed3D;
                if (GetDisplayMode() != DisplayModes.Result)
                    picOutline.Refresh();
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
                picOutline.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtZoomPercent_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (double.TryParse(txtZoomPercent.Text, out double value))
                    {
                        double fac = 0.01 * value;
                        if (zoomFactor != fac)
                        {
                            zoomFactor = fac;
                            picOutline.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetAllStartEndPoints()
        {
            pointInfos.Clear();
            for (int i = 0; i < pathInfos.Length; i++)
            {
                var pathInfo = pathInfos[i];
                pathInfo.GetStartEndPoints(out PointF? startP, out PointF? endP);
                if (startP != null)
                    pointInfos.Add(new PointInfo((PointF)startP, i, isStartPoint: true));
                if (endP != null)
                    pointInfos.Add(new PointInfo((PointF)endP, i, isStartPoint: false));
            }
        }

        private void SetPointIndex(PointF p, bool isStart, bool lockToPoints = false)
        {
            if (selectedRowIndex < 0 || GetDisplayMode() != DisplayModes.All)
                return;
            var pathInfo = pathInfos[selectedRowIndex];
            int index = -1;
            PointF foundPoint = PointF.Empty;
            if (lockToPoints)
            {
                SetAllStartEndPoints();
                PointF[] joinPoints = pointInfos.Select(pi => pi.Point).ToArray();
                index = Tools.FindClosestIndex(p, joinPoints);
                if (index >= 0)
                {
                    PointF lockPoint = joinPoints[index];
                    index = pathInfo.FindClosestIndex(lockPoint, out PointF p2);
                    if (index >= 0)
                        foundPoint = p2;
                }
            }
            if (index < 0)
                index = pathInfo.FindClosestIndex(p, out foundPoint);
            if (index < 0)
            {
                MessageBox.Show("Didn't find the point on the path outline.");
                return;
            }
            if (isStart)
            {
                pathInfo.StartIndex = index;
            }
            else
            {
                pathInfo.EndIndex = index;
            }
            picOutline.Refresh();
        }

        private void setStartPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetPointIndex(dragStart, isStart: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setEndPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetPointIndex(dragStart, isStart: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setLockedStartPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetPointIndex(dragStart, isStart: true, lockToPoints: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setLockedEndPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetPointIndex(dragStart, isStart: false, lockToPoints: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


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

        private void ShowPoint(Graphics g, PathOutlineList.PathInfo pathInfo, int index, 
                               bool isStart, bool isSelected)
        {
            if (index >= 0)
            {
                PointF pt = pathInfo.GetCurvePoint(index);
                Tools.DrawSquare(g, isStart ? Color.Blue : Color.LimeGreen, pt, size: isSelected ? 3 : 1);
            }
        }

        private void picOutline_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DisplayModes displayMode = GetDisplayMode();
                if (displayMode == DisplayModes.One)
                {
                    if (selectedRowIndex < 0)
                        return;
                    var pathInfo = pathInfos[selectedRowIndex];
                    pathInfo.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                    pathInfo.PathPattern.DrawFilled(e.Graphics, null);
                }
                else if (displayMode == DisplayModes.All)
                {
                    pathOutlineList.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                    for (int i = 0; i < pathInfos.Length; i++)
                    {
                        var pathInfo = pathInfos[i];
                        Color color = pathInfo.PathPattern.BoundaryColor;
                        if (i == selectedRowIndex)
                            pathInfo.PathPattern.BoundaryColor = Color.Red;
                        else
                            pathInfo.PathPattern.BoundaryColor = Color.Black;
                        pathInfo.PathPattern.DrawFilled(e.Graphics, null);
                        pathInfo.PathPattern.BoundaryColor = color;
                    }
                    if (selectedRowIndex >= 0)
                    {
                        var selPathInfo = pathInfos[selectedRowIndex];
                        ShowPoint(e.Graphics, selPathInfo, selPathInfo.StartIndex, isStart: true, isSelected: true);
                        ShowPoint(e.Graphics, selPathInfo, selPathInfo.EndIndex, isStart: false, isSelected: true);
                    }
                    for (int i = 0; i < pathInfos.Length; i++)
                    {
                        if (i != selectedRowIndex)
                        {
                            var pathInfo = pathInfos[i];
                            ShowPoint(e.Graphics, pathInfo, pathInfo.StartIndex, isStart: true, isSelected: false);
                            ShowPoint(e.Graphics, pathInfo, pathInfo.EndIndex, isStart: false, isSelected: false);
                        }
                    }
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
            selectedRowIndex = -1;
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

                Label lbl = new Label();
                lbl.AutoSize = true;
                lbl.Text = "Sort:";
                lbl.Location = new Point(left, top);
                pnlOutlines.Controls.Add(lbl);
                left += 35;

                var txtSort = new TextBox();
                txtSort.Tag = index;
                txtSort.Width = 40;
                txtSort.TextAlign = HorizontalAlignment.Right;
                txtSort.Text = pathInfo.SortId.ToString("0.##");
                txtSort.Location = new Point(left, top);
                rowOutlineInfo.TxtSortId = txtSort;
                pnlOutlines.Controls.Add(txtSort);

                top += 20;
                left = saveLoc.X;
                var chkClockwise = new CheckBox();
                chkClockwise.Text = "Clockwise";
                chkClockwise.Location = new Point(left, top);
                chkClockwise.Checked = pathInfo.Clockwise;
                rowOutlineInfo.ChkClockwise = chkClockwise;
                pnlOutlines.Controls.Add(chkClockwise);

                outlineInfos.Add(rowOutlineInfo);

                top = saveLoc.Y + pictureBoxSize.Height + 5;
                left = 5;
            }
        }

    }
}
