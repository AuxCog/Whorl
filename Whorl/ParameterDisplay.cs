using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public class ParameterDisplay: BaseParameterDisplay
    {
        private class PropertyInfoItem
        {
            public PropertyInfo PropertyInfo { get; }

            public PropertyInfoItem(PropertyInfo propertyInfo)
            {
                PropertyInfo = propertyInfo;
            }

            public override string ToString()
            {
                return PropertyInfo == null ? string.Empty : PropertyInfo.Name;
            }
        }

        private class ComboBoxInfo
        {
            public CustomParameter CustomParameter { get; }
            public TextBox RelatedTextBox { get; }

            public ComboBoxInfo(CustomParameter customParameter, TextBox relatedTextBox)
            {
                CustomParameter = customParameter;
                RelatedTextBox = relatedTextBox;
            }
        }

        private class PropertyTextBoxInfo
        {
            public PropertyInfo PropertyInfo { get; }
            public CustomParameter CustomParameter { get; }

            public PropertyTextBoxInfo(CustomParameter customParameter, PropertyInfo propertyInfo)
            {
                CustomParameter = customParameter;
                PropertyInfo = propertyInfo;
            }
        }

        public ParameterDisplay(Panel pnlParameters, ParamChanged paramChangedFn, 
                                ActionSelectedDelegate actionSelectedFn = null,
                                int controlWidth = defaultControlWidth) 
               : base(pnlParameters, paramChangedFn, actionSelectedFn, controlWidth)
        {
        }

        public override void AddAllParametersControls(FormulaSettings formulaSettings,
                                                      Pattern sourcesPattern = null)
        {
            const int editSpan = 1;
            try
            {
                handleEvents = false;
                base.AddAllParametersControls(formulaSettings);
                int parameterIndex = 0;
                AddActionComboBox(ref parameterIndex);
                var label = new Label();
                foreach (BaseParameter baseParam in formulaSettings.BaseParameters)
                {
                    var customParam = baseParam as CustomParameter;
                    if (customParam != null && customParam.CustomType != CustomParameterTypes.RandomRange)
                        continue;
                    //var param = baseParam as Parameter;
                    //if (param != null && param.ForInfluenceValue)
                    //    continue;
                    formulaSettings.ConfigureInfluenceParameter(baseParam);
                    string paramLabel = GetParameterLabel(baseParam);
                    int textWidth = TextRenderer.MeasureText(paramLabel, label.Font).Width;
                    int labelSpan;
                    if (textWidth > critControlWidth - critLeftMargin)
                    {
                        labelSpan = 3;
                    }
                    else
                        labelSpan = 1;
                    int columnInd = parameterIndex % critColumnCount;
                    int columnsRequired = labelSpan + editSpan;
                    int columnsRemaining = critColumnCount - columnInd;
                    if (customParam != null)
                    {
                        if (columnsRemaining < critColumnCount)
                            parameterIndex += columnsRemaining;
                        columnsRemaining = critColumnCount;
                    }
                    else if (columnsRequired > columnsRemaining)
                    {
                        parameterIndex += columnsRemaining;  //Start new row.
                    }
                    if (AddParameterControls(ParametersPanel, baseParam, parameterIndex, labelSpan))
                        parameterIndex += columnsRequired;
                }
            }
            finally
            {
                handleEvents = true;
            }
        }

        public override void RefreshComboBoxes()
        {
            foreach (Control ctl in ParametersPanel.Controls)
            {
                var cbo = ctl as ComboBox;
                if (cbo != null)
                {
                    var baseParameter = ctl.Tag as BaseParameter;
                    if (baseParameter != null && baseParameter.HasChoices)
                        cbo.SelectedItem = baseParameter.GetValueChoice(baseParameter.SelectedChoice?.ParentObject);
                }
            }
        }

        /// <summary>
        /// Add dynamic controls to panel for one parameter.
        /// </summary>
        /// <param name="ParametersPanel"></param>
        /// <param name="baseParameter"></param>
        private bool AddParameterControls(Panel ParametersPanel, BaseParameter baseParameter, int parameterIndex, int labelSpan)
        {
            //if (baseParameter is CustomParameter)
            //    return false;
            GetControlPosition(parameterIndex, out int left, out int top);
            Control ctl;
            ComboBox cbo = null;
            LinkLabel lnkInfluence = null;
            //Label lbl = null;
            bool useLabel = true;
            TextBox customTextBox = null;
            var boolParam = baseParameter as BooleanParameter;
            var customParam = baseParameter as CustomParameter;
            object controlTag = baseParameter;
            object selectedItem = null;
            if (boolParam != null)
            {
                var chk = new CheckBox();
                chk.Text = baseParameter.GetLabel();
                chk.Checked = boolParam.BooleanValue;
                chk.CheckedChanged += ParametersCheckBox_CheckChanged;
                ctl = chk;
                useLabel = false;
            }
            else
            {
                if (customParam != null)
                {
                    if (customParam.CustomType != CustomParameterTypes.RandomRange)
                        return false;
                    cbo = new ComboBox();
                    cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                    var items = RandomRange.ParameterProperties.Select(
                                propInfo => new PropertyInfoItem(propInfo)).ToList();
                    items.Insert(0, new PropertyInfoItem(null));
                    cbo.DataSource = items;
                    cbo.Width = critControlWidth + 50;
                    selectedItem = items.Find(itm => itm.PropertyInfo == customParam.SelectedPropertyInfo);
                    ctl = cbo;
                    customTextBox = new TextBox();
                    customTextBox.KeyPress += ParametersTextBox_KeyPress;
                    customTextBox.Leave += ParametersTextBox_Leave;
                    customTextBox.Width = critControlWidth;
                    //customTextBox.Visible = false;
                    controlTag = new ComboBoxInfo(customParam, customTextBox);
                }
                else if (baseParameter.HasChoices)
                {
                    cbo = new ComboBox();
                    cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                    cbo.DataSource = baseParameter.ParameterChoices.ToList();
                    ctl = cbo;
                    ctl.Width = critControlWidth;
                    selectedItem = baseParameter.GetParameterChoice(baseParameter.SelectedText);
                }
                else
                {
                    var dblParam = baseParameter as DoubleParameter;
                    if (dblParam == null)
                        return false;
                    var txtBox = new TextBox();
                    txtBox.Text = GetValueText(dblParam.Value);
                    txtBox.KeyPress += ParametersTextBox_KeyPress;
                    txtBox.Leave += ParametersTextBox_Leave;
                    ctl = txtBox;
                    int txtWidth = critControlWidth;
                    var parameter = dblParam as Parameter;
                    if (FnEditInfluenceLink != null && parameter != null)
                    {
                        lnkInfluence = CreateInfluenceLinkLabel(parameter.ParameterName, ref txtWidth);
                    }
                    ctl.Width = txtWidth;
                }
            }
            if (useLabel)
            {
                AddLabel(GetParameterLabel(baseParameter), labelSpan, top, ref left);
                //lbl.AutoSize = false;
                //lbl.Text = baseParameter.GetLabel();
                //int textWidth = TextRenderer.MeasureText(lbl.Text, lbl.Font).Width;
                //int maxWidth = labelSpan * critControlWidth - critLeftMargin;
                //if (lbl.Width > maxWidth)
                //{
                //    lbl.AutoSize = false;
                //    lbl.Width = maxWidth;
                //}
                ////lbl.Width = Math.Min(labelSpan * critControlWidth - critLeftMargin, textWidth);
                //lbl.Top = top;
                //left += labelSpan * critControlWidth;
                //if (labelSpan > 1)
                //    left += critLeftMargin;
                //lbl.Left = left - lbl.Width - critLeftMargin;
                //ParametersPanel.Controls.Add(lbl);
            }
            ctl.Tag = controlTag;
            ctl.Top = top;
            ctl.Left = left;
            left += ctl.Width + critLeftMargin;
            ParametersPanel.Controls.Add(ctl);
            if (lnkInfluence != null)
            {
                lnkInfluence.Top = top;
                lnkInfluence.Left = left;
                ParametersPanel.Controls.Add(lnkInfluence);
            }
            else if (customTextBox != null)
            {
                customTextBox.Top = top;
                customTextBox.Left = left;
                ParametersPanel.Controls.Add(customTextBox);
            }
            if (cbo != null)
            {
                if (selectedItem != null)
                {
                    cbo.SelectedItem = selectedItem;
                    if (customParam != null)
                        CustomParamComboBoxChanged(cbo);
                }
                cbo.SelectedIndexChanged += ParametersComboBox_SelectedIndexChanged;
            }
            return true;
        }

        private string GetValueText(object value)
        {
            string sVal;
            if (value is double)
            {
                double val = (double)value;
                sVal = Math.Round(val, 5).ToString();
            }
            else
                sVal = string.Empty;
            return sVal;
        }

        private void ParametersCheckBox_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                var chkBox = (CheckBox)sender;
                BooleanParameter parameter = (BooleanParameter)chkBox.Tag;
                if (parameter.BooleanValue != chkBox.Checked)
                {
                    parameter.BooleanValue = chkBox.Checked;
                    OnParameterChanged(parameter);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void CustomParamComboBoxChanged(ComboBox cbo)
        {
            var comboBoxInfo = cbo.Tag as ComboBoxInfo;
            if (comboBoxInfo != null)
            {
                var item = cbo.SelectedItem as PropertyInfoItem;
                if (item != null && item.PropertyInfo != null)
                {
                    comboBoxInfo.CustomParameter.SelectedPropertyInfo = item.PropertyInfo;
                    TextBox textBox = comboBoxInfo.RelatedTextBox;
                    textBox.Tag = new PropertyTextBoxInfo(comboBoxInfo.CustomParameter, item.PropertyInfo);
                    textBox.Visible = true;
                    textBox.Top = cbo.Top;
                    object val = item.PropertyInfo.GetValue(comboBoxInfo.CustomParameter.Context);
                    textBox.Text = val == null ? string.Empty : val.ToString();
                }
                else
                {
                    comboBoxInfo.RelatedTextBox.Visible = false;
                }
            }
        }

        private void ParametersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                var cbo = (ComboBox)sender;
                if (cbo.SelectedItem == null)
                    return;
                BaseParameter parameter = cbo.Tag as BaseParameter;
                if (parameter != null)
                {
                    if (parameter.SetValueFromParameterChoice((ParameterChoice)cbo.SelectedItem))
                        OnParameterChanged(parameter);
                }
                else
                    CustomParamComboBoxChanged(cbo);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetParameterFromTextBox(TextBox txtBox, bool refresh)
        {
            var dblParam = txtBox.Tag as DoubleParameter;
            if (dblParam != null)
            {
                if (double.TryParse(txtBox.Text, out double val))
                {
                    if (!object.Equals(dblParam.Value, val))
                    {
                        dblParam.Value = val;
                        OnParameterChanged(dblParam);
                    }
                }
                else
                {
                    txtBox.Text = GetValueText(dblParam.Value);
                }
            }
            else
            {
                var propInfo = txtBox.Tag as PropertyTextBoxInfo;
                if (propInfo != null)
                {
                    object val;
                    object previousVal = propInfo.PropertyInfo.GetValue(propInfo.CustomParameter.Context);
                    try
                    {
                        val = Convert.ChangeType(txtBox.Text, propInfo.PropertyInfo.PropertyType);
                    }
                    catch
                    {
                        val = null;
                    }
                    if (val == null)
                    {
                        txtBox.Text = previousVal == null ? string.Empty : previousVal.ToString();
                    }
                    else
                    {
                        if (!object.Equals(previousVal, val))
                        {
                            propInfo.PropertyInfo.SetValue(propInfo.CustomParameter.Context, val);
                            OnParameterChanged(propInfo.CustomParameter);
                        }
                    }
                }
            }
        }

        private void ParametersTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (handleEvents && e.KeyChar == '\r')
                {
                    e.Handled = true;
                    SetParameterFromTextBox((TextBox)sender, refresh: true);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ParametersTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                if (handleEvents)
                    SetParameterFromTextBox((TextBox)sender, refresh: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
