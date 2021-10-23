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
    public partial class FrmEditKeyEnumParameters : Form
    {
        public FrmEditKeyEnumParameters()
        {
            InitializeComponent();
        }

        private Pattern pattern { get; set; }
        public FormulaSettings FormulaSettings { get; private set; }
        public object ParamsObject { get; private set; }
        private CSharpParameterDisplay cSharpParameterDisplay { get; set; }
        public bool ParametersChanged { get; private set; }
        public bool ShouldDisplayParameters { get; private set; }

        private void FrmEditKeyEnumParameters_Load(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void parameterChanged(object sender, ParameterChangedEventArgs e)
        {
            ParametersChanged = true;
        }

        public void Initialize(Pattern pattern)
        {
            cSharpParameterDisplay = new CSharpParameterDisplay(pnlParameters, parameterChanged)
            {
                UpdateParametersObject = false
            };
            BaseParameterDisplay.ClearParametersControls(pnlParameters);
            cboEnumKey.DataSource = null;
            this.pattern = pattern;
            var categories = new List<ValueTextItem>() { new ValueTextItem(null, string.Empty) };
            if (pattern.HasPixelRendering
                && pattern.PixelRendering.FormulaSettings != null
                && pattern.PixelRendering.FormulaSettings.HasKeyEnumParameters)
            {
                categories.Add(new ValueTextItem(pattern.PixelRendering, "PixelRendering"));
            }
            categories.AddRange(pattern.Transforms.Where(t => t.TransformSettings.HasKeyEnumParameters)
                                .Select(t => new ValueTextItem(t, $"Transform: {t.TransformName}")));
            cboCategory.DataSource = categories;
            ParametersChanged = ShouldDisplayParameters = false;
        }

        private void OnChangeCategory()
        {
            FormulaSettings = GetFormulaSettings();
            var items = new List<object>() { string.Empty };
            items.AddRange(pattern.InfluencePointInfoList.KeyEnumParamsDict.Values
                                  .Where(v => v.Parent.FormulaSettings == FormulaSettings));
            cboEnumKey.DataSource = items;
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnChangeCategory();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private FormulaSettings GetFormulaSettings()
        {
            var item = cboCategory.SelectedItem as ValueTextItem;
            if (item == null) return null;
            var pixelRendering = item.Value as Pattern.RenderingInfo;
            if (pixelRendering != null)
                return pixelRendering.FormulaSettings;
            var transform = item.Value as PatternTransform;
            if (transform != null)
                return transform.TransformSettings;
            return null;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void DisplayParameters()
        {
            if (FormulaSettings == null) return;
            var keyParams = cboEnumKey.SelectedItem as KeyEnumParameters;
            if (keyParams?.ParametersObject == null)
                return;
            cSharpParameterDisplay.SetParametersObject(keyParams.ParametersObject);
            cSharpParameterDisplay.AddAllParametersControls(FormulaSettings);
        }

        private void cboEnumKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DisplayParameters();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnEditInMainForm_Click(object sender, EventArgs e)
        {
            try
            {
                var keyParams = cboEnumKey.SelectedItem as KeyEnumParameters;
                if (FormulaSettings == null || keyParams?.ParametersObject == null)
                {
                    MessageBox.Show("There is no parameters object to edit.");
                    return;
                }
                ParamsObject = keyParams.ParametersObject;
                ShouldDisplayParameters = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }
    }
}
