namespace Whorl
{
    partial class RenderDesignForm
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
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRender = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkScalePenWidth = new System.Windows.Forms.CheckBox();
            this.chkDraftMode = new System.Windows.Forms.CheckBox();
            this.chkQualityMode = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(236, 37);
            this.txtHeight.Margin = new System.Windows.Forms.Padding(2);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(68, 20);
            this.txtHeight.TabIndex = 30;
            this.txtHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtHeight.TextChanged += new System.EventHandler(this.txtHeight_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(164, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Image Height:";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(81, 37);
            this.txtWidth.Margin = new System.Windows.Forms.Padding(2);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(68, 20);
            this.txtWidth.TabIndex = 28;
            this.txtWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtWidth.TextChanged += new System.EventHandler(this.txtWidth_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Image Width:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(101, 93);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 26;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnRender
            // 
            this.btnRender.Location = new System.Drawing.Point(10, 93);
            this.btnRender.Name = "btnRender";
            this.btnRender.Size = new System.Drawing.Size(75, 23);
            this.btnRender.TabIndex = 25;
            this.btnRender.Text = "Render";
            this.btnRender.UseVisualStyleBackColor = true;
            this.btnRender.Click += new System.EventHandler(this.btnRender_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "File:";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(67, 11);
            this.lblFileName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(43, 13);
            this.lblFileName.TabIndex = 32;
            this.lblFileName.Text = "(Select)";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(37, 6);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(25, 23);
            this.btnBrowse.TabIndex = 33;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // chkScalePenWidth
            // 
            this.chkScalePenWidth.AutoSize = true;
            this.chkScalePenWidth.Checked = true;
            this.chkScalePenWidth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScalePenWidth.Location = new System.Drawing.Point(13, 63);
            this.chkScalePenWidth.Margin = new System.Windows.Forms.Padding(2);
            this.chkScalePenWidth.Name = "chkScalePenWidth";
            this.chkScalePenWidth.Size = new System.Drawing.Size(112, 17);
            this.chkScalePenWidth.TabIndex = 34;
            this.chkScalePenWidth.Text = "Scale Line Widths";
            this.chkScalePenWidth.UseVisualStyleBackColor = true;
            // 
            // chkDraftMode
            // 
            this.chkDraftMode.AutoSize = true;
            this.chkDraftMode.Location = new System.Drawing.Point(129, 63);
            this.chkDraftMode.Margin = new System.Windows.Forms.Padding(2);
            this.chkDraftMode.Name = "chkDraftMode";
            this.chkDraftMode.Size = new System.Drawing.Size(79, 17);
            this.chkDraftMode.TabIndex = 35;
            this.chkDraftMode.Text = "Draft Mode";
            this.chkDraftMode.UseVisualStyleBackColor = true;
            // 
            // chkQualityMode
            // 
            this.chkQualityMode.AutoSize = true;
            this.chkQualityMode.Location = new System.Drawing.Point(225, 63);
            this.chkQualityMode.Margin = new System.Windows.Forms.Padding(2);
            this.chkQualityMode.Name = "chkQualityMode";
            this.chkQualityMode.Size = new System.Drawing.Size(88, 17);
            this.chkQualityMode.TabIndex = 36;
            this.chkQualityMode.Text = "Quality Mode";
            this.chkQualityMode.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(10, 123);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(328, 23);
            this.progressBar1.TabIndex = 37;
            // 
            // RenderDesignForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 150);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.chkQualityMode);
            this.Controls.Add(this.chkDraftMode);
            this.Controls.Add(this.chkScalePenWidth);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRender);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RenderDesignForm";
            this.Text = "Render Design Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRender;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkScalePenWidth;
        private System.Windows.Forms.CheckBox chkDraftMode;
        private System.Windows.Forms.CheckBox chkQualityMode;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}