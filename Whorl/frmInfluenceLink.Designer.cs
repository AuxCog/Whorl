
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
            this.label5 = new System.Windows.Forms.Label();
            this.BtnCreateRandomSettings = new System.Windows.Forms.Button();
            this.BtnEditRandomSettings = new System.Windows.Forms.Button();
            this.BtnDeleteRandomSettings = new System.Windows.Forms.Button();
            this.BtnReseedRandom = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPointRandomWeight = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtParentRandomWeight = new System.Windows.Forms.TextBox();
            this.BtnApply = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transform:";
            // 
            // lblTransformName
            // 
            this.lblTransformName.AutoSize = true;
            this.lblTransformName.Location = new System.Drawing.Point(75, 36);
            this.lblTransformName.Name = "lblTransformName";
            this.lblTransformName.Size = new System.Drawing.Size(85, 13);
            this.lblTransformName.TabIndex = 1;
            this.lblTransformName.Text = "Transform Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Influence Point Id:";
            // 
            // cboInfluencePointInfo
            // 
            this.cboInfluencePointInfo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInfluencePointInfo.FormattingEnabled = true;
            this.cboInfluencePointInfo.Location = new System.Drawing.Point(115, 123);
            this.cboInfluencePointInfo.Name = "cboInfluencePointInfo";
            this.cboInfluencePointInfo.Size = new System.Drawing.Size(97, 21);
            this.cboInfluencePointInfo.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Influence Factor:";
            // 
            // txtInfluenceFactor
            // 
            this.txtInfluenceFactor.Enabled = false;
            this.txtInfluenceFactor.Location = new System.Drawing.Point(115, 151);
            this.txtInfluenceFactor.Name = "txtInfluenceFactor";
            this.txtInfluenceFactor.Size = new System.Drawing.Size(75, 20);
            this.txtInfluenceFactor.TabIndex = 5;
            this.txtInfluenceFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // BtnCreateLink
            // 
            this.BtnCreateLink.Location = new System.Drawing.Point(30, 67);
            this.BtnCreateLink.Name = "BtnCreateLink";
            this.BtnCreateLink.Size = new System.Drawing.Size(75, 23);
            this.BtnCreateLink.TabIndex = 6;
            this.BtnCreateLink.Text = "Create Link";
            this.BtnCreateLink.UseVisualStyleBackColor = true;
            this.BtnCreateLink.Click += new System.EventHandler(this.BtnCreateLink_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(280, 39);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 7;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(280, 68);
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
            this.BtnDeleteLink.Location = new System.Drawing.Point(113, 67);
            this.BtnDeleteLink.Name = "BtnDeleteLink";
            this.BtnDeleteLink.Size = new System.Drawing.Size(75, 23);
            this.BtnDeleteLink.TabIndex = 9;
            this.BtnDeleteLink.Text = "Delete Link";
            this.BtnDeleteLink.UseVisualStyleBackColor = true;
            this.BtnDeleteLink.Click += new System.EventHandler(this.BtnDeleteLink_Click);
            // 
            // BtnEditInfluencePoint
            // 
            this.BtnEditInfluencePoint.Location = new System.Drawing.Point(218, 123);
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
            this.cboParameter.Location = new System.Drawing.Point(115, 96);
            this.cboParameter.Name = "cboParameter";
            this.cboParameter.Size = new System.Drawing.Size(144, 21);
            this.cboParameter.TabIndex = 12;
            this.cboParameter.SelectedIndexChanged += new System.EventHandler(this.cboParameter_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(45, 104);
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
            this.chkMultiply.Location = new System.Drawing.Point(115, 183);
            this.chkMultiply.Name = "chkMultiply";
            this.chkMultiply.Size = new System.Drawing.Size(94, 17);
            this.chkMultiply.TabIndex = 13;
            this.chkMultiply.Text = "Multiply Factor";
            this.chkMultiply.UseVisualStyleBackColor = true;
            // 
            // BtnFindLink
            // 
            this.BtnFindLink.Location = new System.Drawing.Point(194, 67);
            this.BtnFindLink.Name = "BtnFindLink";
            this.BtnFindLink.Size = new System.Drawing.Size(65, 23);
            this.BtnFindLink.TabIndex = 14;
            this.BtnFindLink.Text = "Find Link";
            this.BtnFindLink.UseVisualStyleBackColor = true;
            this.BtnFindLink.Click += new System.EventHandler(this.BtnFindLink_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 212);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Random Settings:";
            // 
            // BtnCreateRandomSettings
            // 
            this.BtnCreateRandomSettings.Location = new System.Drawing.Point(113, 207);
            this.BtnCreateRandomSettings.Name = "BtnCreateRandomSettings";
            this.BtnCreateRandomSettings.Size = new System.Drawing.Size(55, 23);
            this.BtnCreateRandomSettings.TabIndex = 16;
            this.BtnCreateRandomSettings.Text = "Create";
            this.BtnCreateRandomSettings.UseVisualStyleBackColor = true;
            this.BtnCreateRandomSettings.Click += new System.EventHandler(this.BtnCreateRandomSettings_Click);
            // 
            // BtnEditRandomSettings
            // 
            this.BtnEditRandomSettings.Location = new System.Drawing.Point(174, 207);
            this.BtnEditRandomSettings.Name = "BtnEditRandomSettings";
            this.BtnEditRandomSettings.Size = new System.Drawing.Size(55, 23);
            this.BtnEditRandomSettings.TabIndex = 17;
            this.BtnEditRandomSettings.Text = "Edit";
            this.BtnEditRandomSettings.UseVisualStyleBackColor = true;
            this.BtnEditRandomSettings.Click += new System.EventHandler(this.BtnEditRandomSettings_Click);
            // 
            // BtnDeleteRandomSettings
            // 
            this.BtnDeleteRandomSettings.Location = new System.Drawing.Point(296, 207);
            this.BtnDeleteRandomSettings.Name = "BtnDeleteRandomSettings";
            this.BtnDeleteRandomSettings.Size = new System.Drawing.Size(55, 23);
            this.BtnDeleteRandomSettings.TabIndex = 18;
            this.BtnDeleteRandomSettings.Text = "Delete";
            this.BtnDeleteRandomSettings.UseVisualStyleBackColor = true;
            this.BtnDeleteRandomSettings.Click += new System.EventHandler(this.BtnDeleteRandomSettings_Click);
            // 
            // BtnReseedRandom
            // 
            this.BtnReseedRandom.Location = new System.Drawing.Point(235, 207);
            this.BtnReseedRandom.Name = "BtnReseedRandom";
            this.BtnReseedRandom.Size = new System.Drawing.Size(55, 23);
            this.BtnReseedRandom.TabIndex = 19;
            this.BtnReseedRandom.Text = "Reseed";
            this.BtnReseedRandom.UseVisualStyleBackColor = true;
            this.BtnReseedRandom.Click += new System.EventHandler(this.BtnReseedRandom_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 248);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Parent Random Weight:";
            // 
            // txtPointRandomWeight
            // 
            this.txtPointRandomWeight.Enabled = false;
            this.txtPointRandomWeight.Location = new System.Drawing.Point(147, 274);
            this.txtPointRandomWeight.Name = "txtPointRandomWeight";
            this.txtPointRandomWeight.Size = new System.Drawing.Size(75, 20);
            this.txtPointRandomWeight.TabIndex = 23;
            this.txtPointRandomWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 277);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Point Random Weight:";
            // 
            // txtParentRandomWeight
            // 
            this.txtParentRandomWeight.Enabled = false;
            this.txtParentRandomWeight.Location = new System.Drawing.Point(147, 245);
            this.txtParentRandomWeight.Name = "txtParentRandomWeight";
            this.txtParentRandomWeight.Size = new System.Drawing.Size(75, 20);
            this.txtParentRandomWeight.TabIndex = 24;
            this.txtParentRandomWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // BtnApply
            // 
            this.BtnApply.Location = new System.Drawing.Point(280, 99);
            this.BtnApply.Name = "BtnApply";
            this.BtnApply.Size = new System.Drawing.Size(75, 23);
            this.BtnApply.TabIndex = 25;
            this.BtnApply.Text = "Apply";
            this.BtnApply.UseVisualStyleBackColor = true;
            this.BtnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(363, 24);
            this.menuStrip1.TabIndex = 26;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // frmInfluenceLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 301);
            this.Controls.Add(this.BtnApply);
            this.Controls.Add(this.txtParentRandomWeight);
            this.Controls.Add(this.txtPointRandomWeight);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.BtnReseedRandom);
            this.Controls.Add(this.BtnDeleteRandomSettings);
            this.Controls.Add(this.BtnEditRandomSettings);
            this.Controls.Add(this.BtnCreateRandomSettings);
            this.Controls.Add(this.label5);
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
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmInfluenceLink";
            this.Text = "Edit Influence Link";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button BtnCreateRandomSettings;
        private System.Windows.Forms.Button BtnEditRandomSettings;
        private System.Windows.Forms.Button BtnDeleteRandomSettings;
        private System.Windows.Forms.Button BtnReseedRandom;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPointRandomWeight;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtParentRandomWeight;
        private System.Windows.Forms.Button BtnApply;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
    }
}