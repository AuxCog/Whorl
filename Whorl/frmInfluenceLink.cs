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
        public frmInfluenceLink()
        {
            InitializeComponent();
        }

        private FormulaSettings formulaSettings { get; set; }
        private List<object> cboPointInfoList { get; set; }

        private string _parameterName;
        private string parameterName
        {
            get => _parameterName;
            set
            {
                if (_parameterName == value)
                    return;
                _parameterName = value;
                BaseInfluenceLinkParent linkParent;
                if (formulaSettings.InfluenceLinkParentCollection == null)
                    linkParent = null;
                else
                {
                    if (!formulaSettings.InfluenceLinkParentCollection.InfluenceLinkParentsByParameterName
                                        .TryGetValue(_parameterName, out linkParent))
                    {
                        linkParent = null;
                    }
                }
                cboPointInfoList = new List<object>() { "(Please Select)" };
                foreach (var pointInfo in pattern.InfluencePointInfoList.InfluencePointInfos)
                {
                    bool hasLink = linkParent != null && linkParent.InfluenceLinks.Any(l => l.InfluencePointInfo == pointInfo);
                    string idText = pointInfo.Id.ToString();
                    if (hasLink)
                        idText += "*";
                    cboPointInfoList.Add(new ValueTextItem(pointInfo, idText));
                }
                cboInfluencePointInfo.DataSource = cboPointInfoList;
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
                BtnDeleteLink.Enabled = txtInfluenceFactor.Enabled = chkMultiply.Enabled = _influenceLink != null;
                if (_influenceLink != null)
                {
                    txtInfluenceFactor.Text = _influenceLink.LinkFactor.ToString("0.####");
                    chkMultiply.Checked = _influenceLink.Multiply;
                }
            }
        }
        private InfluenceLink newInfluenceLink { get; set; }
        private Pattern pattern { get; set; }

        private bool ignoreEvents { get; set; }

        public void Initialize(Pattern pattern, FormulaSettings formulaSettings, string parameterName)
        {
            try
            {
                ignoreEvents = true;
                this.pattern = pattern;
                this.formulaSettings = formulaSettings;
                this.parameterName = parameterName;  //Populates cboInfluencePointInfo
                lblTransformName.Text = formulaSettings.FormulaName;
                var parameterNames = new List<string>();
                if (formulaSettings.IsCSharpFormula)
                {
                    if (formulaSettings.EvalInstance != null)
                    {
                        var propertyInfos = formulaSettings.EvalInstance.GetParameterPropertyInfos();
                        if (propertyInfos != null)
                        {
                            parameterNames.AddRange(propertyInfos.Select(pi => pi.Name).OrderBy(s => s));
                        }
                    }
                }
                else
                {
                    parameterNames.AddRange(formulaSettings.Parameters.Select(p => p.ParameterName));
                }
                cboParameter.DataSource = parameterNames;
                cboParameter.SelectedItem = parameterName;
                InfluenceLink link = null;
                if (formulaSettings.InfluenceLinkParentCollection != null)
                {
                    if (formulaSettings.InfluenceLinkParentCollection.InfluenceLinkParentsByParameterName.TryGetValue(
                                        parameterName, out var linkParent))
                    {
                        if (linkParent.InfluenceLinks.Count() == 1)
                        {
                            link = linkParent.InfluenceLinks.First();
                        }
                    }
                }
                influenceLink = link;
                ValueTextItem cboValue = null;
                var pointInfo = influenceLink?.InfluencePointInfo;
                if (pointInfo != null)
                {
                    cboValue = cboPointInfoList.Select(v => v as ValueTextItem)
                                               .FirstOrDefault(vti => vti != null && vti.Value == pointInfo);
                }
                if (cboValue != null)
                    cboInfluencePointInfo.SelectedItem = cboValue;
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

        private void cboInfluencePointInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //if (ignoreEvents)
                //{
                //    return;
                //}
                //FindInfluenceLink();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private InfluenceLink FindInfluenceLink(out bool isValid)
        {
            var influencePointInfo = GetSelectedPointInfo();
            if (parameterName == null || formulaSettings?.InfluenceLinkParentCollection == null)
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
                if (formulaSettings.InfluenceLinkParentCollection.InfluenceLinkParentsByParameterName
                                   .TryGetValue(parameterName, out var linkParent))
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

        private BaseInfluenceLinkParent GetInfluenceLinkParent(string parameterName)
        {
            if (!formulaSettings.InfluenceLinkParentCollection.InfluenceLinkParentsByParameterName.TryGetValue(parameterName, out var influenceLinkParent))
            {
                if (formulaSettings.IsCSharpFormula)
                    influenceLinkParent = new PropertyInfluenceLinkParent(formulaSettings.InfluenceLinkParentCollection, parameterName);
                else
                    influenceLinkParent = new ParameterInfluenceLinkParent(formulaSettings.InfluenceLinkParentCollection, parameterName);
                formulaSettings.InfluenceLinkParentCollection.InfluenceLinkParentsByParameterName.Add(parameterName, influenceLinkParent);
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
                if (formulaSettings == null || parameterName == null) return;
                RemoveNewInfluenceLink();
                var influencePointInfo = GetSelectedPointInfo();
                if (influencePointInfo == null)
                {
                    MessageBox.Show("Please select an Influence Point.");
                    return;
                }
                if (formulaSettings.InfluenceLinkParentCollection == null)
                {
                    formulaSettings.InfluenceLinkParentCollection = new InfluenceLinkParentCollection(pattern, formulaSettings);
                }
                BaseInfluenceLinkParent influenceLinkParent = GetInfluenceLinkParent(parameterName);
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

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (influenceLink == null)
                {
                    MessageBox.Show("Please click Create Link first, or click Cancel.");
                    return;
                }
                var influencePointInfo = GetSelectedPointInfo();
                if (influencePointInfo == null)
                {
                    MessageBox.Show("Please select an Influence Point.");
                    return;
                }
                if (!double.TryParse(txtInfluenceFactor.Text, out double val))
                {
                    MessageBox.Show("Please enter a number for Influence Factor.");
                    return;
                }
                if (influenceLink.Parent.ParameterName != parameterName)
                {
                    var newParent = GetInfluenceLinkParent(parameterName);
                    if (newParent != null)
                    {
                        influenceLink.Parent.RemoveInfluenceLink(influenceLink);
                        newParent.AddInfluenceLink(influenceLink);
                    }
                }
                influenceLink.InfluencePointInfo = influencePointInfo;
                influenceLink.LinkFactor = val;
                influenceLink.Multiply = chkMultiply.Checked;
                pattern.LastEditedInfluenceLink = influenceLink;
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
                    parameterName = name;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
