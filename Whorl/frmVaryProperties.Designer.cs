namespace Whorl
{
    partial class frmVaryProperties
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
            this.treeviewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setPropertyActionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removePropertyActionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabChooseProperty = new System.Windows.Forms.TabPage();
            this.btnDeselectProperty = new System.Windows.Forms.Button();
            this.txtPropertyValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSelectedPropertyName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTargetType = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tvProperties = new System.Windows.Forms.TreeView();
            this.tabRun = new System.Windows.Forms.TabPage();
            this.pnlRun = new System.Windows.Forms.Panel();
            this.btnSetValue = new System.Windows.Forms.Button();
            this.btnNegateIncrement = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtCurrentStep = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnApplyAndClose = new System.Windows.Forms.Button();
            this.txtRateOfChange = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnToggleRun = new System.Windows.Forms.Button();
            this.txtRunPropertyValue = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPropertyIncrement = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.picPattern = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeviewContextMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabChooseProperty.SuspendLayout();
            this.tabRun.SuspendLayout();
            this.pnlRun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPattern)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeviewContextMenu
            // 
            this.treeviewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setPropertyActionToolStripMenuItem,
            this.removePropertyActionToolStripMenuItem});
            this.treeviewContextMenu.Name = "treeviewContextMenu";
            this.treeviewContextMenu.Size = new System.Drawing.Size(204, 48);
            // 
            // setPropertyActionToolStripMenuItem
            // 
            this.setPropertyActionToolStripMenuItem.Name = "setPropertyActionToolStripMenuItem";
            this.setPropertyActionToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.setPropertyActionToolStripMenuItem.Text = "Set Property Action";
            this.setPropertyActionToolStripMenuItem.Click += new System.EventHandler(this.setPropertyActionToolStripMenuItem_Click);
            // 
            // removePropertyActionToolStripMenuItem
            // 
            this.removePropertyActionToolStripMenuItem.Name = "removePropertyActionToolStripMenuItem";
            this.removePropertyActionToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.removePropertyActionToolStripMenuItem.Text = "Remove Property Action";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabChooseProperty);
            this.tabControl1.Controls.Add(this.tabRun);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.Location = new System.Drawing.Point(0, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(740, 442);
            this.tabControl1.TabIndex = 4;
            // 
            // tabChooseProperty
            // 
            this.tabChooseProperty.Controls.Add(this.btnDeselectProperty);
            this.tabChooseProperty.Controls.Add(this.txtPropertyValue);
            this.tabChooseProperty.Controls.Add(this.label4);
            this.tabChooseProperty.Controls.Add(this.lblSelectedPropertyName);
            this.tabChooseProperty.Controls.Add(this.label3);
            this.tabChooseProperty.Controls.Add(this.lblTargetType);
            this.tabChooseProperty.Controls.Add(this.label1);
            this.tabChooseProperty.Controls.Add(this.tvProperties);
            this.tabChooseProperty.Location = new System.Drawing.Point(4, 22);
            this.tabChooseProperty.Name = "tabChooseProperty";
            this.tabChooseProperty.Padding = new System.Windows.Forms.Padding(3);
            this.tabChooseProperty.Size = new System.Drawing.Size(732, 416);
            this.tabChooseProperty.TabIndex = 0;
            this.tabChooseProperty.Text = "Choose Property";
            this.tabChooseProperty.UseVisualStyleBackColor = true;
            // 
            // btnDeselectProperty
            // 
            this.btnDeselectProperty.Location = new System.Drawing.Point(574, 20);
            this.btnDeselectProperty.Name = "btnDeselectProperty";
            this.btnDeselectProperty.Size = new System.Drawing.Size(145, 23);
            this.btnDeselectProperty.TabIndex = 14;
            this.btnDeselectProperty.Text = "Deselect Property";
            this.btnDeselectProperty.UseVisualStyleBackColor = true;
            this.btnDeselectProperty.Click += new System.EventHandler(this.btnDeselectProperty_Click);
            // 
            // txtPropertyValue
            // 
            this.txtPropertyValue.Location = new System.Drawing.Point(451, 22);
            this.txtPropertyValue.Name = "txtPropertyValue";
            this.txtPropertyValue.ReadOnly = true;
            this.txtPropertyValue.Size = new System.Drawing.Size(100, 20);
            this.txtPropertyValue.TabIndex = 13;
            this.txtPropertyValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(366, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Property Value:";
            // 
            // lblSelectedPropertyName
            // 
            this.lblSelectedPropertyName.AutoSize = true;
            this.lblSelectedPropertyName.Location = new System.Drawing.Point(260, 25);
            this.lblSelectedPropertyName.Name = "lblSelectedPropertyName";
            this.lblSelectedPropertyName.Size = new System.Drawing.Size(37, 13);
            this.lblSelectedPropertyName.TabIndex = 9;
            this.lblSelectedPropertyName.Text = "(none)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(160, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Selected Property:";
            // 
            // lblTargetType
            // 
            this.lblTargetType.AutoSize = true;
            this.lblTargetType.Location = new System.Drawing.Point(81, 25);
            this.lblTargetType.Name = "lblTargetType";
            this.lblTargetType.Size = new System.Drawing.Size(62, 13);
            this.lblTargetType.TabIndex = 6;
            this.lblTargetType.Text = "TargetType";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Target Type:";
            // 
            // tvProperties
            // 
            this.tvProperties.Location = new System.Drawing.Point(6, 48);
            this.tvProperties.Name = "tvProperties";
            this.tvProperties.Size = new System.Drawing.Size(713, 360);
            this.tvProperties.TabIndex = 4;
            this.tvProperties.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvProperties_MouseDown);
            // 
            // tabRun
            // 
            this.tabRun.Controls.Add(this.pnlRun);
            this.tabRun.Controls.Add(this.picPattern);
            this.tabRun.Location = new System.Drawing.Point(4, 22);
            this.tabRun.Name = "tabRun";
            this.tabRun.Padding = new System.Windows.Forms.Padding(3);
            this.tabRun.Size = new System.Drawing.Size(732, 416);
            this.tabRun.TabIndex = 1;
            this.tabRun.Text = "Run";
            this.tabRun.UseVisualStyleBackColor = true;
            // 
            // pnlRun
            // 
            this.pnlRun.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlRun.Controls.Add(this.btnSetValue);
            this.pnlRun.Controls.Add(this.btnNegateIncrement);
            this.pnlRun.Controls.Add(this.lblStatus);
            this.pnlRun.Controls.Add(this.label8);
            this.pnlRun.Controls.Add(this.txtCurrentStep);
            this.pnlRun.Controls.Add(this.label2);
            this.pnlRun.Controls.Add(this.btnApplyAndClose);
            this.pnlRun.Controls.Add(this.txtRateOfChange);
            this.pnlRun.Controls.Add(this.label7);
            this.pnlRun.Controls.Add(this.btnToggleRun);
            this.pnlRun.Controls.Add(this.txtRunPropertyValue);
            this.pnlRun.Controls.Add(this.label6);
            this.pnlRun.Controls.Add(this.txtPropertyIncrement);
            this.pnlRun.Controls.Add(this.label5);
            this.pnlRun.Location = new System.Drawing.Point(8, 8);
            this.pnlRun.Name = "pnlRun";
            this.pnlRun.Size = new System.Drawing.Size(306, 256);
            this.pnlRun.TabIndex = 25;
            // 
            // btnSetValue
            // 
            this.btnSetValue.Location = new System.Drawing.Point(232, 12);
            this.btnSetValue.Name = "btnSetValue";
            this.btnSetValue.Size = new System.Drawing.Size(67, 23);
            this.btnSetValue.TabIndex = 37;
            this.btnSetValue.Text = "Set Value";
            this.btnSetValue.UseVisualStyleBackColor = true;
            this.btnSetValue.Click += new System.EventHandler(this.btnSetValue_Click);
            // 
            // btnNegateIncrement
            // 
            this.btnNegateIncrement.Location = new System.Drawing.Point(232, 37);
            this.btnNegateIncrement.Name = "btnNegateIncrement";
            this.btnNegateIncrement.Size = new System.Drawing.Size(56, 23);
            this.btnNegateIncrement.TabIndex = 36;
            this.btnNegateIncrement.Text = "Negate";
            this.btnNegateIncrement.UseVisualStyleBackColor = true;
            this.btnNegateIncrement.Click += new System.EventHandler(this.btnNegateIncrement_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(122, 149);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(43, 13);
            this.lblStatus.TabIndex = 35;
            this.lblStatus.Text = "(Status)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 150);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 34;
            this.label8.Text = "Status:";
            // 
            // txtCurrentStep
            // 
            this.txtCurrentStep.Location = new System.Drawing.Point(122, 91);
            this.txtCurrentStep.Name = "txtCurrentStep";
            this.txtCurrentStep.ReadOnly = true;
            this.txtCurrentStep.Size = new System.Drawing.Size(100, 20);
            this.txtCurrentStep.TabIndex = 33;
            this.txtCurrentStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Current Step:";
            // 
            // btnApplyAndClose
            // 
            this.btnApplyAndClose.Location = new System.Drawing.Point(122, 120);
            this.btnApplyAndClose.Name = "btnApplyAndClose";
            this.btnApplyAndClose.Size = new System.Drawing.Size(100, 23);
            this.btnApplyAndClose.TabIndex = 31;
            this.btnApplyAndClose.Text = "Apply and Close";
            this.btnApplyAndClose.UseVisualStyleBackColor = true;
            this.btnApplyAndClose.Click += new System.EventHandler(this.btnApplyAndClose_Click);
            // 
            // txtRateOfChange
            // 
            this.txtRateOfChange.Location = new System.Drawing.Point(122, 65);
            this.txtRateOfChange.Name = "txtRateOfChange";
            this.txtRateOfChange.Size = new System.Drawing.Size(100, 20);
            this.txtRateOfChange.TabIndex = 30;
            this.txtRateOfChange.Text = "500";
            this.txtRateOfChange.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 68);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 13);
            this.label7.TabIndex = 29;
            this.label7.Text = "Rate of Change:";
            // 
            // btnToggleRun
            // 
            this.btnToggleRun.Location = new System.Drawing.Point(20, 120);
            this.btnToggleRun.Name = "btnToggleRun";
            this.btnToggleRun.Size = new System.Drawing.Size(75, 23);
            this.btnToggleRun.TabIndex = 28;
            this.btnToggleRun.Text = "Run";
            this.btnToggleRun.UseVisualStyleBackColor = true;
            this.btnToggleRun.Click += new System.EventHandler(this.btnToggleRun_Click);
            // 
            // txtRunPropertyValue
            // 
            this.txtRunPropertyValue.Location = new System.Drawing.Point(122, 15);
            this.txtRunPropertyValue.Name = "txtRunPropertyValue";
            this.txtRunPropertyValue.Size = new System.Drawing.Size(100, 20);
            this.txtRunPropertyValue.TabIndex = 27;
            this.txtRunPropertyValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Property Value:";
            // 
            // txtPropertyIncrement
            // 
            this.txtPropertyIncrement.Location = new System.Drawing.Point(122, 39);
            this.txtPropertyIncrement.Name = "txtPropertyIncrement";
            this.txtPropertyIncrement.Size = new System.Drawing.Size(100, 20);
            this.txtPropertyIncrement.TabIndex = 25;
            this.txtPropertyIncrement.Text = "1";
            this.txtPropertyIncrement.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Property Increment:";
            // 
            // picPattern
            // 
            this.picPattern.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picPattern.Location = new System.Drawing.Point(324, 8);
            this.picPattern.Name = "picPattern";
            this.picPattern.Size = new System.Drawing.Size(400, 400);
            this.picPattern.TabIndex = 24;
            this.picPattern.TabStop = false;
            this.picPattern.Paint += new System.Windows.Forms.PaintEventHandler(this.picPattern_Paint);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(740, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.UndoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.RedoToolStripMenuItem_Click);
            // 
            // frmVaryProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 469);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.tabControl1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmVaryProperties";
            this.Text = "Vary Properties or Parameters";
            this.treeviewContextMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabChooseProperty.ResumeLayout(false);
            this.tabChooseProperty.PerformLayout();
            this.tabRun.ResumeLayout(false);
            this.pnlRun.ResumeLayout(false);
            this.pnlRun.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPattern)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip treeviewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem setPropertyActionToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabChooseProperty;
        private System.Windows.Forms.TabPage tabRun;
        private System.Windows.Forms.Label lblTargetType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView tvProperties;
        private System.Windows.Forms.ToolStripMenuItem removePropertyActionToolStripMenuItem;
        private System.Windows.Forms.Label lblSelectedPropertyName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPropertyValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox picPattern;
        private System.Windows.Forms.Button btnDeselectProperty;
        private System.Windows.Forms.Panel pnlRun;
        private System.Windows.Forms.Button btnApplyAndClose;
        private System.Windows.Forms.TextBox txtRateOfChange;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnToggleRun;
        private System.Windows.Forms.TextBox txtRunPropertyValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPropertyIncrement;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtCurrentStep;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnNegateIncrement;
        private System.Windows.Forms.Button btnSetValue;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
    }
}