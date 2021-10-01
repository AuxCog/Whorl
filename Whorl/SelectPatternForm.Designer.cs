namespace Whorl
{
    partial class SelectPatternForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPatternToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPatternFromDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlPatternDashboard = new System.Windows.Forms.Panel();
            this.btnFilterPatterns = new System.Windows.Forms.Button();
            this.cboPatternName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.pnlPatterns = new System.Windows.Forms.Panel();
            this.cboPageRange = new System.Windows.Forms.ComboBox();
            this.lblPageRange = new System.Windows.Forms.Label();
            this.pnlPages = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlPatternFilters = new System.Windows.Forms.Panel();
            this.cboHasRandom = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboUsesSection = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlRibbonFilters = new System.Windows.Forms.Panel();
            this.cboRibbonUsesFormula = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkRibbonType = new System.Windows.Forms.CheckBox();
            this.cboRibbonType = new System.Windows.Forms.ComboBox();
            this.cboTransformName = new System.Windows.Forms.ComboBox();
            this.chkPatternType = new System.Windows.Forms.CheckBox();
            this.cboPatternType = new System.Windows.Forms.ComboBox();
            this.chkTransform = new System.Windows.Forms.CheckBox();
            this.chkBasicOutlineType = new System.Windows.Forms.CheckBox();
            this.cboBasicOutlineType = new System.Windows.Forms.ComboBox();
            this.chkFilterPatterns = new System.Windows.Forms.CheckBox();
            this.btnCloseFilters = new System.Windows.Forms.Button();
            this.btnApplyFilters = new System.Windows.Forms.Button();
            this.chkOnlyNamedPatterns = new System.Windows.Forms.CheckBox();
            this.cboUsesRecursion = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.pnlPatternDashboard.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.pnlPatternFilters.SuspendLayout();
            this.pnlRibbonFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(526, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(607, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeItemToolStripMenuItem,
            this.editPatternToolStripMenuItem,
            this.setPatternFromDefaultToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(204, 70);
            // 
            // removeItemToolStripMenuItem
            // 
            this.removeItemToolStripMenuItem.Name = "removeItemToolStripMenuItem";
            this.removeItemToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.removeItemToolStripMenuItem.Text = "Remove Pattern";
            this.removeItemToolStripMenuItem.Click += new System.EventHandler(this.removeItemToolStripMenuItem_Click);
            // 
            // editPatternToolStripMenuItem
            // 
            this.editPatternToolStripMenuItem.Name = "editPatternToolStripMenuItem";
            this.editPatternToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.editPatternToolStripMenuItem.Text = "Edit Pattern";
            this.editPatternToolStripMenuItem.Click += new System.EventHandler(this.editPatternToolStripMenuItem_Click);
            // 
            // setPatternFromDefaultToolStripMenuItem
            // 
            this.setPatternFromDefaultToolStripMenuItem.Name = "setPatternFromDefaultToolStripMenuItem";
            this.setPatternFromDefaultToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.setPatternFromDefaultToolStripMenuItem.Text = "Set Pattern From Default";
            this.setPatternFromDefaultToolStripMenuItem.Click += new System.EventHandler(this.setPatternFromDefaultToolStripMenuItem_Click);
            // 
            // pnlPatternDashboard
            // 
            this.pnlPatternDashboard.Controls.Add(this.btnFilterPatterns);
            this.pnlPatternDashboard.Controls.Add(this.cboPatternName);
            this.pnlPatternDashboard.Controls.Add(this.label1);
            this.pnlPatternDashboard.Location = new System.Drawing.Point(17, 4);
            this.pnlPatternDashboard.Name = "pnlPatternDashboard";
            this.pnlPatternDashboard.Size = new System.Drawing.Size(503, 32);
            this.pnlPatternDashboard.TabIndex = 6;
            // 
            // btnFilterPatterns
            // 
            this.btnFilterPatterns.Location = new System.Drawing.Point(365, 5);
            this.btnFilterPatterns.Name = "btnFilterPatterns";
            this.btnFilterPatterns.Size = new System.Drawing.Size(90, 23);
            this.btnFilterPatterns.TabIndex = 14;
            this.btnFilterPatterns.Text = "Filter Patterns";
            this.btnFilterPatterns.UseVisualStyleBackColor = true;
            this.btnFilterPatterns.Click += new System.EventHandler(this.btnFilterPatterns_Click);
            // 
            // cboPatternName
            // 
            this.cboPatternName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPatternName.FormattingEnabled = true;
            this.cboPatternName.Location = new System.Drawing.Point(119, 5);
            this.cboPatternName.Name = "cboPatternName";
            this.cboPatternName.Size = new System.Drawing.Size(229, 21);
            this.cboPatternName.TabIndex = 8;
            this.cboPatternName.SelectedIndexChanged += new System.EventHandler(this.cboPatternName_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Go to Named Pattern:";
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.pnlPatterns);
            this.pnlBottom.Controls.Add(this.cboPageRange);
            this.pnlBottom.Controls.Add(this.lblPageRange);
            this.pnlBottom.Controls.Add(this.pnlPages);
            this.pnlBottom.Location = new System.Drawing.Point(1, 172);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(691, 593);
            this.pnlBottom.TabIndex = 7;
            // 
            // pnlPatterns
            // 
            this.pnlPatterns.Location = new System.Drawing.Point(10, 3);
            this.pnlPatterns.Name = "pnlPatterns";
            this.pnlPatterns.Size = new System.Drawing.Size(670, 537);
            this.pnlPatterns.TabIndex = 9;
            // 
            // cboPageRange
            // 
            this.cboPageRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPageRange.FormattingEnabled = true;
            this.cboPageRange.Location = new System.Drawing.Point(562, 562);
            this.cboPageRange.Margin = new System.Windows.Forms.Padding(2);
            this.cboPageRange.Name = "cboPageRange";
            this.cboPageRange.Size = new System.Drawing.Size(114, 21);
            this.cboPageRange.TabIndex = 8;
            // 
            // lblPageRange
            // 
            this.lblPageRange.AutoSize = true;
            this.lblPageRange.Location = new System.Drawing.Point(490, 565);
            this.lblPageRange.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPageRange.Name = "lblPageRange";
            this.lblPageRange.Size = new System.Drawing.Size(70, 13);
            this.lblPageRange.TabIndex = 7;
            this.lblPageRange.Text = "Page Range:";
            // 
            // pnlPages
            // 
            this.pnlPages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPages.Location = new System.Drawing.Point(7, 544);
            this.pnlPages.Name = "pnlPages";
            this.pnlPages.Size = new System.Drawing.Size(478, 43);
            this.pnlPages.TabIndex = 6;
            // 
            // pnlPatternFilters
            // 
            this.pnlPatternFilters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlPatternFilters.Controls.Add(this.cboUsesRecursion);
            this.pnlPatternFilters.Controls.Add(this.label6);
            this.pnlPatternFilters.Controls.Add(this.cboHasRandom);
            this.pnlPatternFilters.Controls.Add(this.label4);
            this.pnlPatternFilters.Controls.Add(this.cboUsesSection);
            this.pnlPatternFilters.Controls.Add(this.label3);
            this.pnlPatternFilters.Controls.Add(this.pnlRibbonFilters);
            this.pnlPatternFilters.Controls.Add(this.cboTransformName);
            this.pnlPatternFilters.Controls.Add(this.chkPatternType);
            this.pnlPatternFilters.Controls.Add(this.cboPatternType);
            this.pnlPatternFilters.Controls.Add(this.chkTransform);
            this.pnlPatternFilters.Controls.Add(this.chkBasicOutlineType);
            this.pnlPatternFilters.Controls.Add(this.cboBasicOutlineType);
            this.pnlPatternFilters.Controls.Add(this.chkFilterPatterns);
            this.pnlPatternFilters.Controls.Add(this.btnCloseFilters);
            this.pnlPatternFilters.Controls.Add(this.btnApplyFilters);
            this.pnlPatternFilters.Controls.Add(this.chkOnlyNamedPatterns);
            this.pnlPatternFilters.Location = new System.Drawing.Point(17, 42);
            this.pnlPatternFilters.Name = "pnlPatternFilters";
            this.pnlPatternFilters.Size = new System.Drawing.Size(664, 124);
            this.pnlPatternFilters.TabIndex = 8;
            this.pnlPatternFilters.Visible = false;
            // 
            // cboHasRandom
            // 
            this.cboHasRandom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHasRandom.FormattingEnabled = true;
            this.cboHasRandom.Location = new System.Drawing.Point(81, 61);
            this.cboHasRandom.Name = "cboHasRandom";
            this.cboHasRandom.Size = new System.Drawing.Size(54, 21);
            this.cboHasRandom.TabIndex = 31;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "Has Random:";
            // 
            // cboUsesSection
            // 
            this.cboUsesSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsesSection.FormattingEnabled = true;
            this.cboUsesSection.Location = new System.Drawing.Point(234, 61);
            this.cboUsesSection.Name = "cboUsesSection";
            this.cboUsesSection.Size = new System.Drawing.Size(54, 21);
            this.cboUsesSection.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(155, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Uses Section:";
            // 
            // pnlRibbonFilters
            // 
            this.pnlRibbonFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRibbonFilters.Controls.Add(this.cboRibbonUsesFormula);
            this.pnlRibbonFilters.Controls.Add(this.label5);
            this.pnlRibbonFilters.Controls.Add(this.label2);
            this.pnlRibbonFilters.Controls.Add(this.chkRibbonType);
            this.pnlRibbonFilters.Controls.Add(this.cboRibbonType);
            this.pnlRibbonFilters.Location = new System.Drawing.Point(6, 88);
            this.pnlRibbonFilters.Name = "pnlRibbonFilters";
            this.pnlRibbonFilters.Size = new System.Drawing.Size(652, 29);
            this.pnlRibbonFilters.TabIndex = 27;
            this.pnlRibbonFilters.Visible = false;
            // 
            // cboRibbonUsesFormula
            // 
            this.cboRibbonUsesFormula.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRibbonUsesFormula.FormattingEnabled = true;
            this.cboRibbonUsesFormula.Location = new System.Drawing.Point(315, 3);
            this.cboRibbonUsesFormula.Name = "cboRibbonUsesFormula";
            this.cboRibbonUsesFormula.Size = new System.Drawing.Size(54, 21);
            this.cboRibbonUsesFormula.TabIndex = 31;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(235, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Uses Formula:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Ribbon:";
            // 
            // chkRibbonType
            // 
            this.chkRibbonType.AutoSize = true;
            this.chkRibbonType.Location = new System.Drawing.Point(56, 5);
            this.chkRibbonType.Name = "chkRibbonType";
            this.chkRibbonType.Size = new System.Drawing.Size(53, 17);
            this.chkRibbonType.TabIndex = 28;
            this.chkRibbonType.Text = "Type:";
            this.chkRibbonType.UseVisualStyleBackColor = true;
            // 
            // cboRibbonType
            // 
            this.cboRibbonType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRibbonType.FormattingEnabled = true;
            this.cboRibbonType.Location = new System.Drawing.Point(112, 3);
            this.cboRibbonType.Name = "cboRibbonType";
            this.cboRibbonType.Size = new System.Drawing.Size(105, 21);
            this.cboRibbonType.TabIndex = 27;
            this.cboRibbonType.SelectedIndexChanged += new System.EventHandler(this.cboRibbonType_SelectedIndexChanged);
            // 
            // cboTransformName
            // 
            this.cboTransformName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTransformName.FormattingEnabled = true;
            this.cboTransformName.Location = new System.Drawing.Point(321, 6);
            this.cboTransformName.Name = "cboTransformName";
            this.cboTransformName.Size = new System.Drawing.Size(199, 21);
            this.cboTransformName.TabIndex = 24;
            this.cboTransformName.SelectedIndexChanged += new System.EventHandler(this.cboTransformName_SelectedIndexChanged);
            // 
            // chkPatternType
            // 
            this.chkPatternType.AutoSize = true;
            this.chkPatternType.Location = new System.Drawing.Point(284, 33);
            this.chkPatternType.Name = "chkPatternType";
            this.chkPatternType.Size = new System.Drawing.Size(90, 17);
            this.chkPatternType.TabIndex = 19;
            this.chkPatternType.Text = "Pattern Type:";
            this.chkPatternType.UseVisualStyleBackColor = true;
            // 
            // cboPatternType
            // 
            this.cboPatternType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPatternType.FormattingEnabled = true;
            this.cboPatternType.Location = new System.Drawing.Point(380, 31);
            this.cboPatternType.Name = "cboPatternType";
            this.cboPatternType.Size = new System.Drawing.Size(140, 21);
            this.cboPatternType.TabIndex = 12;
            this.cboPatternType.SelectedIndexChanged += new System.EventHandler(this.cboPatternType_SelectedIndexChanged);
            // 
            // chkTransform
            // 
            this.chkTransform.AutoSize = true;
            this.chkTransform.Location = new System.Drawing.Point(225, 10);
            this.chkTransform.Name = "chkTransform";
            this.chkTransform.Size = new System.Drawing.Size(98, 17);
            this.chkTransform.TabIndex = 23;
            this.chkTransform.Text = "Has Transform:";
            this.chkTransform.UseVisualStyleBackColor = true;
            // 
            // chkBasicOutlineType
            // 
            this.chkBasicOutlineType.AutoSize = true;
            this.chkBasicOutlineType.Location = new System.Drawing.Point(5, 35);
            this.chkBasicOutlineType.Name = "chkBasicOutlineType";
            this.chkBasicOutlineType.Size = new System.Drawing.Size(113, 17);
            this.chkBasicOutlineType.TabIndex = 20;
            this.chkBasicOutlineType.Text = "Has Basic Outline:";
            this.chkBasicOutlineType.UseVisualStyleBackColor = true;
            // 
            // cboBasicOutlineType
            // 
            this.cboBasicOutlineType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBasicOutlineType.FormattingEnabled = true;
            this.cboBasicOutlineType.Location = new System.Drawing.Point(118, 33);
            this.cboBasicOutlineType.Name = "cboBasicOutlineType";
            this.cboBasicOutlineType.Size = new System.Drawing.Size(135, 21);
            this.cboBasicOutlineType.TabIndex = 18;
            this.cboBasicOutlineType.SelectedIndexChanged += new System.EventHandler(this.cboBasicOutlineType_SelectedIndexChanged);
            // 
            // chkFilterPatterns
            // 
            this.chkFilterPatterns.AutoSize = true;
            this.chkFilterPatterns.Location = new System.Drawing.Point(5, 10);
            this.chkFilterPatterns.Name = "chkFilterPatterns";
            this.chkFilterPatterns.Size = new System.Drawing.Size(90, 17);
            this.chkFilterPatterns.TabIndex = 16;
            this.chkFilterPatterns.Text = "Filter Patterns";
            this.chkFilterPatterns.UseVisualStyleBackColor = true;
            this.chkFilterPatterns.CheckedChanged += new System.EventHandler(this.chkFilterPatterns_CheckedChanged);
            // 
            // btnCloseFilters
            // 
            this.btnCloseFilters.Location = new System.Drawing.Point(615, 3);
            this.btnCloseFilters.Name = "btnCloseFilters";
            this.btnCloseFilters.Size = new System.Drawing.Size(43, 23);
            this.btnCloseFilters.TabIndex = 15;
            this.btnCloseFilters.Text = "Close";
            this.btnCloseFilters.UseVisualStyleBackColor = true;
            this.btnCloseFilters.Click += new System.EventHandler(this.btnCloseFilters_Click);
            // 
            // btnApplyFilters
            // 
            this.btnApplyFilters.Location = new System.Drawing.Point(526, 3);
            this.btnApplyFilters.Name = "btnApplyFilters";
            this.btnApplyFilters.Size = new System.Drawing.Size(83, 23);
            this.btnApplyFilters.TabIndex = 13;
            this.btnApplyFilters.Text = "Apply Filters";
            this.btnApplyFilters.UseVisualStyleBackColor = true;
            this.btnApplyFilters.Click += new System.EventHandler(this.btnApplyFilters_Click);
            // 
            // chkOnlyNamedPatterns
            // 
            this.chkOnlyNamedPatterns.AutoSize = true;
            this.chkOnlyNamedPatterns.Location = new System.Drawing.Point(101, 10);
            this.chkOnlyNamedPatterns.Name = "chkOnlyNamedPatterns";
            this.chkOnlyNamedPatterns.Size = new System.Drawing.Size(126, 17);
            this.chkOnlyNamedPatterns.TabIndex = 10;
            this.chkOnlyNamedPatterns.Text = "Only Named Patterns";
            this.chkOnlyNamedPatterns.UseVisualStyleBackColor = true;
            // 
            // cboUsesRecursion
            // 
            this.cboUsesRecursion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsesRecursion.FormattingEnabled = true;
            this.cboUsesRecursion.Location = new System.Drawing.Point(392, 61);
            this.cboUsesRecursion.Name = "cboUsesRecursion";
            this.cboUsesRecursion.Size = new System.Drawing.Size(54, 21);
            this.cboUsesRecursion.TabIndex = 33;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(301, 64);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "Uses Recursion:";
            // 
            // SelectPatternForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 768);
            this.ControlBox = false;
            this.Controls.Add(this.pnlPatternFilters);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlPatternDashboard);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "SelectPatternForm";
            this.Text = "Select Pattern";
            this.contextMenuStrip1.ResumeLayout(false);
            this.pnlPatternDashboard.ResumeLayout(false);
            this.pnlPatternDashboard.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.pnlPatternFilters.ResumeLayout(false);
            this.pnlPatternFilters.PerformLayout();
            this.pnlRibbonFilters.ResumeLayout(false);
            this.pnlRibbonFilters.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editPatternToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPatternFromDefaultToolStripMenuItem;
        private System.Windows.Forms.Panel pnlPatternDashboard;
        private System.Windows.Forms.ComboBox cboPatternName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnFilterPatterns;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlPatterns;
        private System.Windows.Forms.ComboBox cboPageRange;
        private System.Windows.Forms.Label lblPageRange;
        private System.Windows.Forms.FlowLayoutPanel pnlPages;
        private System.Windows.Forms.Panel pnlPatternFilters;
        private System.Windows.Forms.ComboBox cboHasRandom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboUsesSection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel pnlRibbonFilters;
        private System.Windows.Forms.ComboBox cboRibbonUsesFormula;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkRibbonType;
        private System.Windows.Forms.ComboBox cboRibbonType;
        private System.Windows.Forms.ComboBox cboTransformName;
        private System.Windows.Forms.CheckBox chkPatternType;
        private System.Windows.Forms.ComboBox cboPatternType;
        private System.Windows.Forms.CheckBox chkTransform;
        private System.Windows.Forms.CheckBox chkBasicOutlineType;
        private System.Windows.Forms.ComboBox cboBasicOutlineType;
        private System.Windows.Forms.CheckBox chkFilterPatterns;
        private System.Windows.Forms.Button btnCloseFilters;
        private System.Windows.Forms.Button btnApplyFilters;
        private System.Windows.Forms.CheckBox chkOnlyNamedPatterns;
        private System.Windows.Forms.ComboBox cboUsesRecursion;
        private System.Windows.Forms.Label label6;
    }
}