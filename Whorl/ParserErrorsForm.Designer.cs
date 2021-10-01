namespace Whorl
{
    partial class ParserErrorsForm
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
            this.dgvErrorInfo = new System.Windows.Forms.DataGridView();
            this.SelectButtonColumn = new System.Windows.Forms.DataGridViewLinkColumn();
            this.MessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrorInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvErrorInfo
            // 
            this.dgvErrorInfo.AllowUserToAddRows = false;
            this.dgvErrorInfo.AllowUserToDeleteRows = false;
            this.dgvErrorInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrorInfo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SelectButtonColumn,
            this.MessageColumn});
            this.dgvErrorInfo.Location = new System.Drawing.Point(2, 3);
            this.dgvErrorInfo.Name = "dgvErrorInfo";
            this.dgvErrorInfo.ReadOnly = true;
            this.dgvErrorInfo.Size = new System.Drawing.Size(824, 128);
            this.dgvErrorInfo.TabIndex = 0;
            this.dgvErrorInfo.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvErrorInfo_CellContentClick);
            // 
            // SelectButtonColumn
            // 
            this.SelectButtonColumn.HeaderText = "";
            this.SelectButtonColumn.Name = "SelectButtonColumn";
            this.SelectButtonColumn.ReadOnly = true;
            this.SelectButtonColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.SelectButtonColumn.Text = "Select";
            this.SelectButtonColumn.UseColumnTextForLinkValue = true;
            this.SelectButtonColumn.Width = 70;
            // 
            // MessageColumn
            // 
            this.MessageColumn.DataPropertyName = "Message";
            this.MessageColumn.HeaderText = "Error Message";
            this.MessageColumn.Name = "MessageColumn";
            this.MessageColumn.ReadOnly = true;
            this.MessageColumn.Width = 700;
            // 
            // ParserErrorsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 132);
            this.Controls.Add(this.dgvErrorInfo);
            this.Name = "ParserErrorsForm";
            this.Text = "Parse Errors";
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrorInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvErrorInfo;
        private System.Windows.Forms.DataGridViewLinkColumn SelectButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MessageColumn;
    }
}