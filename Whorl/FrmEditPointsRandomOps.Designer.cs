
namespace Whorl
{
    partial class FrmEditPointsRandomOps
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtHorizCount = new System.Windows.Forms.TextBox();
            this.txtVertCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPointRandomWeight = new System.Windows.Forms.TextBox();
            this.txtValueWeight = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPower = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.BtnDisplay = new System.Windows.Forms.Button();
            this.picDisplay = new System.Windows.Forms.PictureBox();
            this.BtnReseedPoints = new System.Windows.Forms.Button();
            this.BtnReseedValues = new System.Windows.Forms.Button();
            this.BtnDisplayPoints = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtInnerOffset = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtInnerWeight = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(509, 55);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(53, 23);
            this.BtnCancel.TabIndex = 10;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(509, 26);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(53, 23);
            this.BtnOK.TabIndex = 9;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Number of Columns:";
            // 
            // txtHorizCount
            // 
            this.txtHorizCount.Location = new System.Drawing.Point(124, 28);
            this.txtHorizCount.Name = "txtHorizCount";
            this.txtHorizCount.Size = new System.Drawing.Size(56, 20);
            this.txtHorizCount.TabIndex = 12;
            this.txtHorizCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtVertCount
            // 
            this.txtVertCount.Location = new System.Drawing.Point(299, 28);
            this.txtVertCount.Name = "txtVertCount";
            this.txtVertCount.Size = new System.Drawing.Size(56, 20);
            this.txtVertCount.TabIndex = 14;
            this.txtVertCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(204, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Number of Rows:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Point Variation %:";
            // 
            // txtPointRandomWeight
            // 
            this.txtPointRandomWeight.Location = new System.Drawing.Point(124, 57);
            this.txtPointRandomWeight.Name = "txtPointRandomWeight";
            this.txtPointRandomWeight.Size = new System.Drawing.Size(56, 20);
            this.txtPointRandomWeight.TabIndex = 16;
            this.txtPointRandomWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtValueWeight
            // 
            this.txtValueWeight.Location = new System.Drawing.Point(299, 57);
            this.txtValueWeight.Name = "txtValueWeight";
            this.txtValueWeight.Size = new System.Drawing.Size(56, 20);
            this.txtValueWeight.TabIndex = 18;
            this.txtValueWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(219, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Value Weight:";
            // 
            // txtPower
            // 
            this.txtPower.Location = new System.Drawing.Point(299, 86);
            this.txtPower.Name = "txtPower";
            this.txtPower.Size = new System.Drawing.Size(56, 20);
            this.txtPower.TabIndex = 22;
            this.txtPower.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(208, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Distance Power:";
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(124, 86);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(56, 20);
            this.txtOffset.TabIndex = 20;
            this.txtOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(29, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Distance Offset:";
            // 
            // BtnDisplay
            // 
            this.BtnDisplay.Location = new System.Drawing.Point(36, 138);
            this.BtnDisplay.Name = "BtnDisplay";
            this.BtnDisplay.Size = new System.Drawing.Size(144, 23);
            this.BtnDisplay.TabIndex = 23;
            this.BtnDisplay.Text = "Display Random Values";
            this.BtnDisplay.UseVisualStyleBackColor = true;
            this.BtnDisplay.Click += new System.EventHandler(this.BtnDisplay_Click);
            // 
            // picDisplay
            // 
            this.picDisplay.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.picDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picDisplay.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.picDisplay.Location = new System.Drawing.Point(0, 167);
            this.picDisplay.Name = "picDisplay";
            this.picDisplay.Size = new System.Drawing.Size(568, 377);
            this.picDisplay.TabIndex = 24;
            this.picDisplay.TabStop = false;
            this.picDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.picDisplay_Paint);
            // 
            // BtnReseedPoints
            // 
            this.BtnReseedPoints.Location = new System.Drawing.Point(379, 26);
            this.BtnReseedPoints.Name = "BtnReseedPoints";
            this.BtnReseedPoints.Size = new System.Drawing.Size(105, 23);
            this.BtnReseedPoints.TabIndex = 25;
            this.BtnReseedPoints.Text = "Reseed Points";
            this.BtnReseedPoints.UseVisualStyleBackColor = true;
            this.BtnReseedPoints.Click += new System.EventHandler(this.BtnReseedPoints_Click);
            // 
            // BtnReseedValues
            // 
            this.BtnReseedValues.Location = new System.Drawing.Point(379, 55);
            this.BtnReseedValues.Name = "BtnReseedValues";
            this.BtnReseedValues.Size = new System.Drawing.Size(105, 23);
            this.BtnReseedValues.TabIndex = 26;
            this.BtnReseedValues.Text = "Reseed Values";
            this.BtnReseedValues.UseVisualStyleBackColor = true;
            this.BtnReseedValues.Click += new System.EventHandler(this.BtnReseedValues_Click);
            // 
            // BtnDisplayPoints
            // 
            this.BtnDisplayPoints.Location = new System.Drawing.Point(211, 138);
            this.BtnDisplayPoints.Name = "BtnDisplayPoints";
            this.BtnDisplayPoints.Size = new System.Drawing.Size(144, 23);
            this.BtnDisplayPoints.TabIndex = 27;
            this.BtnDisplayPoints.Text = "Display Random Points";
            this.BtnDisplayPoints.UseVisualStyleBackColor = true;
            this.BtnDisplayPoints.Click += new System.EventHandler(this.BtnDisplayPoints_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(568, 24);
            this.menuStrip1.TabIndex = 28;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copySettingsToolStripMenuItem,
            this.pasteSettingsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // copySettingsToolStripMenuItem
            // 
            this.copySettingsToolStripMenuItem.Name = "copySettingsToolStripMenuItem";
            this.copySettingsToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.copySettingsToolStripMenuItem.Text = "Copy Settings";
            this.copySettingsToolStripMenuItem.Click += new System.EventHandler(this.copySettingsToolStripMenuItem_Click);
            // 
            // pasteSettingsToolStripMenuItem
            // 
            this.pasteSettingsToolStripMenuItem.Name = "pasteSettingsToolStripMenuItem";
            this.pasteSettingsToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.pasteSettingsToolStripMenuItem.Text = "Paste Settings";
            this.pasteSettingsToolStripMenuItem.Click += new System.EventHandler(this.pasteSettingsToolStripMenuItem_Click);
            // 
            // txtInnerOffset
            // 
            this.txtInnerOffset.Location = new System.Drawing.Point(299, 112);
            this.txtInnerOffset.Name = "txtInnerOffset";
            this.txtInnerOffset.Size = new System.Drawing.Size(56, 20);
            this.txtInnerOffset.TabIndex = 32;
            this.txtInnerOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(228, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 13);
            this.label7.TabIndex = 31;
            this.label7.Text = "Inner Offset:";
            // 
            // txtInnerWeight
            // 
            this.txtInnerWeight.Location = new System.Drawing.Point(124, 112);
            this.txtInnerWeight.Name = "txtInnerWeight";
            this.txtInnerWeight.Size = new System.Drawing.Size(56, 20);
            this.txtInnerWeight.TabIndex = 30;
            this.txtInnerWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(41, 115);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 13);
            this.label8.TabIndex = 29;
            this.label8.Text = "Inner Weight:";
            // 
            // FrmEditPointsRandomOps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 544);
            this.Controls.Add(this.txtInnerOffset);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtInnerWeight);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.BtnDisplayPoints);
            this.Controls.Add(this.BtnReseedValues);
            this.Controls.Add(this.BtnReseedPoints);
            this.Controls.Add(this.picDisplay);
            this.Controls.Add(this.BtnDisplay);
            this.Controls.Add(this.txtPower);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtOffset);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtValueWeight);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPointRandomWeight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtVertCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtHorizCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FrmEditPointsRandomOps";
            this.Text = "Edit Rendering Random Settings";
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHorizCount;
        private System.Windows.Forms.TextBox txtVertCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPointRandomWeight;
        private System.Windows.Forms.TextBox txtValueWeight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPower;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button BtnDisplay;
        private System.Windows.Forms.PictureBox picDisplay;
        private System.Windows.Forms.Button BtnReseedPoints;
        private System.Windows.Forms.Button BtnReseedValues;
        private System.Windows.Forms.Button BtnDisplayPoints;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copySettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteSettingsToolStripMenuItem;
        private System.Windows.Forms.TextBox txtInnerOffset;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtInnerWeight;
        private System.Windows.Forms.Label label8;
    }
}