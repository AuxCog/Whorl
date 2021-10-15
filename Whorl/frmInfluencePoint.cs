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
    public partial class frmInfluencePoint : Form
    {
        public frmInfluencePoint()
        {
            InitializeComponent();
        }

        private InfluencePointInfo origInfluencePointInfo { get; set; }
        private InfluencePointInfo editedInfluencePointInfo { get; set; }

        private void frmInfluencePoint_Load(object sender, EventArgs e)
        {
            try
            {
                cboTransformFunction.DataSource = InfluencePointInfo.TransformFunctionNames.ToList();
                cboTransformFunction.SelectedItem = editedInfluencePointInfo.TransformFunctionName;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(InfluencePointInfo influencePointInfo)
        {
            try
            {
                origInfluencePointInfo = influencePointInfo;
                editedInfluencePointInfo = new InfluencePointInfo();
                editedInfluencePointInfo.CopyProperties(origInfluencePointInfo);
                lblPointID.Text = influencePointInfo.Id.ToString();
                chkEnabled.Checked = editedInfluencePointInfo.Enabled;
                txtInfluenceFactor.Text = editedInfluencePointInfo.InfluenceFactor.ToString("0.####");
                txtAverageWeight.Text = editedInfluencePointInfo.AverageWeight.ToString("0.######");
                txtDivisor.Text = (1.0 / editedInfluencePointInfo.DivFactor).ToString("0.00");
                txtOffset.Text = editedInfluencePointInfo.Offset.ToString("0.####");
                txtPower.Text = editedInfluencePointInfo.Power.ToString("0.####");
                txtFunctionOffset.Text = editedInfluencePointInfo.FunctionOffset.ToString("0.####");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private string PopulateObject()
        {
            var errMessages = new List<string>();
            editedInfluencePointInfo.Enabled = chkEnabled.Checked;
            double val;
            if (double.TryParse(txtInfluenceFactor.Text, out val))
                editedInfluencePointInfo.InfluenceFactor = val;
            else
                errMessages.Add("Influence Factor must be a number.");
            if (double.TryParse(txtAverageWeight.Text, out val))
                editedInfluencePointInfo.AverageWeight = val;
            else
                errMessages.Add("Average Weight must be a number.");
            if (!double.TryParse(txtDivisor.Text, out val))
                val = -1.0;
            if (val >= 0.01)
                editedInfluencePointInfo.DivFactor = 1.0 / val;
            else
                errMessages.Add("Divisor must be >= 0.01");
            if (!double.TryParse(txtOffset.Text, out val))
                val = -1.0;
            if (val >= 0.0001)
                editedInfluencePointInfo.Offset = val;
            else
                errMessages.Add("Offset must be >= 0.0001");
            if (double.TryParse(txtPower.Text, out val))
                editedInfluencePointInfo.Power = val;
            else
                errMessages.Add("Power must be a number.");
            if (double.TryParse(txtFunctionOffset.Text, out val))
                editedInfluencePointInfo.FunctionOffset = val;
            else
                errMessages.Add("Function Offset must be a number.");
            editedInfluencePointInfo.TransformFunctionName = (string)cboTransformFunction.SelectedItem;
            return errMessages.Any() ? string.Join(Environment.NewLine, errMessages) : null;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string errMessage = PopulateObject();
                if (errMessage != null)
                    MessageBox.Show(errMessage);
                else
                {
                    origInfluencePointInfo.CopyProperties(editedInfluencePointInfo);
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

    }
}
