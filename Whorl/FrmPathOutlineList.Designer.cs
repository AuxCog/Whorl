namespace Whorl
{
    partial class FrmPathOutlineList
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
            this.picOutline = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.pnlOutlines = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cboDisplayMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtZoomPercent = new System.Windows.Forms.TextBox();
            this.cboMode = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setNextPointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectCurveSectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteCurveSectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeCurvePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlayResultOutlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.computeResultOutlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNextSectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPreviousSectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustEndPointsToIntersectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.picOutline)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picOutline
            // 
            this.picOutline.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.picOutline.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picOutline.Location = new System.Drawing.Point(321, 99);
            this.picOutline.Name = "picOutline";
            this.picOutline.Size = new System.Drawing.Size(300, 300);
            this.picOutline.TabIndex = 0;
            this.picOutline.TabStop = false;
            this.picOutline.Paint += new System.Windows.Forms.PaintEventHandler(this.picOutline_Paint);
            this.picOutline.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picOutline_MouseDown);
            this.picOutline.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picOutline_MouseMove);
            this.picOutline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picOutline_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path Patterns:";
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(475, 35);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(62, 23);
            this.BtnOK.TabIndex = 3;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(543, 35);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 4;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pnlOutlines
            // 
            this.pnlOutlines.AutoScroll = true;
            this.pnlOutlines.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlOutlines.Location = new System.Drawing.Point(15, 64);
            this.pnlOutlines.Name = "pnlOutlines";
            this.pnlOutlines.Size = new System.Drawing.Size(297, 335);
            this.pnlOutlines.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(318, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Display:";
            // 
            // cboDisplayMode
            // 
            this.cboDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayMode.FormattingEnabled = true;
            this.cboDisplayMode.Location = new System.Drawing.Point(368, 37);
            this.cboDisplayMode.Name = "cboDisplayMode";
            this.cboDisplayMode.Size = new System.Drawing.Size(76, 21);
            this.cboDisplayMode.TabIndex = 10;
            this.cboDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cboDisplayMode_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(314, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Zoom %:";
            // 
            // txtZoomPercent
            // 
            this.txtZoomPercent.AcceptsReturn = true;
            this.txtZoomPercent.Location = new System.Drawing.Point(368, 72);
            this.txtZoomPercent.Name = "txtZoomPercent";
            this.txtZoomPercent.Size = new System.Drawing.Size(49, 20);
            this.txtZoomPercent.TabIndex = 12;
            this.txtZoomPercent.Text = "100";
            this.txtZoomPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtZoomPercent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtZoomPercent_KeyPress);
            this.txtZoomPercent.Leave += new System.EventHandler(this.txtZoomPercent_Leave);
            // 
            // cboMode
            // 
            this.cboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMode.FormattingEnabled = true;
            this.cboMode.Location = new System.Drawing.Point(475, 72);
            this.cboMode.Name = "cboMode";
            this.cboMode.Size = new System.Drawing.Size(143, 21);
            this.cboMode.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(432, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Mode:";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setNextPointToolStripMenuItem,
            this.selectCurveSectionToolStripMenuItem,
            this.deleteCurveSectionToolStripMenuItem,
            this.closeCurvePathToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(208, 92);
            // 
            // setNextPointToolStripMenuItem
            // 
            this.setNextPointToolStripMenuItem.Name = "setNextPointToolStripMenuItem";
            this.setNextPointToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.setNextPointToolStripMenuItem.Text = "Set Next Point";
            this.setNextPointToolStripMenuItem.Click += new System.EventHandler(this.setNextPointToolStripMenuItem_Click);
            // 
            // selectCurveSectionToolStripMenuItem
            // 
            this.selectCurveSectionToolStripMenuItem.Name = "selectCurveSectionToolStripMenuItem";
            this.selectCurveSectionToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.selectCurveSectionToolStripMenuItem.Text = "Display Curve Section";
            this.selectCurveSectionToolStripMenuItem.Click += new System.EventHandler(this.selectCurveSectionToolStripMenuItem_Click);
            // 
            // deleteCurveSectionToolStripMenuItem
            // 
            this.deleteCurveSectionToolStripMenuItem.Name = "deleteCurveSectionToolStripMenuItem";
            this.deleteCurveSectionToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.deleteCurveSectionToolStripMenuItem.Text = "Delete Last Curve Section";
            this.deleteCurveSectionToolStripMenuItem.Click += new System.EventHandler(this.deleteCurveSectionToolStripMenuItem_Click);
            // 
            // closeCurvePathToolStripMenuItem
            // 
            this.closeCurvePathToolStripMenuItem.Name = "closeCurvePathToolStripMenuItem";
            this.closeCurvePathToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.closeCurvePathToolStripMenuItem.Text = "Close Curve Path";
            this.closeCurvePathToolStripMenuItem.Click += new System.EventHandler(this.closeCurvePathToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(630, 24);
            this.menuStrip1.TabIndex = 15;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetPanToolStripMenuItem,
            this.overlayResultOutlineToolStripMenuItem,
            this.computeResultOutlineToolStripMenuItem,
            this.selectNextSectionToolStripMenuItem,
            this.selectPreviousSectionToolStripMenuItem,
            this.adjustEndPointsToIntersectionsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // resetPanToolStripMenuItem
            // 
            this.resetPanToolStripMenuItem.Name = "resetPanToolStripMenuItem";
            this.resetPanToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.resetPanToolStripMenuItem.Text = "Reset Pan";
            this.resetPanToolStripMenuItem.Click += new System.EventHandler(this.resetPanToolStripMenuItem_Click);
            // 
            // overlayResultOutlineToolStripMenuItem
            // 
            this.overlayResultOutlineToolStripMenuItem.CheckOnClick = true;
            this.overlayResultOutlineToolStripMenuItem.Name = "overlayResultOutlineToolStripMenuItem";
            this.overlayResultOutlineToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.overlayResultOutlineToolStripMenuItem.Text = "Overlay Result Outline";
            this.overlayResultOutlineToolStripMenuItem.Click += new System.EventHandler(this.overlayResultOutlineToolStripMenuItem_Click);
            // 
            // computeResultOutlineToolStripMenuItem
            // 
            this.computeResultOutlineToolStripMenuItem.Name = "computeResultOutlineToolStripMenuItem";
            this.computeResultOutlineToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.computeResultOutlineToolStripMenuItem.Text = "Compute Result Outline";
            this.computeResultOutlineToolStripMenuItem.Click += new System.EventHandler(this.computeResultOutlineToolStripMenuItem_Click);
            // 
            // selectNextSectionToolStripMenuItem
            // 
            this.selectNextSectionToolStripMenuItem.Name = "selectNextSectionToolStripMenuItem";
            this.selectNextSectionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.selectNextSectionToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.selectNextSectionToolStripMenuItem.Text = "Display Next Section";
            this.selectNextSectionToolStripMenuItem.Click += new System.EventHandler(this.selectNextSectionToolStripMenuItem_Click);
            // 
            // selectPreviousSectionToolStripMenuItem
            // 
            this.selectPreviousSectionToolStripMenuItem.Name = "selectPreviousSectionToolStripMenuItem";
            this.selectPreviousSectionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.selectPreviousSectionToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.selectPreviousSectionToolStripMenuItem.Text = "Display Previous Section";
            this.selectPreviousSectionToolStripMenuItem.Click += new System.EventHandler(this.selectPreviousSectionToolStripMenuItem_Click);
            // 
            // adjustEndPointsToIntersectionsToolStripMenuItem
            // 
            this.adjustEndPointsToIntersectionsToolStripMenuItem.CheckOnClick = true;
            this.adjustEndPointsToIntersectionsToolStripMenuItem.Name = "adjustEndPointsToIntersectionsToolStripMenuItem";
            this.adjustEndPointsToIntersectionsToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.adjustEndPointsToIntersectionsToolStripMenuItem.Text = "Adjust End Points to Intersections";
            // 
            // FrmPathOutlineList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 407);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.cboMode);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtZoomPercent);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboDisplayMode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pnlOutlines);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picOutline);
            this.Name = "FrmPathOutlineList";
            this.Text = "Path Outlines Editor";
            this.Load += new System.EventHandler(this.FrmPathOutlineList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picOutline)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picOutline;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Panel pnlOutlines;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboDisplayMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtZoomPercent;
        private System.Windows.Forms.ComboBox cboMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem setNextPointToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetPanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overlayResultOutlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem computeResultOutlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectCurveSectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteCurveSectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNextSectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectPreviousSectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeCurvePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adjustEndPointsToIntersectionsToolStripMenuItem;
    }
}