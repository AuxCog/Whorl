using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class BackgroundForm : Form, IColor
    {
        public BackgroundForm()
        {
            InitializeComponent();
            cboImageMode.DataSource = Enum.GetValues(typeof(TextureImageModes));
        }

        private GraphicsPath backgroundGrPath = null;
        private PathGradientBrush backgroundPthGrBrush = null;
        private WhorlDesign design;
        private GradientColors gradientColors;

        //private string backgroundImageFileName { get; set; }

        private WhorlDesign previewDesign = new WhorlDesign();

        private PathColorTypes pathColorType;

        public Color TransparencyColor
        {
            get
            {
                return pathColorType == PathColorTypes.Center ?
                       btnCenterColor.BackColor : btnBoundaryColor.BackColor;
            }
            set
            {
                if (pathColorType == PathColorTypes.Center)
                    btnCenterColor.BackColor = gradientColors.CenterColor = value;
                else
                    btnBoundaryColor.BackColor = gradientColors.BoundaryColor = value;
                Preview();
            }
        }

        public void Initialize(WhorlDesign design)
        {
            this.design = design;
            gradientColors = (GradientColors)design.BackgroundGradientColors.Clone();
            btnBoundaryColor.BackColor = gradientColors.BoundaryColor;
            btnCenterColor.BackColor = gradientColors.CenterColor;
            chkSolidColor.Checked = (gradientColors.BoundaryColor == gradientColors.CenterColor);
            chkUseBackgroundImage.Checked = 
                !string.IsNullOrEmpty(design.BackgroundImageFileName);
            txtBackgroundImageFileName.Text = design.BackgroundImageFileName;
            cboImageMode.SelectedItem = design.BackgroundImageMode;
            Preview();
        }

        private bool EditColor(Button colorButton)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = colorButton.BackColor;
            bool retVal = (dlg.ShowDialog() == DialogResult.OK);
            if (retVal)
            {
                colorButton.BackColor = dlg.Color;
            }
            return retVal;
        }

        private void PopulateDesign(WhorlDesign targetDesign)
        {
            targetDesign.BackgroundGradientColors = gradientColors;
            if (chkSolidColor.Checked)
                targetDesign.BackgroundGradientColors.CenterColor =
                        gradientColors.BoundaryColor;
            targetDesign.BackgroundImageFileName = GetBackgroundImageFileName();
            targetDesign.BackgroundImageMode = (TextureImageModes)cboImageMode.SelectedItem;
        }

        private void Preview()
        {
            PopulateDesign(previewDesign);
            if (picPreview.Image != null)
                picPreview.Image.Dispose();
            picPreview.Image = previewDesign.CreateDesignBitmap(
                picPreview.ClientRectangle.Width, picPreview.ClientRectangle.Height,
                ref backgroundGrPath, ref backgroundPthGrBrush);
        }

        private string GetBackgroundImageFileName()
        {
            return chkUseBackgroundImage.Checked ? txtBackgroundImageFileName.Text : null;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            PopulateDesign(design);
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void chkSolidColor_CheckedChanged(object sender, EventArgs e)
        {
            bool solidColor = chkSolidColor.Checked;
            lblCenterColor.Visible = btnCenterColor.Visible = !solidColor;
            lblBoundaryColor.Text = solidColor ? "Solid Color:" : "Boundary Color:";
            if (solidColor)
            {
                gradientColors.CenterColor = gradientColors.BoundaryColor;
                btnCenterColor.BackColor = gradientColors.CenterColor;
            }
            Preview();
        }

        private Button clickedColorButton;

        private void chooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainForm.SelectColorForm.Initialize();
                if (MainForm.SelectColorForm.ShowDialog() == DialogResult.OK)
                {
                    if (MainForm.SelectColorForm.SelectedColor != null)
                    {
                        clickedColorButton.BackColor = (Color)MainForm.SelectColorForm.SelectedColor;
                        SetGradientColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void addColorToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Color newColor = clickedColorButton.BackColor;
                if (!MainForm.SelectColorForm.PatternChoices.AddColor(newColor))
                    MessageBox.Show("This color has already been added to the color choices.", 
                                    "Message");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnSwapColors_Click(object sender, EventArgs e)
        {
           try
            {
                Color saveColor = btnBoundaryColor.BackColor;
                btnBoundaryColor.BackColor = gradientColors.BoundaryColor = btnCenterColor.BackColor;
                btnCenterColor.BackColor = gradientColors.CenterColor = saveColor;
                Preview();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetGradientColor()
        {
            bool setCenter = clickedColorButton == this.btnCenterColor;
            if (setCenter)
                gradientColors.CenterColor = clickedColorButton.BackColor;
            else
                gradientColors.BoundaryColor = clickedColorButton.BackColor;
            Preview();
        }

        private void ColorButton_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                clickedColorButton = (Button)sender;
                pathColorType = clickedColorButton == btnCenterColor ? 
                                PathColorTypes.Center : PathColorTypes.Boundary;
                if (e.Button == MouseButtons.Left)
                {
                    if (EditColor((Button)sender))
                    {
                        SetGradientColor();
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Show(clickedColorButton, e.X, e.Y);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private ColorTransparencyForm transparencyForm = null;

        private void setTransparencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (transparencyForm == null || transparencyForm.IsDisposed)
                {
                    transparencyForm = new ColorTransparencyForm();
                    transparencyForm.Initialize(this);
                    transparencyForm.Show();
                }
                else
                    transparencyForm.WindowState = FormWindowState.Normal;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void GradientColorsForm_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (!this.Visible)
                {
                    if (transparencyForm != null && !transparencyForm.IsDisposed)
                        transparencyForm.Close();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnBrowseBackgroundImageFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Image file (*.png;*.jpg)|*.png;*.jpg;*.jpeg";
                dlg.InitialDirectory = WhorlSettings.Instance.TexturesFolder;
                if (!string.IsNullOrEmpty(txtBackgroundImageFileName.Text))
                    dlg.FileName = Path.GetFileName(txtBackgroundImageFileName.Text);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtBackgroundImageFileName.Text = dlg.FileName;
                    Preview();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void chkUseBackgroundImage_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                pnlColor.Enabled = !chkUseBackgroundImage.Checked;
                pnlImage.Enabled = chkUseBackgroundImage.Checked;
                Preview();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
