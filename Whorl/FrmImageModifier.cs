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
        public Pattern[] OutlinePatterns { get; private set; }
        private float scale { get; set; }
        private bool ignoreEvents { get; set; }

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

        public void SetOutlinePatterns(IEnumerable<Pattern> outlinePatterns)
        {
            OutlinePatterns = outlinePatterns.ToArray();
        }

        public void Initialize(MainForm mainForm, WhorlDesign design, Bitmap bitmap, Size pictureBoxSize)
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
            else
            {
                DisplaySettings();
            }
            FillCboGoToStep();
            ImageBitmap = bitmap;
            scale = (float)bitmap.Height / pictureBoxSize.Height;
        }

        private void FillCboGoToStep()
        {
            int stepsCount = WhorlDesign.ImageModifySettings.Steps.Count;
            try
            {
                ignoreEvents = true;
                cboGoToStep.DataSource = Enumerable.Range(1, stepsCount).ToArray();
                if (stepNumber > 0 && stepNumber <= stepsCount)
                {
                    cboGoToStep.SelectedItem = stepNumber;
                }
            }
            finally
            {
                ignoreEvents = false;
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
                int newStepNumber;
                bool cloneImage = ImageModifier.ImageBitmap == null;
                if (!chkCumulative.Checked)
                {
                    cloneImage = true;
                    if (stepNumber > 1 && stepsCount > 1)
                        WhorlDesign.ImageModifySettings.Steps.RemoveRange(0, Math.Min(stepsCount - 1, stepNumber - 1));
                    stepNumber = newStepNumber = 1;
                }
                else
                    newStepNumber = stepNumber + 1;
                if (cloneImage)
                    ImageModifier.ImageBitmap = (Bitmap)ImageBitmap.Clone();
                bool isNewStep = stepNumber > stepsCount;
                ImageModifyStepSettings stepSettings;
                if (isNewStep)
                    stepSettings = new ImageModifyStepSettings();
                else
                    stepSettings = WhorlDesign.ImageModifySettings.Steps[stepNumber - 1];
                stepSettings.BoundMode = boundsMode;
                stepSettings.ColorMode = colorMode;
                stepSettings.ModifiedColor = ImageModifier.ModifiedColor;
                stepSettings.IsCumulative = chkCumulative.Checked;
                stepSettings.OutlinePatterns = OutlinePatterns;
                if (isNewStep)
                {
                    WhorlDesign.ImageModifySettings.Steps.Add(stepSettings);
                }
                stepsCount = WhorlDesign.ImageModifySettings.Steps.Count;
                if (newStepNumber > stepsCount)
                {
                    WhorlDesign.ImageModifySettings.Steps.Add(new ImageModifyStepSettings());
                }
                if (stepNumber != newStepNumber)
                {
                    stepNumber = newStepNumber;
                    FillCboGoToStep();
                }
                ImageModifier.ModifyColors(WhorlDesign, colorMode, boundsMode, OutlinePatterns, scale);
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

        private void DisplaySettings()
        {
            if (stepNumber > WhorlDesign.ImageModifySettings.Steps.Count)
                return;
            var stepSettings = WhorlDesign.ImageModifySettings.Steps[stepNumber - 1];
            cboBoundsMode.SelectedItem = stepSettings.BoundMode;
            cboColorMode.SelectedItem = stepSettings.ColorMode;
            picModifiedColor.BackColor = stepSettings.ModifiedColor;
            chkCumulative.Checked = stepSettings.IsCumulative;
            OutlinePatterns = stepSettings.OutlinePatterns;
        }

        private void cboGoToStep_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ignoreEvents) return;
                if (cboGoToStep.SelectedItem is int)
                {
                    int stepN = (int)cboGoToStep.SelectedItem;
                    if (stepN == stepNumber || stepN > WhorlDesign.ImageModifySettings.Steps.Count)
                        return;
                    stepNumber = stepN;
                    DisplaySettings();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
