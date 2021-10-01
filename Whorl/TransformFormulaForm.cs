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
    public partial class TransformFormulaForm : Form, IFormulaForm
    {
        private PatternTransform editedTransform { get; set; }

        public TransformFormulaForm()
        {
            InitializeComponent();
        }

        public void Initialize(PatternTransform transform, int transformCount)
        {
            try
            {
                this.editedTransform = transform;
                chkIsCSharpFormula.Checked = transform.TransformSettings.IsCSharpFormula;
                this.txtTransformName.Text = transform.TransformName;
                this.txtTransformFormula.Text = transform.TransformSettings.Formula;
                this.cboSequenceNumber.DataSource = 
                    Enumerable.Range(0, transformCount).ToList();
                transform.TransformSettings.PopulateInsertTokensComboBox(cboInsertTokens);
                transform.SequenceNumber = Math.Max(0, 
                    Math.Min(transformCount - 1, transform.SequenceNumber));
                this.cboSequenceNumber.SelectedItem = transform.SequenceNumber;
                //PopulateSavedFormulaNameComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PopulateSavedFormulas()
        {
            if (frmFormulaEntries != null)
                frmFormulaEntries.ApplyFilters();
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ParseFormula(ifChanged: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool ParseFormula(bool ifChanged = true)
        {
            //string saveFormula = editedTransform.TransformSettings.Formula;
            // editedTransform.TransformSettings.Formula = txtTransformFormula.Text;
            editedTransform.TransformSettings.IsCSharpFormula = chkIsCSharpFormula.Checked;
            return editedTransform.TransformSettings.Parse(txtTransformFormula.Text, throwException: false, formulaForm: this, ifChanged: ifChanged);
            //{
            //    //editedTransform.TransformSettings.Formula = saveFormula;
            //    return false;
            //}
            //return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTransformName.Text))
                {
                    MessageBox.Show("Please enter the Transform Name.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtTransformFormula.Text))
                {
                    MessageBox.Show("Please enter the Transform Formula.");
                    return;
                }
                if (!ParseFormula())
                    return;
                editedTransform.TransformName = txtTransformName.Text;
                editedTransform.SequenceNumber = (int)cboSequenceNumber.SelectedItem;
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        //private bool AdjustFormulaName(out bool cancelled)
        //{
        //    string formulaName = txtTransformName.Text;
        //    if (string.IsNullOrWhiteSpace(formulaName))
        //        formulaName = "Transform";
        //    formulaName = MainForm.FormulaEntryList.GetEntryName<TransformFormulaEntry>(
        //        formulaName);
        //    cancelled = formulaName == null;
        //    bool adjusted = !cancelled && txtTransformName.Text != formulaName;
        //    if (adjusted)
        //        txtTransformName.Text = formulaName;
        //    return adjusted;
        //}

        private void addToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var formulaSettings = editedTransform.TransformSettings;
                if (!formulaSettings.IsValid)
                {
                    MessageBox.Show("The formula is not valid.");
                }
                else if (formulaSettings.Formula != txtTransformFormula.Text)
                {
                    MessageBox.Show("Please Parse the formula first.");
                }
                else
                {
                    var newEntry = MainForm.FormulaEntryList.AddFormulaEntry(
                        FormulaTypes.Transform, txtTransformName.Text, txtTransformFormula.Text, 
                        formulaSettings.IsCSharpFormula, out var status);
                    if (status == FormulaEntryList.AddFormulaStatus.Added ||
                        status == FormulaEntryList.AddFormulaStatus.Replaced)
                    {
                        txtTransformName.Text = newEntry.FormulaName;
                        PopulateSavedFormulas();
                    }
                }
                //bool cancelled;
                //bool adjusted = AdjustFormulaName(out cancelled);
                //if (cancelled || adjusted)
                //{
                //    if (adjusted)
                //        MessageBox.Show("Click Add to Choices again to add the formula.");
                //    return;
                //}
                //if (MainForm.FormulaEntryList.AddTransformFormulaEntry(
                //            txtTransformName.Text, txtTransformFormula.Text))
                //    PopulateSavedFormulaNameComboBox();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private FormulaEntry GetSelectedEntry()
        //{
        //    //FormulaEntry fe = null;
        //    //string formulaName = (string)cboSavedFormulaName.SelectedItem;
        //    //if (!string.IsNullOrEmpty(formulaName))
        //    //    fe = MainForm.FormulaEntryList.GetFormulaByName(formulaName);
        //    //return fe;
        //    return cboSavedFormulaName.SelectedItem as FormulaEntry;
        //}

        //private void lnkSetFormula_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    try
        //    {
        //        FormulaEntry fe = GetSelectedEntry();
        //        if (fe != null)
        //        {
        //            txtTransformName.Text = fe.FormulaName;
        //            txtTransformFormula.Text = fe.Formula;
        //            chkIsCSharpFormula.Checked = fe.IsCSharp;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void lnkDeleteFormula_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    try
        //    {
        //        FormulaEntry fe = GetSelectedEntry();
        //        if (fe != null)
        //        {
        //            MainForm.FormulaEntryList.RemoveFormula(fe.FormulaName);
        //            PopulateSavedFormulaNameComboBox();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private void resetToOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                txtTransformName.Text = this.editedTransform.TransformName;
                txtTransformFormula.Text = this.editedTransform.TransformSettings.Formula;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void cboInsertTokens_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                FormulaSettings.cboInsertTokensOnChanged((ComboBox)sender, txtTransformFormula);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void insertTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                frmFormulaInsert.RunInsertText(txtTransformFormula, editedTransform.TransformSettings);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void SelectError(ParserEngine.Token token)
        {
            try
            {
                txtTransformFormula.Focus();
                txtTransformFormula.SelectionStart = token.CharIndex;
                txtTransformFormula.SelectionLength = token.Text.Length;
                txtTransformFormula.ScrollToCaret();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void TranslateToCSharpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ParseFormula())
                    return;
                FormulaSettings formulaSettings = editedTransform.TransformSettings;
                string cSharpCode = formulaSettings.TranslateToCSharp(FormulaTypes.Transform);
                if (cSharpCode == null)
                    return;  //Couldn't translate.
                frmTextEditor frmTextEditor = new frmTextEditor();
                frmTextEditor.DisplayText(cSharpCode);
                if (frmTextEditor.ShowDialog() == DialogResult.OK)
                {
                    txtTransformFormula.Text = frmTextEditor.EditedText;
                    chkIsCSharpFormula.Checked = true;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool isLegacyFormula;

        private void SwitchBetweenLegacyAndCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormulaSettings formulaSettings = editedTransform.TransformSettings;
                if (!formulaSettings.IsCSharpFormula || formulaSettings.LegacyFormula == null)
                    return;
                isLegacyFormula = !isLegacyFormula;
                string formula = isLegacyFormula ? formulaSettings.LegacyFormula : formulaSettings.Formula;
                txtTransformFormula.Text = formula;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void SetFormula(FormulaEntry fe)
        {
            txtTransformFormula.Text = fe.Formula;
            txtTransformName.Text = fe.FormulaName;
            chkIsCSharpFormula.Checked = fe.IsCSharp;
        }

        private frmFormulaEntries frmFormulaEntries;

        private void LnkSavedFormulas_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (frmFormulaEntries == null || frmFormulaEntries.IsDisposed)
                {
                    frmFormulaEntries = new frmFormulaEntries(MainForm.FormulaEntryList, this);
                    frmFormulaEntries.Initialize(FormulaTypes.Transform);
                }
                Tools.DisplayForm(frmFormulaEntries);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
