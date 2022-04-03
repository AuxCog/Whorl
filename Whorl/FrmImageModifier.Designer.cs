
namespace Whorl
{
    partial class FrmImageModifier
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
            this.picModifiedColor = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboBoundsMode = new System.Windows.Forms.ComboBox();
            this.cboColorMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.BtnModifyImage = new System.Windows.Forms.Button();
            this.chkCumulative = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picModifiedColor)).BeginInit();
            this.SuspendLayout();
            // 
            // picModifiedColor
            // 
            this.picModifiedColor.BackColor = System.Drawing.Color.Black;
            this.picModifiedColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picModifiedColor.Location = new System.Drawing.Point(95, 12);
            this.picModifiedColor.Name = "picModifiedColor";
            this.picModifiedColor.Size = new System.Drawing.Size(54, 50);
            this.picModifiedColor.TabIndex = 0;
            this.picModifiedColor.TabStop = false;
            this.picModifiedColor.Click += new System.EventHandler(this.picModifiedColor_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Modified Color:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(169, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Bounds Mode:";
            // 
            // cboBoundsMode
            // 
            this.cboBoundsMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBoundsMode.FormattingEnabled = true;
            this.cboBoundsMode.Location = new System.Drawing.Point(251, 9);
            this.cboBoundsMode.Name = "cboBoundsMode";
            this.cboBoundsMode.Size = new System.Drawing.Size(121, 21);
            this.cboBoundsMode.TabIndex = 3;
            // 
            // cboColorMode
            // 
            this.cboColorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboColorMode.FormattingEnabled = true;
            this.cboColorMode.Location = new System.Drawing.Point(251, 41);
            this.cboColorMode.Name = "cboColorMode";
            this.cboColorMode.Size = new System.Drawing.Size(121, 21);
            this.cboColorMode.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(169, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Color Mode:";
            // 
            // BtnModifyImage
            // 
            this.BtnModifyImage.Location = new System.Drawing.Point(393, 7);
            this.BtnModifyImage.Name = "BtnModifyImage";
            this.BtnModifyImage.Size = new System.Drawing.Size(75, 23);
            this.BtnModifyImage.TabIndex = 6;
            this.BtnModifyImage.Text = "Modify";
            this.BtnModifyImage.UseVisualStyleBackColor = true;
            this.BtnModifyImage.Click += new System.EventHandler(this.BtnModifyImage_Click);
            // 
            // chkCumulative
            // 
            this.chkCumulative.AutoSize = true;
            this.chkCumulative.Location = new System.Drawing.Point(393, 44);
            this.chkCumulative.Name = "chkCumulative";
            this.chkCumulative.Size = new System.Drawing.Size(78, 17);
            this.chkCumulative.TabIndex = 7;
            this.chkCumulative.Text = "Cumulative";
            this.chkCumulative.UseVisualStyleBackColor = true;
            // 
            // FrmImageModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 74);
            this.Controls.Add(this.chkCumulative);
            this.Controls.Add(this.BtnModifyImage);
            this.Controls.Add(this.cboColorMode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboBoundsMode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picModifiedColor);
            this.Name = "FrmImageModifier";
            this.Text = "Image Modifier";
            this.Load += new System.EventHandler(this.FrmImageModifier_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picModifiedColor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picModifiedColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboBoundsMode;
        private System.Windows.Forms.ComboBox cboColorMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BtnModifyImage;
        private System.Windows.Forms.CheckBox chkCumulative;
    }
}