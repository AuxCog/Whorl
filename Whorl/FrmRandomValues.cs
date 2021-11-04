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
    public partial class FrmRandomValues : Form
    {
        public FrmRandomValues()
        {
            InitializeComponent();
        }

        private RandomValues randomValues { get; set; }

        public void Initialize(RandomValues randomValues)
        {
            try
            {
                if (randomValues == null)
                    throw new NullReferenceException("randomValues cannot be null.");
                this.randomValues = randomValues;
                txtWeight.Text = randomValues.Settings.Weight.ToString("0.##");
                txtSmoothness.Text = randomValues.Settings.Smoothness.ToString("0.##");
                ChkClosed.Checked = randomValues.Settings.Closed;
                ChkClipValues.Checked = randomValues.Settings.ClipYValues;
                chkReseedRandom.Checked = false;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool PopulateSettings()
        {
            var sbErrors = new StringBuilder();
            if (!float.TryParse(txtWeight.Text, out float weight))
                weight = -1F;
            if (weight < 0F)
                sbErrors.AppendLine("Weight must be a non-negative number.");
            if (!float.TryParse(txtSmoothness.Text, out float smoothness))
                smoothness = -1F;
            if (smoothness <= 0F)
                sbErrors.AppendLine("Smoothness must be a positive number.");
            if (sbErrors.Length == 0)
            {   //No input errors.
                randomValues.Settings.Weight = weight;
                randomValues.Settings.Smoothness = smoothness;
                randomValues.Settings.Closed = ChkClosed.Checked;
                randomValues.Settings.ClipYValues = ChkClipValues.Checked;
                if (chkReseedRandom.Checked)
                    randomValues.Settings.ReseedRandom();
            }
            else
            {   //Show error messages.
                MessageBox.Show(sbErrors.ToString());
            }
            return sbErrors.Length == 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
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

        private void btnCancel_Click(object sender, EventArgs e)
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

    }
}
