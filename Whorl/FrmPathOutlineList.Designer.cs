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
            this.picOutline = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.pnlOutlines = new System.Windows.Forms.Panel();
            this.BtnSort = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cboDisplayMode = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.picOutline)).BeginInit();
            this.SuspendLayout();
            // 
            // picOutline
            // 
            this.picOutline.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.picOutline.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picOutline.Location = new System.Drawing.Point(318, 41);
            this.picOutline.Name = "picOutline";
            this.picOutline.Size = new System.Drawing.Size(300, 300);
            this.picOutline.TabIndex = 0;
            this.picOutline.TabStop = false;
            this.picOutline.Paint += new System.Windows.Forms.PaintEventHandler(this.picOutline_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path Outlines:";
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(475, 12);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(62, 23);
            this.BtnOK.TabIndex = 3;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(543, 12);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 4;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // pnlOutlines
            // 
            this.pnlOutlines.AutoScroll = true;
            this.pnlOutlines.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlOutlines.Location = new System.Drawing.Point(15, 41);
            this.pnlOutlines.Name = "pnlOutlines";
            this.pnlOutlines.Size = new System.Drawing.Size(297, 300);
            this.pnlOutlines.TabIndex = 7;
            // 
            // BtnSort
            // 
            this.BtnSort.Location = new System.Drawing.Point(113, 12);
            this.BtnSort.Name = "BtnSort";
            this.BtnSort.Size = new System.Drawing.Size(54, 23);
            this.BtnSort.TabIndex = 8;
            this.BtnSort.Text = "Sort";
            this.BtnSort.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(318, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Display:";
            // 
            // cboDisplayMode
            // 
            this.cboDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayMode.FormattingEnabled = true;
            this.cboDisplayMode.Location = new System.Drawing.Point(368, 14);
            this.cboDisplayMode.Name = "cboDisplayMode";
            this.cboDisplayMode.Size = new System.Drawing.Size(76, 21);
            this.cboDisplayMode.TabIndex = 10;
            this.cboDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cboDisplayMode_SelectedIndexChanged);
            // 
            // FrmPathOutlineList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 351);
            this.Controls.Add(this.cboDisplayMode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BtnSort);
            this.Controls.Add(this.pnlOutlines);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picOutline);
            this.Name = "FrmPathOutlineList";
            this.Text = "Path Outlines Editor";
            this.Load += new System.EventHandler(this.FrmPathOutlineList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picOutline)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picOutline;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Panel pnlOutlines;
        private System.Windows.Forms.Button BtnSort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboDisplayMode;
    }
}