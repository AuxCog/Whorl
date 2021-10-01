namespace Whorl
{
    partial class frmColorGradient
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
            this.picGradient = new System.Windows.Forms.PictureBox();
            this.gradientContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pickColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chooseColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editTransparencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addColorToChoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGradientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteGradientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPaletteToChoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.picGradient)).BeginInit();
            this.gradientContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // picGradient
            // 
            this.picGradient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picGradient.Location = new System.Drawing.Point(0, 0);
            this.picGradient.Name = "picGradient";
            this.picGradient.Size = new System.Drawing.Size(718, 170);
            this.picGradient.TabIndex = 0;
            this.picGradient.TabStop = false;
            // 
            // gradientContextMenu
            // 
            this.gradientContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pickColorToolStripMenuItem,
            this.chooseColorToolStripMenuItem,
            this.addColorToolStripMenuItem,
            this.deleteColorToolStripMenuItem,
            this.editTransparencyToolStripMenuItem,
            this.addColorToChoicesToolStripMenuItem,
            this.copyGradientToolStripMenuItem,
            this.pasteGradientToolStripMenuItem,
            this.addPaletteToChoicesToolStripMenuItem,
            this.selectPaletteToolStripMenuItem});
            this.gradientContextMenu.Name = "gradientContextMenu";
            this.gradientContextMenu.Size = new System.Drawing.Size(195, 246);
            // 
            // pickColorToolStripMenuItem
            // 
            this.pickColorToolStripMenuItem.Name = "pickColorToolStripMenuItem";
            this.pickColorToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.pickColorToolStripMenuItem.Text = "Pick Color";
            // 
            // chooseColorToolStripMenuItem
            // 
            this.chooseColorToolStripMenuItem.Name = "chooseColorToolStripMenuItem";
            this.chooseColorToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.chooseColorToolStripMenuItem.Text = "Choose Color";
            // 
            // addColorToolStripMenuItem
            // 
            this.addColorToolStripMenuItem.Name = "addColorToolStripMenuItem";
            this.addColorToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addColorToolStripMenuItem.Text = "Add Color";
            // 
            // deleteColorToolStripMenuItem
            // 
            this.deleteColorToolStripMenuItem.Name = "deleteColorToolStripMenuItem";
            this.deleteColorToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.deleteColorToolStripMenuItem.Text = "Delete Color";
            // 
            // editTransparencyToolStripMenuItem
            // 
            this.editTransparencyToolStripMenuItem.Name = "editTransparencyToolStripMenuItem";
            this.editTransparencyToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.editTransparencyToolStripMenuItem.Text = "Edit Transparency";
            // 
            // addColorToChoicesToolStripMenuItem
            // 
            this.addColorToChoicesToolStripMenuItem.Name = "addColorToChoicesToolStripMenuItem";
            this.addColorToChoicesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addColorToChoicesToolStripMenuItem.Text = "Add Color to Choices";
            // 
            // copyGradientToolStripMenuItem
            // 
            this.copyGradientToolStripMenuItem.Name = "copyGradientToolStripMenuItem";
            this.copyGradientToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyGradientToolStripMenuItem.Text = "Copy Gradient";
            // 
            // pasteGradientToolStripMenuItem
            // 
            this.pasteGradientToolStripMenuItem.Name = "pasteGradientToolStripMenuItem";
            this.pasteGradientToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.pasteGradientToolStripMenuItem.Text = "Paste Gradient";
            // 
            // addPaletteToChoicesToolStripMenuItem
            // 
            this.addPaletteToChoicesToolStripMenuItem.Name = "addPaletteToChoicesToolStripMenuItem";
            this.addPaletteToChoicesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addPaletteToChoicesToolStripMenuItem.Text = "Add Palette to Choices";
            // 
            // selectPaletteToolStripMenuItem
            // 
            this.selectPaletteToolStripMenuItem.Name = "selectPaletteToolStripMenuItem";
            this.selectPaletteToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.selectPaletteToolStripMenuItem.Text = "Select Palette";
            // 
            // frmColorGradient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(718, 170);
            this.Controls.Add(this.picGradient);
            this.Name = "frmColorGradient";
            this.Text = "frmColorGradient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmColorGradient_FormClosing);
            this.Resize += new System.EventHandler(this.frmColorGradient_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picGradient)).EndInit();
            this.gradientContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picGradient;
        private System.Windows.Forms.ContextMenuStrip gradientContextMenu;
        private System.Windows.Forms.ToolStripMenuItem pickColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chooseColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addColorToChoicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGradientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteGradientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editTransparencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPaletteToChoicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectPaletteToolStripMenuItem;
    }
}