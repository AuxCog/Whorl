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
        private class OutlineControls
        {
            public int Index { get; }
            public PathPattern PathPattern { get; }
            public PictureBox PictureBox { get; }
            public TextBox TxtSortId { get; }

            public OutlineControls(int index, PathPattern pathPattern, PictureBox pictureBox, TextBox txtSortId)
            {
                Index = index;
                PathPattern = pathPattern;
                PictureBox = pictureBox;
                TxtSortId = txtSortId;
            }
        }
        public FrmPathOutlineList()
        {
            InitializeComponent();
        }
        public readonly Size picSize = new Size(75, 75);

        private PathOutlineList pathOutlineList { get; set; }
        private List<OutlineControls> outlineControls { get; } = new List<OutlineControls>();

        public void Initialize(PathOutlineList pathOutlineList)
        {
            try
            {
                this.pathOutlineList = pathOutlineList;
                PopulatePanel();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Image GetPathPatternImage(PathPattern pathPattern, Size size)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            if (pathPattern.CurvePoints == null)
                pathPattern.ComputeCurvePoints(pathPattern.ZVector);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                pathPattern.DrawFilled(g, null);
            }
            return bitmap;
        }

        private void PopulatePanel()
        {
            outlineControls.Clear();
            pnlOutlines.Controls.Clear();
            int index = 0, left = 5, top = 5;
            foreach (var pathInfo in pathOutlineList.PathInfos)
            {
                if (pathInfo.PathPattern == null)
                    continue;

                var pic = new PictureBox();
                pic.Tag = index;
                pic.Size = picSize;
                pic.BackColor = Color.White;
                pic.Location = new Point(left, top);
                pic.Image = GetPathPatternImage(pathInfo.PathPattern, pic.ClientSize);
                pnlOutlines.Controls.Add(pic);
                left += picSize.Width + 5;

                Label lbl = new Label();
                lbl.Text = "Sort:";
                lbl.Location = new Point(left, top);
                pnlOutlines.Controls.Add(lbl);
                left += 50;

                var txtSort = new TextBox();
                txtSort.Tag = index;
                txtSort.Text = pathInfo.SortId.ToString("0.##");
                txtSort.Width = 60;
                txtSort.Location = new Point(left, top);
                pnlOutlines.Controls.Add(txtSort);
                left += txtSort.Width + 5;

                outlineControls.Add(new OutlineControls(index, pathInfo.PathPattern, pic, txtSort));

                top += picSize.Height + 5;
                index++;
            }
        }
    }
}
