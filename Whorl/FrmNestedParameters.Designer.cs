
namespace Whorl
{
    partial class FrmNestedParameters
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
            this.BtnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblParentParameterName = new System.Windows.Forms.Label();
            this.pnlParameters = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.Location = new System.Drawing.Point(288, 12);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(53, 23);
            this.BtnClose.TabIndex = 12;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Parent Parameter Name:";
            // 
            // lblParentParameterName
            // 
            this.lblParentParameterName.AutoSize = true;
            this.lblParentParameterName.Location = new System.Drawing.Point(142, 17);
            this.lblParentParameterName.Name = "lblParentParameterName";
            this.lblParentParameterName.Size = new System.Drawing.Size(124, 13);
            this.lblParentParameterName.TabIndex = 14;
            this.lblParentParameterName.Text = "lblParentParameterName";
            // 
            // pnlParameters
            // 
            this.pnlParameters.AutoScroll = true;
            this.pnlParameters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlParameters.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlParameters.Location = new System.Drawing.Point(0, 41);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(353, 430);
            this.pnlParameters.TabIndex = 15;
            // 
            // FrmNestedParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(353, 471);
            this.Controls.Add(this.pnlParameters);
            this.Controls.Add(this.lblParentParameterName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnClose);
            this.Name = "FrmNestedParameters";
            this.Text = "Edit Nested Parameters";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblParentParameterName;
        private System.Windows.Forms.Panel pnlParameters;
    }
}