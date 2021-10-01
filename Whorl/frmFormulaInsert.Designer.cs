namespace Whorl
{
    partial class frmFormulaInsert
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabClipboard = new System.Windows.Forms.TabPage();
            this.dgvClipboard = new System.Windows.Forms.DataGridView();
            this.lnkInsertClipboardText = new System.Windows.Forms.DataGridViewLinkColumn();
            this.tabMembers = new System.Windows.Forms.TabPage();
            this.chkShowStaticMembers = new System.Windows.Forms.CheckBox();
            this.dgvMemberInfo = new System.Windows.Forms.DataGridView();
            this.lnkInsertMember = new System.Windows.Forms.DataGridViewLinkColumn();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabOther = new System.Windows.Forms.TabPage();
            this.pnlOther = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cboOtherMode = new System.Windows.Forms.ComboBox();
            this.dgvOther = new System.Windows.Forms.DataGridView();
            this.lnkInsertOtherText = new System.Windows.Forms.DataGridViewLinkColumn();
            this.clipboardTextDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.standardFormulaTextBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.memberInfoInfoDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.memberItemInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl1.SuspendLayout();
            this.tabClipboard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClipboard)).BeginInit();
            this.tabMembers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMemberInfo)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.tabOther.SuspendLayout();
            this.pnlOther.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOther)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.standardFormulaTextBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memberItemInfoBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabClipboard);
            this.tabControl1.Controls.Add(this.tabMembers);
            this.tabControl1.Controls.Add(this.tabOther);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.Location = new System.Drawing.Point(0, 45);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(669, 192);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabClipboard
            // 
            this.tabClipboard.Controls.Add(this.dgvClipboard);
            this.tabClipboard.Location = new System.Drawing.Point(4, 22);
            this.tabClipboard.Name = "tabClipboard";
            this.tabClipboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabClipboard.Size = new System.Drawing.Size(661, 166);
            this.tabClipboard.TabIndex = 0;
            this.tabClipboard.Text = "Clipboard";
            this.tabClipboard.UseVisualStyleBackColor = true;
            // 
            // dgvClipboard
            // 
            this.dgvClipboard.AllowUserToAddRows = false;
            this.dgvClipboard.AutoGenerateColumns = false;
            this.dgvClipboard.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvClipboard.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.lnkInsertClipboardText,
            this.clipboardTextDataGridViewTextBoxColumn});
            this.dgvClipboard.DataSource = this.standardFormulaTextBindingSource;
            this.dgvClipboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvClipboard.Location = new System.Drawing.Point(3, 3);
            this.dgvClipboard.Name = "dgvClipboard";
            this.dgvClipboard.ReadOnly = true;
            this.dgvClipboard.Size = new System.Drawing.Size(655, 160);
            this.dgvClipboard.TabIndex = 0;
            this.dgvClipboard.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvClipboard_CellContentClick);
            this.dgvClipboard.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dgvClipboard_UserDeletingRow);
            // 
            // lnkInsertClipboardText
            // 
            this.lnkInsertClipboardText.HeaderText = "";
            this.lnkInsertClipboardText.Name = "lnkInsertClipboardText";
            this.lnkInsertClipboardText.ReadOnly = true;
            this.lnkInsertClipboardText.Text = "Insert";
            this.lnkInsertClipboardText.UseColumnTextForLinkValue = true;
            this.lnkInsertClipboardText.Width = 70;
            // 
            // tabMembers
            // 
            this.tabMembers.Controls.Add(this.chkShowStaticMembers);
            this.tabMembers.Controls.Add(this.dgvMemberInfo);
            this.tabMembers.Controls.Add(this.cboType);
            this.tabMembers.Controls.Add(this.label1);
            this.tabMembers.Location = new System.Drawing.Point(4, 22);
            this.tabMembers.Name = "tabMembers";
            this.tabMembers.Padding = new System.Windows.Forms.Padding(3);
            this.tabMembers.Size = new System.Drawing.Size(661, 166);
            this.tabMembers.TabIndex = 1;
            this.tabMembers.Text = "Type Members";
            this.tabMembers.UseVisualStyleBackColor = true;
            // 
            // chkShowStaticMembers
            // 
            this.chkShowStaticMembers.AutoSize = true;
            this.chkShowStaticMembers.Location = new System.Drawing.Point(299, 10);
            this.chkShowStaticMembers.Name = "chkShowStaticMembers";
            this.chkShowStaticMembers.Size = new System.Drawing.Size(129, 17);
            this.chkShowStaticMembers.TabIndex = 3;
            this.chkShowStaticMembers.Text = "Show Static Members";
            this.chkShowStaticMembers.UseVisualStyleBackColor = true;
            this.chkShowStaticMembers.CheckedChanged += new System.EventHandler(this.chkShowStaticMembers_CheckedChanged);
            // 
            // dgvMemberInfo
            // 
            this.dgvMemberInfo.AllowUserToAddRows = false;
            this.dgvMemberInfo.AllowUserToDeleteRows = false;
            this.dgvMemberInfo.AllowUserToResizeRows = false;
            this.dgvMemberInfo.AutoGenerateColumns = false;
            this.dgvMemberInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMemberInfo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.lnkInsertMember,
            this.memberInfoInfoDataGridViewTextBoxColumn});
            this.dgvMemberInfo.DataSource = this.memberItemInfoBindingSource;
            this.dgvMemberInfo.Location = new System.Drawing.Point(3, 34);
            this.dgvMemberInfo.Name = "dgvMemberInfo";
            this.dgvMemberInfo.Size = new System.Drawing.Size(655, 135);
            this.dgvMemberInfo.TabIndex = 2;
            this.dgvMemberInfo.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMemberInfo_CellContentClick);
            // 
            // lnkInsertMember
            // 
            this.lnkInsertMember.FillWeight = 70F;
            this.lnkInsertMember.HeaderText = "";
            this.lnkInsertMember.Name = "lnkInsertMember";
            this.lnkInsertMember.ReadOnly = true;
            this.lnkInsertMember.Text = "Insert";
            this.lnkInsertMember.UseColumnTextForLinkValue = true;
            this.lnkInsertMember.Width = 70;
            // 
            // cboType
            // 
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(48, 7);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(228, 21);
            this.cboType.TabIndex = 1;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(669, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(599, 27);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(63, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "MemberInfoInfo";
            this.dataGridViewTextBoxColumn1.HeaderText = "Type Member";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 350;
            // 
            // tabOther
            // 
            this.tabOther.Controls.Add(this.pnlOther);
            this.tabOther.Location = new System.Drawing.Point(4, 22);
            this.tabOther.Name = "tabOther";
            this.tabOther.Size = new System.Drawing.Size(661, 166);
            this.tabOther.TabIndex = 2;
            this.tabOther.Text = "Other";
            this.tabOther.UseVisualStyleBackColor = true;
            // 
            // pnlOther
            // 
            this.pnlOther.Controls.Add(this.dgvOther);
            this.pnlOther.Controls.Add(this.label2);
            this.pnlOther.Controls.Add(this.cboOtherMode);
            this.pnlOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOther.Location = new System.Drawing.Point(0, 0);
            this.pnlOther.Name = "pnlOther";
            this.pnlOther.Size = new System.Drawing.Size(661, 166);
            this.pnlOther.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Mode:";
            // 
            // cboOtherMode
            // 
            this.cboOtherMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOtherMode.FormattingEnabled = true;
            this.cboOtherMode.Location = new System.Drawing.Point(53, 3);
            this.cboOtherMode.Name = "cboOtherMode";
            this.cboOtherMode.Size = new System.Drawing.Size(136, 21);
            this.cboOtherMode.TabIndex = 2;
            this.cboOtherMode.SelectedIndexChanged += new System.EventHandler(this.cboOtherMode_SelectedIndexChanged);
            // 
            // dgvOther
            // 
            this.dgvOther.AllowUserToAddRows = false;
            this.dgvOther.AllowUserToDeleteRows = false;
            this.dgvOther.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOther.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.lnkInsertOtherText});
            this.dgvOther.Location = new System.Drawing.Point(3, 30);
            this.dgvOther.Name = "dgvOther";
            this.dgvOther.ReadOnly = true;
            this.dgvOther.Size = new System.Drawing.Size(650, 133);
            this.dgvOther.TabIndex = 4;
            this.dgvOther.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOther_CellContentClick);
            // 
            // lnkInsertOtherText
            // 
            this.lnkInsertOtherText.HeaderText = "";
            this.lnkInsertOtherText.Name = "lnkInsertOtherText";
            this.lnkInsertOtherText.ReadOnly = true;
            this.lnkInsertOtherText.Text = "Insert";
            this.lnkInsertOtherText.UseColumnTextForLinkValue = true;
            this.lnkInsertOtherText.Width = 70;
            // 
            // clipboardTextDataGridViewTextBoxColumn
            // 
            this.clipboardTextDataGridViewTextBoxColumn.DataPropertyName = "ClipboardText";
            this.clipboardTextDataGridViewTextBoxColumn.HeaderText = "Text";
            this.clipboardTextDataGridViewTextBoxColumn.Name = "clipboardTextDataGridViewTextBoxColumn";
            this.clipboardTextDataGridViewTextBoxColumn.ReadOnly = true;
            this.clipboardTextDataGridViewTextBoxColumn.Width = 400;
            // 
            // standardFormulaTextBindingSource
            // 
            this.standardFormulaTextBindingSource.DataSource = typeof(Whorl.StandardFormulaText);
            // 
            // memberInfoInfoDataGridViewTextBoxColumn
            // 
            this.memberInfoInfoDataGridViewTextBoxColumn.DataPropertyName = "MemberInfoInfo";
            this.memberInfoInfoDataGridViewTextBoxColumn.HeaderText = "Type Member";
            this.memberInfoInfoDataGridViewTextBoxColumn.Name = "memberInfoInfoDataGridViewTextBoxColumn";
            this.memberInfoInfoDataGridViewTextBoxColumn.ReadOnly = true;
            this.memberInfoInfoDataGridViewTextBoxColumn.Width = 400;
            // 
            // memberItemInfoBindingSource
            // 
            this.memberItemInfoBindingSource.DataSource = typeof(Whorl.MemberItemInfo);
            // 
            // frmFormulaInsert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 237);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "frmFormulaInsert";
            this.Text = "Insert Text in Formula";
            this.tabControl1.ResumeLayout(false);
            this.tabClipboard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvClipboard)).EndInit();
            this.tabMembers.ResumeLayout(false);
            this.tabMembers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMemberInfo)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabOther.ResumeLayout(false);
            this.pnlOther.ResumeLayout(false);
            this.pnlOther.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOther)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.standardFormulaTextBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memberItemInfoBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabClipboard;
        private System.Windows.Forms.TabPage tabMembers;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.DataGridView dgvClipboard;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvMemberInfo;
        private System.Windows.Forms.CheckBox chkShowStaticMembers;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.BindingSource standardFormulaTextBindingSource;
        private System.Windows.Forms.BindingSource memberItemInfoBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewLinkColumn lnkInsertClipboardText;
        private System.Windows.Forms.DataGridViewTextBoxColumn clipboardTextDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewLinkColumn lnkInsertMember;
        private System.Windows.Forms.DataGridViewTextBoxColumn memberInfoInfoDataGridViewTextBoxColumn;
        private System.Windows.Forms.TabPage tabOther;
        private System.Windows.Forms.Panel pnlOther;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboOtherMode;
        private System.Windows.Forms.DataGridView dgvOther;
        private System.Windows.Forms.DataGridViewLinkColumn lnkInsertOtherText;
    }
}