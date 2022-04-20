namespace Whorl
{
    partial class frmFormulaEntries
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
            this.dgvFormulas = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDeleteButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.pnlTypeFilters = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.cboIsCSharp = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.btnCopyFormula = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cboFormulaType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFormulaName = new System.Windows.Forms.TextBox();
            this.ChkIsSystem = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFormulas)).BeginInit();
            this.pnlFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvFormulas
            // 
            this.dgvFormulas.AllowUserToAddRows = false;
            this.dgvFormulas.AllowUserToDeleteRows = false;
            this.dgvFormulas.AllowUserToOrderColumns = true;
            this.dgvFormulas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFormulas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.colDeleteButton});
            this.dgvFormulas.Location = new System.Drawing.Point(3, 60);
            this.dgvFormulas.Name = "dgvFormulas";
            this.dgvFormulas.Size = new System.Drawing.Size(468, 433);
            this.dgvFormulas.TabIndex = 0;
            this.dgvFormulas.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvFormulas_CellContentClick);
            this.dgvFormulas.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvFormulas_RowEnter);
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "FormulaName";
            this.Column1.HeaderText = "Formula Name";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 150;
            // 
            // Column2
            // 
            this.Column2.DataPropertyName = "FormulaType";
            this.Column2.HeaderText = "Type";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.DataPropertyName = "IsCSharp";
            this.Column3.HeaderText = "C#";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 40;
            // 
            // colDeleteButton
            // 
            this.colDeleteButton.HeaderText = "";
            this.colDeleteButton.Name = "colDeleteButton";
            this.colDeleteButton.ReadOnly = true;
            this.colDeleteButton.Text = "Delete";
            this.colDeleteButton.UseColumnTextForButtonValue = true;
            this.colDeleteButton.Width = 50;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filters:";
            // 
            // pnlFilters
            // 
            this.pnlFilters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlFilters.Controls.Add(this.pnlTypeFilters);
            this.pnlFilters.Controls.Add(this.label3);
            this.pnlFilters.Controls.Add(this.cboIsCSharp);
            this.pnlFilters.Controls.Add(this.label2);
            this.pnlFilters.Location = new System.Drawing.Point(43, 5);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(814, 49);
            this.pnlFilters.TabIndex = 3;
            // 
            // pnlTypeFilters
            // 
            this.pnlTypeFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTypeFilters.Location = new System.Drawing.Point(220, 4);
            this.pnlTypeFilters.Name = "pnlTypeFilters";
            this.pnlTypeFilters.Size = new System.Drawing.Size(587, 38);
            this.pnlTypeFilters.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(135, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Formula Types:";
            // 
            // cboIsCSharp
            // 
            this.cboIsCSharp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIsCSharp.FormattingEnabled = true;
            this.cboIsCSharp.Location = new System.Drawing.Point(47, 10);
            this.cboIsCSharp.Name = "cboIsCSharp";
            this.cboIsCSharp.Size = new System.Drawing.Size(64, 21);
            this.cboIsCSharp.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Is C#:";
            // 
            // txtFormula
            // 
            this.txtFormula.Location = new System.Drawing.Point(481, 111);
            this.txtFormula.Multiline = true;
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.ReadOnly = true;
            this.txtFormula.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFormula.Size = new System.Drawing.Size(486, 381);
            this.txtFormula.TabIndex = 4;
            // 
            // btnCopyFormula
            // 
            this.btnCopyFormula.Location = new System.Drawing.Point(872, 4);
            this.btnCopyFormula.Name = "btnCopyFormula";
            this.btnCopyFormula.Size = new System.Drawing.Size(95, 23);
            this.btnCopyFormula.TabIndex = 5;
            this.btnCopyFormula.Text = "Copy Formula";
            this.btnCopyFormula.UseVisualStyleBackColor = true;
            this.btnCopyFormula.Click += new System.EventHandler(this.BtnCopyFormula_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(873, 31);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(95, 23);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(478, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Formula Type:";
            // 
            // cboFormulaType
            // 
            this.cboFormulaType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFormulaType.FormattingEnabled = true;
            this.cboFormulaType.Location = new System.Drawing.Point(558, 60);
            this.cboFormulaType.Name = "cboFormulaType";
            this.cboFormulaType.Size = new System.Drawing.Size(158, 21);
            this.cboFormulaType.TabIndex = 8;
            this.cboFormulaType.SelectedIndexChanged += new System.EventHandler(this.CboFormulaType_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(722, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Name:";
            // 
            // txtFormulaName
            // 
            this.txtFormulaName.AcceptsReturn = true;
            this.txtFormulaName.Location = new System.Drawing.Point(766, 60);
            this.txtFormulaName.Name = "txtFormulaName";
            this.txtFormulaName.Size = new System.Drawing.Size(201, 20);
            this.txtFormulaName.TabIndex = 10;
            this.txtFormulaName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtFormulaName_KeyDown);
            // 
            // ChkIsSystem
            // 
            this.ChkIsSystem.AutoSize = true;
            this.ChkIsSystem.Location = new System.Drawing.Point(481, 88);
            this.ChkIsSystem.Name = "ChkIsSystem";
            this.ChkIsSystem.Size = new System.Drawing.Size(100, 17);
            this.ChkIsSystem.TabIndex = 11;
            this.ChkIsSystem.Text = "System Formula";
            this.ChkIsSystem.UseVisualStyleBackColor = true;
            this.ChkIsSystem.CheckedChanged += new System.EventHandler(this.ChkIsSystem_CheckedChanged);
            // 
            // frmFormulaEntries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 499);
            this.ControlBox = false;
            this.Controls.Add(this.ChkIsSystem);
            this.Controls.Add(this.txtFormulaName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboFormulaType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCopyFormula);
            this.Controls.Add(this.txtFormula);
            this.Controls.Add(this.pnlFilters);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvFormulas);
            this.Name = "frmFormulaEntries";
            this.Text = "Formula Entries";
            ((System.ComponentModel.ISupportInitialize)(this.dgvFormulas)).EndInit();
            this.pnlFilters.ResumeLayout(false);
            this.pnlFilters.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvFormulas;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.ComboBox cboIsCSharp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Button btnCopyFormula;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlTypeFilters;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboFormulaType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFormulaName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column3;
        private System.Windows.Forms.DataGridViewButtonColumn colDeleteButton;
        private System.Windows.Forms.CheckBox ChkIsSystem;
    }
}