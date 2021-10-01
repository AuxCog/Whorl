namespace Whorl
{
    partial class DesignLayersForm
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
            this.dgvDesignLayers = new System.Windows.Forms.DataGridView();
            this.btnAddLayer = new System.Windows.Forms.Button();
            this.designLayerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.colDeleteLayer = new System.Windows.Forms.DataGridViewLinkColumn();
            this.colIsSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colIsDefaultLayer = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colLayerVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colColorBlendType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colLayerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colZOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBlendStrength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDesignLayers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.designLayerBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvDesignLayers
            // 
            this.dgvDesignLayers.AllowUserToAddRows = false;
            this.dgvDesignLayers.AllowUserToDeleteRows = false;
            this.dgvDesignLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDesignLayers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDeleteLayer,
            this.colIsSelected,
            this.colIsDefaultLayer,
            this.colLayerVisible,
            this.colColorBlendType,
            this.colLayerName,
            this.colZOrder,
            this.colBlendStrength});
            this.dgvDesignLayers.Location = new System.Drawing.Point(7, 44);
            this.dgvDesignLayers.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDesignLayers.Name = "dgvDesignLayers";
            this.dgvDesignLayers.RowTemplate.Height = 24;
            this.dgvDesignLayers.Size = new System.Drawing.Size(827, 188);
            this.dgvDesignLayers.TabIndex = 0;
            this.dgvDesignLayers.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDesignLayers_CellContentClick);
            this.dgvDesignLayers.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvDesignLayers_CellMouseUp);
            this.dgvDesignLayers.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDesignLayers_CellValueChanged);
            // 
            // btnAddLayer
            // 
            this.btnAddLayer.Location = new System.Drawing.Point(733, -4);
            this.btnAddLayer.Margin = new System.Windows.Forms.Padding(4);
            this.btnAddLayer.Name = "btnAddLayer";
            this.btnAddLayer.Size = new System.Drawing.Size(101, 40);
            this.btnAddLayer.TabIndex = 1;
            this.btnAddLayer.Text = "Add Layer";
            this.btnAddLayer.UseVisualStyleBackColor = true;
            this.btnAddLayer.Click += new System.EventHandler(this.btnAddLayer_Click);
            // 
            // designLayerBindingSource
            // 
            this.designLayerBindingSource.DataSource = typeof(Whorl.DesignLayer);
            // 
            // colDeleteLayer
            // 
            this.colDeleteLayer.HeaderText = "";
            this.colDeleteLayer.Name = "colDeleteLayer";
            this.colDeleteLayer.Text = "Delete";
            this.colDeleteLayer.UseColumnTextForLinkValue = true;
            this.colDeleteLayer.Width = 70;
            // 
            // colIsSelected
            // 
            this.colIsSelected.HeaderText = "Selected";
            this.colIsSelected.Name = "colIsSelected";
            this.colIsSelected.Width = 85;
            // 
            // colIsDefaultLayer
            // 
            this.colIsDefaultLayer.HeaderText = "Default";
            this.colIsDefaultLayer.Name = "colIsDefaultLayer";
            this.colIsDefaultLayer.Width = 85;
            // 
            // colLayerVisible
            // 
            this.colLayerVisible.DataPropertyName = "Visible";
            this.colLayerVisible.HeaderText = "Visible";
            this.colLayerVisible.Name = "colLayerVisible";
            this.colLayerVisible.Width = 70;
            // 
            // colColorBlendType
            // 
            this.colColorBlendType.DataPropertyName = "ColorBlendType";
            this.colColorBlendType.HeaderText = "Blend";
            this.colColorBlendType.Name = "colColorBlendType";
            // 
            // colLayerName
            // 
            this.colLayerName.DataPropertyName = "LayerName";
            this.colLayerName.HeaderText = "Name";
            this.colLayerName.Name = "colLayerName";
            this.colLayerName.ReadOnly = true;
            // 
            // colZOrder
            // 
            this.colZOrder.DataPropertyName = "ZOrder";
            this.colZOrder.HeaderText = "Z-Order";
            this.colZOrder.Name = "colZOrder";
            this.colZOrder.Width = 80;
            // 
            // colBlendStrength
            // 
            this.colBlendStrength.DataPropertyName = "BlendStrength";
            this.colBlendStrength.HeaderText = "Strength";
            this.colBlendStrength.Name = "colBlendStrength";
            // 
            // DesignLayersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 245);
            this.Controls.Add(this.btnAddLayer);
            this.Controls.Add(this.dgvDesignLayers);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DesignLayersForm";
            this.Text = "Design Layers";
            this.Activated += new System.EventHandler(this.DesignLayersForm_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDesignLayers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.designLayerBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDesignLayers;
        private System.Windows.Forms.Button btnAddLayer;
        private System.Windows.Forms.BindingSource designLayerBindingSource;
        private System.Windows.Forms.DataGridViewLinkColumn colDeleteLayer;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsSelected;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsDefaultLayer;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colLayerVisible;
        private System.Windows.Forms.DataGridViewComboBoxColumn colColorBlendType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLayerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colZOrder;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBlendStrength;
    }
}