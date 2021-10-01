namespace Whorl
{
    partial class ImprovConfigForm
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
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.chkImproviseOnAllPatterns = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkDrawDesignLayers = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(13, 22);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(82, 21);
            this.chkEnabled.TabIndex = 0;
            this.chkEnabled.Text = "Enabled";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // chkImproviseOnAllPatterns
            // 
            this.chkImproviseOnAllPatterns.AutoSize = true;
            this.chkImproviseOnAllPatterns.Location = new System.Drawing.Point(13, 49);
            this.chkImproviseOnAllPatterns.Name = "chkImproviseOnAllPatterns";
            this.chkImproviseOnAllPatterns.Size = new System.Drawing.Size(185, 21);
            this.chkImproviseOnAllPatterns.TabIndex = 1;
            this.chkImproviseOnAllPatterns.Text = "Improvise on all Patterns";
            this.chkImproviseOnAllPatterns.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(65, 121);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 34);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(157, 121);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 34);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkDrawDesignLayers
            // 
            this.chkDrawDesignLayers.AutoSize = true;
            this.chkDrawDesignLayers.Location = new System.Drawing.Point(13, 76);
            this.chkDrawDesignLayers.Name = "chkDrawDesignLayers";
            this.chkDrawDesignLayers.Size = new System.Drawing.Size(157, 21);
            this.chkDrawDesignLayers.TabIndex = 4;
            this.chkDrawDesignLayers.Text = "Draw Design Layers";
            this.chkDrawDesignLayers.UseVisualStyleBackColor = true;
            // 
            // ImprovConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 182);
            this.ControlBox = false;
            this.Controls.Add(this.chkDrawDesignLayers);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chkImproviseOnAllPatterns);
            this.Controls.Add(this.chkEnabled);
            this.Name = "ImprovConfigForm";
            this.Text = "Improvisation Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkImproviseOnAllPatterns;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkDrawDesignLayers;
    }
}