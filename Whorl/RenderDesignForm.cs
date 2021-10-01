using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class RenderDesignForm : Form, IRenderCaller
    {
        private Size designSize;

        public Size NewSize;
        public string FileName { get; private set; }
        public bool ScalePenWidth
        {
            get { return chkScalePenWidth.Checked; }
        }

        public bool DraftMode
        {
            get { return chkDraftMode.Checked; }
        }

        public bool QualityMode
        {
            get { return chkQualityMode.Checked; }
        }

        public RenderDesignForm()
        {
            InitializeComponent();
        }

        public bool CancelRender { get; set; }

        private WhorlDesign design;
        private bool renderStained;
        private string browsedFileName;
        private bool rendering;
        private bool handleEvents;

        public void Initialize(WhorlDesign design, Size designSize, string imageFilePath, bool renderStained)
        {
            try
            {
                handleEvents = false;
                CancelRender = false;
                this.design = design;
                this.designSize = designSize;
                this.renderStained = renderStained;
                int width = WhorlSettings.Instance.RenderWidth;
                if (width == 0)
                    width = designSize.Width;
                txtWidth.Text = width.ToString();
                txtHeight.Text = GetNewHeight(width).ToString();
                string folder = WhorlSettings.Instance.RenderFilesFolder;
                if (string.IsNullOrEmpty(folder))
                    this.FileName = imageFilePath;
                else
                    this.FileName = Path.Combine(folder, Path.GetFileName(imageFilePath));
                lblFileName.Text = this.FileName;
                browsedFileName = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                handleEvents = true;
            }
        }

        private int GetNewHeight(int width)
        {
            float height = (float)width * designSize.Height / designSize.Width;
            return (int)Math.Round(height);
        }

        private int GetNewWidth(int height)
        {
            float width = (float)height * designSize.Width / designSize.Height;
            return (int)Math.Round(width);
        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                if (int.TryParse(txtWidth.Text, out int width))
                {
                    if (width >= 10)
                    {
                        handleEvents = false;
                        txtHeight.Text = GetNewHeight(width).ToString();
                        handleEvents = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtHeight_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                if (int.TryParse(txtHeight.Text, out int height))
                {
                    if (height >= 10)
                    {
                        handleEvents = false;
                        txtWidth.Text = GetNewWidth(height).ToString();
                        handleEvents = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Image file (*.png;*.jpg)|*.png;*.jpg";
                if (!string.IsNullOrEmpty(this.FileName))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(this.FileName);
                    dlg.FileName = Path.GetFileName(this.FileName);
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    browsedFileName = dlg.FileName;
                    this.FileName = dlg.FileName;
                    this.lblFileName.Text = this.FileName;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void RenderCallback(int step, bool initial = false)
        {
            if (InvokeRequired)
            {
                Invoke((Action<int, bool>)RenderCallback, step, initial);
                return;
            }
            if (initial)
                this.progressBar1.Maximum = step;
            else
                this.progressBar1.Value = step;
        }

        private async void btnRender_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(FileName))
                {
                    MessageBox.Show("Please select the output file name.");
                    return;
                }
                if (FileName != browsedFileName && File.Exists(FileName))
                {
                    if (MessageBox.Show($"Overwrite file {FileName}?", "Confirm", 
                                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;
                }
                int width;
                if (!int.TryParse(txtWidth.Text, out width))
                    width = 0;
                if (width <= 0)
                {
                    MessageBox.Show("Please enter a positive integer for Width.");
                    return;
                }
                btnRender.Enabled = false;
                this.NewSize = new Size(width, GetNewHeight(width));

                WhorlSettings.Instance.RenderFilesFolder = Path.GetDirectoryName(this.FileName);
                WhorlSettings.Instance.RenderWidth = width;
                WhorlSettings.Instance.Save();

                int qualitySize = QualityMode ? WhorlSettings.Instance.QualitySize : 0;

                try
                {
                    rendering = true;
                    await design.RenderDesignAsync(FileName, designSize, NewSize, ScalePenWidth,
                                                   DraftMode, this, qualitySize, renderStained);
                }
                finally
                {
                    rendering = false;
                }

                this.Close();
                //this.DialogResult = DialogResult.OK;
                //this.Hide();
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
                if (rendering)
                    CancelRender = true;
                else
                    this.Close();
                //this.DialogResult = DialogResult.Cancel;
                //this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
