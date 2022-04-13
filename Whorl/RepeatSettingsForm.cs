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
    public partial class RepeatSettingsForm : Form
    {
        public RepeatSettingsForm()
        {
            InitializeComponent();
        }

        public enum RepeatModes
        {
            None,
            Circular,
            Radial,
            Horizontal,
            Vertical
        }

        public RepeatModes RepeatMode { get; private set; }

        public float GridInterval { get; private set; }

        public int? Repetitions { get; private set; }

        public bool EntireRibbon
        {
            get { return this.chkEntireRibbon.Checked; }
        }

        public bool FillGrid
        {
            get { return this.chkFillGrid.Checked; }
        }

        public bool ReverseDirection
        {
            get { return this.chkReverse.Checked; }
        }

        public bool UseSelectedPatternCenter
        {
            get { return this.chkSelectedPatternCenter.Checked; }
        }

        public bool RepeatAtVertices
        {
            get { return chkRepeatAtVertices.Checked; }
        }

        public bool TrackPathAngle
        {
            get { return chkTrackPathAngle.Checked; }
        }

        private void chkFillGrid_CheckedChanged(object sender, EventArgs e)
        {
            this.txtRepetitions.Enabled = !chkFillGrid.Checked || string.IsNullOrWhiteSpace(txtGridInterval.Text);
        }

        public void Initialize(PatternList patternList)
        {
            PathPattern pathPattern = null;
            if (patternList.PatternsList.Count == 1)
            {
                pathPattern = patternList.PatternsList[0] as PathPattern;
                if (pathPattern != null && pathPattern.CurveVertexIndices == null)
                {
                    pathPattern.ComputeSeedAndCurvePoints();
                }
            }
            chkRepeatAtVertices.Visible = chkTrackPathAngle.Visible =
                pathPattern?.CurveVertexIndices != null && pathPattern.CurveVertexIndices.Any();
            if (!chkRepeatAtVertices.Visible)
                chkRepeatAtVertices.Checked = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkRepeatAtVertices.Checked)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                    return;
                }
                string message = null;
                if (this.rdoCircular.Checked)
                    RepeatMode = RepeatModes.Circular;
                else if (this.rdoRadial.Checked)
                    RepeatMode = RepeatModes.Radial;
                else if (this.rdoHorizontal.Checked)
                    RepeatMode = RepeatModes.Horizontal;
                else if (this.rdoVertical.Checked)
                    RepeatMode = RepeatModes.Vertical;
                else
                    message = "Please select Circular, Radial, Horizontal, or Vertical.";
                if (message == null)
                {
                    float? fVal = Tools.ConvertNumericInput<float>(txtGridInterval.Text, "Grid Squares Interval", ref message, 
                        minValue: 0.001F, defaultValue: (chkFillGrid.Checked ? (float?)0 : null));
                    if (fVal != null)
                        GridInterval = (float)fVal;
                }
                if (chkFillGrid.Checked && GridInterval != 0)
                {
                    Repetitions = null;
                }
                else if (message == null)
                {
                    int? iVal = Tools.ConvertNumericInput<int>(txtRepetitions.Text, "Repetitions", ref message, minValue: 1);
                    if (iVal != null)
                        Repetitions = (int)iVal;
                }
                if (message != null)
                {
                    MessageBox.Show(message, "Message");
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
    }
}
