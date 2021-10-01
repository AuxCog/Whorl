namespace Whorl
{
    partial class TransformFormulaForm
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
            this.txtTransformFormula = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtTransformName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboSequenceNumber = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.formulaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToChoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToOriginalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translateToCSharpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchBetweenLegacyAndCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cboInsertTokens = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkIsCSharpFormula = new System.Windows.Forms.CheckBox();
            this.lnkSavedFormulas = new System.Windows.Forms.LinkLabel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtTransformFormula
            // 
            this.txtTransformFormula.Location = new System.Drawing.Point(12, 100);
            this.txtTransformFormula.Multiline = true;
            this.txtTransformFormula.Name = "txtTransformFormula";
            this.txtTransformFormula.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTransformFormula.Size = new System.Drawing.Size(557, 420);
            this.txtTransformFormula.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Transform Formula:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(511, 13);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 22);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(433, 13);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(64, 22);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Transform Name:";
            // 
            // txtTransformName
            // 
            this.txtTransformName.Location = new System.Drawing.Point(105, 39);
            this.txtTransformName.Margin = new System.Windows.Forms.Padding(2);
            this.txtTransformName.Name = "txtTransformName";
            this.txtTransformName.Size = new System.Drawing.Size(192, 20);
            this.txtTransformName.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(310, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Sequence Number:";
            // 
            // cboSequenceNumber
            // 
            this.cboSequenceNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSequenceNumber.FormattingEnabled = true;
            this.cboSequenceNumber.Location = new System.Drawing.Point(412, 39);
            this.cboSequenceNumber.Margin = new System.Windows.Forms.Padding(2);
            this.cboSequenceNumber.Name = "cboSequenceNumber";
            this.cboSequenceNumber.Size = new System.Drawing.Size(56, 21);
            this.cboSequenceNumber.TabIndex = 15;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.formulaToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(581, 24);
            this.menuStrip1.TabIndex = 18;
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
            this.switchBetweenLegacyAndCToolStripMenuItem});
            this.formulaToolStripMenuItem.Name = "formulaToolStripMenuItem";
            this.formulaToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.formulaToolStripMenuItem.Text = "Formula";
            // 
            // addToChoicesToolStripMenuItem
            // 
            this.addToChoicesToolStripMenuItem.Name = "addToChoicesToolStripMenuItem";
            this.addToChoicesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.addToChoicesToolStripMenuItem.Text = "Add to Choices";
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
            // switchBetweenLegacyAndCToolStripMenuItem
            // 
            this.switchBetweenLegacyAndCToolStripMenuItem.Name = "switchBetweenLegacyAndCToolStripMenuItem";
            this.switchBetweenLegacyAndCToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.switchBetweenLegacyAndCToolStripMenuItem.Text = "Switch between Legacy and C#";
            this.switchBetweenLegacyAndCToolStripMenuItem.Click += new System.EventHandler(this.SwitchBetweenLegacyAndCToolStripMenuItem_Click);
            // 
            // cboInsertTokens
            // 
            this.cboInsertTokens.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInsertTokens.FormattingEnabled = true;
            this.cboInsertTokens.Location = new System.Drawing.Point(386, 74);
            this.cboInsertTokens.Margin = new System.Windows.Forms.Padding(2);
            this.cboInsertTokens.Name = "cboInsertTokens";
            this.cboInsertTokens.Size = new System.Drawing.Size(183, 21);
            this.cboInsertTokens.TabIndex = 22;
            this.cboInsertTokens.SelectedIndexChanged += new System.EventHandler(this.cboInsertTokens_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(345, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Insert:";
            // 
            // chkIsCSharpFormula
            // 
            this.chkIsCSharpFormula.AutoSize = true;
            this.chkIsCSharpFormula.Location = new System.Drawing.Point(134, 76);
            this.chkIsCSharpFormula.Name = "chkIsCSharpFormula";
            this.chkIsCSharpFormula.Size = new System.Drawing.Size(102, 17);
            this.chkIsCSharpFormula.TabIndex = 30;
            this.chkIsCSharpFormula.Text = "Use C# Formula";
            this.chkIsCSharpFormula.UseVisualStyleBackColor = true;
            // 
            // lnkSavedFormulas
            // 
            this.lnkSavedFormulas.AutoSize = true;
            this.lnkSavedFormulas.Location = new System.Drawing.Point(483, 42);
            this.lnkSavedFormulas.Name = "lnkSavedFormulas";
            this.lnkSavedFormulas.Size = new System.Drawing.Size(92, 13);
            this.lnkSavedFormulas.TabIndex = 31;
            this.lnkSavedFormulas.TabStop = true;
            this.lnkSavedFormulas.Text = "Saved Formulas...";
            this.lnkSavedFormulas.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkSavedFormulas_LinkClicked);
            // 
            // TransformFormulaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(581, 532);
            this.Controls.Add(this.lnkSavedFormulas);
            this.Controls.Add(this.chkIsCSharpFormula);
            this.Controls.Add(this.cboInsertTokens);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboSequenceNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtTransformName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtTransformFormula);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TransformFormulaForm";
            this.Text = "Transform Formula";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTransformFormula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTransformName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboSequenceNumber;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem formulaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToChoicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToOriginalToolStripMenuItem;
        private System.Windows.Forms.ComboBox cboInsertTokens;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem parseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertTextToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkIsCSharpFormula;
        private System.Windows.Forms.ToolStripMenuItem translateToCSharpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchBetweenLegacyAndCToolStripMenuItem;
        private System.Windows.Forms.LinkLabel lnkSavedFormulas;
    }
}