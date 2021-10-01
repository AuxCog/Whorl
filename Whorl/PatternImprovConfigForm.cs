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
    public partial class PatternImprovConfigForm : Form
    {
        private enum ImprovFlagValues
        {
            Default,
            On,
            Off
        }

        public PatternImprovConfigForm()
        {
            InitializeComponent();
            colorGradientFormComponent = new ColorGradientFormComponent(picGradient, this, lockGradient: true);
            improvFlagComboBoxes = new ComboBox[] {cboImprovOnColors, cboImprovOnOutlineTypes,
                                                   cboImprovOnParameters, cboImprovOnPetals,
                                                   cboImprovOnShapes };
            foreach (var cbo in improvFlagComboBoxes)
            {
                cbo.DataSource = Enum.GetValues(typeof(ImprovFlagValues));
            }
            cboSetAllFlags.DataSource = Enum.GetValues(typeof(ImprovFlagValues));
            cboParameterType.DataSource = Enum.GetValues(typeof(ImproviseParameterType));
            dgvParameterConfigs.AutoGenerateColumns = false;
            cboDecimalPlaces.DataSource = Enumerable.Range(0, 7).ToList();
            dgvColors.AutoGenerateColumns = false;
            dgvColors.AllowUserToAddRows = dgvColors.AllowUserToDeleteRows = false;
        }

        private ColorGradientFormComponent colorGradientFormComponent { get; }
        private TextureFillInfo textureFillInfo;
        private ComboBox[] improvFlagComboBoxes;

        private PatternImproviseConfig patternImprovConfig;
        private Pattern targetPattern;
        private DataTable colorsDataTable;
        private bool usePixelRendering { get; set; }

        public void Initialize(Pattern pattern)
        {
            this.patternImprovConfig = pattern.PatternImproviseConfig;
            this.targetPattern = pattern;
            chkPatternEnabled.Checked = patternImprovConfig.Enabled;

            var improvFlags = patternImprovConfig.ImproviseFlags;
            SetImprovFlag(cboImprovOnColors, improvFlags.ImproviseColors);
            SetImprovFlag(cboImprovOnOutlineTypes, improvFlags.ImproviseOnOutlineType);
            SetImprovFlag(cboImprovOnParameters, improvFlags.ImproviseParameters);
            SetImprovFlag(cboImprovOnPetals, improvFlags.ImprovisePetals);
            SetImprovFlag(cboImprovOnShapes, improvFlags.ImproviseShapes);
            usePixelRendering = pattern.PixelRendering != null && pattern.PixelRendering.Enabled &&
                                pattern.PixelRendering.ColorNodes != null;
            int rangeCount = usePixelRendering ? 1 : pattern.PatternLayers.PatternLayers.Count;
            cboLayerIndex.DataSource = Enumerable.Range(1, rangeCount).ToList();
            if (rangeCount > 0)
                cboLayerIndex.SelectedItem = 1;
            if (patternImprovConfig.ParameterConfigs.Count > 0)
            {
                cboParameterType.SelectedItem = 
                    patternImprovConfig.ParameterConfigs.First().ParameterType;
            }
            FilterByParameterType();
        }

        private void SetImprovFlag(ComboBox cbo, bool? flag)
        {
            if (flag == null)
                cbo.SelectedItem = ImprovFlagValues.Default;
            else
                cbo.SelectedItem = (bool)flag ? ImprovFlagValues.On : ImprovFlagValues.Off;
        }

        private bool? GetImprovFlag(ComboBox cbo)
        {
            var flag = (ImprovFlagValues)cbo.SelectedItem;
            if (flag == ImprovFlagValues.Default)
                return null;
            else
                return flag == ImprovFlagValues.On;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                patternImprovConfig.Enabled = chkPatternEnabled.Checked;

                var improvFlags = patternImprovConfig.ImproviseFlags;
                improvFlags.ImproviseColors = GetImprovFlag(cboImprovOnColors);
                improvFlags.ImproviseOnOutlineType = GetImprovFlag(cboImprovOnOutlineTypes);
                improvFlags.ImproviseParameters = GetImprovFlag(cboImprovOnParameters);
                improvFlags.ImprovisePetals = GetImprovFlag(cboImprovOnPetals);
                improvFlags.ImproviseShapes = GetImprovFlag(cboImprovOnShapes);
                improvFlags.SetUsedFlags();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnSetAllFlags_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboSetAllFlags.SelectedItem is ImprovFlagValues)
                {
                    var val = (ImprovFlagValues)cboSetAllFlags.SelectedItem;
                    foreach (var cbo in improvFlagComboBoxes)
                    {
                        cbo.SelectedItem = val;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnResetFromPattern_Click(object sender, EventArgs e)
        {
            try
            {
                patternImprovConfig.PopulateFromPattern(targetPattern, reset: true);
                FilterByParameterType();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        ParameterImproviseConfig editedParamConfig;

        private void EditParamConfig(ParameterImproviseConfig paramConfig)
        {
            editedParamConfig = paramConfig;
            pnlEditParamConfig.Show();
            txtParameterValue.Text = $"{paramConfig.Parameter.Value}";
            txtMinValue.Text = $"{paramConfig.MinValue}";
            txtMaxValue.Text = $"{paramConfig.MaxValue}";
            txtImprovStrength.Text = $"{(paramConfig.ImprovStrength * 100):0.0}";
            cboDecimalPlaces.SelectedItem = paramConfig.DecimalPlaces;
        }

        private void dgvParameterConfigs_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex != colEdit.Index || e.RowIndex < 0)
                    return;
                var dgvRow = dgvParameterConfigs.Rows[e.RowIndex];
                var paramConfig = dgvRow.DataBoundItem as ParameterImproviseConfig;
                if (paramConfig != null)
                    EditParamConfig(paramConfig);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvParameterConfigs_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;
                var col = dgvParameterConfigs.Columns[e.ColumnIndex] as DataGridViewCheckBoxColumn;
                if (col != null)
                {
                    dgvParameterConfigs.EndEdit();
                    var dgvRow = dgvParameterConfigs.Rows[e.RowIndex];
                    var paramConfig = dgvRow.DataBoundItem as ParameterImproviseConfig;
                    if (paramConfig != null && col.DataPropertyName == "Enabled")
                        paramConfig.Enabled = (bool)dgvParameterConfigs.CurrentCell.Value;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void FilterByParameterType()
        {
            if (patternImprovConfig == null)
                return;
            if (cboParameterType.SelectedItem is ImproviseParameterType)
            {
                dgvParameterConfigs.DataSource = patternImprovConfig.ParameterConfigs.FindAll(
                    pc => pc.ParameterType == (ImproviseParameterType)cboParameterType.SelectedItem);
            }
            else
            {
                dgvParameterConfigs.DataSource = patternImprovConfig.ParameterConfigs;
            }
        }

        private void cboParameterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                FilterByParameterType();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private double? GetDouble(string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                double d;
                if (double.TryParse(s, out d))
                    return d;
            }
            return null;
        }

        private void btnApplyParameter_Click(object sender, EventArgs e)
        {
            try
            {
                var parameter = editedParamConfig.Parameter;
                double? minValue = GetDouble(txtMinValue.Text);
                if (minValue != null && parameter.MinValue != null
                    && (double)minValue < (double)parameter.MinValue)
                {
                    minValue = parameter.MinValue;
                    txtMinValue.Text = minValue.ToString();
                }
                editedParamConfig.MinValue = minValue;
                double? maxValue = GetDouble(txtMaxValue.Text);
                if (maxValue != null && parameter.MaxValue != null
                    && (double)maxValue > (double)parameter.MaxValue)
                {
                    maxValue = parameter.MaxValue;
                    txtMaxValue.Text = maxValue.ToString();
                }
                editedParamConfig.MaxValue = maxValue;
                double? strengthPercent = GetDouble(txtImprovStrength.Text);
                if (strengthPercent != null)
                    editedParamConfig.ImprovStrength = (double)strengthPercent / 100D;
                editedParamConfig.DecimalPlaces = (int)cboDecimalPlaces.SelectedItem;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PopulateColorsDataTable(int layerIndex)
        {
            if (colorsDataTable == null)
            {
                colorsDataTable = new DataTable();
                colorsDataTable.Columns.Add("Description");
                colorsDataTable.Columns.Add("Color", typeof(Color));
                colorsDataTable.Columns.Add("Enabled", typeof(bool));
            }
            else
                colorsDataTable.Rows.Clear();
            IColorNodeList colorParent = null;
            textureFillInfo = null;
            if (usePixelRendering)
            {
                colorParent = targetPattern.PixelRendering;
            }
            else if (layerIndex < targetPattern.PatternLayers.PatternLayers.Count)
            {
                PatternLayer patternLayer = targetPattern.PatternLayers.PatternLayers[layerIndex];
                colorParent = patternLayer.FillInfo as PathFillInfo;
                if (colorParent == null)
                    textureFillInfo = patternLayer.FillInfo as TextureFillInfo;
            }
            HashSet<int> colorIndices = patternImprovConfig.GetColorIndices(layerIndex);
            if (colorParent != null)
            {
                var colorNodes = colorParent.ColorNodes;
                for (int i = 0; i < colorNodes.Count; i++)
                {
                    string description = $"Color {i + 1}";
                    Color color = colorNodes.GetColorNode(i).Color;
                    bool enabled = colorIndices != null ? colorIndices.Contains(i) : false;
                    colorsDataTable.Rows.Add(description, color, enabled);
                }
                colorGradientFormComponent.SetPaintHandler();  //Sets to default.
                colorGradientFormComponent.ColorNodes = colorNodes;  //Causes paint event to fire.
            }
            else if (textureFillInfo != null)
            {
                string description = $"Texture: {Path.GetFileNameWithoutExtension(textureFillInfo.TextureImageFileName)}";
                bool enabled = colorIndices != null ? colorIndices.Contains(0) : false;
                colorsDataTable.Rows.Add(description, Color.White, enabled);
                colorGradientFormComponent.SetPaintHandler(picGradient_Paint);
                picGradient.Refresh();
            }
            //PatternLayer patternLayer = targetPattern.PatternLayers.PatternLayers[layerIndex];
            //PathFillInfo pathFillInfo = patternLayer.FillInfo as PathFillInfo;
            //HashSet<int> colorIndices = patternImprovConfig.GetColorIndices(layerIndex);
            //if (pathFillInfo != null)
            //{
            //    int colorCount = pathFillInfo.GetColorCount();
            //    for (int i = 0; i < colorCount; i++)
            //    {
            //        string description;
            //        Color color;
            //        if (i == 0)
            //        {
            //            description = "Boundary";
            //            color = pathFillInfo.BoundaryColor;
            //        }
            //        else if (i == colorCount - 1)
            //        {
            //            description = "Center";
            //            color = pathFillInfo.CenterColor;
            //        }
            //        else
            //        {
            //            description = $"Color {i}";
            //            color = pathFillInfo.ColorInfo.GetColorAtIndex(i);
            //        }
            //        bool enabled = colorIndices != null ? colorIndices.Contains(i) : false;
            //        colorsDataTable.Rows.Add(description, color, enabled);
            //    }
            //}
            //else
            //{
            //    TextureFillInfo textureFillInfo = patternLayer.FillInfo as TextureFillInfo;
            //    if (textureFillInfo != null)
            //    {
            //        string description = $"Texture: {Path.GetFileNameWithoutExtension(textureFillInfo.TextureImageFileName)}";
            //        bool enabled = colorIndices != null ? colorIndices.Contains(0) : false;
            //        colorsDataTable.Rows.Add(description, Color.White, enabled);
            //    }
            //}
            //}
        }

        private void dgvColors_Bind()
        {
            dgvColors.DataSource = null;
            dgvColors.DataSource = colorsDataTable;
        }

        private void dgvColors_SetColors()
        {
            if (colorsDataTable == null)
                return;
            for (int i = 0; i < dgvColors.Rows.Count; i++)
            {
                Color color = (Color)colorsDataTable.Rows[i]["Color"];
                dgvColors.Rows[i].Cells["colColor"].Style.BackColor = color;
            }
        }

        private void PatternLayerChanged()
        {
            if (cboLayerIndex.SelectedItem is int)
            {
                int layerIndex = (int)cboLayerIndex.SelectedItem - 1;
                PopulateColorsDataTable(layerIndex);
                dgvColors_Bind();
            }
        }

        private void cboLayerIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                PatternLayerChanged();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetAllColorsEnabled(bool enabled)
        {
            if (colorsDataTable != null)
            {
                foreach (DataRow dRow in colorsDataTable.Rows)
                {
                    dRow["Enabled"] = enabled;
                }
                dgvColors_Bind();
            }
        }

        private void btnSelectAllColors_Click(object sender, EventArgs e)
        {
            try
            {
                SetAllColorsEnabled(true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnClearAllColors_Click(object sender, EventArgs e)
        {
            try
            {
                SetAllColorsEnabled(false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void tabColors_Enter(object sender, EventArgs e)
        {
            try
            {
                dgvColors_SetColors();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnApplyColors_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorsDataTable == null || !(cboLayerIndex.SelectedItem is int))
                    return;
                int layerIndex = (int)cboLayerIndex.SelectedItem - 1;
                HashSet<int> colorIndices = patternImprovConfig.GetColorIndices(layerIndex);
                if (colorIndices == null)
                    return;
                colorIndices.Clear();
                for (int i = 0; i < colorsDataTable.Rows.Count; i++)
                {
                    if ((bool)colorsDataTable.Rows[i]["Enabled"])
                        colorIndices.Add(i);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnResetColorsFromPattern_Click(object sender, EventArgs e)
        {
            try
            {
                patternImprovConfig.PopulateColorIndicesFromPattern(targetPattern, checkCounts: false);
                PatternLayerChanged();
                dgvColors_SetColors();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picGradient_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (textureFillInfo != null)
                {
                    if (textureFillInfo.FillBrush == null)
                        textureFillInfo.CreateFillBrush();

                    e.Graphics.FillRectangle(textureFillInfo.FillBrush,
                                             new Rectangle(new Point(0, 0), picGradient.ClientSize));
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
