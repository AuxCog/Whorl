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
                object oVal;
                oVal = Tools.ConvertNumericInput(txtQualitySize.Text, typeof(int), "Quality Size", 
                                                 ref message, minValue: 100);
                if (oVal is int)
                    WhorlSettings.Instance.QualitySize = (int)oVal;
                oVal = Tools.ConvertNumericInput(txtBufferSize.Text, typeof(int), "Buffer Size",
                                                 ref message, minValue: 0);
                if (oVal is int)
                    WhorlSettings.Instance.BufferSize = (int)oVal;
                oVal = Tools.ConvertNumericInput(txtThumbnailSize.Text, typeof(int), "Thumbnail Height",
                                                 ref message, minValue: 50, maxValue: 500);
                if (oVal is int)
                    WhorlSettings.Instance.DesignThumbnailHeight = (int)oVal;
                oVal = Tools.ConvertNumericInput(txtImprovisationLevel.Text, typeof(double), "Improvisation Level", ref message, 
                       minValue: 0, maxValue: 500);
                if (oVal is double)
                    WhorlSettings.Instance.ImprovisationLevel = (double)oVal / 1000D;
                oVal = Tools.ConvertNumericInput(txtRecomputeInterval.Text, typeof(double), "Recompute Interval", ref message, 
                       minValue: 0.1);
                if (oVal is double)
                    WhorlSettings.Instance.RecomputeInterval = (double)oVal;
                oVal = Tools.ConvertNumericInput(txtImprovDamping.Text, typeof(double), 
                       "Improvisation Damping", ref message, minValue: 0.1);
                if (oVal is double)
                    WhorlSettings.Instance.ImprovDamping = (double)oVal;
                oVal = Tools.ConvertNumericInput(txtAnimationRate.Text, typeof(int), "Animation Rate", ref message,
                       minValue: 1, maxValue: 100);
                if (oVal is int)
                    WhorlSettings.Instance.AnimationRate = (int)oVal;
                oVal = Tools.ConvertNumericInput(txtSpinRate.Text, typeof(double), "Spin Rate", ref message,
                       minValue: -1000, maxValue: 1000);
                if (oVal is double)
                    WhorlSettings.Instance.SpinRate = (double)oVal;
                oVal = Tools.ConvertNumericInput(txtRevolveRate.Text, typeof(double), "Revolve Rate", ref message,
                       minValue: -1000, maxValue: 1000);
                if (oVal is double)
                    WhorlSettings.Instance.RevolveRate = (double)oVal;
                oVal = Tools.ConvertNumericInput(txtMaxLoopCount.Text, typeof(int), "Max Loop Count", ref message);
                if (oVal is int)
                {
                    int maxLoopCount = (int)oVal;
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
