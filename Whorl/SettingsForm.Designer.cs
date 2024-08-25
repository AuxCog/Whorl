namespace Whorl
{
    partial class SettingsForm
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
            this.SettingsTabControl = new System.Windows.Forms.TabControl();
            this.GeneralTab = new System.Windows.Forms.TabPage();
            this.txtThumbnailQuality = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.chkNewPolygonVersion = new System.Windows.Forms.CheckBox();
            this.cboDraftSize = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkOptimizeExpressions = new System.Windows.Forms.CheckBox();
            this.txtMaxLoopCount = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chkExactOutlines = new System.Windows.Forms.CheckBox();
            this.txtBufferSize = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.chkSaveDesignThumbnails = new System.Windows.Forms.CheckBox();
            this.txtThumbnailSize = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtQualitySize = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnBrowseFilesFolder = new System.Windows.Forms.Button();
            this.txtFilesFolder = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.AnimationsTab = new System.Windows.Forms.TabPage();
            this.txtSlideInterval = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.chkImproviseTextures = new System.Windows.Forms.CheckBox();
            this.txtImprovDamping = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkImproviseParameters = new System.Windows.Forms.CheckBox();
            this.txtAnimationRate = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtRevolveRate = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSpinRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkImproviseBackground = new System.Windows.Forms.CheckBox();
            this.chkImprovisePetals = new System.Windows.Forms.CheckBox();
            this.chkImproviseShapes = new System.Windows.Forms.CheckBox();
            this.chkImproviseColors = new System.Windows.Forms.CheckBox();
            this.chkImproviseOnOutlineType = new System.Windows.Forms.CheckBox();
            this.txtRecomputeInterval = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtImprovisationLevel = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ChkCacheDesignSlides = new System.Windows.Forms.CheckBox();
            this.SettingsTabControl.SuspendLayout();
            this.GeneralTab.SuspendLayout();
            this.AnimationsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingsTabControl
            // 
            this.SettingsTabControl.Controls.Add(this.GeneralTab);
            this.SettingsTabControl.Controls.Add(this.AnimationsTab);
            this.SettingsTabControl.Location = new System.Drawing.Point(12, 12);
            this.SettingsTabControl.Name = "SettingsTabControl";
            this.SettingsTabControl.SelectedIndex = 0;
            this.SettingsTabControl.Size = new System.Drawing.Size(470, 236);
            this.SettingsTabControl.TabIndex = 0;
            // 
            // GeneralTab
            // 
            this.GeneralTab.Controls.Add(this.ChkCacheDesignSlides);
            this.GeneralTab.Controls.Add(this.txtThumbnailQuality);
            this.GeneralTab.Controls.Add(this.label13);
            this.GeneralTab.Controls.Add(this.chkNewPolygonVersion);
            this.GeneralTab.Controls.Add(this.cboDraftSize);
            this.GeneralTab.Controls.Add(this.label12);
            this.GeneralTab.Controls.Add(this.chkOptimizeExpressions);
            this.GeneralTab.Controls.Add(this.txtMaxLoopCount);
            this.GeneralTab.Controls.Add(this.label11);
            this.GeneralTab.Controls.Add(this.chkExactOutlines);
            this.GeneralTab.Controls.Add(this.txtBufferSize);
            this.GeneralTab.Controls.Add(this.label10);
            this.GeneralTab.Controls.Add(this.chkSaveDesignThumbnails);
            this.GeneralTab.Controls.Add(this.txtThumbnailSize);
            this.GeneralTab.Controls.Add(this.label9);
            this.GeneralTab.Controls.Add(this.txtQualitySize);
            this.GeneralTab.Controls.Add(this.label8);
            this.GeneralTab.Controls.Add(this.btnBrowseFilesFolder);
            this.GeneralTab.Controls.Add(this.txtFilesFolder);
            this.GeneralTab.Controls.Add(this.label6);
            this.GeneralTab.Location = new System.Drawing.Point(4, 22);
            this.GeneralTab.Name = "GeneralTab";
            this.GeneralTab.Padding = new System.Windows.Forms.Padding(3);
            this.GeneralTab.Size = new System.Drawing.Size(462, 210);
            this.GeneralTab.TabIndex = 0;
            this.GeneralTab.Text = "General";
            this.GeneralTab.UseVisualStyleBackColor = true;
            // 
            // txtThumbnailQuality
            // 
            this.txtThumbnailQuality.Location = new System.Drawing.Point(258, 123);
            this.txtThumbnailQuality.Name = "txtThumbnailQuality";
            this.txtThumbnailQuality.Size = new System.Drawing.Size(70, 20);
            this.txtThumbnailQuality.TabIndex = 26;
            this.txtThumbnailQuality.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(147, 126);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(105, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "Thumbnail Quality %:";
            // 
            // chkNewPolygonVersion
            // 
            this.chkNewPolygonVersion.AutoSize = true;
            this.chkNewPolygonVersion.Location = new System.Drawing.Point(10, 179);
            this.chkNewPolygonVersion.Name = "chkNewPolygonVersion";
            this.chkNewPolygonVersion.Size = new System.Drawing.Size(149, 17);
            this.chkNewPolygonVersion.TabIndex = 24;
            this.chkNewPolygonVersion.Text = "Use New Polygon Version";
            this.chkNewPolygonVersion.UseVisualStyleBackColor = true;
            // 
            // cboDraftSize
            // 
            this.cboDraftSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDraftSize.FormattingEnabled = true;
            this.cboDraftSize.Location = new System.Drawing.Point(356, 44);
            this.cboDraftSize.Name = "cboDraftSize";
            this.cboDraftSize.Size = new System.Drawing.Size(61, 21);
            this.cboDraftSize.TabIndex = 23;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(294, 46);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 22;
            this.label12.Text = "Draft Size:";
            // 
            // chkOptimizeExpressions
            // 
            this.chkOptimizeExpressions.AutoSize = true;
            this.chkOptimizeExpressions.Location = new System.Drawing.Point(258, 149);
            this.chkOptimizeExpressions.Name = "chkOptimizeExpressions";
            this.chkOptimizeExpressions.Size = new System.Drawing.Size(125, 17);
            this.chkOptimizeExpressions.TabIndex = 21;
            this.chkOptimizeExpressions.Text = "Optimize Expressions";
            this.chkOptimizeExpressions.UseVisualStyleBackColor = true;
            // 
            // txtMaxLoopCount
            // 
            this.txtMaxLoopCount.Location = new System.Drawing.Point(101, 147);
            this.txtMaxLoopCount.Name = "txtMaxLoopCount";
            this.txtMaxLoopCount.Size = new System.Drawing.Size(96, 20);
            this.txtMaxLoopCount.TabIndex = 20;
            this.txtMaxLoopCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 150);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(88, 13);
            this.label11.TabIndex = 19;
            this.label11.Text = "Max Loop Count:";
            // 
            // chkExactOutlines
            // 
            this.chkExactOutlines.AutoSize = true;
            this.chkExactOutlines.Location = new System.Drawing.Point(10, 70);
            this.chkExactOutlines.Name = "chkExactOutlines";
            this.chkExactOutlines.Size = new System.Drawing.Size(177, 17);
            this.chkExactOutlines.TabIndex = 18;
            this.chkExactOutlines.Text = "Draw Selection Patterns Exactly";
            this.chkExactOutlines.UseVisualStyleBackColor = true;
            // 
            // txtBufferSize
            // 
            this.txtBufferSize.Location = new System.Drawing.Point(219, 43);
            this.txtBufferSize.Name = "txtBufferSize";
            this.txtBufferSize.Size = new System.Drawing.Size(59, 20);
            this.txtBufferSize.TabIndex = 17;
            this.txtBufferSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(159, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(61, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Buffer Size:";
            // 
            // chkSaveDesignThumbnails
            // 
            this.chkSaveDesignThumbnails.AutoSize = true;
            this.chkSaveDesignThumbnails.Location = new System.Drawing.Point(10, 99);
            this.chkSaveDesignThumbnails.Name = "chkSaveDesignThumbnails";
            this.chkSaveDesignThumbnails.Size = new System.Drawing.Size(144, 17);
            this.chkSaveDesignThumbnails.TabIndex = 15;
            this.chkSaveDesignThumbnails.Text = "Save Design Thumbnails";
            this.chkSaveDesignThumbnails.UseVisualStyleBackColor = true;
            // 
            // txtThumbnailSize
            // 
            this.txtThumbnailSize.Location = new System.Drawing.Point(258, 97);
            this.txtThumbnailSize.Name = "txtThumbnailSize";
            this.txtThumbnailSize.Size = new System.Drawing.Size(70, 20);
            this.txtThumbnailSize.TabIndex = 14;
            this.txtThumbnailSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(159, 100);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(93, 13);
            this.label9.TabIndex = 13;
            this.label9.Text = "Thumbnail Height:";
            // 
            // txtQualitySize
            // 
            this.txtQualitySize.Location = new System.Drawing.Point(86, 44);
            this.txtQualitySize.Name = "txtQualitySize";
            this.txtQualitySize.Size = new System.Drawing.Size(68, 20);
            this.txtQualitySize.TabIndex = 12;
            this.txtQualitySize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 46);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Quality Size:";
            // 
            // btnBrowseFilesFolder
            // 
            this.btnBrowseFilesFolder.Location = new System.Drawing.Point(358, 12);
            this.btnBrowseFilesFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnBrowseFilesFolder.Name = "btnBrowseFilesFolder";
            this.btnBrowseFilesFolder.Size = new System.Drawing.Size(24, 19);
            this.btnBrowseFilesFolder.TabIndex = 2;
            this.btnBrowseFilesFolder.Text = "...";
            this.btnBrowseFilesFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFilesFolder.Click += new System.EventHandler(this.btnBrowseFilesFolder_Click);
            // 
            // txtFilesFolder
            // 
            this.txtFilesFolder.Location = new System.Drawing.Point(86, 13);
            this.txtFilesFolder.Margin = new System.Windows.Forms.Padding(2);
            this.txtFilesFolder.Name = "txtFilesFolder";
            this.txtFilesFolder.Size = new System.Drawing.Size(269, 20);
            this.txtFilesFolder.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 15);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Files Folder:";
            // 
            // AnimationsTab
            // 
            this.AnimationsTab.Controls.Add(this.txtSlideInterval);
            this.AnimationsTab.Controls.Add(this.label14);
            this.AnimationsTab.Controls.Add(this.chkImproviseTextures);
            this.AnimationsTab.Controls.Add(this.txtImprovDamping);
            this.AnimationsTab.Controls.Add(this.label7);
            this.AnimationsTab.Controls.Add(this.chkImproviseParameters);
            this.AnimationsTab.Controls.Add(this.txtAnimationRate);
            this.AnimationsTab.Controls.Add(this.label5);
            this.AnimationsTab.Controls.Add(this.txtRevolveRate);
            this.AnimationsTab.Controls.Add(this.label4);
            this.AnimationsTab.Controls.Add(this.txtSpinRate);
            this.AnimationsTab.Controls.Add(this.label3);
            this.AnimationsTab.Controls.Add(this.chkImproviseBackground);
            this.AnimationsTab.Controls.Add(this.chkImprovisePetals);
            this.AnimationsTab.Controls.Add(this.chkImproviseShapes);
            this.AnimationsTab.Controls.Add(this.chkImproviseColors);
            this.AnimationsTab.Controls.Add(this.chkImproviseOnOutlineType);
            this.AnimationsTab.Controls.Add(this.txtRecomputeInterval);
            this.AnimationsTab.Controls.Add(this.label2);
            this.AnimationsTab.Controls.Add(this.txtImprovisationLevel);
            this.AnimationsTab.Controls.Add(this.label1);
            this.AnimationsTab.Location = new System.Drawing.Point(4, 22);
            this.AnimationsTab.Name = "AnimationsTab";
            this.AnimationsTab.Padding = new System.Windows.Forms.Padding(3);
            this.AnimationsTab.Size = new System.Drawing.Size(462, 210);
            this.AnimationsTab.TabIndex = 1;
            this.AnimationsTab.Text = "Animations";
            this.AnimationsTab.UseVisualStyleBackColor = true;
            // 
            // txtSlideInterval
            // 
            this.txtSlideInterval.Location = new System.Drawing.Point(274, 18);
            this.txtSlideInterval.Name = "txtSlideInterval";
            this.txtSlideInterval.Size = new System.Drawing.Size(70, 20);
            this.txtSlideInterval.TabIndex = 20;
            this.txtSlideInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(186, 21);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "Slide Delay:";
            // 
            // chkImproviseTextures
            // 
            this.chkImproviseTextures.AutoSize = true;
            this.chkImproviseTextures.Enabled = false;
            this.chkImproviseTextures.Location = new System.Drawing.Point(331, 132);
            this.chkImproviseTextures.Name = "chkImproviseTextures";
            this.chkImproviseTextures.Size = new System.Drawing.Size(130, 17);
            this.chkImproviseTextures.TabIndex = 18;
            this.chkImproviseTextures.Text = "Improvise on Textures";
            this.chkImproviseTextures.UseVisualStyleBackColor = true;
            // 
            // txtImprovDamping
            // 
            this.txtImprovDamping.Location = new System.Drawing.Point(308, 76);
            this.txtImprovDamping.Name = "txtImprovDamping";
            this.txtImprovDamping.Size = new System.Drawing.Size(52, 20);
            this.txtImprovDamping.TabIndex = 17;
            this.txtImprovDamping.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(186, 78);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Improvisation Damping:";
            // 
            // chkImproviseParameters
            // 
            this.chkImproviseParameters.AutoSize = true;
            this.chkImproviseParameters.Location = new System.Drawing.Point(176, 178);
            this.chkImproviseParameters.Name = "chkImproviseParameters";
            this.chkImproviseParameters.Size = new System.Drawing.Size(142, 17);
            this.chkImproviseParameters.TabIndex = 15;
            this.chkImproviseParameters.Text = "Improvise on Parameters";
            this.chkImproviseParameters.UseVisualStyleBackColor = true;
            // 
            // txtAnimationRate
            // 
            this.txtAnimationRate.Location = new System.Drawing.Point(94, 18);
            this.txtAnimationRate.Name = "txtAnimationRate";
            this.txtAnimationRate.Size = new System.Drawing.Size(70, 20);
            this.txtAnimationRate.TabIndex = 14;
            this.txtAnimationRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Animation Rate:";
            // 
            // txtRevolveRate
            // 
            this.txtRevolveRate.Location = new System.Drawing.Point(270, 44);
            this.txtRevolveRate.Name = "txtRevolveRate";
            this.txtRevolveRate.Size = new System.Drawing.Size(70, 20);
            this.txtRevolveRate.TabIndex = 12;
            this.txtRevolveRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(186, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Revolve Rate:";
            // 
            // txtSpinRate
            // 
            this.txtSpinRate.Location = new System.Drawing.Point(94, 44);
            this.txtSpinRate.Name = "txtSpinRate";
            this.txtSpinRate.Size = new System.Drawing.Size(70, 20);
            this.txtSpinRate.TabIndex = 10;
            this.txtSpinRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Spin Rate:";
            // 
            // chkImproviseBackground
            // 
            this.chkImproviseBackground.AutoSize = true;
            this.chkImproviseBackground.Location = new System.Drawing.Point(9, 178);
            this.chkImproviseBackground.Name = "chkImproviseBackground";
            this.chkImproviseBackground.Size = new System.Drawing.Size(147, 17);
            this.chkImproviseBackground.TabIndex = 8;
            this.chkImproviseBackground.Text = "Improvise on Background";
            this.chkImproviseBackground.UseVisualStyleBackColor = true;
            // 
            // chkImprovisePetals
            // 
            this.chkImprovisePetals.AutoSize = true;
            this.chkImprovisePetals.Location = new System.Drawing.Point(176, 155);
            this.chkImprovisePetals.Name = "chkImprovisePetals";
            this.chkImprovisePetals.Size = new System.Drawing.Size(118, 17);
            this.chkImprovisePetals.TabIndex = 7;
            this.chkImprovisePetals.Text = "Improvise on Petals";
            this.chkImprovisePetals.UseVisualStyleBackColor = true;
            // 
            // chkImproviseShapes
            // 
            this.chkImproviseShapes.AutoSize = true;
            this.chkImproviseShapes.Location = new System.Drawing.Point(9, 155);
            this.chkImproviseShapes.Name = "chkImproviseShapes";
            this.chkImproviseShapes.Size = new System.Drawing.Size(125, 17);
            this.chkImproviseShapes.TabIndex = 6;
            this.chkImproviseShapes.Text = "Improvise on Shapes";
            this.chkImproviseShapes.UseVisualStyleBackColor = true;
            // 
            // chkImproviseColors
            // 
            this.chkImproviseColors.AutoSize = true;
            this.chkImproviseColors.Location = new System.Drawing.Point(176, 132);
            this.chkImproviseColors.Name = "chkImproviseColors";
            this.chkImproviseColors.Size = new System.Drawing.Size(118, 17);
            this.chkImproviseColors.TabIndex = 5;
            this.chkImproviseColors.Text = "Improvise on Colors";
            this.chkImproviseColors.UseVisualStyleBackColor = true;
            this.chkImproviseColors.CheckedChanged += new System.EventHandler(this.chkImproviseColors_CheckedChanged);
            // 
            // chkImproviseOnOutlineType
            // 
            this.chkImproviseOnOutlineType.AutoSize = true;
            this.chkImproviseOnOutlineType.Location = new System.Drawing.Point(9, 132);
            this.chkImproviseOnOutlineType.Name = "chkImproviseOnOutlineType";
            this.chkImproviseOnOutlineType.Size = new System.Drawing.Size(154, 17);
            this.chkImproviseOnOutlineType.TabIndex = 4;
            this.chkImproviseOnOutlineType.Text = "Improvise on Outline Types";
            this.chkImproviseOnOutlineType.UseVisualStyleBackColor = true;
            // 
            // txtRecomputeInterval
            // 
            this.txtRecomputeInterval.Location = new System.Drawing.Point(161, 101);
            this.txtRecomputeInterval.Name = "txtRecomputeInterval";
            this.txtRecomputeInterval.Size = new System.Drawing.Size(81, 20);
            this.txtRecomputeInterval.TabIndex = 3;
            this.txtRecomputeInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(149, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Recompute Inteval (seconds):";
            // 
            // txtImprovisationLevel
            // 
            this.txtImprovisationLevel.Location = new System.Drawing.Point(112, 76);
            this.txtImprovisationLevel.Name = "txtImprovisationLevel";
            this.txtImprovisationLevel.Size = new System.Drawing.Size(52, 20);
            this.txtImprovisationLevel.TabIndex = 1;
            this.txtImprovisationLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Improvisation Level:";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(326, 252);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(407, 252);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ChkCacheDesignSlides
            // 
            this.ChkCacheDesignSlides.AutoSize = true;
            this.ChkCacheDesignSlides.Checked = true;
            this.ChkCacheDesignSlides.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkCacheDesignSlides.Location = new System.Drawing.Point(258, 179);
            this.ChkCacheDesignSlides.Name = "ChkCacheDesignSlides";
            this.ChkCacheDesignSlides.Size = new System.Drawing.Size(124, 17);
            this.ChkCacheDesignSlides.TabIndex = 27;
            this.ChkCacheDesignSlides.Text = "Cache Design Slides";
            this.ChkCacheDesignSlides.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 279);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.SettingsTabControl);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.SettingsTabControl.ResumeLayout(false);
            this.GeneralTab.ResumeLayout(false);
            this.GeneralTab.PerformLayout();
            this.AnimationsTab.ResumeLayout(false);
            this.AnimationsTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl SettingsTabControl;
        private System.Windows.Forms.TabPage GeneralTab;
        private System.Windows.Forms.TabPage AnimationsTab;
        private System.Windows.Forms.TextBox txtRecomputeInterval;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtImprovisationLevel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkImproviseOnOutlineType;
        private System.Windows.Forms.CheckBox chkImproviseShapes;
        private System.Windows.Forms.CheckBox chkImproviseColors;
        private System.Windows.Forms.CheckBox chkImprovisePetals;
        private System.Windows.Forms.CheckBox chkImproviseBackground;
        private System.Windows.Forms.TextBox txtAnimationRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtRevolveRate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSpinRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowseFilesFolder;
        private System.Windows.Forms.TextBox txtFilesFolder;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkImproviseParameters;
        private System.Windows.Forms.TextBox txtImprovDamping;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkImproviseTextures;
        private System.Windows.Forms.TextBox txtQualitySize;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtThumbnailSize;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkSaveDesignThumbnails;
        private System.Windows.Forms.TextBox txtBufferSize;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkExactOutlines;
        private System.Windows.Forms.TextBox txtMaxLoopCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chkOptimizeExpressions;
        private System.Windows.Forms.ComboBox cboDraftSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkNewPolygonVersion;
        private System.Windows.Forms.TextBox txtThumbnailQuality;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtSlideInterval;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox ChkCacheDesignSlides;
    }
}