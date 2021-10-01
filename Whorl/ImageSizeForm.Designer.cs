namespace Whorl
{
    partial class ImageSizeForm
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkResizeDesign = new System.Windows.Forms.CheckBox();
            this.chkDockImage = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboImageSize = new System.Windows.Forms.ComboBox();
            this.btnSetImageSize = new System.Windows.Forms.Button();
            this.btnSaveImageSize = new System.Windows.Forms.Button();
            this.txtSettingName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDeleteImageSize = new System.Windows.Forms.Button();
            this.chkMaintainAspectRatio = new System.Windows.Forms.CheckBox();
            this.chkScaleDesign = new System.Windows.Forms.CheckBox();
            this.chkSizeToFit = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(93, 173);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 173);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 17;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 55);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Image Width:";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(83, 55);
            this.txtWidth.Margin = new System.Windows.Forms.Padding(2);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(68, 20);
            this.txtWidth.TabIndex = 20;
            this.txtWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtWidth.TextChanged += new System.EventHandler(this.txtWidth_TextChanged);
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(83, 79);
            this.txtHeight.Margin = new System.Windows.Forms.Padding(2);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(68, 20);
            this.txtHeight.TabIndex = 22;
            this.txtHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtHeight.TextChanged += new System.EventHandler(this.txtHeight_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 79);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Image Height:";
            // 
            // chkResizeDesign
            // 
            this.chkResizeDesign.AutoSize = true;
            this.chkResizeDesign.Location = new System.Drawing.Point(109, 127);
            this.chkResizeDesign.Margin = new System.Windows.Forms.Padding(2);
            this.chkResizeDesign.Name = "chkResizeDesign";
            this.chkResizeDesign.Size = new System.Drawing.Size(97, 17);
            this.chkResizeDesign.TabIndex = 23;
            this.chkResizeDesign.Text = "Adjust Patterns";
            this.chkResizeDesign.UseVisualStyleBackColor = true;
            // 
            // chkDockImage
            // 
            this.chkDockImage.AutoSize = true;
            this.chkDockImage.Location = new System.Drawing.Point(12, 127);
            this.chkDockImage.Margin = new System.Windows.Forms.Padding(2);
            this.chkDockImage.Name = "chkDockImage";
            this.chkDockImage.Size = new System.Drawing.Size(93, 17);
            this.chkDockImage.TabIndex = 24;
            this.chkDockImage.Text = "Maximum Size";
            this.chkDockImage.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 26);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Preset:";
            // 
            // cboImageSize
            // 
            this.cboImageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImageSize.FormattingEnabled = true;
            this.cboImageSize.Location = new System.Drawing.Point(83, 26);
            this.cboImageSize.Name = "cboImageSize";
            this.cboImageSize.Size = new System.Drawing.Size(227, 21);
            this.cboImageSize.TabIndex = 26;
            // 
            // btnSetImageSize
            // 
            this.btnSetImageSize.Location = new System.Drawing.Point(316, 26);
            this.btnSetImageSize.Name = "btnSetImageSize";
            this.btnSetImageSize.Size = new System.Drawing.Size(52, 23);
            this.btnSetImageSize.TabIndex = 27;
            this.btnSetImageSize.Text = "Set";
            this.btnSetImageSize.UseVisualStyleBackColor = true;
            this.btnSetImageSize.Click += new System.EventHandler(this.btnSetImageSize_Click);
            // 
            // btnSaveImageSize
            // 
            this.btnSaveImageSize.Location = new System.Drawing.Point(161, 79);
            this.btnSaveImageSize.Name = "btnSaveImageSize";
            this.btnSaveImageSize.Size = new System.Drawing.Size(149, 22);
            this.btnSaveImageSize.TabIndex = 28;
            this.btnSaveImageSize.Text = "Save Image Size";
            this.btnSaveImageSize.UseVisualStyleBackColor = true;
            this.btnSaveImageSize.Click += new System.EventHandler(this.btnSaveImageSize_Click);
            // 
            // txtSettingName
            // 
            this.txtSettingName.Location = new System.Drawing.Point(83, 103);
            this.txtSettingName.Margin = new System.Windows.Forms.Padding(2);
            this.txtSettingName.Name = "txtSettingName";
            this.txtSettingName.Size = new System.Drawing.Size(125, 20);
            this.txtSettingName.TabIndex = 30;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 103);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Setting Name:";
            // 
            // btnDeleteImageSize
            // 
            this.btnDeleteImageSize.Location = new System.Drawing.Point(374, 26);
            this.btnDeleteImageSize.Name = "btnDeleteImageSize";
            this.btnDeleteImageSize.Size = new System.Drawing.Size(52, 23);
            this.btnDeleteImageSize.TabIndex = 31;
            this.btnDeleteImageSize.Text = "Delete";
            this.btnDeleteImageSize.UseVisualStyleBackColor = true;
            this.btnDeleteImageSize.Click += new System.EventHandler(this.btnDeleteImageSize_Click);
            // 
            // chkMaintainAspectRatio
            // 
            this.chkMaintainAspectRatio.AutoSize = true;
            this.chkMaintainAspectRatio.Location = new System.Drawing.Point(161, 57);
            this.chkMaintainAspectRatio.Name = "chkMaintainAspectRatio";
            this.chkMaintainAspectRatio.Size = new System.Drawing.Size(130, 17);
            this.chkMaintainAspectRatio.TabIndex = 32;
            this.chkMaintainAspectRatio.Text = "Maintain Aspect Ratio";
            this.chkMaintainAspectRatio.UseVisualStyleBackColor = true;
            // 
            // chkScaleDesign
            // 
            this.chkScaleDesign.AutoSize = true;
            this.chkScaleDesign.Location = new System.Drawing.Point(211, 127);
            this.chkScaleDesign.Margin = new System.Windows.Forms.Padding(2);
            this.chkScaleDesign.Name = "chkScaleDesign";
            this.chkScaleDesign.Size = new System.Drawing.Size(89, 17);
            this.chkScaleDesign.TabIndex = 33;
            this.chkScaleDesign.Text = "Scale Design";
            this.chkScaleDesign.UseVisualStyleBackColor = true;
            // 
            // chkSizeToFit
            // 
            this.chkSizeToFit.AutoSize = true;
            this.chkSizeToFit.Checked = true;
            this.chkSizeToFit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSizeToFit.Location = new System.Drawing.Point(304, 127);
            this.chkSizeToFit.Margin = new System.Windows.Forms.Padding(2);
            this.chkSizeToFit.Name = "chkSizeToFit";
            this.chkSizeToFit.Size = new System.Drawing.Size(72, 17);
            this.chkSizeToFit.TabIndex = 34;
            this.chkSizeToFit.Text = "Size to Fit";
            this.chkSizeToFit.UseVisualStyleBackColor = true;
            // 
            // ImageSizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 206);
            this.ControlBox = false;
            this.Controls.Add(this.chkSizeToFit);
            this.Controls.Add(this.chkScaleDesign);
            this.Controls.Add(this.chkMaintainAspectRatio);
            this.Controls.Add(this.btnDeleteImageSize);
            this.Controls.Add(this.txtSettingName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnSaveImageSize);
            this.Controls.Add(this.btnSetImageSize);
            this.Controls.Add(this.cboImageSize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkDockImage);
            this.Controls.Add(this.chkResizeDesign);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ImageSizeForm";
            this.Text = "Set Image Size";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkResizeDesign;
        private System.Windows.Forms.CheckBox chkDockImage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboImageSize;
        private System.Windows.Forms.Button btnSetImageSize;
        private System.Windows.Forms.Button btnSaveImageSize;
        private System.Windows.Forms.TextBox txtSettingName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnDeleteImageSize;
        private System.Windows.Forms.CheckBox chkMaintainAspectRatio;
        private System.Windows.Forms.CheckBox chkScaleDesign;
        private System.Windows.Forms.CheckBox chkSizeToFit;
    }
}