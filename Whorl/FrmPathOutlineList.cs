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
        private class OutlineControls
        {
            public int Index { get; }
            public PictureBox PictureBox { get; set; }
            public TextBox TxtSortId { get; set; }
            public CheckBox ChkClockwise { get; set; }

            public OutlineControls(int index)
            {
                Index = index;
            }
        }
        public FrmPathOutlineList()
        {
            InitializeComponent();
        }

        private static readonly Size pictureBoxSize = new Size(75, 75);

        private PathOutlineList pathOutlineList { get; set; }
        private List<OutlineControls> outlineControls { get; } = new List<OutlineControls>();
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
                    outlineControls[selectedRowIndex].PictureBox.BorderStyle = BorderStyle.None;
                    var prevPathInfo = pathOutlineList.GetPathInfo(selectedRowIndex);
                    prevPathInfo.ClearPoints();
                }
                selectedRowIndex = newIndex;
                pic.BorderStyle = BorderStyle.Fixed3D;
                var pathInfo = pathOutlineList.GetPathInfo(selectedRowIndex);
                pathInfo.ComputePoints();
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

        private void SetPointIndex(PointF p, bool isStart)
        {
            if (selectedRowIndex < 0 || GetDisplayMode() != DisplayModes.All)
                return;
            var pathInfo = pathOutlineList.GetPathInfo(selectedRowIndex);
            int index = pathInfo.FindClosestIndex(p, out PointF foundPoint);
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

        private void ShowPoint(Graphics g, PathOutlineList.PathInfo pathInfo, int index, bool isStart)
        {
            if (index >= 0)
            {
                PointF pt = pathInfo.GetCurvePoint(index);
                Tools.DrawSquare(g, isStart ? Color.Blue : Color.LimeGreen, pt);
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
                    var pathInfo = pathOutlineList.GetPathInfo(selectedRowIndex);
                    pathInfo.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                    pathInfo.PathPattern.DrawFilled(e.Graphics, null);
                }
                else if (displayMode == DisplayModes.All)
                {
                    pathOutlineList.SetForPreview(picOutline.ClientSize, panXYs[GetDisplayIndex()], zoomFactor);
                    int i = 0;
                    foreach (var pathInfo in pathOutlineList.PathInfos)
                    {
                        Color color = pathInfo.PathPattern.BoundaryColor;
                        if (i == selectedRowIndex)
                            pathInfo.PathPattern.BoundaryColor = Color.Red;
                        else
                            pathInfo.PathPattern.BoundaryColor = Color.Black;
                        pathInfo.PathPattern.DrawFilled(e.Graphics, null);
                        pathInfo.PathPattern.BoundaryColor = color;
                        i++;
                    }
                    if (selectedRowIndex >= 0)
                    {
                        var pathInfo = pathOutlineList.GetPathInfo(selectedRowIndex);
                        ShowPoint(e.Graphics, pathInfo, pathInfo.StartIndex, isStart: true);
                        ShowPoint(e.Graphics, pathInfo, pathInfo.EndIndex, isStart: false);
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
            outlineControls.Clear();
            pnlOutlines.Controls.Clear();
            selectedRowIndex = -1;
            int index = 0, left = 5, top = 5;
            foreach (var pathInfo in pathOutlineList.PathInfos)
            {
                if (pathInfo.PathPattern == null)
                    continue;
                var rowOutlineControls = new OutlineControls(index);

                var pic1Outline = new PictureBox();
                pic1Outline.Tag = index;
                pic1Outline.Size = pictureBoxSize;
                pic1Outline.BackColor = Color.White;
                pic1Outline.Location = new Point(left, top);
                pathInfo.SetForPreview(pic1Outline.ClientSize, Point.Empty);
                pic1Outline.Image = GetPatternImage(pathInfo.PathPattern, pic1Outline.ClientSize);
                pic1Outline.Click += Pic1Outline_Click;
                rowOutlineControls.PictureBox = pic1Outline;
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
                rowOutlineControls.TxtSortId = txtSort;
                pnlOutlines.Controls.Add(txtSort);

                top += 20;
                left = saveLoc.X;
                var chkClockwise = new CheckBox();
                chkClockwise.Text = "Clockwise";
                chkClockwise.Location = new Point(left, top);
                chkClockwise.Checked = pathInfo.Clockwise;
                rowOutlineControls.ChkClockwise = chkClockwise;
                pnlOutlines.Controls.Add(chkClockwise);

                outlineControls.Add(rowOutlineControls);

                top = saveLoc.Y + pictureBoxSize.Height + 5;
                left = 5;
                index++;
            }
        }

    }
}
