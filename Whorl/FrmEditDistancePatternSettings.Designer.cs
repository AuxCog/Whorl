
namespace Whorl
{
    partial class FrmEditDistancePatternSettings
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
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.ChkUseFadeout = new System.Windows.Forms.CheckBox();
            this.pnlFadeOut = new System.Windows.Forms.Panel();
            this.ChkAutoEnd = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtEndValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEndPercentage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStartPercentage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cboInfluencePointId = new System.Windows.Forms.ComboBox();
            this.ChkEnabled = new System.Windows.Forms.CheckBox();
            this.ChkCenterSlope = new System.Windows.Forms.CheckBox();
            this.txtCenterSlope = new System.Windows.Forms.TextBox();
            this.pnlFadeOut.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(192, 12);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(66, 23);
            this.BtnCancel.TabIndex = 11;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(120, 12);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(56, 23);
            this.BtnOK.TabIndex = 10;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // ChkUseFadeout
            // 
            this.ChkUseFadeout.AutoSize = true;
            this.ChkUseFadeout.Location = new System.Drawing.Point(12, 115);
            this.ChkUseFadeout.Name = "ChkUseFadeout";
            this.ChkUseFadeout.Size = new System.Drawing.Size(236, 17);
            this.ChkUseFadeout.TabIndex = 12;
            this.ChkUseFadeout.Text = "Use Fade Out when Outside Pattern Bounds";
            this.ChkUseFadeout.UseVisualStyleBackColor = true;
            this.ChkUseFadeout.CheckedChanged += new System.EventHandler(this.ChkUseFadeout_CheckedChanged);
            // 
            // pnlFadeOut
            // 
            this.pnlFadeOut.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlFadeOut.Controls.Add(this.ChkAutoEnd);
            this.pnlFadeOut.Controls.Add(this.label4);
            this.pnlFadeOut.Controls.Add(this.txtEndValue);
            this.pnlFadeOut.Controls.Add(this.label3);
            this.pnlFadeOut.Controls.Add(this.txtEndPercentage);
            this.pnlFadeOut.Controls.Add(this.label2);
            this.pnlFadeOut.Controls.Add(this.txtStartPercentage);
            this.pnlFadeOut.Controls.Add(this.label1);
            this.pnlFadeOut.Location = new System.Drawing.Point(12, 140);
            this.pnlFadeOut.Name = "pnlFadeOut";
            this.pnlFadeOut.Size = new System.Drawing.Size(246, 164);
            this.pnlFadeOut.TabIndex = 13;
            // 
            // ChkAutoEnd
            // 
            this.ChkAutoEnd.AutoSize = true;
            this.ChkAutoEnd.Location = new System.Drawing.Point(20, 101);
            this.ChkAutoEnd.Name = "ChkAutoEnd";
            this.ChkAutoEnd.Size = new System.Drawing.Size(100, 17);
            this.ChkAutoEnd.TabIndex = 7;
            this.ChkAutoEnd.Text = "Auto End Value";
            this.ChkAutoEnd.UseVisualStyleBackColor = true;
            this.ChkAutoEnd.CheckedChanged += new System.EventHandler(this.ChkAutoEnd_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(17, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Fade Out Settings:";
            // 
            // txtEndValue
            // 
            this.txtEndValue.Location = new System.Drawing.Point(122, 128);
            this.txtEndValue.Name = "txtEndValue";
            this.txtEndValue.Size = new System.Drawing.Size(93, 20);
            this.txtEndValue.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "End Distance Value:";
            // 
            // txtEndPercentage
            // 
            this.txtEndPercentage.Location = new System.Drawing.Point(122, 69);
            this.txtEndPercentage.Name = "txtEndPercentage";
            this.txtEndPercentage.Size = new System.Drawing.Size(93, 20);
            this.txtEndPercentage.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "End Percentage:";
            // 
            // txtStartPercentage
            // 
            this.txtStartPercentage.Location = new System.Drawing.Point(122, 34);
            this.txtStartPercentage.Name = "txtStartPercentage";
            this.txtStartPercentage.Size = new System.Drawing.Size(93, 20);
            this.txtStartPercentage.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Percentage:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Influence Point Id:";
            // 
            // cboInfluencePointId
            // 
            this.cboInfluencePointId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInfluencePointId.FormattingEnabled = true;
            this.cboInfluencePointId.Location = new System.Drawing.Point(111, 45);
            this.cboInfluencePointId.Name = "cboInfluencePointId";
            this.cboInfluencePointId.Size = new System.Drawing.Size(61, 21);
            this.cboInfluencePointId.TabIndex = 15;
            // 
            // ChkEnabled
            // 
            this.ChkEnabled.AutoSize = true;
            this.ChkEnabled.Location = new System.Drawing.Point(15, 16);
            this.ChkEnabled.Name = "ChkEnabled";
            this.ChkEnabled.Size = new System.Drawing.Size(65, 17);
            this.ChkEnabled.TabIndex = 16;
            this.ChkEnabled.Text = "Enabled";
            this.ChkEnabled.UseVisualStyleBackColor = true;
            // 
            // ChkCenterSlope
            // 
            this.ChkCenterSlope.AutoSize = true;
            this.ChkCenterSlope.Location = new System.Drawing.Point(15, 83);
            this.ChkCenterSlope.Name = "ChkCenterSlope";
            this.ChkCenterSlope.Size = new System.Drawing.Size(90, 17);
            this.ChkCenterSlope.TabIndex = 17;
            this.ChkCenterSlope.Text = "Center Slope:";
            this.ChkCenterSlope.UseVisualStyleBackColor = true;
            // 
            // txtCenterSlope
            // 
            this.txtCenterSlope.Location = new System.Drawing.Point(111, 81);
            this.txtCenterSlope.Name = "txtCenterSlope";
            this.txtCenterSlope.Size = new System.Drawing.Size(84, 20);
            this.txtCenterSlope.TabIndex = 18;
            // 
            // FrmEditDistancePatternSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(271, 315);
            this.Controls.Add(this.txtCenterSlope);
            this.Controls.Add(this.ChkCenterSlope);
            this.Controls.Add(this.ChkEnabled);
            this.Controls.Add(this.cboInfluencePointId);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pnlFadeOut);
            this.Controls.Add(this.ChkUseFadeout);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Name = "FrmEditDistancePatternSettings";
            this.Text = "Edit Distance Pattern Settings";
            this.pnlFadeOut.ResumeLayout(false);
            this.pnlFadeOut.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.CheckBox ChkUseFadeout;
        private System.Windows.Forms.Panel pnlFadeOut;
        private System.Windows.Forms.TextBox txtEndPercentage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtStartPercentage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtEndValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboInfluencePointId;
        private System.Windows.Forms.CheckBox ChkAutoEnd;
        private System.Windows.Forms.CheckBox ChkEnabled;
        private System.Windows.Forms.CheckBox ChkCenterSlope;
        private System.Windows.Forms.TextBox txtCenterSlope;
    }
}