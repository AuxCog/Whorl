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
            One,
            All,
            Result
        }
        private class OutlineControls
        {
            public int Index { get; }
            public PictureBox PictureBox { get; set; }
            public TextBox TxtSortId { get; set; }
            public List<Control> Controls { get; } = new List<Control>();

            public OutlineControls(int index)
            {
                Index = index;
            }

            public void AddControl(Control control)
            {
                Controls.Add(control);
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


        private void FrmPathOutlineList_Load(object sender, EventArgs e)
        {
            try
            {
                cboDisplayMode.DataSource = Enum.GetValues(typeof(DisplayModes));
                cboDisplayMode.SelectedItem = DisplayModes.One;
                PopulatePanel();
                //foreach (Control control in outlineControls.SelectMany(x => x.Controls))
                //{
                //    pnlOutlines.Controls.Add(control);
                //}
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

        public void Initialize(PathOutlineList pathOutlineList, Size designSize)
        {
            try
            {
                this.pathOutlineList = pathOutlineList;
                this.designSize = designSize;
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
                if (selectedRowIndex >= 0)
                    outlineControls[selectedRowIndex].PictureBox.BorderStyle = BorderStyle.None;
                selectedRowIndex = (int)pic.Tag;
                pic.BorderStyle = BorderStyle.Fixed3D;
                if (GetDisplayMode() == DisplayModes.One)
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
                    pathInfo.SetForPreview(picOutline.ClientSize);
                    pathInfo.PathPattern.DrawFilled(e.Graphics, null);
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
                pathInfo.SetForPreview(pic1Outline.ClientSize);
                pic1Outline.Image = GetPatternImage(pathInfo.PathPattern, pic1Outline.ClientSize);
                pic1Outline.Click += Pic1Outline_Click;
                rowOutlineControls.AddControl(pic1Outline);
                rowOutlineControls.PictureBox = pic1Outline;
                pnlOutlines.Controls.Add(pic1Outline);
                left += pictureBoxSize.Width + 5;

                Label lbl = new Label();
                lbl.AutoSize = true;
                lbl.Text = "Sort:";
                lbl.Location = new Point(left, top);
                rowOutlineControls.AddControl(lbl);
                pnlOutlines.Controls.Add(lbl);
                left += 35;

                var txtSort = new TextBox();
                txtSort.Tag = index;
                txtSort.Width = 40;
                txtSort.TextAlign = HorizontalAlignment.Right;
                txtSort.Text = pathInfo.SortId.ToString("0.##");
                txtSort.Location = new Point(left, top);
                rowOutlineControls.AddControl(txtSort);
                rowOutlineControls.TxtSortId = txtSort;
                pnlOutlines.Controls.Add(txtSort);
                left += 45;

                outlineControls.Add(rowOutlineControls);

                top += pictureBoxSize.Height + 5;
                left = 5;
                index++;
            }
        }

    }
}
