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
    public partial class OutlineFormulaForm : Form, IFormulaForm
    {
        private Size txtFormulaOrigSize { get; }
        private FormulaTypes formulaType { get; set; }
        private BasicOutline basicOutline { get; set; }
        private PathOutline pathOutline { get; set; }
        private PathOutlineTransform pathOutlineTransform { get; set; }
        private Ribbon ribbon { get; set; }
        private Pattern.RenderingInfo renderingInfo { get; set; }
        private PatternTransform editedTransform { get; set; }
        private string AmplitudeFormula { get; set; }
        private string MaxAmplitudeFormula { get; set; }
        private FormulaEntry origFormulaEntry { get; set; }
        public bool UseVertices { get; private set; }
        public string FormulaName
        {
            get { return txtFormulaName.Text; }
        }
        private bool isLegacyFormula { get; set; }
        private TextBox currentFormulaTextBox;
        private FormulaSettings _currentFormulaSettings;
        private FormulaSettings currentFormulaSettings
        {
            get { return _currentFormulaSettings; }
            set
            {
                if (_currentFormulaSettings != value)
                {
                    _currentFormulaSettings = value;
                    _currentFormulaSettings.PopulateInsertTokensComboBox(cboInsertTokens);
                }
            }
        }

        private void Init(FormulaTypes formulaType)
        {
            this.formulaType = formulaType;
            pnlTransform.Visible = formulaType == FormulaTypes.Transform ||
                                   formulaType == FormulaTypes.OutlineTransform;
            basicOutline = null;
            pathOutline = null;
            ribbon = null;
            renderingInfo = null;
            editedTransform = null;
            pathOutlineTransform = null;
            UseVertices = false;
            chkUseVertices.Enabled = false;
        }

        public OutlineFormulaForm()
        {
            InitializeComponent();
            try
            {
                txtFormulaOrigSize = txtFormula.Size;
                currentFormulaTextBox = txtFormula;
                txtFormula.Enter += txtFormula_Enter;
                txtMaxAmplitudeFormula.Enter += txtFormula_Enter;
                cboDrawType.DataSource = Enum.GetValues(typeof(PathOutline.DrawTypes));
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private FormulaUsages GetFormulaUsage()
        {
            if (cboFormulaUsage.SelectedItem is FormulaUsages)
                return (FormulaUsages)cboFormulaUsage.SelectedItem;
            else
                return FormulaUsages.Normal;
        }

        private void SetOrigFormulaEntry()
        {
            origCSharpCode = txtFormula.Text;
            if (chkShowRawCSharp.Visible)
            {
                chkShowRawCSharp.Checked = false;
            }
            if (string.IsNullOrEmpty(txtFormula.Text))
                origFormulaEntry = null;
            else
                origFormulaEntry = new FormulaEntry(this.formulaType)
                {
                    FormulaName = txtFormulaName.Text,
                    Formula = txtFormula.Text,
                    MaxAmplitudeFormula = txtMaxAmplitudeFormula.Text,
                    IsCSharp = chkIsCSharpFormula.Checked,
                    FormulaUsage = GetFormulaUsage()
                };
        }

        private void ResetToOrigFormula()
        {
            if (origFormulaEntry != null)
            {
                SetFormula(origFormulaEntry);
            }
        }

        private void txtFormula_Enter(object sender, EventArgs e)
        {
            currentFormulaTextBox = (TextBox)sender;
            if (basicOutline == null || UseVertices)
                return;
            if (currentFormulaTextBox == txtFormula)
            {
                currentFormulaSettings = basicOutline.customOutline.AmplitudeSettings;
            }
            else
            {
                currentFormulaSettings = basicOutline.customOutline.MaxAmplitudeSettings;
            }
        }

        public void Initialize(BasicOutline outline)
        {
            try
            {
                Init(FormulaTypes.Outline);
                chkIsCSharpFormula.Show();
                this.basicOutline = outline;
                this.pathOutline = outline as PathOutline;
                this.ribbon = null;
                this.renderingInfo = null;
                lblFormula.Text = "Amplitude Formula:";
                var customOutline = outline.customOutline;
                if (customOutline != null)
                {
                    translateMaxAmplitudeToCToolStripMenuItem.Visible = true;
                    txtFormulaName.Text = customOutline.AmplitudeSettings.FormulaName;
                    this.AmplitudeFormula = customOutline.AmplitudeSettings.Formula;
                    this.MaxAmplitudeFormula = customOutline.MaxAmplitudeSettings.Formula;
                    chkIsCSharpFormula.Checked = customOutline.AmplitudeSettings.IsCSharpFormula;
                    chkIsMaxAmpCSharp.Checked = customOutline.MaxAmplitudeSettings.IsCSharpFormula;
                }
                if (pathOutline != null)
                {
                    cboDrawType.SelectedItem = pathOutline.DrawType;
                    chkDrawClosed.Checked = pathOutline.HasClosedPath;
                    txtMaxPathPoints.Text = pathOutline.MaxPathPoints.ToString();
                    chkUsesInfluencePoints.Checked = pathOutline.UsesInfluencePoints;
                }
                this.txtRotationSpan.Text = outline.GetRotationSpan().ToString();
                this.pnlPathSettings.Enabled = pathOutline != null;
                UseVertices = pathOutline != null && pathOutline.UseVertices;
                if (chkUseVertices.Checked != UseVertices)
                    chkUseVertices.Checked = UseVertices;  //event calls UseVerticesChanged()
                else
                    UseVerticesChanged();
                chkUseVertices.Enabled = pathOutline != null;
                //PopulateSavedFormulaNameComboBox();
                cboFormulaUsage.SelectedItem = FormulaUsages.Normal;
                SetOrigFormulaEntry();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StandardInit(FormulaSettings formulaSettings, string formulaLabel, string formulaName = null)
        {
            //PopulateSavedFormulaNameComboBox();
            UseVertices = false;
            txtFormula.Height = pnlPathSettings.Bottom - txtFormula.Top;
            ShowMaxAmpFormula(false);
            pnlPathSettings.Visible = false;
            lblFormula.Text = formulaLabel;
            chkIsCSharpFormula.Checked = formulaSettings.IsCSharpFormula;
            txtFormula.Text = formulaSettings.Formula;
            txtFormulaName.Text = formulaName ?? formulaSettings.FormulaName;
            currentFormulaTextBox = txtFormula;
            currentFormulaSettings = formulaSettings;
            cboFormulaUsage.SelectedItem = FormulaUsages.Normal;
            SetOrigFormulaEntry();
        }

        public void Initialize(Ribbon ribbon)
        {
            try
            {
                Init(FormulaTypes.Ribbon);
                chkIsCSharpFormula.Hide();
                this.ribbon = ribbon;
                StandardInit(ribbon.FormulaSettings, "Ribbon Formula:");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(Pattern.RenderingInfo renderingInfo)
        {
            try
            {
                Init(FormulaTypes.PixelRender);
                chkIsCSharpFormula.Show();
                this.renderingInfo = renderingInfo;
                StandardInit(renderingInfo.FormulaSettings, "Color Formula:");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(PatternTransform transform, int transformCount)
        {
            try
            {
                Init(FormulaTypes.Transform);
                chkIsCSharpFormula.Show();
                this.editedTransform = transform;
                this.cboSequenceNumber.DataSource = Enumerable.Range(0, transformCount).ToList();
                transform.TransformSettings.PopulateInsertTokensComboBox(cboInsertTokens);
                transform.SequenceNumber = Math.Max(0,
                    Math.Min(transformCount - 1, transform.SequenceNumber));
                this.cboSequenceNumber.SelectedItem = transform.SequenceNumber;
                lblFormulaName.Text = "Transform Name:";
                StandardInit(transform.TransformSettings, "Transform Formula:", transform.TransformName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Initialize(PathOutlineTransform pathOutlineTransform, int transformCount)
        {
            try
            {
                Init(FormulaTypes.OutlineTransform);
                chkIsCSharpFormula.Show();
                this.pathOutlineTransform = pathOutlineTransform;
                this.cboSequenceNumber.DataSource = Enumerable.Range(0, transformCount).ToList();
                pathOutlineTransform.SequenceNumber = Math.Max(0,
                    Math.Min(transformCount - 1, pathOutlineTransform.SequenceNumber));
                this.cboSequenceNumber.SelectedItem = pathOutlineTransform.SequenceNumber;
                lblFormulaName.Text = "Outline Transform Name:";
                StandardInit(pathOutlineTransform.VerticesSettings, "Outline Transform Formula:");
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

        private FormulaSettings GetFormulaSettings()
        {
            FormulaSettings formulaSettings = null;
            if (UseVertices)
            {
                formulaSettings = pathOutline.VerticesSettings;
            }
            else if (basicOutline != null)
            {
                formulaSettings = basicOutline.customOutline.AmplitudeSettings;
            }
            else if (renderingInfo != null)
            {
                formulaSettings = renderingInfo.FormulaSettings;
            }
            else if (editedTransform != null)
            {
                formulaSettings = editedTransform.TransformSettings;
            }
            else if (ribbon != null)
            {
                formulaSettings = ribbon.FormulaSettings;
            }
            else if (pathOutlineTransform != null)
            {
                formulaSettings = pathOutlineTransform.VerticesSettings;
            }
            if (formulaSettings == null)
                throw new NullReferenceException("FormulaSettings were not found.");
            return formulaSettings;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (ParseFormula() != ParseStatusValues.Success)
                    return;
                if (pathOutline != null)
                {
                    double rotationSpan;
                    if (!double.TryParse(txtRotationSpan.Text, out rotationSpan))
                        throw new CustomException("Please enter a number for Path Rotations.");
                    pathOutline.RotationSpan = rotationSpan;
                    pathOutline.UseVertices = UseVertices;
                    pathOutline.HasClosedPath = chkDrawClosed.Checked;
                    pathOutline.UsesInfluencePoints = chkUsesInfluencePoints.Checked;
                    if (UseVertices)
                    {
                        if (cboDrawType.SelectedItem is PathOutline.DrawTypes)
                        {
                            var drawType = (PathOutline.DrawTypes)cboDrawType.SelectedItem;
                            if (drawType == PathOutline.DrawTypes.Normal && pathOutline.UserDefinedVertices)
                            {
                                throw new CustomException("Draw Type cannot be Normal for User Defined Vertices.");
                            }
                            pathOutline.DrawType = drawType;
                        }
                        if (!int.TryParse(txtMaxPathPoints.Text, out int maxPoints))
                            throw new CustomException("Please enter an integer for Max Points.");
                        pathOutline.MaxPathPoints = maxPoints;
                    }
                }
                FormulaSettings formulaSettings = GetFormulaSettings();
                if (formulaSettings != null)
                {
                    formulaSettings.FormulaName = txtFormulaName.Text;
                }
                if (basicOutline != null)
                {
                    this.AmplitudeFormula = txtFormula.Text;
                    this.MaxAmplitudeFormula = txtMaxAmplitudeFormula.Text;
                }
                else if (editedTransform != null)
                {
                    editedTransform.TransformName = txtFormulaName.Text;
                    int prevSequenceNo = editedTransform.SequenceNumber;
                    editedTransform.SequenceNumber = (int)cboSequenceNumber.SelectedItem;
                    if (editedTransform.SequenceNumber != prevSequenceNo)
                    {
                        var parent = editedTransform.ParentPattern;
                        parent.Transforms.Remove(editedTransform);
                        parent.Transforms.Insert(editedTransform.SequenceNumber, editedTransform);
                        for (int i = 0; i < parent.Transforms.Count; i++)
                        {
                            parent.Transforms[i].SequenceNumber = i;
                        }
                    }
                }
                else if (pathOutlineTransform != null)
                {
                    int prevSequenceNo = pathOutlineTransform.SequenceNumber;
                    pathOutlineTransform.SequenceNumber = (int)cboSequenceNumber.SelectedItem;
                    if (pathOutlineTransform.SequenceNumber != prevSequenceNo)
                    {
                        var parent = pathOutlineTransform.PathOutline;
                        parent.PathOutlineTransforms.Remove(pathOutlineTransform);
                        parent.PathOutlineTransforms.Insert(pathOutlineTransform.SequenceNumber,
                                                            pathOutlineTransform);
                        for (int i = 0; i < parent.PathOutlineTransforms.Count; i++)
                        {
                            parent.PathOutlineTransforms[i].SequenceNumber = i;
                        }
                    }
                }
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

        private void ShowMaxAmpFormula(bool show)
        {
            chkIsMaxAmpCSharp.Visible = lblMaxFormula.Visible = txtMaxAmplitudeFormula.Visible = show;
        }

        private void UseVerticesChanged()
        {
            UseVertices = chkUseVertices.Checked;
            formulaType = UseVertices ? FormulaTypes.PathVertices : FormulaTypes.Outline;
            if (UseVertices)
            {
                txtFormula.Height = txtMaxAmplitudeFormula.Bottom - txtFormula.Top;
            }
            else
                txtFormula.Height = txtFormulaOrigSize.Height;
            ShowMaxAmpFormula(!UseVertices);
            lblFormula.Text = UseVertices ? "Vertices Formula:" : "Amplitude Formula:";
            if (UseVertices)
            {
                this.txtFormula.Text = pathOutline.VerticesSettings.Formula;
                currentFormulaSettings = pathOutline.VerticesSettings;
                chkIsCSharpFormula.Checked = currentFormulaSettings.IsCSharpFormula;
            }
            else
            {
                this.txtFormula.Text = AmplitudeFormula;
                this.txtMaxAmplitudeFormula.Text = MaxAmplitudeFormula;
                currentFormulaSettings = basicOutline.customOutline.AmplitudeSettings;
            }
            cboDrawType.Enabled = txtMaxPathPoints.Enabled = UseVertices;
            txtFormulaName.Text = currentFormulaSettings.FormulaName;
        }

        private void chkUseVertices_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UseVerticesChanged();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private bool AdjustFormulaName(out bool cancelled)
        //{
        //    string formulaName = txtFormulaName.Text;
        //    if (string.IsNullOrWhiteSpace(formulaName))
        //        formulaName = "Transform";
        //    formulaName = MainForm.FormulaEntryList.GetEntryName<OutlineFormulaEntry>(
        //        formulaName);
        //    cancelled = formulaName == null;
        //    bool adjusted = !cancelled && txtFormulaName.Text != formulaName;
        //    if (adjusted)
        //        txtFormulaName.Text = formulaName;
        //    return adjusted;
        //}

        private void addToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkShowRawCSharp.Visible)
                    chkShowRawCSharp.Checked = false;
                var formulaSettings = GetFormulaSettings();
                if (!formulaSettings.IsValid)
                {
                    MessageBox.Show("The formula is not valid.");
                }
                else if (formulaSettings.Formula != txtFormula.Text)
                {
                    MessageBox.Show("Please Parse the formula first.");
                }
                else
                {
                    var newEntry = MainForm.FormulaEntryList.AddFormulaEntry(
                             formulaType, txtFormulaName.Text, txtFormula.Text, formulaSettings.IsCSharpFormula,
                             out var status, txtMaxAmplitudeFormula.Text);
                    if (status == FormulaEntryList.AddFormulaStatus.Added ||
                        status == FormulaEntryList.AddFormulaStatus.Replaced)
                    {
                        txtFormulaName.Text = newEntry.FormulaName;
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
                //if (MainForm.FormulaEntryList.AddOutlineFormulaEntry(
                //         txtFormulaName.Text, txtFormula.Text, txtMaxAmplitudeFormula.Text))
                //    PopulateSavedFormulaNameComboBox();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void AddFormulaToChoices(FormulaUsages formulaUsage)
        {
            if (GetFormulaUsage() != formulaUsage)
            {
                MessageBox.Show($"Please select {formulaUsage} as Category first.");
                return;
            }
            if (MessageBox.Show($"Formula will be saved with name {txtFormulaName.Text}.", "Confirm", MessageBoxButtons.OKCancel)
                != DialogResult.OK)
            {
                return;
            }
            if (chkShowRawCSharp.Visible)
            {
                chkShowRawCSharp.Checked = false;
            }
            var newEntry = MainForm.FormulaEntryList.AddFormulaEntry(
                     formulaType, txtFormulaName.Text, txtFormula.Text, isCSharp: true,
                     out var status, formulaUsage: formulaUsage);
            if (status == FormulaEntryList.AddFormulaStatus.Added ||
                status == FormulaEntryList.AddFormulaStatus.Replaced)
            {
                txtFormulaName.Text = newEntry.FormulaName;
                //PopulateSavedFormulas();
            }
        }

        private void AddModuleToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AddFormulaToChoices(FormulaUsages.Module);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void saveIncludeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AddFormulaToChoices(FormulaUsages.Include);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void MergeModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkShowRawCSharp.Visible)
                {
                    chkShowRawCSharp.Checked = false;
                }
                using (var frm = new frmFormulaEntries(MainForm.FormulaEntryList, this))
                {
                    frm.Initialize(formulaType, frmFormulaEntries.CopyModes.MergeModule);
                    if (frm.ShowDialog() != DialogResult.OK)
                        return;
                    var processor = CSharpPreprocessor.Instance;
                    string mergedCode = processor.MergeModule(txtFormula.Text, moduleCode: frm.SelectedFormulaEntry.Formula);
                    if (processor.ErrorMessages.Any())
                    {
                        string errMsg = string.Join(Environment.NewLine, processor.ErrorMessages.Select(em => em.Message));
                        MessageBox.Show(errMsg, "Merge Errors");
                        return;
                    }
                    if (processor.Warnings.Any())
                    {
                        string warnings = string.Join(Environment.NewLine, processor.Warnings);
                        if (MessageBox.Show(warnings, "Merge Warnings", MessageBoxButtons.OKCancel) != DialogResult.OK)
                            return;
                    }
                    txtFormula.Text = mergedCode;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void CopyFormula(frmFormulaEntries.CopyModes copyMode)
        {
            using (var frm = new frmFormulaEntries(MainForm.FormulaEntryList, this))
            {
                frm.Initialize(formulaType, copyMode);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    SetFormula(frm.SelectedFormulaEntry);
                }
            }
        }

        private void CopyModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                CopyFormula(frmFormulaEntries.CopyModes.CopyModule);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void copyIncludeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                CopyFormula(frmFormulaEntries.CopyModes.CopyInclude);
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
        //    //{
        //    //    fe = MainForm.FormulaEntryList.GetFormulaByName(formulaName);
        //    //}
        //    return cboSavedFormulaName.SelectedItem as FormulaEntry;
        //}

        //private void lnkSetFormula_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    try
        //    {
        //        FormulaEntry fe = GetSelectedEntry();
        //        if (fe != null)
        //        {
        //            txtFormula.Text = fe.Formula;
        //            txtMaxAmplitudeFormula.Text = fe.MaxAmplitudeFormula;
        //            txtFormulaName.Text = fe.FormulaName;
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
                ResetToOrigFormula();
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
                if (currentFormulaTextBox != null)
                    FormulaSettings.cboInsertTokensOnChanged((ComboBox)sender, currentFormulaTextBox);
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
                if (currentFormulaTextBox != null)
                    frmFormulaInsert.RunInsertText(currentFormulaTextBox, currentFormulaSettings);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
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

        private TextBox parsedTextBox { get; set; }

        private ParseStatusValues ParseFormula(bool ifChanged = true)
        {
            if (chkShowRawCSharp.Visible)
                chkShowRawCSharp.Checked = false;
            FormulaSettings formulaSettings = GetFormulaSettings();
            if (chkIsCSharpFormula.Visible)
            {
                formulaSettings.IsCSharpFormula = chkIsCSharpFormula.Checked;
            }
            var status = ParseFormula(formulaSettings, txtFormula, ifChanged, GetFormulaUsage() == FormulaUsages.Module);
            if (status == ParseStatusValues.Success && basicOutline != null && !UseVertices)
            {
                basicOutline.customOutline.MaxAmplitudeSettings.IsCSharpFormula = chkIsMaxAmpCSharp.Checked;
                status = ParseFormula(basicOutline.customOutline.MaxAmplitudeSettings, txtMaxAmplitudeFormula, 
                                      ifChanged);
            }
            if (formulaSettings.IsCSharpFormula && status != ParseStatusValues.Success)
            {
                chkShowRawCSharp.Checked = status == ParseStatusValues.ParseErrors;
            }
            lnkShowErrors.Visible = status != ParseStatusValues.Success;
            switch (status)
            {
                case ParseStatusValues.Success:
                    lblParseStatus.Text = "Parse succeeded";
                    lblParseStatus.BackColor = Color.LimeGreen;
                    break;
                case ParseStatusValues.PreprocessorErrors:
                    lblParseStatus.Text = "Preprocessor Errors";
                    lblParseStatus.BackColor = Color.Yellow;
                    break;
                case ParseStatusValues.ParseErrors:
                    lblParseStatus.Text = "Parse/Compile Errors";
                    lblParseStatus.BackColor = Color.Red;
                    break;
            }
            timer1.Start();  //Flash color of status.
            return status;
        }

        private ParseStatusValues ParseFormula(FormulaSettings formulaSettings, TextBox textBox, 
                                               bool ifChanged = true, bool isModule = false)
        {
            if (formulaSettings == null)
                return ParseStatusValues.Success;
            parsedTextBox = textBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show("Please enter a nonblank formula.");
                return ParseStatusValues.ParseErrors;
            }
            return formulaSettings.Parse(textBox.Text, throwException: false, formulaForm: this, 
                                         ifChanged: ifChanged, isModule: isModule);
        }

        private string origCSharpCode { get; set; }

        private void ChkShowRawCSharp_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowRawCSharp.Checked)
            {
                origCSharpCode = txtFormula.Text;
                if (!ShowRawCSharpCode())
                    chkShowRawCSharp.Checked = false;
            }
            else
            {
                txtFormula.Text = origCSharpCode;
            }
            txtFormula.ReadOnly = chkShowRawCSharp.Checked;
        }

        private bool ShowRawCSharpCode()
        {
            FormulaSettings formulaSettings = GetFormulaSettings();
            string cSharpCode = formulaSettings.PreprocessCSharp(txtFormula.Text);
            var processor = CSharpPreprocessor.Instance;
            if (processor.ErrorMessages.Any())
            {
                if (!showPreprocessedCodeOnErrorToolStripMenuItem.Checked)
                {
                    parsedTextBox = txtFormula;
                    var frm = ParserErrorsForm.DefaultForm;
                    frm.Initialize(formulaSettings, this, preprocessorErrors: true, 
                                   cSharpCode: txtFormula.Text);
                    Tools.DisplayForm(frm);
                    return false;
                }
                else
                {
                    string errMsg = string.Join(Environment.NewLine, processor.ErrorMessages.Select(em => em.Message));
                    if (MessageBox.Show(errMsg, "Errors from Preprocessor", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return false;
                }
            }
            txtFormula.Text = cSharpCode;
            return true;
            //frmTextEditor frmTextEditor = new frmTextEditor();
            //frmTextEditor.DisplayText(cSharpCode, readOnly: true);
            //frmTextEditor.ShowDialog();
        }

        private void LnkShowErrors_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.DisplayForm(ParserErrorsForm.DefaultForm);
        }

        public void SelectError(ParserEngine.Token token)
        {
            try
            {
                parsedTextBox.Focus();
                parsedTextBox.SelectionStart = token.CharIndex;
                parsedTextBox.SelectionLength = token.Text.Length;
                parsedTextBox.ScrollToCaret();
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
                if (!chkIsCSharpFormula.Visible || chkIsCSharpFormula.Checked ||
                    ParseFormula() != ParseStatusValues.Success)
                {
                    return;
                }
                FormulaSettings formulaSettings = GetFormulaSettings();
                string cSharpCode = formulaSettings.TranslateToCSharp(this.formulaType);
                if (cSharpCode == null)
                    return;  //Couldn't translate.
                frmTextEditor frmTextEditor = new frmTextEditor();
                frmTextEditor.DisplayText(cSharpCode);
                if (frmTextEditor.ShowDialog() == DialogResult.OK)
                {
                    txtFormula.Text = frmTextEditor.EditedText;
                    chkIsCSharpFormula.Checked = true;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void translateMaxAmplitudeToCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkIsMaxAmpCSharp.Checked || ParseFormula() != ParseStatusValues.Success)
                    return;
                FormulaSettings formulaSettings = basicOutline.customOutline.MaxAmplitudeSettings;
                string cSharpCode = formulaSettings.TranslateToCSharp(this.formulaType);
                if (cSharpCode == null)
                    return;  //Couldn't translate.
                frmTextEditor frmTextEditor = new frmTextEditor();
                frmTextEditor.DisplayText(cSharpCode);
                if (frmTextEditor.ShowDialog() == DialogResult.OK)
                {
                    txtMaxAmplitudeFormula.Text = frmTextEditor.EditedText;
                    chkIsMaxAmpCSharp.Checked = true;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SwitchBetweenLegacyAndCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormulaSettings formulaSettings = GetFormulaSettings();
                if (!formulaSettings.IsCSharpFormula || formulaSettings.LegacyFormula == null)
                    return;
                isLegacyFormula = !isLegacyFormula;
                string formula = isLegacyFormula ? formulaSettings.LegacyFormula : formulaSettings.Formula;
                txtFormula.Text = formula;
                chkIsCSharpFormula.Checked = !isLegacyFormula;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void SetFormula(FormulaEntry fe)
        {
            if (chkShowRawCSharp.Visible)
            {
                chkShowRawCSharp.Checked = false;
            }
            txtFormula.Text = fe.Formula;
            txtMaxAmplitudeFormula.Text = fe.MaxAmplitudeFormula;
            txtFormulaName.Text = fe.FormulaName;
            chkIsCSharpFormula.Checked = fe.IsCSharp;
            cboFormulaUsage.SelectedItem = fe.FormulaUsage;
        }

        private frmFormulaEntries frmFormulaEntries;

        private void LnkSavedFormulas_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (frmFormulaEntries == null || frmFormulaEntries.IsDisposed)
                {
                    frmFormulaEntries = new frmFormulaEntries(MainForm.FormulaEntryList, this);
                    frmFormulaEntries.Initialize(this.formulaType);
                }
                Tools.DisplayForm(frmFormulaEntries, this);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ChkIsCSharpFormula_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkIsCSharpFormula.Checked)
                cboFormulaUsage.SelectedItem = FormulaUsages.Normal;
            cboFormulaUsage.Enabled = chkIsCSharpFormula.Checked;
            chkShowRawCSharp.Visible = chkIsCSharpFormula.Checked;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            lblParseStatus.BackColor = Color.Transparent;
            timer1.Stop();
        }

        private void OutlineFormulaForm_Resize(object sender, EventArgs e)
        {
            try
            {
                if (WindowState == FormWindowState.Minimized)
                    return;
                if (pnlPathSettings.Visible || txtMaxAmplitudeFormula.Visible)
                    return;
                int margin = txtFormula.Left;
                txtFormula.Size = new Size(ClientSize.Width - 2 * margin, ClientSize.Height - txtFormula.Top - margin);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void OutlineFormulaForm_Load(object sender, EventArgs e)
        {
            try
            {
                cboFormulaUsage.DataSource = Enum.GetValues(typeof(FormulaUsages));
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }

        private void copyFormulaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                chkShowRawCSharp.Checked = false;
                Clipboard.SetText(txtFormula.Text);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
