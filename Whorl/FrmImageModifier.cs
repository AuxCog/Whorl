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
    public partial class FrmImageModifier : Form
    {
        public FrmImageModifier()
        {
            InitializeComponent();
        }

        public ImageModifier ImageModifier { get; } = new ImageModifier();
        public WhorlDesign WhorlDesign { get; private set; }
        private MainForm mainForm { get; set; }
        private Bitmap ImageBitmap { get; set; }
        private Pattern[] outlinePatterns { get; set; }
        private float scale { get; set; }
        private Size pictureBoxSize { get; set; }

        private void FrmImageModifier_Load(object sender, EventArgs e)
        {
            try
            {
                cboBoundsMode.DataSource = Enum.GetValues(typeof(ImageModifier.BoundModes));
                cboColorMode.DataSource = Enum.GetValues(typeof(ImageModifier.ColorModes));
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(MainForm mainForm, WhorlDesign design, Bitmap bitmap, Size pictureBoxSize, 
                               IEnumerable<Pattern> outlinePatterns)
        {
            this.mainForm = mainForm;
            WhorlDesign = design;
            ImageBitmap = bitmap;
            this.pictureBoxSize = pictureBoxSize;
            scale = (float)bitmap.Height / pictureBoxSize.Height;
            this.outlinePatterns = outlinePatterns.ToArray();
        }

        private void BtnModifyImage_Click(object sender, EventArgs e)
        {
            try
            {
                var boundsMode = (ImageModifier.BoundModes)cboBoundsMode.SelectedItem;
                var colorMode = (ImageModifier.ColorModes)cboColorMode.SelectedItem;
                ImageModifier.ModifiedColor = picModifiedColor.BackColor;
                if (!chkCumulative.Checked || ImageModifier.ImageBitmap == null)
                {
                    ImageModifier.ImageBitmap = (Bitmap)ImageBitmap.Clone();
                }
                ImageModifier.ModifyColors(WhorlDesign, colorMode, boundsMode, outlinePatterns, scale, pictureBoxSize);
                mainForm.SetPictureBoxImage(ImageModifier.ImageBitmap);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picModifiedColor_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new ColorDialog())
                {
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                    picModifiedColor.BackColor = dlg.Color;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
