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
    public partial class frmInfluencePoint : Form
    {
        public frmInfluencePoint()
        {
            InitializeComponent();
        }

        private InfluencePointInfo origInfluencePointInfo { get; set; }
        private InfluencePointInfo editedInfluencePointInfo { get; set; }
        private Pattern parentPattern { get; set; }

        private void frmInfluencePoint_Load(object sender, EventArgs e)
        {
            try
            {
                cboTransformFunction.DataSource = InfluencePointInfo.TransformFunctionNames.ToList();
                cboTransformFunction.SelectedItem = editedInfluencePointInfo.TransformFunctionName;
                if (parentPattern != null)
                {
                    var keyedEnumTypes = parentPattern.GetFormulaSettings().SelectMany(fs => fs.GetKeyedEnumTypes());
                    if (keyedEnumTypes.Any())
                    {
                        var keys = keyedEnumTypes.SelectMany(t => Enum.GetValues(t).Cast<object>().Select(v => Tools.GetEnumKey(v))).ToArray();
                        clbEnumKeys.Items.AddRange(keys);
                        for (int i = 0; i < keys.Length; i++)
                        {
                            if (editedInfluencePointInfo.FilterKeys.Contains(keys[i]))
                                clbEnumKeys.SetItemChecked(i, true);
                        }
                        cboEnumKey.DataSource = keys;
                    }
                }
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
                parentPattern = influencePointInfo.ParentPattern;
                editedInfluencePointInfo = new InfluencePointInfo(origInfluencePointInfo, parentPattern);
                lblPointID.Text = influencePointInfo.Id.ToString();
                chkEnabled.Checked = editedInfluencePointInfo.Enabled;
                txtInfluenceFactor.Text = editedInfluencePointInfo.InfluenceFactor.ToString("0.####");
                txtAverageWeight.Text = editedInfluencePointInfo.AverageWeight.ToString("0.######");
                txtDivisor.Text = (1.0 / editedInfluencePointInfo.DivFactor).ToString("0.00");
                chkUseEllipse.Checked = editedInfluencePointInfo.EllipseStretch != 0;
                txtEllipseStretch.Text = editedInfluencePointInfo.EllipseStretch.ToString("0.#####");
                txtEllipseAngle.Text = editedInfluencePointInfo.EllipseAngle.ToString("0.##");
                txtOffset.Text = editedInfluencePointInfo.Offset.ToString("0.####");
                txtPower.Text = editedInfluencePointInfo.Power.ToString("0.####");
                txtFunctionOffset.Text = editedInfluencePointInfo.FunctionOffset.ToString("0.####");
                EnableEllipseControls();
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
            if (chkUseEllipse.Checked)
            {
                if (double.TryParse(txtEllipseStretch.Text, out val))
                    editedInfluencePointInfo.EllipseStretch = val;
                else
                    errMessages.Add("Ellipse Stretch must be a number.");
                if (double.TryParse(txtEllipseAngle.Text, out val))
                    editedInfluencePointInfo.EllipseAngle = val;
                else
                    errMessages.Add("Ellipse Angle must be a number.");
            }
            else
            {
                editedInfluencePointInfo.EllipseStretch = 0.0;
            }
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
            editedInfluencePointInfo.FilterKeys.Clear();
            editedInfluencePointInfo.FilterKeys.UnionWith(clbEnumKeys.CheckedItems.Cast<string>());
            editedInfluencePointInfo.SetKeyInfosEnabled();
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

        private void cboEnumKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //string key = (string)cboEnumKey.SelectedItem;
                //if (string.IsNullOrEmpty(key))
                //    return;
                //double factor = editedInfluencePointInfo.GetKeyFactor(key);
                //txtKeyFactor.Text = factor.ToString("0.####");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnApplyKeyFactor_Click(object sender, EventArgs e)
        {
            try
            {
                //string key = (string)cboEnumKey.SelectedItem;
                //if (string.IsNullOrEmpty(key))
                //    return;
                //if (!double.TryParse(txtKeyFactor.Text, out double factor))
                //{
                //    MessageBox.Show("Please enter a number for Key Factor.");
                //    return;
                //}
                //editedInfluencePointInfo.FactorsByKey[key] = factor;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private PointF[] graphPoints { get; set; }

        private void BtnGraph_Click(object sender, EventArgs e)
        {
            try
            {
                if (!float.TryParse(txtMaxGraphX.Text, out float maxX))
                {
                    maxX = 0;
                }
                if (maxX <= 0)
                {
                    MessageBox.Show("Please enter a positive number for Maximum X.");
                    return;
                }
                string errMessage = PopulateObject();
                if (errMessage != null)
                {
                    MessageBox.Show(errMessage);
                    return;
                }
                float xInc = maxX / picGraph.ClientSize.Width;
                var yVals = new List<float>();
                for (float x = 0; x <= maxX; x += xInc)
                {
                    var dp = new DoublePoint(x, 0);
                    double val = Math.Abs(editedInfluencePointInfo.ComputeValue(dp, forRendering: false));
                    yVals.Add((float)val);
                }
                float yMax = yVals.Max();
                yVals = yVals.Select(v => yMax - v).ToList();
                float yScale = yMax == 0 ? 1F : 0.95F * picGraph.ClientSize.Height / yMax;
                float xScale = (float)picGraph.ClientSize.Width / yVals.Count;
                graphPoints = Enumerable.Range(0, yVals.Count).Select(i => new PointF(xScale * i, yScale * yVals[i])).ToArray();
                picGraph.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (graphPoints != null)
                {
                    e.Graphics.DrawLines(Pens.Red, graphPoints);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void EnableEllipseControls()
        {
            txtEllipseAngle.Enabled = txtEllipseStretch.Enabled = chkUseEllipse.Checked;
        }

        private void chkUseEllipse_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                EnableEllipseControls();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
