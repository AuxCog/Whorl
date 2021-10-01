namespace Whorl
{
    partial class PatternImprovConfigForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.btnSetAllFlags = new System.Windows.Forms.Button();
            this.cboSetAllFlags = new System.Windows.Forms.ComboBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cboImprovOnParameters = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboImprovOnPetals = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboImprovOnShapes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboImprovOnColors = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cboImprovOnOutlineTypes = new System.Windows.Forms.ComboBox();
            this.chkPatternEnabled = new System.Windows.Forms.CheckBox();
            this.tabParameters = new System.Windows.Forms.TabPage();
            this.btnResetFromPattern = new System.Windows.Forms.Button();
            this.pnlEditParamConfig = new System.Windows.Forms.Panel();
            this.txtParameterValue = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnApplyParameter = new System.Windows.Forms.Button();
            this.cboDecimalPlaces = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtImprovStrength = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMaxValue = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtMinValue = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cboParameterType = new System.Windows.Forms.ComboBox();
            this.dgvParameterConfigs = new System.Windows.Forms.DataGridView();
            this.colEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colParameterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFormulaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEdit = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tabColors = new System.Windows.Forms.TabPage();
            this.btnApplyColors = new System.Windows.Forms.Button();
            this.btnClearAllColors = new System.Windows.Forms.Button();
            this.btnSelectAllColors = new System.Windows.Forms.Button();
            this.cboLayerIndex = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.dgvColors = new System.Windows.Forms.DataGridView();
            this.colColorDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColorEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnResetColorsFromPattern = new System.Windows.Forms.Button();
            this.picGradient = new System.Windows.Forms.PictureBox();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabParameters.SuspendLayout();
            this.pnlEditParamConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameterConfigs)).BeginInit();
            this.tabColors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picGradient)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabParameters);
            this.tabControl1.Controls.Add(this.tabColors);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(665, 356);
            this.tabControl1.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.btnSetAllFlags);
            this.tabGeneral.Controls.Add(this.cboSetAllFlags);
            this.tabGeneral.Controls.Add(this.btnApply);
            this.tabGeneral.Controls.Add(this.label5);
            this.tabGeneral.Controls.Add(this.cboImprovOnParameters);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.cboImprovOnPetals);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Controls.Add(this.cboImprovOnShapes);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.cboImprovOnColors);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Controls.Add(this.cboImprovOnOutlineTypes);
            this.tabGeneral.Controls.Add(this.chkPatternEnabled);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Size = new System.Drawing.Size(657, 330);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // btnSetAllFlags
            // 
            this.btnSetAllFlags.Location = new System.Drawing.Point(14, 42);
            this.btnSetAllFlags.Margin = new System.Windows.Forms.Padding(2);
            this.btnSetAllFlags.Name = "btnSetAllFlags";
            this.btnSetAllFlags.Size = new System.Drawing.Size(86, 21);
            this.btnSetAllFlags.TabIndex = 35;
            this.btnSetAllFlags.Text = "Set all flags to:";
            this.btnSetAllFlags.UseVisualStyleBackColor = true;
            this.btnSetAllFlags.Click += new System.EventHandler(this.btnSetAllFlags_Click);
            // 
            // cboSetAllFlags
            // 
            this.cboSetAllFlags.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSetAllFlags.FormattingEnabled = true;
            this.cboSetAllFlags.Location = new System.Drawing.Point(104, 44);
            this.cboSetAllFlags.Margin = new System.Windows.Forms.Padding(2);
            this.cboSetAllFlags.Name = "cboSetAllFlags";
            this.cboSetAllFlags.Size = new System.Drawing.Size(77, 21);
            this.cboSetAllFlags.TabIndex = 34;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(586, 11);
            this.btnApply.Margin = new System.Windows.Forms.Padding(2);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(56, 21);
            this.btnApply.TabIndex = 33;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 130);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(126, 13);
            this.label5.TabIndex = 32;
            this.label5.Text = "Improvise on Parameters:";
            // 
            // cboImprovOnParameters
            // 
            this.cboImprovOnParameters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovOnParameters.FormattingEnabled = true;
            this.cboImprovOnParameters.Location = new System.Drawing.Point(158, 128);
            this.cboImprovOnParameters.Margin = new System.Windows.Forms.Padding(2);
            this.cboImprovOnParameters.Name = "cboImprovOnParameters";
            this.cboImprovOnParameters.Size = new System.Drawing.Size(77, 21);
            this.cboImprovOnParameters.TabIndex = 31;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(250, 106);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "Improvise on Petals:";
            // 
            // cboImprovOnPetals
            // 
            this.cboImprovOnPetals.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovOnPetals.FormattingEnabled = true;
            this.cboImprovOnPetals.Location = new System.Drawing.Point(359, 103);
            this.cboImprovOnPetals.Margin = new System.Windows.Forms.Padding(2);
            this.cboImprovOnPetals.Name = "cboImprovOnPetals";
            this.cboImprovOnPetals.Size = new System.Drawing.Size(77, 21);
            this.cboImprovOnPetals.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 106);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Improvise on Shapes:";
            // 
            // cboImprovOnShapes
            // 
            this.cboImprovOnShapes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovOnShapes.FormattingEnabled = true;
            this.cboImprovOnShapes.Location = new System.Drawing.Point(158, 103);
            this.cboImprovOnShapes.Margin = new System.Windows.Forms.Padding(2);
            this.cboImprovOnShapes.Name = "cboImprovOnShapes";
            this.cboImprovOnShapes.Size = new System.Drawing.Size(77, 21);
            this.cboImprovOnShapes.TabIndex = 27;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(250, 81);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Improvise on Colors:";
            // 
            // cboImprovOnColors
            // 
            this.cboImprovOnColors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovOnColors.FormattingEnabled = true;
            this.cboImprovOnColors.Location = new System.Drawing.Point(359, 79);
            this.cboImprovOnColors.Margin = new System.Windows.Forms.Padding(2);
            this.cboImprovOnColors.Name = "cboImprovOnColors";
            this.cboImprovOnColors.Size = new System.Drawing.Size(77, 21);
            this.cboImprovOnColors.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 81);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Improvise on Outline Types:";
            // 
            // cboImprovOnOutlineTypes
            // 
            this.cboImprovOnOutlineTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImprovOnOutlineTypes.FormattingEnabled = true;
            this.cboImprovOnOutlineTypes.Location = new System.Drawing.Point(158, 79);
            this.cboImprovOnOutlineTypes.Margin = new System.Windows.Forms.Padding(2);
            this.cboImprovOnOutlineTypes.Name = "cboImprovOnOutlineTypes";
            this.cboImprovOnOutlineTypes.Size = new System.Drawing.Size(77, 21);
            this.cboImprovOnOutlineTypes.TabIndex = 23;
            // 
            // chkPatternEnabled
            // 
            this.chkPatternEnabled.AutoSize = true;
            this.chkPatternEnabled.Location = new System.Drawing.Point(14, 15);
            this.chkPatternEnabled.Name = "chkPatternEnabled";
            this.chkPatternEnabled.Size = new System.Drawing.Size(65, 17);
            this.chkPatternEnabled.TabIndex = 22;
            this.chkPatternEnabled.Text = "Enabled";
            this.chkPatternEnabled.UseVisualStyleBackColor = true;
            // 
            // tabParameters
            // 
            this.tabParameters.Controls.Add(this.btnResetFromPattern);
            this.tabParameters.Controls.Add(this.pnlEditParamConfig);
            this.tabParameters.Controls.Add(this.label6);
            this.tabParameters.Controls.Add(this.cboParameterType);
            this.tabParameters.Controls.Add(this.dgvParameterConfigs);
            this.tabParameters.Location = new System.Drawing.Point(4, 22);
            this.tabParameters.Margin = new System.Windows.Forms.Padding(2);
            this.tabParameters.Name = "tabParameters";
            this.tabParameters.Padding = new System.Windows.Forms.Padding(2);
            this.tabParameters.Size = new System.Drawing.Size(657, 330);
            this.tabParameters.TabIndex = 1;
            this.tabParameters.Text = "Parameters";
            this.tabParameters.UseVisualStyleBackColor = true;
            // 
            // btnResetFromPattern
            // 
            this.btnResetFromPattern.Location = new System.Drawing.Point(542, 11);
            this.btnResetFromPattern.Margin = new System.Windows.Forms.Padding(2);
            this.btnResetFromPattern.Name = "btnResetFromPattern";
            this.btnResetFromPattern.Size = new System.Drawing.Size(108, 21);
            this.btnResetFromPattern.TabIndex = 37;
            this.btnResetFromPattern.Text = "Reset from Pattern";
            this.btnResetFromPattern.UseVisualStyleBackColor = true;
            this.btnResetFromPattern.Click += new System.EventHandler(this.btnResetFromPattern_Click);
            // 
            // pnlEditParamConfig
            // 
            this.pnlEditParamConfig.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlEditParamConfig.Controls.Add(this.txtParameterValue);
            this.pnlEditParamConfig.Controls.Add(this.label11);
            this.pnlEditParamConfig.Controls.Add(this.btnApplyParameter);
            this.pnlEditParamConfig.Controls.Add(this.cboDecimalPlaces);
            this.pnlEditParamConfig.Controls.Add(this.label10);
            this.pnlEditParamConfig.Controls.Add(this.txtImprovStrength);
            this.pnlEditParamConfig.Controls.Add(this.label9);
            this.pnlEditParamConfig.Controls.Add(this.txtMaxValue);
            this.pnlEditParamConfig.Controls.Add(this.label8);
            this.pnlEditParamConfig.Controls.Add(this.txtMinValue);
            this.pnlEditParamConfig.Controls.Add(this.label7);
            this.pnlEditParamConfig.Location = new System.Drawing.Point(450, 43);
            this.pnlEditParamConfig.Margin = new System.Windows.Forms.Padding(2);
            this.pnlEditParamConfig.Name = "pnlEditParamConfig";
            this.pnlEditParamConfig.Size = new System.Drawing.Size(204, 268);
            this.pnlEditParamConfig.TabIndex = 3;
            this.pnlEditParamConfig.Visible = false;
            // 
            // txtParameterValue
            // 
            this.txtParameterValue.Location = new System.Drawing.Point(76, 17);
            this.txtParameterValue.Margin = new System.Windows.Forms.Padding(2);
            this.txtParameterValue.Name = "txtParameterValue";
            this.txtParameterValue.ReadOnly = true;
            this.txtParameterValue.Size = new System.Drawing.Size(76, 20);
            this.txtParameterValue.TabIndex = 36;
            this.txtParameterValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 20);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(37, 13);
            this.label11.TabIndex = 35;
            this.label11.Text = "Value:";
            // 
            // btnApplyParameter
            // 
            this.btnApplyParameter.Location = new System.Drawing.Point(18, 179);
            this.btnApplyParameter.Margin = new System.Windows.Forms.Padding(2);
            this.btnApplyParameter.Name = "btnApplyParameter";
            this.btnApplyParameter.Size = new System.Drawing.Size(56, 21);
            this.btnApplyParameter.TabIndex = 34;
            this.btnApplyParameter.Text = "Apply";
            this.btnApplyParameter.UseVisualStyleBackColor = true;
            this.btnApplyParameter.Click += new System.EventHandler(this.btnApplyParameter_Click);
            // 
            // cboDecimalPlaces
            // 
            this.cboDecimalPlaces.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDecimalPlaces.FormattingEnabled = true;
            this.cboDecimalPlaces.Location = new System.Drawing.Point(101, 144);
            this.cboDecimalPlaces.Margin = new System.Windows.Forms.Padding(2);
            this.cboDecimalPlaces.Name = "cboDecimalPlaces";
            this.cboDecimalPlaces.Size = new System.Drawing.Size(50, 21);
            this.cboDecimalPlaces.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 146);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Decimal Places:";
            // 
            // txtImprovStrength
            // 
            this.txtImprovStrength.Location = new System.Drawing.Point(147, 115);
            this.txtImprovStrength.Margin = new System.Windows.Forms.Padding(2);
            this.txtImprovStrength.Name = "txtImprovStrength";
            this.txtImprovStrength.Size = new System.Drawing.Size(44, 20);
            this.txtImprovStrength.TabIndex = 5;
            this.txtImprovStrength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 118);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(126, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Improvisation Strength %:";
            // 
            // txtMaxValue
            // 
            this.txtMaxValue.Location = new System.Drawing.Point(76, 84);
            this.txtMaxValue.Margin = new System.Windows.Forms.Padding(2);
            this.txtMaxValue.Name = "txtMaxValue";
            this.txtMaxValue.Size = new System.Drawing.Size(76, 20);
            this.txtMaxValue.TabIndex = 3;
            this.txtMaxValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 86);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Max Value:";
            // 
            // txtMinValue
            // 
            this.txtMinValue.Location = new System.Drawing.Point(76, 53);
            this.txtMinValue.Margin = new System.Windows.Forms.Padding(2);
            this.txtMinValue.Name = "txtMinValue";
            this.txtMinValue.Size = new System.Drawing.Size(76, 20);
            this.txtMinValue.TabIndex = 1;
            this.txtMinValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 55);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Min Value:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Type of Parameters:";
            // 
            // cboParameterType
            // 
            this.cboParameterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParameterType.FormattingEnabled = true;
            this.cboParameterType.Location = new System.Drawing.Point(120, 16);
            this.cboParameterType.Margin = new System.Windows.Forms.Padding(2);
            this.cboParameterType.Name = "cboParameterType";
            this.cboParameterType.Size = new System.Drawing.Size(99, 21);
            this.cboParameterType.TabIndex = 1;
            this.cboParameterType.SelectedIndexChanged += new System.EventHandler(this.cboParameterType_SelectedIndexChanged);
            // 
            // dgvParameterConfigs
            // 
            this.dgvParameterConfigs.AllowUserToAddRows = false;
            this.dgvParameterConfigs.AllowUserToDeleteRows = false;
            this.dgvParameterConfigs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParameterConfigs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colEnabled,
            this.colParameterName,
            this.colFormulaName,
            this.colEdit});
            this.dgvParameterConfigs.Location = new System.Drawing.Point(15, 43);
            this.dgvParameterConfigs.Margin = new System.Windows.Forms.Padding(2);
            this.dgvParameterConfigs.Name = "dgvParameterConfigs";
            this.dgvParameterConfigs.RowTemplate.Height = 24;
            this.dgvParameterConfigs.Size = new System.Drawing.Size(424, 267);
            this.dgvParameterConfigs.TabIndex = 0;
            this.dgvParameterConfigs.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvParameterConfigs_CellContentClick);
            this.dgvParameterConfigs.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvParameterConfigs_CellMouseUp);
            // 
            // colEnabled
            // 
            this.colEnabled.DataPropertyName = "Enabled";
            this.colEnabled.HeaderText = "Enabled";
            this.colEnabled.Name = "colEnabled";
            this.colEnabled.Width = 70;
            // 
            // colParameterName
            // 
            this.colParameterName.DataPropertyName = "ParameterName";
            this.colParameterName.HeaderText = "Name";
            this.colParameterName.Name = "colParameterName";
            this.colParameterName.ReadOnly = true;
            // 
            // colFormulaName
            // 
            this.colFormulaName.DataPropertyName = "FormulaName";
            this.colFormulaName.HeaderText = "Formula";
            this.colFormulaName.Name = "colFormulaName";
            this.colFormulaName.ReadOnly = true;
            // 
            // colEdit
            // 
            this.colEdit.HeaderText = "";
            this.colEdit.Name = "colEdit";
            this.colEdit.ReadOnly = true;
            this.colEdit.Text = "Edit";
            this.colEdit.UseColumnTextForButtonValue = true;
            this.colEdit.Width = 60;
            // 
            // tabColors
            // 
            this.tabColors.Controls.Add(this.picGradient);
            this.tabColors.Controls.Add(this.btnApplyColors);
            this.tabColors.Controls.Add(this.btnClearAllColors);
            this.tabColors.Controls.Add(this.btnSelectAllColors);
            this.tabColors.Controls.Add(this.cboLayerIndex);
            this.tabColors.Controls.Add(this.label12);
            this.tabColors.Controls.Add(this.dgvColors);
            this.tabColors.Controls.Add(this.btnResetColorsFromPattern);
            this.tabColors.Location = new System.Drawing.Point(4, 22);
            this.tabColors.Name = "tabColors";
            this.tabColors.Padding = new System.Windows.Forms.Padding(3);
            this.tabColors.Size = new System.Drawing.Size(657, 330);
            this.tabColors.TabIndex = 2;
            this.tabColors.Text = "Colors";
            this.tabColors.UseVisualStyleBackColor = true;
            this.tabColors.Enter += new System.EventHandler(this.tabColors_Enter);
            // 
            // btnApplyColors
            // 
            this.btnApplyColors.Location = new System.Drawing.Point(489, 5);
            this.btnApplyColors.Margin = new System.Windows.Forms.Padding(2);
            this.btnApplyColors.Name = "btnApplyColors";
            this.btnApplyColors.Size = new System.Drawing.Size(71, 21);
            this.btnApplyColors.TabIndex = 44;
            this.btnApplyColors.Text = "Apply";
            this.btnApplyColors.UseVisualStyleBackColor = true;
            this.btnApplyColors.Click += new System.EventHandler(this.btnApplyColors_Click);
            // 
            // btnClearAllColors
            // 
            this.btnClearAllColors.Location = new System.Drawing.Point(489, 119);
            this.btnClearAllColors.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearAllColors.Name = "btnClearAllColors";
            this.btnClearAllColors.Size = new System.Drawing.Size(71, 21);
            this.btnClearAllColors.TabIndex = 43;
            this.btnClearAllColors.Text = "Clear All";
            this.btnClearAllColors.UseVisualStyleBackColor = true;
            this.btnClearAllColors.Click += new System.EventHandler(this.btnClearAllColors_Click);
            // 
            // btnSelectAllColors
            // 
            this.btnSelectAllColors.Location = new System.Drawing.Point(489, 85);
            this.btnSelectAllColors.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelectAllColors.Name = "btnSelectAllColors";
            this.btnSelectAllColors.Size = new System.Drawing.Size(71, 21);
            this.btnSelectAllColors.TabIndex = 42;
            this.btnSelectAllColors.Text = "Select All";
            this.btnSelectAllColors.UseVisualStyleBackColor = true;
            this.btnSelectAllColors.Click += new System.EventHandler(this.btnSelectAllColors_Click);
            // 
            // cboLayerIndex
            // 
            this.cboLayerIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLayerIndex.FormattingEnabled = true;
            this.cboLayerIndex.Location = new System.Drawing.Point(61, 6);
            this.cboLayerIndex.Name = "cboLayerIndex";
            this.cboLayerIndex.Size = new System.Drawing.Size(83, 21);
            this.cboLayerIndex.TabIndex = 41;
            this.cboLayerIndex.SelectedIndexChanged += new System.EventHandler(this.cboLayerIndex_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(19, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(36, 13);
            this.label12.TabIndex = 40;
            this.label12.Text = "Layer:";
            // 
            // dgvColors
            // 
            this.dgvColors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colColorDescription,
            this.colColor,
            this.colColorEnabled});
            this.dgvColors.Location = new System.Drawing.Point(22, 85);
            this.dgvColors.Name = "dgvColors";
            this.dgvColors.Size = new System.Drawing.Size(447, 225);
            this.dgvColors.TabIndex = 39;
            // 
            // colColorDescription
            // 
            this.colColorDescription.DataPropertyName = "Description";
            this.colColorDescription.HeaderText = "Description";
            this.colColorDescription.Name = "colColorDescription";
            this.colColorDescription.ReadOnly = true;
            this.colColorDescription.Width = 200;
            // 
            // colColor
            // 
            this.colColor.HeaderText = "Color";
            this.colColor.Name = "colColor";
            this.colColor.ReadOnly = true;
            this.colColor.Width = 60;
            // 
            // colColorEnabled
            // 
            this.colColorEnabled.DataPropertyName = "Enabled";
            this.colColorEnabled.HeaderText = "Enabled";
            this.colColorEnabled.Name = "colColorEnabled";
            this.colColorEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colColorEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colColorEnabled.Width = 60;
            // 
            // btnResetColorsFromPattern
            // 
            this.btnResetColorsFromPattern.Location = new System.Drawing.Point(581, 5);
            this.btnResetColorsFromPattern.Margin = new System.Windows.Forms.Padding(2);
            this.btnResetColorsFromPattern.Name = "btnResetColorsFromPattern";
            this.btnResetColorsFromPattern.Size = new System.Drawing.Size(71, 21);
            this.btnResetColorsFromPattern.TabIndex = 38;
            this.btnResetColorsFromPattern.Text = "Reset";
            this.btnResetColorsFromPattern.UseVisualStyleBackColor = true;
            this.btnResetColorsFromPattern.Click += new System.EventHandler(this.btnResetColorsFromPattern_Click);
            // 
            // picGradient
            // 
            this.picGradient.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picGradient.Location = new System.Drawing.Point(22, 33);
            this.picGradient.Name = "picGradient";
            this.picGradient.Size = new System.Drawing.Size(445, 46);
            this.picGradient.TabIndex = 45;
            this.picGradient.TabStop = false;
            this.picGradient.Paint += new System.Windows.Forms.PaintEventHandler(this.picGradient_Paint);
            // 
            // PatternImprovConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 382);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PatternImprovConfigForm";
            this.Text = "Pattern Improvise Configuration";
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabParameters.ResumeLayout(false);
            this.tabParameters.PerformLayout();
            this.pnlEditParamConfig.ResumeLayout(false);
            this.pnlEditParamConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameterConfigs)).EndInit();
            this.tabColors.ResumeLayout(false);
            this.tabColors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picGradient)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabParameters;
        private System.Windows.Forms.CheckBox chkPatternEnabled;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboImprovOnOutlineTypes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboImprovOnParameters;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboImprovOnPetals;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboImprovOnShapes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboImprovOnColors;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnSetAllFlags;
        private System.Windows.Forms.ComboBox cboSetAllFlags;
        private System.Windows.Forms.DataGridView dgvParameterConfigs;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFormulaName;
        private System.Windows.Forms.DataGridViewButtonColumn colEdit;
        private System.Windows.Forms.ComboBox cboParameterType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pnlEditParamConfig;
        private System.Windows.Forms.TextBox txtMinValue;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtMaxValue;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtImprovStrength;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cboDecimalPlaces;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnApplyParameter;
        private System.Windows.Forms.TextBox txtParameterValue;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnResetFromPattern;
        private System.Windows.Forms.TabPage tabColors;
        private System.Windows.Forms.Button btnResetColorsFromPattern;
        private System.Windows.Forms.DataGridView dgvColors;
        private System.Windows.Forms.ComboBox cboLayerIndex;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnClearAllColors;
        private System.Windows.Forms.Button btnSelectAllColors;
        private System.Windows.Forms.Button btnApplyColors;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColorDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColor;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colColorEnabled;
        private System.Windows.Forms.PictureBox picGradient;
    }
}