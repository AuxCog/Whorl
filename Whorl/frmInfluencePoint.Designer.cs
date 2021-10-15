
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
            this.txtAverageWeight = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
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
            this.txtDivisor.Location = new System.Drawing.Point(108, 73);
            this.txtDivisor.Name = "txtDivisor";
            this.txtDivisor.Size = new System.Drawing.Size(58, 20);
            this.txtDivisor.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(60, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Divisor:";
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(108, 101);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(58, 20);
            this.txtOffset.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(60, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Offset:";
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(108, 170);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(56, 23);
            this.BtnOK.TabIndex = 8;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(180, 170);
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
            this.chkEnabled.Size = new System.Drawing.Size(91, 17);
            this.chkEnabled.TabIndex = 10;
            this.chkEnabled.Text = "Enable Single";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // txtPower
            // 
            this.txtPower.Location = new System.Drawing.Point(270, 73);
            this.txtPower.Name = "txtPower";
            this.txtPower.Size = new System.Drawing.Size(58, 20);
            this.txtPower.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(222, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Power:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 135);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Transform Function:";
            // 
            // cboTransformFunction
            // 
            this.cboTransformFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTransformFunction.FormattingEnabled = true;
            this.cboTransformFunction.Location = new System.Drawing.Point(108, 132);
            this.cboTransformFunction.Name = "cboTransformFunction";
            this.cboTransformFunction.Size = new System.Drawing.Size(135, 21);
            this.cboTransformFunction.TabIndex = 14;
            // 
            // txtFunctionOffset
            // 
            this.txtFunctionOffset.Location = new System.Drawing.Point(270, 101);
            this.txtFunctionOffset.Name = "txtFunctionOffset";
            this.txtFunctionOffset.Size = new System.Drawing.Size(58, 20);
            this.txtFunctionOffset.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(182, 104);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Function Offset:";
            // 
            // txtAverageWeight
            // 
            this.txtAverageWeight.Location = new System.Drawing.Point(270, 47);
            this.txtAverageWeight.Name = "txtAverageWeight";
            this.txtAverageWeight.Size = new System.Drawing.Size(58, 20);
            this.txtAverageWeight.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(177, 50);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Average Weight:";
            // 
            // frmInfluencePoint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 204);
            this.Controls.Add(this.txtAverageWeight);
            this.Controls.Add(this.label8);
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
        private System.Windows.Forms.TextBox txtAverageWeight;
        private System.Windows.Forms.Label label8;
    }
}