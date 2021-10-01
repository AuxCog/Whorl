namespace Whorl
{
    partial class frmBlendImages
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
            this.picImage = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBlendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBlendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBlendAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBlendDepthPct = new System.Windows.Forms.TextBox();
            this.txtSlopePct = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtXOffset = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.picBlendFunction = new System.Windows.Forms.PictureBox();
            this.btnApplyBlend = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtImage1FileName = new System.Windows.Forms.TextBox();
            this.txtImage2FileName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnBrowseImage1 = new System.Windows.Forms.Button();
            this.btnBrowseImage2 = new System.Windows.Forms.Button();
            this.txtBlendOffsetPct = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cboDisplayMode = new System.Windows.Forms.ComboBox();
            this.txtBlendAngle = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.RenderGroupBox = new System.Windows.Forms.GroupBox();
            this.txtRenderWidth = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtRenderHeight = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btnRender = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBlendFunction)).BeginInit();
            this.RenderGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // picImage
            // 
            this.picImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picImage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.picImage.Location = new System.Drawing.Point(0, 179);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(1114, 736);
            this.picImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picImage.TabIndex = 0;
            this.picImage.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1114, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBlendToolStripMenuItem,
            this.saveBlendToolStripMenuItem,
            this.saveBlendAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openBlendToolStripMenuItem
            // 
            this.openBlendToolStripMenuItem.Name = "openBlendToolStripMenuItem";
            this.openBlendToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.openBlendToolStripMenuItem.Text = "Open Blend";
            this.openBlendToolStripMenuItem.Click += new System.EventHandler(this.openBlendToolStripMenuItem_Click);
            // 
            // saveBlendToolStripMenuItem
            // 
            this.saveBlendToolStripMenuItem.Name = "saveBlendToolStripMenuItem";
            this.saveBlendToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.saveBlendToolStripMenuItem.Text = "Save Blend";
            this.saveBlendToolStripMenuItem.Click += new System.EventHandler(this.saveBlendToolStripMenuItem_Click);
            // 
            // saveBlendAsToolStripMenuItem
            // 
            this.saveBlendAsToolStripMenuItem.Name = "saveBlendAsToolStripMenuItem";
            this.saveBlendAsToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.saveBlendAsToolStripMenuItem.Text = "Save Blend As";
            this.saveBlendAsToolStripMenuItem.Click += new System.EventHandler(this.saveBlendAsToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Blend Depth %:";
            // 
            // txtBlendDepthPct
            // 
            this.txtBlendDepthPct.Location = new System.Drawing.Point(99, 25);
            this.txtBlendDepthPct.Name = "txtBlendDepthPct";
            this.txtBlendDepthPct.Size = new System.Drawing.Size(68, 20);
            this.txtBlendDepthPct.TabIndex = 3;
            this.txtBlendDepthPct.Text = "100";
            this.txtBlendDepthPct.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSlopePct
            // 
            this.txtSlopePct.Location = new System.Drawing.Point(99, 51);
            this.txtSlopePct.Name = "txtSlopePct";
            this.txtSlopePct.Size = new System.Drawing.Size(68, 20);
            this.txtSlopePct.TabIndex = 5;
            this.txtSlopePct.Text = "100";
            this.txtSlopePct.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Slope %:";
            // 
            // txtXOffset
            // 
            this.txtXOffset.Location = new System.Drawing.Point(99, 77);
            this.txtXOffset.Name = "txtXOffset";
            this.txtXOffset.Size = new System.Drawing.Size(68, 20);
            this.txtXOffset.TabIndex = 7;
            this.txtXOffset.Text = "0";
            this.txtXOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "X Offset %:";
            // 
            // picBlendFunction
            // 
            this.picBlendFunction.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.picBlendFunction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picBlendFunction.Location = new System.Drawing.Point(240, 25);
            this.picBlendFunction.Name = "picBlendFunction";
            this.picBlendFunction.Size = new System.Drawing.Size(298, 100);
            this.picBlendFunction.TabIndex = 8;
            this.picBlendFunction.TabStop = false;
            // 
            // btnApplyBlend
            // 
            this.btnApplyBlend.Location = new System.Drawing.Point(175, 23);
            this.btnApplyBlend.Name = "btnApplyBlend";
            this.btnApplyBlend.Size = new System.Drawing.Size(59, 23);
            this.btnApplyBlend.TabIndex = 9;
            this.btnApplyBlend.Text = "Apply";
            this.btnApplyBlend.UseVisualStyleBackColor = true;
            this.btnApplyBlend.Click += new System.EventHandler(this.btnApplyBlend_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(186, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Image 1:";
            // 
            // txtImage1FileName
            // 
            this.txtImage1FileName.Location = new System.Drawing.Point(240, 127);
            this.txtImage1FileName.Name = "txtImage1FileName";
            this.txtImage1FileName.ReadOnly = true;
            this.txtImage1FileName.Size = new System.Drawing.Size(538, 20);
            this.txtImage1FileName.TabIndex = 11;
            // 
            // txtImage2FileName
            // 
            this.txtImage2FileName.Location = new System.Drawing.Point(240, 153);
            this.txtImage2FileName.Name = "txtImage2FileName";
            this.txtImage2FileName.ReadOnly = true;
            this.txtImage2FileName.Size = new System.Drawing.Size(538, 20);
            this.txtImage2FileName.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(186, 156);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Image 2:";
            // 
            // btnBrowseImage1
            // 
            this.btnBrowseImage1.Location = new System.Drawing.Point(784, 125);
            this.btnBrowseImage1.Name = "btnBrowseImage1";
            this.btnBrowseImage1.Size = new System.Drawing.Size(59, 23);
            this.btnBrowseImage1.TabIndex = 14;
            this.btnBrowseImage1.Text = "Browse...";
            this.btnBrowseImage1.UseVisualStyleBackColor = true;
            this.btnBrowseImage1.Click += new System.EventHandler(this.BtnBrowseImage1_Click);
            // 
            // btnBrowseImage2
            // 
            this.btnBrowseImage2.Location = new System.Drawing.Point(784, 150);
            this.btnBrowseImage2.Name = "btnBrowseImage2";
            this.btnBrowseImage2.Size = new System.Drawing.Size(59, 23);
            this.btnBrowseImage2.TabIndex = 15;
            this.btnBrowseImage2.Text = "Browse...";
            this.btnBrowseImage2.UseVisualStyleBackColor = true;
            this.btnBrowseImage2.Click += new System.EventHandler(this.btnBrowseImage2_Click);
            // 
            // txtBlendOffsetPct
            // 
            this.txtBlendOffsetPct.Location = new System.Drawing.Point(99, 105);
            this.txtBlendOffsetPct.Name = "txtBlendOffsetPct";
            this.txtBlendOffsetPct.Size = new System.Drawing.Size(68, 20);
            this.txtBlendOffsetPct.TabIndex = 17;
            this.txtBlendOffsetPct.Text = "0";
            this.txtBlendOffsetPct.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 108);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Blend Offset %:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(553, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Display:";
            // 
            // cboDisplayMode
            // 
            this.cboDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayMode.FormattingEnabled = true;
            this.cboDisplayMode.Location = new System.Drawing.Point(604, 24);
            this.cboDisplayMode.Name = "cboDisplayMode";
            this.cboDisplayMode.Size = new System.Drawing.Size(91, 21);
            this.cboDisplayMode.TabIndex = 19;
            this.cboDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cboDisplayMode_SelectedIndexChanged);
            // 
            // txtBlendAngle
            // 
            this.txtBlendAngle.Location = new System.Drawing.Point(99, 131);
            this.txtBlendAngle.Name = "txtBlendAngle";
            this.txtBlendAngle.Size = new System.Drawing.Size(68, 20);
            this.txtBlendAngle.TabIndex = 21;
            this.txtBlendAngle.Text = "0";
            this.txtBlendAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(25, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Blend Angle:";
            // 
            // RenderGroupBox
            // 
            this.RenderGroupBox.Controls.Add(this.btnRender);
            this.RenderGroupBox.Controls.Add(this.txtRenderHeight);
            this.RenderGroupBox.Controls.Add(this.label10);
            this.RenderGroupBox.Controls.Add(this.txtRenderWidth);
            this.RenderGroupBox.Controls.Add(this.label9);
            this.RenderGroupBox.Enabled = false;
            this.RenderGroupBox.Location = new System.Drawing.Point(549, 53);
            this.RenderGroupBox.Name = "RenderGroupBox";
            this.RenderGroupBox.Size = new System.Drawing.Size(229, 72);
            this.RenderGroupBox.TabIndex = 22;
            this.RenderGroupBox.TabStop = false;
            this.RenderGroupBox.Text = "Rendering";
            // 
            // txtRenderWidth
            // 
            this.txtRenderWidth.Location = new System.Drawing.Point(57, 19);
            this.txtRenderWidth.Name = "txtRenderWidth";
            this.txtRenderWidth.Size = new System.Drawing.Size(66, 20);
            this.txtRenderWidth.TabIndex = 7;
            this.txtRenderWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtRenderWidth.TextChanged += new System.EventHandler(this.txtRenderWidth_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Width:";
            // 
            // txtRenderHeight
            // 
            this.txtRenderHeight.Location = new System.Drawing.Point(57, 45);
            this.txtRenderHeight.Name = "txtRenderHeight";
            this.txtRenderHeight.Size = new System.Drawing.Size(66, 20);
            this.txtRenderHeight.TabIndex = 9;
            this.txtRenderHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtRenderHeight.TextChanged += new System.EventHandler(this.txtRenderHeight_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 48);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Height:";
            // 
            // btnRender
            // 
            this.btnRender.Location = new System.Drawing.Point(139, 17);
            this.btnRender.Name = "btnRender";
            this.btnRender.Size = new System.Drawing.Size(65, 23);
            this.btnRender.TabIndex = 10;
            this.btnRender.Text = "Render...";
            this.btnRender.UseVisualStyleBackColor = true;
            this.btnRender.Click += new System.EventHandler(this.btnRender_Click);
            // 
            // frmBlendImages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1114, 915);
            this.Controls.Add(this.RenderGroupBox);
            this.Controls.Add(this.txtBlendAngle);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cboDisplayMode);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtBlendOffsetPct);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnBrowseImage2);
            this.Controls.Add(this.btnBrowseImage1);
            this.Controls.Add(this.txtImage2FileName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtImage1FileName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnApplyBlend);
            this.Controls.Add(this.picBlendFunction);
            this.Controls.Add(this.txtXOffset);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSlopePct);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBlendDepthPct);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picImage);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmBlendImages";
            this.Text = "frmBlendImages";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBlendImages_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBlendFunction)).EndInit();
            this.RenderGroupBox.ResumeLayout(false);
            this.RenderGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBlendDepthPct;
        private System.Windows.Forms.TextBox txtSlopePct;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtXOffset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox picBlendFunction;
        private System.Windows.Forms.Button btnApplyBlend;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtImage1FileName;
        private System.Windows.Forms.TextBox txtImage2FileName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnBrowseImage1;
        private System.Windows.Forms.Button btnBrowseImage2;
        private System.Windows.Forms.TextBox txtBlendOffsetPct;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem openBlendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBlendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBlendAsToolStripMenuItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cboDisplayMode;
        private System.Windows.Forms.TextBox txtBlendAngle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox RenderGroupBox;
        private System.Windows.Forms.TextBox txtRenderHeight;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtRenderWidth;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnRender;
    }
}