using ParserEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Whorl
{
    public partial class frmVaryProperties : Form
    {
        private enum RunModes
        {
            Object,
            Parameter,
            CSharpParameter
        }
        private class PropertyAttributes
        {
            public PropertyInfo PropertyInfo { get; }
            public PropertyActionAttribute Attribute { get; }

            public PropertyAttributes(PropertyInfo prpInfo, PropertyActionAttribute attr)
            {
                PropertyInfo = prpInfo;
                Attribute = attr;
            }

            public string DisplayName
            {
                get
                {
                    if (Attribute != null && Attribute.Name != null)
                        return Attribute.Name;
                    else
                        return PropertyInfo.Name;
                }
            }
        }

        public frmVaryProperties()
        {
            InitializeComponent();
            tvProperties.BeforeExpand += TvProperties_BeforeExpand;
        }

        private RunModes runMode { get; set; }

        private List<object> undoValues { get; } = new List<object>();

        private int undoIndex { get; set; }

        private BasePropertyAction _basePropertyAction;
        public BasePropertyAction BasePropertyAction
        {
            get { return _basePropertyAction; }
            private set
            {
                Run(false);
                _basePropertyAction = value;
                if (_basePropertyAction == null)
                {
                    lblSelectedPropertyName.Text = "(none)";
                    txtPropertyValue.Text = string.Empty;
                }
                else
                {
                    _basePropertyAction.SetValue();
                    lblSelectedPropertyName.Text = _basePropertyAction.GetPropertyName();
                    txtPropertyValue.Text = txtRunPropertyValue.Text = _basePropertyAction.PropertyValue?.ToString();
                }
                pnlRun.Enabled = _basePropertyAction != null;
                undoValues.Clear();
                undoIndex = 0;
            }
        }

        private List<BasePropertyAction> runPropertyActions { get; } = new List<BasePropertyAction>();

        private PropertyAction PropertyAction
        {
            get { return BasePropertyAction as PropertyAction; }
        }

        private ParameterAction ParameterAction
        {
            get { return BasePropertyAction as ParameterAction; }
        }

        private PatternForm patternForm { get; set; }
        private Pattern targetPattern { get; set; }
        private Pattern previewPattern { get; set; }

        private object targetObject { get; set; }
        private object previewTarget { get; set; }

        private FormulaSettings targetFormulaSettings { get; set; }
        private FormulaSettings previewFormulaSettings { get; set; }

        private TreeNode clickedTreeNode { get; set; }

        private string parameterLabelText { get; set; }
        private TreeNode selectedParamTreeNode { get; set; }
        
        private void Initialize(Pattern targetPattern, PatternForm patternForm)
        {
            if (targetPattern == null)
                throw new NullReferenceException("targetPattern cannot be null.");
            this.patternForm = patternForm;
            previewPattern = targetPattern.GetCopy();
            previewPattern.SetForPreview(picPattern.ClientSize);
            picPattern.BackColor = previewPattern.GetPreviewBackColor();
            this.targetPattern = targetPattern;
            runPropertyActions.Clear();
        }

        public void Initialize(object targetObject, Pattern targetPattern, PatternForm patternForm)
        {
            try
            {
                runMode = RunModes.Object;
                Initialize(targetPattern, patternForm);
                if (targetObject == null)
                    throw new NullReferenceException("targetObject cannot be null.");
                var propInfoPath = new PropertyInfoPath();
                if (!propInfoPath.FindPropertyInfoPath(targetPattern, previewPattern, targetObject, includeNonPublic: true))
                {
                    throw new Exception("Couldn't find matching object in copied pattern.");
                }
                previewTarget = propInfoPath.Target2;
                lblTargetType.Text = previewTarget.GetType().Name;
                this.BasePropertyAction = BasePropertyAction as PropertyAction;
                if (this.targetObject?.GetType() != previewTarget.GetType())
                {
                    this.BasePropertyAction = null;
                }
                else if (this.BasePropertyAction != null)
                {
                    this.PropertyAction.SetSourceObject(previewTarget);
                    this.BasePropertyAction = this.BasePropertyAction;
                }
                this.targetObject = targetObject;
                targetFormulaSettings = null;
                parameterLabelText = null;
                PopulateTreeNodes();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void InitializeForParameters(FormulaSettings formulaSettings, Pattern targetPattern, PatternForm patternForm, 
                                            Label parameterLabel = null)
        {
            if (formulaSettings == null)
                throw new NullReferenceException("formulaSettings cannot be null.");
            parameterLabelText = parameterLabel?.Text;
            runMode = formulaSettings.IsCSharpFormula ? RunModes.CSharpParameter : RunModes.Parameter;
            lblTargetType.Text = "Parameters";
            Initialize(targetPattern, patternForm);
            this.targetFormulaSettings = formulaSettings;
            var propInfoPath = new PropertyInfoPath();
            if (!propInfoPath.FindPropertyInfoPath(targetPattern, previewPattern, targetFormulaSettings, includeNonPublic: true))
            {
                throw new CustomException("Couldn't find matching formula settings in copied pattern.");
            }
            previewFormulaSettings = (FormulaSettings)propInfoPath.Target2;
            if (runMode == RunModes.CSharpParameter)
            {
                targetObject = targetFormulaSettings.EvalInstance?.ParamsObj;
                previewTarget = previewFormulaSettings.EvalInstance?.ParamsObj;
                if (targetObject != null && previewTarget == null)
                {
                    throw new CustomException("Copied formula does not have parameters.");
                }
            }
            else
                previewTarget = null;
            this.BasePropertyAction = null;
            PopulateTreeNodes();
        }

        private void PopulateTreeNodes()
        {
            tvProperties.Nodes.Clear();
            switch (runMode)
            {
                case RunModes.Object:
                    PopulateTreeNodes(tvProperties.Nodes, previewTarget.GetType());
                    break;
                case RunModes.Parameter:
                    PopulateTreeNodes(tvProperties.Nodes, previewFormulaSettings.Parameters);
                    break;
                case RunModes.CSharpParameter:
                    if (targetObject != null)
                        PopulateTreeNodes(tvProperties.Nodes, targetObject.GetType());
                    break;
            }
            if (selectedParamTreeNode != null)
            {
                SetPropertyAction(selectedParamTreeNode);
                tabControl1.SelectedIndex = 1;  //Show Run tab.
            }
        }

        private TreeNode CreateTreeNode(string text)
        {
            var treeNode = new TreeNode(text);
            if (text == parameterLabelText)
                selectedParamTreeNode = treeNode;
            return treeNode;
        }

        private void PopulateTreeNodes(TreeNodeCollection treeNodeCollection, Type type)
        {
            if (Tools.IsListType(type))
                return;
            bool forObject = runMode == RunModes.Object;
            foreach (PropertyAttributes propAttr in GetFilteredProperties(type, forObject).OrderBy(pa => pa.DisplayName))
            {
                PropertyInfo propInfo = propAttr.PropertyInfo;
                bool createTreeNode = true;
                TreeNode emptyNode = null;
                if (forObject && propInfo.PropertyType.IsClass)
                {
                    if (Tools.IsListType(propInfo.PropertyType) ||
                        !GetFilteredProperties(propInfo.PropertyType, forObject).Any())
                    {
                        createTreeNode = false;  //If no included properties of class object, don't include in treeview.
                    }
                    else
                    {
                        emptyNode = new TreeNode("(Empty)");
                    }
                }
                if (createTreeNode)
                {
                    if (propInfo.PropertyType == typeof(double[]) && previewTarget != null)
                    {
                        Array array = propInfo.GetValue(previewTarget) as Array;
                        if (array != null)
                        {
                            for (int i = 0; i < array.Length; i++)
                            {
                                var arrayAction = new ArrayAction(array, i, propInfo);
                                TreeNode treeNode = CreateTreeNode(
                                         text: BaseParameterDisplay.GetCSharpParameterLabel(propInfo, i));
                                treeNode.Tag = arrayAction;
                                treeNodeCollection.Add(treeNode);
                            }
                        }
                    }
                    else
                    {
                        string nodeText;
                        switch (runMode)
                        {
                            case RunModes.Object:
                                nodeText = propAttr.DisplayName;
                                break;
                            case RunModes.CSharpParameter:
                                nodeText = BaseParameterDisplay.GetCSharpParameterLabel(propInfo);
                                break;
                            default:
                                throw new Exception("Invalid runMode.");
                        }
                        TreeNode treeNode = CreateTreeNode(text: nodeText);
                        treeNode.Tag = propInfo;
                        if (emptyNode != null)
                            treeNode.Nodes.Add(emptyNode);
                        treeNodeCollection.Add(treeNode);
                    }
                }
            }
        }

        private void PopulateTreeNodes(TreeNodeCollection treeNodeCollection, IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                var treeNode = CreateTreeNode(text: BaseParameterDisplay.GetParameterLabel(parameter));
                treeNode.Tag = parameter;
                treeNodeCollection.Add(treeNode);
            }
        }

        private void SetPropertyAction(TreeNode treeNode)
        {
            switch (runMode)
            {
                case RunModes.Object:
                    PropertyAction propAction = new PropertyAction();
                    propAction.SetPathAndObject(GetPropertyInfoPath(treeNode), previewTarget);
                    //propAction.SetValue();  //Before setting BasePropertyAction.
                    this.BasePropertyAction = propAction;
                    break;
                case RunModes.Parameter:
                    ParameterAction parameterAction = new ParameterAction();
                    parameterAction.Parameter = (Parameter)treeNode.Tag;
                    //parameterAction.SetValue();
                    this.BasePropertyAction = parameterAction;
                    break;
                case RunModes.CSharpParameter:
                    ArrayAction arrayAction = treeNode.Tag as ArrayAction;
                    if (arrayAction != null)
                    {
                        //arrayAction.SetValue();
                        BasePropertyAction = arrayAction;
                    }
                    else
                    {
                        SimplePropertyAction simplePropertyAction = new SimplePropertyAction();
                        simplePropertyAction.SetPropertyInfoAndObject(
                                GetTreeNodePropertyInfo(treeNode), previewTarget);
                        //simplePropertyAction.SetValue();
                        BasePropertyAction = simplePropertyAction;
                    }
                    break;
            }
        }

        private void setPropertyActionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetPropertyAction(clickedTreeNode);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void tvProperties_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    clickedTreeNode = tvProperties.GetNodeAt(e.Location);
                    bool isValid;
                    if (runMode == RunModes.Parameter)
                        isValid = clickedTreeNode.Tag is Parameter;
                    else
                        isValid = clickedTreeNode.Tag is PropertyInfo || clickedTreeNode.Tag is ArrayAction;
                    if (isValid)
                    {
                        tvProperties.SelectedNode = clickedTreeNode;
                        treeviewContextMenu.Show(tvProperties, e.Location);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private IEnumerable<PropertyInfo> GetPropertyInfoPath(TreeNode node)
        {
            return GetTreeNodePath(node).Select(tn => GetTreeNodePropertyInfo(tn)).Where(pi => pi != null);
        }

        private IEnumerable<TreeNode> GetTreeNodePath(TreeNode node)
        {
            if (node != null)
            {
                foreach (TreeNode ancestor in GetTreeNodePath(node.Parent))
                {
                    yield return ancestor;
                }
                yield return node;
            }
        }

        private PropertyInfo GetTreeNodePropertyInfo(TreeNode node)
        {
            var propertyInfo = node.Tag as PropertyInfo;
            if (propertyInfo == null && !(node.Tag is ArrayAction))
            {
                throw new Exception("TreeNode's Tag did not hold a PropertyInfo.");
            }
            return propertyInfo;
        }

        private IEnumerable<PropertyAttributes> GetFilteredProperties(Type type, bool matchingAttribute)
        {
            foreach (PropertyInfo propInfo in type.GetProperties())
            {
                if (matchingAttribute)
                {
                    var attribute = PropertyActionAttribute.GetPropertyAttribute(propInfo);
                    if (attribute != null && !attribute.Exclude)
                    {
                        var propAttr = new PropertyAttributes(propInfo, attribute);
                        yield return propAttr;
                    }
                }
                else if (propInfo.PropertyType == typeof(double) || propInfo.PropertyType == typeof(double[]))
                {
                    yield return new PropertyAttributes(propInfo, null);
                }
            }
        }

        private void TvProperties_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                if (runMode != RunModes.Object)
                    return;
                TreeNode node = e.Node;
                if (node.Nodes.Count == 1)
                {
                    TreeNode child = node.Nodes[0];
                    if (child.Tag == null)
                    {
                        PropertyInfo propertyInfo = node.Tag as PropertyInfo;
                        if (propertyInfo == null)
                        {
                            e.Cancel = true;
                            return;
                        }
                        node.Nodes.Remove(child);
                        PopulateTreeNodes(node.Nodes, propertyInfo.PropertyType);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnDeselectProperty_Click(object sender, EventArgs e)
        {
            try
            {
                this.BasePropertyAction = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool IsRunning
        {
            get { return timer1.Enabled; }
        }

        private void btnToggleRun_Click(object sender, EventArgs e)
        {
            try
            {
                Run(!IsRunning);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool Run(bool run)
        {
            if (run == IsRunning)
                return false;
            if (run)
            {
                string message = null;
                double increment = 0;
                if (BasePropertyAction == null)
                    message = "Please select a property.";
                else if (BasePropertyAction.GetStatus() != BasePropertyAction.IncrementStatus.Success)
                    message = "Cannot run with the selected property.";
                else if (!double.TryParse(txtPropertyIncrement.Text, out increment))
                    message = "Please enter a number for Increment.";
                if (message != null)
                {
                    MessageBox.Show(message);
                    return false;
                }
                if (!runPropertyActions.Contains(BasePropertyAction))
                {
                    runPropertyActions.Add(BasePropertyAction);
                }
                BasePropertyAction.Increment = increment;
                BasePropertyAction.CurrentStep = 0;
                if (!int.TryParse(txtRateOfChange.Text, out int interval))
                    interval = 500;
                DisplayForRun();
                if (undoIndex < undoValues.Count - 1)
                {
                    undoValues.RemoveRange(undoIndex + 1, undoValues.Count - 1 - undoIndex);
                }
                if (undoValues.Count == 0 && BasePropertyAction.PropertyValue != null)
                    undoValues.Add(BasePropertyAction.PropertyValue);
                undoIndex = undoValues.Count;
                timer1.Interval = interval;
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
                if (BasePropertyAction?.PropertyValue != null)
                    undoValues.Add(BasePropertyAction.PropertyValue);
            }
            btnToggleRun.Text = IsRunning ? "Stop" : "Run";
            txtRunPropertyValue.ReadOnly = IsRunning;
            return true;
        }

        private void DisplayForRun()
        {
            txtRunPropertyValue.Text = BasePropertyAction.PropertyValue?.ToString();
            txtCurrentStep.Text = BasePropertyAction.CurrentStep.ToString();
            PreviewPattern();
        }

        private void PreviewPattern()
        {
            previewPattern.ComputeSeedPoints();
            picPattern.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                var status = BasePropertyAction.IncrementValue();
                lblStatus.Text = status.ToString();
                if (status != BasePropertyAction.IncrementStatus.Success)
                {
                    Run(false);
                }
                else
                {
                    BasePropertyAction.CurrentStep++;
                    DisplayForRun();
                }
            }
            catch (Exception ex)
            {
                Run(false);
                Tools.HandleException(ex);
            }
        }

        private void picPattern_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                previewPattern.DrawFilled(e.Graphics, null, computeRandom: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnApplyAndClose_Click(object sender, EventArgs e)
        {
            try
            {
                bool oneValid = false;
                foreach (var basePropAction in runPropertyActions)
                {
                    if (basePropAction.GetStatus() != BasePropertyAction.IncrementStatus.Success)
                        continue;
                    var parameterAction = basePropAction as ParameterAction;
                    if (parameterAction != null)
                    {
                        Parameter targetParam = targetFormulaSettings.Parameters.Where(
                                  p => p.ParameterName == parameterAction.Parameter.ParameterName).FirstOrDefault();
                        if (targetParam != null)
                        {
                            targetParam.Value = parameterAction.PropertyValue;
                            oneValid = true;
                        }
                    }
                    else
                    {
                        object source;
                        var simplePropertyAction = basePropAction as SimplePropertyAction;
                        if (simplePropertyAction != null)
                        {
                            var propertyAction = simplePropertyAction as PropertyAction;
                            if (propertyAction != null)
                            {
                                source = Whorl.PropertyAction.GetSourceObject(propertyAction.PropertyInfoPath, targetObject);
                            }
                            else
                            {
                                source = targetObject;
                            }
                            if (source != null)
                            {
                                simplePropertyAction.PropertyInfo.SetValue(source, simplePropertyAction.PropertyValue);
                                oneValid = true;
                            }
                        }
                        else
                        {
                            var arrayAction = basePropAction as ArrayAction;
                            if (arrayAction?.PropertyValue != null && targetObject != null)
                            {
                                Array array = arrayAction.PropertyInfo.GetValue(targetObject) as Array;
                                if (array != null)
                                {
                                    array.SetValue(arrayAction.PropertyValue, arrayAction.Index);
                                    oneValid = true;
                                }
                            }
                        }
                    }
                }
                if (oneValid)
                {
                    patternForm.PopulateControlsForPattern(targetPattern, refreshParameters: runMode != RunModes.Object);
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Couldn't update the pattern.");
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnNegateIncrement_Click(object sender, EventArgs e)
        {
            try
            {
                if (BasePropertyAction != null)
                {
                    BasePropertyAction.Increment = -BasePropertyAction.Increment;
                    txtPropertyIncrement.Text = BasePropertyAction.Increment.ToString();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnSetValue_Click(object sender, EventArgs e)
        {
            try
            {
                if (BasePropertyAction != null)
                {
                    if (!double.TryParse(txtRunPropertyValue.Text, out double val))
                    {
                        MessageBox.Show("Enter a number for the value.");
                        return;
                    }
                    var status = BasePropertyAction.SetValue(val);
                    lblStatus.Text = status.ToString();
                    if (status == BasePropertyAction.IncrementStatus.Success)
                    {
                        PreviewPattern();
                    }
                    else
                    {
                        txtRunPropertyValue.Text = BasePropertyAction.PropertyValue?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void UndoOrRedoAction()
        {
            object val = undoValues[undoIndex];
            var status = BasePropertyAction.SetValue(val);
            if (status == BasePropertyAction.IncrementStatus.Success)
            {
                txtRunPropertyValue.Text = val.ToString();
                PreviewPattern();
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsRunning)
                    return;
                if (undoIndex > 0 && BasePropertyAction != null)
                {
                    undoIndex--;
                    UndoOrRedoAction();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsRunning)
                    return;
                if (undoIndex < undoValues.Count - 1 && BasePropertyAction != null)
                {
                    undoIndex++;
                    UndoOrRedoAction();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
