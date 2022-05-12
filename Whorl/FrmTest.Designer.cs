namespace Whorl
{
    partial class FrmTest
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
            this.picPattern = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMinAngle = new System.Windows.Forms.TextBox();
            this.txtPadding = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkClockwise = new System.Windows.Forms.CheckBox();
            this.BtnSelectPattern = new System.Windows.Forms.Button();
            this.BtnDrawPadding = new System.Windows.Forms.Button();
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDistanceSquaresPointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkTransformPath = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPattern)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picPattern
            // 
            this.picPattern.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.picPattern.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.picPattern.Location = new System.Drawing.Point(0, 102);
            this.picPattern.Name = "picPattern";
            this.picPattern.Size = new System.Drawing.Size(689, 591);
            this.picPattern.TabIndex = 0;
            this.picPattern.TabStop = false;
            this.picPattern.Paint += new System.Windows.Forms.PaintEventHandler(this.picPattern_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Min Angle:";
            // 
            // txtMinAngle
            // 
            this.txtMinAngle.Location = new System.Drawing.Point(75, 31);
            this.txtMinAngle.Name = "txtMinAngle";
            this.txtMinAngle.Size = new System.Drawing.Size(47, 20);
            this.txtMinAngle.TabIndex = 2;
            this.txtMinAngle.Text = "3";
            this.txtMinAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtPadding
            // 
            this.txtPadding.Location = new System.Drawing.Point(193, 31);
            this.txtPadding.Name = "txtPadding";
            this.txtPadding.Size = new System.Drawing.Size(47, 20);
            this.txtPadding.TabIndex = 4;
            this.txtPadding.Text = "5";
            this.txtPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(138, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Padding:";
            // 
            // chkClockwise
            // 
            this.chkClockwise.AutoSize = true;
            this.chkClockwise.Checked = true;
            this.chkClockwise.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClockwise.Location = new System.Drawing.Point(259, 34);
            this.chkClockwise.Name = "chkClockwise";
            this.chkClockwise.Size = new System.Drawing.Size(74, 17);
            this.chkClockwise.TabIndex = 5;
            this.chkClockwise.Text = "Clockwise";
            this.chkClockwise.UseVisualStyleBackColor = true;
            // 
            // BtnSelectPattern
            // 
            this.BtnSelectPattern.Location = new System.Drawing.Point(585, 30);
            this.BtnSelectPattern.Name = "BtnSelectPattern";
            this.BtnSelectPattern.Size = new System.Drawing.Size(95, 23);
            this.BtnSelectPattern.TabIndex = 6;
            this.BtnSelectPattern.Text = "Select Pattern";
            this.BtnSelectPattern.UseVisualStyleBackColor = true;
            this.BtnSelectPattern.Click += new System.EventHandler(this.BtnSelectPattern_Click);
            // 
            // BtnDrawPadding
            // 
            this.BtnDrawPadding.Location = new System.Drawing.Point(446, 29);
            this.BtnDrawPadding.Name = "BtnDrawPadding";
            this.BtnDrawPadding.Size = new System.Drawing.Size(62, 23);
            this.BtnDrawPadding.TabIndex = 7;
            this.BtnDrawPadding.Text = "Draw";
            this.BtnDrawPadding.UseVisualStyleBackColor = true;
            this.BtnDrawPadding.Click += new System.EventHandler(this.BtnDrawPadding_Click);
            // 
            // txtMessages
            // 
            this.txtMessages.Location = new System.Drawing.Point(15, 60);
            this.txtMessages.Multiline = true;
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ReadOnly = true;
            this.txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessages.Size = new System.Drawing.Size(661, 34);
            this.txtMessages.TabIndex = 8;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(689, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewDistanceSquaresPointsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // viewDistanceSquaresPointsToolStripMenuItem
            // 
            this.viewDistanceSquaresPointsToolStripMenuItem.CheckOnClick = true;
            this.viewDistanceSquaresPointsToolStripMenuItem.Name = "viewDistanceSquaresPointsToolStripMenuItem";
            this.viewDistanceSquaresPointsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.viewDistanceSquaresPointsToolStripMenuItem.Text = "Distance Squares Points";
            this.viewDistanceSquaresPointsToolStripMenuItem.Click += new System.EventHandler(this.viewDistanceSquaresPointsToolStripMenuItem_Click);
            // 
            // chkTransformPath
            // 
            this.chkTransformPath.AutoSize = true;
            this.chkTransformPath.Checked = true;
            this.chkTransformPath.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTransformPath.Location = new System.Drawing.Point(339, 34);
            this.chkTransformPath.Name = "chkTransformPath";
            this.chkTransformPath.Size = new System.Drawing.Size(73, 17);
            this.chkTransformPath.TabIndex = 10;
            this.chkTransformPath.Text = "Transform";
            this.chkTransformPath.UseVisualStyleBackColor = true;
            // 
            // FrmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 693);
            this.Controls.Add(this.chkTransformPath);
            this.Controls.Add(this.txtMessages);
            this.Controls.Add(this.BtnDrawPadding);
            this.Controls.Add(this.BtnSelectPattern);
            this.Controls.Add(this.chkClockwise);
            this.Controls.Add(this.txtPadding);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMinAngle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picPattern);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FrmTest";
            this.Text = "FrmTest";
            ((System.ComponentModel.ISupportInitialize)(this.picPattern)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picPattern;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMinAngle;
        private System.Windows.Forms.TextBox txtPadding;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkClockwise;
        private System.Windows.Forms.Button BtnSelectPattern;
        private System.Windows.Forms.Button BtnDrawPadding;
        private System.Windows.Forms.TextBox txtMessages;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewDistanceSquaresPointsToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkTransformPath;
    }
}