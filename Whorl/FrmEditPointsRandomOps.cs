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
    public partial class FrmEditPointsRandomOps : Form
    {
        public FrmEditPointsRandomOps()
        {
            InitializeComponent();
        }

        private Pattern.RenderingInfo renderingInfo { get; set; }
        private PointsRandomOps editedOps { get; set; }
        private bool seedsChanged { get; set; }
        private bool displayPoints { get; set; }
        private static PointsRandomOps copiedPointsRandomOps { get; set; }

        public void Initialize(Pattern.RenderingInfo renderingInfo)
        {
            try
            {
                if (renderingInfo == null)
                    throw new NullReferenceException("renderingInfo cannot be null.");
                this.renderingInfo = renderingInfo;
                if (renderingInfo.PointsRandomOps == null)
                    editedOps = new PointsRandomOps();
                else
                    editedOps = new PointsRandomOps(renderingInfo.PointsRandomOps); //Create copy.
                seedsChanged = displayPoints = false;
                PopulateControls();
                pasteSettingsToolStripMenuItem.Enabled = copiedPointsRandomOps != null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
                Close();
            }
        }

        private void BtnReseedPoints_Click(object sender, EventArgs e)
        {
            try
            {
                editedOps.PointRandomOps.SetNewSeed();
                seedsChanged = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnReseedValues_Click(object sender, EventArgs e)
        {
            try
            {
                editedOps.ValueRandomOps.SetNewSeed();
                seedsChanged = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (PopulateRandomOps(out _))
                {
                    if (renderingInfo.PointsRandomOps != null)
                        renderingInfo.PointsRandomOps.ClearValues();
                    renderingInfo.PointsRandomOps = editedOps;
                    DialogResult = DialogResult.OK;
                    Close();
                }
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
                Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool ComputePoints()
        {
            if (!PopulateRandomOps(out bool createPoints))
                return false;
            if (createPoints || seedsChanged)
            {
                editedOps.ComputePoints();
                seedsChanged = false;
            }
            return true;
        }

        private void BtnDisplay_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ComputePoints())
                    return;
                Size size = picDisplay.ClientSize;
                editedOps.UnitScalePoint = new PointF(1F / size.Width, 1F / size.Height);
                editedOps.PanPoint = new PointF(0, 0);
                int[] pixels = new int[size.Width * size.Height];
                int pixInd = 0;
                for (int y = 0; y < size.Height; y++)
                {
                    for (int x = 0; x < size.Width; x++)
                    {
                        double distanceValue = editedOps.ComputeDistanceValue(new PointF(x, y));
                        int colorValue = (50 + (int)(200.0 * distanceValue)) % 255;
                        Color color = Color.FromArgb(red: 0, green: 0, blue: colorValue);
                        pixels[pixInd++] = color.ToArgb();
                    }
                }
                Bitmap bitmap = BitmapTools.CreateFormattedBitmap(size);
                BitmapTools.CopyColorArrayToBitmap(bitmap, pixels);
                displayPoints = true;
                picDisplay.Image = bitmap;
                picDisplay.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnDisplayPoints_Click(object sender, EventArgs e)
        {
            try
            {
                if (ComputePoints())
                {
                    picDisplay.Image = null;
                    displayPoints = true;
                    picDisplay.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picDisplay_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (displayPoints)
                {
                    var size = new SizeF(4, 4);
                    var pScale = new PointF(picDisplay.ClientSize.Width, picDisplay.ClientSize.Height);
                    foreach (PointsRandomOps.RandomPoint randomPoint in editedOps.RandomPoints)
                    {
                        PointF p = new PointF(pScale.X * randomPoint.Point.X, pScale.Y * randomPoint.Point.Y);
                        var rect = new RectangleF(p, size);
                        e.Graphics.FillEllipse(Brushes.Orange, rect);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PopulateControls()
        {
            txtHorizCount.Text = editedOps.HorizCount.ToString();
            txtVertCount.Text = editedOps.VertCount.ToString();
            txtPointRandomWeight.Text = (100.0 * editedOps.PointRandomWeight).ToString("0.##");
            txtValueWeight.Text = editedOps.ValueWeight.ToString("0.####");
            txtOffset.Text = (1000.0 * editedOps.DistanceOffset).ToString("0.####");
            txtPower.Text = editedOps.DistancePower.ToString("0.####");
        }

        private bool PopulateRandomOps(out bool createPoints)
        {
            var sbErrors = new StringBuilder();
            if (!int.TryParse(txtVertCount.Text, out int vertCount))
            {
                vertCount = -1;
            }
            if (vertCount <= 0)
                sbErrors.AppendLine("Number of Rows must be a positive integer.");
            if (!int.TryParse(txtHorizCount.Text, out int horizCount))
            {
                horizCount = -1;
            }
            if (horizCount <= 0)
                sbErrors.AppendLine("Number of Columns must be a positive integer.");
            if (!double.TryParse(txtPointRandomWeight.Text, out double pointWeight))
            {
                pointWeight = -1;
            }
            if (pointWeight < 0 || pointWeight > 100.0)
                sbErrors.AppendLine("Point Variation % must be between 0 and 100.");
            else
                pointWeight *= 0.01;
            if (!double.TryParse(txtValueWeight.Text, out double valueWeight))
            {
                sbErrors.AppendLine("Value Weight must be a number.");
            }
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                offset = -1;
            }
            if (offset <= 0)
                sbErrors.AppendLine("Distance Offset must be a positive number.");
            else
                offset *= 0.001;
            if (!double.TryParse(txtPower.Text, out double power))
            {
                sbErrors.AppendLine("Distance Power must be a number.");
            }
            bool isValid = sbErrors.Length == 0;  //No errors.
            createPoints = editedOps.RandomPoints == null;
            if (isValid)
            {
                if (editedOps.VertCount != vertCount)
                {
                    createPoints = true;
                    editedOps.VertCount = vertCount;
                }
                if (editedOps.HorizCount != horizCount)
                {
                    createPoints = true;
                    editedOps.HorizCount = horizCount;
                }
                if (editedOps.PointRandomWeight != pointWeight)
                {
                    createPoints = true;
                    editedOps.PointRandomWeight = pointWeight;
                }
                editedOps.ValueWeight = valueWeight;
                editedOps.DistanceOffset = offset;
                editedOps.DistancePower = power;
            }
            else
            {
                MessageBox.Show(sbErrors.ToString());
            }
            return isValid;
        }

        private void copySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                copiedPointsRandomOps = new PointsRandomOps(editedOps);
                pasteSettingsToolStripMenuItem.Enabled = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void pasteSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (copiedPointsRandomOps != null)
                {
                    editedOps = new PointsRandomOps(copiedPointsRandomOps);
                    PopulateControls();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
