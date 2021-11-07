using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using ParserEngine;
using System.Drawing.Drawing2D;

namespace Whorl
{
    public partial class PatternForm : Form, IColor
    {
        private enum TabKeys
        {
            tabFill,
            tabOutlines,
            tabTransforms,
            tabRibbon,
            tabRibbonFormula,
            tabSection,
            tabPatternLayers,
            tabTiling,
            tabRendering,
            tabStringPattern
        }

        private TabPage GetTabPage(TabKeys tabKey)
        {
            return tabControl1.TabPages[tabKey.ToString()];
        }

        private void SetTabPageParametersPanel(TabKeys tabKey, Panel pnl)
        {
            TabPage tabPage = GetTabPage(tabKey);
            if (tabPage == null)
                throw new Exception($"Invalid tab key: {tabKey}.");
            tabPage.Tag = pnl;
        }

        private Panel GetTabPageParametersPanel(TabPage tabPage)
        {
            return tabPage.Tag as Panel;
        }

        public PatternForm()
        {
            InitializeComponent();
            try
            {
                for (int i = 1; i <= 4; i++)
                {
                    var itm = new ToolStripMenuItem(i == 1 ? "None" : i.ToString());
                    itm.Tag = i;
                    itm.Click += SetDraftSize;
                    draftSizeToolStripMenuItem.DropDownItems.Add(itm);
                }
                List<string> missingKeys = new List<string>();
                foreach (TabKeys tabKey in Enum.GetValues(typeof(TabKeys)))
                {
                    if (GetTabPage(tabKey) == null)
                        missingKeys.Add(tabKey.ToString());
                }
                if (missingKeys.Any())
                {
                    MessageBox.Show("Missing tab keys: " + string.Join(", ", missingKeys));
                }
                Panel[] parametersPanels = new Panel[]
                {
                    pnlOutlineParameters,
                    pnlRenderingParameters,
                    pnlRibbonFormulaParameters,
                    pnlTransformParameters
                };
                SetTabPageParametersPanel(TabKeys.tabOutlines, pnlOutlineParameters);
                SetTabPageParametersPanel(TabKeys.tabRendering, pnlRenderingParameters);
                SetTabPageParametersPanel(TabKeys.tabRibbonFormula, pnlRibbonFormulaParameters);
                SetTabPageParametersPanel(TabKeys.tabTransforms, pnlTransformParameters);

                parameterDisplaysContainer = new ParameterDisplaysContainer(
                                             parametersPanels, OnParameterChanged, OnParameterActionSelected);
                renderingParamsDisplay = parameterDisplaysContainer.GetParameterDisplays(
                         pnlRenderingParameters, throwException: true).CSharpParameterDisplay;

                var paramDisplays = parameterDisplaysContainer.GetParameterDisplays(pnlTransformParameters, throwException: true);
                paramDisplays.ParameterDisplay.FnEditInfluenceLink = EditTransformInfluenceLink;
                paramDisplays.CSharpParameterDisplay.FnEditInfluenceLink = EditTransformInfluenceLink;

                paramDisplays = parameterDisplaysContainer.GetParameterDisplays(pnlRenderingParameters, throwException: true);
                paramDisplays.ParameterDisplay.FnEditInfluenceLink = EditRenderingInfluenceLink;
                paramDisplays.CSharpParameterDisplay.FnEditInfluenceLink = EditRenderingInfluenceLink;

                colorGradientFormComponent = new ColorGradientFormComponent(picGradient, this);
                colorGradientFormComponent.ShowContextMenu += showGradientContextMenu;
                colorGradientFormComponent.GradientChanged += gradientChanged;
                this.pickGradientColorToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.pickColorToolStripMenuItem_Click);
                this.pickAddedColorToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.addColorToolStripMenuItem_Click);
                this.chooseAddedColorToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.chooseAddedColorToolStripMenuItem_Click);
                this.deleteGradientColorToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.deleteColorToolStripMenuItem_Click);
                this.editGradientTransparencyToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.editTransparencyToolStripMenuItem_Click);
                this.chooseGradientColorToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.chooseColorToolStripMenuItem_Click);
                this.addGradientColorToChoicesToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.addColorToChoicesToolStripMenuItem_Click);
                this.copyGradientToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.copyGradientToolStripMenuItem_Click);
                this.pasteGradientToolStripMenuItem.Click +=
                    new System.EventHandler(colorGradientFormComponent.pasteGradientToolStripMenuItem_Click);
                this.addPaletteToChoicesToolStripMenuItem.Click +=
                    new EventHandler(colorGradientFormComponent.addGradientToChoicesToolStripMenuItem_Click);
                this.selectPaletteToolStripMenuItem.Click +=
                    new EventHandler(colorGradientFormComponent.selectGradientToolStripMenuItem_Click);
                this.dgvBasicOutlines.AutoGenerateColumns = false;
                this.dgvTransforms.AutoGenerateColumns = false;
                //this.dgvTransformParameters.AutoGenerateColumns = false;
                //this.dgvTransformFnParameters.AutoGenerateColumns = false;
                //this.dgvCustomParameters.AutoGenerateColumns = false;
                //this.dgvOutlineFnParameters.AutoGenerateColumns = false;
                this.dgvLayers.AutoGenerateColumns = false;
                this.colorModeContextMenuStrip.Visible = false;
                //String values for BasicOutlineType DataGridView column:
                BasicOutlineType.DataSource = Enum.GetNames(typeof(BasicOutlineTypes));
                cboFillType.DataSource = Enum.GetValues(typeof(FillInfo.FillTypes));
                cboMergeOperation.DataSource = Enum.GetValues(typeof(Pattern.MergeOperations));
                cboDrawingMode.DataSource = Enum.GetValues(typeof(RibbonDrawingModes));
                //cboPathMode.DataSource = Enum.GetValues(typeof(RibbonPathModes));
                cboRenderMode.DataSource = Enum.GetValues(typeof(Pattern.RenderModes));
                cboStainBlendType.DataSource = Enum.GetValues(typeof(ColorBlendTypes));
                cboPathColorMode.DataSource = Enum.GetValues(typeof(FillInfo.PathColorModes));
                cboImageMode.DataSource = Enum.GetValues(typeof(TextureImageModes));
                //FnParamFunctionName.DataSource = ParserEngine.VarFunctionParameter.GetValidMethodNames().ToList();
                //OutlineFnParamColumn.DataSource = ParserEngine.VarFunctionParameter.GetValidMethodNames().ToList();
                cboImageMode.SelectedItem = TextureImageModes.Tile;
                //cboImageWrapMode.DataSource = Enum.GetValues(typeof(WrapMode));
                outlinesDataTable = new DataTable();
                outlinesDataTable.Columns.Add("BasicOutlineType");
                outlinesDataTable.Columns.Add("Petals", typeof(int));
                outlinesDataTable.Columns.Add("Weight", typeof(double));
                outlinesDataTable.Columns.Add("Phase", typeof(double));
                outlinesDataTable.Columns.Add("Pointiness", typeof(double));
                outlinesDataTable.Columns.Add("Enabled", typeof(bool));
                //outlinesDataTable.Columns.Add("AmplitudeFormula", typeof(string));
                //outlinesDataTable.Columns.Add("MaxAmplitudeFormula", typeof(string));
                outlinesDataTable.Columns.Add("BasicOutline", typeof(BasicOutline));
                dgvPatternRecursionInfo.DataSource = patternRecursionInfoBindingSource;
                this.dgvLayers.RowsRemoved +=
                    new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(
                        this.dgvLayers_RowsRemoved);
                //dgvLayers.RowsAdded += dgvLayers_RowsAdded;
                AddControlEventHandlers(this);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool EditInfluenceLink(string parameterKey, Pattern pattern, FormulaSettings formulaSettings)
        {
            if (pattern.InfluencePointInfoList.Count == 0)
            {
                MessageBox.Show("Please add an influence point first, in the main form.");
                return false;
            }
            using (var frm = new frmInfluenceLink())
            {
                frm.Initialize(pattern, formulaSettings, parameterKey);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    PreviewChanges();
                    //lastEditedInfluenceLink = pattern.LastEditedInfluenceLink;
                    return true;
                }
            }
            return false;
        }

        private bool EditTransformInfluenceLink(string parameterKey)
        {
            Pattern pattern = EditedPattern;
            if (pattern == null) 
                return false;
            return EditInfluenceLink(parameterKey, pattern, selectedTransform.TransformSettings);
        }

        private bool EditRenderingInfluenceLink(string parameterKey)
        {
            Pattern pattern = EditedPattern;
            if (pattern == null || pattern.PixelRendering?.FormulaSettings == null)
                return false;
            return EditInfluenceLink(parameterKey, pattern, pattern.PixelRendering.FormulaSettings);
        }

        private void gradientChanged(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = EditedPattern;
                if (pattern?.PixelRendering != null && pattern.PixelRendering.Enabled)
                {
                    pattern.PixelRendering.ColorNodes = colorGradientFormComponent.ColorNodes;
                    if (PreviewPatternGroup != null && PreviewPatternGroup.PatternsList.Count == 1)
                    {
                        Pattern previewPattern = PreviewPatternGroup.PatternsList[0];
                        if (previewPattern.PixelRendering != null)
                        {
                            previewPattern.PixelRendering.ColorNodes = colorGradientFormComponent.ColorNodes;
                            RedrawPreview();
                        }
                    }
                    else
                        PreviewChanges();
                }
                else
                {
                    PathFillInfo fillInfo = GetEditedPathFillInfo();
                    if (fillInfo != null)
                    {
                        fillInfo.SetFromColorNodes(colorGradientFormComponent.ColorNodes);
                        btnBoundaryColor.BackColor = fillInfo.BoundaryColor;
                        btnCenterColor.BackColor = fillInfo.CenterColor;
                        PreviewChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void showGradientContextMenu(object sender, MouseEventArgs e)
        {
            try
            {
                gradientContextMenu.Show(picGradient, e.Location);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
        public PatternTransform EditedTransform { get; private set; }
        private ParameterDisplaysContainer parameterDisplaysContainer { get; }
        private CSharpParameterDisplay renderingParamsDisplay { get; set; }
        private WhorlDesign design { get; set; }
        //private InfluenceLink lastEditedInfluenceLink { get; set; }
        private bool handleControlEvents = false;
        private PatternList editedPatternGroup = null;
        private PatternList previewPatternGroup = null;
        private PathOutline CurrentCartesianPathOutline { get; set; }
        private ColorGradientFormComponent colorGradientFormComponent { get; set; }
        private SelectPatternForm selectPatternForm = null;
        private PatternTransform selectedTransform { get; set; }

        //private static DataTable EmptyParametersDataTable { get; } =
        //    FormulaSettings.CreateParametersDataTable(typeof(double));
        //private static DataTable EmptyFnParametersDataTable { get; } =
        //    FormulaSettings.CreateParametersDataTable(typeof(string));
        private PointF mousePoint = PointF.Empty;
        private FillInfo.PathColorModes _currentColorMode = FillInfo.PathColorModes.Normal;
        private FillInfo.PathColorModes currentColorMode
        {
            get { return _currentColorMode; }
            set
            {
                if (_currentColorMode != value)
                {
                    _currentColorMode = value;
                    if (_currentColorMode != FillInfo.PathColorModes.Normal)
                        picPreview.ContextMenuStrip = colorModeContextMenuStrip;
                    else
                        picPreview.ContextMenuStrip = null;
                }
            }
        }

        public PatternList EditedPatternGroup
        {
            get { return editedPatternGroup; }
            set
            {
                if (editedPatternGroup != null)
                    editedPatternGroup.Dispose();
                editedPatternGroup = value;
                equalizeCentersToolStripMenuItem.Enabled =
                   editedPatternGroup.Patterns.Count() > 1;
            }
        }

        private bool initialized { get; set; }

        private MainForm mainForm { get; set; }

        private PatternList PreviewPatternGroup
        {
            get { return previewPatternGroup; }
            set
            {
                if (previewPatternGroup != null)
                    previewPatternGroup.Dispose();
                previewPatternGroup = value;
            }
        }

        private Pattern EditedPattern
        {
            get
            {
                return GetEditedPattern(EditedPatternGroup);
            }
            set
            {
                SetEditedPattern(EditedPatternGroup, value);
            }
        }

        private Pattern PreviewPattern
        {
            get
            {
                return GetEditedPattern(PreviewPatternGroup);
            }
        }

        private Ribbon pathRibbon
        {
            get
            {
                PathPattern pathPattern = EditedPattern as PathPattern;
                return pathPattern?.PathRibbon;
            }
        }

        private int editedPatternIndex;

        private int EditedPatternIndex
        {
            get { return editedPatternIndex; }
            set
            {
                bool changed = (editedPatternIndex != value);
                editedPatternIndex = value;
                editNextPatternToolStripMenuItem.Enabled =
                    (editedPatternIndex < EditedPatternGroup.Patterns.Count() - 1);
                editPreviousPatternToolStripMenuItem.Enabled = (editedPatternIndex > 0);
                if (editedLayers != null)
                    editedLayers.LayersChanged -= PatternLayerChanged;
                _editedLayers = EditedPattern?.PatternLayers;
                if (editedLayers != null)
                {
                    editedLayers.LayersChanged += PatternLayerChanged;
                    editedLayers.PopulateLayersDataTable();
                    this.dgvLayers.DataSource = editedLayers.LayersDataTable;
                }
                if (changed && initialized)
                {
                    PopulateControls();
                    picPreview.Refresh();
                    //PreviewChanges();
                }
                Pattern editedPattern = EditedPattern;
                if (editedPattern != null)
                    editedPatternModulus = editedPattern.ZVector.GetModulus();
            }
        }

        private PathColorTypes pathColorType;

        public Color TransparencyColor
        {
            get
            {
                return pathColorType == PathColorTypes.Center ?
                       btnCenterColor.BackColor : btnBoundaryColor.BackColor;
            }
            set
            {
                if (pathColorType == PathColorTypes.Boundary || addColorModeColor != null)
                    btnBoundaryColor.BackColor = value;
                else
                    btnCenterColor.BackColor = value;
                if (addColorModeColor != null)
                {
                    //SetColorModeColor(previewChanges: false);
                    return;
                }
                PathFillInfo pathFillInfo = GetEditedPathFillInfo();
                if (pathFillInfo == null)
                    return;
                if (pathColorType == PathColorTypes.Center)
                    pathFillInfo.CenterColor = value;
                else
                    pathFillInfo.BoundaryColor = value;
                if (!EditedPattern.HasSurroundColors())
                    PreviewChanges();
                //SetPatternColor();
            }
        }

        private DataTable outlinesDataTable;
        private double editedPatternModulus;
        PatternLayerList _editedLayers;
        private PatternLayerList editedLayers
        {
            get { return _editedLayers; }
        }
        private PatternLayer _editedLayer;
        private PatternLayer editedLayer
        {
            get { return _editedLayer; }
            set
            {
                _editedLayer = value;
                PopulateLayerControls();
            }
        }

        private IFillInfo fillInfoParent
        {
            get
            {
                if (chkEditLayers.Checked && editedLayer != null)
                    return (IFillInfo)editedLayer;
                else
                {
                    Ribbon pathRibbon = this.pathRibbon;
                    if (pathRibbon != null)
                        return (IFillInfo)pathRibbon;
                    else
                        return (IFillInfo)EditedPattern;
                }
            }
        }

        private Pattern GetEditedPattern(PatternList patternGroup)
        {
            List<Pattern> patterns = patternGroup?.PatternsList;
            return (patterns != null &&
                    EditedPatternIndex >= 0 &&
                    EditedPatternIndex < patterns.Count) ?
                patterns[EditedPatternIndex] : null;
        }

        private void SetEditedPattern(PatternList patternGroup, Pattern pattern)
        {
            List<Pattern> patterns = patternGroup?.PatternsList;
            if (patterns != null &&
                EditedPatternIndex >= 0 &&
                EditedPatternIndex < patterns.Count)
            {
                patterns[EditedPatternIndex] = pattern;
                PopulateControls();
            }
        }

        private void AddControlEventHandlers(Control ctl)
        {
            if ((ctl.Tag as string) == "X")
                return;
            TextBox txt = ctl as TextBox;
            if (txt != null)
                txt.KeyPress += TextBox_KeyPress;
            else
            {
                ComboBox comboBox = ctl as ComboBox;
                if (comboBox != null)
                    comboBox.SelectedIndexChanged += new EventHandler(OnControlValueChanged);
                else
                {
                    CheckBox checkBox = ctl as CheckBox;
                    if (checkBox != null)
                        checkBox.CheckedChanged += OnControlValueChanged;
                }
            }
            foreach (Control subCtl in ctl.Controls)
            {
                AddControlEventHandlers(subCtl);
            }
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == '\r')
                {
                    e.Handled = true;
                    if (handleControlEvents)
                        PreviewChanges(updatePattern: true);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void TxtPreviewControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == '\r')
                {
                    e.Handled = true;
                    if (handleControlEvents)
                        PreviewChanges(updatePattern: false);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private void OnControlValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (handleControlEvents)
                    PreviewChanges(updatePattern: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void Initialize(PatternList patternGroup, SelectPatternForm selPatternForm, 
                               WhorlDesign design = null, 
                               bool forRecursionPattern = false, bool forPathRibbon = false)
        {
            initialized = false;
            handleControlEvents = false;
            if (design == null)
                throw new NullReferenceException("design cannot be null.");
            this.design = design;
            setPatternFromDefaultToolStripMenuItem.Enabled = this.design != null;
            EditedTransform = null;
            //foreach (Pattern pattern in patternGroup.Patterns)
            //{
            //    pattern.OrigCenterPathPattern = pattern.CenterPathPattern;
            //}
            this.selectPatternForm = selPatternForm;
            this.EditedPatternGroup = patternGroup.GetCopy(keepRecursiveParents: true);
            //INFMOD
            //lastEditedInfluenceLink = design.CopiedLastEditedInfluenceLink;
            foreach (Pattern pattern in EditedPatternGroup.Patterns)
            {
                pattern.NormalizeAmplitudeFactors();
            }
            EditedPatternIndex = 0;
            //dgvTransformParameters.DataSource = EmptyParametersDataTable;
            //dgvTransformFnParameters.DataSource = EmptyFnParametersDataTable;
            PopulateControls();
            Ribbon ribbon = EditedPattern?.GetRibbon();
            if (ribbon != null && ribbon.FormulaSettings != null)
            {
                AddParameterControls(pnlRibbonFormulaParameters, ribbon.FormulaSettings);
            }
            else
            {
                ClearParameterControls(pnlRibbonFormulaParameters);
            }
            var renderingInfo = EditedPattern?.PixelRendering;
            if (renderingInfo?.FormulaSettings != null)
            {
                AddRenderingParameterControls();
            }
            else
            {
                ClearParameterControls(pnlRenderingParameters);
            }
            mousePoint = PointF.Empty;
            btnEditRecursionPattern.Enabled = btnAddRecursionPattern.Enabled = !forRecursionPattern;
            cboPatternType.Enabled = !forPathRibbon;
            PreviewChanges();
            handleControlEvents = true;
            initialized = true;
        }

        private void PopulateControls()
        {
            bool saveHandleEvents = handleControlEvents;
            try
            {
                handleControlEvents = false;
                chkEditLayers.Checked = false;
                dgvLayers.Enabled = false;
                editedLayer = null;
                CancelColorModeColorChoice();
                Pattern pattern = EditedPattern;
                if (pattern == null)
                    return;
                ClearParameterControls(pnlOutlineParameters);
                ClearParameterControls(pnlTransformParameters);
                txtPatternName.Text = EditedPatternGroup.PatternListName;
                chkPixelRendering.Checked = pattern.PixelRendering != null && pattern.PixelRendering.Enabled;
                chkPixelFormulaEnabled.Checked = pattern.PixelRendering != null && pattern.PixelRendering.FormulaEnabled;
                chkSmoothedDraft.Checked = pattern.PixelRendering != null && pattern.PixelRendering.SmoothedDraft;
                int layerCount = pattern.PatternLayers.PatternLayers.Count;
                cboRenderLayerIndex.DataSource = Enumerable.Range(1, layerCount).ToList();
                if (pattern.PixelRendering != null && pattern.PixelRendering.Enabled)
                {
                    colorGradientFormComponent.ColorNodes = pattern.PixelRendering.ColorNodes;
                    cboRenderLayerIndex.SelectedItem = Math.Max(1, Math.Min(layerCount, pattern.PixelRendering.PatternLayerIndex + 1));
                    txtRenderPanX.Text = pattern.PixelRendering.PanXY.X.ToString();
                    txtRenderPanY.Text = pattern.PixelRendering.PanXY.Y.ToString();
                    txtRenderZoomPct.Text = (100F * pattern.PixelRendering.ZoomFactor).ToString("0.##");
                    chkUseDistancePattern.Checked = pattern.PixelRendering.UseDistanceOutline;
                    chkUseTestFormula.Checked = pattern.PixelRendering.FormulaSettings?.TestCSharpEvalType != null;
                }
                this.txtRotationSteps.Text = pattern.RotationSteps.ToString();
                this.chkAllowRandom.Checked = pattern.AllowRandom;
                this.chkIsRecursive.Checked = pattern.Recursion.IsRecursive;
                this.chkRecursionDrawAsPatterns.Checked = pattern.Recursion.DrawAsPatterns;
                this.chkRecursionUnderlay.Checked = pattern.Recursion.UnderlayDrawnPatterns;
                this.chkOffsetRecursivePatterns.Checked = pattern.Recursion.OffsetPatterns;
                chkPerspectiveScale.Checked = pattern.Recursion.UsePerspectiveScale;
                this.chkRecursionSkipFirst.Checked = pattern.Recursion.SkipFirstDrawnPattern;
                this.txtRecursiveDepth.Text = pattern.Recursion.Depth.ToString();
                this.txtRecursiveRepetitions.Text = pattern.Recursion.Repetitions.ToString();
                this.txtRecursiveScale.Text = (pattern.Recursion.Scale * 100F).ToString("0.##");
                txtOffsetAdjustmentPercent.Text = (pattern.Recursion.OffsetAdjustmentFactor * 100F).ToString("0.##");
                this.txtRecursiveRotation.Text =
                    Tools.RadiansToDegrees(pattern.Recursion.RotationAngle).ToString("0.00");
                this.txtRecursionAutoFactor.Text = pattern.Recursion.AutoSampleFactor.ToString();
                if (pattern.Recursion.RecursionPatterns.Count == 0)
                    cboRecursionPatternIndex.DataSource = null;
                else
                    cboRecursionPatternIndex.DataSource =
                        Enumerable.Range(1, pattern.Recursion.RecursionPatterns.Count).ToList();
                dgvPatternRecursionInfo_Bind(pattern);
                //chkTransformCenterPath.Enabled = pattern.CenterPathPattern != null;
                //chkTransformCenterPath.Checked = pattern.PreserveCenterPath;
                PathPattern pathPattern = pattern as PathPattern;
                txtPathPenWidth.Enabled = pathPattern != null;
                if (pathPattern != null)
                {
                    txtPathPenWidth.Text = pathPattern.PenWidth.ToString();
                    chkInterpolatePoints.Checked = pathPattern.InterpolatePoints;
                }
                PopulateOutlinesControls();
                //CurrentCartesianPathOutline = pathPattern?.CartesianPathOutline;
                //chkCartesianPath.Checked = CurrentCartesianPathOutline != null;
                //dgvBasicOutlines.AllowUserToAddRows = CurrentCartesianPathOutline == null;
                //outlinesDataTable.Rows.Clear();
                //List<BasicOutline> basicOutlines;
                //if (CurrentCartesianPathOutline == null)
                //{
                //    basicOutlines = pattern.BasicOutlines;
                //    BasicOutlineType.DataSource =
                //        Enum.GetNames(typeof(BasicOutlineTypes));
                //}
                //else
                //{
                //    basicOutlines = new List<BasicOutline>() { CurrentCartesianPathOutline };
                //    BasicOutlineType.DataSource = new List<string>()
                //            { BasicOutlineTypes.Path.ToString() };
                //}
                //foreach (BasicOutline otl in basicOutlines)
                //{
                //    int petals = (int)otl.Petals; //(int)(2 * otl.UsedFrequency);
                //    double weight = Math.Round(otl.AmplitudeFactor, 4);
                //    double phase = Math.Round(Tools.RadiansToDegrees(otl.AngleOffset), 1);
                //    double pointiness = Math.Round(otl.GetPointiness(), 2);
                //    //string formula = otl.customOutline == null ? string.Empty
                //    //            : otl.customOutline.AmplitudeSettings.Formula;
                //    //string maxFormula = otl.customOutline == null ? string.Empty
                //    //            : otl.customOutline.MaxAmplitudeSettings.Formula;
                //    outlinesDataTable.Rows.Add(otl.BasicOutlineType.ToString(), petals, weight,
                //                               phase, pointiness, otl.Enabled, otl);
                //}
                //this.dgvBasicOutlines.DataSource = outlinesDataTable;
                Ribbon targetRibbon;
                if (pathPattern != null)
                    targetRibbon = pathPattern.PathRibbon;
                else
                    targetRibbon = pattern as Ribbon;
                Pattern targetPattern = targetRibbon ?? pattern;
                chkUseLinearGradient.Checked = targetPattern.UseLinearGradient;
                txtGradientPadding.Text = targetPattern.GradientPadding.ToString();
                txtGradientRotation.Text = Tools.RadiansToDegrees(targetPattern.GradientRotation).ToString("0.0");
                this.cboFillType.SelectedItem = targetPattern.FillInfo.FillType;
                FillTypeOnChanged(targetPattern.FillInfo.FillType);
                PopulateFillControls(targetPattern.FillInfo);
                this.cboMergeOperation.SelectedItem = pattern.MergeOperation;
                this.cboPatternType.SelectedItem = pattern is PathPattern ? "Path" :
                     pattern is StringPattern ? "Text" : pattern.GetType().Name;
                if (targetRibbon == null)
                    targetRibbon = targetPattern as Ribbon;
                this.tabRibbon.Enabled = tabRibbonFormula.Enabled = targetRibbon != null;
                StringPattern stringPattern = pattern as StringPattern;
                if (stringPattern != null)
                {
                    txtText.Text = stringPattern.Text;
                    chkTextKeepAtRightAngle.Checked = stringPattern.KeepRightAngle;
                }
                this.tabOutlines.Enabled = this.tabPatternLayers.Enabled = this.tabRecursion.Enabled = this.tabSection.Enabled
                    = this.tabTiling.Enabled = this.tabTransforms.Enabled = stringPattern == null;
                this.cboRenderMode.SelectedItem = pattern.RenderMode;
                this.cboStainBlendType.SelectedItem = pattern.StainBlendType;
                this.txtStainWidth.Text = pattern.StainWidth.ToString();
                this.txtZoomPercentage.Text = Math.Round(100D * pattern.ZoomFactor, 1).ToString();
                this.txtLoopFactor.Text = pattern.LoopFactor.ToString();
                this.chkShrinkPattern.Checked = pattern.ShrinkPattern;
                this.chkShrinkPatternLayers.Checked = pattern.ShrinkPatternLayers;
                this.txtShrinkPadding.Text = pattern.ShrinkPadding.ToString();
                this.txtShrinkClipFactor.Text = pattern.ShrinkClipFactor.ToString();
                this.txtShrinkClipCenterFactor.Text = pattern.ShrinkClipCenterFactor.ToString();
                this.chkIsSection.Checked = targetPattern.PatternSectionInfo.IsSection;
                this.txtSectionAmplitudePercentage.Text =
                    (100F * targetPattern.PatternSectionInfo.SectionAmplitudeRatio).ToString();
                this.chkRecomputeInnerSection.Checked =
                    targetPattern.PatternSectionInfo.RecomputeInnerSection;
                if (targetRibbon != null)
                {
                    PopulateRibbonControls(targetRibbon);
                }
                chkTilePattern.Checked = pattern.PatternTileInfo.TilePattern;
                txtTilePatternsPerRow.Text = pattern.PatternTileInfo.PatternsPerRow.ToString();
                //txtTilePatternSizePercent.Text = (100D * pattern.PatternTileInfo.PatternSizeRatio).ToString();
                txtTileBorderWidth.Text = pattern.PatternTileInfo.BorderWidth.ToString();
                txtTileTopMargin.Text = (100F * pattern.PatternTileInfo.TopMargin).ToString();
                txtTileLeftMargin.Text = (100F * pattern.PatternTileInfo.LeftMargin).ToString();
                txtTileBottomMargin.Text = (100F * pattern.PatternTileInfo.BottomMargin).ToString();
                txtTileRightMargin.Text = (100F * pattern.PatternTileInfo.RightMargin).ToString();
                //txtTileGridAdjX.Text = pattern.PatternTileInfo.GridAdjustment.X.ToString();
                //txtTileGridAdjY.Text = pattern.PatternTileInfo.GridAdjustment.Y.ToString();
                EditedPattern.SortTransforms();
                BindTransformsGrid();
            }
            finally
            {
                handleControlEvents = saveHandleEvents;
            }
        }

        private void PopulateOutlinesControls()
        {
            Pattern pattern = EditedPattern;
            if (pattern == null)
                return;
            PathPattern pathPattern = pattern as PathPattern;
            CurrentCartesianPathOutline = pathPattern?.CartesianPathOutline;
            chkCartesianPath.Checked = CurrentCartesianPathOutline != null;
            dgvBasicOutlines.AllowUserToAddRows = CurrentCartesianPathOutline == null;
            outlinesDataTable.Rows.Clear();
            List<BasicOutline> basicOutlines;
            if (CurrentCartesianPathOutline == null)
            {
                basicOutlines = pattern.BasicOutlines;
                BasicOutlineType.DataSource =
                    Enum.GetNames(typeof(BasicOutlineTypes));
            }
            else
            {
                basicOutlines = new List<BasicOutline>() { CurrentCartesianPathOutline };
                BasicOutlineType.DataSource = new List<string>()
                            { BasicOutlineTypes.Path.ToString() };
            }
            foreach (BasicOutline otl in basicOutlines)
            {
                int petals = (int)otl.Petals; //(int)(2 * otl.UsedFrequency);
                double weight = Math.Round(otl.AmplitudeFactor, 4);
                double phase = Math.Round(Tools.RadiansToDegrees(otl.AngleOffset), 1);
                double pointiness = Math.Round(otl.GetPointiness(), 2);
                //string formula = otl.customOutline == null ? string.Empty
                //            : otl.customOutline.AmplitudeSettings.Formula;
                //string maxFormula = otl.customOutline == null ? string.Empty
                //            : otl.customOutline.MaxAmplitudeSettings.Formula;
                outlinesDataTable.Rows.Add(otl.BasicOutlineType.ToString(), petals, weight,
                                           phase, pointiness, otl.Enabled, otl);
            }
            this.dgvBasicOutlines.DataSource = outlinesDataTable;
        }

        private void dgvPatternRecursionInfo_Bind(Pattern pattern)
        {
            patternRecursionInfoBindingSource.Clear();
            foreach (var info in pattern.Recursion.InfoList)
            {
                patternRecursionInfoBindingSource.Add(info);
            }
        }

        private void PopulateFillControls(FillInfo fillInfo)
        {
            bool handle = handleControlEvents;
            try
            {
                handleControlEvents = false;
                PathFillInfo pathFillInfo = fillInfo as PathFillInfo;
                if (pathFillInfo != null)
                {
                    this.btnBoundaryColor.BackColor = pathFillInfo.BoundaryColor;
                    this.btnCenterColor.BackColor = pathFillInfo.CenterColor;
                    if (EditedPattern?.PixelRendering == null || !EditedPattern.PixelRendering.Enabled)
                        colorGradientFormComponent.ColorNodes = pathFillInfo.GetColorNodes();
                    this.cboPathColorMode.SelectedItem = pathFillInfo.ColorMode;
                    if (pathFillInfo.ColorInfo != null)
                    {
                        this.chkTranslucentWaves.Checked = pathFillInfo.ColorInfo.TranslucentWaves;
                        this.txtTranslucentCycles.Text =
                            pathFillInfo.ColorInfo.TranslucentCycles.ToString();
                        this.txtTranslucentStrength.Text =
                            (100F * pathFillInfo.ColorInfo.TranslucentStrength).ToString();
                        this.chkRandomWaves.Checked = pathFillInfo.ColorInfo.RandomWaves;
                    }
                }
                else
                {
                    TextureFillInfo textureFillInfo = fillInfo as TextureFillInfo;
                    if (textureFillInfo != null)
                    {
                        this.txtTextureImageFileName.Text = textureFillInfo.TextureImageFileName;
                        //chkPreserveTextureSize.Checked = textureFillInfo.PreserveTextureSize;
                        txtTextureScale.Text = (100F * textureFillInfo.TextureScale).ToString("0.#");
                        cboImageMode.SelectedItem = textureFillInfo.ImageMode;
                    }
                }
            }
            finally
            {
                handleControlEvents = handle;
            }
        }

        private void PopulateRibbonControls(Ribbon ribbon)
        {
            this.txtRibbonDistance.Text = ribbon.RibbonDistance.ToString();
            this.txtVertexPadding.Text = (ribbon.VertexPadding * 100.0).ToString();
            this.txtSegmentPadding.Text = (ribbon.SegmentPadding * 100.0).ToString();
            this.txtRibbonPenWidth.Text = ribbon.PenWidth.ToString();
            double rotation = ribbon.PatternZVector.GetArgument();
            rotation = Math.Round(Tools.RadiansToDegrees(rotation % (2D * Math.PI)), 2);
            this.txtRibbonPatternRotation.Text = rotation.ToString();
            double amplitude = Math.Round(ribbon.PatternZVector.GetModulus(), 2);
            this.txtRibbonPatternSize.Text = amplitude.ToString();
            this.txtTaperPercentage.Text = Math.Round(ribbon.TaperPercentage, 2).ToString();
            this.txtRibbonAngleOffset.Text = Tools.RadiansToDegrees(ribbon.DirectFillAngleOffset).ToString("0.0");
            this.txtRibbonAmpOffset.Text = ribbon.DirectFillAmpOffset.ToString("0.000");
            this.txtRibbonMinSegments.Text = ribbon.MinSegments.ToString();
            this.chkTrackAngle.Checked = ribbon.TrackAngle;
            this.chkCycleColors.Checked = ribbon.CycleColors;
            this.chkSmoothRibbonGradient.Checked = ribbon.SmoothGradient;
            this.chkRibbonLinearGradientPerSegment.Checked = ribbon.LinearGradientPerSegment;
            this.chkFitToPathVertices.Checked = ribbon.FitToPathVertices;
            this.chkScaleRibbonTaper.Checked = ribbon.ScaleTaper;
            this.chkRibbonFormulaEnabled.Checked = ribbon.FormulaEnabled;
            this.cboDrawingMode.SelectedItem = ribbon.DrawingMode;
            //this.cboPathMode.SelectedItem = ribbon.PathMode;
            this.cboClipSegmentCount.DataSource = Enumerable.Range(1, Math.Max(0, ribbon.RibbonPath.Count - 1)).ToList();
        }

        private DataRow GetDataRow(DataGridView dgv, int rowIndex)
        {
            var dataRowView = (DataRowView)dgv.Rows[rowIndex].DataBoundItem;
            return dataRowView.Row;
        }

        private void dgvBasicOutlines_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;
                var dgvCol = dgvBasicOutlines.Columns[e.ColumnIndex];
                if (dgvCol != btnCustomSettings && dgvCol != btnIncrementOutlineProperty)
                    return;
                DataRow dRow = GetDataRow(dgvBasicOutlines, e.RowIndex);   //this.outlinesDataTable.Rows[e.RowIndex];
                BasicOutline outline = GetDataRowBasicOutline(dRow);
                if (dgvCol == btnIncrementOutlineProperty)
                {
                    if (outline != null)
                        ShowPropertyActionsForm(outline);
                    return;
                }
                BasicOutlineTypes? outlineType = GetOutlineType(dRow);
                if (outlineType != BasicOutlineTypes.Custom &&
                    outlineType != BasicOutlineTypes.Path)
                    return;
                outline = GetDataRowBasicOutline(dRow, (BasicOutlineTypes)outlineType);
                if (chkCartesianPath.Checked && outline is PathOutline)
                {
                    EditCartesianFormula((PathOutline)outline);
                    return;
                }
                FormulaSettings formulaSettings = outline.GetFormulaSettings();
                //object oFormula = dRow["AmplitudeFormula"];
                //object oMaxFormula = dRow["MaxAmplitudeFormula"];
                using (var frm = new OutlineFormulaForm())
                {
                    frm.Initialize(outline);
                    //formulaSettings?.FormulaName,
                    //oFormula == DBNull.Value ? string.Empty : oFormula.ToString(),
                    //oMaxFormula == DBNull.Value ? string.Empty : oMaxFormula.ToString());
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        //if (formulaSettings != null)
                        //    formulaSettings.FormulaName = frm.FormulaName;
                        //dRow["AmplitudeFormula"] = frm.AmplitudeFormula;
                        //dRow["MaxAmplitudeFormula"] = frm.MaxAmplitudeFormula;
                        AddParameterControls(pnlOutlineParameters, formulaSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private BasicOutline GetDataRowBasicOutline(DataRow dRow)
        {
            return dRow["BasicOutline"] as BasicOutline;
        }

        private BasicOutline GetDataRowBasicOutline(DataRow dRow, BasicOutlineTypes outlineType)
        {
            BasicOutline outline = GetDataRowBasicOutline(dRow);
            if (outline == null || outline.BasicOutlineType != outlineType)
            {
                if (outlineType == BasicOutlineTypes.Path)
                {
                    outline = new PathOutline();
                    //if (outline == null)
                    //    outline = new PathOutline();
                    //else
                    //    outline = new PathOutline(outline);
                }
                else //if (outline == null)
                    outline = new BasicOutline(outlineType);
                //else
                //    outline.BasicOutlineType = outlineType;
                dRow["BasicOutline"] = outline;
            }
            return outline;
        }

        private void dgvBasicOutlines_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex < 0) return;
            MessageBox.Show("Column: " + dgvBasicOutlines.Columns[e.ColumnIndex].HeaderText + ":\n" + e.Exception.Message);
            e.ThrowException = false;
        }

        private void dgvBasicOutlines_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.RowIndex >= outlinesDataTable.Rows.Count)
                    return;
                DataRow dRow = GetDataRow(dgvBasicOutlines, e.RowIndex); //this.outlinesDataTable.Rows[e.RowIndex];
                if (dRow.RowState == DataRowState.Detached ||
                    dRow.RowState == DataRowState.Deleted)
                    return;
                BasicOutlineTypes? outlineType = GetOutlineType(dRow);
                if (outlineType != BasicOutlineTypes.Custom &&
                    outlineType != BasicOutlineTypes.Path)
                {
                    //ClearParameterControls(pnlOutlineParameters);
                    return;
                }
                BasicOutline outline = GetDataRowBasicOutline(dRow, (BasicOutlineTypes)outlineType);
                FormulaSettings formulaSettings = outline.GetFormulaSettings();
                if (formulaSettings != null)
                    AddParameterControls(pnlOutlineParameters, formulaSettings);
                //dgvCustomParameters.DataSource =
                //    outline.customOutline.AmplitudeSettings.ParametersDataTable;
                //dgvOutlineFnParameters.DataSource =
                //    outline.customOutline.AmplitudeSettings.FnParametersDataTable;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvTransforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;
                var dgvRow = dgvTransforms.Rows[e.RowIndex];
                PatternTransform transform = (PatternTransform)dgvRow.DataBoundItem; //EditedPattern.Transforms[e.RowIndex];
                bool changed = false;
                if (dgvTransforms.Columns[e.ColumnIndex] == btnEditTransform)
                {
                    changed = EditTransform(transform);
                    if (changed && transform.SequenceNumberChanged)
                    {
                        EditedPattern.Transforms.Remove(transform);
                        EditedPattern.Transforms.Insert(transform.SequenceNumber, transform);
                        for (int i = 0; i < EditedPattern.Transforms.Count; i++)
                        {
                            EditedPattern.Transforms[i].SequenceNumber = i;
                            EditedPattern.Transforms[i].SequenceNumberChanged = false;
                        }
                    }
                }
                else if (dgvTransforms.Columns[e.ColumnIndex] == btnDeleteTransform)
                {
                    EditedPattern.Transforms.Remove(transform);
                    changed = true;
                }
                if (changed)
                {
                    BindTransformsGrid();
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvTransforms_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.RowIndex >= dgvTransforms.Rows.Count)
                    return;
                selectedTransform = (PatternTransform)dgvTransforms.Rows[e.RowIndex].DataBoundItem;
                // EditedPattern.Transforms[e.RowIndex];
                AddParameterControls(pnlTransformParameters, selectedTransform.TransformSettings);
                //parameterDisplay.AddAllParametersControls(pnlTransformParameters, transform.TransformSettings);
                //dgvTransformParameters.DataSource = transform.TransformSettings.ParametersDataTable;
                //dgvTransformFnParameters.DataSource = transform.TransformSettings.FnParametersDataTable;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvTransforms_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0
                    && dgvTransforms.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    dgvTransforms.EndEdit();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvTransforms_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex != colTransformEnabled.Index)
                    return;
                var cell = dgvTransforms.CurrentCell;
                if (cell.Value is bool)
                {
                    var dgvRow = dgvTransforms.Rows[e.RowIndex];
                    var transform = dgvRow.DataBoundItem as PatternTransform;
                    if (transform != null)
                    {
                        transform.Enabled = (bool)cell.Value;
                        PreviewChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvTransforms_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex < 0) return;
            MessageBox.Show("Column: " + dgvTransforms.Columns[e.ColumnIndex].HeaderText +
                ":\n" + e.Exception.Message);
            e.ThrowException = false;
        }

        private BasicOutlineTypes? GetOutlineType(DataRow dRow)
        {
            string sOutlineType = (string)Tools.IfDbNull(dRow["BasicOutlineType"], null);
            BasicOutlineTypes outlineType;
            if (Enum.TryParse(sOutlineType, out outlineType))
            {
                if (chkCartesianPath.Checked
                    && outlineType != BasicOutlineTypes.Path)
                {
                    outlineType = BasicOutlineTypes.Path;
                    dRow["BasicOutlineType"] = outlineType.ToString();
                    MessageBox.Show("Outline must be of type Path for Cartesian Path.");
                }
                return outlineType;
            }
            else
                return null;
        }

        private void UpdateRecursionAutoFactor(List<string> messages)
        {
            Pattern pattern = EditedPattern;
            if (!float.TryParse(txtRecursionAutoFactor.Text, out float fVal))
                fVal = -1;
            if (fVal > 0F && fVal <= 1000F)
                pattern.Recursion.AutoSampleFactor = fVal;
            else
                messages.Add("Please enter a positive value less than 1000 for Autosample Factor.");
        }

        private bool UpdatePattern()
        {
            int iVal;
            float fVal;
            double dblVal;
            List<string> messages = new List<string>();
            Pattern pattern = EditedPattern;
            if (pattern == null)
                return true;
            pattern.ClearRenderingCache();
            if (pattern.PixelRendering != null)
            {
                pattern.PixelRendering.FormulaEnabled = chkPixelFormulaEnabled.Checked;
                pattern.PixelRendering.SmoothedDraft = chkSmoothedDraft.Checked;
                int index = cboRenderLayerIndex.SelectedItem is int ? (int)cboRenderLayerIndex.SelectedItem - 1 : -1;
                pattern.PixelRendering.PatternLayerIndex = index;
                if (float.TryParse(txtRenderPanX.Text, out float panX) &&
                    float.TryParse(txtRenderPanY.Text, out float panY))
                {
                    pattern.PixelRendering.PanXY = new PointF(panX, panY);
                }
                if (float.TryParse(txtRenderZoomPct.Text, out fVal))
                {
                    if (fVal > 0)
                        pattern.PixelRendering.ZoomFactor = 0.01F * fVal;
                }
                pattern.PixelRendering.UseDistanceOutline = chkUseDistancePattern.Checked;
            }
            EditedPatternGroup.PatternListName = txtPatternName.Text;
            if (int.TryParse(txtRotationSteps.Text, out iVal))
            {
                if (iVal < Pattern.MinRotationSteps)
                    MessageBox.Show($"Precision cannot be less than {Pattern.MinRotationSteps}.");
                else
                    pattern.RotationSteps = iVal;
            }
            pattern.AllowRandom = chkAllowRandom.Checked;
            IFillInfo iFillInfoParent = fillInfoParent;
            FillInfo.FillTypes fillType = (FillInfo.FillTypes)cboFillType.SelectedItem;
            if (iFillInfoParent != null)
            {
                if (fillType != iFillInfoParent.FillInfo.FillType)
                {
                    iFillInfoParent.FillInfo.Dispose();
                    if (fillType == FillInfo.FillTypes.Path)
                        iFillInfoParent.FillInfo = new PathFillInfo(pattern);
                    else
                        iFillInfoParent.FillInfo = new TextureFillInfo(pattern);
                }
                PathFillInfo pathFillInfo = iFillInfoParent.FillInfo as PathFillInfo;
                if (pathFillInfo != null)
                {
                    pathFillInfo.ColorMode =
                        (FillInfo.PathColorModes)cboPathColorMode.SelectedItem;
                    pathFillInfo.BoundaryColor = this.btnBoundaryColor.BackColor;
                    pathFillInfo.CenterColor = this.btnCenterColor.BackColor;
                    var colorInfo = pathFillInfo.ColorInfo;
                    if (colorInfo != null)
                    {
                        colorInfo.TranslucentWaves = this.chkTranslucentWaves.Checked;
                        colorInfo.RandomWaves = chkRandomWaves.Checked;
                        if (this.chkTranslucentWaves.Checked)
                        {
                            if (!int.TryParse(txtTranslucentCycles.Text, out iVal))
                                iVal = -1;
                            if (iVal <= 0)
                                messages.Add(
                                    "Please enter a positive integer for Translucent Cycles.");
                            else
                                colorInfo.TranslucentCycles = iVal;
                            if (!float.TryParse(txtTranslucentStrength.Text, out fVal))
                                fVal = -1;
                            if (fVal < 0 || fVal > 100)
                                messages.Add(
                    "Please enter a percentage between 0 and 100 for Translucent Strength.");
                            else
                                colorInfo.TranslucentStrength = fVal / 100F;
                        }
                    }
                }
                else
                {
                    TextureFillInfo textureFillInfo = iFillInfoParent.FillInfo as TextureFillInfo;
                    if (textureFillInfo != null)
                    {
                        textureFillInfo.TextureImageFileName = txtTextureImageFileName.Text;
                        //textureFillInfo.PreserveTextureSize = chkPreserveTextureSize.Checked;
                        if (float.TryParse(txtTextureScale.Text, out float textureScale))
                        {
                            if (textureScale > 0F)
                                textureFillInfo.TextureScale = textureScale / 100F;
                        }
                        textureFillInfo.ImageMode = (TextureImageModes)cboImageMode.SelectedItem;
                    }
                }
            }
            pattern.MergeOperation = (Pattern.MergeOperations)cboMergeOperation.SelectedItem;
            pattern.RenderMode = (Pattern.RenderModes)cboRenderMode.SelectedItem;
            pattern.StainBlendType = (ColorBlendTypes)cboStainBlendType.SelectedItem;
            if (!int.TryParse(txtStainWidth.Text, out iVal))
                iVal = -1;
            if (iVal <= 0)
                messages.Add("Please enter a positive integer for Stain Width.");
            else
                pattern.StainWidth = iVal;
            if (!float.TryParse(txtZoomPercentage.Text, out fVal))
                fVal = -1F;
            if (fVal > 0)
                pattern.ZoomFactor = (double)fVal / 100D;
            else
                messages.Add("Please enter a positive number for Zoom %.");
            if (float.TryParse(txtLoopFactor.Text, out fVal))
            {
                if (fVal >= 0F && fVal < 1000F)
                    pattern.LoopFactor = fVal;
            }
            pattern.ShrinkPattern = chkShrinkPattern.Checked;
            if (pattern.ShrinkPatternLayers != chkShrinkPatternLayers.Checked)
            {
                pattern.ShrinkPatternLayers = chkShrinkPatternLayers.Checked;
                if (pattern.ShrinkPatternLayers)
                    pattern.ComputeSeedPoints();
            }
            if (float.TryParse(txtShrinkPadding.Text, out fVal))
                pattern.ShrinkPadding = fVal;
            if (float.TryParse(txtShrinkClipFactor.Text, out fVal))
                pattern.ShrinkClipFactor = fVal;
            if (float.TryParse(txtShrinkClipCenterFactor.Text, out fVal))
                pattern.ShrinkClipCenterFactor = fVal;
            pattern.PatternTileInfo.TilePattern = chkTilePattern.Checked;
            if (int.TryParse(txtTilePatternsPerRow.Text, out iVal))
            {
                if (iVal > 0)
                    pattern.PatternTileInfo.PatternsPerRow = iVal;
            }
            //if (double.TryParse(txtTilePatternSizePercent.Text, out dblVal))
            //{
            //    if (dblVal > 0)
            //        pattern.PatternTileInfo.PatternSizeRatio = 0.01 * dblVal;
            //}
            if (int.TryParse(txtTileBorderWidth.Text, out iVal))
            {
                pattern.PatternTileInfo.BorderWidth = Math.Max(0, iVal);
            }
            if (float.TryParse(txtTileTopMargin.Text, out fVal))
            {
                pattern.PatternTileInfo.TopMargin = 0.01F * fVal;
            }
            if (float.TryParse(txtTileLeftMargin.Text, out fVal))
            {
                pattern.PatternTileInfo.LeftMargin = 0.01F * fVal;
            }
            if (float.TryParse(txtTileBottomMargin.Text, out fVal))
            {
                pattern.PatternTileInfo.BottomMargin = 0.01F * fVal;
            }
            if (float.TryParse(txtTileRightMargin.Text, out fVal))
            {
                pattern.PatternTileInfo.RightMargin = 0.01F * fVal;
            }
            //if (float.TryParse(txtTileGridAdjX.Text, out float fX))
            //{
            //    if (float.TryParse(txtTileGridAdjY.Text, out float fY))
            //    {
            //        pattern.PatternTileInfo.GridAdjustment = new PointF(fX, fY);
            //    }
            //}
            //pattern.PreserveCenterPath = chkTransformCenterPath.Checked;
            pattern.Recursion.IsRecursive = chkIsRecursive.Checked;
            pattern.Recursion.DrawAsPatterns = chkRecursionDrawAsPatterns.Checked;
            pattern.Recursion.UnderlayDrawnPatterns = chkRecursionUnderlay.Checked;
            pattern.Recursion.OffsetPatterns = chkOffsetRecursivePatterns.Checked;
            pattern.Recursion.UsePerspectiveScale = chkPerspectiveScale.Checked;
            pattern.Recursion.SkipFirstDrawnPattern = chkRecursionSkipFirst.Checked;
            if (!int.TryParse(txtRecursiveDepth.Text, out iVal))
                iVal = -1;
            if (iVal >= 1)
                pattern.Recursion.Depth = iVal;
            else
                messages.Add("Please enter a positive integer for Recursive Depth.");
            if (!int.TryParse(txtRecursiveRepetitions.Text, out iVal))
                iVal = -1;
            if (iVal >= 1)
                pattern.Recursion.Repetitions = iVal;
            else
                messages.Add("Please enter a positive integer for Recursive Depth.");
            if (!float.TryParse(txtRecursiveScale.Text, out fVal))
                fVal = -1;
            if (fVal > 0F)
                pattern.Recursion.Scale = fVal / 100F;
            else
                messages.Add("Please enter a positive percentage for Recursive Scale.");
            if (float.TryParse(txtOffsetAdjustmentPercent.Text, out fVal))
                pattern.Recursion.OffsetAdjustmentFactor = fVal / 100F;
            if (double.TryParse(txtRecursiveRotation.Text, out dblVal))
                pattern.Recursion.RotationAngle = Tools.DegreesToRadians(dblVal);
            if (pattern.Recursion.IsRecursive && messages.Count == 0)
            {
                if (pattern.SeedPoints == null)
                    pattern.ComputeSeedPoints();
                pattern.Recursion.RotationOffsetRatio =
                        (float)pattern.MaxPointIndex / (pattern.SeedPoints.Length - 1);
            }
            UpdateRecursionAutoFactor(messages);
            StringPattern stringPattern = pattern as StringPattern;
            if (stringPattern != null)
            {
                stringPattern.Text = txtText.Text;
                stringPattern.KeepRightAngle = chkTextKeepAtRightAngle.Checked;
            }
            var pathPattern = pattern as PathPattern;
            if (pathPattern != null)
            {
                if (float.TryParse(txtPathPenWidth.Text, out fVal))
                {
                    if (fVal > 0)
                        pathPattern.PenWidth = fVal;
                }
                pathPattern.InterpolatePoints = chkInterpolatePoints.Checked;
            }
            List<BasicOutline> outlines = new List<BasicOutline>();
            foreach (DataRow dRow in this.outlinesDataTable.Rows)
            {
                BasicOutlineTypes? outlineType = GetOutlineType(dRow);
                if (outlineType == null)
                    continue;
                BasicOutline outline = GetDataRowBasicOutline(dRow, (BasicOutlineTypes)outlineType);
                outline.Frequency = (double)Math.Max(
                    Tools.GetDBValue<int>(dRow["Petals"], 1), 1) / 2D;
                outline.AmplitudeFactor = Tools.GetDBValue<double>(dRow["Weight"], 1D);
                outline.AngleOffset = Tools.DegreesToRadians(
                        Tools.GetDBValue<double>(dRow["Phase"], 0D));
                outline.Enabled = Tools.GetDBValue<bool>(dRow["Enabled"], false);
                //if (outlineType == BasicOutlineTypes.Custom ||
                //    outlineType == BasicOutlineTypes.Path)
                //{
                //    PathOutline pathOutline = outline as PathOutline;
                //    FormulaSettings formulaSettings = outline.GetFormulaSettings();
                //    if (pathOutline == null)
                //    {
                //        formulaSettings.Parse(
                //            Tools.GetDBValue<string>(dRow["AmplitudeFormula"], string.Empty));
                //        outline.customOutline.MaxAmplitudeSettings.Parse(
                //            Tools.GetDBValue<string>(dRow["MaxAmplitudeFormula"], string.Empty));
                //    }
                //    //else if (pathOutline.UseVertices)
                //    //{
                //    //    pathOutline.AddVertices();
                //    //}
                //}
                //AddDenom property computes UnitFactor, so set after formulas are set:
                outline.SetPointiness(Tools.GetDBValue<double>(dRow["Pointiness"], 0D));
                outlines.Add(outline);
            }
            pattern.BasicOutlines.Clear();
            pattern.BasicOutlines.AddRange(outlines);
            pattern.SetVertexAnglesParameters();
            pattern.ComputeSeedPoints();
            Ribbon pathRibbon = this.pathRibbon;
            Pattern targetPattern = pathRibbon ?? pattern;
            targetPattern.PatternSectionInfo.IsSection = chkIsSection.Checked;
            targetPattern.PatternSectionInfo.RecomputeInnerSection =
                chkRecomputeInnerSection.Checked;
            targetPattern.UseLinearGradient = chkUseLinearGradient.Checked;
            if (float.TryParse(txtGradientPadding.Text, out fVal))
            {
                targetPattern.GradientPadding = fVal;
            }
            if (float.TryParse(txtGradientRotation.Text, out fVal))
            {
                targetPattern.GradientRotation = (float)Tools.DegreesToRadians(fVal);
            }
            if (!float.TryParse(txtSectionAmplitudePercentage.Text, out fVal))
                fVal = -1;
            if (fVal <= 0 || fVal >= 100F)
            {
                messages.Add("Please enter a value > 0% and < 100% for Size Percentage.");
            }
            else
                targetPattern.PatternSectionInfo.SectionAmplitudeRatio = fVal / 100F;
            Ribbon ribbon = targetPattern as Ribbon;
            if (ribbon != null)
            {
                ribbon.TrackAngle = this.chkTrackAngle.Checked;
                ribbon.CycleColors = this.chkCycleColors.Checked;
                ribbon.SmoothGradient = this.chkSmoothRibbonGradient.Checked;
                ribbon.LinearGradientPerSegment = this.chkRibbonLinearGradientPerSegment.Checked;
                ribbon.FitToPathVertices = this.chkFitToPathVertices.Checked;
                ribbon.ScaleTaper = this.chkScaleRibbonTaper.Checked;
                ribbon.FormulaEnabled = this.chkRibbonFormulaEnabled.Checked;
                ribbon.DrawingMode = (RibbonDrawingModes)cboDrawingMode.SelectedItem;
                //ribbon.PathMode = (RibbonPathModes)cboPathMode.SelectedItem;
                dblVal = 0;
                if (double.TryParse(txtRibbonDistance.Text, out dblVal))
                {
                    if (dblVal >= 1 && dblVal <= 500)
                        ribbon.RibbonDistance = dblVal;
                    else
                        dblVal = 0;
                }
                if (dblVal == 0)
                {
                    messages.Add("Please enter a number between 1 and 500 for Ribbon Distance.");
                }
                if (double.TryParse(txtVertexPadding.Text, out dblVal))
                {
                    if (dblVal >= 0 && dblVal <= 90)
                        ribbon.VertexPadding = dblVal / 100.0;
                    else
                        dblVal = -1;
                }
                else
                    dblVal = -1;
                if (dblVal == -1)
                {
                    messages.Add("Please enter a number between 0 and 90 for Vertex Padding %.");
                }
                if (double.TryParse(txtSegmentPadding.Text, out dblVal))
                {
                    if (dblVal >= 0 && dblVal <= 90)
                        ribbon.SegmentPadding = dblVal / 100.0;
                    else
                        dblVal = -1;
                }
                else
                    dblVal = -1;
                if (dblVal == -1)
                {
                    messages.Add("Please enter a number between 0 and 90 for Segment Padding %.");
                }
                if (float.TryParse(txtRibbonPenWidth.Text, out fVal))
                {
                    if (fVal >= Pattern.MinPenWidth && fVal <= 200)
                        ribbon.PenWidth = fVal;
                    else
                        fVal = 0;
                }
                if (fVal == 0)
                {
                    messages.Add($"Please enter a number between {Pattern.MinPenWidth} and 200 for Ribbon Thickness.");
                }
                double argument = 0, modulus = 0;
                if (double.TryParse(txtRibbonPatternRotation.Text, out dblVal))
                {
                    argument = Tools.DegreesToRadians(dblVal);
                }
                else
                {
                    messages.Add("Please enter a number for Pattern Rotation.");
                }
                if (double.TryParse(txtRibbonPatternSize.Text, out dblVal))
                {
                    if (dblVal > 0)
                        modulus = dblVal;
                }
                if (modulus == 0)
                {
                    messages.Add("Please enter a positive number for Pattern Size.");
                }
                float taperPercentage = -1000F;
                if (float.TryParse(txtTaperPercentage.Text, out taperPercentage))
                {
                    if (Math.Abs(taperPercentage) <= 100F)
                        ribbon.TaperPercentage = taperPercentage;
                    else
                        taperPercentage = -1000F;
                }
                if (taperPercentage == -1000F)
                {
                    messages.Add("Please enter a number between -100 and 100 for Taper Percentage.");
                }
                float offset = -1000F;
                if (float.TryParse(txtRibbonAngleOffset.Text, out offset))
                {
                    if (offset >= 0 && offset <= 360)
                        ribbon.DirectFillAngleOffset = (float)Tools.DegreesToRadians(offset);
                    else
                        offset = -1000F;
                }
                if (offset == -1000F)
                {
                    messages.Add("Please enter a number between 0 and 360 for Angle Offset.");
                }
                offset = -1000F;
                if (float.TryParse(txtRibbonAmpOffset.Text, out offset))
                {
                    if (Math.Abs(offset) <= 5F)
                        ribbon.DirectFillAmpOffset = offset;
                    else
                        offset = -1000F;
                }
                if (offset == -1000F)
                {
                    messages.Add("Please enter a number between -5 and 5 for Amp Offset.");
                }
                if (!int.TryParse(txtRibbonMinSegments.Text, out iVal))
                    iVal = -1;
                if (iVal >= 0)
                    ribbon.MinSegments = iVal;
                else
                    messages.Add("Please enter zero or a positive integer for Minimum Segments.");
                if (messages.Count == 0)
                {
                    ribbon.PatternZVector =
                        Complex.CreateFromModulusAndArgument(modulus, argument);
                    PopulateRibbonControls(ribbon);
                }
            }
            //SetParametersFromControls(pnlOutlineParameters);
            //SetParametersFromControls(pnlTransformParameters);
            //SetParametersFromControls(pnlRibbonFormulaParameters);
            EditedPatternGroup.SetProperties();
            PreviewPatternGroup = null;
            if (messages.Count != 0)
                MessageBox.Show(string.Join(Environment.NewLine, messages), "Message");
            else
                EditedPatternGroup.IsChanged = true;
            return (messages.Count == 0);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (UpdatePattern())
                {
                    //INFMOD
                    //EditedPattern.LastEditedInfluenceLink = lastEditedInfluenceLink;
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void PreviewChanges(bool updatePattern = false)
        {
            if (updatePattern)
                UpdatePattern();
            if (PreviewPatternGroup != null)
                PreviewPatternGroup.Dispose();
            PreviewPatternGroup = EditedPatternGroup.GetCopy();
            if (!chkPreviewFullSize.Checked)
            {
                PreviewPatternGroup.SetForPreview(picPreview.ClientSize);
                if (double.TryParse(txtPreviewZoomPct.Text, out double zoomFactor))
                    zoomFactor *= 0.01;
                else
                    zoomFactor = 1;
                if (double.TryParse(txtPreviewRotation.Text, out double rotation))
                    rotation = Tools.DegreesToRadians(rotation);
                else
                    rotation = 0;
                if (EditedPatternGroup.PatternsList.Count > 0)
                {
                    var ptn1 = EditedPatternGroup.PatternsList.First();
                    var previewPtn1 = PreviewPatternGroup.PatternsList.First();
                    rotation += ptn1.ZVector.GetArgument() - previewPtn1.ZVector.GetArgument();
                }
                Complex zTransform = Complex.CreateFromModulusAndArgument(zoomFactor, rotation);
                foreach (Pattern ptn in PreviewPatternGroup.Patterns)
                {
                    ptn.ZVector *= zTransform;
                    ptn.ComputeSeedPoints();
                }
            }
            if (!useDesignBackgroundForPreviewToolStripMenuItem.Checked)
                picPreview.BackColor = PreviewPatternGroup.GetPreviewBackgroundColor();
            RedrawPreview();
        }

        private void UseDesignBackgroundForPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (useDesignBackgroundForPreviewToolStripMenuItem.Checked)
                {
                    if (design == null)
                    {
                        useDesignBackgroundForPreviewToolStripMenuItem.Checked = false;
                        return;
                    }
                }
                RedrawPreview();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void RedrawPreview()
        {
            Bitmap bitmap;
            bool useDesignBackground = useDesignBackgroundForPreviewToolStripMenuItem.Checked && design != null;
            Size bitmapSize = picPreview.ClientSize;
            double scaleFac = 1.0;
            if (chkPreviewFullSize.Checked && PreviewPatternGroup.Patterns.Any())
            {
                Pattern pattern = PreviewPatternGroup.Patterns.First();
                scaleFac = 2.0 * pattern.ZVector.GetModulus() / picPreview.ClientSize.Width;
                bitmapSize = new Size((int)(scaleFac * bitmapSize.Width), (int)(scaleFac * bitmapSize.Height));
                pattern.Center = new PointF(bitmapSize.Width / 2, bitmapSize.Height / 2);
            }
            if (useDesignBackground)
            {
                GraphicsPath gpth = null;
                PathGradientBrush pgbr = null;
                bitmap = design.CreateDesignBitmap(bitmapSize.Width, bitmapSize.Height, ref gpth, ref pgbr);
            }
            else
                bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                if (useDesignBackground)
                {
                    foreach (Pattern pattern in design.DesignPatterns.Where(ptn => ptn.IsBackgroundPattern && !ptn.HasPixelRendering))
                    {
                        using (Pattern copy = pattern.GetCopy())
                        {
                            copy.SetForPreview(picPreview.Width / 2);
                            copy.DrawFilled(g, null);
                        }
                    }
                }
                if (PreviewPatternGroup != null)
                {
                    int prevDraftSize = WhorlSettings.Instance.DraftSize;
                    try
                    {
                        if (draftMode)
                            WhorlSettings.Instance.DraftSize = draftSize;
                        PreviewPatternGroup.DrawFilled(g, null, bitmap.Size, checkTilePattern: false,
                                                       draftMode: this.draftMode);
                    }
                    finally
                    {
                        WhorlSettings.Instance.DraftSize = prevDraftSize;
                    }
                }
            }
            if (picPreview.Image != null)
                picPreview.Image.Dispose();
            if (scaleFac != 1.0)
                bitmap = (Bitmap)BitmapTools.ScaleImage(bitmap, picPreview.ClientSize);
            picPreview.Image = bitmap;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                PreviewChanges(updatePattern: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetCurrentColorMode()
        {
            PathFillInfo pathFillInfo = GetEditedPathFillInfo();
            if (pathFillInfo != null)
                currentColorMode = pathFillInfo.ColorMode;
            else
                currentColorMode = FillInfo.PathColorModes.Normal;
        }

        private void picPreview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                const int margin = 5;
                SetCurrentColorMode();
                Rectangle picRect = picPreview.ClientRectangle;
                if (currentColorMode == FillInfo.PathColorModes.Normal
                    || e.X <= margin || e.X >= picRect.Width - margin
                    || e.Y <= margin || e.Y >= picRect.Height - margin)
                {
                    bool changed = mousePoint != PointF.Empty;
                    if (changed)
                    {
                        mousePoint = PointF.Empty;
                        picPreview.Refresh();
                    }
                }
                else
                {
                    mousePoint = new PointF(e.X, e.Y);
                    picPreview.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pen GetLinePen()
        {
            if (picPreview.BackColor == Color.Black)
                return Pens.White;
            else
                return Pens.Black;
        }

        private void DrawCircle(Graphics g, Pen pen, PointF center, int radius)
        {
            if (radius > 0)
            {
                int diameter = 2 * radius;
                g.DrawEllipse(pen, new Rectangle((int)center.X - radius,
                                                 (int)center.Y - radius,
                                                 diameter, diameter));
            }
        }

        private PointF colorModeCenter;

        private void DisplayColorModeLines(Graphics g)
        {
            if (!initialized)
                return;
            Pattern previewPattern = PreviewPattern;
            PathFillInfo pathFillInfo = GetEditedPathFillInfo();
            if (pathFillInfo == null || previewPattern == null)
                return;
            currentColorMode = pathFillInfo.ColorMode;
            PointF center;
            if (previewPattern is Ribbon)
                center = new PointF(picPreview.ClientRectangle.Width / 2,
                                    picPreview.ClientRectangle.Height / 2);
            else
            {
                center = previewPattern.Center;
                PointF axisPoint = new PointF(center.X + (float)previewPattern.ZVector.Re,
                                              center.Y + (float)previewPattern.ZVector.Im);
                DrawCircle(g, Pens.Red, center: axisPoint, radius: 3);
            }
            colorModeCenter = center;
            switch (currentColorMode)
            {
                case FillInfo.PathColorModes.Radial:
                    double size = previewPattern.ZVector.GetModulus();
                    foreach (float pos in pathFillInfo.ColorInfo.Positions)
                    {
                        DrawCircle(g, Pens.LimeGreen, center, (int)((1F - pos) * size));
                    }
                    if (mousePoint != PointF.Empty)
                    {
                        //Draw circle centered on pattern:
                        int radius = (int)(Tools.Distance(center, mousePoint));
                        DrawCircle(g, GetLinePen(), center, radius);
                    }
                    break;
                case FillInfo.PathColorModes.Surround:
                    //double editedAngle = EditedPattern.ZVector.GetArgument();
                    foreach (float pos in pathFillInfo.ColorInfo.Positions)
                    {
                        double angle = 2 * Math.PI * pos; // - editedAngle;
                        Complex vector = previewPattern.ZVector *
                            Complex.CreateFromModulusAndArgument(1D, angle);
                        PointF point = new PointF((float)vector.Re + center.X,
                                                  (float)vector.Im + center.Y);
                        g.DrawLine(Pens.LimeGreen, center, point);
                    }
                    if (mousePoint != PointF.Empty)
                    {
                        //Draw line from center to mouse point:
                        g.DrawLine(GetLinePen(), center, mousePoint);
                    }
                    break;
            }
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (PreviewPatternGroup != null)
                {
                    if (graphPathVerticesToolStripMenuItem.Checked)
                    {
                        if (GraphPathVertices(e.Graphics))
                            return;
                    }
                    if (graphCartesianToolStripMenuItem.Checked)
                    {
                        GraphCartesian(e.Graphics);
                        return;
                    }
                    //PreviewPatternGroup.DrawFilled(e.Graphics, computeRandom: false);
                    Pattern previewPattern = PreviewPattern;
                    if (previewPattern != null)
                    {
                        if (PreviewPatternGroup.Patterns.Count() > 1
                            && showSelectionToolStripMenuItem.Checked)
                            previewPattern.DrawSelectionOutline(e.Graphics);
                        if (chkDisplayColorModeLines.Checked)
                            DisplayColorModeLines(e.Graphics);
                        if (viewInfluencePointsToolStripMenuItem.Checked)
                        {
                            foreach (var influencePointInfo in previewPattern.InfluencePointInfoList.InfluencePointInfos)
                            {
                                influencePointInfo.Draw(e.Graphics, null, this.Font);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool GraphCartesian(Graphics g)
        {
            Pattern previewPattern = PreviewPattern;
            if (previewPattern == null || previewPattern.SeedPoints == null)
                return false;
            double xMax = previewPattern.SeedPoints.Select(
                    p => p.Angle).Max();
            double yMax = previewPattern.SeedPoints.Select(
                    p => p.Modulus).Max();
            double xMin = previewPattern.SeedPoints.Select(
                    p => p.Angle).Min();
            double yMin = previewPattern.SeedPoints.Select(
                    p => p.Modulus).Min();
            double xSpan = xMax - xMin;
            double ySpan = yMax - yMin;
            if (ySpan == 0 || xSpan == 0)
                return false;
            float xFactor = (float)(picPreview.ClientRectangle.Width / xSpan);
            float yFactor = (float)(picPreview.ClientRectangle.Height / ySpan);
            float picHeight = (float)picPreview.ClientRectangle.Height;
            PointF[] points = previewPattern.SeedPoints.Select(
                p => new PointF((float)(p.Angle - xMin) * xFactor,
                                 picHeight - (float)(p.Modulus - yMin) * yFactor)
                              ).ToArray();
            g.DrawLines(Pens.Red, points);
            PointF origin = new PointF(-(float)xMin * xFactor,
                                       picHeight + (float)yMin * yFactor);
            g.DrawLine(Pens.Green,
                new PointF(origin.X - 2, origin.Y),
                new PointF(origin.X + 2, origin.Y));
            g.DrawLine(Pens.Green,
                new PointF(origin.X, origin.Y - 2),
                new PointF(origin.X, origin.Y + 2));
            return true;
        }

        private bool GraphPathVertices(Graphics g)
        {
            Pattern previewPattern = PreviewPattern;
            if (previewPattern == null)
                return false;
            PathOutline pathOtl =
                (from otl in previewPattern.BasicOutlines
                 where otl is PathOutline
                 select (PathOutline)otl).Where(potl => potl.UseVertices).FirstOrDefault();
            if (pathOtl == null)
                return false;
            pathOtl.AddVertices();
            IEnumerable<PointF> vertices = pathOtl.PathVertices;
            if (vertices.Count() == 0)
                return false;
            double xMax = vertices.Select(p => p.X).Max();
            double yMax = vertices.Select(p => p.Y).Max();
            double xMin = vertices.Select(p => p.X).Min();
            double yMin = vertices.Select(p => p.Y).Min();
            double xSpan = xMax - xMin;
            double ySpan = yMax - yMin;
            if (xSpan == 0) xSpan = 1;
            if (ySpan == 0) ySpan = 1;
            float picHeight = (float)picPreview.ClientRectangle.Height;
            float xFactor = 0.9F * (float)(picPreview.ClientRectangle.Width / xSpan);
            float yFactor = 0.9F * (float)(picHeight / ySpan);
            float factor = Math.Min(xFactor, yFactor);
            float offset = 5F;
            PointF[] points = vertices.Select(
                p => new PointF((float)(p.X - xMin) * factor + offset,
                                picHeight - (float)(p.Y - yMin) * factor - offset)).ToArray();
            for (int i = 0; i < points.Length - 1; i++)
            {
                g.DrawLine(Pens.Red, points[i], points[i + 1]);
            }
            PointF origin = new PointF(-(float)xMin * factor + offset,
                                       picHeight + (float)yMin * factor - offset);
            g.DrawLine(Pens.Green,
                new PointF(origin.X - 2, origin.Y),
                new PointF(origin.X + 2, origin.Y));
            g.DrawLine(Pens.Green,
                new PointF(origin.X, origin.Y - 2),
                new PointF(origin.X, origin.Y + 2));
            return true;
        }

        private void savePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (PreviewPatternGroup == null)
                    return;
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Pattern xml file (*.ptn)|*.ptn";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    XmlTools.WriteToXml(dlg.FileName, PreviewPatternGroup, "PatternGroup");
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void openPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Pattern xml file (*.ptn)|*.ptn";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    EditedPatternGroup = new PatternList(design);
                    Tools.ReadFromXml(dlg.FileName, EditedPatternGroup, "PatternGroup");
                    EditedPatternIndex = 0;
                    PopulateControls();
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Color? ChooseColor(Color? defaultColor = null)
        {
            Color? retColor;
            ColorDialog dlg = new ColorDialog();
            if (defaultColor != null)
                dlg.Color = (Color)defaultColor;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (defaultColor != null)
                    retColor = ColorNode.GetColor((Color)defaultColor, dlg.Color);
                else
                    retColor = dlg.Color;
            }
            else
                retColor = null;
            return retColor;
        }

        private bool EditColor(Button colorButton)
        {
            Color? color = ChooseColor(colorButton.BackColor);
            if (color != null)
                colorButton.BackColor = (Color)color;
            return color != null;
        }

        private void btnSwapColors_Click(object sender, EventArgs e)
        {
            PathFillInfo pathFillInfo = GetEditedPathFillInfo();
            if (pathFillInfo != null)
            {
                Color saveColor = pathFillInfo.BoundaryColor;
                pathFillInfo.BoundaryColor = btnBoundaryColor.BackColor =
                    pathFillInfo.CenterColor;
                pathFillInfo.CenterColor = btnCenterColor.BackColor = saveColor;
                //PopulateControls();
                PreviewChanges();
            }
        }

        private void editNextPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditedPatternIndex++;
        }

        private void editPreviousPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditedPatternIndex--;
        }

        private void equalizeCentersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Pattern> editedPatterns = EditedPatternGroup.PatternsList;
            if (editedPatterns.Count <= 1)
                return;
            PointF center = editedPatterns[0].Center;
            for (int i = 1; i < editedPatterns.Count; i++)
                editedPatterns[i].Center = center;
            PreviewChanges();
        }

        /// <summary>
        /// Change type of edited pattern.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboPatternType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!handleControlEvents)
                return;
            try
            {
                if (!initialized)
                    return;
                List<Pattern> editedPatterns = EditedPatternGroup.PatternsList;
                if (EditedPatternIndex >= editedPatterns.Count)
                    return;
                handleControlEvents = false;
                string patternTypeName = (string)cboPatternType.SelectedItem;
                string typeName = patternTypeName == "Path" ?
                                  nameof(PathPattern) :
                                  patternTypeName == "Text" ?
                                  nameof(StringPattern) :
                                  patternTypeName;
                if (editedPatterns.Count != 1)
                {
                    if (typeName != nameof(Pattern))
                    {
                        MessageBox.Show("Only a single pattern can be a ribbon or a path.",
                                        "Message");
                        cboPatternType.SelectedItem = nameof(Pattern);
                    }
                    return;
                }
                Pattern pattern1 = editedPatterns[EditedPatternIndex];
                Pattern newPattern = null;
                Ribbon newRibbon = null;
                switch (typeName)
                {
                    case nameof(Pattern):
                        if (pattern1.GetType() != typeof(Pattern))
                        {
                            newPattern = new Pattern(pattern1, isRecursivePattern: false);
                        }
                        break;
                    case nameof(Ribbon):
                        if (!(pattern1 is Ribbon))
                        {
                            newRibbon = new Ribbon(pattern1);
                            newPattern = newRibbon;
                        }
                        break;
                    case nameof(PathPattern):
                        if (!(pattern1 is PathPattern))
                        {
                            PathPattern pathPattern = new PathPattern(pattern1);
                            pathPattern.ComputeSeedPoints();
                            newPattern = pathPattern;
                        }
                        break;
                    case nameof(StringPattern):
                        if (!(pattern1 is StringPattern))
                        {
                            newPattern = new StringPattern(pattern1);
                        }
                        break;
                }
                if (newPattern == null)
                    return;
                editedPatterns[EditedPatternIndex] = newPattern;
                if (newRibbon != null)
                    PopulateRibbonControls(newRibbon);
                else if (newPattern is PathPattern || newPattern is StringPattern)
                {
                    EditedPatternGroup.SetProperties();
                }
                tabRibbon.Enabled = tabRibbonFormula.Enabled = newRibbon != null;
                PopulateControls();
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                handleControlEvents = true;
            }
        }

        private Button clickedColorButton;

        private void chooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainForm.SelectColorForm.Initialize();
                if (MainForm.SelectColorForm.ShowDialog() == DialogResult.OK)
                {
                    clickedColorButton.BackColor =
                        ColorNode.GetColor(clickedColorButton.BackColor, (Color)MainForm.SelectColorForm.SelectedColor);
                    SetPatternColor();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void addColorToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Color newColor = clickedColorButton.BackColor;
                if (!MainForm.SelectColorForm.PatternChoices.AddColor(newColor))
                    MessageBox.Show("This color has already been added to the color choices.", "Message");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetPatternColor()
        {
            if (addColorModeColor != null)
            {
                if (clickedColorButton == this.btnBoundaryColor)
                    SetColorModeColor();
                return;
            }
            PathFillInfo pathFillInfo = GetEditedPathFillInfo();
            if (pathFillInfo != null)
            {
                bool setCenter = clickedColorButton == this.btnCenterColor;
                if (setCenter)
                    pathFillInfo.CenterColor = clickedColorButton.BackColor;
                else
                    pathFillInfo.BoundaryColor = clickedColorButton.BackColor;
                PreviewChanges();
                //if (previewPattern != null)
                //{
                //    if (setCenter)
                //        previewPattern.CenterColor = clickedColorButton.BackColor;
                //    else
                //        previewPattern.BoundaryColor = clickedColorButton.BackColor;
                //    PreviewWithoutUpdate();
                //}
            }
        }

        private void ColorButton_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                clickedColorButton = (Button)sender;
                pathColorType = clickedColorButton == btnCenterColor ?
                                PathColorTypes.Center : PathColorTypes.Boundary;
                if (e.Button == MouseButtons.Left)
                {
                    if (EditColor((Button)sender))
                    {
                        SetPatternColor();
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Show(clickedColorButton, e.X, e.Y);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void showSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picPreview.Invalidate();
        }

        private ColorTransparencyForm transparencyForm = null;

        private void setTransparencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (transparencyForm == null || transparencyForm.IsDisposed)
                {
                    transparencyForm = new ColorTransparencyForm();
                    transparencyForm.Initialize(this);
                    transparencyForm.Show();
                }
                else
                    transparencyForm.WindowState = FormWindowState.Normal;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PatternForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                if (transparencyForm != null && !transparencyForm.IsDisposed)
                    transparencyForm.Close();
                if (editedLayers != null)
                    editedLayers.LayersChanged -= PatternLayerChanged;
            }
        }

        //private static TransformFormulaForm transformForm = null;

        //private void InitTransformForm()
        //{
        //    if (transformForm == null || transformForm.IsDisposed)
        //        transformForm = new TransformFormulaForm();
        //}

        private void BindTransformsGrid()
        {
            this.patternTransformBindingSource.DataSource = null;
            this.patternTransformBindingSource.DataSource = EditedPattern.Transforms;
            this.dgvTransforms.DataSource = patternTransformBindingSource;
        }

        private bool EditTransform(PatternTransform transform, bool forAdd = false)
        {
            //.Where(p => p is Parameter || p is VarFunctionParameter).ToList();
            int transformCount = EditedPattern.Transforms.Count;
            if (forAdd)
                transformCount++;
            using (var frm = new OutlineFormulaForm())
            {
                frm.Initialize(transform, transformCount);
                bool retVal = (frm.ShowDialog() == DialogResult.OK);
                if (retVal)
                {
                    ArrayParameter arrayParam =
                        transform.TransformSettings.GetParameter(PatternTransform.VertexAnglesParameterName) as ArrayParameter;
                    if (arrayParam != null)
                    {
                        List<double> vertexAngles = EditedPattern.GetVertexAngles();
                        if (vertexAngles != null)
                        {
                            transform.SetVertexAnglesParameter(arrayParam, vertexAngles);
                        }
                    }
                    AddParameterControls(pnlTransformParameters, transform.TransformSettings);
                }
                return retVal;
            }
        }

        private void btnAddTransform_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = EditedPattern;
                PatternTransform newTransform = new PatternTransform(pattern);
                newTransform.SequenceNumber = EditedPattern.Transforms.Count;
                if (EditTransform(newTransform, forAdd: true))
                {
                    pattern.Transforms.Add(newTransform);
                    BindTransformsGrid();
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private PatternForm editRibbonForm;

        private void editPathRibbonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PathPattern pathPattern = EditedPattern as PathPattern;
                Ribbon ribbon = pathPattern?.PathRibbon;
                if (ribbon == null)
                    return;
                if (editRibbonForm == null || editRibbonForm.IsDisposed)
                    editRibbonForm = new PatternForm();
                PatternList ptnList = new PatternList(design);
                ptnList.AddPattern(ribbon);
                editRibbonForm.Initialize(ptnList, selectPatternForm, design, forPathRibbon: true);
                if (editRibbonForm.ShowDialog() == DialogResult.OK)
                {
                    pathPattern.PathRibbon =
                        editRibbonForm.EditedPatternGroup.Patterns.FirstOrDefault() as Ribbon;
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void clearPathRibbonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PathPattern pathPattern = EditedPattern as PathPattern;
                if (pathPattern != null)
                {
                    pathPattern.PathRibbon = null;
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void graphCartesianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void graphPathVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        /// <summary>
        /// Add single pattern to choices.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addPatternToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (EditedPatternGroup.Patterns.Count() == 1)
                    MainForm.AddPatternGroupToChoices(EditedPatternGroup);
                else
                {
                    PatternList ptnList = new PatternList(design);
                    ptnList.AddPattern(EditedPattern);
                    MainForm.AddPatternGroupToChoices(ptnList);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void addPatternGroupToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainForm.AddPatternGroupToChoices(EditedPatternGroup);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void addPatternGroupToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainForm.AddPatternGroupToChoices(EditedPatternGroup, MainForm.ClipboardPatterns);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private void chkCartesianPath_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                PathPattern pathPattern = EditedPattern as PathPattern;
                if (chkCartesianPath.Checked)
                {
                    if (pathPattern == null)
                    {
                        chkCartesianPath.Checked = false;
                        MessageBox.Show("Please change Pattern Type to Path.");
                        return;
                    }
                    if (CurrentCartesianPathOutline == null)
                    {
                        CurrentCartesianPathOutline = new PathOutline();
                        CurrentCartesianPathOutline.PathOutlineType = PathOutlineTypes.Cartesian;
                    }
                    pathPattern.CartesianPathOutline = CurrentCartesianPathOutline;
                }
                else if (pathPattern != null)
                    pathPattern.CartesianPathOutline = null;
                PopulateControls();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private PathVerticesFormulaForm verticesForm;

        private void EditCartesianFormula(PathOutline cartesianOutline)
        {
            using (var frm = new OutlineFormulaForm())
            {
                FormulaSettings formulaSettings = cartesianOutline.GetFormulaSettings();
                frm.Initialize(cartesianOutline);
                frm.ShowDialog();
            }
            //if (verticesForm == null || verticesForm.IsDisposed)
            //    verticesForm = new PathVerticesFormulaForm();
            //verticesForm.Initialize(cartesianOutline.VerticesSettings);
            //if (verticesForm.ShowDialog() == DialogResult.OK)
            //    cartesianOutline.AddVertices();
        }

        private void newPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList ptnList = new PatternList(design);
                ptnList.AddPattern(new Pattern(design, FillInfo.FillTypes.Path));
                Initialize(ptnList, selectPatternForm, design);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnBrowseTextureImageFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Image file (*.png;*.jpg)|*.png;*.jpg;*.jpeg";
                dlg.InitialDirectory = Tools.GetTexturesFolder();
                //string defaultFolder = WhorlSettings.Instance.FilesFolder;
                //if (Directory.Exists(defaultFolder))
                //    dlg.InitialDirectory = defaultFolder;
                if (!string.IsNullOrEmpty(txtTextureImageFileName.Text))
                    dlg.FileName = Path.GetFileName(txtTextureImageFileName.Text);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtTextureImageFileName.Text = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void FillTypeOnChanged(FillInfo.FillTypes fillType)
        {
            pnlPathFill.Enabled = fillType == FillInfo.FillTypes.Path;
            pnlTextureFill.Enabled = fillType == FillInfo.FillTypes.Texture;
        }

        private void cboFillType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                FillInfo.FillTypes fillType = (FillInfo.FillTypes)cboFillType.SelectedItem;
                FillTypeOnChanged(fillType);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void editPatternLayersToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Pattern pattern = this.EditedPattern;
        //        if (pattern == null)
        //            return;
        //        using (var patternLayersForm = new PatternLayersForm())
        //        {
        //            patternLayersForm.Initialize(pattern);
        //            if (patternLayersForm.ShowDialog() == DialogResult.OK)
        //            {
        //                this.EditedPattern = patternLayersForm.EditedPattern;
        //                PreviewChanges();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private void deleteCurrentPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (editedPatternIndex < 0 ||
                    editedPatternIndex >= EditedPatternGroup.PatternsList.Count ||
                    EditedPatternGroup.PatternsList.Count <= 1)
                    return;
                EditedPatternGroup.PatternsList.RemoveAt(editedPatternIndex);
                if (editedPatternIndex >= EditedPatternGroup.PatternsList.Count)
                    editedPatternIndex = EditedPatternGroup.PatternsList.Count - 1;
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private PathFillInfo GetEditedPathFillInfo()
        {
            IFillInfo iFillInfo = fillInfoParent;
            return iFillInfo?.FillInfo as PathFillInfo;
        }

        private PathFillInfo.ColorPositions GetEditedPatternColorInfo()
        {
            PathFillInfo pathFillInfo = GetEditedPathFillInfo();
            return pathFillInfo?.ColorInfo;
        }

        private float GetColorModePosition()
        {
            float position = 0;
            Pattern previewPattern = PreviewPattern;
            if (previewPattern == null || previewPattern.ZVector == Complex.Zero)
                return position;
            SetCurrentColorMode();
            switch (currentColorMode)
            {
                case FillInfo.PathColorModes.Radial:
                    double radius = Tools.Distance(colorModeCenter, mousePoint);
                    double modulus = previewPattern.ZVector.GetModulus();
                    //if (chkEditLayers.Checked && editedLayer != null)
                    //    modulus *= editedLayer.ModulusRatio;
                    if (modulus > 0)
                        position = 1F - (float)(radius / modulus);
                    break;
                case FillInfo.PathColorModes.Surround:
                    //Find angle of mouse vector relative to angle of pattern vector:
                    Complex mouseVec = new Complex(mousePoint.X - colorModeCenter.X,
                                                   mousePoint.Y - colorModeCenter.Y);
                    mouseVec /= previewPattern.ZVector;  //Subtract ZVector angle.
                    double angle = Tools.AdjustAngle(mouseVec.GetArgument());
                    position = (float)(angle / (2 * Math.PI));
                    break;
            }
            position = Math.Max(0, Math.Min(1, position));
            return position;
        }

        private float colorModePosition;
        private bool? addColorModeColor = null;

        private void InitColorModeColorChoice(bool addColor)
        {
            if (currentColorMode == FillInfo.PathColorModes.Normal)
                return;
            bool setControls = addColorModeColor == null;
            addColorModeColor = addColor;
            colorModePosition = GetColorModePosition();
            var colorInfo = GetEditedPatternColorInfo();
            if (colorInfo != null)
            {
                float pos = colorModePosition;
                int index = colorInfo.FindNearestIndex(ref pos);
                if (index >= 0)
                    btnBoundaryColor.BackColor = colorInfo.GetColorAtIndex(index);
            }
            if (setControls)
                SetColorModeColorChoiceControls(isColorMode: true);
        }

        private void CancelColorModeColorChoice()
        {
            if (addColorModeColor == null)
                return;
            addColorModeColor = null;
            SetColorModeColorChoiceControls(isColorMode: false);
            var pathFillInfo = GetEditedPathFillInfo();
            if (pathFillInfo != null)
                btnBoundaryColor.BackColor = pathFillInfo.BoundaryColor;
        }

        private void SetColorModeColorChoiceControls(bool isColorMode)
        {
            lblBoundaryColor.Text = isColorMode ?
                $"{currentColorMode} Color:" : "Boundary Color:";
            btnCenterColor.Visible = lblCenterColor.Visible = !isColorMode;
            btnSwapColors.Enabled = !isColorMode;
            btnSetColor.Visible = btnCancelColor.Visible = isColorMode;
        }

        private void SetColorModeColor(bool previewChanges = true)
        {
            if (addColorModeColor == null)
                return;
            Color color = btnBoundaryColor.BackColor;
            var colorInfo = GetEditedPatternColorInfo();
            if (colorInfo == null)
                return;
            float position;
            if (addColorModeColor == false)
            {
                position = colorModePosition;
                int index = colorInfo.FindNearestIndex(ref position);
                if (index < 0)
                    return;
                colorInfo.SetColorAtIndex((Color)color, index);
            }
            else
            {
                position = (float)Math.Round(colorModePosition, 2);
                colorInfo.AddOrSetColorAtPosition((Color)color, position);
            }
            if (currentColorMode == FillInfo.PathColorModes.Surround
                && (position == 0 || position == 1))
            {
                var pathFillInfo = GetEditedPathFillInfo();
                if (pathFillInfo != null)
                    pathFillInfo.BoundaryColor = color;
            }
            if (previewChanges)
            {
                CancelColorModeColorChoice();
                PreviewChanges();
            }
        }

        private void btnSetColor_Click(object sender, EventArgs e)
        {
            try
            {
                SetColorModeColor();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancelColor_Click(object sender, EventArgs e)
        {
            try
            {
                CancelColorModeColorChoice();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnReseedRandomWaves_Click(object sender, EventArgs e)
        {
            try
            {
                var pathFillInfo = GetEditedPathFillInfo();
                if (pathFillInfo != null)
                {
                    if (pathFillInfo.ColorMode == FillInfo.PathColorModes.Normal)
                        pathFillInfo.ColorMode = FillInfo.PathColorModes.Radial;
                    pathFillInfo.ColorInfo.WaveRandomGenerator.ReseedRandom();
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        /// <summary>
        /// Set ColorMode color for edited pattern.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                InitColorModeColorChoice(addColor: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void addColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                InitColorModeColorChoice(addColor: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void deleteColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var colorInfo = GetEditedPatternColorInfo();
                if (colorInfo == null || currentColorMode == FillInfo.PathColorModes.Normal)
                    return;
                float position = GetColorModePosition();
                colorInfo.DeleteColorAtPosition(position);
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }

        private void chkDisplayColorModeLines_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                SetCurrentColorMode();
                picPreview.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PopulateRenderLayerComboBox()
        {
            Pattern pattern = EditedPattern;
            if (pattern == null)
                return;
            int layerCount = pattern.PatternLayers.PatternLayers.Count;
            try
            {
                handleControlEvents = false;
                object prevVal = cboRenderLayerIndex.SelectedItem;
                cboRenderLayerIndex.DataSource = Enumerable.Range(1, layerCount).ToList();
                if (prevVal is int && (int)prevVal <= layerCount)
                    cboRenderLayerIndex.SelectedItem = prevVal;
            }
            finally
            {
                handleControlEvents = true;
            }
        }

        private void dgvLayers_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            try
            {
                if (!(handleControlEvents && chkEditLayers.Checked))
                    return;
                if (e.RowIndex + e.RowCount > editedLayers.PatternLayers.Count)
                    return;
                if (MessageBox.Show($"Remove {e.RowCount} layers?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                editedLayers.PatternLayers.RemoveRange(e.RowIndex, e.RowCount);
                editedLayers.RaiseLayersChanged();
                //PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void dgvLayers_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        //{
        //    try
        //    {
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private int currentLayerIndex { get; set; } = -1;

        private void dgvLayers_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!chkEditLayers.Checked)
                    return;
                if (e.RowIndex < 0)
                    return;
                DataRowView drv = dgvLayers.Rows[e.RowIndex].DataBoundItem as DataRowView;
                DataRow dRow = drv?.Row;
                if (dRow == null ||
                    dRow.RowState == DataRowState.Detached ||
                    dRow.RowState == DataRowState.Deleted)
                    return;
                var layer = dRow[1] as PatternLayer;
                if (layer == null)
                {
                    layer = new PatternLayer(editedLayers);
                    layer.FillInfo = new PathFillInfo(EditedPattern);
                    dRow[1] = layer;
                }
                editedLayer = layer;
                currentLayerIndex = e.RowIndex;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PatternLayerChanged(object sender, EventArgs e)
        {
            if (!chkEditLayers.Checked)
                return;
            PopulateRenderLayerComboBox();
            PreviewChanges();
        }

        private void PopulateLayerControls()
        {
            if (!chkEditLayers.Checked)
                return;
            CancelColorModeColorChoice();
            PatternLayer layer = this.editedLayer;
            this.cboFillType.Enabled = this.pnlPathFill.Enabled =
                                       this.pnlTextureFill.Enabled = layer != null;
            if (layer == null)
                return;
            if (layer.FillInfo == null)
                layer.FillInfo = new PathFillInfo(EditedPattern);
            this.cboFillType.SelectedItem = layer.FillInfo.FillType;
            FillTypeOnChanged(layer.FillInfo.FillType);
            PopulateFillControls(layer.FillInfo);
        }

        private void chkEditLayers_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!initialized)
                    return;
                this.dgvLayers.Enabled = chkEditLayers.Checked;
                if (chkEditLayers.Checked)
                {
                    Pattern editedPattern = EditedPattern;
                    if (editedPattern != null)
                    {
                        editedLayer = editedPattern.PatternLayers.PatternLayers.FirstOrDefault();
                    }
                }
                else
                    editedLayer = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnEditPatternLayerInMainForm_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentLayerIndex == -1)
                {
                    MessageBox.Show("Please select a Pattern Layer first.");
                    return;
                }
                if (editedLayers != null)
                {
                    editedLayers.EditedIndex = currentLayerIndex;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        PatternForm editRecursionPatternForm = null;

        private void EditRecursionPattern(Pattern recursionPattern)
        {
            PatternList ptnList = new PatternList(design);
            ptnList.AddPattern(recursionPattern);
            if (editRecursionPatternForm == null || editRecursionPatternForm.IsDisposed)
                editRecursionPatternForm = new PatternForm();
            editRecursionPatternForm.Initialize(ptnList, selectPatternForm, design, forRecursionPattern: true);
            if (editRecursionPatternForm.ShowDialog() == DialogResult.OK)
            {
                int ind = EditedPattern.Recursion.RecursionPatterns.IndexOf(recursionPattern);
                EditedPattern.Recursion.RecursionPatterns[ind] =
                    editRecursionPatternForm.EditedPatternGroup.Patterns.First();
                PreviewChanges();
            }
        }

        private void btnAddRecursionPattern_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = EditedPattern;
                Pattern recursionPattern =
                    pattern.Recursion.AddRecursionPattern(origPattern: EditedPattern);
                cboRecursionPatternIndex.DataSource = Enumerable.Range(1, pattern.Recursion.RecursionPatterns.Count).ToList();
                EditRecursionPattern(recursionPattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnDeleteRecursivePattern_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboRecursionPatternIndex.SelectedItem == null)
                    return;
                int ind = (int)cboRecursionPatternIndex.SelectedItem - 1;
                Pattern pattern = EditedPattern;
                pattern.Recursion.RecursionPatterns.RemoveAt(ind);
                cboRecursionPatternIndex.SelectedItem = null;
                cboRecursionPatternIndex.DataSource = Enumerable.Range(1, pattern.Recursion.RecursionPatterns.Count).ToList();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnEditRecursionPattern_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboRecursionPatternIndex.SelectedItem == null)
                    return;
                int ind = (int)cboRecursionPatternIndex.SelectedItem - 1;
                Pattern recursionPattern = EditedPattern.Recursion.RecursionPatterns[ind];
                EditRecursionPattern(recursionPattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtRecursiveRepetitions_Leave(object sender, EventArgs e)
        {
            try
            {
                int repetitions;
                if (!int.TryParse(txtRecursiveRepetitions.Text, out repetitions))
                    return;
                Pattern pattern = EditedPattern;
                if (pattern == null || pattern.Recursion.Repetitions == repetitions)
                    return;
                pattern.Recursion.Repetitions = repetitions;
                dgvPatternRecursionInfo_Bind(pattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void autosampleForRecursionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = EditedPattern;
                if (pattern == null || !pattern.Recursion.IsRecursive)
                {
                    MessageBox.Show("The current pattern is not recursive.");
                    return;
                }
                var messages = new List<string>();
                UpdateRecursionAutoFactor(messages);
                if (messages.Count > 0)
                {
                    MessageBox.Show(messages[0]);
                    return;
                }
                pattern.RecursionAutoSample();
                this.dgvPatternRecursionInfo_Bind(pattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void choosePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Changes to the current pattern will be lost.", "Confirm", MessageBoxButtons.OKCancel)
                                    != DialogResult.OK)
                    return;
                if (selectPatternForm == null)
                    return;
                selectPatternForm.Initialize();
                if (selectPatternForm.ShowDialog() == DialogResult.OK)
                {
                    Initialize(selectPatternForm.SelectedPatternGroup, selectPatternForm, design);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setPatternFromDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (design == null)
                    return;
                if (MessageBox.Show("Changes to the current pattern will be lost.", "Confirm", MessageBoxButtons.OKCancel)
                                    != DialogResult.OK)
                    return;
                Initialize(design.DefaultPatternGroup, selectPatternForm, design);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setPatternAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (design != null)
                    design.DefaultPatternGroup = EditedPatternGroup.GetCopy();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnClipRibbonSegments_Click(object sender, EventArgs e)
        {
            try
            {
                var ribbon = EditedPattern as Ribbon;
                if (ribbon == null)
                    return;
                int clipCount = (int)this.cboClipSegmentCount.SelectedItem;
                if (clipCount > ribbon.RibbonPath.Count - 1)
                    return;
                ribbon.RibbonPath.RemoveRange(ribbon.RibbonPath.Count - clipCount, clipCount);
                MessageBox.Show("Clipped the ribbon's path.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnEditRibbonFormula_Click(object sender, EventArgs e)
        {
            try
            {
                var ribbon = EditedPattern?.GetRibbon();
                if (ribbon == null)
                    return;
                ribbon.InitFormulaSettings();
                using (var frm = new OutlineFormulaForm())
                {
                    frm.Initialize(ribbon);
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        AddParameterControls(pnlRibbonFormulaParameters, ribbon.FormulaSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void cboDrawingMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboDrawingMode.SelectedItem != null)
                {
                    var drawingMode = (RibbonDrawingModes)cboDrawingMode.SelectedItem;
                    txtRibbonAmpOffset.Enabled = txtRibbonAngleOffset.Enabled = drawingMode == RibbonDrawingModes.DirectFill;
                    btnEditRibbonCopiedPattern.Enabled = drawingMode == RibbonDrawingModes.CopyPattern;
                    if (handleControlEvents)
                        PreviewChanges(updatePattern: true);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void viewDebugMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.ViewDebugMessages();
        }

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.RefreshDisplay)
                PreviewChanges();
        }

        private void OnParameterActionSelected(object sender, ParameterActionEventArgs e)
        {
            try
            {
                FormulaSettings formulaSettings = e.BaseParameterDisplay.FormulaSettings;
                if (!formulaSettings.IsCSharpFormula && !formulaSettings.Parameters.Any())
                {
                    MessageBox.Show("There are no numeric parameters to act on.");
                    return;
                }
                ShowPropertyActionsForm(formulaSettings, e.BaseParameterDisplay.SelectedLabel);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        frmVaryProperties frmVaryProperties;

        private void ShowPropertyActionsForm(object targetObject, Label parameterLabel = null)
        {
            if (targetObject == null)
                return;
            Pattern targetPattern = EditedPattern;
            if (targetPattern == null)
                return;
            if (frmVaryProperties == null || frmVaryProperties.IsDisposed)
                frmVaryProperties = new frmVaryProperties();
            var formulaSettings = targetObject as FormulaSettings;
            if (formulaSettings != null)
                frmVaryProperties.InitializeForParameters(formulaSettings, targetPattern, this, parameterLabel);
            else
                frmVaryProperties.Initialize(targetObject, targetPattern, this);
            Tools.DisplayForm(frmVaryProperties, this);
        }

        public void PopulateControlsForPattern(Pattern pattern, bool refreshParameters)
        {
            if (pattern == EditedPattern)
            {
                PopulateControls();
                if (refreshParameters)
                {
                    Panel pnlParameters = GetTabPageParametersPanel(tabControl1.SelectedTab);
                    if (pnlParameters != null)
                    {
                        RefreshParameters(pnlParameters, pattern);
                    }
                }
            }
            else if (!EditedPatternGroup.PatternsList.Contains(pattern))
                return;
            PreviewChanges();
        }

        private void RefreshParameters(Panel pnlParameters, Pattern sourcesPattern = null)
        {
            var paramsDisplay =
                parameterDisplaysContainer.GetParameterDisplays(pnlParameters, throwException: true).BaseParameterDisplay;
            parameterDisplaysContainer.AddParametersControls(pnlParameters, paramsDisplay.FormulaSettings,
                                                             sourcesPattern);
        }

        private void propertyActionsFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ShowPropertyActionsForm(EditedPattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void btnEditColorGradient_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Pattern pattern = EditedPattern;
        //        if (pattern?.PixelRendering != null)
        //        {
        //            pattern.PixelRendering.EditColorGradient();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private void chkPixelRendering_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleControlEvents)
                    return;
                Pattern pattern = EditedPattern;
                if (pattern == null)
                    return;
                bool enabled = chkPixelRendering.Checked;
                bool wasNull = pattern.PixelRendering == null;
                if (enabled && wasNull)
                {
                    string errMessage = pattern.CheckCreatePixelRendering();
                    if (errMessage != null)
                    {
                        handleControlEvents = false;
                        chkPixelRendering.Checked = false;
                        handleControlEvents = true;
                        MessageBox.Show(errMessage);
                        return;
                    }
                }
                if (pattern.PixelRendering != null)
                {
                    pattern.PixelRendering.Enabled = enabled;
                    if (wasNull)
                    {
                        pattern.PixelRendering.ColorNodes = pattern.FillInfo.GetColorNodes();
                    }
                    colorGradientFormComponent.ColorNodes = pattern.PixelRendering.ColorNodes;
                }
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void AddParameterControls(Panel panel, FormulaSettings formulaSettings,
                                          Pattern sourcesPattern = null)
        {
            parameterDisplaysContainer.AddParametersControls(panel, formulaSettings, sourcesPattern);
        }

        private void ClearParameterControls(Panel panel)
        {
            BaseParameterDisplay.ClearParametersControls(panel);
        }

        private void AddRenderingParameterControls()
        {
            Pattern pattern = EditedPattern;
            if (pattern?.PixelRendering == null)
                return;
            AddParameterControls(pnlRenderingParameters, pattern.PixelRendering.FormulaSettings, pattern);
        }

        private void btnEditColorFormula_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = EditedPattern;
                if (pattern == null || pattern.PixelRendering == null)
                    return;
                pattern.PixelRendering.CheckCreateFormulaSettings();
                using (var frm = new OutlineFormulaForm())
                {
                    frm.Initialize(pattern.PixelRendering);
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        AddRenderingParameterControls();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void RefreshTabPage()
        {
            var pnlParameters = GetTabPageParametersPanel(tabControl1.SelectedTab);
            if (pnlParameters != null)
            {
                parameterDisplaysContainer.RefreshComboBoxes(pnlParameters);
            }
            //if (tabControl1.SelectedTab.Name == "tabRendering")
            //{
            //    Pattern pattern = EditedPattern;
            //    if (pattern == null || pattern.PixelRendering?.FormulaSettings == null)
            //        return;
            //    var paramsDisplay = tabControl1.SelectedTab.Tag as BaseParameterDisplay;
            //    if (paramsDisplay != null)
            //        paramsDisplay.RefreshComboBoxes();
            //    //if (pattern.PixelRendering.FormulaSettings.IsCSharpFormula)
            //    //    renderParameterDisplay.RefreshComboBoxes(pattern.PixelRendering.FormulaSettings);
            //    //else
            //    //    parameterDisplay.RefreshComboBoxes(pnlRenderingParameters, pattern.PixelRendering.FormulaSettings);
            //}
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefreshTabPage();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PatternForm_Activated(object sender, EventArgs e)
        {
            try
            {
                RefreshTabPage();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void chkEditColorGradient_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                picGradient.Visible = chkEditColorGradient.Checked;
                if (picGradient.Visible)
                {
                    pnlRenderingParameters.Height = picGradient.Top - pnlRenderingParameters.Top - 3;
                }
                else
                {
                    pnlRenderingParameters.Height = picGradient.Bottom - pnlRenderingParameters.Top + 1;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnTextFont_Click(object sender, EventArgs e)
        {
            try
            {
                StringPattern stringPattern = EditedPattern as StringPattern;
                if (stringPattern == null)
                    return;
                var fontDlg = new FontDialog();
                fontDlg.Font = stringPattern.Font;
                if (fontDlg.ShowDialog() == DialogResult.OK)
                {
                    stringPattern.Font = fontDlg.Font;
                    PreviewChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SizeRectangleToImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = EditedPattern;
                if (pattern == null)
                    return;
                BasicOutline rectOutline = pattern.GetSingleBasicOutline(BasicOutlineTypes.Rectangle);
                if (rectOutline == null)
                    return;
                if (pattern.FillInfo.FillBrush == null)
                    pattern.FillInfo.CreateFillBrush();
                TextureBrush textureBrush = pattern.FillInfo.FillBrush as TextureBrush;
                if (textureBrush == null || textureBrush.Image == null)
                    return;
                double aspectRatio = (double)textureBrush.Image.Width / textureBrush.Image.Height;
                rectOutline.AddDenom = aspectRatio;
                PopulateOutlinesControls();
                PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ChkPreviewFullSize_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                PreviewChanges(updatePattern: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ChkUseTestFormula_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!handleControlEvents)
                    return;
                Pattern pattern = EditedPattern;
                if (pattern?.PixelRendering == null)
                    return;
                pattern.PixelRendering.CheckCreateFormulaSettings();
                pattern.PixelRendering.FormulaSettings.TestCSharpEvalType = chkUseTestFormula.Checked ?
                        typeof(WhorlEvalTest.WhorlEvalClass) : null;
                RefreshParameters(pnlRenderingParameters, pattern);
                PreviewChanges(updatePattern: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private int draftSize { get; set; } = 1;

        private bool draftMode
        {
            get { return draftSize > 1; }
        }

        private void SetDraftSize(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            foreach (var itm in draftSizeToolStripMenuItem.DropDownItems)
            {
                var mnuItm = itm as ToolStripMenuItem;
                if (mnuItm != null)
                    mnuItm.Checked = mnuItm == menuItem;
            }
            int size = (int)menuItem.Tag;
            if (draftSize != size)
            {
                draftSize = size;
                PreviewChanges();
            }
        }

        private void chkSmoothedDraft_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (draftMode)
                    PreviewChanges();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void influencePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                picPreview.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PatternForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                PreviewPatternGroup = null;  //Disposes if not null.
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnEditTransformInMainForm_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedTransform == null)
                    return;
                EditedTransform = selectedTransform;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        //private void AddAllParametersControls(Panel pnlParameters, FormulaSettings formulaSettings)
        //{
        //    parameterDisplay.AddAllParametersControls(pnlParameters, formulaSettings);
        //    //ClearParametersControls(pnlParameters);
        //    //int parameterIndex = 0;
        //    //foreach (BaseParameter parameter in formulaSettings.BaseParameters)
        //    //{
        //    //    if (AddParameterControls(pnlParameters, parameter, parameterIndex))
        //    //        parameterIndex++;
        //    //}
        //}

        //const int critTopMargin = 2,
        //          critLeftMargin = 5,
        //          critControlWidth = 85,
        //          critRowHeight = 25,
        //          critColumnWidth = critLeftMargin + 2 * critControlWidth,
        //          critColumnCount = 3;

        ///// <summary>
        ///// Add dynamic controls to panel for one parameter.
        ///// </summary>
        ///// <param name="pnlParameters"></param>
        ///// <param name="baseParameter"></param>
        //private bool AddParameterControls(Panel pnlParameters, BaseParameter baseParameter, int parameterIndex)
        //{
        //    if (baseParameter is CustomParameter)
        //        return false;
        //    int rowIndex, colIndex;
        //    rowIndex = parameterIndex / critColumnCount;
        //    colIndex = parameterIndex % critColumnCount;
        //    Control ctl;
        //    ComboBox cbo = null;
        //    Label lbl = null;
        //    var boolParam = baseParameter as BooleanParameter;
        //    if (boolParam != null)
        //    {
        //        var chk = new CheckBox();
        //        chk.Text = baseParameter.GetLabel();
        //        chk.Checked = boolParam.BooleanValue;
        //        chk.CheckedChanged += ParametersCheckBox_CheckChanged;
        //        ctl = chk;
        //    }
        //    else
        //    {
        //        if (baseParameter.HasChoices)
        //        {
        //            cbo = new ComboBox();
        //            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
        //            cbo.DataSource = baseParameter.ValueChoices.ToList();
        //            ctl = cbo;
        //            ctl.Width = critControlWidth;
        //            lbl = new Label();
        //        }
        //        else
        //        {
        //            var dblParam = baseParameter as DoubleParameter;
        //            if (dblParam == null)
        //                return false;
        //            var txtBox = new TextBox();
        //            txtBox.Text = dblParam.Value == null ? string.Empty : dblParam.Value.ToString();
        //            txtBox.KeyPress += ParametersTextBox_KeyPress;
        //            txtBox.Leave += ParametersTextBox_Leave;
        //            ctl = txtBox;
        //            ctl.Width = critControlWidth;
        //            lbl = new Label();
        //        }
        //    }
        //    int left = critLeftMargin + colIndex * critColumnWidth;
        //    int top = critTopMargin + rowIndex * critRowHeight;
        //    if (lbl != null)
        //    {
        //        lbl.AutoSize = false;
        //        lbl.Text = baseParameter.GetLabel();
        //        int textWidth = TextRenderer.MeasureText(lbl.Text, lbl.Font).Width;
        //        lbl.Width = Math.Min(critControlWidth - critLeftMargin, textWidth);
        //        lbl.Top = top;
        //        left += critControlWidth;
        //        lbl.Left = left - lbl.Width - critLeftMargin;
        //        pnlParameters.Controls.Add(lbl);
        //    }
        //    ctl.Tag = baseParameter;
        //    ctl.Top = top;
        //    ctl.Left = left;
        //    pnlParameters.Controls.Add(ctl);
        //    if (cbo != null)
        //    {
        //        cbo.SelectedItem = baseParameter.GetValueChoice(baseParameter.Value);
        //        cbo.SelectedIndexChanged += ParametersComboBox_SelectedIndexChanged;
        //    }
        //    //baseParameter.ValueChanged += Parameter_OnValueChanged;
        //    return true;
        //}

        ////private void Parameter_OnValueChanged(object sender, EventArgs e)
        ////{
        ////    PreviewChanges();
        ////}

        //private void ClearParametersControls(Panel pnlParameters)
        //{
        //    foreach (Control ctl in pnlParameters.Controls)
        //    {
        //        ctl.Dispose();
        //    }
        //    pnlParameters.Controls.Clear();
        //}

        ////private void SetParametersFromControls(Panel pnlParameters)
        ////{
        ////    foreach (Control ctl in pnlParameters.Controls)
        ////    {
        ////        var txtBox = ctl as TextBox;
        ////        var baseParameter = (BaseParameter)ctl.Tag;
        ////        if (txtBox != null)
        ////        {
        ////            if (double.TryParse(txtBox.Text, out double val))
        ////            {
        ////                baseParameter.Value = val;
        ////            }
        ////            else
        ////            {
        ////                txtBox.Text = baseParameter.Value == null ? string.Empty : baseParameter.Value.ToString();
        ////            }
        ////        }
        ////        else
        ////        {
        ////            var cbo = ctl as ComboBox;
        ////            if (cbo != null)
        ////            {
        ////                if (cbo.SelectedItem != null)
        ////                    baseParameter.SetValueFromParameterChoice((ParameterChoice)cbo.SelectedItem);
        ////            }
        ////            else
        ////            {
        ////                var chkBox = ctl as CheckBox;
        ////                if (chkBox != null)
        ////                {
        ////                    ((BooleanParameter)baseParameter).BooleanValue = chkBox.Checked;
        ////                }
        ////            }
        ////        }
        ////    }
        ////}

        //private void OnParameterChanged()
        //{
        //    EditedPattern.ComputeSeedPoints();
        //    PreviewChanges();
        //}

        //private void viewDebugMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    MainForm.ViewDebugMessages();
        //}

        //private void ParametersCheckBox_CheckChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var chkBox = (CheckBox)sender;
        //        ((BooleanParameter)chkBox.Tag).BooleanValue = chkBox.Checked;
        //        OnParameterChanged();
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void ParametersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var cbo = (ComboBox)sender;
        //        if (cbo.SelectedItem != null)
        //        {
        //            ((BaseParameter)cbo.Tag).SetValueFromParameterChoice((ParameterChoice)cbo.SelectedItem);
        //            OnParameterChanged();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void SetParameterFromTextBox(TextBox txtBox)
        //{
        //    var dblParam = (DoubleParameter)txtBox.Tag;
        //    if (double.TryParse(txtBox.Text, out double val))
        //    {
        //        dblParam.Value = val;
        //        OnParameterChanged();
        //    }
        //    else
        //    {
        //        txtBox.Text = dblParam.Value == null ? string.Empty : dblParam.Value.ToString();
        //    }
        //}

        //private void ParametersTextBox_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    try
        //    {
        //        if (e.KeyChar == '\r')
        //        {
        //            e.Handled = true;
        //            SetParameterFromTextBox((TextBox)sender);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void ParametersTextBox_Leave(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SetParameterFromTextBox((TextBox)sender);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}
    }
}
