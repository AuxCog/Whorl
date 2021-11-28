using ParserEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class frmFormulaInsert : Form
    {
        private enum OtherModes
        {
            GlobalVariables,
            OutputParameters,
            ParameterAttributes,
            PreprocessorReservedWords
        }

        public frmFormulaInsert(StandardFormulaTextList standardTextsList)
        {
            InitializeComponent();
            try
            {
                this.StandardTextsList = standardTextsList;
                standardFormulaTextBindingSource.DataSource = StandardTextsList.StandardFormulaTexts;
                memberItemInfoBindingSource.DataSource = memberItems;
                dgvClipboard.AutoGenerateColumns = false;
                dgvMemberInfo.AutoGenerateColumns = false;
                var registeredTypes = ExpressionParser.GetRegisteredTypes().ToList();
                registeredTypes.Add(new ExpressionParser.RegType(typeof(PixelRenderInfo)));
                registeredTypes.Insert(0, new ExpressionParser.RegType(type: null, name: string.Empty));
                cboType.DataSource = registeredTypes;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public static frmFormulaInsert Instance { get; set; }
        public string InsertionText { get; private set; }
        public StandardFormulaTextList StandardTextsList { get; }

        private List<MemberItemInfo> memberItems { get; } = new List<MemberItemInfo>();
        private FormulaSettings formulaSettings { get; set; }
        private bool handleEvents { get; set; }

        public static bool RunInsertText(TextBox txtFormula, FormulaSettings formulaSettings)
        {
            try
            {
                if (Instance == null)
                    throw new Exception("Instance of form not set.");
                Instance.Initialize(formulaSettings);
                if (Instance.ShowDialog() == DialogResult.OK)
                {
                    Tools.InsertTextInTextBox(txtFormula, Instance.InsertionText);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            return false;
        }

        public void Initialize(FormulaSettings formulaSettings)
        {
            try
            {
                handleEvents = false;
                this.formulaSettings = formulaSettings;
                pnlOther.Enabled = formulaSettings != null && formulaSettings.HaveParsedFormula;
                if (pnlOther.Enabled)
                {
                    cboOtherMode.DataSource = Enum.GetValues(typeof(OtherModes));
                    cboOtherMode.SelectedItem = OtherModes.GlobalVariables;
                    OtherModeChanged();
                }
            }
            finally
            {
                handleEvents = true;
            }
        }

        private void HideForm(bool cancelled = false)
        {
            this.DialogResult = cancelled ? DialogResult.Cancel : DialogResult.OK;
            this.Hide();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                editToolStripMenuItem.Visible = tabControl1.SelectedTab == tabClipboard;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool IncludeMemberInfo(Type type, MemberInfo memberInfo)
        {
            bool include = memberInfo.DeclaringType == type;
            if (include && memberInfo.MemberType == MemberTypes.Method)
            {
                if (((MethodInfo)memberInfo).IsSpecialName)
                    include = false;
            }
            return include;
        }

        private void dgvMemberInfo_Bind()
        {
            if (!handleEvents)
                return;
            var regType = (ExpressionParser.RegType)cboType.SelectedItem;
            if (regType.Type == null)
                return;
            BindingFlags bindingFlags = BindingFlags.Public;
            if (chkShowStaticMembers.Checked)
                bindingFlags |= BindingFlags.Static;
            else
                bindingFlags |= BindingFlags.Instance;
            memberItems.Clear();
            memberItems.AddRange(regType.Type.GetMembers(bindingFlags)
                                 .Where(mi => IncludeMemberInfo(regType.Type, mi))
                                 .OrderBy(mi => mi.Name)
                                 .Select(mi => new MemberItemInfo(mi)));
            memberItemInfoBindingSource.ResetBindings(metadataChanged: false);
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                dgvMemberInfo_Bind();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void chkShowStaticMembers_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                dgvMemberInfo_Bind();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string text = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text))
                    return;
                StandardTextsList.StandardFormulaTexts.Add(new StandardFormulaText(text));
                StandardTextsList.StandardTextsChanged = true;
                standardFormulaTextBindingSource.ResetBindings(metadataChanged: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvClipboard_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                var stdText = e.Row.DataBoundItem as StandardFormulaText;
                if (stdText != null)
                {
                    StandardTextsList.StandardFormulaTexts.Remove(stdText);
                    StandardTextsList.StandardTextsChanged = true;
                    standardFormulaTextBindingSource.ResetBindings(metadataChanged: false);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                HideForm(cancelled: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvClipboard_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                if (e.ColumnIndex < 0 || e.RowIndex < 0)
                    return;
                if (dgvClipboard.Columns[e.ColumnIndex] != lnkInsertClipboardText)
                    return;
                var item = dgvClipboard.Rows[e.RowIndex].DataBoundItem as StandardFormulaText;
                if (item == null)
                    throw new Exception("Unable to get text item.");
                InsertionText = item.ClipboardText;
                HideForm();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvMemberInfo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                if (e.ColumnIndex < 0 || e.RowIndex < 0)
                    return;
                if (dgvMemberInfo.Columns[e.ColumnIndex] != lnkInsertMember)
                    return;
                var item = dgvMemberInfo.Rows[e.RowIndex].DataBoundItem as MemberItemInfo;
                if (item == null)
                    throw new Exception("Unable to get MemberInfo item.");
                InsertionText = item.MemberInfoInfo.ToString();
                HideForm();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvOther_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!handleEvents)
                return;
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;
            if (dgvOther.Columns[e.ColumnIndex] != lnkInsertOtherText)
                return;
            var dgvRow = dgvOther.Rows[e.RowIndex];
            string text = dgvRow.Cells[1].Value as string;
            if (!string.IsNullOrEmpty(text))
            {
                InsertionText = text;
                HideForm();
            }
        }

        private string GetOutputParamDec(string name, ExpressionParser.ParameterAttributeNames paramClass)
        {
            string defaultValue = paramClass == ExpressionParser.ParameterAttributeNames.boolean ? "false" : "0";
            return $"Output {name} default = {defaultValue};";
        }

        private void OtherModeChanged()
        {
            if (formulaSettings == null)
                return;
            if (cboOtherMode.SelectedItem is OtherModes)
            {
                OtherModes otherMode = (OtherModes)cboOtherMode.SelectedItem;
                switch (otherMode)
                {
                    case OtherModes.GlobalVariables:
                        var globalVars = formulaSettings.Parser.GetVariables().Where(vid => vid.IsGlobal);
                        dgvOther.DataSource = globalVars.Select(vr => new { Name = vr.Name, Type = vr.IdentifierType.Name }).ToList();
                        break;
                    case OtherModes.OutputParameters:
                        var outputPrms = formulaSettings.Parser.ValidOutputParamsDict;
                        dgvOther.DataSource = outputPrms.Select(vr => new { Text = GetOutputParamDec(vr.Key, vr.Value) }).ToList();
                        break;
                    case OtherModes.ParameterAttributes:
                        var attrNames = Enum.GetNames(typeof(ExpressionParser.ParameterAttributeNames));
                        dgvOther.DataSource = attrNames.Select(nm => new { Name = nm }).ToList();
                        break;
                    case OtherModes.PreprocessorReservedWords:
                        dgvOther.DataSource = (from nm in Enum.GetNames(typeof(CSharpPreprocessor.ReservedWords))
                                               select new { Name = nm }).ToList();
                        break;
                }
            }
        }

        private void cboOtherMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleEvents)
                    return;
                OtherModeChanged();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
