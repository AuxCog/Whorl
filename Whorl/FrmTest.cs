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
    public partial class FrmTest : Form
    {
        public FrmTest()
        {
            InitializeComponent();
        }

        private SelectPatternForm selectPatternForm { get; set; }
        private Pattern pattern { get; set; }
        private PathPadding pathPadding { get; } = new PathPadding();
        private List<List<PointF>> paddingPoints { get; set; }

        public void Initialize(SelectPatternForm selPatternForm)
        {
            selectPatternForm = selPatternForm;
        }

        private void BtnSelectPattern_Click(object sender, EventArgs e)
        {
            try
            {
                selectPatternForm.Initialize();
                if (selectPatternForm.ShowDialog() == DialogResult.OK)
                {
                    var ptn = selectPatternForm.SelectedPatternGroup.Patterns.FirstOrDefault();
                    if (ptn != null)
                    {
                        pattern = ptn.GetCopy();
                        pattern.ZVector = new Complex(0.4 * picPattern.ClientSize.Width, 0);
                        pattern.Center = new PointF(picPattern.ClientSize.Width / 2, picPattern.ClientSize.Height / 2);
                        picPattern.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnDrawPadding_Click(object sender, EventArgs e)
        {
            try
            {
                if (pattern == null) return;
                if (!double.TryParse(txtMinAngle.Text, out double minAngle))
                    minAngle = 3;
                if (!float.TryParse(txtPadding.Text, out float padding))
                    padding = 5F;
                double sign = chkClockwise.Checked ? -1 : 1;
                if (pattern.CurvePoints == null)
                    pattern.ComputeCurvePoints(pattern.ZVector);
                pathPadding.MinAngle = Tools.DegreesToRadians(minAngle);
                pathPadding.Padding = padding;
                pathPadding.TransformPath = chkTransformPath.Checked;
                paddingPoints = pathPadding.ComputePath(pattern.CurvePoints, sign);
                //paddingPoints = path.ToArray();
                picPattern.Refresh();
                //txtMessages.Text = "Computed angles: " + String.Join(", ", 
                //                    pathPadding.AngleInfos.Select(o => Tools.RadiansToDegrees(o.Angle).ToString("0.#")));
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DrawLines(Graphics g,  PointF[] points, Color color)
        {
            using (Pen pen = new Pen(color))
            {
                g.DrawLines(pen, points);
            }
        }

        private void DisplayPattern(Graphics g)
        {
            if (pattern != null && pattern.ComputeCurvePoints(pattern.ZVector))
            {
                DrawLines(g, pattern.CurvePoints, Color.Green);
            }
        }

        private void DisplayPadding(Graphics g)
        {
            if (paddingPoints != null)
            {
                foreach (List<PointF> points in paddingPoints)
                {
                    if (points.Count > 1)
                    {
                        DrawLines(g, points.ToArray(), Color.Red);
                        //foreach (PointF point in points)
                        //{
                        //    Tools.DrawSquare(g, Color.Red, point, size: 1);
                        //}
                        if (paddingPoints.Count > 1)
                        {
                            Tools.DrawSquare(g, Color.Yellow, points.First(), size: 2);
                        }
                    }
                }
                foreach (PointF p in pathPadding.Corners)
                {
                    Tools.DrawSquare(g, Color.Navy, p, size: 1);
                }
                //for (int i = 0; i < pathPadding.AngleInfos.Count; i++)
                //{
                //    var angleInfo = pathPadding.AngleInfos[i];
                //    if (angleInfo.StartIndex >= 0)
                //    {
                //        DisplayPoint(g, angleInfo.StartIndex, Color.Blue, (i + 1).ToString());
                //    }
                //    if (angleInfo.EndIndex >= 0)
                //    {
                //        DisplayPoint(g, angleInfo.EndIndex, Color.Yellow);
                //    }
                //}
            }
        }

        private void DisplayPoint(Graphics g, int index, Color color, string label = null)
        {
            PointF p = pattern.CurvePoints[index];
            Tools.DrawSquare(g, color, p, size: 2);
            if (label != null)
            {
                g.DrawString(label, Font, Brushes.Black, new PointF(p.X + 10, p.Y));
            }
        }

        private void DisplayDistSquaresPoints(Graphics g, Color color)
        {
            if (viewDistanceSquaresPointsToolStripMenuItem.Checked && pathPadding.distanceSquares != null)
            {
                foreach (PointF p in pathPadding.distanceSquares.SelectMany(x => x.Points))
                {
                    Tools.DrawSquare(g, color, p, size: 1);
                }
                if (pathPadding.points != null && pathPadding.points.Length > 1)
                    g.DrawLines(Pens.Orange, pathPadding.points);
            }
        }

        private void picPattern_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DisplayPattern(e.Graphics);
                DisplayPadding(e.Graphics);
                DisplayDistSquaresPoints(e.Graphics, Color.LightBlue);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void viewDistanceSquaresPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picPattern.Refresh();
        }
    }
}
