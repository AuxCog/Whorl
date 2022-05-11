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
        private PointF[] paddingPoints { get; set; }

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
                double sign = chkClockwise.Checked ? 1 : -1;
                if (pattern.CurvePoints == null)
                    pattern.ComputeCurvePoints(pattern.ZVector);
                pathPadding.MinAngle = Tools.DegreesToRadians(minAngle);
                pathPadding.Padding = padding;
                var path = pathPadding.ComputePath(pattern.CurvePoints, sign);
                paddingPoints = path.ToArray();
                picPattern.Refresh();
                txtMessages.Text = $"Computed {pathPadding.AngleInfos.Count} angle infos.";
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DrawLines(Graphics g,  PointF[] points, Color color)
        {
            using (Pen pen = new Pen(color, 2F))
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
                DrawLines(g, paddingPoints, Color.Red);
                for (int i = 0; i < pathPadding.AngleInfos.Count; i++)
                {
                    var angleInfo = pathPadding.AngleInfos[i];
                    if (angleInfo.StartIndex >= 0)
                    {
                        DisplayPoint(g, angleInfo.StartIndex, Color.Blue);
                    }
                    if (angleInfo.EndIndex >= 0)
                    {
                        DisplayPoint(g, angleInfo.EndIndex, Color.Yellow);
                    }
                }
            }
        }

        private void DisplayPoint(Graphics g, int index, Color color)
        {
            PointF p = pattern.CurvePoints[index];
            Tools.DrawSquare(g, color, p, size: 2);
        }

        private void picPattern_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DisplayPattern(e.Graphics);
                DisplayPadding(e.Graphics);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
