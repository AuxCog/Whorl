using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Whorl
{
    public class CSharpParameterDisplay : BaseParameterDisplay
    {
        public class ParamSourceInfo
        {
            public CSharpParameterDisplay Parent { get; }

            private int _distancePatternsCount = 0;
            public int DistancePatternsCount
            {
                get { return _distancePatternsCount; }
                set
                {
                    SetProperty(ref _distancePatternsCount, value, DistancePatternsCountPropInfo);
                }
            }
            public PropertyInfo DistancePatternsCountPropInfo { get; set; }
            public bool AutoPopulateControls { get; set; }

            private bool initialState { get; set; }

            public ParamSourceInfo(CSharpParameterDisplay parent)
            {
                Parent = parent;
            }

            public void SetInitial(bool initial)
            {
                initialState = initial;
                if (initial)
                {
                    _distancePatternsCount = 0;
                    DistancePatternsCountPropInfo = null;
                }
            }

            private void SetProperty<TValue>(ref TValue field, TValue value,
                                             PropertyInfo propertyInfo)
            {
                if (!Equals(field, value))
                {
                    field = value;
                    if (Parent.FormulaSettings != null && Parent.ParametersObject != null &&
                        propertyInfo != null)
                    {
                        propertyInfo.SetValue(Parent.ParametersObject, field);
                        if (!initialState)
                        {
                            var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
                            if (attr != null && attr.UpdateParametersOnChange)
                            {
                                Parent.UpdateParameters(AutoPopulateControls);
                            }
                        }
                    }
                }
            }

        }

        private class ArrayInfo
        {
            public Array ParamArray { get; }
            public int Index { get; }
            public PropertyInfo PropertyInfo { get; }

            public ArrayInfo(Array paramArray, int index, PropertyInfo propertyInfo)
            {
                ParamArray = paramArray;
                Index = index;
                PropertyInfo = propertyInfo;
            }
        }

        private class OptionsParamInfo
        {
            public IOptionsParameter OptionsParameter { get; }
            public PropertyInfo PropertyInfo { get; }

            public OptionsParamInfo(IOptionsParameter iOptParam, PropertyInfo propertyInfo)
            {
                OptionsParameter = iOptParam;
                PropertyInfo = propertyInfo;
            }
        }
        public ParamSourceInfo ParameterSourceInfo { get; }
        public object ParametersObject { get; private set; }
        private CSharpCompiledInfo CSharpCompiledInfo { get; set; }

        public CSharpParameterDisplay(Panel pnlParameters, ParamChanged paramChangedFn = null,
                                      ActionSelectedDelegate actionSelectedFn = null,
                                      bool singleColumn = false,
                                      int controlWidth = defaultControlWidth)
               : base(pnlParameters, paramChangedFn, actionSelectedFn, controlWidth, singleColumn)
        {
            ParameterSourceInfo = new ParamSourceInfo(this);
        }

        public override void SetFormulaSettings(FormulaSettings formulaSettings)
        {
            base.SetFormulaSettings(formulaSettings);
            CSharpCompiledInfo = formulaSettings.CSharpCompiledInfo;
            if (UpdateParametersObject)
                ParametersObject = formulaSettings.EvalInstance?.ParamsObj;
        }

        public void SetParametersObject(object paramsObj)
        {
            ParametersObject = paramsObj;
        }

        public void InitializeSources(FormulaSettings formulaSettings, Pattern sourcesPattern)
        {
            SetFormulaSettings(formulaSettings);
            if (ParametersObject == null)
                return;
            ParameterSourceInfo.SetInitial(true);
            try
            {
                foreach (PropertyInfo propertyInfo in ParametersObject.GetType().GetProperties()
                         .Where(pi => CSharpSharedCompiledInfo.ParamPropInfoIsValid(pi, allowAllParams: false)))
                {
                    var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
                    if (attr == null || attr.ParameterSource == ParameterSources.None)
                        continue;
                    object value = propertyInfo.GetValue(ParametersObject);
                    switch (attr.ParameterSource)
                    {
                        case ParameterSources.DistanceCount:
                            if (!(value is int))
                                continue;
                            ParameterSourceInfo.DistancePatternsCountPropInfo = propertyInfo;
                            ParameterSourceInfo.DistancePatternsCount = (int)value;
                            break;
                        default:
                            throw new Exception($"ParameterSource {attr.ParameterSource} not implemented.");
                    }
                }
            }
            finally
            {
                ParameterSourceInfo.SetInitial(false);
            }
            if (sourcesPattern.HasPixelRendering)
            {
                ParameterSourceInfo.DistancePatternsCount =
                    sourcesPattern.PixelRendering.GetDistancePathsCount();
            }
        }

        public override void AddAllParametersControls(FormulaSettings formulaSettings, 
                                                      Pattern sourcesPattern = null)
        {
            if (sourcesPattern != null)
                InitializeSources(formulaSettings, sourcesPattern);
            AddAllParametersControls1(formulaSettings);
        }

        private void AddAllParametersControls1(FormulaSettings formulaSettings)
        {
            try
            {
                handleEvents = false;
                base.AddAllParametersControls(formulaSettings); //Also calls SetFormulaSettings()
                if (ParametersObject == null)
                    return;
                int parameterIndex = 0;
                AddActionComboBox(ref parameterIndex);
                foreach (PropertyInfo propertyInfo in ParametersObject.GetType().GetProperties()
                         .Where(pi => CSharpSharedCompiledInfo.ParamPropInfoIsValid(pi)))
                {
                    object oParam = propertyInfo.GetValue(ParametersObject);
                    if (oParam == null)
                        continue;
                    if (oParam.GetType().IsArray)
                    {
                        var paramArray = oParam as Array;
                        for (int i = 0; i < paramArray.Length; i++)
                        {
                            var arrayInfo = new ArrayInfo(paramArray, i, propertyInfo);
                            object elemParam = paramArray.GetValue(i);
                            AddParameterControls(elemParam, ref parameterIndex, propertyInfo, arrayInfo);
                        }
                    }
                    else
                    {
                        AddParameterControls(oParam, ref parameterIndex, propertyInfo);
                    }
                }
            }
            finally
            {
                handleEvents = true;
            }
        }

        private void AddParameterControls(object oParam, ref int parameterIndex, PropertyInfo propertyInfo, 
                                          ArrayInfo arrayInfo = null)
        {
            if (oParam == null)
                return;
            IOptionsParameter iOptParam = oParam as IOptionsParameter;
            if (iOptParam != null)
            {
                FormulaSettings.ConfigureCSharpInfluenceParameter(oParam);
            }
            string labelText = GetCSharpParameterLabel(propertyInfo, arrayInfo == null ? -1 : arrayInfo.Index);
            int labelSpan, editSpan;
            var label = new Label();
            int labelWidth = TextRenderer.MeasureText(labelText, label.Font).Width;
            if (oParam is bool)
            {
                labelWidth += 50;  //Checkbox.
                editSpan = 0;
            }
            else
                editSpan = 1;
            labelWidth += critLeftMargin;
            if (labelWidth > critColumnWidth && !SingleColumn)
                labelSpan = 2;
            else
                labelSpan = 1;
            int comboboxWidth;
            if (iOptParam?.OptionTexts == null || !iOptParam.OptionTexts.Any())
                comboboxWidth = 0;
            else
            {
                var cbo = new ComboBox();
                comboboxWidth = 20 + iOptParam.OptionTexts.Select(s => TextRenderer.MeasureText(s, cbo.Font).Width).Max();
                if (comboboxWidth - critControlWidth >= 10)
                {
                    if (!SingleColumn)
                    {
                        editSpan = 2;
                        comboboxWidth = Math.Min(editSpan * critColumnWidth - critLeftMargin, comboboxWidth);
                    }
                }
                else
                    comboboxWidth = critControlWidth;
            }
            int columnsRequired;
            if (SingleColumn)
                columnsRequired = 2;
            else
            {
                int columnInd = parameterIndex % critColumnCount;
                columnsRequired = (labelSpan + editSpan);
                int columnsRemaining = critColumnCount - columnInd;
                if (columnsRequired > columnsRemaining)
                {
                    parameterIndex += columnsRemaining;  //Start new row.
                }
            }
            AddParameterControls(oParam, propertyInfo, arrayInfo, parameterIndex, labelText, labelSpan, comboboxWidth);
            parameterIndex += columnsRequired;
        }

        /// <summary>
        /// Add dynamic controls to panel for one parameter.
        /// </summary>
        private void AddParameterControls(object oParam, PropertyInfo propertyInfo, ArrayInfo arrayInfo, 
                     int parameterIndex, string labelText, int labelSpan, int comboboxWidth)
        {
            GetControlPosition(parameterIndex, out int left, out int top);
            Control ctl;
            ComboBox cbo = null;
            LinkLabel lnkInfluence = null;
            bool useLabel = true;
            //TextBox customTextBox = null;
            object controlTag;
            if (arrayInfo != null)
                controlTag = arrayInfo;
            else
                controlTag = propertyInfo;
            bool isNestedParamsParam = propertyInfo.GetCustomAttribute<NestedParametersAttribute>() != null;
            string selectedItem = null;
            var iOptionsParam = oParam as IOptionsParameter;
            if (iOptionsParam != null)
            {
                if (arrayInfo == null)
                    controlTag = new OptionsParamInfo(iOptionsParam, propertyInfo);
                cbo = new ComboBox();
                cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                cbo.DataSource = iOptionsParam.OptionTexts;
                cbo.Width = comboboxWidth;
                selectedItem = iOptionsParam.SelectedText;
                ctl = cbo;
            }
            else if (oParam is bool)
            {
                useLabel = false;
                var chk = new CheckBox();
                chk.Text = labelText;
                chk.Checked = (bool)oParam;
                chk.CheckedChanged += ParametersCheckBox_CheckChanged;
                ctl = chk;
            }
            else if (isNestedParamsParam)
            {
                var LnkEditNestedParams = new LinkLabel() { Text = "Edit..." };
                ctl = LnkEditNestedParams;
                LnkEditNestedParams.Click += LnkEditNestedParams_Click;
            }
            else
            {
                var txtBox = new TextBox();
                txtBox.Text = GetValueText(oParam);
                txtBox.KeyPress += ParametersTextBox_KeyPress;
                txtBox.Leave += ParametersTextBox_Leave;
                int txtWidth = critControlWidth;
                ctl = txtBox;
                if (propertyInfo != null && 
                    (propertyInfo.PropertyType == typeof(double) || 
                     propertyInfo.PropertyType == typeof(double[])))
                {
                    if (FnEditInfluenceLink != null)
                    {
                        string key = propertyInfo.Name;
                        if (arrayInfo != null)
                            key += $"[{arrayInfo.Index + 1}]";
                        lnkInfluence = CreateInfluenceLinkLabel(key, ref txtWidth);
                    }
                }
                txtBox.Width = txtWidth;
            }
            if (useLabel)
            {
                AddLabel(labelText, labelSpan, top, ref left);
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
            //if (customTextBox != null)
            //{
            //    customTextBox.Top = top;
            //    customTextBox.Left = left;
            //    ParametersPanel.Controls.Add(customTextBox);
            //}
            if (cbo != null)
            {
                if (selectedItem != null)
                {
                    cbo.SelectedItem = selectedItem;
                }
                cbo.SelectedIndexChanged += ParametersComboBox_SelectedIndexChanged;
            }
            var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
            if (attr != null && !attr.Enabled)
                ctl.Enabled = false;
        }

        private FrmNestedParameters frmNestedParameters { get; set; }

        private void LnkEditNestedParams_Click(object sender, EventArgs e)
        {
            try
            {
                var linkLabel = (LinkLabel)sender;
                var propertyInfo = (PropertyInfo)linkLabel.Tag;
                if (frmNestedParameters == null || frmNestedParameters.IsDisposed)
                    frmNestedParameters = new FrmNestedParameters();
                frmNestedParameters.Initialize(propertyInfo, FormulaSettings, ParamChangedFn);
                Tools.DisplayForm(frmNestedParameters);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public override void RefreshComboBoxes()
        {
            try
            {
                handleEvents = false;
                foreach (Control ctl in ParametersPanel.Controls)
                {
                    var cbo = ctl as ComboBox;
                    if (cbo != null)
                    {
                        var iOptParam = GetParamObj(cbo.Tag, throwException: false) as IOptionsParameter;
                        if (iOptParam != null)
                            cbo.SelectedItem = iOptParam.SelectedText;
                    }
                }
            }
            finally
            {
                handleEvents = true;
            }
        }

        private string GetValueText(object value)
        {
            if (value == null)
                return string.Empty;
            string sVal = value.ToString();
            if (double.TryParse(sVal, out double val))
            {
                sVal = Math.Round(val, 5).ToString();
            }
            return sVal;
        }

        public bool UpdateParameters(bool addControls = true)
        {
            if (FormulaSettings?.EvalInstance != null)
            {
                if (FormulaSettings.EvalInstance.UpdateParameters())
                {
                    if (addControls)
                        AddAllParametersControls1(FormulaSettings);
                    return true;
                }
            }
            return false;
        }

        private void OnParameterChanged(object paramInfo, PropertyInfo propertyInfo, bool refreshDisplay = true)
        {
            if (handleEvents)
            {
                var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
                bool updateParams = attr != null && attr.UpdateParametersOnChange;
                if (updateParams)
                {
                    refreshDisplay = false;
                    UpdateParameters();
                }
                base.OnParameterChanged(paramInfo, refreshDisplay);
            }
        }

        private object GetParamObj(object controlTag, bool throwException = true)
        {
            return GetParamObj(controlTag, out _, out _, throwException);
        }

        public object GetParamObj(object controlTag, out PropertyInfo propertyInfo, out Type paramType, 
                                  bool throwException = true)
        {
            propertyInfo = controlTag as PropertyInfo;
            if (propertyInfo != null)
            {
                paramType = propertyInfo.PropertyType;
                return propertyInfo.GetValue(ParametersObject);
            }
            var optParamInfo = controlTag as OptionsParamInfo;
            if (optParamInfo != null)
            {
                propertyInfo = optParamInfo.PropertyInfo;
                paramType = propertyInfo.PropertyType;
                return optParamInfo.OptionsParameter;
            }
            var arrayInfo = controlTag as ArrayInfo;
            if (arrayInfo != null)
            {
                propertyInfo = arrayInfo.PropertyInfo;
                paramType = arrayInfo.ParamArray.GetType().GetElementType();
                return arrayInfo.ParamArray.GetValue(arrayInfo.Index);
            }
            if (throwException)
                throw new Exception("Invalid control tag.");
            else
            {
                paramType = null;
                return null;
            }
        }

        //public string GetParamName(object controlTag)
        //{
        //    GetParamObj(controlTag, out PropertyInfo propertyInfo, out Type paramType);
        //    var arrayInfo = controlTag as ArrayInfo;
        //    string paramName = propertyInfo.Name;
        //    if (arrayInfo != null)
        //        paramName += $"[{arrayInfo.Index + 1}]";
        //    return paramName;
        //}

        private void SetParamValue(object controlTag, object value)
        {
            var propertyInfo = controlTag as PropertyInfo;
            if (propertyInfo != null)
                propertyInfo.SetValue(ParametersObject, value);
            else
            {
                var arrayInfo = controlTag as ArrayInfo;
                if (arrayInfo != null)
                    arrayInfo.ParamArray.SetValue(value, arrayInfo.Index);
                else
                    throw new Exception("Invalid call to SetParamValue.");
            }
        }

        private void ParametersCheckBox_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                var chkBox = (CheckBox)sender;
                bool val = (bool)GetParamObj(chkBox.Tag, out PropertyInfo propertyInfo, out Type paramType);
                if (val != chkBox.Checked)
                {
                    SetParamValue(chkBox.Tag, chkBox.Checked);
                    OnParameterChanged(chkBox.Tag, propertyInfo);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void CustomParamComboBoxChanged(ComboBox cbo)
        //{
        //    var comboBoxInfo = cbo.Tag as ComboBoxInfo;
        //    if (comboBoxInfo != null)
        //    {
        //        var item = cbo.SelectedItem as PropertyInfoItem;
        //        if (item != null && item.PropertyInfo != null)
        //        {
        //            comboBoxInfo.CustomParameter.SelectedPropertyInfo = item.PropertyInfo;
        //            TextBox textBox = comboBoxInfo.RelatedTextBox;
        //            textBox.Tag = new PropertyTextBoxInfo(comboBoxInfo.CustomParameter, item.PropertyInfo);
        //            textBox.Visible = true;
        //            textBox.Top = cbo.Top;
        //            object val = item.PropertyInfo.GetValue(comboBoxInfo.CustomParameter.Context);
        //            textBox.Text = val == null ? string.Empty : val.ToString();
        //        }
        //        else
        //        {
        //            comboBoxInfo.RelatedTextBox.Visible = false;
        //        }
        //    }
        //}

        private void ParametersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                var cbo = (ComboBox)sender;
                if (cbo.SelectedItem == null)
                    return;
                var iOptionsParam = GetParamObj(cbo.Tag, out PropertyInfo propertyInfo, out Type paramType) as IOptionsParameter;
                if (iOptionsParam != null)
                {
                    object selOption = iOptionsParam.GetOptionByText((string)cbo.SelectedItem);
                    if (iOptionsParam.SelectedOptionObject != selOption)
                    {
                        iOptionsParam.SelectedOptionObject = selOption;
                        OnParameterChanged(cbo.Tag, propertyInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetParameterFromTextBox(TextBox txtBox, bool refresh)
        {
            if (!handleEvents)
                return;
            object previousVal = GetParamObj(txtBox.Tag, out PropertyInfo propertyInfo, out Type paramType);
            object val = Tools.GetCSharpParameterValue(txtBox.Text, paramType, out bool isValid);

            //bool isValid;
            //MethodInfo tryParseMethod = paramType.GetTryParseMethod();
            //if (tryParseMethod != null)
            //{
            //    var oParams = new object[] { txtBox.Text, null };
            //    isValid = (bool)tryParseMethod.Invoke(null, oParams);
            //    if (isValid)
            //        val = oParams[1];
            //    else
            //        val = null;
            //}
            //else
            //{
            //    try
            //    {
            //        val = Convert.ChangeType(txtBox.Text, paramType);
            //        isValid = true;
            //    }
            //    catch
            //    {
            //        val = null;
            //        isValid = false;
            //    }
            //}
            if (isValid && val != null)
            {
                var attr = propertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
                if (attr != null && paramType.IsValueType && double.TryParse(val.ToString(), out double dVal))
                {
                    isValid = dVal >= attr.MinValue && dVal <= attr.MaxValue;
                }
            }
            if (isValid)
            {
                if (!object.Equals(previousVal, val))
                {
                    SetParamValue(txtBox.Tag, val);
                    OnParameterChanged(txtBox.Tag, propertyInfo, refresh);
                }
            }
            else
                txtBox.Text = previousVal == null ? string.Empty : previousVal.ToString();
        }

        private void ParametersTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == '\r')
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
                SetParameterFromTextBox((TextBox)sender, refresh: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
