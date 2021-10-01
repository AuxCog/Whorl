namespace Whorl
{
    partial class NamedRandomSeedForm
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
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboNamedSeed = new System.Windows.Forms.ComboBox();
            this.btnSetSeed = new System.Windows.Forms.Button();
            this.btnRenameSeed = new System.Windows.Forms.Button();
            this.btnAddSeed = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSeedName = new System.Windows.Forms.TextBox();
            this.btnSetSeedNew = new System.Windows.Forms.Button();
            this.btnDeleteSeed = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(205, 118);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 28);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Seed Names:";
            // 
            // cboNamedSeed
            // 
            this.cboNamedSeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNamedSeed.FormattingEnabled = true;
            this.cboNamedSeed.Location = new System.Drawing.Point(117, 25);
            this.cboNamedSeed.Name = "cboNamedSeed";
            this.cboNamedSeed.Size = new System.Drawing.Size(211, 24);
            this.cboNamedSeed.TabIndex = 6;
            this.cboNamedSeed.SelectedIndexChanged += new System.EventHandler(this.cboNamedSeed_SelectedIndexChanged);
            // 
            // btnSetSeed
            // 
            this.btnSetSeed.Location = new System.Drawing.Point(335, 22);
            this.btnSetSeed.Margin = new System.Windows.Forms.Padding(4);
            this.btnSetSeed.Name = "btnSetSeed";
            this.btnSetSeed.Size = new System.Drawing.Size(61, 28);
            this.btnSetSeed.TabIndex = 10;
            this.btnSetSeed.Text = "Set";
            this.btnSetSeed.UseVisualStyleBackColor = true;
            this.btnSetSeed.Click += new System.EventHandler(this.btnSetSeed_Click);
            // 
            // btnRenameSeed
            // 
            this.btnRenameSeed.Location = new System.Drawing.Point(335, 71);
            this.btnRenameSeed.Margin = new System.Windows.Forms.Padding(4);
            this.btnRenameSeed.Name = "btnRenameSeed";
            this.btnRenameSeed.Size = new System.Drawing.Size(75, 28);
            this.btnRenameSeed.TabIndex = 11;
            this.btnRenameSeed.Text = "Rename";
            this.btnRenameSeed.UseVisualStyleBackColor = true;
            this.btnRenameSeed.Click += new System.EventHandler(this.btnRenameSeed_Click);
            // 
            // btnAddSeed
            // 
            this.btnAddSeed.Location = new System.Drawing.Point(491, 22);
            this.btnAddSeed.Margin = new System.Windows.Forms.Padding(4);
            this.btnAddSeed.Name = "btnAddSeed";
            this.btnAddSeed.Size = new System.Drawing.Size(94, 28);
            this.btnAddSeed.TabIndex = 12;
            this.btnAddSeed.Text = "Add Seed";
            this.btnAddSeed.UseVisualStyleBackColor = true;
            this.btnAddSeed.Click += new System.EventHandler(this.btnAddSeed_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Seed Name:";
            // 
            // txtSeedName
            // 
            this.txtSeedName.Location = new System.Drawing.Point(117, 74);
            this.txtSeedName.Name = "txtSeedName";
            this.txtSeedName.Size = new System.Drawing.Size(211, 22);
            this.txtSeedName.TabIndex = 14;
            // 
            // btnSetSeedNew
            // 
            this.btnSetSeedNew.Location = new System.Drawing.Point(404, 22);
            this.btnSetSeedNew.Margin = new System.Windows.Forms.Padding(4);
            this.btnSetSeedNew.Name = "btnSetSeedNew";
            this.btnSetSeedNew.Size = new System.Drawing.Size(80, 28);
            this.btnSetSeedNew.TabIndex = 15;
            this.btnSetSeedNew.Text = "Set New";
            this.btnSetSeedNew.UseVisualStyleBackColor = true;
            this.btnSetSeedNew.Click += new System.EventHandler(this.btnSetSeedNew_Click);
            // 
            // btnDeleteSeed
            // 
            this.btnDeleteSeed.Location = new System.Drawing.Point(593, 22);
            this.btnDeleteSeed.Margin = new System.Windows.Forms.Padding(4);
            this.btnDeleteSeed.Name = "btnDeleteSeed";
            this.btnDeleteSeed.Size = new System.Drawing.Size(94, 28);
            this.btnDeleteSeed.TabIndex = 16;
            this.btnDeleteSeed.Text = "Delete Seed";
            this.btnDeleteSeed.UseVisualStyleBackColor = true;
            this.btnDeleteSeed.Click += new System.EventHandler(this.btnDeleteSeed_Click);
            // 
            // NamedRandomSeedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 159);
            this.ControlBox = false;
            this.Controls.Add(this.btnDeleteSeed);
            this.Controls.Add(this.btnSetSeedNew);
            this.Controls.Add(this.txtSeedName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnAddSeed);
            this.Controls.Add(this.btnRenameSeed);
            this.Controls.Add(this.btnSetSeed);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboNamedSeed);
            this.Name = "NamedRandomSeedForm";
            this.Text = "Random Seeds";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboNamedSeed;
        private System.Windows.Forms.Button btnSetSeed;
        private System.Windows.Forms.Button btnRenameSeed;
        private System.Windows.Forms.Button btnAddSeed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSeedName;
        private System.Windows.Forms.Button btnSetSeedNew;
        private System.Windows.Forms.Button btnDeleteSeed;
    }
}