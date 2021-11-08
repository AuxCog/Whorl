
namespace Whorl
{
    partial class FrmRandomValues
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.txtSmoothness = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ChkClosed = new System.Windows.Forms.CheckBox();
            this.ChkClipValues = new System.Windows.Forms.CheckBox();
            this.chkReseedRandom = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboDomainType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(220, 45);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(220, 16);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Strength:";
            // 
            // txtWeight
            // 
            this.txtWeight.Location = new System.Drawing.Point(95, 18);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.Size = new System.Drawing.Size(100, 20);
            this.txtWeight.TabIndex = 7;
            this.txtWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSmoothness
            // 
            this.txtSmoothness.Location = new System.Drawing.Point(95, 44);
            this.txtSmoothness.Name = "txtSmoothness";
            this.txtSmoothness.Size = new System.Drawing.Size(100, 20);
            this.txtSmoothness.TabIndex = 9;
            this.txtSmoothness.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Smoothness:";
            // 
            // ChkClosed
            // 
            this.ChkClosed.AutoSize = true;
            this.ChkClosed.Location = new System.Drawing.Point(16, 110);
            this.ChkClosed.Name = "ChkClosed";
            this.ChkClosed.Size = new System.Drawing.Size(136, 17);
            this.ChkClosed.TabIndex = 10;
            this.ChkClosed.Text = "Generate Closed Curve";
            this.ChkClosed.UseVisualStyleBackColor = true;
            // 
            // ChkClipValues
            // 
            this.ChkClipValues.AutoSize = true;
            this.ChkClipValues.Location = new System.Drawing.Point(178, 110);
            this.ChkClipValues.Name = "ChkClipValues";
            this.ChkClipValues.Size = new System.Drawing.Size(116, 17);
            this.ChkClipValues.TabIndex = 11;
            this.ChkClipValues.Text = "Clip Random Value";
            this.ChkClipValues.UseVisualStyleBackColor = true;
            // 
            // chkReseedRandom
            // 
            this.chkReseedRandom.AutoSize = true;
            this.chkReseedRandom.Location = new System.Drawing.Point(16, 133);
            this.chkReseedRandom.Name = "chkReseedRandom";
            this.chkReseedRandom.Size = new System.Drawing.Size(106, 17);
            this.chkReseedRandom.TabIndex = 12;
            this.chkReseedRandom.Text = "Reseed Random";
            this.chkReseedRandom.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Domain Type:";
            // 
            // cboDomainType
            // 
            this.cboDomainType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDomainType.FormattingEnabled = true;
            this.cboDomainType.Location = new System.Drawing.Point(95, 73);
            this.cboDomainType.Name = "cboDomainType";
            this.cboDomainType.Size = new System.Drawing.Size(125, 21);
            this.cboDomainType.TabIndex = 14;
            // 
            // FrmRandomValues
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 177);
            this.Controls.Add(this.cboDomainType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkReseedRandom);
            this.Controls.Add(this.ChkClipValues);
            this.Controls.Add(this.ChkClosed);
            this.Controls.Add(this.txtSmoothness);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtWeight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "FrmRandomValues";
            this.Text = "Edit Random Settings";
            this.Load += new System.EventHandler(this.FrmRandomValues_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWeight;
        private System.Windows.Forms.TextBox txtSmoothness;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ChkClosed;
        private System.Windows.Forms.CheckBox ChkClipValues;
        private System.Windows.Forms.CheckBox chkReseedRandom;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboDomainType;
    }
}