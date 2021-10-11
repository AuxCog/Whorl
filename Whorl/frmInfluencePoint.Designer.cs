
namespace Whorl
{
    partial class frmInfluencePoint
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblPointID = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtInfluenceFactor = new System.Windows.Forms.TextBox();
            this.txtDivisor = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.txtPower = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cboTransformFunction = new System.Windows.Forms.ComboBox();
            this.txtFunctionOffset = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID: ";
            // 
            // lblPointID
            // 
            this.lblPointID.AutoSize = true;
            this.lblPointID.Location = new System.Drawing.Point(37, 13);
            this.lblPointID.Name = "lblPointID";
            this.lblPointID.Size = new System.Drawing.Size(10, 13);
            this.lblPointID.TabIndex = 1;
            this.lblPointID.Text = " ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Influence Factor:";
            // 
            // txtInfluenceFactor
            // 
            this.txtInfluenceFactor.Location = new System.Drawing.Point(108, 47);
            this.txtInfluenceFactor.Name = "txtInfluenceFactor";
            this.txtInfluenceFactor.Size = new System.Drawing.Size(58, 20);
            this.txtInfluenceFactor.TabIndex = 3;
            // 
            // txtDivisor
            // 
            this.txtDivisor.Location = new System.Drawing.Point(243, 47);
            this.txtDivisor.Name = "txtDivisor";
            this.txtDivisor.Size = new System.Drawing.Size(58, 20);
            this.txtDivisor.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(195, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Divisor:";
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(108, 82);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(58, 20);
            this.txtOffset.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(60, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Offset:";
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(105, 168);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(56, 23);
            this.BtnOK.TabIndex = 8;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(177, 168);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(66, 23);
            this.BtnCancel.TabIndex = 9;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(108, 13);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(65, 17);
            this.chkEnabled.TabIndex = 10;
            this.chkEnabled.Text = "Enabled";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // txtPower
            // 
            this.txtPower.Location = new System.Drawing.Point(243, 82);
            this.txtPower.Name = "txtPower";
            this.txtPower.Size = new System.Drawing.Size(58, 20);
            this.txtPower.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(195, 85);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Power:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Transform Function:";
            // 
            // cboTransformFunction
            // 
            this.cboTransformFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTransformFunction.FormattingEnabled = true;
            this.cboTransformFunction.Location = new System.Drawing.Point(108, 113);
            this.cboTransformFunction.Name = "cboTransformFunction";
            this.cboTransformFunction.Size = new System.Drawing.Size(135, 21);
            this.cboTransformFunction.TabIndex = 14;
            // 
            // txtFunctionOffset
            // 
            this.txtFunctionOffset.Location = new System.Drawing.Point(108, 142);
            this.txtFunctionOffset.Name = "txtFunctionOffset";
            this.txtFunctionOffset.Size = new System.Drawing.Size(58, 20);
            this.txtFunctionOffset.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 145);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Function Offset:";
            // 
            // frmInfluencePoint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 228);
            this.Controls.Add(this.txtFunctionOffset);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cboTransformFunction);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtPower);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.txtOffset);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDivisor);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtInfluenceFactor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblPointID);
            this.Controls.Add(this.label1);
            this.Name = "frmInfluencePoint";
            this.Text = "Edit Influence Point";
            this.Load += new System.EventHandler(this.frmInfluencePoint_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblPointID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtInfluenceFactor;
        private System.Windows.Forms.TextBox txtDivisor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.TextBox txtPower;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cboTransformFunction;
        private System.Windows.Forms.TextBox txtFunctionOffset;
        private System.Windows.Forms.Label label7;
    }
}