namespace Whorl
{
    partial class BackgroundForm
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
            this.components = new System.ComponentModel.Container();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.chooseColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addColorToChoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setTransparencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlColor = new System.Windows.Forms.Panel();
            this.btnSwapColors = new System.Windows.Forms.Button();
            this.chkSolidColor = new System.Windows.Forms.CheckBox();
            this.btnCenterColor = new System.Windows.Forms.Button();
            this.btnBoundaryColor = new System.Windows.Forms.Button();
            this.lblCenterColor = new System.Windows.Forms.Label();
            this.lblBoundaryColor = new System.Windows.Forms.Label();
            this.pnlImage = new System.Windows.Forms.Panel();
            this.cboImageMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBrowseBackgroundImageFile = new System.Windows.Forms.Button();
            this.txtBackgroundImageFileName = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.chkUseBackgroundImage = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.pnlColor.SuspendLayout();
            this.pnlImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(444, 49);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 28);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(444, 13);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(85, 28);
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picPreview.Location = new System.Drawing.Point(218, 13);
            this.picPreview.Margin = new System.Windows.Forms.Padding(4);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(218, 175);
            this.picPreview.TabIndex = 17;
            this.picPreview.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chooseColorToolStripMenuItem,
            this.addColorToChoicesToolStripMenuItem,
            this.setTransparencyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(226, 82);
            // 
            // chooseColorToolStripMenuItem
            // 
            this.chooseColorToolStripMenuItem.Name = "chooseColorToolStripMenuItem";
            this.chooseColorToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.chooseColorToolStripMenuItem.Text = "Choose Color";
            this.chooseColorToolStripMenuItem.Click += new System.EventHandler(this.chooseColorToolStripMenuItem_Click);
            // 
            // addColorToChoicesToolStripMenuItem
            // 
            this.addColorToChoicesToolStripMenuItem.Name = "addColorToChoicesToolStripMenuItem";
            this.addColorToChoicesToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.addColorToChoicesToolStripMenuItem.Text = "Add Color to Choices";
            this.addColorToChoicesToolStripMenuItem.Click += new System.EventHandler(this.addColorToChoicesToolStripMenuItem_Click);
            // 
            // setTransparencyToolStripMenuItem
            // 
            this.setTransparencyToolStripMenuItem.Name = "setTransparencyToolStripMenuItem";
            this.setTransparencyToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.setTransparencyToolStripMenuItem.Text = "Set Transparency";
            this.setTransparencyToolStripMenuItem.Click += new System.EventHandler(this.setTransparencyToolStripMenuItem_Click);
            // 
            // pnlColor
            // 
            this.pnlColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlColor.Controls.Add(this.btnSwapColors);
            this.pnlColor.Controls.Add(this.chkSolidColor);
            this.pnlColor.Controls.Add(this.btnCenterColor);
            this.pnlColor.Controls.Add(this.btnBoundaryColor);
            this.pnlColor.Controls.Add(this.lblCenterColor);
            this.pnlColor.Controls.Add(this.lblBoundaryColor);
            this.pnlColor.Location = new System.Drawing.Point(11, 13);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(197, 175);
            this.pnlColor.TabIndex = 32;
            // 
            // btnSwapColors
            // 
            this.btnSwapColors.Location = new System.Drawing.Point(14, 103);
            this.btnSwapColors.Margin = new System.Windows.Forms.Padding(4);
            this.btnSwapColors.Name = "btnSwapColors";
            this.btnSwapColors.Size = new System.Drawing.Size(100, 28);
            this.btnSwapColors.TabIndex = 37;
            this.btnSwapColors.Text = "Swap Colors";
            this.btnSwapColors.UseVisualStyleBackColor = true;
            this.btnSwapColors.Click += new System.EventHandler(this.btnSwapColors_Click);
            // 
            // chkSolidColor
            // 
            this.chkSolidColor.AutoSize = true;
            this.chkSolidColor.Location = new System.Drawing.Point(13, 139);
            this.chkSolidColor.Margin = new System.Windows.Forms.Padding(4);
            this.chkSolidColor.Name = "chkSolidColor";
            this.chkSolidColor.Size = new System.Drawing.Size(127, 21);
            this.chkSolidColor.TabIndex = 36;
            this.chkSolidColor.Text = "Use Solid Color";
            this.chkSolidColor.UseVisualStyleBackColor = true;
            this.chkSolidColor.CheckedChanged += new System.EventHandler(this.chkSolidColor_CheckedChanged);
            // 
            // btnCenterColor
            // 
            this.btnCenterColor.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCenterColor.Location = new System.Drawing.Point(129, 56);
            this.btnCenterColor.Margin = new System.Windows.Forms.Padding(4);
            this.btnCenterColor.Name = "btnCenterColor";
            this.btnCenterColor.Size = new System.Drawing.Size(53, 42);
            this.btnCenterColor.TabIndex = 35;
            this.btnCenterColor.UseVisualStyleBackColor = false;
            this.btnCenterColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorButton_MouseDown);
            // 
            // btnBoundaryColor
            // 
            this.btnBoundaryColor.Location = new System.Drawing.Point(129, 9);
            this.btnBoundaryColor.Margin = new System.Windows.Forms.Padding(4);
            this.btnBoundaryColor.Name = "btnBoundaryColor";
            this.btnBoundaryColor.Size = new System.Drawing.Size(53, 42);
            this.btnBoundaryColor.TabIndex = 34;
            this.btnBoundaryColor.UseVisualStyleBackColor = true;
            this.btnBoundaryColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorButton_MouseDown);
            // 
            // lblCenterColor
            // 
            this.lblCenterColor.AutoSize = true;
            this.lblCenterColor.Location = new System.Drawing.Point(10, 70);
            this.lblCenterColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCenterColor.Name = "lblCenterColor";
            this.lblCenterColor.Size = new System.Drawing.Size(91, 17);
            this.lblCenterColor.TabIndex = 33;
            this.lblCenterColor.Text = "Center Color:";
            // 
            // lblBoundaryColor
            // 
            this.lblBoundaryColor.AutoSize = true;
            this.lblBoundaryColor.Location = new System.Drawing.Point(10, 23);
            this.lblBoundaryColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBoundaryColor.Name = "lblBoundaryColor";
            this.lblBoundaryColor.Size = new System.Drawing.Size(110, 17);
            this.lblBoundaryColor.TabIndex = 32;
            this.lblBoundaryColor.Text = "Boundary Color:";
            // 
            // pnlImage
            // 
            this.pnlImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlImage.Controls.Add(this.cboImageMode);
            this.pnlImage.Controls.Add(this.label1);
            this.pnlImage.Controls.Add(this.btnBrowseBackgroundImageFile);
            this.pnlImage.Controls.Add(this.txtBackgroundImageFileName);
            this.pnlImage.Controls.Add(this.label16);
            this.pnlImage.Enabled = false;
            this.pnlImage.Location = new System.Drawing.Point(11, 223);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(535, 72);
            this.pnlImage.TabIndex = 33;
            // 
            // cboImageMode
            // 
            this.cboImageMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImageMode.FormattingEnabled = true;
            this.cboImageMode.Location = new System.Drawing.Point(167, 40);
            this.cboImageMode.Name = "cboImageMode";
            this.cboImageMode.Size = new System.Drawing.Size(139, 24);
            this.cboImageMode.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 17);
            this.label1.TabIndex = 34;
            this.label1.Text = "Image Mode:";
            // 
            // btnBrowseBackgroundImageFile
            // 
            this.btnBrowseBackgroundImageFile.Location = new System.Drawing.Point(434, 8);
            this.btnBrowseBackgroundImageFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseBackgroundImageFile.Name = "btnBrowseBackgroundImageFile";
            this.btnBrowseBackgroundImageFile.Size = new System.Drawing.Size(91, 28);
            this.btnBrowseBackgroundImageFile.TabIndex = 33;
            this.btnBrowseBackgroundImageFile.Text = "Browse...";
            this.btnBrowseBackgroundImageFile.UseVisualStyleBackColor = true;
            this.btnBrowseBackgroundImageFile.Click += new System.EventHandler(this.btnBrowseBackgroundImageFile_Click);
            // 
            // txtBackgroundImageFileName
            // 
            this.txtBackgroundImageFileName.Location = new System.Drawing.Point(167, 11);
            this.txtBackgroundImageFileName.Name = "txtBackgroundImageFileName";
            this.txtBackgroundImageFileName.Size = new System.Drawing.Size(250, 22);
            this.txtBackgroundImageFileName.TabIndex = 32;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(4, 14);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(156, 17);
            this.label16.TabIndex = 31;
            this.label16.Text = "Background Image File:";
            // 
            // chkUseBackgroundImage
            // 
            this.chkUseBackgroundImage.AutoSize = true;
            this.chkUseBackgroundImage.Location = new System.Drawing.Point(10, 195);
            this.chkUseBackgroundImage.Margin = new System.Windows.Forms.Padding(4);
            this.chkUseBackgroundImage.Name = "chkUseBackgroundImage";
            this.chkUseBackgroundImage.Size = new System.Drawing.Size(177, 21);
            this.chkUseBackgroundImage.TabIndex = 39;
            this.chkUseBackgroundImage.Text = "Use Background Image";
            this.chkUseBackgroundImage.UseVisualStyleBackColor = true;
            this.chkUseBackgroundImage.CheckedChanged += new System.EventHandler(this.chkUseBackgroundImage_CheckedChanged);
            // 
            // BackgroundForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 307);
            this.ControlBox = false;
            this.Controls.Add(this.chkUseBackgroundImage);
            this.Controls.Add(this.pnlImage);
            this.Controls.Add(this.pnlColor);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BackgroundForm";
            this.Text = "Background Colors";
            this.VisibleChanged += new System.EventHandler(this.GradientColorsForm_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.pnlColor.ResumeLayout(false);
            this.pnlColor.PerformLayout();
            this.pnlImage.ResumeLayout(false);
            this.pnlImage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem chooseColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addColorToChoicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTransparencyToolStripMenuItem;
        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Button btnSwapColors;
        private System.Windows.Forms.CheckBox chkSolidColor;
        private System.Windows.Forms.Button btnCenterColor;
        private System.Windows.Forms.Button btnBoundaryColor;
        private System.Windows.Forms.Label lblCenterColor;
        private System.Windows.Forms.Label lblBoundaryColor;
        private System.Windows.Forms.Panel pnlImage;
        private System.Windows.Forms.Button btnBrowseBackgroundImageFile;
        private System.Windows.Forms.TextBox txtBackgroundImageFileName;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox chkUseBackgroundImage;
        private System.Windows.Forms.ComboBox cboImageMode;
        private System.Windows.Forms.Label label1;
    }
}