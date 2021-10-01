
namespace Whorl
{
    partial class frmInfluenceLink
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
            this.lblTransformName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboInfluencePointInfo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtInfluenceFactor = new System.Windows.Forms.TextBox();
            this.BtnCreateLink = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnDeleteLink = new System.Windows.Forms.Button();
            this.BtnEditInfluencePoint = new System.Windows.Forms.Button();
            this.cboParameter = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkMultiply = new System.Windows.Forms.CheckBox();
            this.BtnFindLink = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transform:";
            // 
            // lblTransformName
            // 
            this.lblTransformName.AutoSize = true;
            this.lblTransformName.Location = new System.Drawing.Point(75, 9);
            this.lblTransformName.Name = "lblTransformName";
            this.lblTransformName.Size = new System.Drawing.Size(85, 13);
            this.lblTransformName.TabIndex = 1;
            this.lblTransformName.Text = "Transform Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Influence Point Id:";
            // 
            // cboInfluencePointInfo
            // 
            this.cboInfluencePointInfo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInfluencePointInfo.FormattingEnabled = true;
            this.cboInfluencePointInfo.Location = new System.Drawing.Point(115, 96);
            this.cboInfluencePointInfo.Name = "cboInfluencePointInfo";
            this.cboInfluencePointInfo.Size = new System.Drawing.Size(97, 21);
            this.cboInfluencePointInfo.TabIndex = 3;
            this.cboInfluencePointInfo.SelectedIndexChanged += new System.EventHandler(this.cboInfluencePointInfo_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Influence Factor:";
            // 
            // txtInfluenceFactor
            // 
            this.txtInfluenceFactor.Enabled = false;
            this.txtInfluenceFactor.Location = new System.Drawing.Point(115, 124);
            this.txtInfluenceFactor.Name = "txtInfluenceFactor";
            this.txtInfluenceFactor.Size = new System.Drawing.Size(75, 20);
            this.txtInfluenceFactor.TabIndex = 5;
            this.txtInfluenceFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // BtnCreateLink
            // 
            this.BtnCreateLink.Location = new System.Drawing.Point(30, 40);
            this.BtnCreateLink.Name = "BtnCreateLink";
            this.BtnCreateLink.Size = new System.Drawing.Size(75, 23);
            this.BtnCreateLink.TabIndex = 6;
            this.BtnCreateLink.Text = "Create Link";
            this.BtnCreateLink.UseVisualStyleBackColor = true;
            this.BtnCreateLink.Click += new System.EventHandler(this.BtnCreateLink_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(15, 189);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 7;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(115, 189);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 8;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnDeleteLink
            // 
            this.BtnDeleteLink.Enabled = false;
            this.BtnDeleteLink.Location = new System.Drawing.Point(113, 40);
            this.BtnDeleteLink.Name = "BtnDeleteLink";
            this.BtnDeleteLink.Size = new System.Drawing.Size(75, 23);
            this.BtnDeleteLink.TabIndex = 9;
            this.BtnDeleteLink.Text = "Delete Link";
            this.BtnDeleteLink.UseVisualStyleBackColor = true;
            this.BtnDeleteLink.Click += new System.EventHandler(this.BtnDeleteLink_Click);
            // 
            // BtnEditInfluencePoint
            // 
            this.BtnEditInfluencePoint.Location = new System.Drawing.Point(218, 96);
            this.BtnEditInfluencePoint.Name = "BtnEditInfluencePoint";
            this.BtnEditInfluencePoint.Size = new System.Drawing.Size(41, 21);
            this.BtnEditInfluencePoint.TabIndex = 10;
            this.BtnEditInfluencePoint.Text = "Edit";
            this.BtnEditInfluencePoint.UseVisualStyleBackColor = true;
            this.BtnEditInfluencePoint.Click += new System.EventHandler(this.BtnEditInfluencePoint_Click);
            // 
            // cboParameter
            // 
            this.cboParameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParameter.FormattingEnabled = true;
            this.cboParameter.Location = new System.Drawing.Point(115, 69);
            this.cboParameter.Name = "cboParameter";
            this.cboParameter.Size = new System.Drawing.Size(144, 21);
            this.cboParameter.TabIndex = 12;
            this.cboParameter.SelectedIndexChanged += new System.EventHandler(this.cboParameter_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Parameter:";
            // 
            // chkMultiply
            // 
            this.chkMultiply.AutoSize = true;
            this.chkMultiply.Checked = true;
            this.chkMultiply.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMultiply.Enabled = false;
            this.chkMultiply.Location = new System.Drawing.Point(115, 156);
            this.chkMultiply.Name = "chkMultiply";
            this.chkMultiply.Size = new System.Drawing.Size(94, 17);
            this.chkMultiply.TabIndex = 13;
            this.chkMultiply.Text = "Multiply Factor";
            this.chkMultiply.UseVisualStyleBackColor = true;
            // 
            // BtnFindLink
            // 
            this.BtnFindLink.Location = new System.Drawing.Point(194, 40);
            this.BtnFindLink.Name = "BtnFindLink";
            this.BtnFindLink.Size = new System.Drawing.Size(65, 23);
            this.BtnFindLink.TabIndex = 14;
            this.BtnFindLink.Text = "Find Link";
            this.BtnFindLink.UseVisualStyleBackColor = true;
            this.BtnFindLink.Click += new System.EventHandler(this.BtnFindLink_Click);
            // 
            // frmInflenceLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 224);
            this.Controls.Add(this.BtnFindLink);
            this.Controls.Add(this.chkMultiply);
            this.Controls.Add(this.cboParameter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BtnEditInfluencePoint);
            this.Controls.Add(this.BtnDeleteLink);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.BtnCreateLink);
            this.Controls.Add(this.txtInfluenceFactor);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboInfluencePointInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblTransformName);
            this.Controls.Add(this.label1);
            this.Name = "frmInflenceLink";
            this.Text = "Edit Influence Link";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTransformName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboInfluencePointInfo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtInfluenceFactor;
        private System.Windows.Forms.Button BtnCreateLink;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnDeleteLink;
        private System.Windows.Forms.Button BtnEditInfluencePoint;
        private System.Windows.Forms.ComboBox cboParameter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkMultiply;
        private System.Windows.Forms.Button BtnFindLink;
    }
}