namespace Whorl
{
    partial class ColorTransparencyForm
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
            this.hscrlTransparency = new System.Windows.Forms.HScrollBar();
            this.txtTransparency = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // hscrlTransparency
            // 
            this.hscrlTransparency.Location = new System.Drawing.Point(27, 16);
            this.hscrlTransparency.Name = "hscrlTransparency";
            this.hscrlTransparency.Size = new System.Drawing.Size(204, 21);
            this.hscrlTransparency.TabIndex = 0;
            this.hscrlTransparency.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hscrlTransparency_Scroll);
            // 
            // txtTransparency
            // 
            this.txtTransparency.Location = new System.Drawing.Point(248, 16);
            this.txtTransparency.Name = "txtTransparency";
            this.txtTransparency.Size = new System.Drawing.Size(76, 22);
            this.txtTransparency.TabIndex = 1;
            this.txtTransparency.TextChanged += new System.EventHandler(this.txtTransparency_TextChanged);
            // 
            // ColorTransparencyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 49);
            this.Controls.Add(this.txtTransparency);
            this.Controls.Add(this.hscrlTransparency);
            this.Name = "ColorTransparencyForm";
            this.Text = "Set Transparency";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HScrollBar hscrlTransparency;
        private System.Windows.Forms.TextBox txtTransparency;
    }
}