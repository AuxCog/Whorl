using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Whorl.Pattern.RenderingInfo;

namespace Whorl
{
    public partial class FrmEditDistancePatternSettings : Form
    {
        public FrmEditDistancePatternSettings()
        {
            InitializeComponent();
        }

        private DistancePatternSettings distancePatternSettings { get; set; }

        public void Initialize(DistancePatternSettings distancePatternSettings, Pattern pattern)
        {
            this.distancePatternSettings = distancePatternSettings;
            ChkUseFadeout.Checked = distancePatternSettings.UseFadeOut;
            txtStartPercentage.Text = (100.0 * distancePatternSettings.FadeStartRatio).ToString("0.##");
            txtEndPercentage.Text = (100.0 * distancePatternSettings.FadeEndRatio).ToString("0.##");
            txtEndValue.Text = distancePatternSettings.EndDistanceValue.ToString("0.##");
            ChkAutoEnd.Checked = distancePatternSettings.AutoEndValue;
            pnlFadeOut.Enabled = ChkUseFadeout.Checked;
            var idItems = new List<object>() { "(None)" };
            idItems.AddRange(pattern.InfluencePointInfoList.InfluencePointInfos.OrderBy(ip => ip.Id));
            cboInfluencePointId.DataSource = idItems;
            if (distancePatternSettings.InfluencePointId != null)
                cboInfluencePointId.SelectedItem = pattern.InfluencePointInfoList.InfluencePointInfos
                                                   .FirstOrDefault(ip => ip.Id == distancePatternSettings.InfluencePointId);
        }

        private void ChkUseFadeout_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                pnlFadeOut.Enabled = ChkUseFadeout.Checked;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private bool PopulateSettings()
        {
            var sbErrors = new StringBuilder();
            double startRatio = 0, endRatio = 0, endValue = 0;
            if (ChkUseFadeout.Checked)
            {
                if (!ValueParser.TryParseDouble(txtStartPercentage.Text, out startRatio, val => val > 0,
                                               "Start Percentage must be a non-negative number.", sbErrors))
                {
                    startRatio = 0;
                }
                ValueParser.TryParseDouble(txtEndPercentage.Text, out endRatio, val => val > startRatio,
                                           "End Percentage must be a number greater than Start Percentage.", sbErrors);
                if (!ChkAutoEnd.Checked)
                {
                    ValueParser.TryParseDouble(txtEndValue.Text, out endValue, val => val >= 0,
                                               "End Distance Value must be a non-negative number.", sbErrors);
                }
            }
            if (sbErrors.Length > 0)
            {
                MessageBox.Show(sbErrors.ToString());
            }
            else
            {
                distancePatternSettings.UseFadeOut = ChkUseFadeout.Checked;
                if (distancePatternSettings.UseFadeOut)
                {
                    distancePatternSettings.FadeStartRatio = 0.01 * startRatio;
                    distancePatternSettings.FadeEndRatio = 0.01 * endRatio;
                    distancePatternSettings.AutoEndValue = ChkAutoEnd.Checked;
                    if (!distancePatternSettings.AutoEndValue)
                        distancePatternSettings.EndDistanceValue = endValue;
                }
                var influencePoint = cboInfluencePointId.SelectedItem as InfluencePointInfo;
                distancePatternSettings.Parent.SetInfluencePointCenter(influencePoint);
            }
            return sbErrors.Length == 0;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (PopulateSettings())
                {
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
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ChkAutoEnd_CheckedChanged(object sender, EventArgs e)
        {
            txtEndValue.Enabled = !ChkAutoEnd.Checked;
        }
    }
}
