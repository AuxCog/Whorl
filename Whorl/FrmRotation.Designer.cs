namespace Whorl
{
    partial class FrmRotation
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
            this.txtRotationDegrees = new System.Windows.Forms.TextBox();
            this.chkUseImageCenter = new System.Windows.Forms.CheckBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rotation Degrees:";
            // 
            // txtRotationDegrees
            // 
            this.txtRotationDegrees.Location = new System.Drawing.Point(113, 13);
            this.txtRotationDegrees.Name = "txtRotationDegrees";
            this.txtRotationDegrees.Size = new System.Drawing.Size(72, 20);
            this.txtRotationDegrees.TabIndex = 1;
            // 
            // chkUseImageCenter
            // 
            this.chkUseImageCenter.AutoSize = true;
            this.chkUseImageCenter.Checked = true;
            this.chkUseImageCenter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseImageCenter.Location = new System.Drawing.Point(16, 46);
            this.chkUseImageCenter.Name = "chkUseImageCenter";
            this.chkUseImageCenter.Size = new System.Drawing.Size(111, 17);
            this.chkUseImageCenter.TabIndex = 2;
            this.chkUseImageCenter.Text = "Use Image Center";
            this.chkUseImageCenter.UseVisualStyleBackColor = true;
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(16, 70);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 3;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(113, 69);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 4;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // FrmRotation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(202, 101);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.chkUseImageCenter);
            this.Controls.Add(this.txtRotationDegrees);
            this.Controls.Add(this.label1);
            this.Name = "FrmRotation";
            this.Text = "FrmRotation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRotationDegrees;
        private System.Windows.Forms.CheckBox chkUseImageCenter;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
    }
}