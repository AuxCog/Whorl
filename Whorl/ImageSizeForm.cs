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
    public partial class ImageSizeForm : Form
    {
        public ImageSizeForm()
        {
            InitializeComponent();
        }

        public Size ImageSize { get; private set; }
        public bool DockImage { get { return chkDockImage.Checked; } }
        public bool ResizeDesign { get { return chkResizeDesign.Checked; } }
        public bool ScaleDesign { get { return chkScaleDesign.Checked; } }

        private bool handleTextChanged;

        public class ImageSizeInfo
        {
            private float _aspectRatio;
            public float AspectRatio
            {
                get { return _aspectRatio; }
                set
                {
                    _aspectRatio = value;
                    SetSize();
                }
            }
            private int _imageWidth;
            public int ImageWidth
            {
                get { return _imageWidth; }
                set
                {
                    _imageWidth = value;
                    SetSize();
                }
            }
            public string Description { get; set; }
            public Size Size { get; private set; }

            public ImageSizeInfo(float aspectRatio, int imageWidth, string description)
            {
                AspectRatio = aspectRatio;
                ImageWidth = imageWidth;
                Description = description;
            }

            private void SetSize()
            {
                int height = (int)Math.Round(AspectRatio * ImageWidth);
                if (Math.Abs(height - MainForm.ContainerSize.Height) <= 1)
                    height = MainForm.ContainerSize.Height;
                Size = new Size(ImageWidth, height);
            }

            public override string ToString()
            {
                Size size = Size;
                return $"{size.Width.ToString().PadLeft(4)} x {size.Height.ToString().PadLeft(4)}:{Description}";
            }

            public string GetSetting()
            {
                return $"{AspectRatio:0.000}:{ImageWidth}:{Description}";
            }
        }

        public void Initialize(Size imageSize)
        {
            try
            {
                if (imageSize.Width == 0 || imageSize.Height == 0)
                    throw new Exception("Invalid imageSize.");
                handleTextChanged = false;
                ImageSize = imageSize;
                txtWidth.Text = imageSize.Width.ToString();
                txtHeight.Text = imageSize.Height.ToString();
                chkDockImage.Checked = false;
                chkResizeDesign.Checked = true;
                ImageSizeInfo[] imageSizeInfos = GetImageSizes(WhorlSettings.Instance.ImageSizes);
                cboImageSize.DataSource = imageSizeInfos;
                float aspectRatio = (float)GetAspectRatio(imageSize);
                ImageSizeInfo curSizeInfo = imageSizeInfos.Where(sz => Tools.NumbersEqual(sz.AspectRatio, aspectRatio, 0.005F)).FirstOrDefault();
                if (curSizeInfo != null)
                    cboImageSize.SelectedItem = curSizeInfo;
                handleTextChanged = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public static ImageSizeInfo[] GetImageSizes(string settingSizes)
        {
            string[] sizes = settingSizes.Split(',');
            ImageSizeInfo[] imgSizes = new ImageSizeInfo[sizes.Length];
            for (int i = 0; i < sizes.Length; i++)
            {
                string[] parts = sizes[i].Split(':');
                imgSizes[i] = new ImageSizeInfo(float.Parse(parts[0]), int.Parse(parts[1]), parts[2]);
            }
            return imgSizes;
        }

        private Size? GetImageSize()
        {
            string message = null;
            int width = 0, height = 0;
            object oVal = Tools.ConvertNumericInput(txtWidth.Text, typeof(int),
                          "Image Width", ref message, minValue: 1);
            if (oVal is int)
                width = (int)oVal;
            oVal = Tools.ConvertNumericInput(txtHeight.Text, typeof(int),
                   "Image Height", ref message, minValue: 1);
            if (oVal is int)
                height = (int)oVal;
            if (message != null)
            {
                MessageBox.Show(message, "Message");
                return null;
            }
            else
                return new Size(width, height);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                Size? imgSize1 = GetImageSize();
                if (imgSize1 != null)
                {
                    this.ImageSize = (Size)imgSize1;
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

        public static ImageSizeInfo GetPrevImageSize(string imgSizeText)
        {
            ImageSizeInfo imageSizeInfo;
            string settingName;
            string[] sizeAndName = imgSizeText.Split(':');
            if (sizeAndName.Length == 2)
            {
                imgSizeText = sizeAndName[0];
                settingName = sizeAndName[1];
            }
            else
            {
                return null;
            }
            string[] widthHeight = imgSizeText.Split('x');
            if (int.TryParse(widthHeight[0], out int width) && int.TryParse(widthHeight[1], out int height))
            {
                var imgSize = new Size(width, height);
                imageSizeInfo = new ImageSizeInfo((float)GetAspectRatio(imgSize), width, settingName);
            }
            else
                imageSizeInfo = null;
            return imageSizeInfo;
        }

        private void btnSetImageSize_Click(object sender, EventArgs e)
        {
            try
            {
                var sizeInfo = cboImageSize.SelectedItem as ImageSizeInfo;
                if (sizeInfo == null)
                    return;
                txtSettingName.Text = sizeInfo.Description;
                if (chkSizeToFit.Checked)
                    ImageSize = WhorlDesign.FitSizeToContainer(MainForm.ContainerSize, sizeInfo.Size);
                else
                    ImageSize = sizeInfo.Size;
                handleTextChanged = false;
                txtWidth.Text = ImageSize.Width.ToString();
                txtHeight.Text = ImageSize.Height.ToString();
                handleTextChanged = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private string GetImageSizeString(string setting)
        //{
        //    int ind = setting.IndexOf(':');
        //    return ind == -1 ? setting : setting.Substring(0, ind);
        //}

        public static double GetAspectRatio(Size size)
        {
            return Math.Round((double)size.Height / size.Width, 3);
        }

        public static string GetSizeSetting(Size size, string description)
        {
            double aspectRatio = GetAspectRatio(size);
            return $"{aspectRatio}:{description}";
        }




        private void btnSaveImageSize_Click(object sender, EventArgs e)
        {
            try
            {
                Size? imgSize = GetImageSize();
                if (imgSize == null)
                    return;  //Can't parse size.
                if (string.IsNullOrWhiteSpace(txtSettingName.Text))
                {
                    MessageBox.Show("Please enter the setting name.");
                    return;
                }
                Size imageSize = (Size)imgSize;
                List<ImageSizeInfo> savedSizes = GetImageSizes(WhorlSettings.Instance.ImageSizes).ToList();
                float aspectRatio = (float)GetAspectRatio(imageSize);
                ImageSizeInfo sizeInfo = new ImageSizeInfo(aspectRatio, imageSize.Width, txtSettingName.Text);
                int ind = savedSizes.FindIndex(sz => sz.AspectRatio == aspectRatio && sz.ImageWidth == imageSize.Width);
                if (ind == -1)
                    savedSizes.Add(sizeInfo);
                else
                    savedSizes[ind] = sizeInfo;
                WhorlSettings.Instance.ImageSizes = GetWhorlSetting(savedSizes);
                WhorlSettings.Instance.Save();
                cboImageSize.DataSource = savedSizes;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnDeleteImageSize_Click(object sender, EventArgs e)
        {
            try
            {
                var sizeInfo = cboImageSize.SelectedItem as ImageSizeInfo;
                if (sizeInfo == null)
                    return;
                List<ImageSizeInfo> savedSizes = GetImageSizes(WhorlSettings.Instance.ImageSizes).ToList();
                int ind = savedSizes.FindIndex(sz => sz.AspectRatio == sizeInfo.AspectRatio && sz.ImageWidth == sizeInfo.ImageWidth);
                if (ind != -1)
                {
                    savedSizes.RemoveAt(ind);
                    WhorlSettings.Instance.ImageSizes = GetWhorlSetting(savedSizes);
                    WhorlSettings.Instance.Save();
                    cboImageSize.DataSource = savedSizes;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private string GetWhorlSetting(IEnumerable<ImageSizeInfo> infos)
        {
            return string.Join(",", infos.Select(si => si.GetSetting()));
        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleTextChanged)
                    return;
                if (int.TryParse(txtWidth.Text, out int width))
                {
                    if (chkMaintainAspectRatio.Checked)
                    {
                        int height = width * ImageSize.Height / ImageSize.Width;
                        handleTextChanged = false;
                        txtHeight.Text = height.ToString();
                        handleTextChanged = true;
                    }
                    else
                    {
                        ImageSize = new Size(width, ImageSize.Height);
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
                if (!handleTextChanged)
                    return;
                if (int.TryParse(txtHeight.Text, out int height))
                {
                    if (chkMaintainAspectRatio.Checked)
                    {
                        int width = height * ImageSize.Width / ImageSize.Height;
                        handleTextChanged = false;
                        txtWidth.Text = width.ToString();
                        handleTextChanged = true;
                    }
                    else
                    {
                        ImageSize = new Size(ImageSize.Width, height);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
