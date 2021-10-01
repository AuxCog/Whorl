namespace Whorl
{
    partial class frmMessage
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
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.btnClearMessages = new System.Windows.Forms.Button();
            this.txtFormula = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnParse = new System.Windows.Forms.Button();
            this.btnEval = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.picImage = new System.Windows.Forms.PictureBox();
            this.pnlParameters = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtImageWidth = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFormulaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFormulaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fractalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.isFractalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useColorGradientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testOptimizationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorGradientFormToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtZoomX = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtZoomY = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOffsetY = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtOffsetX = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtZoom = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtMaxIter = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtColorFactor = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtColorCount = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btnRenderFractal = new System.Windows.Forms.Button();
            this.txtDraftSize = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnRenderCompiled = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.cboOutlineMethod = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMessages
            // 
            this.txtMessages.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.txtMessages.Location = new System.Drawing.Point(7, 69);
            this.txtMessages.Multiline = true;
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ReadOnly = true;
            this.txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessages.Size = new System.Drawing.Size(491, 202);
            this.txtMessages.TabIndex = 0;
            // 
            // btnClearMessages
            // 
            this.btnClearMessages.Location = new System.Drawing.Point(4, 40);
            this.btnClearMessages.Name = "btnClearMessages";
            this.btnClearMessages.Size = new System.Drawing.Size(77, 23);
            this.btnClearMessages.TabIndex = 1;
            this.btnClearMessages.Text = "Clear All";
            this.btnClearMessages.UseVisualStyleBackColor = true;
            this.btnClearMessages.Click += new System.EventHandler(this.btnClearMessages_Click);
            // 
            // txtFormula
            // 
            this.txtFormula.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.txtFormula.Location = new System.Drawing.Point(7, 312);
            this.txtFormula.Multiline = true;
            this.txtFormula.Name = "txtFormula";
            this.txtFormula.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFormula.Size = new System.Drawing.Size(401, 242);
            this.txtFormula.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 296);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Test Formula:";
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(87, 39);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(58, 23);
            this.btnParse.TabIndex = 4;
            this.btnParse.Text = "Parse";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // btnEval
            // 
            this.btnEval.Location = new System.Drawing.Point(151, 39);
            this.btnEval.Name = "btnEval";
            this.btnEval.Size = new System.Drawing.Size(58, 23);
            this.btnEval.TabIndex = 5;
            this.btnEval.Text = "Run";
            this.btnEval.UseVisualStyleBackColor = true;
            this.btnEval.Click += new System.EventHandler(this.btnEval_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(7, 274);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(74, 13);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Elapsed Time:";
            // 
            // picImage
            // 
            this.picImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picImage.Location = new System.Drawing.Point(422, 296);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(299, 258);
            this.picImage.TabIndex = 7;
            this.picImage.TabStop = false;
            // 
            // pnlParameters
            // 
            this.pnlParameters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlParameters.Location = new System.Drawing.Point(7, 591);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(714, 196);
            this.pnlParameters.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 568);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Size:";
            // 
            // txtImageWidth
            // 
            this.txtImageWidth.Location = new System.Drawing.Point(43, 565);
            this.txtImageWidth.Name = "txtImageWidth";
            this.txtImageWidth.Size = new System.Drawing.Size(47, 20);
            this.txtImageWidth.TabIndex = 11;
            this.txtImageWidth.Text = "200";
            this.txtImageWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.fractalToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(727, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveFormulaToolStripMenuItem,
            this.openFormulaToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveFormulaToolStripMenuItem
            // 
            this.saveFormulaToolStripMenuItem.Name = "saveFormulaToolStripMenuItem";
            this.saveFormulaToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.saveFormulaToolStripMenuItem.Text = "Save Formula";
            this.saveFormulaToolStripMenuItem.Click += new System.EventHandler(this.saveFormulaToolStripMenuItem_Click);
            // 
            // openFormulaToolStripMenuItem
            // 
            this.openFormulaToolStripMenuItem.Name = "openFormulaToolStripMenuItem";
            this.openFormulaToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.openFormulaToolStripMenuItem.Text = "Open Formula";
            this.openFormulaToolStripMenuItem.Click += new System.EventHandler(this.openFormulaToolStripMenuItem_Click);
            // 
            // fractalToolStripMenuItem
            // 
            this.fractalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.isFractalToolStripMenuItem,
            this.useColorGradientToolStripMenuItem,
            this.testOptimizationToolStripMenuItem});
            this.fractalToolStripMenuItem.Name = "fractalToolStripMenuItem";
            this.fractalToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.fractalToolStripMenuItem.Text = "Fractal";
            // 
            // isFractalToolStripMenuItem
            // 
            this.isFractalToolStripMenuItem.CheckOnClick = true;
            this.isFractalToolStripMenuItem.Name = "isFractalToolStripMenuItem";
            this.isFractalToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.isFractalToolStripMenuItem.Text = "Is Fractal";
            // 
            // useColorGradientToolStripMenuItem
            // 
            this.useColorGradientToolStripMenuItem.CheckOnClick = true;
            this.useColorGradientToolStripMenuItem.Name = "useColorGradientToolStripMenuItem";
            this.useColorGradientToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.useColorGradientToolStripMenuItem.Text = "Use Color Gradient";
            // 
            // testOptimizationToolStripMenuItem
            // 
            this.testOptimizationToolStripMenuItem.CheckOnClick = true;
            this.testOptimizationToolStripMenuItem.Name = "testOptimizationToolStripMenuItem";
            this.testOptimizationToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.testOptimizationToolStripMenuItem.Text = "Test Optimization";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorGradientFormToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // colorGradientFormToolStripMenuItem
            // 
            this.colorGradientFormToolStripMenuItem.Name = "colorGradientFormToolStripMenuItem";
            this.colorGradientFormToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.colorGradientFormToolStripMenuItem.Text = "Color Gradient Form";
            this.colorGradientFormToolStripMenuItem.Click += new System.EventHandler(this.colorGradientFormToolStripMenuItem_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(498, 37);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(223, 23);
            this.progressBar1.TabIndex = 14;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(215, 40);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(58, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtZoomX
            // 
            this.txtZoomX.Location = new System.Drawing.Point(256, 565);
            this.txtZoomX.Name = "txtZoomX";
            this.txtZoomX.Size = new System.Drawing.Size(47, 20);
            this.txtZoomX.TabIndex = 17;
            this.txtZoomX.Text = "1";
            this.txtZoomX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(204, 568);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Zoom X:";
            // 
            // txtZoomY
            // 
            this.txtZoomY.Location = new System.Drawing.Point(337, 565);
            this.txtZoomY.Name = "txtZoomY";
            this.txtZoomY.Size = new System.Drawing.Size(47, 20);
            this.txtZoomY.TabIndex = 19;
            this.txtZoomY.Text = "1";
            this.txtZoomY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(314, 568);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Y:";
            // 
            // txtOffsetY
            // 
            this.txtOffsetY.Location = new System.Drawing.Point(532, 565);
            this.txtOffsetY.Name = "txtOffsetY";
            this.txtOffsetY.Size = new System.Drawing.Size(47, 20);
            this.txtOffsetY.TabIndex = 23;
            this.txtOffsetY.Text = "0";
            this.txtOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(509, 568);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Y:";
            // 
            // txtOffsetX
            // 
            this.txtOffsetX.Location = new System.Drawing.Point(451, 565);
            this.txtOffsetX.Name = "txtOffsetX";
            this.txtOffsetX.Size = new System.Drawing.Size(47, 20);
            this.txtOffsetX.TabIndex = 21;
            this.txtOffsetX.Text = "0";
            this.txtOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(399, 568);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Offset X:";
            // 
            // txtZoom
            // 
            this.txtZoom.Location = new System.Drawing.Point(141, 565);
            this.txtZoom.Name = "txtZoom";
            this.txtZoom.Size = new System.Drawing.Size(47, 20);
            this.txtZoom.TabIndex = 25;
            this.txtZoom.Text = "1";
            this.txtZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(98, 568);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Zoom:";
            // 
            // txtMaxIter
            // 
            this.txtMaxIter.Location = new System.Drawing.Point(594, 69);
            this.txtMaxIter.Name = "txtMaxIter";
            this.txtMaxIter.Size = new System.Drawing.Size(47, 20);
            this.txtMaxIter.TabIndex = 27;
            this.txtMaxIter.Text = "255";
            this.txtMaxIter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(521, 72);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "Max Iter:";
            // 
            // txtColorFactor
            // 
            this.txtColorFactor.Location = new System.Drawing.Point(594, 95);
            this.txtColorFactor.Name = "txtColorFactor";
            this.txtColorFactor.Size = new System.Drawing.Size(47, 20);
            this.txtColorFactor.TabIndex = 29;
            this.txtColorFactor.Tag = "Render";
            this.txtColorFactor.Text = "1";
            this.txtColorFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(521, 98);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(67, 13);
            this.label9.TabIndex = 28;
            this.label9.Text = "Color Factor:";
            // 
            // txtColorCount
            // 
            this.txtColorCount.Location = new System.Drawing.Point(594, 121);
            this.txtColorCount.Name = "txtColorCount";
            this.txtColorCount.Size = new System.Drawing.Size(47, 20);
            this.txtColorCount.TabIndex = 31;
            this.txtColorCount.Tag = "Render";
            this.txtColorCount.Text = "255";
            this.txtColorCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(521, 124);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Color Count:";
            // 
            // btnRenderFractal
            // 
            this.btnRenderFractal.Location = new System.Drawing.Point(279, 40);
            this.btnRenderFractal.Name = "btnRenderFractal";
            this.btnRenderFractal.Size = new System.Drawing.Size(58, 23);
            this.btnRenderFractal.TabIndex = 32;
            this.btnRenderFractal.Text = "Render";
            this.btnRenderFractal.UseVisualStyleBackColor = true;
            this.btnRenderFractal.Click += new System.EventHandler(this.btnRenderFractal_Click);
            // 
            // txtDraftSize
            // 
            this.txtDraftSize.Location = new System.Drawing.Point(594, 147);
            this.txtDraftSize.Name = "txtDraftSize";
            this.txtDraftSize.Size = new System.Drawing.Size(47, 20);
            this.txtDraftSize.TabIndex = 34;
            this.txtDraftSize.Text = "1";
            this.txtDraftSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(521, 150);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(53, 13);
            this.label11.TabIndex = 33;
            this.label11.Text = "Draft Size";
            // 
            // btnCompile
            // 
            this.btnCompile.Enabled = false;
            this.btnCompile.Location = new System.Drawing.Point(101, 283);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(58, 23);
            this.btnCompile.TabIndex = 35;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            // 
            // btnRenderCompiled
            // 
            this.btnRenderCompiled.Enabled = false;
            this.btnRenderCompiled.Location = new System.Drawing.Point(174, 283);
            this.btnRenderCompiled.Name = "btnRenderCompiled";
            this.btnRenderCompiled.Size = new System.Drawing.Size(58, 23);
            this.btnRenderCompiled.TabIndex = 36;
            this.btnRenderCompiled.Text = "Render";
            this.btnRenderCompiled.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(504, 179);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(40, 13);
            this.label12.TabIndex = 37;
            this.label12.Text = "Outline";
            // 
            // cboOutlineMethod
            // 
            this.cboOutlineMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOutlineMethod.FormattingEnabled = true;
            this.cboOutlineMethod.Location = new System.Drawing.Point(550, 176);
            this.cboOutlineMethod.Name = "cboOutlineMethod";
            this.cboOutlineMethod.Size = new System.Drawing.Size(153, 21);
            this.cboOutlineMethod.TabIndex = 38;
            // 
            // frmMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 792);
            this.Controls.Add(this.cboOutlineMethod);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.btnRenderCompiled);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.txtDraftSize);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btnRenderFractal);
            this.Controls.Add(this.txtColorCount);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtColorFactor);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtMaxIter);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtZoom);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtOffsetY);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtOffsetX);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtZoomY);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtZoomX);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.txtImageWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pnlParameters);
            this.Controls.Add(this.picImage);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnEval);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFormula);
            this.Controls.Add(this.btnClearMessages);
            this.Controls.Add(this.txtMessages);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMessage";
            this.Text = "Messages";
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMessages;
        private System.Windows.Forms.Button btnClearMessages;
        private System.Windows.Forms.TextBox txtFormula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.Button btnEval;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.Panel pnlParameters;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtImageWidth;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFormulaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFormulaToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ToolStripMenuItem fractalToolStripMenuItem;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolStripMenuItem isFractalToolStripMenuItem;
        private System.Windows.Forms.TextBox txtZoomX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtZoomY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtOffsetY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtOffsetX;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtZoom;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorGradientFormToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useColorGradientToolStripMenuItem;
        private System.Windows.Forms.TextBox txtMaxIter;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtColorFactor;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtColorCount;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnRenderFractal;
        private System.Windows.Forms.TextBox txtDraftSize;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolStripMenuItem testOptimizationToolStripMenuItem;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnRenderCompiled;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cboOutlineMethod;
    }
}