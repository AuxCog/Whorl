using ParserEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Whorl.ColorGradient;

namespace Whorl
{
    public partial class frmBlendImages : Form
    {
        private enum DisplayModes
        {
            Blend,
            Image1,
            Image2
        }
        private BlendedImage blendedImage { get; } = new BlendedImage();
        private DisplayModes displayMode { get; set; } = DisplayModes.Image1;
        private string currentFileName { get; set; }
        private Size image1Size { get; set; } = Size.Empty;
        private bool ignoreEvents { get; set; }

        public frmBlendImages()
        {
            InitializeComponent();
            try
            {
                blendedImage.Initialize(txtBlendDepthPct, txtSlopePct, txtXOffset,
                                        txtBlendOffsetPct, txtBlendAngle, picBlendFunction);
                cboDisplayMode.DataSource = Enum.GetValues(typeof(DisplayModes));
                cboDisplayMode.SelectedItem = DisplayModes.Image1;
                ShowTitle();
                blendedImage.IsChangedChangedEvent += BlendedImage_IsChangedChanged;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void frmBlendImages_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (blendedImage.IsChanged)
                {
                    switch (MessageBox.Show("Save changes?", "Confirm", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.No:
                            break;
                        case DialogResult.Yes:
                            SaveBlend(saveAs: false);
                            break;
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BlendedImage_IsChangedChanged(object sender, EventArgs e)
        {
            ShowTitle();
        }

        private void ShowTitle()
        {
            string title = "Blend Images: ";
            if (currentFileName == null)
                title += "New Blend";
            else
                title += Path.GetFileName(currentFileName);
            if (blendedImage.IsChanged)
                title += "*";
            this.Text = title;
        }

        private bool ApplyBlend(bool parseSettings = true)
        {
            bool success = blendedImage.ApplyBlend(parseSettings);
            if (success)
            {
                DisplayImage(DisplayModes.Blend);
                RenderGroupBox.Enabled = true;
            }
            return success;
        }

        private void btnApplyBlend_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyBlend();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetRenderingInfo()
        {
            image1Size = blendedImage.ImageInfo1.OriginalSize;
            ignoreEvents = true;
            txtRenderWidth.Text = image1Size.Width.ToString();
            txtRenderHeight.Text = image1Size.Height.ToString();
            ignoreEvents = false;
        }

        private void BtnBrowseImage1_Click(object sender, EventArgs e)
        {
            try
            {
                if (blendedImage.OpenImage(blendedImage.ImageInfo1) != null)
                {
                    DisplayImage(DisplayModes.Image1);
                    txtImage1FileName.Text = blendedImage.ImageInfo1.FileName;
                    SetRenderingInfo();
                    if (blendedImage.ImageInfo2.FloatPixels != null)
                    {
                        if (MessageBox.Show("Preserve Image 2 Settings?", "Confirm",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            blendedImage.OpenImage(blendedImage.ImageInfo2,
                                blendedImage.ImageInfo2.FileName, 
                                blendedImage.ImageInfo1.ImageSize);
                        }
                        else
                        {
                            blendedImage.ImageInfo2.ClearSettings();
                            txtImage2FileName.Text = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnBrowseImage2_Click(object sender, EventArgs e)
        {
            try
            {
                if (blendedImage.ImageInfo1.FloatPixels == null)
                {
                    MessageBox.Show("Please browse to Image 1 first.");
                    return;
                }
                if (blendedImage.OpenImage(blendedImage.ImageInfo2,
                                           blendedImage.ImageInfo1.ImageSize) != null)
                {
                    DisplayImage(DisplayModes.Image2);
                    txtImage2FileName.Text = blendedImage.ImageInfo2.FileName;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private string GetFilesFolder()
        {
            return Path.Combine(WhorlSettings.Instance.FilesFolder, "BlendedImages");
        }

        private void SaveBlend(bool saveAs)
        {
            string errMessage = blendedImage.ValidateForSave();
            if (errMessage != null)
            {
                MessageBox.Show(errMessage);
                return;
            }
            string fileName;
            if (saveAs || currentFileName == null)
            {
                //Browse for fileName.
                fileName = Tools.GetSaveXmlFileName("Blend file (*.xml)",
                                  GetFilesFolder(), currentFileName);
                if (fileName == null) 
                    return;  //User cancelled.
                currentFileName = fileName;
            }
            else
            {
                fileName = currentFileName;
            }
            var xmlTools = new XmlTools();
            xmlTools.CreateXml(blendedImage);
            xmlTools.SaveXml(fileName);
            ShowTitle();
        }

        private void saveBlendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveBlend(saveAs: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void saveBlendAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveBlend(saveAs: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void openBlendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Blend file (*.xml)|*.xml";
                string filesFolder = GetFilesFolder();
                if (Directory.Exists(filesFolder))
                    dlg.InitialDirectory = filesFolder;
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                currentFileName = dlg.FileName;
                var xmlTools = new XmlTools();
                xmlTools.LoadXmlDocument(dlg.FileName);
                xmlTools.ReadXml(blendedImage, nameof(BlendedImage));
                txtImage1FileName.Text = blendedImage.ImageInfo1.FileName;
                txtImage2FileName.Text = blendedImage.ImageInfo2.FileName;
                ApplyBlend(parseSettings: false);
                ShowTitle();
                SetRenderingInfo();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DisplayImage(DisplayModes mode)
        {
            if (mode == displayMode)
                DisplayImage();
            else
                cboDisplayMode.SelectedItem = mode;  //Event handler displays image.
        }

        private void DisplayImage()
        {
            switch (displayMode)
            {
                case DisplayModes.Blend:
                    picImage.Image = blendedImage.BlendedImageInfo.Bitmap;
                    break;
                case DisplayModes.Image1:
                    picImage.Image = blendedImage.ImageInfo1.Bitmap;
                    break;
                case DisplayModes.Image2:
                    picImage.Image = blendedImage.ImageInfo2.Bitmap;
                    break;
            }
        }

        private void cboDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDisplayMode.SelectedItem is DisplayModes)
            {
                displayMode = (DisplayModes)cboDisplayMode.SelectedItem;
                DisplayImage();
            }
        }

        private void txtRenderWidth_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (ignoreEvents || image1Size == Size.Empty) return;
                if (int.TryParse(txtRenderWidth.Text, out int width))
                {
                    int height = width * image1Size.Height / image1Size.Width;
                    ignoreEvents = true;
                    txtRenderHeight.Text = height.ToString();
                    ignoreEvents = false;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtRenderHeight_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (ignoreEvents || image1Size == Size.Empty) return;
                if (int.TryParse(txtRenderHeight.Text, out int height))
                {
                    int width = height * image1Size.Width / image1Size.Height;
                    ignoreEvents = true;
                    txtRenderWidth.Text = width.ToString();
                    ignoreEvents = false;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnRender_Click(object sender, EventArgs e)
        {
            try
            {
                if (blendedImage.ImageInfo1.FloatPixels == null ||
                    blendedImage.ImageInfo2.FloatPixels == null)
                    return;
                if (!int.TryParse(txtRenderWidth.Text, out int width))
                    return;
                if (!int.TryParse(txtRenderHeight.Text, out int height))
                    return;
                if (width <= 0 || height <= 0)
                    return;
                var renderSize = new Size(width, height);
                var renderBlend = new BlendedImage();
                renderBlend.Initialize(txtBlendDepthPct, txtSlopePct, txtXOffset,
                                       txtBlendOffsetPct, txtBlendAngle);
                renderBlend.OpenImage(renderBlend.ImageInfo1,
                                      blendedImage.ImageInfo1.FileName,
                                      renderSize);
                renderBlend.OpenImage(renderBlend.ImageInfo2,
                                      blendedImage.ImageInfo2.FileName,
                                      renderSize);
                if (!renderBlend.ApplyBlend(parseSettings: true))
                    return;
                var dlg = new SaveFileDialog();
                dlg.Filter = "Image file (*.jpg or *.png)|*.jpg;*.png";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Tools.SavePngOrJpegImageFile(dlg.FileName, renderBlend.BlendedImageInfo.Bitmap);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
