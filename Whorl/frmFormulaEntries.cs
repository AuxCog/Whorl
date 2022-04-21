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
    public partial class frmFormulaEntries : Form
    {
        public enum CopyModes
        {
            CopyFormula,
            MergeModule,
            CopyModule,
            CopyInclude
        }
        public FormulaEntry SelectedFormulaEntry { get; private set; }

        private bool handleEvents;
        private IFormulaForm formulaForm { get; }
        private FormulaEntryList formulaEntryList { get; }
        private List<FormulaEntry> filteredFormulaEntries { get; set; }
        private List<CheckBox> typeFilterCheckBoxes { get; } = new List<CheckBox>();
        private FormulaUsages formulaUsage { get; set; } = FormulaUsages.Normal;

        public frmFormulaEntries(FormulaEntryList formulaEntryList, IFormulaForm formulaForm)
        {
            InitializeComponent();
            try
            {
                dgvFormulas.AutoGenerateColumns = false;
                this.formulaEntryList = formulaEntryList;
                this.formulaForm = formulaForm;
                cboIsCSharp.DataSource = BooleanItem.GetYesNoItems();
                cboIsCSharp.SelectedIndexChanged += FilterChanged;
                cboFormulaType.DataSource = Enum.GetValues(typeof(FormulaTypes));
                CreateTypeFilterControls();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(FormulaTypes formulaType, CopyModes copyMode = CopyModes.CopyFormula)
        {
            try
            {
                handleEvents = false;
                if (copyMode == CopyModes.MergeModule || copyMode == CopyModes.CopyModule)
                    formulaUsage = FormulaUsages.Module;
                else if (copyMode == CopyModes.CopyInclude)
                    formulaUsage = FormulaUsages.Include;
                else
                    formulaUsage = FormulaUsages.Normal;
                btnCopyFormula.Text = copyMode == CopyModes.MergeModule ? "Merge Module" : "Copy Formula";
                SelectedFormulaEntry = null;
                foreach (CheckBox chk in typeFilterCheckBoxes)
                {
                    FormulaTypes chkType = (FormulaTypes)chk.Tag;
                    chk.Checked = chkType == formulaType;
                }
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                handleEvents = true;
            }
        }

        private void CreateTypeFilterControls()
        {
            const int margin = 3;
            var allTypes = (from FormulaTypes ft in Enum.GetValues(typeof(FormulaTypes)) select ft).ToArray();
            int colWidth = (pnlTypeFilters.Width - margin) / allTypes.Length;
            int chkWidth = colWidth - margin;
            int left = margin, top = margin;
            foreach (FormulaTypes formulaType in allTypes)
            {
                var chkBox = new CheckBox();
                chkBox.AutoSize = false;
                chkBox.Width = chkWidth;
                chkBox.Text = formulaType.ToString();
                chkBox.Tag = formulaType;
                chkBox.Top = top;
                chkBox.Left = left;
                pnlTypeFilters.Controls.Add(chkBox);
                typeFilterCheckBoxes.Add(chkBox);
                chkBox.CheckedChanged += FilterChanged;
                left += colWidth;
            }
        }

        private bool Matches(FormulaEntry formulaEntry, bool? isCSharp, HashSet<FormulaTypes> formulaTypes)
        {
            return  formulaEntry.FormulaUsage == formulaUsage &&
                    (isCSharp == null || formulaEntry.IsCSharp == isCSharp) && 
                    formulaTypes.Contains(formulaEntry.FormulaType);
        }

        public void ApplyFilters()
        {
            BooleanItem booleanItem = cboIsCSharp.SelectedItem as BooleanItem;
            bool? isCSharp = booleanItem?.Value;
            var formulaTypes = new HashSet<FormulaTypes>(typeFilterCheckBoxes
                               .Where(chk => chk.Checked).Select(chk => (FormulaTypes)chk.Tag));
            filteredFormulaEntries = formulaEntryList.UnsortedFormulaEntries
                                     .Where(fe => Matches(fe, isCSharp, formulaTypes))
                                     .OrderBy(fe => fe.IsSystem ? 0 : 1)
                                     .ThenBy(fe => fe.FormulaName).ToList();
            dgvFormulas.DataSource = filteredFormulaEntries;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DgvFormulas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;
                var entry = dgvFormulas.Rows[e.RowIndex].DataBoundItem as FormulaEntry;
                if (entry == null)
                    return;
                if (e.ColumnIndex == colDeleteButton.Index)
                {
                    if (MessageBox.Show($"Delete formula {entry.FormulaName}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        formulaEntryList.RemoveFormula(entry.FormulaName);
                        ApplyFilters();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DgvFormulas_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;
                var entry = dgvFormulas.Rows[e.RowIndex].DataBoundItem as FormulaEntry;
                if (entry != null)
                {
                    SelectedFormulaEntry = entry;
                    handleEvents = false;
                    cboFormulaType.SelectedItem = entry.FormulaType;
                    handleEvents = true;
                    txtFormulaName.Text = entry.FormulaName;
                    ChkIsSystem.Checked = entry.IsSystem;
                    if (entry.FormulaType == FormulaTypes.Outline)
                    {
                        txtFormula.Text = entry.Formula + Environment.NewLine + "*** " + entry.MaxAmplitudeFormula;
                    }
                    else
                        txtFormula.Text = entry.Formula;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void BtnCopyFormula_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedFormulaEntry == null)
                    return;
                if (!this.Modal)
                    formulaForm.SetFormula(SelectedFormulaEntry);
                DialogResult = DialogResult.OK;
                this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void RebindDataGrid()
        {

            int rowInd = dgvFormulas.FirstDisplayedScrollingRowIndex;
            dgvFormulas.DataSource = null;
            dgvFormulas.DataSource = filteredFormulaEntries;
            dgvFormulas.FirstDisplayedScrollingRowIndex = Math.Min(rowInd, dgvFormulas.Rows.Count - 1);
        }

        private void CboFormulaType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SelectedFormulaEntry == null || !handleEvents)
                    return;
                if (cboFormulaType.SelectedItem is FormulaTypes)
                {
                    SelectedFormulaEntry.FormulaType = (FormulaTypes)cboFormulaType.SelectedItem;
                    RebindDataGrid();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ChkIsSystem_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (SelectedFormulaEntry == null || !handleEvents)
                    return;
                SelectedFormulaEntry.IsSystem = ChkIsSystem.Checked;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void TxtFormulaName_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (SelectedFormulaEntry == null || !handleEvents)
                    return;
                if (e.KeyCode == Keys.Enter)
                {
                    if (SelectedFormulaEntry.FormulaName != txtFormulaName.Text)
                    {
                        if (formulaEntryList.RenameFormula(SelectedFormulaEntry, txtFormulaName.Text))
                        {
                            RebindDataGrid();
                        }
                        else
                        {
                            txtFormulaName.Text = SelectedFormulaEntry.FormulaName;
                        }
                    }
                    //e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
