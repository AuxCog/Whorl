using ParserEngine;
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
    public partial class frmInfluenceLink : Form
    {
        private enum LinkCopyModes
        {
            All,
            ParentLink,
            InfluenceLink
        }
        public frmInfluenceLink()
        {
            InitializeComponent();
            try
            {
                ConfigureMenu();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private FormulaSettings formulaSettings { get; set; }
        private InfluenceLinkParentCollection influenceLinkParentCollection { get; set; }
        private List<object> cboPointInfoList { get; set; }

        private string _parameterKey;
        private string parameterKey
        {
            get => _parameterKey;
            set
            {
                if (_parameterKey == value)
                    return;
                _parameterKey = value;
                BaseInfluenceLinkParent linkParent;
                if (influenceLinkParentCollection == null)
                    linkParent = null;
                else
                {
                    linkParent = influenceLinkParentCollection.GetLinkParent(_parameterKey);
                }
                influenceLinkParent = linkParent;
                cboPointInfoList = new List<object>() { "(Please Select)" };
                foreach (var pointInfo in pattern.InfluencePointInfoList.InfluencePointInfos)
                {
                    bool hasLink = linkParent != null && linkParent.InfluenceLinks.Any(l => l.InfluencePointInfo == pointInfo);
                    string idText = pointInfo.Id.ToString();
                    if (hasLink)
                        idText += "*";
                    cboPointInfoList.Add(new ValueTextItem(pointInfo, idText));
                }
                var pointItem = cboInfluencePointInfo.SelectedItem as ValueTextItem;
                cboInfluencePointInfo.DataSource = cboPointInfoList;
                if (pointItem != null)
                {
                    pointItem = cboPointInfoList.Select(o => o as ValueTextItem).FirstOrDefault(it => it?.Value == pointItem.Value);
                }
                if (pointItem != null)
                    cboInfluencePointInfo.SelectedItem = pointItem;
                else
                    cboInfluencePointInfo.SelectedIndex = 0;
                bool hasRandom = randomValuesByParameterKey.ContainsKey(_parameterKey);
                BtnCreateRandomSettings.Enabled = !hasRandom;
                BtnEditRandomSettings.Enabled = BtnDeleteRandomSettings.Enabled = hasRandom;
            }
        }
        private static BaseInfluenceLinkParent copiedInfluenceLinkParent { get; set; }
        private static InfluenceLink copiedInfluenceLink { get; set; }

        private BaseInfluenceLinkParent _influenceLinkParent;
        private BaseInfluenceLinkParent influenceLinkParent 
        {
            get => _influenceLinkParent;
            set
            {
                if (_influenceLinkParent == value)
                    return;
                _influenceLinkParent = value;
                txtParentRandomWeight.Enabled = _influenceLinkParent != null;
                if (_influenceLinkParent != null)
                {
                    if (!Equals(cboParameter.SelectedItem, _influenceLinkParent.ParameterKey))
                    {
                        ignoreEvents = true;
                        try
                        {
                            cboParameter.SelectedItem = _influenceLinkParent.ParameterKey;
                        }
                        finally
                        {
                            ignoreEvents = false;
                        }
                    }
                    txtParentRandomWeight.Text = _influenceLinkParent.RandomWeight.ToString("0.####");
                }
            }
        }

        private InfluenceLink _influenceLink;
        private InfluenceLink influenceLink 
        {
            get => _influenceLink;
            set
            {
                if (_influenceLink == value) 
                    return;
                _influenceLink = value;
                BtnCreateLink.Enabled = _influenceLink == null;
                BtnDeleteLink.Enabled = txtInfluenceFactor.Enabled = chkMultiply.Enabled = txtPointRandomWeight.Enabled = _influenceLink != null;
                if (_influenceLink != null)
                {
                    txtInfluenceFactor.Text = _influenceLink.LinkFactor.ToString("0.####");
                    chkMultiply.Checked = _influenceLink.Multiply;
                    txtPointRandomWeight.Text = _influenceLink.RandomWeight.ToString("0.####");
                }
            }
        }

        private InfluenceLink newInfluenceLink { get; set; }
        private Pattern pattern { get; set; }
        private Dictionary<string, RandomValues> randomValuesByParameterKey { get; } = new Dictionary<string, RandomValues>();
        private bool ignoreEvents { get; set; }

        private void AddMenuItem(ToolStripMenuItem menuItem, EventHandler eventHandler, LinkCopyModes mode, string text)
        {
            var childItem = new ToolStripMenuItem(text);
            childItem.Click += eventHandler;
            childItem.Tag = mode;
            menuItem.DropDownItems.Add(childItem);
        }

        private void AddMenuItems(ToolStripMenuItem menuItem, EventHandler eventHandler)
        {
            AddMenuItem(menuItem, eventHandler, LinkCopyModes.All, "All");
            AddMenuItem(menuItem, eventHandler, LinkCopyModes.InfluenceLink, "Influence Link");
            AddMenuItem(menuItem, eventHandler, LinkCopyModes.ParentLink, "Parent Link");
        }

        private void ConfigureMenu()
        {
            AddMenuItems(copyToolStripMenuItem, new EventHandler(CopyAction));
            AddMenuItems(pasteToolStripMenuItem, new EventHandler(PasteAction));
        }

        private LinkCopyModes GetLinkCopyMode(object sender, out ToolStripMenuItem menuItem)
        {
            menuItem = (ToolStripMenuItem)sender;
            return (LinkCopyModes)menuItem.Tag;
        }

        private void CopyAction(object sender, EventArgs e)
        {            
            try
            {
                LinkCopyModes mode = GetLinkCopyMode(sender, out var menuItem);
                copiedInfluenceLinkParent = null;
                copiedInfluenceLink = null;
                if (mode == LinkCopyModes.All || mode == LinkCopyModes.ParentLink)
                {
                    if (influenceLinkParent != null)
                        copiedInfluenceLinkParent = influenceLinkParent.GetCopy(influenceLinkParent.ParentCollection);
                }
                if (mode == LinkCopyModes.All || mode == LinkCopyModes.InfluenceLink)
                {
                    if (influenceLink != null)
                    {
                        if (influenceLinkParent == null || influenceLinkParent.ParameterKey != influenceLink.Parent.ParameterKey)
                        {
                            MessageBox.Show("Cannot copy influence link.");
                        }
                        else
                            copiedInfluenceLink = influenceLink.GetCopy(influenceLink.Parent);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetLinkParent(BaseInfluenceLinkParent linkParent)
        {
            if (influenceLinkParent == null)
                throw new NullReferenceException("influenceLinkParent cannot be null.");
            influenceLinkParent.ParentCollection.SetLinkParent(linkParent);
            if (linkParent.RandomValues != null)
                randomValuesByParameterKey[linkParent.ParameterKey] = new RandomValues(linkParent.RandomValues);
            else
                randomValuesByParameterKey.Remove(linkParent.ParameterKey);
        }

        private void PasteAction(object sender, EventArgs e)
        {
            try
            {
                if (influenceLinkParent == null)
                {
                    influenceLinkParent = GetInfluenceLinkParent(parameterKey);
                }
                LinkCopyModes mode = GetLinkCopyMode(sender, out var menuItem);
                var sbMessages = new StringBuilder();
                string paramKey = null;
                InfluencePointInfo newPointInfo = null;
                if (mode == LinkCopyModes.All || mode == LinkCopyModes.ParentLink)
                {
                    if (copiedInfluenceLinkParent == null)
                        sbMessages.AppendLine("No copied link parent found.");
                    else
                        paramKey = copiedInfluenceLinkParent.ParameterKey;
                }
                if (mode == LinkCopyModes.All || mode == LinkCopyModes.InfluenceLink)
                {
                    if (copiedInfluenceLink == null)
                        sbMessages.AppendLine("No copied influence link found.");
                    else
                    {
                        newPointInfo = copiedInfluenceLink.InfluencePointInfo.FindByKeyGuid(pattern.InfluencePointInfoList.InfluencePointInfos);
                        if (newPointInfo == null)
                        {
                            sbMessages.AppendLine("The copied influence link's Influence Point is not present in this pattern.");
                            paramKey = null;
                        }
                        else if (paramKey == null)
                            paramKey = copiedInfluenceLink.Parent.ParameterKey;
                        else if (copiedInfluenceLink.Parent.ParameterKey != paramKey)
                        {
                            sbMessages.AppendLine("The copied influence link is not valid.");
                            paramKey = null;
                        }
                    }
                }
                if (sbMessages.Length > 0)
                    MessageBox.Show(sbMessages.ToString());
                if (paramKey == null)
                    return;
                if (paramKey != parameterKey)
                {
                    switch (MessageBox.Show($"Paste to parameter {parameterKey}?", "Confirm", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            paramKey = parameterKey;
                            break;
                        case DialogResult.No:
                            if (!cboParameter.Items.Contains(paramKey))
                            {
                                MessageBox.Show($"The parameter {paramKey} is not valid in this case.");
                                return;
                            }
                            break;
                    }
                }
                if (mode == LinkCopyModes.All || mode == LinkCopyModes.ParentLink)
                {
                    if (copiedInfluenceLinkParent != null)
                    {
                        var newLinkParent = copiedInfluenceLinkParent.GetCopy(influenceLinkParent.ParentCollection, paramKey);
                        SetLinkParent(newLinkParent);
                        this.influenceLinkParent = newLinkParent;
                    }
                }
                if (mode == LinkCopyModes.All || mode == LinkCopyModes.InfluenceLink)
                {
                    if (copiedInfluenceLink != null)
                    {
                        var linkParent = influenceLinkParent.ParentCollection.GetLinkParent(paramKey);
                        if (linkParent == null)
                        {
                            linkParent = copiedInfluenceLink.Parent.GetCopy(influenceLinkParent.ParentCollection, paramKey);
                            SetLinkParent(linkParent);
                        }
                        var newInfluenceLink = copiedInfluenceLink.GetCopy(linkParent);
                        newInfluenceLink.InfluencePointInfo = newPointInfo;
                        linkParent.RemoveInfluenceLink(newInfluenceLink);
                        linkParent.AddInfluenceLink(newInfluenceLink);
                        this.influenceLink = newInfluenceLink;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(Pattern pattern, FormulaSettings formulaSettings, string parameterKey)
        {
            try
            {
                ignoreEvents = true;
                this.pattern = pattern;
                this.formulaSettings = formulaSettings;
                influenceLinkParentCollection = formulaSettings.InfluenceLinkParentCollection;
                if (influenceLinkParentCollection == null)
                    influenceLinkParentCollection = new InfluenceLinkParentCollection(pattern, formulaSettings);
                lblTransformName.Text = formulaSettings.FormulaName;
                _parameterKey = null;
                randomValuesByParameterKey.Clear();
                if (influenceLinkParentCollection != null)
                {
                    foreach (var linkParent in influenceLinkParentCollection.GetLinkParents())
                    {
                        if (linkParent.RandomValues != null)
                        {
                            var copy = new RandomValues(linkParent.RandomValues);
                            randomValuesByParameterKey.Add(linkParent.ParameterKey, copy);
                        }
                    }
                }
                this.parameterKey = parameterKey;  //Populates cboInfluencePointInfo
                var parameterKeys = new List<string>();
                if (formulaSettings.IsCSharpFormula)
                {
                    if (formulaSettings.EvalInstance != null)
                    {
                        parameterKeys.AddRange(formulaSettings.EvalInstance.GetParameterKeys().OrderBy(s => s));
                    }
                }
                else
                {
                    parameterKeys.AddRange(formulaSettings.Parameters.Select(p => p.ParameterName));
                }
                cboParameter.DataSource = parameterKeys;
                cboParameter.SelectedItem = parameterKey;
                InfluenceLink link = null;
                if (influenceLinkParentCollection != null)
                {
                    var linkParent = influenceLinkParentCollection.GetLinkParent(parameterKey);
                    if (linkParent != null && linkParent.InfluenceLinks.Count() == 1)
                    {
                        link = linkParent.InfluenceLinks.First();
                    }
                }
                influenceLink = link;
                ValueTextItem pointItem = null;
                var pointInfo = influenceLink?.InfluencePointInfo;
                if (pointInfo != null)
                {
                    pointItem = cboPointInfoList.Select(v => v as ValueTextItem)
                                                .FirstOrDefault(vti => vti != null && vti.Value == pointInfo);
                }
                if (pointItem != null)
                    cboInfluencePointInfo.SelectedItem = pointItem;
                else
                    cboInfluencePointInfo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                ignoreEvents = false;
            }
        }

        private InfluencePointInfo GetSelectedPointInfo()
        {
            var cboValue = cboInfluencePointInfo.SelectedItem as ValueTextItem;
            return cboValue?.Value as InfluencePointInfo;
        }

        private InfluenceLink FindInfluenceLink(out bool isValid)
        {
            var influencePointInfo = GetSelectedPointInfo();
            if (parameterKey == null || influenceLinkParentCollection == null)
            {
                isValid = false;
            }
            else if (influencePointInfo == null)
            {
                isValid = false;
            }
            else
            {
                isValid = true;
                var linkParent = influenceLinkParentCollection.GetLinkParent(parameterKey);
                if (linkParent != null)
                {
                    return FindInfluenceLink(linkParent);
                }
            }
            return null;
        }

        private void RemoveNewInfluenceLink()
        {
            if (newInfluenceLink != null)
            {
                newInfluenceLink.Parent.RemoveInfluenceLink(newInfluenceLink);
                newInfluenceLink = null;
            }
        }

        private void BtnEditInfluencePoint_Click(object sender, EventArgs e)
        {
            try
            {
                var influencePointInfo = GetSelectedPointInfo();
                if (influencePointInfo == null)
                    return;
                using (var frm = new frmInfluencePoint())
                {
                    frm.Initialize(influencePointInfo);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private BaseInfluenceLinkParent GetInfluenceLinkParent(string parameterKey)
        {
            var influenceLinkParent = influenceLinkParentCollection.GetLinkParent(parameterKey);
            if (influenceLinkParent == null)
            {
                if (formulaSettings.IsCSharpFormula)
                    influenceLinkParent = new PropertyInfluenceLinkParent(influenceLinkParentCollection, parameterKey);
                else
                    influenceLinkParent = new ParameterInfluenceLinkParent(influenceLinkParentCollection, parameterKey);
                influenceLinkParentCollection.AddLinkParent(influenceLinkParent);
            }
            return influenceLinkParent;
        }

        private InfluenceLink FindInfluenceLink(BaseInfluenceLinkParent influenceLinkParent)
        {
            var pointInfo = GetSelectedPointInfo();
            if (pointInfo == null)
                return null;
            else
                return influenceLinkParent.GetInfluencePointInfluenceLink(pointInfo.Id);
        }

        private void BtnCreateLink_Click(object sender, EventArgs e)
        {
            try
            {
                if (formulaSettings == null || parameterKey == null) return;
                RemoveNewInfluenceLink();
                var influencePointInfo = GetSelectedPointInfo();
                if (influencePointInfo == null)
                {
                    MessageBox.Show("Please select an Influence Point.");
                    return;
                }
                if (influenceLinkParentCollection == null)
                {
                    influenceLinkParentCollection = new InfluenceLinkParentCollection(pattern, formulaSettings);
                }
                BaseInfluenceLinkParent influenceLinkParent = GetInfluenceLinkParent(parameterKey);
                if (influenceLinkParent == null)
                    return;
                if (FindInfluenceLink(influenceLinkParent) != null)
                {
                    MessageBox.Show("There is already a link for these settings. Click Find Link to view it.");
                    return;
                }
                influenceLink = new InfluenceLink(influenceLinkParent, influencePointInfo);
                influenceLinkParent.AddInfluenceLink(influenceLink);
                newInfluenceLink = influenceLink;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnDeleteLink_Click(object sender, EventArgs e)
        {
            try
            {
                if (influenceLink == null) return;
                influenceLink.Parent.RemoveInfluenceLink(influenceLink);
                influenceLink = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnFindLink_Click(object sender, EventArgs e)
        {
            try
            {
                InfluenceLink link = FindInfluenceLink(out bool isValid);
                if (isValid)
                    influenceLink = link;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool ApplySettings(bool requireInfluencePoint)
        {
            if (influenceLinkParent != null)
            {
                if (double.TryParse(txtParentRandomWeight.Text, out double weight))
                {
                    influenceLinkParent.RandomWeight = weight;
                }
                var randomValues = GetRandomValues();
                if (influenceLinkParent.RandomValues != null)
                    influenceLinkParent.RandomValues.Dispose();
                influenceLinkParent.RandomValues = randomValues;
            }
            if (influenceLink != null)
            {
                var influencePointInfo = GetSelectedPointInfo();
                if (influencePointInfo == null)
                {
                    if (requireInfluencePoint)
                    {
                        MessageBox.Show("Please select an Influence Point.");
                        return false;
                    }
                }
                else
                {
                    if (!double.TryParse(txtInfluenceFactor.Text, out double val))
                    {
                        MessageBox.Show("Please enter a number for Influence Factor.");
                        return false;
                    }
                    if (influenceLink.Parent.ParameterName != parameterKey)
                    {
                        var newParent = GetInfluenceLinkParent(parameterKey);
                        if (newParent != null)
                        {
                            influenceLink.Parent.RemoveInfluenceLink(influenceLink);
                            newParent.AddInfluenceLink(influenceLink);
                        }
                    }
                    influenceLink.InfluencePointInfo = influencePointInfo;
                    influenceLink.LinkFactor = val;
                    influenceLink.Multiply = chkMultiply.Checked;
                    if (double.TryParse(txtPointRandomWeight.Text, out double weight))
                    {
                        influenceLink.RandomWeight = weight;
                    }
                }
            }
            return true;
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                ApplySettings(requireInfluencePoint: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (influenceLink == null)
                {
                    MessageBox.Show("Please click Create Link first, or click Cancel.");
                    return;
                }
                if (!ApplySettings(requireInfluencePoint: true))
                    return;
                formulaSettings.InfluenceLinkParentCollection = influenceLinkParentCollection;
                string errMessage = influenceLink.Parent.ParentCollection.ResolveReferences(throwException: false);
                if (errMessage != null)
                    MessageBox.Show(errMessage);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                RemoveNewInfluenceLink();
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void cboParameter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ignoreEvents) return;
                string name = cboParameter.SelectedItem as string;
                if (name != null)
                    parameterKey = name;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnCreateRandomSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (parameterKey == null) return;
                if (formulaSettings.FormulaType != FormulaTypes.Transform)
                {
                    MessageBox.Show("Random settings are only implemented for Transforms.");
                    return;
                }
                var randomValues = new RandomValues(setNewSeed: true);
                if (randomValues.Settings.DomainType == RandomValues.RandomDomainTypes.Angle)
                {
                    randomValues.Settings.XLength = pattern.RotationSteps;
                    randomValues.Settings.ReferenceXLength = Pattern.DefaultRotationSteps;
                }
                else
                {
                    randomValues.Settings.XLength = RandomValues.RandomSettings.DefaultXLength;
                    randomValues.Settings.ReferenceXLength = null;
                }
                randomValues.Settings.ReferenceXLength = randomValues.Settings.XLength;
                randomValuesByParameterKey.Add(parameterKey, randomValues);
                BtnCreateRandomSettings.Enabled = false;
                BtnEditRandomSettings.Enabled = BtnDeleteRandomSettings.Enabled = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private RandomValues GetRandomValues()
        {
            if (parameterKey != null && randomValuesByParameterKey.TryGetValue(parameterKey, out var randomValues))
            {
                return randomValues;
            }
            else
                return null;
        }

        private void BtnEditRandomSettings_Click(object sender, EventArgs e)
        {
            try
            {
                RandomValues randomValues = GetRandomValues();
                if (randomValues != null)
                {
                    using (var frm = new FrmRandomValues())
                    {
                        frm.Initialize(randomValues);
                        frm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnDeleteRandomSettings_Click(object sender, EventArgs e)
        {
            try
            {
                RandomValues randomValues = GetRandomValues();
                if (randomValues != null)
                {
                    randomValues.Dispose();
                    randomValuesByParameterKey.Remove(parameterKey);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnReseedRandom_Click(object sender, EventArgs e)
        {
            try
            {
                RandomValues randomValues = GetRandomValues();
                if (randomValues != null)
                {
                    randomValues.Settings.ReseedRandom();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
