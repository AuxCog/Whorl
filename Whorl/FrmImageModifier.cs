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
        //private Size pictureBoxSize { get; set; }
        private int stepNumber 
        {
            get => WhorlDesign.ImageModifySettings.StepNumber;
            set => WhorlDesign.ImageModifySettings.StepNumber = value;
        }

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
            if (WhorlDesign.ImageModifySettings == null)
            {
                WhorlDesign.ImageModifySettings = new ImageModifySettings();
            }
            if (WhorlDesign.ImageModifySettings.Steps.Count == 0)
            {
                WhorlDesign.ImageModifySettings.Steps.Add(new ImageModifyStepSettings());
            }
            FillCboGoToStep();
            ImageBitmap = bitmap;
            scale = (float)bitmap.Height / pictureBoxSize.Height;
            this.outlinePatterns = outlinePatterns.ToArray();
        }

        private void FillCboGoToStep()
        {
            int stepsCount = WhorlDesign.ImageModifySettings.Steps.Count;
            cboGoToStep.DataSource = Enumerable.Range(1, stepsCount).ToArray();
            if (stepNumber > 0 && stepNumber <= stepsCount)
            {
                cboGoToStep.SelectedItem = stepNumber;
            }
        }

        private void BtnModifyImage_Click(object sender, EventArgs e)
        {
            try
            {
                var boundsMode = (ImageModifier.BoundModes)cboBoundsMode.SelectedItem;
                var colorMode = (ImageModifier.ColorModes)cboColorMode.SelectedItem;
                ImageModifier.ModifiedColor = picModifiedColor.BackColor;
                int stepsCount = WhorlDesign.ImageModifySettings.Steps.Count;
                if (!chkCumulative.Checked || ImageModifier.ImageBitmap == null)
                {
                    ImageModifier.ImageBitmap = (Bitmap)ImageBitmap.Clone();
                    if (stepNumber > 1)
                        WhorlDesign.ImageModifySettings.Steps.RemoveRange(0, stepNumber - 1);
                    stepNumber = 1;
                }
                else
                    stepNumber++;
                bool isNewStep = stepNumber > stepsCount;
                ImageModifyStepSettings stepSettings;
                if (isNewStep)
                    stepSettings = new ImageModifyStepSettings();
                else
                    stepSettings = WhorlDesign.ImageModifySettings.Steps[stepNumber - 1];
                stepSettings.BoundMode = boundsMode;
                stepSettings.ColorMode = colorMode;
                stepSettings.ModifiedColor = ImageModifier.ModifiedColor;
                stepSettings.OutlinePatterns = outlinePatterns;
                if (isNewStep)
                {
                    WhorlDesign.ImageModifySettings.Steps.Add(stepSettings);
                    FillCboGoToStep();
                }
                ImageModifier.ModifyColors(WhorlDesign, colorMode, boundsMode, outlinePatterns, scale);
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
                    dlg.Color = picModifiedColor.BackColor;
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

        private void cboGoToStep_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboGoToStep.SelectedItem is int)
                {
                    int stepN = (int)cboGoToStep.SelectedItem;
                    if (stepN == stepNumber || stepN > WhorlDesign.ImageModifySettings.Steps.Count)
                        return;
                    stepNumber = stepN;
                    var stepSettings = WhorlDesign.ImageModifySettings.Steps[stepNumber - 1];
                    cboBoundsMode.SelectedItem = stepSettings.BoundMode;
                    cboColorMode.SelectedItem = stepSettings.ColorMode;
                    picModifiedColor.BackColor = stepSettings.ModifiedColor;
                    outlinePatterns = stepSettings.OutlinePatterns;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
