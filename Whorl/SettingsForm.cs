using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Whorl
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            cboDraftSize.DataSource = Enumerable.Range(2, 19).ToList();
        }

        public void Initialize()
        {
            txtFilesFolder.Text = WhorlSettings.Instance.FilesFolder;
            txtQualitySize.Text = WhorlSettings.Instance.QualitySize.ToString();
            txtBufferSize.Text = WhorlSettings.Instance.BufferSize.ToString();
            txtThumbnailSize.Text = WhorlSettings.Instance.DesignThumbnailHeight.ToString();
            chkExactOutlines.Checked = WhorlSettings.Instance.ExactOutline;
            chkSaveDesignThumbnails.Checked = WhorlSettings.Instance.SaveDesignThumbnails;
            chkNewPolygonVersion.Checked = WhorlSettings.Instance.UseNewPolygonVersion;
            txtAnimationRate.Text = WhorlSettings.Instance.AnimationRate.ToString();
            txtSpinRate.Text = WhorlSettings.Instance.SpinRate.ToString();
            txtRevolveRate.Text = WhorlSettings.Instance.RevolveRate.ToString();
            txtImprovisationLevel.Text = (1000D * WhorlSettings.Instance.ImprovisationLevel).ToString();
            txtImprovDamping.Text = WhorlSettings.Instance.ImprovDamping.ToString();
            txtRecomputeInterval.Text = WhorlSettings.Instance.RecomputeInterval.ToString();
            chkImproviseOnOutlineType.Checked = WhorlSettings.Instance.ImproviseOnOutlineType;
            chkImproviseColors.Checked = WhorlSettings.Instance.ImproviseColors;
            chkImproviseTextures.Checked = WhorlSettings.Instance.ImproviseTextures;
            chkImproviseShapes.Checked = WhorlSettings.Instance.ImproviseShapes;
            chkImprovisePetals.Checked = WhorlSettings.Instance.ImprovisePetals;
            chkImproviseBackground.Checked = WhorlSettings.Instance.ImproviseBackground;
            chkImproviseParameters.Checked = WhorlSettings.Instance.ImproviseParameters;
            txtMaxLoopCount.Text = WhorlSettings.Instance.MaxLoopCount.ToString();
            chkOptimizeExpressions.Checked = WhorlSettings.Instance.OptimizeExpressions;
            cboDraftSize.SelectedItem = WhorlSettings.Instance.DraftSize;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
           try
            {
                bool validDirectory = !string.IsNullOrEmpty(txtFilesFolder.Text);
                if (validDirectory && !Directory.Exists(txtFilesFolder.Text))
                {
                    if (MessageBox.Show("Create folder " + txtFilesFolder.Text + "?",
                        "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Directory.CreateDirectory(txtFilesFolder.Text);
                    }
                    else
                    {
                        validDirectory = false;
                    }
                }
                if (validDirectory)
                {
                    WhorlSettings.Instance.FilesFolder = txtFilesFolder.Text;
                }
                string message = null;
                int? iVal;
                iVal = Tools.ConvertNumericInput<int>(txtQualitySize.Text,  "Quality Size", 
                                                 ref message, minValue: 100);
                if (iVal != null)
                    WhorlSettings.Instance.QualitySize = (int)iVal;
                iVal = Tools.ConvertNumericInput<int>(txtBufferSize.Text,  "Buffer Size",
                                                 ref message, minValue: 0);
                if (iVal != null)
                    WhorlSettings.Instance.BufferSize = (int)iVal;
                iVal = Tools.ConvertNumericInput<int>(txtThumbnailSize.Text,  "Thumbnail Height",
                                                 ref message, minValue: 50, maxValue: 500);
                if (iVal != null)
                    WhorlSettings.Instance.DesignThumbnailHeight = (int)iVal;
                double? dVal;
                dVal = Tools.ConvertNumericInput<double>(txtImprovisationLevel.Text,  "Improvisation Level", ref message, 
                       minValue: 0, maxValue: 500);
                if (dVal != null)
                    WhorlSettings.Instance.ImprovisationLevel = (double)dVal / 1000D;
                dVal = Tools.ConvertNumericInput<double>(txtRecomputeInterval.Text,  "Recompute Interval", ref message, 
                       minValue: 0.1);
                if (dVal != null)
                    WhorlSettings.Instance.RecomputeInterval = (double)dVal;
                dVal = Tools.ConvertNumericInput<double>(txtImprovDamping.Text,  
                       "Improvisation Damping", ref message, minValue: 0.1);
                if (dVal != null)
                    WhorlSettings.Instance.ImprovDamping = (double)iVal;
                iVal = Tools.ConvertNumericInput<int>(txtAnimationRate.Text,  "Animation Rate", ref message,
                       minValue: 1, maxValue: 100);
                if (iVal != null)
                    WhorlSettings.Instance.AnimationRate = (int)iVal;
                dVal = Tools.ConvertNumericInput<double>(txtSpinRate.Text,  "Spin Rate", ref message,
                       minValue: -1000, maxValue: 1000);
                if (dVal != null)
                    WhorlSettings.Instance.SpinRate = (double)dVal;
                dVal = Tools.ConvertNumericInput<double>(txtRevolveRate.Text,  "Revolve Rate", ref message,
                       minValue: -1000, maxValue: 1000);
                if (dVal != null)
                    WhorlSettings.Instance.RevolveRate = (double)dVal;
                iVal = Tools.ConvertNumericInput<int>(txtMaxLoopCount.Text,  "Max Loop Count", ref message);
                if (iVal != null)
                {
                    int maxLoopCount = (int)iVal;
                    if (maxLoopCount <= 0)
                        maxLoopCount = int.MaxValue;
                    WhorlSettings.Instance.MaxLoopCount = maxLoopCount;
                    ParserEngine.Expression.MaxLoopCount = maxLoopCount;
                }
                if (message != null)
                {
                    MessageBox.Show(message);
                }
                else
                {
                    WhorlSettings.Instance.DraftSize = (int)cboDraftSize.SelectedItem;
                    WhorlSettings.Instance.ExactOutline = chkExactOutlines.Checked;
                    WhorlSettings.Instance.SaveDesignThumbnails = chkSaveDesignThumbnails.Checked;
                    WhorlSettings.Instance.ImproviseOnOutlineType = chkImproviseOnOutlineType.Checked;
                    WhorlSettings.Instance.ImproviseColors = chkImproviseColors.Checked;
                    WhorlSettings.Instance.ImproviseTextures = chkImproviseTextures.Checked;
                    WhorlSettings.Instance.ImproviseShapes = chkImproviseShapes.Checked;
                    WhorlSettings.Instance.ImprovisePetals = chkImprovisePetals.Checked;
                    WhorlSettings.Instance.ImproviseBackground = chkImproviseBackground.Checked;
                    WhorlSettings.Instance.ImproviseParameters = chkImproviseParameters.Checked;

                    WhorlSettings.Instance.UseNewPolygonVersion = chkNewPolygonVersion.Checked;
                    WhorlSettings.Instance.OptimizeExpressions = chkOptimizeExpressions.Checked;

                    WhorlSettings.Instance.Save();

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
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnBrowseFilesFolder_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (Directory.Exists(txtFilesFolder.Text))
                    dlg.SelectedPath = txtFilesFolder.Text;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtFilesFolder.Text = dlg.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void chkImproviseColors_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                chkImproviseTextures.Enabled = chkImproviseColors.Checked;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
