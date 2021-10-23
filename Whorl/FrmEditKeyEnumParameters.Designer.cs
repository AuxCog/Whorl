
namespace Whorl
{
    partial class FrmEditKeyEnumParameters
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
            this.pnlParameters = new System.Windows.Forms.Panel();
            this.BtnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.cboEnumKey = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnEditInMainForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pnlParameters
            // 
            this.pnlParameters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlParameters.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlParameters.Location = new System.Drawing.Point(0, 41);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(684, 370);
            this.pnlParameters.TabIndex = 0;
            // 
            // BtnClose
            // 
            this.BtnClose.Location = new System.Drawing.Point(617, 12);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(55, 23);
            this.BtnClose.TabIndex = 11;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(72, 14);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(142, 21);
            this.cboCategory.TabIndex = 13;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.cboCategory_SelectedIndexChanged);
            // 
            // cboEnumKey
            // 
            this.cboEnumKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEnumKey.FormattingEnabled = true;
            this.cboEnumKey.Location = new System.Drawing.Point(298, 14);
            this.cboEnumKey.Name = "cboEnumKey";
            this.cboEnumKey.Size = new System.Drawing.Size(181, 21);
            this.cboEnumKey.TabIndex = 15;
            this.cboEnumKey.SelectedIndexChanged += new System.EventHandler(this.cboEnumKey_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(234, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Enum Key:";
            // 
            // BtnEditInMainForm
            // 
            this.BtnEditInMainForm.Location = new System.Drawing.Point(500, 12);
            this.BtnEditInMainForm.Name = "BtnEditInMainForm";
            this.BtnEditInMainForm.Size = new System.Drawing.Size(98, 23);
            this.BtnEditInMainForm.TabIndex = 16;
            this.BtnEditInMainForm.Text = "Edit in Main Form";
            this.BtnEditInMainForm.UseVisualStyleBackColor = true;
            this.BtnEditInMainForm.Click += new System.EventHandler(this.BtnEditInMainForm_Click);
            // 
            // FrmEditKeyEnumParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 411);
            this.Controls.Add(this.BtnEditInMainForm);
            this.Controls.Add(this.cboEnumKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnClose);
            this.Controls.Add(this.pnlParameters);
            this.Name = "FrmEditKeyEnumParameters";
            this.Text = "Edit Key Parameters";
            this.Load += new System.EventHandler(this.FrmEditKeyEnumParameters_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlParameters;
        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.ComboBox cboEnumKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnEditInMainForm;
    }
}