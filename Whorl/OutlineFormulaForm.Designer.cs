namespace Whorl
{
    partial class OutlineFormulaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblFormula = new System.Windows.Forms.Label();
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.lblMaxFormula = new System.Windows.Forms.Label();
            this.txtMaxAmplitudeFormula = new System.Windows.Forms.TextBox();
            this.pnlPathSettings = new System.Windows.Forms.Panel();
            this.chkUseVertices = new System.Windows.Forms.CheckBox();
            this.txtRotationSpan = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.formulaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToChoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToOriginalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translateToCSharpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translateMaxAmplitudeToCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchBetweenLegacyAndCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addModuleToChoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveIncludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyIncludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFormulaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPreprocessedCodeOnErrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtFormulaName = new System.Windows.Forms.TextBox();
            this.lblFormulaName = new System.Windows.Forms.Label();
            this.cboInsertTokens = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkIsCSharpFormula = new System.Windows.Forms.CheckBox();
            this.lnkSavedFormulas = new System.Windows.Forms.LinkLabel();
            this.pnlTransform = new System.Windows.Forms.Panel();
            this.cboSequenceNumber = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkShowRawCSharp = new System.Windows.Forms.CheckBox();
            this.lnkShowErrors = new System.Windows.Forms.LinkLabel();
            this.lblParseStatus = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.chkIsMaxAmpCSharp = new System.Windows.Forms.CheckBox();
            this.cboFormulaUsage = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboDrawType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkDrawClosed = new System.Windows.Forms.CheckBox();
            this.pnlPathSettings.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.pnlTransform.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(639, 6);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(73, 29);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(537, 6);
            this.btnOK.Margin = new System.Windows.Forms.Padding(5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(72, 29);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblFormula
            // 
            this.lblFormula.AutoSize = true;
            this.lblFormula.Location = new System.Drawing.Point(10, 95);
            this.lblFormula.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFormula.Name = "lblFormula";
            this.lblFormula.Size = new System.Drawing.Size(129, 17);
            this.lblFormula.TabIndex = 6;
            this.lblFormula.Text = "Amplitude Formula:";
            // 
            // txtFormula
            // 
            this.txtFormula.Location = new System.Drawing.Point(11, 116);
            this.txtFormula.Margin = new System.Windows.Forms.Padding(4);
            this.txtFormula.Multiline = true;
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFormula.Size = new System.Drawing.Size(719, 240);
            this.txtFormula.TabIndex = 7;
            // 
            // lblMaxFormula
            // 
            this.lblMaxFormula.AutoSize = true;
            this.lblMaxFormula.Location = new System.Drawing.Point(8, 360);
            this.lblMaxFormula.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMaxFormula.Name = "lblMaxFormula";
            this.lblMaxFormula.Size = new System.Drawing.Size(158, 17);
            this.lblMaxFormula.TabIndex = 8;
            this.lblMaxFormula.Text = "Max Amplitude Formula:";
            // 
            // txtMaxAmplitudeFormula
            // 
            this.txtMaxAmplitudeFormula.Location = new System.Drawing.Point(12, 384);
            this.txtMaxAmplitudeFormula.Multiline = true;
            this.txtMaxAmplitudeFormula.Name = "txtMaxAmplitudeFormula";
            this.txtMaxAmplitudeFormula.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMaxAmplitudeFormula.Size = new System.Drawing.Size(719, 47);
            this.txtMaxAmplitudeFormula.TabIndex = 9;
            // 
            // pnlPathSettings
            // 
            this.pnlPathSettings.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlPathSettings.Controls.Add(this.chkDrawClosed);
            this.pnlPathSettings.Controls.Add(this.label4);
            this.pnlPathSettings.Controls.Add(this.cboDrawType);
            this.pnlPathSettings.Controls.Add(this.chkUseVertices);
            this.pnlPathSettings.Controls.Add(this.txtRotationSpan);
            this.pnlPathSettings.Controls.Add(this.label3);
            this.pnlPathSettings.Location = new System.Drawing.Point(12, 437);
            this.pnlPathSettings.Name = "pnlPathSettings";
            this.pnlPathSettings.Size = new System.Drawing.Size(719, 48);
            this.pnlPathSettings.TabIndex = 10;
            // 
            // chkUseVertices
            // 
            this.chkUseVertices.AutoSize = true;
            this.chkUseVertices.Location = new System.Drawing.Point(227, 10);
            this.chkUseVertices.Name = "chkUseVertices";
            this.chkUseVertices.Size = new System.Drawing.Size(140, 21);
            this.chkUseVertices.TabIndex = 10;
            this.chkUseVertices.Text = "Use Path Vertices";
            this.chkUseVertices.UseVisualStyleBackColor = true;
            this.chkUseVertices.CheckedChanged += new System.EventHandler(this.chkUseVertices_CheckedChanged);
            // 
            // txtRotationSpan
            // 
            this.txtRotationSpan.Location = new System.Drawing.Point(120, 7);
            this.txtRotationSpan.Name = "txtRotationSpan";
            this.txtRotationSpan.Size = new System.Drawing.Size(84, 23);
            this.txtRotationSpan.TabIndex = 8;
            this.txtRotationSpan.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 10);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Path Rotations:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.formulaToolStripMenuItem,
            this.moduleToolStripMenuItem,
            this.editToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(743, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // formulaToolStripMenuItem
            // 
            this.formulaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToChoicesToolStripMenuItem,
            this.resetToOriginalToolStripMenuItem,
            this.parseToolStripMenuItem,
            this.insertTextToolStripMenuItem,
            this.translateToCSharpToolStripMenuItem,
            this.translateMaxAmplitudeToCToolStripMenuItem,
            this.switchBetweenLegacyAndCToolStripMenuItem});
            this.formulaToolStripMenuItem.Name = "formulaToolStripMenuItem";
            this.formulaToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.formulaToolStripMenuItem.Text = "Formula";
            // 
            // addToChoicesToolStripMenuItem
            // 
            this.addToChoicesToolStripMenuItem.Name = "addToChoicesToolStripMenuItem";
            this.addToChoicesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.addToChoicesToolStripMenuItem.Text = "Save Formula";
            this.addToChoicesToolStripMenuItem.Click += new System.EventHandler(this.addToChoicesToolStripMenuItem_Click);
            // 
            // resetToOriginalToolStripMenuItem
            // 
            this.resetToOriginalToolStripMenuItem.Name = "resetToOriginalToolStripMenuItem";
            this.resetToOriginalToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.resetToOriginalToolStripMenuItem.Text = "Reset to Original";
            this.resetToOriginalToolStripMenuItem.Click += new System.EventHandler(this.resetToOriginalToolStripMenuItem_Click);
            // 
            // parseToolStripMenuItem
            // 
            this.parseToolStripMenuItem.Name = "parseToolStripMenuItem";
            this.parseToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.parseToolStripMenuItem.Text = "Parse";
            this.parseToolStripMenuItem.Click += new System.EventHandler(this.parseToolStripMenuItem_Click);
            // 
            // insertTextToolStripMenuItem
            // 
            this.insertTextToolStripMenuItem.Name = "insertTextToolStripMenuItem";
            this.insertTextToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.insertTextToolStripMenuItem.Text = "Insert...";
            this.insertTextToolStripMenuItem.Click += new System.EventHandler(this.insertTextToolStripMenuItem_Click);
            // 
            // translateToCSharpToolStripMenuItem
            // 
            this.translateToCSharpToolStripMenuItem.Name = "translateToCSharpToolStripMenuItem";
            this.translateToCSharpToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.translateToCSharpToolStripMenuItem.Text = "Translate to C#";
            this.translateToCSharpToolStripMenuItem.Click += new System.EventHandler(this.TranslateToCSharpToolStripMenuItem_Click);
            // 
            // translateMaxAmplitudeToCToolStripMenuItem
            // 
            this.translateMaxAmplitudeToCToolStripMenuItem.Name = "translateMaxAmplitudeToCToolStripMenuItem";
            this.translateMaxAmplitudeToCToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.translateMaxAmplitudeToCToolStripMenuItem.Text = "Translate Max Amplitude to C#";
            this.translateMaxAmplitudeToCToolStripMenuItem.Visible = false;
            this.translateMaxAmplitudeToCToolStripMenuItem.Click += new System.EventHandler(this.translateMaxAmplitudeToCToolStripMenuItem_Click);
            // 
            // switchBetweenLegacyAndCToolStripMenuItem
            // 
            this.switchBetweenLegacyAndCToolStripMenuItem.Name = "switchBetweenLegacyAndCToolStripMenuItem";
            this.switchBetweenLegacyAndCToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.switchBetweenLegacyAndCToolStripMenuItem.Text = "Switch Between Legacy and C#";
            this.switchBetweenLegacyAndCToolStripMenuItem.Click += new System.EventHandler(this.SwitchBetweenLegacyAndCToolStripMenuItem_Click);
            // 
            // moduleToolStripMenuItem
            // 
            this.moduleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addModuleToChoicesToolStripMenuItem,
            this.mergeModuleToolStripMenuItem,
            this.copyModuleToolStripMenuItem,
            this.saveIncludeToolStripMenuItem,
            this.copyIncludeToolStripMenuItem});
            this.moduleToolStripMenuItem.Name = "moduleToolStripMenuItem";
            this.moduleToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.moduleToolStripMenuItem.Text = "Module";
            // 
            // addModuleToChoicesToolStripMenuItem
            // 
            this.addModuleToChoicesToolStripMenuItem.Name = "addModuleToChoicesToolStripMenuItem";
            this.addModuleToChoicesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addModuleToChoicesToolStripMenuItem.Text = "Save Module";
            this.addModuleToChoicesToolStripMenuItem.Click += new System.EventHandler(this.AddModuleToChoicesToolStripMenuItem_Click);
            // 
            // mergeModuleToolStripMenuItem
            // 
            this.mergeModuleToolStripMenuItem.Name = "mergeModuleToolStripMenuItem";
            this.mergeModuleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.mergeModuleToolStripMenuItem.Text = "Merge Module";
            this.mergeModuleToolStripMenuItem.Click += new System.EventHandler(this.MergeModuleToolStripMenuItem_Click);
            // 
            // copyModuleToolStripMenuItem
            // 
            this.copyModuleToolStripMenuItem.Name = "copyModuleToolStripMenuItem";
            this.copyModuleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyModuleToolStripMenuItem.Text = "Copy Module";
            this.copyModuleToolStripMenuItem.Click += new System.EventHandler(this.CopyModuleToolStripMenuItem_Click);
            // 
            // saveIncludeToolStripMenuItem
            // 
            this.saveIncludeToolStripMenuItem.Name = "saveIncludeToolStripMenuItem";
            this.saveIncludeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveIncludeToolStripMenuItem.Text = "Save Include";
            this.saveIncludeToolStripMenuItem.Click += new System.EventHandler(this.saveIncludeToolStripMenuItem_Click);
            // 
            // copyIncludeToolStripMenuItem
            // 
            this.copyIncludeToolStripMenuItem.Name = "copyIncludeToolStripMenuItem";
            this.copyIncludeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyIncludeToolStripMenuItem.Text = "Copy Include";
            this.copyIncludeToolStripMenuItem.Click += new System.EventHandler(this.copyIncludeToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyFormulaToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // copyFormulaToolStripMenuItem
            // 
            this.copyFormulaToolStripMenuItem.Name = "copyFormulaToolStripMenuItem";
            this.copyFormulaToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.copyFormulaToolStripMenuItem.Text = "Copy Formula";
            this.copyFormulaToolStripMenuItem.Click += new System.EventHandler(this.copyFormulaToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showPreprocessedCodeOnErrorToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // showPreprocessedCodeOnErrorToolStripMenuItem
            // 
            this.showPreprocessedCodeOnErrorToolStripMenuItem.CheckOnClick = true;
            this.showPreprocessedCodeOnErrorToolStripMenuItem.Name = "showPreprocessedCodeOnErrorToolStripMenuItem";
            this.showPreprocessedCodeOnErrorToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.showPreprocessedCodeOnErrorToolStripMenuItem.Text = "Show Preprocessed Code On Error";
            // 
            // txtFormulaName
            // 
            this.txtFormulaName.Location = new System.Drawing.Point(127, 37);
            this.txtFormulaName.Name = "txtFormulaName";
            this.txtFormulaName.Size = new System.Drawing.Size(254, 23);
            this.txtFormulaName.TabIndex = 22;
            // 
            // lblFormulaName
            // 
            this.lblFormulaName.AutoSize = true;
            this.lblFormulaName.Location = new System.Drawing.Point(16, 40);
            this.lblFormulaName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFormulaName.Name = "lblFormulaName";
            this.lblFormulaName.Size = new System.Drawing.Size(104, 17);
            this.lblFormulaName.TabIndex = 21;
            this.lblFormulaName.Text = "Formula Name:";
            // 
            // cboInsertTokens
            // 
            this.cboInsertTokens.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInsertTokens.FormattingEnabled = true;
            this.cboInsertTokens.Location = new System.Drawing.Point(530, 85);
            this.cboInsertTokens.Margin = new System.Windows.Forms.Padding(2);
            this.cboInsertTokens.Name = "cboInsertTokens";
            this.cboInsertTokens.Size = new System.Drawing.Size(200, 25);
            this.cboInsertTokens.TabIndex = 28;
            this.cboInsertTokens.SelectedIndexChanged += new System.EventHandler(this.cboInsertTokens_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(478, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 17);
            this.label5.TabIndex = 27;
            this.label5.Text = "Insert:";
            // 
            // chkIsCSharpFormula
            // 
            this.chkIsCSharpFormula.AutoSize = true;
            this.chkIsCSharpFormula.Location = new System.Drawing.Point(13, 66);
            this.chkIsCSharpFormula.Name = "chkIsCSharpFormula";
            this.chkIsCSharpFormula.Size = new System.Drawing.Size(113, 21);
            this.chkIsCSharpFormula.TabIndex = 29;
            this.chkIsCSharpFormula.Text = "Is C# Formula";
            this.chkIsCSharpFormula.UseVisualStyleBackColor = true;
            this.chkIsCSharpFormula.CheckedChanged += new System.EventHandler(this.ChkIsCSharpFormula_CheckedChanged);
            // 
            // lnkSavedFormulas
            // 
            this.lnkSavedFormulas.AutoSize = true;
            this.lnkSavedFormulas.Location = new System.Drawing.Point(387, 40);
            this.lnkSavedFormulas.Name = "lnkSavedFormulas";
            this.lnkSavedFormulas.Size = new System.Drawing.Size(122, 17);
            this.lnkSavedFormulas.TabIndex = 30;
            this.lnkSavedFormulas.TabStop = true;
            this.lnkSavedFormulas.Text = "Saved Formulas...";
            this.lnkSavedFormulas.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkSavedFormulas_LinkClicked);
            // 
            // pnlTransform
            // 
            this.pnlTransform.Controls.Add(this.cboSequenceNumber);
            this.pnlTransform.Controls.Add(this.label1);
            this.pnlTransform.Location = new System.Drawing.Point(520, 39);
            this.pnlTransform.Name = "pnlTransform";
            this.pnlTransform.Size = new System.Drawing.Size(210, 37);
            this.pnlTransform.TabIndex = 34;
            // 
            // cboSequenceNumber
            // 
            this.cboSequenceNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSequenceNumber.FormattingEnabled = true;
            this.cboSequenceNumber.Location = new System.Drawing.Point(136, 2);
            this.cboSequenceNumber.Margin = new System.Windows.Forms.Padding(2);
            this.cboSequenceNumber.Name = "cboSequenceNumber";
            this.cboSequenceNumber.Size = new System.Drawing.Size(56, 25);
            this.cboSequenceNumber.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 17);
            this.label1.TabIndex = 34;
            this.label1.Text = "Sequence Number:";
            // 
            // chkShowRawCSharp
            // 
            this.chkShowRawCSharp.AutoSize = true;
            this.chkShowRawCSharp.Location = new System.Drawing.Point(364, 66);
            this.chkShowRawCSharp.Name = "chkShowRawCSharp";
            this.chkShowRawCSharp.Size = new System.Drawing.Size(150, 21);
            this.chkShowRawCSharp.TabIndex = 35;
            this.chkShowRawCSharp.Text = "Show Raw C# Code";
            this.chkShowRawCSharp.UseVisualStyleBackColor = true;
            this.chkShowRawCSharp.CheckedChanged += new System.EventHandler(this.ChkShowRawCSharp_CheckedChanged);
            // 
            // lnkShowErrors
            // 
            this.lnkShowErrors.AutoSize = true;
            this.lnkShowErrors.Location = new System.Drawing.Point(356, 90);
            this.lnkShowErrors.Name = "lnkShowErrors";
            this.lnkShowErrors.Size = new System.Drawing.Size(85, 17);
            this.lnkShowErrors.TabIndex = 36;
            this.lnkShowErrors.TabStop = true;
            this.lnkShowErrors.Text = "Show Errors";
            this.lnkShowErrors.Visible = false;
            this.lnkShowErrors.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkShowErrors_LinkClicked);
            // 
            // lblParseStatus
            // 
            this.lblParseStatus.AutoSize = true;
            this.lblParseStatus.Location = new System.Drawing.Point(156, 93);
            this.lblParseStatus.Name = "lblParseStatus";
            this.lblParseStatus.Size = new System.Drawing.Size(48, 17);
            this.lblParseStatus.TabIndex = 37;
            this.lblParseStatus.Text = "Status";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // chkIsMaxAmpCSharp
            // 
            this.chkIsMaxAmpCSharp.AutoSize = true;
            this.chkIsMaxAmpCSharp.Location = new System.Drawing.Point(173, 359);
            this.chkIsMaxAmpCSharp.Name = "chkIsMaxAmpCSharp";
            this.chkIsMaxAmpCSharp.Size = new System.Drawing.Size(113, 21);
            this.chkIsMaxAmpCSharp.TabIndex = 38;
            this.chkIsMaxAmpCSharp.Text = "Is C# Formula";
            this.chkIsMaxAmpCSharp.UseVisualStyleBackColor = true;
            // 
            // cboFormulaUsage
            // 
            this.cboFormulaUsage.FormattingEnabled = true;
            this.cboFormulaUsage.Location = new System.Drawing.Point(205, 64);
            this.cboFormulaUsage.Name = "cboFormulaUsage";
            this.cboFormulaUsage.Size = new System.Drawing.Size(140, 25);
            this.cboFormulaUsage.TabIndex = 39;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(131, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 17);
            this.label2.TabIndex = 40;
            this.label2.Text = "Category:";
            // 
            // cboDrawType
            // 
            this.cboDrawType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDrawType.FormattingEnabled = true;
            this.cboDrawType.Location = new System.Drawing.Point(459, 9);
            this.cboDrawType.Name = "cboDrawType";
            this.cboDrawType.Size = new System.Drawing.Size(87, 25);
            this.cboDrawType.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(373, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Draw Type:";
            // 
            // chkDrawClosed
            // 
            this.chkDrawClosed.AutoSize = true;
            this.chkDrawClosed.Location = new System.Drawing.Point(561, 9);
            this.chkDrawClosed.Name = "chkDrawClosed";
            this.chkDrawClosed.Size = new System.Drawing.Size(106, 21);
            this.chkDrawClosed.TabIndex = 13;
            this.chkDrawClosed.Text = "Draw Closed";
            this.chkDrawClosed.UseVisualStyleBackColor = true;
            // 
            // OutlineFormulaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 497);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboFormulaUsage);
            this.Controls.Add(this.chkIsMaxAmpCSharp);
            this.Controls.Add(this.lblParseStatus);
            this.Controls.Add(this.lnkShowErrors);
            this.Controls.Add(this.chkShowRawCSharp);
            this.Controls.Add(this.pnlTransform);
            this.Controls.Add(this.lnkSavedFormulas);
            this.Controls.Add(this.chkIsCSharpFormula);
            this.Controls.Add(this.cboInsertTokens);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtFormulaName);
            this.Controls.Add(this.lblFormulaName);
            this.Controls.Add(this.pnlPathSettings);
            this.Controls.Add(this.txtMaxAmplitudeFormula);
            this.Controls.Add(this.lblMaxFormula);
            this.Controls.Add(this.txtFormula);
            this.Controls.Add(this.lblFormula);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(759, 524);
            this.Name = "OutlineFormulaForm";
            this.Text = "Show Errors";
            this.Load += new System.EventHandler(this.OutlineFormulaForm_Load);
            this.Resize += new System.EventHandler(this.OutlineFormulaForm_Resize);
            this.pnlPathSettings.ResumeLayout(false);
            this.pnlPathSettings.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pnlTransform.ResumeLayout(false);
            this.pnlTransform.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblFormula;
        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Label lblMaxFormula;
        private System.Windows.Forms.TextBox txtMaxAmplitudeFormula;
        private System.Windows.Forms.Panel pnlPathSettings;
        private System.Windows.Forms.TextBox txtRotationSpan;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkUseVertices;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem formulaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToChoicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToOriginalToolStripMenuItem;
        private System.Windows.Forms.TextBox txtFormulaName;
        private System.Windows.Forms.Label lblFormulaName;
        private System.Windows.Forms.ComboBox cboInsertTokens;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem parseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertTextToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkIsCSharpFormula;
        private System.Windows.Forms.ToolStripMenuItem translateToCSharpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchBetweenLegacyAndCToolStripMenuItem;
        private System.Windows.Forms.LinkLabel lnkSavedFormulas;
        private System.Windows.Forms.ToolStripMenuItem moduleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addModuleToChoicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeModuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyModuleToolStripMenuItem;
        private System.Windows.Forms.Panel pnlTransform;
        private System.Windows.Forms.ComboBox cboSequenceNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPreprocessedCodeOnErrorToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkShowRawCSharp;
        private System.Windows.Forms.LinkLabel lnkShowErrors;
        private System.Windows.Forms.Label lblParseStatus;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox chkIsMaxAmpCSharp;
        private System.Windows.Forms.ToolStripMenuItem translateMaxAmplitudeToCToolStripMenuItem;
        private System.Windows.Forms.ComboBox cboFormulaUsage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem copyIncludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveIncludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyFormulaToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboDrawType;
        private System.Windows.Forms.CheckBox chkDrawClosed;
    }
}