namespace Whorl
{
    partial class RepeatSettingsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtGridInterval = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRepetitions = new System.Windows.Forms.TextBox();
            this.chkFillGrid = new System.Windows.Forms.CheckBox();
            this.rdoRadial = new System.Windows.Forms.RadioButton();
            this.rdoHorizontal = new System.Windows.Forms.RadioButton();
            this.rdoVertical = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.rdoCircular = new System.Windows.Forms.RadioButton();
            this.chkReverse = new System.Windows.Forms.CheckBox();
            this.chkEntireRibbon = new System.Windows.Forms.CheckBox();
            this.chkSelectedPatternCenter = new System.Windows.Forms.CheckBox();
            this.chkRepeatAtVertices = new System.Windows.Forms.CheckBox();
            this.chkTrackPathAngle = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Grid Squares Interval:";
            // 
            // txtGridInterval
            // 
            this.txtGridInterval.Location = new System.Drawing.Point(128, 10);
            this.txtGridInterval.Name = "txtGridInterval";
            this.txtGridInterval.Size = new System.Drawing.Size(38, 20);
            this.txtGridInterval.TabIndex = 1;
            this.txtGridInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(262, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Repetitions:";
            // 
            // txtRepetitions
            // 
            this.txtRepetitions.Location = new System.Drawing.Point(331, 10);
            this.txtRepetitions.Name = "txtRepetitions";
            this.txtRepetitions.Size = new System.Drawing.Size(38, 20);
            this.txtRepetitions.TabIndex = 3;
            this.txtRepetitions.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkFillGrid
            // 
            this.chkFillGrid.AutoSize = true;
            this.chkFillGrid.Checked = true;
            this.chkFillGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFillGrid.Location = new System.Drawing.Point(190, 12);
            this.chkFillGrid.Name = "chkFillGrid";
            this.chkFillGrid.Size = new System.Drawing.Size(60, 17);
            this.chkFillGrid.TabIndex = 4;
            this.chkFillGrid.Text = "Fill Grid";
            this.chkFillGrid.UseVisualStyleBackColor = true;
            this.chkFillGrid.CheckedChanged += new System.EventHandler(this.chkFillGrid_CheckedChanged);
            // 
            // rdoRadial
            // 
            this.rdoRadial.AutoSize = true;
            this.rdoRadial.Location = new System.Drawing.Point(76, 59);
            this.rdoRadial.Name = "rdoRadial";
            this.rdoRadial.Size = new System.Drawing.Size(55, 17);
            this.rdoRadial.TabIndex = 5;
            this.rdoRadial.TabStop = true;
            this.rdoRadial.Text = "Radial";
            this.rdoRadial.UseVisualStyleBackColor = true;
            // 
            // rdoHorizontal
            // 
            this.rdoHorizontal.AutoSize = true;
            this.rdoHorizontal.Location = new System.Drawing.Point(137, 59);
            this.rdoHorizontal.Name = "rdoHorizontal";
            this.rdoHorizontal.Size = new System.Drawing.Size(72, 17);
            this.rdoHorizontal.TabIndex = 6;
            this.rdoHorizontal.TabStop = true;
            this.rdoHorizontal.Text = "Horizontal";
            this.rdoHorizontal.UseVisualStyleBackColor = true;
            // 
            // rdoVertical
            // 
            this.rdoVertical.AutoSize = true;
            this.rdoVertical.Location = new System.Drawing.Point(215, 59);
            this.rdoVertical.Name = "rdoVertical";
            this.rdoVertical.Size = new System.Drawing.Size(60, 17);
            this.rdoVertical.TabIndex = 7;
            this.rdoVertical.TabStop = true;
            this.rdoVertical.Text = "Vertical";
            this.rdoVertical.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(225, 116);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(317, 116);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // rdoCircular
            // 
            this.rdoCircular.AutoSize = true;
            this.rdoCircular.Location = new System.Drawing.Point(9, 59);
            this.rdoCircular.Name = "rdoCircular";
            this.rdoCircular.Size = new System.Drawing.Size(60, 17);
            this.rdoCircular.TabIndex = 10;
            this.rdoCircular.TabStop = true;
            this.rdoCircular.Text = "Circular";
            this.rdoCircular.UseVisualStyleBackColor = true;
            // 
            // chkReverse
            // 
            this.chkReverse.AutoSize = true;
            this.chkReverse.Location = new System.Drawing.Point(281, 59);
            this.chkReverse.Name = "chkReverse";
            this.chkReverse.Size = new System.Drawing.Size(111, 17);
            this.chkReverse.TabIndex = 11;
            this.chkReverse.Text = "Reverse Direction";
            this.chkReverse.UseVisualStyleBackColor = true;
            // 
            // chkEntireRibbon
            // 
            this.chkEntireRibbon.AutoSize = true;
            this.chkEntireRibbon.Location = new System.Drawing.Point(10, 36);
            this.chkEntireRibbon.Name = "chkEntireRibbon";
            this.chkEntireRibbon.Size = new System.Drawing.Size(128, 17);
            this.chkEntireRibbon.TabIndex = 12;
            this.chkEntireRibbon.Text = "Repeat Entire Ribbon";
            this.chkEntireRibbon.UseVisualStyleBackColor = true;
            // 
            // chkSelectedPatternCenter
            // 
            this.chkSelectedPatternCenter.AutoSize = true;
            this.chkSelectedPatternCenter.Location = new System.Drawing.Point(144, 36);
            this.chkSelectedPatternCenter.Name = "chkSelectedPatternCenter";
            this.chkSelectedPatternCenter.Size = new System.Drawing.Size(168, 17);
            this.chkSelectedPatternCenter.TabIndex = 13;
            this.chkSelectedPatternCenter.Text = "Use Selected Pattern\'s Center";
            this.chkSelectedPatternCenter.UseVisualStyleBackColor = true;
            // 
            // chkRepeatAtVertices
            // 
            this.chkRepeatAtVertices.AutoSize = true;
            this.chkRepeatAtVertices.Location = new System.Drawing.Point(10, 91);
            this.chkRepeatAtVertices.Name = "chkRepeatAtVertices";
            this.chkRepeatAtVertices.Size = new System.Drawing.Size(188, 17);
            this.chkRepeatAtVertices.TabIndex = 14;
            this.chkRepeatAtVertices.Text = "Repeat Default Pattern at Vertices";
            this.chkRepeatAtVertices.UseVisualStyleBackColor = true;
            // 
            // chkTrackPathAngle
            // 
            this.chkTrackPathAngle.AutoSize = true;
            this.chkTrackPathAngle.Location = new System.Drawing.Point(204, 91);
            this.chkTrackPathAngle.Name = "chkTrackPathAngle";
            this.chkTrackPathAngle.Size = new System.Drawing.Size(109, 17);
            this.chkTrackPathAngle.TabIndex = 15;
            this.chkTrackPathAngle.Text = "Track Path Angle";
            this.chkTrackPathAngle.UseVisualStyleBackColor = true;
            // 
            // RepeatSettingsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 151);
            this.Controls.Add(this.chkTrackPathAngle);
            this.Controls.Add(this.chkRepeatAtVertices);
            this.Controls.Add(this.chkSelectedPatternCenter);
            this.Controls.Add(this.chkEntireRibbon);
            this.Controls.Add(this.chkReverse);
            this.Controls.Add(this.rdoCircular);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.rdoVertical);
            this.Controls.Add(this.rdoHorizontal);
            this.Controls.Add(this.rdoRadial);
            this.Controls.Add(this.chkFillGrid);
            this.Controls.Add(this.txtRepetitions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtGridInterval);
            this.Controls.Add(this.label1);
            this.Name = "RepeatSettingsForm";
            this.Text = "Repeat Pattern Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtGridInterval;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRepetitions;
        private System.Windows.Forms.CheckBox chkFillGrid;
        private System.Windows.Forms.RadioButton rdoRadial;
        private System.Windows.Forms.RadioButton rdoHorizontal;
        private System.Windows.Forms.RadioButton rdoVertical;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rdoCircular;
        private System.Windows.Forms.CheckBox chkReverse;
        private System.Windows.Forms.CheckBox chkEntireRibbon;
        private System.Windows.Forms.CheckBox chkSelectedPatternCenter;
        private System.Windows.Forms.CheckBox chkRepeatAtVertices;
        private System.Windows.Forms.CheckBox chkTrackPathAngle;
    }
}