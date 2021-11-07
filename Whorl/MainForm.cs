using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Drawing.Imaging;
using ParserEngine;
using static Whorl.Pattern.RenderingInfo;
using System.Security.Cryptography;

namespace Whorl
{
    public partial class MainForm : Form //, IRenderCaller
    {
        private enum DragStates
        {
            None,
            Dragging
        }

        private enum GridTypes
        {
            None,
            Square,
            Circular
        }

        private enum MenuItemTypes
        {
            Normal,
            Paste,
            DistancePattern
        }


        private const int controlMargin = 5;

        //public const int QualitySize = 2000;

        public MainForm(string designFileName = null)
        {
            InitializeComponent();
            try
            {
                DefaultMainForm = this;
                ignoreEvents = true;
                initialDesignFileName = designFileName;
                savedRandomSeed = seededRandom.RandomSeed;
                renderCaller = new RenderCaller(RenderCallback);
                cboDraftSize.DataSource = Enumerable.Range(2, 19).ToList();
                
                var parametersPanels = new Panel[] { pnlParameters };
                parameterDisplaysContainer = new ParameterDisplaysContainer(
                                             parametersPanels, OnParameterChanged, OnParameterActionSelected,
                                             singleColumn: true);
                paramDisplays = parameterDisplaysContainer.GetParameterDisplays(pnlParameters);
                paramDisplays.SetIsCSharp(true);
                cSharpParamsDisplay = paramDisplays.CSharpParameterDisplay;
                cSharpParamsDisplay.FnEditInfluenceLink = EditInfluenceLink;
                cSharpParamsDisplay.UpdateParametersObject = false;
                var paramsDisplay = paramDisplays.ParameterDisplay;
                paramsDisplay.FnEditInfluenceLink = EditInfluenceLink;

                pasteDefaultPatternToolStripMenuItem.Tag = MenuItemTypes.Paste;
                pasteCopiedPatternsToolStripMenuItem.Tag = MenuItemTypes.Paste;

                redrawDistancePatternToolStripMenuItem.Tag = MenuItemTypes.DistancePattern;
                editDistancePatternToolStripMenuItem.Tag = MenuItemTypes.DistancePattern;
                editDistancePatternSettingsToolStripMenuItem.Tag = MenuItemTypes.DistancePattern;
                deleteDistancePatternToolStripMenuItem.Tag = MenuItemTypes.DistancePattern;
                addDistancePatternToClipboardToolStripMenuItem.Tag = MenuItemTypes.DistancePattern;
                showDistanceInfluencePointsToolStripMenuItem.Tag = MenuItemTypes.DistancePattern;

                string customDesignsFolder = Path.Combine(WhorlSettings.Instance.FilesFolder, WhorlSettings.Instance.CustomDesignParentFolder);
                bool folderExists = Directory.Exists(customDesignsFolder);
                openDesignFromFolderToolStripMenuItem.Visible = folderExists;
                if (folderExists)
                {
                    foreach (string folderPath in Directory.EnumerateDirectories(customDesignsFolder))
                    {
                        string thumbnailsFolder = Path.Combine(folderPath, WhorlSettings.Instance.DesignThumbnailsFolder);
                        if (!Directory.Exists(thumbnailsFolder))
                            Directory.CreateDirectory(thumbnailsFolder);

                        string subfolder = Path.GetFileName(folderPath);

                        var menuItem = new ToolStripMenuItem(text: subfolder) { Tag = subfolder };
                        openDesignFromFolderToolStripMenuItem.DropDownItems.Add(menuItem);
                        menuItem.Click += new System.EventHandler(this.openDesignFromFolderToolStripMenuItem_Click);

                        menuItem = new ToolStripMenuItem(text: subfolder) { Tag = subfolder };
                        saveDesignToFolderToolStripMenuItem.DropDownItems.Add(menuItem);
                        menuItem.Click += new System.EventHandler(this.saveDesignToFolderToolStripMenuItem_Click);

                        menuItem = new ToolStripMenuItem(text: subfolder) { Tag = subfolder };
                        moveDesignToFolderToolStripMenuItem.DropDownItems.Add(menuItem);
                        menuItem.Click += new System.EventHandler(this.moveDesignToFolderToolStripMenuItem_Click);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
                Close();
            }
        }

        private bool ignoreEvents;
        public static MainForm DefaultMainForm { get; private set; }
        public static Size ContainerSize { get; private set; }

        //public ToolStripProgressBar DesignProgressBar
        //{
        //    get { return this.toolStripProgressBar1; }
        //}

        private WhorlDesign _design { get; set; }
        public WhorlDesign Design
        {
            get { return _design; }
        }

        private void SetDesign(WhorlDesign newDesign)
        {
            if (newDesign == null)
                throw new NullReferenceException("Cannot set design to null.");
            if (Design != newDesign)
            {
                if (Design != null)
                {
                    Design.DesignLayerList.LayerChanged -= DesignLayerChanged;
                    Design.IsDirtyChanged -= DesignDirtyChanged;
                    Design.EditedPattern = null;
                }
                _design = newDesign;
                InitializeForDesign();
                Design.DesignLayerList.LayerChanged += DesignLayerChanged;
                Design.IsDirtyChanged += DesignDirtyChanged;
                if (selectPatternForm != null)
                    selectPatternForm.Design = Design;
                ClearTileGrid();
            }
            designLayersForm.SetDesign(Design);
        }

        private WhorlDesign origDesign = null;
        private string _designFileName = null;
        private string designFileName
        {
            get { return _designFileName; }
            set
            {
                _designFileName = value;
                WriteTitle();
            }
        }
        //Contains patterns, colors, and formulas user can choose from:
        public static PatternGroupList PatternChoices { get; private set; }
        public static PatternGroupList ClipboardPatterns { get; set; }

        //private PatternGroupList savedPatternChoices;
        private List<WhorlDesign> improvDesigns { get; set; } = new List<WhorlDesign>();
        //private List<int> improvRandomSeeds = new List<int>();
        private Bitmap currentBitmap;
        private Color[] gradientColors = { Color.Red, Color.Yellow, Color.Blue, Color.Green };
        private int colorIndex;
        private int gridSize = 50;
        private GridTypes gridType = GridTypes.Square;
        private DragStates dragState = DragStates.None;
        private Point dragStart;
        private Point dragEnd;
        private Point? lockedCenter = null;
        private Complex spinRotationVector;
        private Complex swayRotationVector;
        private Complex revolveRotationVector;
        private int animationStep;
        private int savedRandomSeed = 0;
        private RandomGenerator seededRandom = new RandomGenerator();
        private Random improvRandom // = new Random();
        {
            get { return seededRandom.Random; }
        }
        public static bool LoadErrors { get; set; } = false;
        private List<ToolStripMenuItem> animationMenuItems;
        private List<ToolStripMenuItem> startupAnimationMenuItems;
        private Size origPictureBoxSize = Size.Empty;
        private string initialDesignFileName { get; }
        private RenderCaller renderCaller { get; }
        private ParameterDisplaysContainer parameterDisplaysContainer { get; }
        private ParameterDisplaysContainer.ParameterDisplays paramDisplays { get; }
        private CSharpParameterDisplay cSharpParamsDisplay { get; }
     
        private InfluencePointInfo nearestInfluencePoint { get; set; }

        private void OnDistancePatternsCountChanged(object sender, EventArgs e)
        {
            var oRender = (Pattern.RenderingInfo)sender;
            cSharpParamsDisplay.ParameterSourceInfo.DistancePatternsCount = 
                                   oRender.GetDistancePathsCount();
        }

        private bool UseDraftMode
        {
            get { return chkDraftMode.Checked; }
        }
        //public bool CancelRender { get; set; }
        //public VideoOps VideoOps { get; } = new VideoOps();

        private SelectPatternForm selectPatternForm = null;
        public static SelectPatternForm ClipboardPatternsForm { get; private set; }
        public static SelectPatternForm SelectColorForm { get; private set; }
        public static SelectPatternForm SelectPaletteForm { get; private set; }

        public static FormulaEntryList FormulaEntryList
        {
            get { return PatternChoices.FormulaEntryList; }
        }

        public PatternList LogoPatternGroup { get; set; }

        private Pattern influencePointsPattern { get; set; }

        public FormulaSettings EditedFormulaSettings { get; set; }
        private KeyEnumParameters editedKeyEnumParameters { get; set; }
        private object editedParametersObject { get; set; }

        private bool FilesFolderExists()
        {
            return Directory.Exists(WhorlSettings.Instance.FilesFolder);
        }

        private void SetPictureBoxSize(Size size, bool scale = true)
        {
            Size maxSize = GetPictureBoxMaxSize();
            if (scale)
            {
                float scaleFac = Math.Min((float)maxSize.Width / size.Width, (float)maxSize.Height / size.Height);
                size = new Size((int)(scaleFac * size.Width), (int)(scaleFac * size.Height));
            }
            else
                size = new Size(Math.Min(maxSize.Width, size.Width), Math.Min(maxSize.Height, size.Height));
            picDesign.Size = size;
        }

        private Size GetPictureBoxMaxSize()
        {
            int width = pnlSettings.Visible ? pnlSettings.Left - controlMargin : ClientSize.Width;
            return new Size(width,
                            this.ClientSize.Height - menuStrip1.Height - statusStrip1.Height);
        }

        private void DockPictureBox()
        {
            picDesign.Size = GetPictureBoxMaxSize();
        }

        private void SetPictureBoxSize(Size size)
        {
            Size maxSize = GetPictureBoxMaxSize();
            size = new Size(Math.Min(maxSize.Width, size.Width), Math.Min(maxSize.Height, size.Height));
            picDesign.Size = size;
        }

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            try
            {
                if (e.RefreshDisplay && Design.EditedPattern != null)
                {
                    Design.IsDirty = true;
                    Design.EditedPattern.ComputeSeedPoints();
                    RedrawPatternsAsync();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void OnParameterActionSelected(object sender, ParameterActionEventArgs e)
        {
            try
            {
                //FormulaSettings formulaSettings = e.BaseParameterDisplay.FormulaSettings;
                //if (!formulaSettings.IsCSharpFormula && !formulaSettings.Parameters.Any())
                //{
                //    MessageBox.Show("There are no numeric parameters to act on.");
                //    return;
                //}
                //ShowPropertyActionsForm(formulaSettings, e.BaseParameterDisplay.SelectedLabel);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                cboDraftSize.SelectedItem = WhorlSettings.Instance.DraftSize;
                DockPictureBox();
                origPictureBoxSize = picDesign.Size;
                pnlParameters.Height = pnlSettings.ClientSize.Height - lblParameters.Bottom - controlMargin;
                ContainerSize = GetPictureBoxMaxSize();
                WriteTitle();
                WhorlDesign.pictureBox = picDesign;
                animationMenuItems = new List<ToolStripMenuItem>
                        {  this.spinToolStripMenuItem, this.swayToolStripMenuItem,
                           this.revolveToolStripMenuItem, this.colorsToolStripMenuItem,
                           this.improviseToolStripMenuItem,
                           this.animateRecomputePatternsToolStripMenuItem };
                startupAnimationMenuItems = new List<ToolStripMenuItem>();
                foreach (ToolStripMenuItem itm in animationMenuItems)
                {
                    ToolStripMenuItem designItem = new ToolStripMenuItem(itm.Text);
                    designItem.Name = "Startup" + itm.Name;
                    designItem.CheckOnClick = true;
                    startupAnimationMenuItems.Add(designItem);
                }
                startupAnimationsToolStripMenuItem.DropDownItems.AddRange(
                    startupAnimationMenuItems.ToArray());
                WhorlDesign.SavedMenuItems.Add(this.animateOnSelectedPatternsToolStripMenuItem);
                WhorlDesign.UndoHandlerFn = this.OnUndoInfoChanged;
                gridTypeSquareToolStripMenuItem.Tag = GridTypes.Square;
                gridTypeCircularToolStripMenuItem.Tag = GridTypes.Circular;
                this.DoubleBuffered = true;
                WhorlDesign initialDesign = new WhorlDesign(OnDistancePatternsCountChanged);
                bool readDesign = false;
                if (!string.IsNullOrEmpty(initialDesignFileName))
                {
                    try
                    {
                        initialDesign.ReadDesignFromXmlFile(initialDesignFileName);
                        designFileName = initialDesignFileName;
                        readDesign = true;
                    }
                    catch (Exception ex1)
                    {
                        initialDesign = new WhorlDesign(OnDistancePatternsCountChanged);
                        MessageBox.Show(ex1.Message);
                    }
                }
                //picDesign.BackColor = WhorlSettings.Instance.DefaultBackgroundColor;
                SetDesign(initialDesign);
                string filePath = PatternChoicesFilePath;
                PatternChoices = new PatternGroupList(Design);
                ClipboardPatterns = new PatternGroupList(Design);
                if (File.Exists(filePath))
                {
                    Tools.ReadFromXml(filePath, PatternChoices, "PatternChoices");
                }
                else
                {
                    Tools.ReadFromXmlResource("Whorl.PatternChoices.PatternChoices.config",
                        PatternChoices, "PatternChoices");
                }
                PatternChoices.ClearIsChanged();
                filePath = Path.Combine(WhorlSettings.Instance.FilesFolder,
                                        WhorlSettings.Instance.DefaultPatternFileName);
                if (File.Exists(filePath))
                {
                    PatternChoices.DefaultPatternGroup = new PatternList(Design);
                    Tools.ReadFromXml(filePath, PatternChoices.DefaultPatternGroup, "DefaultPatternGroup");
                }
                if (readDesign)
                    DisplayDesign();
                else
                {
                    CreateCurrentBitmap();
                    picDesign.Image = currentBitmap;
                }
                colorIndex = 0;
                swayRotationVector = Complex.CreateFromModulusAndArgument(1D, Math.PI / 300D);
                //improvRandomSeeds = new List<int>();

                InitPatternChoiceSettings();
                if (!FilesFolderExists())
                {
                    MessageBox.Show("Please select the folder for Whorl user files (Files Folder).",
                                    "Required Action");
                    EditSettings();
                }
                WriteStatus("Files Folder: " + WhorlSettings.Instance.FilesFolder);
            }
            catch (Exception ex)
            {
                LoadErrors = true;
                Tools.HandleException(ex);
            }
            finally
            {
                ignoreEvents = false;
            }
        }

        private void InitPatternChoiceSettings()
        {
            Design.DefaultPatternGroup = PatternChoices.DefaultPatternGroup;
            //savedPatternChoices = (PatternGroupList)patternChoices.Clone();
            //savedPatternChoices = new PatternGroupList();
            //savedPatternChoices.PatternGroups.AddRange(patternChoices.PatternGroups);
            //savedPatternChoices.ColorChoices.AddRange(patternChoices.ColorChoices);
            //savedPatternChoices.DefaultPatternGroup = patternChoices.DefaultPatternGroup;
            selectPatternForm = new
                SelectPatternForm(SelectPatternForm.TargetTypes.Pattern, PatternChoices);
            selectPatternForm.Design = Design;
            ClipboardPatternsForm = 
                new SelectPatternForm(SelectPatternForm.TargetTypes.Pattern, ClipboardPatterns);
            SelectColorForm = new
                SelectPatternForm(SelectPatternForm.TargetTypes.Color, PatternChoices);
            SelectPaletteForm = new
                SelectPatternForm(SelectPatternForm.TargetTypes.Palette, PatternChoices);
            if (Design.DefaultPatternGroup == null)
            {   //Create default pattern:
                Design.DefaultPatternGroup = new PatternList(Design);
                Pattern defaultPattern = new Pattern(Design, FillInfo.FillTypes.Path);
                BasicOutline outline = new BasicOutline(BasicOutlineTypes.Round);
                defaultPattern.BasicOutlines.Add(outline);
                outline = new BasicOutline(BasicOutlineTypes.Pointed);
                outline.AddDenom = 0.25;
                outline.AngleOffset = Math.PI / 2D;
                outline.AmplitudeFactor = 2D;
                defaultPattern.BasicOutlines.Add(outline);
                defaultPattern.ComputeSeedPoints();
                Design.DefaultPatternGroup.AddPattern(defaultPattern);
                Design.DefaultPatternGroup.SetProperties();
            }
        }

        private void OnUndoInfoChanged(int undoIndex, bool canUndo, bool canRedo)
        {
            this.undoToolStripMenuItem.Enabled = canUndo;
            this.redoToolStripMenuItem.Enabled = canRedo;
            //WriteStatus("UndoIndex = " + undoIndex.ToString());
        }

        /// <summary>
        /// Handles DesignLayer.LayerChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignLayerChanged(object sender, DesignLayerChangedEventArgs e)
        {
            try
            {
                if (e.WhorlDesignChanged)
                    Design.IsDirty = true;
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DesignDirtyChanged(object sender, EventArgs e)
        {
            WriteTitle();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!CheckSaveDesign())
                {
                    e.Cancel = true;
                    return;  //User cancelled.
                }
                if (!LoadErrors && FilesFolderExists())
                {
                    //var newPatternChoices = patternChoices;
                    //bool choicesChanged = patternChoices.IsChanged;
                    PatternChoices.FinalizeIsChanged();
                    if (PatternChoices.IsChanged)
                    {
                        var changedList = new List<string>();
                        if (PatternChoices.IsPatternChanged)
                            changedList.Add("pattern");
                        if (PatternChoices.IsColorChanged)
                            changedList.Add("color");
                        if (PatternChoices.IsPaletteChanged)
                            changedList.Add("palette");
                        if (PatternChoices.IsFormulaChanged)
                            changedList.Add("formula");
                        switch (MessageBox.Show(
                            $"Save changes to {string.Join("/", changedList)} choices?", "Confirm Save",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1))
                        {
                            case DialogResult.Yes:
                                SavePatternChoices();
                                break;
                            case DialogResult.No:
                                break;
                                //choicesChanged = false;
                                //newPatternChoices = savedPatternChoices;
                                //break;
                            case DialogResult.Cancel:
                                e.Cancel = true;
                                return;
                        }
                    }
                    if (frmColorGradient != null && !frmColorGradient.IsDisposed)
                        frmColorGradient.Close();
                    if (PatternChoices.DefaultPatternGroup != Design.DefaultPatternGroup)
                    {
                        //Save Default Pattern Group
                        string filePath = Path.Combine(WhorlSettings.Instance.FilesFolder,
                                                       WhorlSettings.Instance.DefaultPatternFileName);
                        XmlTools.WriteToXml(filePath, Design.DefaultPatternGroup, "DefaultPatternGroup");
                    }
                    if (frmFormulaInsert.Instance != null)
                    {
                        var stdTextsList = frmFormulaInsert.Instance.StandardTextsList;
                        if (stdTextsList.StandardTextsChanged)
                        {
                            string filePath = Path.Combine(WhorlSettings.Instance.FilesFolder, InitialSetup.StandardTextsFileName);
                            XmlTools.WriteToXml(filePath, stdTextsList);
                        }
                    }
                }
                WhorlSettings.Instance.Save(ifChanged: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SavePatternChoices()
        {
            string patternChoicesFilePath = PatternChoicesFilePath;
            XmlTools.WriteToXml(patternChoicesFilePath, PatternChoices, "PatternChoices");
            PatternChoices.ClearIsChanged();

        }

        private void saveChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SavePatternChoices();
                //savedPatternChoices = (PatternGroupList)patternChoices.Clone();
                WriteStatus("Saved choices to their file.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private static string PatternChoicesFilePath
        {
            get
            {
                return Path.Combine(WhorlSettings.Instance.FilesFolder,
                                    WhorlSettings.Instance.PatternChoicesFileName);
            }
        }

        public bool Animating
        {
            get { return animationMenuItems.Exists(it => it.Checked); }
        }

        private int GridSize
        {
            get { return gridSize; }
            set
            {
                gridSize = value;
                OnchangeGridSettings();
            }
        }

        private GridTypes GridType
        {
            get { return gridType; }
            set
            {
                gridType = value;
                OnchangeGridSettings();
            }
        }

        private void OnchangeGridSettings()
        {
            picDesign.Invalidate();
        }

        private GraphicsPath backgroundGrPath = null;
        private PathGradientBrush backgroundPthGrBrush;
        //private Rectangle backgroundRect;

        private void CreateCurrentBitmap()
        {
            if (currentBitmap != null)
                currentBitmap.Dispose();
            currentBitmap = Design.CreateDesignBitmap(picDesign.Width, picDesign.Height,
                                            ref backgroundGrPath, ref backgroundPthGrBrush);
        }

        private Color GetNextColor(bool increment = true)
        {
            Color color = this.gradientColors[colorIndex];
            if (increment)
                colorIndex = (colorIndex + 1) % this.gradientColors.Length;
            return color;
        }

        private PatternList redrawnPatternGroup = null;
        private PatternList origRedrawnPatternGroup;
        private PatternList defaultPatternGroup = null;

        private PatternList DrawnPatternGroup
        {
            get
            {
                return redrawnPatternGroup ?? defaultPatternGroup;
            }
        }

        //private void InitializeForMovePattern()
        //{
        //    if (!selectAllToolStripMenuItem.Checked)
        //    {
        //        Pattern selPattern = design.SelectedPattern;
        //        if (selPattern == null)
        //            return;
        //        foreach (Pattern pattern in design.DrawnPatterns)
        //        {
        //            if (Tools.Distance(pattern.Center, selPattern.Center) < 10D)
        //                pattern.Selected = true;
        //        }
        //    }
        //}

        private Point mouseDownPoint;
        private PathOutline polygonOutline;
        //private List<PointF> polygonPoints;
        private bool getPolygonCenter;
        private bool selectingRectangle;

        private void picDesign_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (dragState == DragStates.None)
                {
                    if (getPolygonCenter && polygonOutline != null)
                    {
                        getPolygonCenter = false;
                        PointF center = new PointF(e.X, e.Y);
                        FinishDrawnPolygon(center);
                        return;
                    }
                    else if (setGradientCenterPattern != null)
                    {
                        PointF gradientCenter = new PointF(e.X, e.Y);
                        Pattern targetPattern = setGradientCenterPattern.GetCopy();
                        targetPattern.SetCenterOffset(gradientCenter);
                        Design.ReplacePattern(setGradientCenterPattern, targetPattern);
                        setGradientCenterPattern = null;
                        RedrawPatterns();
                        WriteStatus("Set gradient center.");
                        return;
                    }
                    else if (drawLogoPatternToolStripMenuItem.Checked)
                    {
                        AddLogoPattern(e.X, e.Y);
                        drawLogoPatternToolStripMenuItem.Checked = false;
                        return;
                    }
                    dragState = DragStates.Dragging;
                    mouseDownPoint = new Point(e.X, e.Y);
                    dragStart = new Point(e.X, e.Y);
                    if (moveInfluencePoint)
                    {
                        dragEnd = dragStart;
                        return;
                    }
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        selectingRectangle = true;
                        return;  //Dragging to select controls.
                    }
                    //if (distanceParentPattern != null && lockDistancePatternToolStripMenuItem.Checked)
                    //{
                    //    dragStart = new Point((int)distanceParentPattern.Center.X, 
                    //                          (int)distanceParentPattern.Center.Y);
                    //}
                    if (lockedCenter != null &&   //this.lockCenterToolStripMenuItem.Checked &&
                        !moveTextureToolStripMenuItem.Checked &&
                        !moveSelectedPatternsToolStripMenuItem.Checked &&
                        !DrawUserVertices) // && selPattern != null)
                    {
                        dragStart = (Point)lockedCenter; //new Point((int)selPattern.Center.X, (int)selPattern.Center.Y);
                    }
                    dragEnd = dragStart;
                    if (moveSelectedPatternsToolStripMenuItem.Checked)
                    {
                        //InitializeForMovePattern();
                        RedrawPatterns(excludeSelected: true);
                    }
                    else if (!moveTextureToolStripMenuItem.Checked &&
                             !DrawUserVertices)
                    {
                        if (redrawPatternToolStripMenuItem.Checked &&
                            redrawnPatternGroup != null)
                        {
                            RedrawPatterns(excludeSelected: true);
                        }
                        else
                        {
                            if (!continueRibbonToolStripMenuItem.Checked)
                            {
                                defaultPatternGroup =
                                    Design.DefaultPatternGroup.GetCopy(copyKeyGuid: false);
                                foreach (Pattern pattern in defaultPatternGroup.Patterns)
                                    pattern.OrigRandomSeed = null;
                                Ribbon ribbon = defaultPatternGroup.GetRibbon();
                                if (ribbon != null)
                                    ribbon.RibbonPath.Clear();
                            }
                        }
                        DrawnPatternGroup.SetCenters(new PointF(dragStart.X, dragStart.Y));
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                continueRibbonToolStripMenuItem.Checked = false;
                distanceParentPattern = null;
                distancePatternInfo = null;
                if (getPolygonCenter)
                {
                    //Cancel polygon drawing.
                    getPolygonCenter = false;
                    CancelDrawnPolygon();
                    picDesign.Invalidate();
                }
                else if (setGradientCenterPattern != null)
                {
                    setGradientCenterPattern = null;
                    picDesign.Refresh();
                    WriteStatus("Cancelled set gradient center.");
                    return;
                }
                else if (drawLogoPatternToolStripMenuItem.Checked)
                    drawLogoPatternToolStripMenuItem.Checked = false;
                else if (dragState == DragStates.Dragging)
                {
                    dragState = DragStates.None;
                    if (moveInfluencePoint)
                    {
                        nearestInfluencePoint.Selected = false;
                        moveInfluencePoint = false;
                        picDesign.Invalidate();
                        return;
                    }
                    if (DrawUserVertices && polygonOutline != null)
                    {
                        polygonOutline.SegmentVertices.Add(new PointF(e.X, e.Y));
                        WriteStatus("Click on the center of the polygon.");
                        getPolygonCenter = true;
                        picDesign.Invalidate();
                    }
                    else
                    {
                        ClearRedrawPattern();
                        picDesign.Invalidate();
                    }
                }
                else  //Show context menu.
                {
                    dragStart = new Point(e.X, e.Y);
                    if (editInfluencePointsModeToolStripMenuItem.Checked
                        && influencePointsPattern != null)
                    {
                        nearestInfluencePoint = GetNearestInfluencePoint(dragStart);
                        foreach (ToolStripMenuItem item in influenceContextMenuStrip.Items)
                        {
                            if (item != addInfluencePointToolStripMenuItem)
                                item.Visible = nearestInfluencePoint != null;
                        }
                        influenceContextMenuStrip.Show(picDesign, dragStart);
                    }
                    else
                    {
                        ConfigureContextMenuItems();
                        contextMenuStrip1.Show(picDesign, dragStart);
                    }
                }
            }
        }

        private void ConfigureContextMenuItems()
        {
            var nearbyPatterns = GetNearbyPatterns(dragStart, includeRecursive: true).ToList();
            if (!nearbyPatterns.Any() && useSelectedPatternToolStripMenuItem.Checked)
            {
                Pattern singleSelPattern = FindSingleSelectedPattern();
                if (singleSelPattern != null)
                    nearbyPatterns.Add(new Pattern.PatternInfo(singleSelPattern));
            }
            bool haveNearbyPattern = nearbyPatterns.Count != 0;
            mouseDistancePatternInfo = FindDistanceInfo(dragStart, out Pattern parent);
            mouseDistanceParentPattern = parent;
            ShowMenuItems(contextMenuStrip1.Items, haveNearbyPattern, mouseDistancePatternInfo != null, out _);
            //foreach (var mnuItem in contextMenuStrip1.Items)
            //{
            //    var menuItem = mnuItem as ToolStripMenuItem;
            //    if (menuItem != null)
            //    {
            //        var menuItemType = (MenuItemTypes)menuItem.Tag;
            //        if (menuItemType == MenuItemTypes.Normal)
            //            menuItem.Visible = haveNearbyPattern;
            //        else if (menuItemType == MenuItemTypes.DistancePattern)
            //            menuItem.Visible = mouseDistancePatternInfo != null;
            //        //menuItem.Visible = haveNearbyPattern || 
            //        //                   menuItem == pasteDefaultPatternToolStripMenuItem || 
            //        //                   menuItem == pasteCopiedPatternsToolStripMenuItem;
            //    }
            //}
            if (haveNearbyPattern)
            {
                bool isSelected = nearbyPatterns.Any(x => x.Pattern.Selected);
                if (isSelected)
                    selectPatternToolStripMenuItem.Text = "Unselect Pattern";
                else
                    selectPatternToolStripMenuItem.Text = "Select Pattern";
            }
        }

        private void ShowMenuItems(ToolStripItemCollection items, 
                                   bool haveNearbyPattern,
                                   bool haveDistancePattern,
                                   out bool setParentVisible)
        {
            setParentVisible = false;
            foreach (var mnuItem in items)
            {
                var menuItem = mnuItem as ToolStripMenuItem;
                if (menuItem != null)
                {
                    MenuItemTypes menuItemType;
                    if (menuItem.Tag is MenuItemTypes)
                        menuItemType = (MenuItemTypes)menuItem.Tag;
                    else
                        menuItemType = MenuItemTypes.Normal;
                    if (!(haveNearbyPattern || haveDistancePattern))
                    {
                        if (menuItemType != MenuItemTypes.Paste)
                            menuItem.Visible = false;
                    }
                    else
                    {
                        bool isVisible;
                        if (menuItem.DropDownItems.Count != 0)
                        {
                            ShowMenuItems(menuItem.DropDownItems, haveNearbyPattern, haveDistancePattern,
                                          out isVisible);
                        }
                        else if (menuItemType == MenuItemTypes.Normal)
                            isVisible = haveNearbyPattern;
                        else if (menuItemType == MenuItemTypes.DistancePattern)
                            isVisible = haveDistancePattern;
                        else
                            isVisible = true;
                        if (isVisible)
                            setParentVisible = true;
                        menuItem.Visible = isVisible;
                        //menuItem.Visible = haveNearbyPattern || 
                        //                   menuItem == pasteDefaultPatternToolStripMenuItem || 
                        //                   menuItem == pasteCopiedPatternsToolStripMenuItem;
                    }
                }
            }

        }

        private void AddLogoPattern(int x, int y)
        {
            if (PatternChoices.LogoPatternGroup == null)
                return;
            PatternList logoCopy = PatternChoices.LogoPatternGroup.GetCopy();
            Pattern leftMostPattern = logoCopy.Patterns.OrderBy(ptn => ptn.Center.X).FirstOrDefault();
            if (leftMostPattern == null)
                return;
            foreach (Pattern ptn in logoCopy.Patterns)
            {
                if (ptn != leftMostPattern)
                    ptn.Center = new PointF(x + ptn.Center.X - leftMostPattern.Center.X, y + ptn.Center.Y - leftMostPattern.Center.Y);
            }
            leftMostPattern.Center = new PointF(x, y);
            Design.AddPatterns(logoCopy.Patterns);
            RedrawPatterns();
        }

        private void StartDrawnPolygon()
        {
            polygonOutline = new PathOutline();
            if (!drawClosedCurveToolStripMenuItem.Checked)
                polygonOutline.PolygonUserVertices = WhorlSettings.Instance.UseNewPolygonVersion;
            polygonOutline.InitUserDefinedVertices(drawClosedCurveToolStripMenuItem.Checked);
            getPolygonCenter = false;
        }

        private void FinishDrawnPolygon(PointF center)
        {
            if (polygonOutline?.SegmentVertices != null && polygonOutline.SegmentVertices.Count() >= 3)
            {
                polygonOutline.SegmentVerticesCenter = center;
                polygonOutline.SetClosedVertices(polygonOutline.SegmentVertices,
                                                 drawClosedCurveToolStripMenuItem.Checked);
                Complex zVector = polygonOutline.NormalizePathVertices();
                PathPattern ptn = new PathPattern(Design);
                ptn.BoundaryColor = Color.Red;
                ptn.ZVector = zVector;
                ptn.Center = center;
                ptn.BasicOutlines.Add(polygonOutline);
                ptn.SetVertexAnglesParameters();
                ptn.ComputeSeedPoints();
                Design.AddPattern(ptn);
                RedrawPatterns();
            }
            CancelDrawnPolygon();
        }

        private void CancelDrawnPolygon()
        {
            drawPolygonToolStripMenuItem.Checked =
                drawClosedCurveToolStripMenuItem.Checked = false;
            polygonOutline = null;
        }

        private void SetPatternCenter(Pattern pattern, int X, int Y)
        {
            if (pattern != null)
                pattern.Center = new PointF(X, Y);
        }

        private void MovePatterns(int dX, int dY)
        {
            foreach (Pattern pattern in Design.DesignPatterns)
            {
                if (pattern.Selected)
                {
                    Ribbon ribbon = pattern as Ribbon;
                    if (ribbon != null)
                    {
                        ribbon.TranslateRibbonPath(new PointF(dX, dY));
                        //for (int i = 0; i < ribbon.RibbonPath.Count; i++)
                        //{
                        //    ribbon.RibbonPath[i] = new PointF(ribbon.RibbonPath[i].X + dX,
                        //                                      ribbon.RibbonPath[i].Y + dY);
                        //}
                    }
                    pattern.Center = new PointF(pattern.Center.X + dX, pattern.Center.Y + dY);
                }
            }
        }

        private void ClearRedrawPattern()
        {
            //if (redrawPreservingCenterPathToolStripMenuItem.Checked && redrawnPatternGroup != null)
            //{
            //    foreach (Pattern ptn in redrawnPatternGroup.Patterns)
            //    {
            //        ptn.PreserveCenterPath = false;
            //    }
            //}
            redrawnPatternGroup = null;
            //redrawPreservingCenterPathToolStripMenuItem.Checked = false;
            redrawPatternToolStripMenuItem.Checked = false;
        }

        //private void MovePatterns(int X, int Y)
        //{
        //    foreach (Pattern pattern in design.DrawnPatterns)
        //    {
        //        if (pattern.Selected)
        //            SetPatternCenter(pattern, X, Y);
        //    }
        //}

        private void picDesign_MouseMove(object sender, MouseEventArgs e)
        {
            if (polygonOutline != null)
            {
                dragEnd = new Point(e.X, e.Y);
                picDesign.Invalidate();
            }
            else if (e.Button == MouseButtons.Left && dragState == DragStates.Dragging)
            {
                if (!selectingRectangle && moveSelectedPatternsToolStripMenuItem.Checked)
                {
                    MovePatterns(e.X - dragEnd.X, e.Y - dragEnd.Y);
                }
                else if (moveInfluencePoint)
                {
                    MoveInfluencePoint(e.X - dragEnd.X, e.Y - dragEnd.Y);
                }
                dragEnd = new Point(e.X, e.Y);
                picDesign.Invalidate();
            }
        }

        private void MoveInfluencePoint(double xDiff, double yDiff)
        {
            nearestInfluencePoint.InfluencePoint = new DoublePoint(
                                  nearestInfluencePoint.InfluencePoint.X + xDiff,
                                  nearestInfluencePoint.InfluencePoint.Y + yDiff);
        }

        private void continueRibbonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbon = GetNearbyRibbon(dragStart, out PathPattern pathPattern);
                if (ribbon == null || pathPattern != null)
                    return;
                defaultPatternGroup = new PatternList(Design);
                defaultPatternGroup.AddPattern(ribbon);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        Rectangle GetSelectionRectangle()
        {
            Point topLeft = new Point(Math.Min(dragStart.X, dragEnd.X),
                                      Math.Min(dragStart.Y, dragEnd.Y));
            Point bottomRight = new Point(Math.Max(dragStart.X, dragEnd.X),
                                          Math.Max(dragStart.Y, dragEnd.Y));
            return new Rectangle(topLeft.X, topLeft.Y,
                                 bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        private void picDesign_MouseUp(object sender, MouseEventArgs e)
        {
            Pattern distanceParent = distanceParentPattern;
            var distanceInfo = distancePatternInfo;
            distanceParentPattern = null;
            distancePatternInfo = null;
            if (e.Button == MouseButtons.Left && dragState == DragStates.Dragging)
            {
                Point saveDragEnd = dragEnd;
                dragEnd = new Point(e.X, e.Y);
                if (selectingRectangle)
                {
                    selectingRectangle = false;
                    dragState = DragStates.None;
                    Rectangle selRect = GetSelectionRectangle();
                    foreach (Pattern pattern in Design.DesignPatterns)
                    {
                        if (selRect.Contains((int)pattern.Center.X, (int)pattern.Center.Y))
                            pattern.Selected = true;
                    }
                    picDesign.Invalidate();
                    return;
                }
                if (DrawUserVertices)
                {
                    if (polygonOutline == null)
                        StartDrawnPolygon();
                    polygonOutline.SegmentVertices.Add(dragEnd);
                    return;
                }
                dragState = DragStates.None;
                if (moveInfluencePoint)
                {
                    MoveInfluencePoint(e.X - saveDragEnd.X, e.Y - saveDragEnd.Y);
                    nearestInfluencePoint.Selected = false;
                    RedrawInfluencePattern();
                    picDesign.Refresh();
                    moveInfluencePoint = false;
                    Design.IsDirty = true;
                    return;
                }
                if (moveSelectedPatternsToolStripMenuItem.Checked)
                {
                    MovePatterns(e.X - saveDragEnd.X, e.Y - saveDragEnd.Y);
                    PointF delta = new PointF(dragEnd.X - dragStart.X, dragEnd.Y - dragStart.Y);
                    Design.AddMovePatternsOperation(delta);
                    RedrawPatterns();
                    moveSelectedPatternsToolStripMenuItem.Checked = false;
                }
                else if (moveTextureToolStripMenuItem.Checked)
                {
                    if (ShiftPatternTexture())
                        RedrawPatterns();
                    moveTexturePattern = null;
                }
                else if (Tools.Distance(new PointF(mouseDownPoint.X, mouseDownPoint.Y),
                                        new PointF(dragEnd.X, dragEnd.Y)) > 3.0)
                {
                    PatternList patternGroup = this.DrawnPatternGroup;  //Redrawn or default pattern.
                    Complex zVector = patternGroup.PreviewZFactor * new Complex(dragEnd.X - dragStart.X, dragEnd.Y - dragStart.Y);
                    bool processDistance = distanceParent?.PixelRendering != null && patternGroup.PatternsList.Any();
                    if (processDistance)
                    {   //Process distance pattern.
                        Pattern distancePattern = patternGroup.PatternsList.First();
                        distancePattern.ZVector = zVector;
                        Pattern distanceParentCopy;
                        if (distanceInfo == null)  //Newly added distance pattern.
                        {
                            distanceParentCopy = distanceParent.GetCopy();
                            if (Design.EditedPattern == distanceParent)
                                Design.EditedPattern = distanceParentCopy;
                            distanceParentCopy.PixelRendering.AddDistancePattern(distanceParentCopy, distancePattern);
                            AddRenderingControls(distanceParentCopy);
                        }
                        else   //Redrawn distance pattern.
                        {
                            distanceParentCopy = CopyForDistanceInfo(distanceParent, distanceInfo,
                                                                     out var distanceInfoCopy);
                            if (Design.EditedPattern == distanceParent)
                                Design.EditedPattern = distanceParentCopy;
                            distanceInfoCopy.SetDistancePattern(distanceParentCopy, distancePattern, transform: true);
                            AddRenderingControls(distanceParentCopy);
                        }
                        distanceParentCopy.PixelRendering.UseDistanceOutline = true;
                        Design.ReplacePattern(distanceParent, distanceParentCopy);
                        RedrawPatterns();
                    }
                    else
                    {
                        patternGroup.SetZVectors(zVector);
                        foreach (Pattern pattern in patternGroup.Patterns)
                        {
                            if (pattern.HasRandomElements)
                                pattern.SetNewRandomSeed();
                        }
                        if (patternGroup != defaultPatternGroup)
                        {
                            if (patternGroup == redrawnPatternGroup)
                            {
                                Design.ReplacePatterns(origRedrawnPatternGroup.PatternsList,
                                                       redrawnPatternGroup.PatternsList);
                            }
                            RedrawPatterns();
                        }
                        else
                        {   //Drawing chosen default pattern group:
                            if (cycleColorsToolStripMenuItem.Checked)
                            {
                                foreach (Pattern pattern in patternGroup.Patterns)
                                {
                                    pattern.BoundaryColor = GetNextColor();
                                    pattern.CenterColor = GetNextColor(increment: false);
                                }
                            }
                            Ribbon ribbon = patternGroup.GetRibbon();
                            if (drawLineRibbonToolStripMenuItem.Checked)
                            {
                                if (ribbon != null)
                                {
                                    double length = Tools.Distance(dragStart, dragEnd);
                                    int segments = (int)Math.Round(length / ribbon.RibbonDistance);
                                    PointF segInc = new PointF((float)(dragEnd.X - dragStart.X) / segments,
                                                               (float)(dragEnd.Y - dragStart.Y) / segments);
                                    for (int i = 0; i <= segments; i++)
                                    {
                                        ribbon.RibbonPath.Add(new PointF(dragStart.X + i * segInc.X, dragStart.Y + i * segInc.Y));
                                    }

                                }
                            }
                            if (continueRibbonToolStripMenuItem.Checked)
                            {
                                RedrawPatterns();
                            }
                            else
                            {
                                bool fixedRibbon = ribbon != null && ribbon.ScaleRibbonPath();
                                int insertIndex = -1;
                                if (drawUnderSelectedPatternToolStripMenuItem.Checked)
                                {
                                    insertIndex = Design.FindPatternIndex(ptn => ptn.Selected);
                                }
                                foreach (Pattern pattern in patternGroup.Patterns)
                                {
                                    pattern.DesignLayer = Design.DefaultDesignLayer;
                                }
                                Design.AddPatterns(patternGroup.Patterns, insertIndex);
                                if (insertIndex == -1 && !fixedRibbon)
                                    DrawFilledPattern(patternGroup);
                                else
                                    RedrawPatterns();
                            }
                        }
                    }
                    ClearRedrawPattern();
                }
                else
                {
                    ClearRedrawPattern();
                    picDesign.Invalidate();
                }
                continueRibbonToolStripMenuItem.Checked = false;
            }
        }

        private void RedrawInfluencePattern()
        {
            if (influencePointsPattern != null)
            {
                influencePointsPattern.ClearRenderingCache();
                influencePointsPattern.ComputeSeedPoints();
                RedrawPatterns();
            }
        }

        private void AddRenderingControls(Pattern pattern)
        {
            FormulaSettings formulaSettings = pattern.PixelRendering?.FormulaSettings;
            if (formulaSettings == null)
                return;
            object paramsObject = formulaSettings.EvalInstance?.ParamsObj;
            if (paramsObject == null)
                return;
            if (pattern == Design.EditedPattern)
            {
                EditedFormulaSettings = formulaSettings;
                //editedParametersObject = paramsObject;
                editedKeyEnumParameters = null;
                EditParameters();
            }
            else
            {
                cSharpParamsDisplay.InitializeSources(formulaSettings, pattern);
            }
        }

        private void EditParameters()
        {
            Pattern pattern = Design.EditedPattern;
            FormulaSettings formulaSettings = editedKeyEnumParameters == null ? EditedFormulaSettings 
                                            : editedKeyEnumParameters.Parent.FormulaSettings;
            if (pattern == null || formulaSettings == null)
                return;
            if (formulaSettings.IsCSharpFormula)
            {
                object paramsObject;
                if (editedKeyEnumParameters != null)
                {
                    paramsObject = editedKeyEnumParameters.ParametersObject;
                    cSharpParamsDisplay.FnEditInfluenceLink = null;
                }
                else
                {
                    paramsObject = formulaSettings.EvalInstance?.ParamsObj;
                    cSharpParamsDisplay.FnEditInfluenceLink = EditInfluenceLink;
                }
                if (paramsObject == null)
                    return;
                //editedParametersObject = paramsObject;
                cSharpParamsDisplay.SetParametersObject(paramsObject);
            }
            ShowRenderingPanels(true);
            parameterDisplaysContainer.AddParametersControls(pnlParameters, formulaSettings, pattern);
        }

        private bool ShiftPatternTexture()
        {
            TextureFillInfo txtFill = moveTexturePattern?.FillInfo as TextureFillInfo;
            if (txtFill != null)
            {
                txtFill.TextureOffset = new Point(
                    initialTextureOffset.X + dragEnd.X - dragStart.X,
                    initialTextureOffset.Y + dragEnd.Y - dragStart.Y);
                txtFill.ApplyTransforms();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draw pattern in normal drawing mode (not Quality).
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="g"></param>
        private void DrawPattern(Pattern pattern, Graphics g,
                                 bool computeRandom = false)
        {
            pattern.FillInfo.ApplyTransforms(GetTextureScale());
            pattern.DrawFilled(g, renderCaller, computeRandom, draftMode: UseDraftMode);
            pattern.FillInfo.ApplyTransforms(scale: 1F);
        }

        private void picDesign_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (graphPoints != null)
                {
                    e.Graphics.DrawCurve(Pens.Red, graphPoints);
                    foreach (PointF p in graphFitPoints)
                    {
                        e.Graphics.FillEllipse(Brushes.Green, new RectangleF(p, new SizeF(4F, 4F)));
                    }
                }
                bool showGrid = showGridToolStripMenuItem.Checked
                     && (GridType != GridTypes.None || tileInfoPattern != null);
                //WriteTrace("Repaint: dragState=" + dragState.ToString());
                if (polygonOutline != null)
                {
                    List<PointF> points = new List<PointF>(polygonOutline.SegmentVertices);
                    if (!getPolygonCenter)
                        points.Add(dragEnd);
                    if (points.Count > 1)
                    {
                        if (points.Count == 2)
                            e.Graphics.DrawLines(Pens.Red, points.ToArray());
                        else if (drawClosedCurveToolStripMenuItem.Checked &&
                                 points.Count >= 3)
                        {
                            polygonOutline.SetClosedVertices(points, setCurve: true);
                            e.Graphics.DrawPolygon(Pens.Red,
                                        polygonOutline.PathVertices.ToArray());
                        }
                        else
                            e.Graphics.DrawPolygon(Pens.Red, points.ToArray());
                    }
                    if (showGrid)
                    {
                        ShowGrid(e.Graphics);
                    }
                    return;
                }
                if (Animating && !pauseImprovToolStripMenuItem.Checked)
                {
                    if (ShouldDrawDesignLayersForAnimate())
                        return;
                    //if (VideoOps.IsRecording || ShouldDrawDesignLayersForAnimate())
                    //    return;
                    var animationPatterns = AnimationPatterns;
                    for (int i = 0; i < animationPatterns.Count; i++)
                    {
                        Pattern pattern = animationPatterns[i];
                        if (displayInterpolatedPatterns)
                        {
                            SmoothAnimationPatternInfo ptnInfo = GetSmoothPatternInfo(i);
                            if (ptnInfo != null)
                            {
                                ptnInfo.InterpolatedPattern.DrawFilled(e.Graphics, null,
                                    computeRandom: false, draftMode: true);
                                continue;
                            }
                        }
                        pattern.DrawFilled(e.Graphics, null, computeRandom: false, draftMode: true);
                    }
                    displayInterpolatedPatterns = false;
                    return;
                }
                else if (dragState == DragStates.Dragging && !moveInfluencePoint)
                {
                    if (selectingRectangle)
                    {
                        Rectangle selRect = GetSelectionRectangle();
                        e.Graphics.DrawRectangle(Pens.Blue, selRect);
                    }
                    else if (moveSelectedPatternsToolStripMenuItem.Checked)
                    {
                        foreach (Pattern pattern in Design.DesignPatterns)
                        {
                            if (pattern.RenderMode == Pattern.RenderModes.Stain)
                                pattern.DrawSelectionOutline(e.Graphics);
                            else if (pattern.Selected)
                            {
                                if (!alwaysFillMovedPattternsToolStripMenuItem.Checked &&
                                    (pattern.Recursion.IsRecursive
                                    || pattern.HasSurroundColors()))
                                    pattern.DrawSelectionOutline(e.Graphics);
                                else
                                    DrawPattern(pattern, e.Graphics);
                            }
                        }
                    }
                    else if (moveTextureToolStripMenuItem.Checked)
                    {
                        if (ShiftPatternTexture())
                            DrawPattern(moveTexturePattern, e.Graphics);
                    }
                    else
                    {
                        PatternList patternGroup = this.DrawnPatternGroup;
                        if (patternGroup == null)
                            return;
                        patternGroup.SetOutlineZVectors(patternGroup.PreviewZFactor *
                            new Complex(dragEnd.X - dragStart.X, dragEnd.Y - dragStart.Y));
                        if (!drawLineRibbonToolStripMenuItem.Checked)
                        {
                            Ribbon ribbon = patternGroup.GetRibbon();
                            if (ribbon != null)
                            {
                                if (ribbon.ShouldAddToPath)
                                {
                                    ribbon.ZVector = ribbon.OutlineZVector;
                                    PointF newP = ribbon.AddToRibbonPath(new PointF(dragEnd.X, dragEnd.Y), interpolate: true);
                                    if (newP != dragEnd)
                                    {
                                        Point newMouseP = new Point((int)newP.X, (int)newP.Y);
                                        Cursor.Position = picDesign.PointToScreen(newMouseP);
                                        dragEnd = newMouseP;
                                    }
                                    dragStart = dragEnd;
                                }
                                ribbon.DrawFilled(e.Graphics, null, computeRandom: false, draftMode: UseDraftMode);
                                if (ribbon.RibbonPath.Count > 0)
                                    ribbon.Center = ribbon.RibbonPath.Last();
                            }
                        }
                        //if (targetPatterns.Count > 0)
                        //    WriteTrace("Outline Patterns[0] center = " + targetPatterns[0].Center.ToString());
                        patternGroup.DrawOutlines(e.Graphics,
                            Tools.InverseColor(Design.BackgroundColor));
                    }
                }
                else
                {
                    if (Design != null)
                    {
                        if (showSelectionsToolStripMenuItem.Checked)
                        {
                            foreach (var ptnInfo in Design.GetAllPatternsInfo(includeRecursive: true))
                            {
                                Pattern pattern = ptnInfo.Pattern;
                                if (pattern.Selected)
                                {
                                    var subPtnInfo = ptnInfo as Pattern.SubpatternInfo;
                                    if (subPtnInfo != null)
                                    {
                                        pattern.Center = subPtnInfo.Center;
                                        pattern.ZVector = subPtnInfo.ZVector;
                                    }
                                    pattern.DrawSelectionOutline(e.Graphics);
                                }
                            }
                        }
                        if (influencePointsPattern != null && showInfluencePointsToolStripMenuItem.Checked)
                        {
                            foreach (var influencePointInfo in influencePointsPattern.InfluencePointInfoList.InfluencePointInfos)
                            {
                                influencePointInfo.Draw(e.Graphics, currentBitmap, this.Font);
                            }
                        }
                        if (showInfluencePointsDistancePattern != null)
                        {
                            foreach (var influencePointInfo in showInfluencePointsDistancePattern.InfluencePointInfoList.InfluencePointInfos)
                            {
                                influencePointInfo.Draw(e.Graphics, currentBitmap, this.Font);
                            }
                        }
                        if (viewDistanceOutlineToolStripMenuItem.Checked)
                        {
                            foreach (var pattern in Design.DesignPatterns)
                            {
                                if (pattern.UsesDistancePattern)
                                {
                                    int index = 1;
                                    bool showIndexes = pattern.PixelRendering.GetDistancePatternInfos().Count() > 1;
                                    foreach (var info in pattern.PixelRendering.GetDistancePatternInfos())
                                    {
                                        Pattern distancePattern = info.GetDistancePattern(pattern);
                                        distancePattern.DrawSelectionOutline(e.Graphics);
                                        if (showIndexes)
                                        {
                                            e.Graphics.DrawString(index.ToString(), this.Font, 
                                                                  Brushes.Black, distancePattern.Center);
                                            index++;
                                        }
                                        //Tools.DrawSquare(e.Graphics, Color.Red, info.DistancePatternCenter);
                                    }
                                }
                            }
                        }
                    }
                    if (setGradientCenterPattern != null)
                    {
                        PointF gradCenter = setGradientCenterPattern.GetPathGradientCenter();
                        Color cColor = Color.Red;
                        Point iGradCenter = new Point((int)gradCenter.X, (int)gradCenter.Y);
                        Rectangle bitRect = new Rectangle(Point.Empty, currentBitmap.Size);
                        if (bitRect.Contains(iGradCenter))
                        {
                            cColor = Tools.InverseColor(
                                currentBitmap.GetPixel(iGradCenter.X, iGradCenter.Y));
                        }
                        Tools.DrawSquare(e.Graphics, cColor, gradCenter);
                    }
                    if (editedGradientPattern != null)
                    {
                        editedGradientPattern.DrawFilled(e.Graphics, renderCaller, computeRandom: false,
                            draftMode: UseDraftMode, recursiveDepth: Pattern.NonRecursiveDepth);
                    }
                }
                if (showGrid)
                {
                    ShowGrid(e.Graphics);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Rectangle PictureBoxRectangle
        {
            get
            {
                Rectangle bounds = picDesign.ClientRectangle;
                //if (picDesign.Dock == DockStyle.Left)
                //    bounds.Height -= statusStrip1.Height;
                return bounds;
            }
        }

        private void ShowGrid(Graphics g)
        {
            bool drawTileGrid = tileInfoPattern != null;
            SizeF gridRectSize;
            Rectangle bounds = PictureBoxRectangle;
            if (drawTileGrid)
            {
                var tileInfo = tileInfoPattern.PatternTileInfo;
                bounds = tileInfo.GetInnerRectangle(picDesign.ClientSize);
                gridRectSize = tileInfo.GetGridSize(bounds);
            }
            else
            {
                if (GridSize <= 0)
                    return;
                gridRectSize = new SizeF(GridSize, GridSize);
            }
            GridTypes gridType = drawTileGrid ? GridTypes.Square : GridType;
            using (Pen pen = new Pen(Tools.InverseColor(Design.BackgroundColor)))
            {
                switch (gridType)
                {
                    case GridTypes.Square:
                        int xSteps = (int)Math.Round((float)bounds.Width / (float)gridRectSize.Width);
                        int ySteps = (int)Math.Round((float)bounds.Height / (float)gridRectSize.Height);
                        int startX;
                        int startY;
                        if (drawTileGrid)
                        {
                            startX = bounds.Left;
                            startY = bounds.Top;
                        }
                        else
                        {
                            startX = (int)(bounds.Width - (xSteps * gridRectSize.Width)) / 2;
                            startY = (int)(bounds.Height - (ySteps * gridRectSize.Height)) / 2;
                        }
                        int endX = (int)(startX + xSteps * gridRectSize.Width);
                        int endY = (int)(startY + ySteps * gridRectSize.Height);
                        if (drawTileGrid)
                        {
                            xSteps++;
                            ySteps++;
                        }
                        float y = startY;
                        for (int yStep = 0; yStep < ySteps; yStep++)
                        {
                            //Draw horizontal lines
                            g.DrawLine(pen, new PointF(startX, y), new PointF(endX, y));
                            y += gridRectSize.Height;
                        }
                        float x = startX;
                        for (int xStep = 0; xStep < xSteps; xStep++)
                        {
                            //Draw vertical lines
                            g.DrawLine(pen, new PointF(x, startY), new PointF(x, endY));
                            x += gridRectSize.Width;
                        }
                        break;
                    case GridTypes.Circular:
                        Point picCenter;
                        int radialSteps;
                        double radialAngle = GetGridRadialAngleAndInfo(out picCenter, out radialSteps);
                        //Draw circles
                        int maxRadius = Math.Max(picCenter.X, picCenter.Y);
                        int circleSteps = (int)Math.Ceiling((double)maxRadius / (double)GridSize);
                        maxRadius = circleSteps * GridSize;
                        for (int radius = maxRadius + 2; radius > 0; radius -= GridSize)
                        {
                            Rectangle circleRect = new Rectangle(picCenter.X - radius, picCenter.Y - radius, 2 * radius, 2 * radius);
                            g.DrawEllipse(pen, circleRect);
                        }
                        //Draw radial lines
                        Complex zRotation = Complex.CreateFromModulusAndArgument(1D, radialAngle);
                        Complex z = new Complex(1, 0);
                        for (int step = 0; step < radialSteps; step++)
                        {
                            Point pEnd = new Point((int)(z.Re * maxRadius) + picCenter.X, (int)(z.Im * maxRadius) + picCenter.Y);
                            Point pStart = new Point((int)(z.Re * 10) + picCenter.X, (int)(z.Im * 10) + picCenter.Y);
                            g.DrawLine(pen, pStart, pEnd);
                            z = z * zRotation;
                        }
                        break;
                }
            }
        }

        private Point GetPictureBoxCenter()
        {
            Rectangle bounds = PictureBoxRectangle;
            return new Point(bounds.Width / 2, bounds.Height / 2);
        }

        private double GetGridRadialAngleAndInfo(out Point picCenter, out int radialSteps)
        {
            picCenter = GetPictureBoxCenter();
            int radius = Math.Min(picCenter.X, picCenter.Y);
            Complex z = new Complex(radius, GridSize);
            double radialAngle = z.GetArgument();
            radialSteps = (int)Math.Floor(Math.PI / 2.0 / radialAngle);
            if (radialSteps <= 0)
                radialAngle = Math.PI / 2.0;
            else
                radialAngle = Math.PI / 2.0 / (double)radialSteps;
            radialSteps *= 4;
            return radialAngle;
        }

        private void RenderStained(List<Pattern> patterns)
        {
            Pattern.RenderStained(patterns, currentBitmap,
                                  squareSize: UseDraftMode ? 5 : 1);
        }

        private void DrawFilledPattern(PatternList patternGroup,
                                       bool computeRandom = true)
        {
            using (Graphics g = Graphics.FromImage(this.currentBitmap))
            {
                foreach (Pattern pattern in patternGroup.Patterns)
                    DrawPattern(pattern, g, computeRandom);
                //patternGroup.DrawFilled(g);
            }
            var patternList = patternGroup.PatternsList;
            RenderStained(Design.DesignPatterns.ToList());
            //if (patternList.Exists(ptn => ptn.RenderMode == Pattern.RenderModes.Stain))
            //    RenderStained(patternList);
            this.picDesign.Image = this.currentBitmap;
        }

        private float GetQualitySizeRatio()
        {
            int minDim = Math.Min(picDesign.ClientRectangle.Width,
                                  picDesign.ClientRectangle.Height);
            return (float)WhorlSettings.Instance.QualitySize / minDim;
            //List<Pattern> surroundPatterns =
            //    design.DesignPatterns.Where(ptn => ptn.FillInfo is PathFillInfo 
            //                            && ((PathFillInfo)ptn.FillInfo).ColorMode 
            //                            == FillInfo.PathColorModes.Surround);
            //double defaultRatio = (double)QualitySize / minDim;
            //if (surroundPatterns.Count == 0)
            //    return defaultRatio;
            //double minSize = surroundPatterns.Select(ptn => ptn.ZVector.GetModulus()).Min();
            //minSize = Math.Max(minSize, 10D);
            //double ratio = WhorlSettings.Instance.MinPatternSize / minSize;
            //ratio = Math.Min(ratio, 10 * defaultRatio);
            //return Math.Max(defaultRatio, ratio);
        }

        private float GetTextureScale()
        {
            return 1F; // / GetQualitySizeRatio();
        }
        
        private void RedrawPatternsAsync(bool computeRandom = false)
        {
            if (isRendering)
            {
                interruptRendering = true;
                renderCaller.CancelRender = true;
                do
                {
                    Application.DoEvents();
                } while (isRendering);
            }
            Task.Run(() => RedrawPatterns(computeRandom: computeRandom))
                            .ContinueWith(Tools.AsyncTaskFailed, 
                                          TaskContinuationOptions.OnlyOnFaulted);
        }

        private bool interruptRendering { get; set; }
        private bool isRendering { get; set; }

        private void RedrawPatterns(bool excludeSelected = false,
                            bool renderStained = true,
                            bool computeRandom = false,
                            bool drawLayers = true,
                            bool nonQualityMode = false,
                            IEnumerable<Pattern> overridePatterns = null,
                            bool computeSeedPoints = false)
        {
            try
            {
                picDesign.EnablePaint = false;
                renderCaller.CancelRender = false;
                isRendering = true;
                bool qualityMode = !nonQualityMode && !excludeSelected
                                && qualityModeToolStripMenuItem.Checked;
                if (excludeSelected || !renderStainedToolStripMenuItem.Checked)
                    renderStained = false;
                if (excludeSelected)
                    drawLayers = false;
                Stopwatch stopwatch;
                if (timeRedrawsToolStripMenuItem.Checked)
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }
                else
                    stopwatch = null;
                if (computeSeedPoints)
                {
                    foreach (Pattern pattern in Design.DesignPatterns)
                    {
                        pattern.ComputeSeedPoints();
                    }
                }
                if (qualityMode)
                {
                    Size prevSize = picDesign.ClientRectangle.Size;
                    float sizeRatio = GetQualitySizeRatio();
                    Size newSize = new Size((int)(sizeRatio * prevSize.Width),
                                            (int)(sizeRatio * prevSize.Height));
                    using (Bitmap bitmap = Design.RenderDesign(prevSize, newSize,
                                            scalePenWidth: false, draftMode: false,
                                            renderStained: false, caller: renderCaller))
                    {
                        currentBitmap = (Bitmap)BitmapTools.ScaleImage(bitmap, prevSize);
                    }
                }
                else
                {
                    CreateCurrentBitmap();
                    if (drawLayers)
                    {
                        DrawDesign.DrawDesignLayers(
                                    Design, currentBitmap, renderCaller,
                                    GetTextureScale(), computeRandom, overridePatterns, draftMode: UseDraftMode);
                    }
                    else
                    {
                        DrawDesign.RedrawPatterns(currentBitmap, Design, renderCaller, GetTextureScale(),
                                                    excludeSelected, computeRandom,
                                                    overridePatterns, draftMode: UseDraftMode);
                    }
                    if (renderCaller.CancelRender)
                    {
                        RenderCallback(0);
                        if (interruptRendering)
                            interruptRendering = false;
                        else
                            WriteStatus("Cancelled Render.");
                        return;
                    }
                }
                if (renderStained)
                    RenderStained(Design.DesignPatterns.ToList());
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    float seconds = 0.001F * stopwatch.ElapsedMilliseconds;
                    WriteStatus($"Rendered design in {seconds:0.00} seconds.");
                }
                SetPicDesignImage();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                picDesign.EnablePaint = true;
                isRendering = false;
            }
        }

        private void SetPicDesignImage()
        {
            if (InvokeRequired)
            {
                Invoke((Action)SetPicDesignImage);
            }
            picDesign.EnablePaint = true;
            picDesign.Image = currentBitmap;
        }

        private PatternForm patternForm = null;

        private bool EditPatternGroup(PatternList patternGroup)
        {
            try
            {
                if (patternForm == null || patternForm.IsDisposed)
                    patternForm = new PatternForm();
                patternForm.Initialize(patternGroup, selectPatternForm, Design);
                return (patternForm.ShowDialog() == DialogResult.OK);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
                return false;
            }
        }

        private void editDefaultPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditPatternGroup(Design.DefaultPatternGroup))
                Design.DefaultPatternGroup = patternForm.EditedPatternGroup;
        }

        //private void editSelectedPatternToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var selectedPatterns = design.DesignPatterns.Where(ptn => ptn.Selected);
        //        if (selectedPatterns.Count() != 1)
        //        {
        //            MessageBox.Show("Please select a single pattern to edit.");
        //            return;
        //        }
        //        Pattern selPattern = selectedPatterns.First();
        //        PatternList ptnGroup = new PatternList();
        //        ptnGroup.AddPattern(selPattern);
        //        if (EditPatternGroup(ptnGroup))
        //        {
        //            design.ReplacePatterns(ptnGroup.PatternsList, patternForm.EditedPatternGroup.PatternsList);
        //            RedrawPatterns();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}


        private string GetSaveDesignFileName(string currentFileName, string fileFolder = null)
        {
            return Tools.GetSaveXmlFileName("Design xml file (*.xml)", 
                         fileFolder ?? WhorlSettings.Instance.FilesFolder, 
                         currentFileName);
        }

        private bool SaveDesign(bool saveAs, string fileFolder = null)
        {
            bool doSave = true;
            string fileName = designFileName;
            if (saveAs || fileName == null)
            {
                fileName = GetSaveDesignFileName(fileName, fileFolder);
                if (fileName == null)
                    doSave = false;  //User cancelled.
            }
            if (doSave)
            {
                if (fileFolder == null)
                {
                    fileFolder = WhorlSettings.Instance.FilesFolder;
                    if (Path.GetDirectoryName(fileName) != fileFolder)
                    {
                        fileName = Path.Combine(fileFolder, Path.GetFileName(fileName));
                    }
                }
                Design.StartupAnimationNames = string.Join(",",
                    (from itm in startupAnimationMenuItems where itm.Checked select itm.Name));
                Design.SetPreviousSize();
                XmlTools.WriteToXml(fileName, Design);
                designFileName = fileName;
                Design.IsDirty = false;
                if (WhorlSettings.Instance.SaveDesignThumbnails && currentBitmap != null)
                {
                    SaveThumbnailImage(fileName);
                }
                WriteStatus($"Saved design file {fileName}.");
            }
            return doSave;
        }

        private void deleteDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (designFileName == null)
                {
                    MessageBox.Show("Please save the design first.");
                    return;
                }
                if (MessageBox.Show($"Delete file {Path.GetFileName(designFileName)}?",
                    "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
                if (File.Exists(designFileName))
                    File.Delete(designFileName);
                string thumbnailFileName = GetThumbnailFileName(designFileName);
                if (File.Exists(thumbnailFileName))
                {
                    File.Delete(thumbnailFileName);
                }
                designFileName = null;
                WriteStatus("Deleted Design File.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private string RenameDesignFile(string newFileName = null)
        {
            if (designFileName == null)
            {
                MessageBox.Show("Please save the design first.");
                return null;
            }
            if (newFileName == null)
            {
                newFileName = GetSaveDesignFileName(designFileName, Path.GetDirectoryName(designFileName));
                if (newFileName == null)
                    return null;  //User cancelled.
            }
            if (File.Exists(newFileName))
            {
                if (MessageBox.Show($"The file {newFileName} already exists.  Replace it?", "Confirm", MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                {
                    File.Delete(newFileName);
                }
                else
                    return null;
            }
            File.Move(designFileName, newFileName);
            string oldThumbnailFileName = GetThumbnailFileName(designFileName);
            string newThumbnailFileName = GetThumbnailFileName(newFileName);
            if (File.Exists(oldThumbnailFileName))
            {
                if (File.Exists(newThumbnailFileName))
                {
                    File.Delete(newThumbnailFileName);
                }
                File.Move(oldThumbnailFileName, newThumbnailFileName);
            }
            WriteStatus($"Renamed design file {designFileName} to {newFileName}.");
            return newFileName;
        }

        private void renameDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string newFileName = RenameDesignFile();
                if (newFileName != null)
                    designFileName = newFileName;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void moveDesignToFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (designFileName == null)
                {
                    MessageBox.Show("Please save the design first.");
                    return;
                }
                var menuItem = (ToolStripMenuItem)sender;
                string subfolder = (string)menuItem.Tag;
                string fileFolder = Path.Combine(WhorlSettings.Instance.FilesFolder,
                                    WhorlSettings.Instance.CustomDesignParentFolder, subfolder);
                string newFileName = Path.Combine(fileFolder, Path.GetFileName(designFileName));
                if (newFileName == designFileName)
                {
                    MessageBox.Show($"The design is already saved in folder {subfolder}.");
                    return;
                }
                RenameDesignFile(newFileName);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private string GetThumbnailFileName(string xmlFileName)
        {
            string thumbnailsFolder = Path.Combine(
                Path.GetDirectoryName(xmlFileName), 
                WhorlSettings.Instance.DesignThumbnailsFolder);
            if (!Directory.Exists(thumbnailsFolder))
                Directory.CreateDirectory(thumbnailsFolder);
            string thumbnailFileName = Path.GetFileNameWithoutExtension(xmlFileName) + ".jpg";
            return Path.Combine(thumbnailsFolder, thumbnailFileName);
        }

        private void SaveThumbnailImage(string xmlFileName)
        {
            string thumbnailFileName = GetThumbnailFileName(xmlFileName);
            int height = WhorlSettings.Instance.DesignThumbnailHeight;
            Size newSize = new Size(currentBitmap.Width * height / currentBitmap.Height, height);
            var thumbnailBitmap = (Bitmap)BitmapTools.ScaleImage(currentBitmap, newSize);
            Tools.SavePngOrJpegImageFile(thumbnailFileName, thumbnailBitmap);
        }

        private void saveDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveDesign(saveAs: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void saveDesignAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveDesign(saveAs: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void saveDesignToFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    var menuItem = (ToolStripMenuItem)sender;
                    string subfolder = (string)menuItem.Tag;
                    string fileFolder = Path.Combine(WhorlSettings.Instance.FilesFolder, 
                                        WhorlSettings.Instance.CustomDesignParentFolder, subfolder);
                    SaveDesign(saveAs: true, fileFolder);
                }
                catch (Exception ex)
                {
                    Tools.HandleException(ex);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private bool CheckSaveDesign()
        {
            if (!Design.IsDirty)
                return true;
            switch (MessageBox.Show("Save changes to design?", "Confirm",
                                    MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    return SaveDesign(saveAs: false);
                case DialogResult.No:
                    return true;
                default:
                    return false;
            }
        }

        private void SetDesignStartupItems()
        {
            string startupItemNames = Design.StartupAnimationNames;
            if (startupItemNames == null)
                startupItemNames = string.Empty;
            string[] itemNames = startupItemNames.Split(',');
            foreach (ToolStripMenuItem startupItem in startupAnimationMenuItems)
            {
                startupItem.Checked = Array.IndexOf(itemNames, startupItem.Name) >= 0;
            }
        }

        private OpenFileDialog GetOpenDesignFileDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Design xml file (*.xml)|*.xml";
            if (FilesFolderExists())
                dlg.InitialDirectory = WhorlSettings.Instance.FilesFolder;
            return dlg;
        }

        private DialogResult ShowDialog(OpenFileDialog dialog)
        {
            if (InvokeRequired)
            {
                Invoke((Func<OpenFileDialog, DialogResult>)ShowDialog, dialog);
            }
            return dialog.ShowDialog();
        }

        /// <summary>
        /// Open design from XML file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void openDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CheckSaveDesign())
                    return;  //User cancelled.
                OpenFileDialog dlg = GetOpenDesignFileDialog();
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                   await OpenDesignFromXml(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ClearTileGrid()
        {
            tileInfoPattern = null;
            showTileGridToolStripMenuItem.Checked = false;
        }

        private void InitializeForDesign()
        {
            influencePointsPattern = null;
            showInfluencePointsDistancePattern = null;
            showInfluencePointsToolStripMenuItem.Checked = false;
            showDistanceInfluencePointsToolStripMenuItem.Checked = false;
            editInfluencePointsModeToolStripMenuItem.Checked = false;
        }

        private async Task OpenDesignFromXml(string fileName)
        {
            try
            {
                picDesign.EnablePaint = false;
                StopAnimating();
                PatternList defaultPatternGroup = Design.DefaultPatternGroup;
                Design.ReadDesignFromXmlFile(fileName, showWarnings: true);
                WriteStatus($"Opened design file {fileName}.");
                Design.EditedPattern = null;
                InitializeForDesign();
                Design.DefaultPatternGroup = defaultPatternGroup;
                SetDesignStartupItems();
                designLayersForm.SetDesign(Design);
                await DisplayDesignAsync();
                designFileName = fileName;
                int index = 0;
                pauseImprovToolStripMenuItem.Checked = false;
                foreach (ToolStripMenuItem startupItem in startupAnimationMenuItems)
                {
                    if (startupItem.Checked)
                        animationMenuItems[index].Checked = true;
                    index++;
                }
                if (Animating)
                {
                    AnimatingChanged(true);
                }
                Design.IsDirty = false;
                ClearTileGrid();
                if (Design.DesignPatterns.Any(p => p.HasPixelRendering))
                {
                    ShowRenderingPanels(true);
                }
            }
            finally
            {
                picDesign.EnablePaint = true;
            }
        }


        private async Task<string> OpenDesignFromImage(string imagesFolder)
        {
            if (!CheckSaveDesign())
                return null;  //User cancelled.
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Design image file (*.jpg)|*.jpg";
            if (imagesFolder != null && Directory.Exists(imagesFolder))
                dlg.InitialDirectory = imagesFolder;
            if (ShowDialog(dlg) == DialogResult.OK)
            {
                //Get XML file name:
                string xmlFolder = Path.GetDirectoryName(Path.GetDirectoryName(dlg.FileName));
                string xmlFileName = Path.Combine(xmlFolder, Path.GetFileNameWithoutExtension(dlg.FileName) + ".xml");
                if (!File.Exists(xmlFileName))
                {
                    MessageBox.Show($"Design XML file was not found: {xmlFileName}.");
                }
                else
                {
                    await OpenDesignFromXml(xmlFileName);
                }
                return dlg.FileName;
            }
            else
                return null;
        }

        /// <summary>
        /// Open design via thumbnail image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void openDesignFromImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string thumbnailsFolder = Path.Combine(WhorlSettings.Instance.FilesFolder, WhorlSettings.Instance.DesignThumbnailsFolder);
                await OpenDesignFromImage(thumbnailsFolder);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        /// <summary>
        /// Open XML design file based on selected image file, from subfolder of files folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void openDesignFromFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var menuItem = (ToolStripMenuItem)sender;
                string folder = (string)menuItem.Tag;
                string thumbnailsFolder = Path.Combine(WhorlSettings.Instance.FilesFolder, 
                    WhorlSettings.Instance.CustomDesignParentFolder, folder, 
                    WhorlSettings.Instance.DesignThumbnailsFolder);
                await OpenDesignFromImage(thumbnailsFolder);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private string otherImagesFolder = null;

        private async void openDesignFromOtherImageFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string imageFileName = await OpenDesignFromImage(otherImagesFolder);
                if (imageFileName != null)
                    otherImagesFolder = Path.GetDirectoryName(imageFileName);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void InitDisplayDesign()
        {
            if (InvokeRequired)
                Invoke((Action)InitDisplayDesign);
            if (Design.PictureBoxSize.Width >= origPictureBoxSize.Width)
            {
                DockPictureBox();
            }
            else
            {
                SetPictureBoxSize(Design.PictureBoxSize);
            }
            scaleToFitToolStripMenuItem.Checked = Design.ScaleToFit;
            ClearBackgroundGraphicsPath();
        }

        private void DisplayDesign()
        {
            try
            {
                picDesign.EnablePaint = false;
                InitDisplayDesign();
                RedrawPatterns();
            }
            finally
            {
                picDesign.EnablePaint = true;
            }
        }

        private async Task DisplayDesignAsync()
        {
            InitDisplayDesign();
            await Task.Run(() => RedrawPatterns());
        }

        private string GetDefaultImageFileName()
        {
            if (designFileName == null)
                return string.Empty;
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(designFileName);
                return fileName + (saveImagesAsJpegFilesToolStripMenuItem.Checked ?
                                   ".jpg" : ".png");
            }
        }

        private void saveDesignImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentBitmap == null)
                {
                    MessageBox.Show("There is no image to save.", "Message");
                    return;
                }
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Image file (*.png;*.jpg)|*.png;*.jpg;*.jpeg";
                if (FilesFolderExists())
                    dlg.InitialDirectory = WhorlSettings.Instance.FilesFolder;
                if (designFileName != null)
                {
                    dlg.FileName = GetDefaultImageFileName();
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp;
                    if (removeBorderLinesOnSavingToolStripMenuItem.Checked)
                    {
                        var rect = new Rectangle(0, 0, currentBitmap.Width - 2, currentBitmap.Height - 2);
                        bmp = (Bitmap)currentBitmap.Clone(rect, PixelFormat.Format32bppPArgb);
                    }
                    else
                        bmp = currentBitmap;
                    Tools.SavePngOrJpegImageFile(dlg.FileName, bmp);
                    WriteStatus("Saved design image file.");
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void saveImprovisationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (improvDesigns.Count == 0)
                    return;
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Improvisations file (*.improv)|*.improv";
                if (FilesFolderExists())
                    dlg.InitialDirectory = WhorlSettings.Instance.FilesFolder;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    WhorlDesign.WriteDesignListXml(dlg.FileName, improvDesigns);
                    WriteStatus("Saved improvisations file.");
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void openImprovisationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Filter = "Improvisations file (*.improv)|*.improv";
                    if (FilesFolderExists())
                        dlg.InitialDirectory = WhorlSettings.Instance.FilesFolder;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        improvDesigns = WhorlDesign.ReadDesignListFromXmlFile(dlg.FileName, ContainerSize);
                        replayImprovisationsToolStripMenuItem.Checked = true;
                        replayImprovisationsToolStripMenuItem_Click(null, null);
                    }
                }
                catch (Exception ex)
                {
                    Tools.HandleException(new Exception("Error reading improvisations file.", ex));
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private bool isAnimating = false;
        private ImproviseFlags defaultImprovFlags;
        private ImproviseFlags improvFlags;

        private void AnimationDrawDesign()
        {
            if (animateOnSelectedPatternsToolStripMenuItem.Checked)
                RedrawPatterns(excludeSelected: true);  //Animation will draw selected patterns.
            else
            {
                CreateCurrentBitmap();
                picDesign.Image = currentBitmap;
            }
        }

        //private int SeedImprovRandom(int? seed = null)
        //{
        //    if (seed == null)
        //        seed = RandomGenerator.GetNewSeed();
        //    improvRandom = new Random((int)seed);
        //    return (int)seed;
        //}

        private void AnimatingChanged(bool isChecked)
        {
            try
            {
                if (isChecked)
                {
                    if (!Design.DesignPatterns.Any())
                    {
                        StopAnimating();
                    }
                    else
                    {
                        pauseImprovToolStripMenuItem.Checked = false;
                        if (!isAnimating)
                        {
                            isAnimating = true;
                            origDesign = Design;
                            SetDesign(new WhorlDesign(Design));  //Clone design
                            parameterIncrements.Clear();
                            defaultImprovFlags = new ImproviseFlags();
                            //parameterImprovStep = 0;
                            AnimationDrawDesign();
                        }
                        animationStep = 0;
                        this.stopAnimatingToolStripMenuItem.Enabled = true;
                        this.resetDesignToolStripMenuItem.Enabled = false;
                        spinRotationVector = Complex.CreateFromModulusAndArgument(1D,
                            WhorlSettings.Instance.SpinRate * (Math.PI / 180D));
                        revolveRotationVector = Complex.CreateFromModulusAndArgument(1D,
                            WhorlSettings.Instance.RevolveRate * (Math.PI / 180D));
                        smoothAnimations = smoothAnimationModeToolStripMenuItem.Checked &&
                            (improviseToolStripMenuItem.Checked ||
                             animateRecomputePatternsToolStripMenuItem.Checked ||
                             colorsToolStripMenuItem.Checked);
                        smoothAnimationStep = 0;
                        if (WhorlSettings.Instance.AnimationRate > 0)
                            AnimationTimer.Interval = 1000 / WhorlSettings.Instance.AnimationRate;
                        AnimationTimer.Start();
                        Animate(improvDesignsIndex);
                    }
                }
                else
                {
                    if (!Animating)
                    {
                        isAnimating = false;
                        AnimationTimer.Stop();
                        pauseImprovToolStripMenuItem.Checked = false;
                        this.stopAnimatingToolStripMenuItem.Enabled = false;
                        this.resetDesignToolStripMenuItem.Enabled = true;
                        RedrawPatterns();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void spinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimatingChanged(spinToolStripMenuItem.Checked);
        }

        private void swayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimatingChanged(swayToolStripMenuItem.Checked);
        }

        private void revolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimatingChanged(revolveToolStripMenuItem.Checked);
        }

        private void animateRecomputePatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimatingChanged(animateRecomputePatternsToolStripMenuItem.Checked);
        }

        private void colorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimatingChanged(colorsToolStripMenuItem.Checked);
        }

        private void improviseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (improviseToolStripMenuItem.Checked)
            {
                switch (MessageBox.Show("Add new random seed?", "Confirm", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        improvisationToolStripMenuItem.Checked = false;
                        return;
                    case DialogResult.Yes:
                        Design.AddNamedRandomSeed(seededRandom);
                        break;
                }
                ClearImprovDesigns();
            }
            AnimatingChanged(improviseToolStripMenuItem.Checked);
        }

        private void ClearImprovDesigns()
        {
            foreach (WhorlDesign dgn in improvDesigns)
            {
                if (dgn != this.Design)
                    dgn.Dispose();
            }
            improvDesigns.Clear();
        }

        private int improvDesignsIndex = 0;

        private bool DisplayImprovisation(int index)
        {
            bool retVal = (improvDesignsIndex >= 0 && 
                           improvDesignsIndex < improvDesigns.Count);
            if (retVal)
            {
                SetDesign(improvDesigns[improvDesignsIndex]);
                RedrawPatterns();
            }
            return retVal;
        }

        private void replayImprovisationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (replayImprovisationsToolStripMenuItem.Checked)
                {
                    if (improvDesigns.Count == 0)
                        return;
                    StopAnimating();
                    improvDesignsIndex = 0;
                    WriteStatus("Playing improvisations...");
                    AnimationTimer.Start();
                    Animate(improvDesignsIndex);
                }
                else
                {
                    AnimationTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Complex swayVec = new Complex(1, 0);
        private List<double> randomFactors = null;
        private double[] frequencyFactors = null;

        private List<Pattern> AnimationPatterns
        {
            get
            {
                if (animateOnSelectedPatternsToolStripMenuItem.Checked)
                    return Design.DesignPatterns.Where(ptn => ptn.Selected).ToList();
                else
                    return Design.DesignPatterns.ToList();
            }
        }

        private class SmoothAnimationPatternInfo: IDisposable
        {
            public Pattern OrigPattern { get; private set; }
            public Pattern InterpolatedPattern { get; private set; }
            public ColorGradient BoundaryGradient { get; private set; }
            public ColorGradient CenterGradient { get; private set; }

            private Pattern CopySmoothPattern(Pattern pattern, Pattern copy)
            {
                if (copy != null)
                    copy.Dispose();
                copy = pattern.GetCopy();
                copy.CopySeedPoints(pattern);
                copy.DesignLayer = pattern.DesignLayer;
                return copy;
            }

            public void Initialize(Pattern pattern)
            {
                OrigPattern = CopySmoothPattern(pattern, OrigPattern);
                InterpolatedPattern = CopySmoothPattern(pattern, InterpolatedPattern);
                InterpolatedPattern.SetSeedPointsChanged();
            }

            public void InitializeGradients(Pattern nextPattern, int smoothSteps)
            {
                if (BoundaryGradient == null)
                    BoundaryGradient = new ColorGradient();
                if (CenterGradient == null)
                    CenterGradient = new ColorGradient();
                BoundaryGradient.Initialize(steps: smoothSteps, color1: OrigPattern.BoundaryColor, color2: nextPattern.BoundaryColor);
                CenterGradient.Initialize(steps: smoothSteps, color1: OrigPattern.CenterColor, color2: nextPattern.CenterColor);
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (OrigPattern != null)
                            OrigPattern.Dispose();
                        if (InterpolatedPattern != null)
                            InterpolatedPattern.Dispose();
                    }
                    disposedValue = true;
                }
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }

        private SmoothAnimationPatternInfo[] smoothPatternsArray;

        private SmoothAnimationPatternInfo GetSmoothPatternInfo(int i)
        {
            return (i < smoothPatternsArray.Length ? smoothPatternsArray[i] : null);
        }

        private void DisposeSmoothPatternsArray()
        {
            if (smoothPatternsArray != null)
            {
                for (int i = 0; i < smoothPatternsArray.Length; i++)
                {
                    smoothPatternsArray[i].Dispose();
                }
                smoothPatternsArray = null;
            }
        }

        private bool smoothAnimations;
        private int smoothAnimationStep;
        private bool displayInterpolatedPatterns = false;
        private int randomFactorInd;

        private void Animate(int improvIndex, bool replay = false)
        {
            if (ReplayImprovisation(improvIndex, replay))
                return;
            List<Pattern> animationPatterns = this.AnimationPatterns;
            randomFactorInd = 0;
            bool redraw = false;
            if (WhorlSettings.Instance.ImproviseBackground && improviseToolStripMenuItem.Checked)
            {   //Improvise on design background
                ImproviseOnBackground();
                redraw = true;
            }
            //if (improviseToolStripMenuItem.Checked)
            //    improvRandomSeeds.Add(SeedImprovRandom());
            if (animationPatterns.Count == 0)
            {
                if (redraw)
                {
                    AddToImprovDesigns();
                    DrawAnimation(animationPatterns);
                }
                return;
            }
            if (smoothAnimations)
            {
                int smoothSteps = WhorlSettings.Instance.SmoothAnimationSteps;
                bool smoothColors = colorsToolStripMenuItem.Checked ||
                                    (improviseToolStripMenuItem.Checked && 
                                     WhorlSettings.Instance.ImproviseColors);
                if (smoothAnimationStep == 0)
                {
                    InitSmoothAnimation(animationPatterns); //Clone patterns
                    TransformAnimationPatterns(animationPatterns);
                    AddToImprovDesigns();
                    if (smoothColors)
                        InitSmoothColors(animationPatterns, smoothSteps);
                }
                DisplayInterpolatedPatterns(animationPatterns,
                                            smoothAnimationStep, smoothSteps, smoothColors);
                if (++smoothAnimationStep > smoothSteps)
                    smoothAnimationStep = 0;
            }
            else
            {
                TransformAnimationPatterns(animationPatterns);
                AddToImprovDesigns();
                DrawAnimation(animationPatterns);
            }
        }

        private void AddToImprovDesigns()
        {
            animationStep++;
            if (improviseToolStripMenuItem.Checked)
            {
                WriteStatus("Animation step: " + animationStep);
                if (!keepHistoryToolStripMenuItem.Checked)
                    return;
                if (WhorlSettings.Instance.MaxSavedImprovs > 0 &&
                    improvDesigns.Count >= WhorlSettings.Instance.MaxSavedImprovs)
                {
                    WhorlDesign dgn = improvDesigns[0];
                    dgn.Dispose();
                    improvDesigns.RemoveAt(0);
                }
                var designCopy = new WhorlDesign(Design);  //Clone design
                improvDesigns.Add(designCopy);
                improvDesignsIndex = improvDesigns.Count - 1;

                //Test code:
                //foreach (Pattern ptn in designCopy.DesignPatterns)
                //{
                //    if (ptn.SeedPoints != null || ptn.CurvePoints != null)
                //    {
                //        MessageBox.Show("Non-null SeedPoints or CurvePoints for copied design.");
                //        break;
                //    }
                //}
            }
        }

        private bool ReplayImprovisation(int improvIndex, bool replay)
        {
            if (replayImprovisationsToolStripMenuItem.Checked || replay)
            {
                if (DisplayImprovisation(improvIndex))
                {
                    WriteStatus($"Improvisation step: {improvIndex + 1}");
                }
                else
                {
                    AnimationTimer.Stop();
                    replayImprovisationsToolStripMenuItem.Checked = false;
                }
                return true;
            }
            return false;
        }

        private void InitSmoothAnimation(List<Pattern> animationPatterns)
        {
            if (smoothPatternsArray == null ||
                smoothPatternsArray.Length != animationPatterns.Count)
            {
                DisposeSmoothPatternsArray();
                smoothPatternsArray = new SmoothAnimationPatternInfo[animationPatterns.Count];
                for (int i = 0; i < smoothPatternsArray.Length; i++)
                    smoothPatternsArray[i] = new SmoothAnimationPatternInfo();
            }
            //Clone current patterns
            for (int i = 0; i < animationPatterns.Count; i++)
            {
                SmoothAnimationPatternInfo ptnInfo = smoothPatternsArray[i];
                Pattern pattern = animationPatterns[i];
                if (pattern.SeedPoints == null)
                    pattern.ComputeSeedPoints();
                ptnInfo.Initialize(pattern);
            }
        }

        private void InitSmoothColors(List<Pattern> animationPatterns, int smoothSteps)
        {
            for (int i = 0; i < animationPatterns.Count; i++)
            {
                SmoothAnimationPatternInfo ptnInfo = smoothPatternsArray[i];
                Pattern pattern = animationPatterns[i];
                ptnInfo.InitializeGradients(pattern, smoothSteps);
            }
        }

        private void DisplayInterpolatedPatterns(List<Pattern> animationPatterns,
                                  int curStep, int smoothSteps, bool smoothColors)
        {
            bool changed = false;
            if (curStep == 0)
                changed = true;
            else
            {
                float factor = (float)curStep / smoothSteps;
                for (int ptnInd = 0; ptnInd < animationPatterns.Count; ptnInd++)
                {
                    InterpolatePattern(animationPatterns, ptnInd, factor, smoothColors,
                                       ref changed);
                }
            }
            if (changed)
            {
                displayInterpolatedPatterns = true;
                DrawAnimation(animationPatterns);
                //WriteStatus($"Drew Smooth Pattern for Step = {curStep}.");
            }
        }

        private void DrawAnimation(List<Pattern> animationPatterns)
        {
            bool drawLayers = ShouldDrawDesignLayersForAnimate();
            if (/*VideoOps.IsRecording ||*/ drawLayers
                || WhorlSettings.Instance.ImproviseBackground)
            {
                AnimationTimer.Stop();
                List<Pattern> overridePatterns = null;
                if (displayInterpolatedPatterns)
                {
                    displayInterpolatedPatterns = false;
                    overridePatterns = new List<Pattern>(Design.DesignPatterns);
                    for (int i = 0; i < animationPatterns.Count; i++)
                    {
                        var smoothPattern = GetSmoothPatternInfo(i);
                        if (smoothPattern != null)
                        {
                            int ind = Design.IndexOfPattern(animationPatterns[i]);
                            if (ind >= 0)
                                overridePatterns[i] = smoothPattern.InterpolatedPattern;
                        }
                    }
                }
                RedrawPatterns(renderStained: false, drawLayers: drawLayers, 
                               overridePatterns: overridePatterns);
                //if (VideoOps.IsRecording)
                //{
                //    VideoOps.AddFrame(currentBitmap);
                //}
                AnimationTimer.Start();
            }
            else
            {
                picDesign.Invalidate();
            }
        }

        private bool ShouldDrawDesignLayersForAnimate()
        {
            var improvConfig = Design.ImproviseConfig;
            return improvConfig != null && improvConfig.Enabled && improvConfig.DrawDesignLayers
                   && Design.DesignLayerList.DesignLayers.Any();
        }

        private void InterpolatePattern(List<Pattern> animationPatterns, 
                                        int ptnInd, float factor, bool smoothColors,
                                        ref bool changed)
        {
            SmoothAnimationPatternInfo ptnInfo = GetSmoothPatternInfo(ptnInd);
            if (ptnInfo == null)
                return;
            Pattern pattern = animationPatterns[ptnInd];
            Pattern origPattern = ptnInfo.OrigPattern;
            Pattern interPattern = ptnInfo.InterpolatedPattern;
            //Interpolate complex vector:
            Complex zDelta = pattern.ZVector - origPattern.ZVector;
            if (zDelta != Complex.Zero)
            {
                interPattern.ZVector = origPattern.ZVector + (double)factor * zDelta;
                changed = true;
            }
            //Interpolate polar SeedPoints coordinates:
            bool seedPointsChanged = false;
            for (int i = 0; i < pattern.SeedPoints.Length; i++)
            {
                PolarCoord origCoord = origPattern.SeedPoints[i];
                PolarCoord newCoord = pattern.SeedPoints[i];
                float deltaMod = newCoord.Modulus - origCoord.Modulus;
                float deltaAngle = newCoord.Angle - origCoord.Angle;
                if (deltaMod != 0 || deltaAngle != 0)
                {
                    seedPointsChanged = true;
                    interPattern.SeedPoints[i] = new PolarCoord(
                        origCoord.Angle + factor * deltaAngle,
                        origCoord.Modulus + factor * deltaMod);
                }
            }
            if (smoothColors &&
                ptnInfo.BoundaryGradient != null && ptnInfo.CenterGradient != null)
            {
                interPattern.BoundaryColor = ptnInfo.BoundaryGradient.GetCurrentColor();
                interPattern.CenterColor = ptnInfo.CenterGradient.GetCurrentColor();
                changed = true;
            }
            if (seedPointsChanged)
            {
                changed = true;
                interPattern.SetSeedPointsChanged();
            }
        }

        private void TransformAnimationPatterns(List<Pattern> animationPatterns)
        {
            //randomFactorInd = 0;
            int patternCount = animationPatterns.Count;
            for (int patternInd = 0; patternInd < animationPatterns.Count; patternInd++)
            {
                Pattern pattern = animationPatterns[patternInd];
                //if (animateOnSelectedPatternsToolStripMenuItem.Checked && !pattern.Selected)
                //    continue;
                //redraw = true;
                if (spinToolStripMenuItem.Checked)
                {
                    pattern.ZVector = pattern.ZVector * spinRotationVector;
                }
                if (revolveToolStripMenuItem.Checked)
                {
                    Point picCenter = GetPictureBoxCenter();
                    Ribbon ribbon = pattern as Ribbon;
                    if (ribbon != null)
                    {
                        for (int i = 0; i < ribbon.RibbonPath.Count; i++)
                        {
                            PointF p = ribbon.RibbonPath[i];
                            Complex z = new Complex(p.X - picCenter.X, p.Y - picCenter.Y);
                            z = z * revolveRotationVector;
                            ribbon.RibbonPath[i] = 
                                new PointF(picCenter.X + (float)z.Re, 
                                           picCenter.Y + (float)z.Im);
                        }
                    }
                    else
                    {
                        Complex z = new Complex(pattern.Center.X - picCenter.X, 
                                                pattern.Center.Y - picCenter.Y);
                        z = z * revolveRotationVector;
                        pattern.ZVector = pattern.ZVector * revolveRotationVector;
                        pattern.Center = new PointF(picCenter.X + (float)z.Re, 
                                                    picCenter.Y + (float)z.Im);
                    }
                }
                if (swayToolStripMenuItem.Checked)
                {
                    swayVec = swayVec * swayRotationVector;
                    pattern.Center = new PointF(pattern.Center.X + 3 * (float)swayVec.Re,
                                                pattern.Center.Y + 3 * (float)swayVec.Im);
                }
                if (colorsToolStripMenuItem.Checked)
                {
                    bool changeColors;
                    if (smoothAnimations)
                        changeColors = true; // animationStep % patternCount == patternInd;
                    else
                        changeColors = animationStep % 8 == 0
                            && (animationStep / 8) % patternCount == patternInd;
                    if (changeColors)
                    {
                        Color newColor = GetNextColor();
                        if (pattern.BoundaryColor.ToArgb() == newColor.ToArgb())
                            newColor = GetNextColor();
                        pattern.BoundaryColor = newColor;
                        pattern.CenterColor = GetNextColor(increment: false);
                    }
                }
                bool computeSeedPoints = animateRecomputePatternsToolStripMenuItem.Checked;
                if (improviseToolStripMenuItem.Checked)
                {
                    if (ImproviseOnProperties(pattern, patternInd))
                        computeSeedPoints =  true;
                }
                if (computeSeedPoints)
                {
                    pattern.ComputeSeedPoints();
                }
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (slideShowToolStripMenuItem.Checked)
                {
                    ShowSlide();
                    return;
                }
                bool replaying = replayImprovisationsToolStripMenuItem.Checked;
                if (replaying)
                    improvDesignsIndex++;
                try
                {
                    AnimationTimer.Stop();
                    Animate(improvDesignsIndex);
                }
                finally
                {
                    AnimationTimer.Start();
                }
                if (replaying && !replayImprovisationsToolStripMenuItem.Checked)
                {
                    WriteStatus("Finished replaying improvisations.");
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private double GetRandomInRange(double minVal, double maxVal)
        {
            return minVal + (maxVal - minVal) * improvRandom.NextDouble();
        }

        private double GetRandomFactor(double scaleFac)
        {
            return 1D - scaleFac + (2 * scaleFac * improvRandom.NextDouble());
        }

        private double GetRandomFactor(bool recompute, double scaleFac)
        {
            double factor;
            if (recompute || randomFactorInd >= randomFactors.Count)
            {
                factor = GetRandomFactor(scaleFac);
                if (randomFactorInd < randomFactors.Count)
                    randomFactors[randomFactorInd] = factor;
                else
                {
                    while (randomFactorInd >= randomFactors.Count)
                        randomFactors.Add(factor);
                }
            }
            else
                factor = randomFactors[randomFactorInd];
            return factor;
        }

        private double ApplyRandomFactor(double value, bool recompute,
                                         double minValue, double maxValue, double scaleFac = 0)
        {
            if (scaleFac == 0)
            {
                scaleFac = WhorlSettings.Instance.ImprovisationLevel;
            }
            double factor = GetRandomFactor(recompute, scaleFac);
            double newValue = value * factor;
            bool invertFactor = false;
            if (recompute)
            {
                double midVal = minValue + 0.5 * (maxValue - minValue);
                if (factor < 1 == newValue < midVal)
                    invertFactor = true;
            }
            else if (newValue < minValue || newValue > maxValue)
                invertFactor = true;
            if (invertFactor)
            {
                factor = 1D / factor;
                newValue = value * factor;
                randomFactors[randomFactorInd] = factor;
            }
            randomFactorInd++;
            newValue += (factor - 1D) / 
                        (WhorlSettings.Instance.ImprovDamping + newValue * newValue);
            return Math.Max(minValue, Math.Min(maxValue, newValue));
        }

        private double AdjustValue(double value, double factor, double minValue, double maxValue)
        {
            if ((factor > 1 && value >= maxValue) || (factor < 1 && value <= minValue))
                factor = 1D / factor;
            return value * factor;
        }

        //int GetRandomRGB(int colorVal, bool recompute, double scaleFac, bool dark)
        //{
        //    const double midVal = 128D;
        //    if (colorVal < 10 && !dark)
        //        colorVal += 10;
        //    double randFac = GetRandomFactor(recompute, scaleFac);
        //    if (!dark)
        //    {
        //        randFac += 0.5 * scaleFac;
        //        if (recompute)
        //        {
        //            double adjustment = (scaleFac * (midVal - (double)colorVal) / midVal);
        //            randFac += adjustment;
        //            randomFactors[randomFactorInd] = randFac;
        //        }
        //    }
        //    double newVal = (double)colorVal * randFac;
        //    randomFactorInd++;
        //    return Math.Max(0, Math.Min(255, (int)newVal));
        //}

        int GetRandomRGB(int colorVal, bool recompute, double scaleFac, bool dark)
        {
            if (!dark && colorVal < 10)
                colorVal += 10;
            double newValue = ApplyRandomFactor((double)colorVal, recompute, 0, 255);
            if (!dark && newValue < 128D)
            {
                newValue *= (2 - Math.Tanh(newValue / 64D));
                newValue = Math.Min(255D, newValue);
            }
            randomFactorInd++;
            return (int)newValue;
        }

        Color ImproviseColor(Color color, bool recompute, double scaleFac = 0.2, bool dark = false)
        {
            int red = GetRandomRGB(color.R, recompute, scaleFac, dark);
            int green = GetRandomRGB(color.G, recompute, scaleFac, dark);
            int blue = GetRandomRGB(color.B, recompute, scaleFac, dark);
            return Color.FromArgb(color.A, red, green, blue);
        }

        private static BasicOutlineTypes[] basicOutlineVals =
            (from BasicOutlineTypes v in
                 Enum.GetValues(typeof(BasicOutlineTypes))
                 where v != BasicOutlineTypes.Custom &&
                       v != BasicOutlineTypes.Path
             select v).ToArray();

        private PatternImproviseConfig patternImprovConfig;

        private Ribbon GetImprovRibbon(Pattern pattern, out bool isPathRibbon)
        {
            Ribbon ribbon = pattern as Ribbon;
            isPathRibbon = false;
            if (ribbon == null)
            {
                PathPattern pathPattern = pattern as PathPattern;
                if (pathPattern != null)
                {
                    ribbon = pathPattern.PathRibbon;
                    isPathRibbon = ribbon != null;
                }
            }
            return ribbon;
        }

        private string[] textureFileNames { get; set; } = null;

        private string GetRandomTextureFileName()
        {
            if (textureFileNames == null)
            {
                string texturesFolder = Tools.GetTexturesFolder();
                if (Directory.Exists(texturesFolder))
                {
                    var fileNames = Directory.EnumerateFiles(texturesFolder, "*.png").Concat(
                                    Directory.EnumerateFiles(texturesFolder, "*.jpg"));
                    textureFileNames = fileNames.ToArray();
                }
                else
                    return null;
            }
            if (textureFileNames.Length == 0)
                return null;
            double dRandom = improvRandom.NextDouble();  //Between 0 and 1.
            int index = (int)Math.Floor(0.99999 * dRandom * textureFileNames.Length);
            return textureFileNames[index];
        }

        private void ImproviseOnBackground()
        {
            int recomputeSteps = GetRecomputeSteps();
            bool recomputeFacs = (randomFactors == null
                                  || animationStep % recomputeSteps == 0);
            if (randomFactors == null)
            {
                randomFactors = new List<double>();
            }
            if (string.IsNullOrEmpty(Design.BackgroundImageFileName))
            {
                Design.BackgroundGradientColors.BoundaryColor
                    = ImproviseColor(Design.BackgroundGradientColors.BoundaryColor,
                                        recomputeFacs);
                Design.BackgroundGradientColors.CenterColor
                    = ImproviseColor(Design.BackgroundGradientColors.CenterColor,
                                        recomputeFacs);
            }
            else if (WhorlSettings.Instance.ImproviseTextures)
            {
                string textureFileName = GetRandomTextureFileName();
                if (textureFileName != null)
                    Design.BackgroundImageFileName = textureFileName;
            }
        }

        private int GetRecomputeSteps()
        {
            return (int)(WhorlSettings.Instance.RecomputeInterval 
                         * 1000.0 / AnimationTimer.Interval);
        }

        private bool ImproviseOnProperties(Pattern pattern, int patternInd)
        {
            int recomputeSteps = GetRecomputeSteps();
            bool recomputeFacs = (randomFactors == null
                                  || animationStep % recomputeSteps == 0);
            if (randomFactors == null)
            {
                randomFactors = new List<double>();
            }
            improvFlags = defaultImprovFlags;
            patternImprovConfig = pattern.PatternImproviseConfig;
            bool useDesignConfig = Design.ImproviseConfig != null && Design.ImproviseConfig.Enabled;
            if (patternImprovConfig != null)
            {
                if (patternImprovConfig.Enabled)
                {
                    if (useDesignConfig)
                        improvFlags = patternImprovConfig.ImproviseFlags;
                }
                else
                    patternImprovConfig = null;
            }
            if (Design.ImproviseConfig != null && Design.ImproviseConfig.Enabled)
            {
                if (!Design.ImproviseConfig.ImproviseOnAllPatterns && patternImprovConfig == null)
                    return false;
            }
            else
                patternImprovConfig = null;
            bool changeFrequency = improvFlags.UsedImprovisePetals;
            if (frequencyFactors == null || frequencyFactors.Length != Design.DesignPatterns.Count())
            {
                frequencyFactors = new double[Design.DesignPatterns.Count()];
                for (int i = 0; i < frequencyFactors.Length; i++)
                {
                    frequencyFactors[i] = 1D;
                }
            }
            if (changeFrequency)
            {
                frequencyFactors[patternInd] = ApplyRandomFactor(
                                               frequencyFactors[patternInd],
                                               recomputeFacs, 0.9, 1.1);
            }
            if (improvFlags.UsedImproviseColors && !colorsToolStripMenuItem.Checked)
            {
                bool isPathRibbon;
                Ribbon pathRibbon = GetImprovRibbon(pattern, out isPathRibbon);
                Pattern pattern1 = isPathRibbon ? pathRibbon : pattern;
                if (!(pattern1 is PathPattern))  //A PathPattern is not filled with color.
                {
                    if (pattern1.PixelRendering != null && pattern1.PixelRendering.Enabled)
                    {
                        HashSet<int> colorIndices = patternImprovConfig?.GetColorIndices(0);
                        ImproviseOnColors(pattern1.PixelRendering, colorIndices, recomputeFacs);
                    }
                    for (int layerIndex = 0; layerIndex < pattern1.PatternLayers.PatternLayers.Count; layerIndex++)
                    {
                        PatternLayer patternLayer = pattern1.PatternLayers.PatternLayers[layerIndex];
                        HashSet<int> colorIndices = patternImprovConfig?.GetColorIndices(layerIndex);
                        PathFillInfo pathFillInfo = patternLayer.FillInfo as PathFillInfo;
                        if (pathFillInfo == null)
                        {
                            if (!WhorlSettings.Instance.ImproviseTextures)
                                continue;
                            if (colorIndices != null && !colorIndices.Contains(0))
                                continue;
                            TextureFillInfo textureFillInfo = patternLayer.FillInfo as TextureFillInfo;
                            if (textureFillInfo == null)
                                continue;
                            string textureFileName = GetRandomTextureFileName();
                            if (textureFileName != null)
                                textureFillInfo.TextureImageFileName = textureFileName;
                        }
                        else
                        {
                            ImproviseOnColors(pathFillInfo, colorIndices, recomputeFacs);
                            //int colorCount = pathFillInfo.GetColorCount();
                            //for (int i = 0; i < colorCount; i++)
                            //{
                            //    if (colorIndices != null && !colorIndices.Contains(i))
                            //        continue;
                            //    if (i == 0)
                            //        pathFillInfo.BoundaryColor = ImproviseColor(pathFillInfo.BoundaryColor,
                            //            recomputeFacs);
                            //    else if (i == colorCount - 1)
                            //        pathFillInfo.CenterColor = ImproviseColor(pathFillInfo.CenterColor,
                            //            recomputeFacs);
                            //    else if (pathFillInfo.ColorInfo != null)
                            //    {
                            //        Color color = pathFillInfo.ColorInfo.GetColorAtIndex(i);
                            //        color = ImproviseColor(color, recomputeFacs);
                            //        pathFillInfo.ColorInfo.SetColorAtIndex(color, i);
                            //    }
                            //}
                        }
                    }
                }
            }
            double ampVal = 0;
            if (changeFrequency)
            {
                ampVal = 0.2 * 
                    pattern.BasicOutlines.Select(otl => otl.AmplitudeFactor).Average();
                if (pattern.BasicOutlines.Where(
                    otl => otl.Frequency > 8D && otl.AmplitudeFactor > ampVal).Count() > 0)
                {
                    frequencyFactors[patternInd] *= 0.9;
                }
            }
            foreach (BasicOutline outline in pattern.BasicOutlines)
            {
                if (recomputeFacs && improvFlags.UsedImproviseOnOutlineType)
                {
                    if (outline.BasicOutlineType != BasicOutlineTypes.Custom 
                        && outline.BasicOutlineType != BasicOutlineTypes.Path)
                    {
                        //Change outline type:
                        int typeInd = (int)Math.Floor(
                            0.999 * improvRandom.NextDouble() * basicOutlineVals.Length);
                        outline.BasicOutlineType = basicOutlineVals[typeInd];
                    }
                }
                if (changeFrequency && (outline.AmplitudeFactor > ampVal 
                                        || outline.Frequency <= 8))
                {
                    outline.Frequency = AdjustValue(outline.Frequency, 
                                        frequencyFactors[patternInd], 0.5, 50D);
                    if (outline.Frequency > 8D)
                    {
                        outline.AmplitudeFactor *= 64D / (outline.Frequency 
                                                        * outline.Frequency);
                    }
                }
                if (improvFlags.UsedImproviseShapes)
                {
                    double addDenom = ApplyRandomFactor(outline.AddDenom,
                                    recomputeFacs, 0.05, 10D);
                    if (addDenom > 0.5)
                        addDenom *= 0.5 + 0.5 / (addDenom + 0.5);
                    outline.AddDenom = addDenom;
                    outline.AmplitudeFactor = ApplyRandomFactor(outline.AmplitudeFactor,
                                                recomputeFacs, 0.1, 10D);
                    outline.AngleOffset = ApplyRandomFactor(outline.AngleOffset,
                                            recomputeFacs, -Math.PI, Math.PI);
                }
            }
            if (improvFlags.UsedImproviseParameters)
            {
                IEnumerable<Parameter> parameters = pattern.EnumerateParameters();
                ImproviseOnParameters(parameters, recomputeFacs);
                //foreach (PatternTransform transform in pattern.Transforms)
                //{
                //    IEnumerable<Parameter> parameters = transform.TransformSettings.Parameters;
                //    if (parameters != null)
                //        ImproviseOnParameters(parameters, recomputeFacs);
                //}
            }
            if (improvFlags.UsedImproviseShapes)
            {
                bool isPathRibbon;
                Ribbon ribbon = GetImprovRibbon(pattern, out isPathRibbon);
                if (ribbon != null)
                {
                    double modulus = ribbon.PatternZVector.GetModulus();
                    double argument = ribbon.PatternZVector.GetArgument();
                    modulus = ApplyRandomFactor(modulus, recomputeFacs,
                                                5, 100);
                    argument = ApplyRandomFactor(argument, recomputeFacs,
                                                -Math.PI, Math.PI);
                    ribbon.PatternZVector = Complex.CreateFromModulusAndArgument(
                                            modulus, argument);
                    if (isPathRibbon)
                        ribbon.ComputeSeedPoints();
                }
            }
            bool computeSeedPoints = (improvFlags.UsedImproviseOnOutlineType 
                                   || improvFlags.UsedImproviseParameters
                                   || improvFlags.UsedImprovisePetals
                                   || improvFlags.UsedImproviseShapes);
            return computeSeedPoints;
        }

        private void ImproviseOnColors(IColorNodeList colorParent, HashSet<int> colorIndices, bool recomputeFacs)
        {
            var colorNodes = colorParent.ColorNodes;
            for (int i = 0; i < colorNodes.Count; i++)
            {
                if (colorIndices != null && !colorIndices.Contains(i))
                    continue;
                var node = colorNodes.GetColorNode(i);
                node.Color = ImproviseColor(node.Color, recomputeFacs);
            }
            colorParent.ColorNodes = colorNodes;
        }

        private class ParameterIncrement
        {
            public double Increment { get; set; }
            public int Step { get; set; }
        }

        private Dictionary<Parameter, ParameterIncrement> parameterIncrements =
            new Dictionary<Parameter, ParameterIncrement>();

        //private int parameterImprovStep;

        private double? FirstNonNullDouble(params double?[] doubles)
        {
            for (int i = 0; i < doubles.Length; i++)
            {
                if (doubles[i] != null)
                    return doubles[i];
            }
            return null;
        }

        private void ImproviseOnParameters(IEnumerable<Parameter> parameters, bool recomputeFacs)
        {
            if (patternImprovConfig == null)
                return;
            foreach (Parameter parameter in parameters)
            {
                if (parameter.Value == null || parameter.Locked)
                    continue;
                ParameterImproviseConfig paramImprovConfig = 
                    patternImprovConfig.ParameterConfigs.Find(
                        pc => pc.ParameterGuid == parameter.Guid);
                if (paramImprovConfig == null || !paramImprovConfig.Enabled)
                    continue;
                double value = (double)parameter.Value;
                double minVal = (double)FirstNonNullDouble(
                       paramImprovConfig.MinValue, parameter.ImprovMinValue, parameter.MinValue, 
                       Math.Min(value, -10D));
                double maxVal = (double)FirstNonNullDouble(
                       paramImprovConfig.MaxValue, parameter.ImprovMaxValue, parameter.MaxValue, 
                       Math.Max(value, 10D));
                //ParameterIncrement paramInc;
                //if (!parameterIncrements.TryGetValue(parameter, out paramInc))
                //{
                //    paramInc = new ParameterIncrement() { Step = 10 };
                //    parameterIncrements.Add(parameter, paramInc);
                //}
                //if (paramInc.Step == 10)
                //{
                //    double targetValue = GetRandomInRange(minVal, maxVal);
                //    paramInc.Increment = (targetValue - value) / 10D;
                //    paramInc.Step = 0;
                //}
                //else
                //    paramInc.Step++;
                //value += paramInc.Increment; //= GetRandomInRange(minVal, maxVal);
                double newValue = 
                    ApplyRandomFactor(value, recomputeFacs, minVal, maxVal);
                value += paramImprovConfig.ImprovStrength * (newValue - value);
                value = Math.Round(value, paramImprovConfig.DecimalPlaces);
                //double delta = value - origValue;
                //value = origValue + Math.Sign(delta) * Math.Pow(Math.Abs(delta), 0.25);
                parameter.Value = Math.Max(Math.Min(value, maxVal), minVal);
            }
        }

        private void StopAnimating()
        {
            if (Animating)
            {
                foreach (ToolStripMenuItem itm in animationMenuItems)
                    itm.Checked = false;
                //this.spinToolStripMenuItem.Checked = this.swayToolStripMenuItem.Checked = this.revolveToolStripMenuItem.Checked
                //    = this.colorsToolStripMenuItem.Checked = this.improviseToolStripMenuItem.Checked = false;
                AnimatingChanged(false);
            }
            pauseImprovToolStripMenuItem.Checked = false;
            isAnimating = false;
        }

        private void stopAnimatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimating();
        }

        private void resetDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                StopAnimating();
                if (origDesign != null)
                {
                    if (Design != null && Design != origDesign)
                        origDesign.CopyNamedRandomSeeds(Design);
                    SetDesign(origDesign);
                    WriteTitle();
                    DisplayDesign();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private IEnumerable<Pattern.PatternInfo> GetPatternsSortedByDistance(PointF p1, bool includeRecursive = false)
        {
            return (from ptn in Design.GetAllPatternsInfo(includeRecursive)
                    orderby Tools.DistanceSquared(p1, ptn.Center) ascending, 
                    Design.IndexOfPattern(ptn.Pattern) descending select ptn).ToList();
        }

        private Pattern.PatternInfo NearestPattern(PointF p1, double maxDistance = -1D, 
                                       bool checkSelected = true, bool includeRecursive = false)
        {
            if (!Design.DesignPatterns.Any())
                return null;
            if (maxDistance == -1D)
                maxDistance = WhorlSettings.Instance.BufferSize;
            var sortedPatterns = GetPatternsSortedByDistance(p1, includeRecursive).Where(
                ptn => Tools.DistanceSquared(p1, ptn.Center) <= maxDistance * maxDistance);
            if (checkSelected)
            {
                var selPatterns = sortedPatterns.Where(ptn => ptn.Pattern.Selected);
                if (selPatterns.Any())
                    sortedPatterns = selPatterns;
            }
            return sortedPatterns.FirstOrDefault();
        }

        private IEnumerable<Pattern.PatternInfo> GetNearbyPatterns(PointF p1, double maxDistance = -1D,
                bool includeRecursive = false)
        {
            if (maxDistance == -1D)
                maxDistance = WhorlSettings.Instance.BufferSize;
            var nearbyPatterns = Design.GetAllPatternsInfo(includeRecursive).Where(
                x => Tools.DistanceSquared(x.Center, p1) < maxDistance * maxDistance)
                .OrderBy(x => Tools.DistanceSquared(x.Center, p1));
            //if (includeSingleSelected && nearbyPatterns.Count == 0)
            //{
            //    var selectedPatterns = design.DesignPatterns.Where(p => p.Selected);
            //    if (selectedPatterns.Count == 1)
            //        nearbyPatterns.Add(selectedPatterns[0]);
            //}
            return nearbyPatterns;
        }

        private Pattern SelectNearestPattern(PointF p1)
        {
            var patternInfo = NearestPattern(p1);
            if (patternInfo != null && !patternInfo.Pattern.Selected)
            {
                patternInfo.Pattern.Selected = true;
                //design.SelectedPattern = pattern;
                picDesign.Invalidate();
            }
            return patternInfo?.Pattern;
        }

        private void selectPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<Pattern.PatternInfo> nearbyPatterns = GetNearbyPatterns(dragStart, includeRecursive: true);
                if (!nearbyPatterns.Any())
                    return;
                //select is true if no nearby pattern is selected:
                bool select = !nearbyPatterns.Any(x => x.Pattern.Selected);
                Pattern.PatternInfo singlePattern = null;
                if (select && nearbyPatterns.Count() > 1)
                {
                    if (MessageBox.Show("Select a pattern group?", "Message", 
                        MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        singlePattern = nearbyPatterns.First();
                        singlePattern.Pattern.Selected = select;
                    }
                }
                if (singlePattern == null)
                {
                    foreach (var pattern in nearbyPatterns)
                    {
                        pattern.Pattern.Selected = select;
                    }
                }
                //Set SelectedPatternIndex to index of topmost pattern in pattern group:
                //design.SelectedPatternIndex = design.DrawnPatterns.IndexOf(nearbyPatterns[nearbyPatterns.Count - 1]);
                showSelectionsToolStripMenuItem.Checked = true;
                //if (Animating)
                //    AnimationDrawDesign();
                //else
                    picDesign.Invalidate();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pattern limitSelectionCenterPattern = null;

        private void limitSelectionToCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (limitSelectionToCenterToolStripMenuItem.Checked)
                {
                    var patternInfo = NearestPattern(dragStart);
                    limitSelectionCenterPattern = patternInfo?.Pattern;
                }
                else
                {
                    limitSelectionCenterPattern = null;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private HashSet<Pattern> keepSelectedPatterns = new HashSet<Pattern>();

        private void SelectNextPattern(int indexIncrement)
        {
            try
            {
                IEnumerable<Pattern> sourcePatterns;
                if (limitSelectionCenterPattern == null)
                    sourcePatterns = Design.DesignPatterns;
                else
                    sourcePatterns = Design.DesignPatterns.Where(ptn => 
                        Tools.DistanceSquared(ptn.Center, limitSelectionCenterPattern.Center) <= 2500D);
                //Get patterns sorted vertically, then horizontally, then by topmost (last) first:
                List<Pattern> sortedPatterns = (from ptn in sourcePatterns
                            where !keepSelectedPatterns.Contains(ptn)
                            orderby Math.Round(ptn.Center.Y / 100F) ascending,
                            ptn.Center.X ascending,
                            Design.IndexOfPattern(ptn) descending
                            select ptn).ToList();
                if (sortedPatterns.Count == 0)
                    return;
                Pattern selPattern = Design.SelectedPattern;
                if (selPattern == null || !selPattern.Selected)
                    selPattern = Design.FindPattern(ptn => ptn.Selected);
                int index = selPattern == null ? 0 : sortedPatterns.IndexOf(selPattern);
                if (selPattern != null)
                {
                    selPattern.Selected = false;
                    index = Tools.GetIndexInRange(index + indexIncrement, sortedPatterns.Count);
                }
                selPattern = sortedPatterns[index];
                Design.SelectedPatternIndex = Design.IndexOfPattern(selPattern);
                foreach (Pattern pattern in Design.DesignPatterns)
                    pattern.Selected = (pattern == selPattern || keepSelectedPatterns.Contains(pattern));
                picDesign.Invalidate();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void selectNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectNextPattern(1);
        }

        private void selectPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectNextPattern(-1);
        }

        private void keepSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var selPatterns = Design.DesignPatterns.Where(
                                  ptn => ptn.Selected && !keepSelectedPatterns.Contains(ptn));
                if (selPatterns.Count() != 1)
                {
                    MessageBox.Show("Exactly one new pattern must be selected to Keep Selected.");
                    return;
                }
                Pattern selPattern = selPatterns.First();
                keepSelectedPatterns.Add(selPattern);
                keepSelectedToolStripMenuItem.Checked = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void clearKeepSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                keepSelectedPatterns.Clear();
                keepSelectedToolStripMenuItem.Checked = false;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void selectKeepSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Pattern pattern in Design.DesignPatterns)
                {
                    pattern.Selected = keepSelectedPatterns.Contains(pattern);
                }
                picDesign.Invalidate();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pattern setGradientCenterPattern = null;

        private void setGradientCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ptnInfo = NearestPattern(dragStart);
                setGradientCenterPattern = ptnInfo?.Pattern;
                if (setGradientCenterPattern != null)
                {
                    WriteStatus("Click on the gradient center.");
                    picDesign.Refresh();  //Display current gradient center.
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void RenderCallback(int step, bool initial = false)
        {
            if (InvokeRequired)
            {
                Invoke((Action<int, bool>)RenderCallback, step, initial);
                return;
            }
            if (initial)
                this.toolStripProgressBar1.Maximum = step;
            else
                this.toolStripProgressBar1.Value = step;
        }

        //public void SetProgressBarMax(int max)
        //{
        //    if (InvokeRequired)
        //    {
        //        Invoke((Action<int>)SetProgressBarMax, max);
        //        return;
        //    }
        //    this.toolStripProgressBar1.Maximum = max;
        //}

        //public void SetProgressBarValue(int value)
        //{
        //    if (InvokeRequired)
        //    {
        //        Invoke((Action<int>)SetProgressBarValue, value);
        //        return;
        //    }
        //    this.toolStripProgressBar1.Value = value;
        //}

        private frmColorGradient frmColorGradient;

        private void CheckCreateColorGradientForm()
        {
            if (frmColorGradient == null || frmColorGradient.IsDisposed)
            {
                frmColorGradient = new frmColorGradient();
                frmColorGradient.Component.GradientChanged += ColorGradientChanged;
                frmColorGradient.VisibleChanged += FrmColorGradient_VisibleChanged;
                frmColorGradient.Component.frmMain = this;
            }
        }

        private void FrmColorGradient_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (!frmColorGradient.Visible)
                {
                    EndEditColorGradient();
                    this.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ColorGradientChanged(object sender, EventArgs e)
        {
            try
            {
                if (colorNodesParent != null)
                {
                    Design.ReplaceColorNodes(colorNodesParent, frmColorGradient.Component.ColorNodes);
                    Design.IsDirty = true;
                    picDesign.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void colorGradientFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                CheckCreateColorGradientForm();
                Tools.DisplayForm(frmColorGradient, this);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void EndEditColorGradient()
        {
            editedGradientPattern = null;
            colorNodesParent = null;
            editColorGradientToolStripMenuItem.Checked = false;
            RedrawPatterns();
            WriteStatus("Ended editing of the color gradient.");
        }

        private Pattern editedGradientPattern;
        private IColorNodeList colorNodesParent;

        private void editColorGradientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (editColorGradientToolStripMenuItem.Checked)
                {
                    var patternInfo = NearestPattern(dragStart, includeRecursive: true);
                    if (patternInfo == null)
                        return;
                    Pattern pattern = patternInfo.Pattern;
                    if (pattern.PixelRendering != null && pattern.PixelRendering.Enabled)
                        colorNodesParent = pattern.PixelRendering;
                    else
                    {
                        colorNodesParent = null;
                        var editedLayer = pattern.PatternLayers.GetEditedPatternLayer();
                        if (editedLayer != null)
                        {
                            colorNodesParent = editedLayer.FillInfo as PathFillInfo;
                        }
                        if (colorNodesParent == null)
                        {
                            colorNodesParent = pattern.PatternLayers.PatternLayers.Select(pl => pl.FillInfo)
                                               .Select(fi => fi as PathFillInfo).Where(p => p != null).FirstOrDefault();
                        }
                    }
                    if (colorNodesParent != null)
                    {
                        editedGradientPattern = pattern;
                        //picDesign.Refresh();
                        RedrawPatterns(overridePatterns: Design.DesignPatterns.Where(ptn => ptn != editedGradientPattern));
                        CheckCreateColorGradientForm();
                        frmColorGradient.Component.ColorNodes = colorNodesParent.ColorNodes.GetCopy();
                        Tools.DisplayForm(frmColorGradient, this);
                    }
                }
                else
                {
                    if (frmColorGradient != null)
                        frmColorGradient.Hide();
                    EndEditColorGradient();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }



        //private void deleteSelectedPatternToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Pattern selPattern = design.SelectedPattern;
        //        if (selPattern == null)
        //            MessageBox.Show("Please select a pattern to delete.");
        //        else
        //        {
        //            DeletePattern(selPattern);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void movePatternToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (moveSelectedPatternsToolStripMenuItem.Checked && !selectAllToolStripMenuItem.Checked)
        //        {
        //            if (SelectNearestPattern(new PointF(dragStart.X, dragStart.Y)) == null)
        //                moveSelectedPatternsToolStripMenuItem.Checked = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private Pattern FindTargetPattern(Predicate<Pattern> predicate)
        {
            var nearbyPatterns = GetNearbyPatterns(dragStart).Where(pi => predicate(pi.Pattern));
            if (nearbyPatterns.Count() > 1)
                nearbyPatterns = nearbyPatterns.Where(pi => pi.Pattern.Selected);
            if (nearbyPatterns.Count() == 1)
                return nearbyPatterns.First().Pattern;
            MessageBox.Show("Please select the target pattern.");
            return null;
        }

        private Pattern.PatternInfo FindNearbyPattern<T>(IEnumerable<Pattern.PatternInfo> nearbyPatterns) where T: Pattern
        {
            return nearbyPatterns.Where(ptn => ptn.Pattern is T)
                                 .OrderBy(ptn => ptn.Pattern.Selected ? 0 : 1)
                                 .FirstOrDefault();
        }

        private IEnumerable<Pattern.PatternInfo> AdjustNearbyPatterns(IEnumerable<Pattern.PatternInfo> nearbyPatterns)
        {
            if (nearbyPatterns.Count() <= 1)
                return nearbyPatterns;
            //Only select ribbon, if within range:
            var ribbonInfo = FindNearbyPattern<Ribbon>(nearbyPatterns);
            var nearbyList = new List<Pattern.PatternInfo>();
            if (ribbonInfo != null)
            {
                var selPattern = nearbyPatterns.Where(ptn => ptn.Pattern.Selected && !(ptn.Pattern is Ribbon));
                if (selPattern == null)
                {
                    nearbyList.Add(ribbonInfo);
                    return nearbyList;
                }
                else
                    nearbyPatterns = nearbyPatterns.Where(ptn => !(ptn.Pattern is Ribbon));
            }
            var pathPatternInfo = FindNearbyPattern<PathPattern>(nearbyPatterns);
            if (pathPatternInfo != null)
            {
                if (pathPatternInfo.Pattern.Selected)
                {
                    nearbyList.Add(pathPatternInfo);
                    return nearbyList;
                }
                else
                    nearbyPatterns = nearbyPatterns.Where(ptn => !(ptn.Pattern is PathPattern));
            }
            return nearbyPatterns;
        }

        private Pattern FindSingleSelectedPattern()
        {
            var selPatterns = Design.DesignPatterns.Where(ptn => ptn.Selected);
            return selPatterns.Count() == 1 ? selPatterns.First() : null;
        }

        private PatternList GetPatternOrGroup(Point p, string message, bool checkSingleSelected = false, 
                                              bool includeRecursive = false)
        {
            PatternList patternGroup = new PatternList(Design);
            PointF p1 = new PointF(p.X, p.Y);
            var nearbyPatterns = AdjustNearbyPatterns(GetNearbyPatterns(p1, includeRecursive: includeRecursive));
            if (!nearbyPatterns.Any())
            {
                if (useSelectedPatternToolStripMenuItem.Checked)
                {
                    Pattern selPattern = FindSingleSelectedPattern();
                    if (selPattern != null)
                    {
                        patternGroup.AddPattern(selPattern);
                        return patternGroup;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
            if (groupSelectedPatternsToolStripMenuItem.Checked)
            {
                var selectedPatterns = Design.GetAllPatternsInfo().Where(ptn => ptn.Pattern.Selected);
                if (selectedPatterns.Any())
                    nearbyPatterns = selectedPatterns;
            }
            else if (checkSingleSelected)
            {
                var selPatterns = nearbyPatterns.Where(ptn => ptn.Pattern.Selected);
                if (selPatterns.Count() == 1)
                {
                    nearbyPatterns = selPatterns;
                }
            }
            if (nearbyPatterns.Count() == 1 || 
                MessageBox.Show(message, "Message", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                patternGroup.AddPatterns(
                    nearbyPatterns.Select(ptn => ptn.Pattern).OrderBy(ptn => Design.IndexOfPattern(ptn)));
            }
            else 
            {
                var selPattern = NearestPattern(p1);
                if (selPattern != null)
                    patternGroup.AddPattern(selPattern.Pattern);
            }
            return patternGroup;
        }

        private async void editPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList patternGroup = GetPatternOrGroup(dragStart, "Edit a pattern group?",
                                           checkSingleSelected: true, includeRecursive: true);
                if (patternGroup == null)
                    return;
                //int editIndex = Design.EditedPattern == null ? -1 : 
                //                patternGroup.PatternsList.IndexOf(Design.EditedPattern);
                bool hasEditedPattern = Design.EditedPattern != null && 
                                        patternGroup.PatternsList.Contains(Design.EditedPattern);
                if (!EditPatternGroup(patternGroup))
                    return;  //User cancelled.
                var ptnsCopy = patternForm.EditedPatternGroup.GetCopy();
                if (patternGroup.PatternsList.Count != ptnsCopy.PatternsList.Count)
                {
                    Design.RemovePatterns(patternGroup.PatternsList);
                    Design.AddPatterns(ptnsCopy.PatternsList);
                }
                else
                {
                    Design.ReplacePatterns(patternGroup.PatternsList, ptnsCopy.PatternsList);
                    if (patternForm.EditedTransform != null)
                    {
                        var editedPattern = patternForm.EditedTransform.ParentPattern.FindByKeyGuid(ptnsCopy.Patterns);
                        if (editedPattern != null)
                        {
                            var transform = patternForm.EditedTransform.FindByKeyGuid(editedPattern.Transforms);
                            if (transform != null)
                            {
                                Design.EditedPattern = editedPattern;
                                EditedFormulaSettings = transform.TransformSettings;
                                editedKeyEnumParameters = null;
                                EditParameters();
                            }
                        }
                    }
                    if (hasEditedPattern)
                    {
                        Design.EditedPattern = Design.EditedPattern.FindByKeyGuid(ptnsCopy.Patterns);
                        if (Design.EditedPattern != null)
                        {
                            if (editedKeyEnumParameters != null)
                            {
                                editedKeyEnumParameters = editedKeyEnumParameters.FindByKeyGuid(
                                    Design.EditedPattern.GetKeyEnumParameters());
                                if (editedKeyEnumParameters != null)
                                {
                                    EditParameters();
                                    WriteStatus("Redisplayed parameters.");
                                }
                            }
                            else if (Design.EditedPattern.HasPixelRendering && pnlParameters.Visible)
                            {
                                AddRenderingControls(Design.EditedPattern);
                            }
                            else
                            {
                                if (EditedFormulaSettings != null)
                                {
                                    EditedFormulaSettings = EditedFormulaSettings.FindByKeyGuid(
                                        Design.EditedPattern.GetFormulaSettings());
                                }
                                if (EditedFormulaSettings != null && pnlParameters.Visible)
                                {
                                    EditParameters();
                                    WriteStatus("Redisplayed parameters.");
                                }
                            }
                        }
                    }
                }
                await Task.Run(() => RedrawPatterns());
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void deletePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern selPattern = Design.SelectedPattern;
                PatternList patternGroup = GetPatternOrGroup(dragStart, 
                            "Delete a pattern group?");
                if (patternGroup != null)
                {
                    Design.RemovePatterns(patternGroup.PatternsList);
                    RedrawPatterns();
                    if (selPattern != null && patternGroup.Patterns.Contains(selPattern))
                    {
                        Design.SelectedPatternIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void redrawPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
           try
            {
                if (redrawPatternToolStripMenuItem.Checked)
                {
                    InitRedrawPattern();
                }
                else
                {
                    redrawnPatternGroup = null;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool InitRedrawPattern()
        {
            origRedrawnPatternGroup =
    GetPatternOrGroup(dragStart, "Redraw a pattern group?");
            if (origRedrawnPatternGroup != null)
            {
                Design.SelectPatterns(origRedrawnPatternGroup.Patterns);
                redrawnPatternGroup = new PatternList(Design);
                redrawnPatternGroup.AddPatterns(
                    origRedrawnPatternGroup.Patterns.Select(ptn => ptn.GetCopy()));
                //if (preserveCenterPath)
                //{
                //    foreach (Pattern ptn in redrawnPatternGroup.Patterns)
                //    {
                //        ptn.PreserveCenterPath = true;
                //    }
                //}
                Ribbon ribbon = redrawnPatternGroup.GetRibbon();
                if (ribbon != null)
                {
                    ribbon.RibbonPath.Clear();
                }
            }
            else
            {
                redrawPatternToolStripMenuItem.Checked = false;
            }
            return redrawPatternToolStripMenuItem.Checked;
        }

        //private void redrawPreservingCenterPathToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        redrawPreservingCenterPathToolStripMenuItem.Checked = true;
        //        redrawPatternToolStripMenuItem.Checked = true;
        //        InitRedrawPattern(preserveCenterPath: true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private void recomputePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ptnInfo = NearestPattern(dragStart);
                Pattern pattern = ptnInfo?.Pattern;
                if (pattern != null)
                {
                    RecomputeRandom(pattern);
                    RedrawPatterns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void RecomputeRandom(Pattern pattern)
        {
            pattern.SetNewRandomSeed();
            pattern.ComputeSeedPoints(computeRandom: true);
            if (pattern.Recursion.IsRecursive)
            {
                foreach (Pattern rPtn in pattern.Recursion.RecursionPatterns)
                {
                    rPtn.SetNewRandomSeed();
                    rPtn.ComputeSeedPoints(computeRandom: true);
                }
            }
        }

        private void redrawAllPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Pattern pattern in Design.DesignPatterns)
                {
                    RecomputeRandom(pattern);
                }
                RedrawPatternsAsync(computeRandom: true);
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
                PatternList patternGroup = GetPatternOrGroup(dragStart, "Set a pattern group as default?");
                if (patternGroup != null)
                {
                    Design.DefaultPatternGroup = patternGroup.GetCopy(copyKeyGuid: false);
                    Design.DefaultPatternGroup.ClearSelected();
                    //foreach (Pattern ptn in design.DefaultPatternGroup.Patterns)
                    //{
                    //    ptn.PatternImproviseConfig = null;
                    //}
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

            //Pattern selPattern = SelectNearestPattern(new PointF(dragStart.X, dragStart.Y));
            //if (selPattern != null)
            //{
            //    //design.DefaultPatternGroup = (Pattern)selPattern.Clone();
            //}
        }

        private Pattern SetPatternFromPattern(Pattern targetPattern, Pattern sourcePattern)
        {
            Pattern newPattern;
            if (sourcePattern is Ribbon && targetPattern is PathPattern)
            {
                newPattern = targetPattern.GetCopy();
                ((PathPattern)newPattern).PathRibbon = (Ribbon)sourcePattern.GetCopy();
            }
            else
            {
                newPattern = sourcePattern.GetCopy();
                newPattern.Center = targetPattern.Center;
                newPattern.ZVector = targetPattern.ZVector;
                if (newPattern is Ribbon && targetPattern is Ribbon)
                {
                    ((Ribbon)newPattern).CopyRibbonPath((Ribbon)targetPattern);
                }
            }
            newPattern.SetKeyGuid(targetPattern);
            return newPattern;
        }

        private void setPatternFromDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList patternGroup = GetPatternOrGroup(dragStart, "Set a pattern group from default?");
                if (patternGroup != null)
                {
                    List<Pattern> targetPatterns = patternGroup.PatternsList;
                    List<Pattern> sourcePatterns = Design.DefaultPatternGroup.PatternsList;
                    if (targetPatterns.Count != sourcePatterns.Count)
                    {
                        MessageBox.Show(
                            "Pattern group does not have same number of patterns as default.", 
                            "Message");
                        return;
                    }
                    List<Pattern> newPatterns = new List<Pattern>();
                    for (int i = 0; i < targetPatterns.Count; i++)
                    {
                        Pattern targetPattern = targetPatterns[i];
                        Pattern sourcePattern = sourcePatterns[i];
                        Pattern newPattern = SetPatternFromPattern(targetPattern, sourcePattern);
                        if (targetPattern.PatternTileInfo.TilePattern)
                        {
                            newPattern.PatternTileInfo.CopyProperties(targetPattern.PatternTileInfo);
                        }
                        newPatterns.Add(newPattern);
                    }
                    Design.ReplacePatterns(targetPatterns, newPatterns);
                    RedrawPatterns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetSelectedPatternsFromDefault_Click(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<Pattern> targetPatterns = Design.DesignPatterns.Where(ptn => ptn.Selected);
                if (!targetPatterns.Any())
                {
                    MessageBox.Show("Please select some patterns.");
                    return;
                }
                Pattern sourcePattern = Design.DefaultPatternGroup.PatternsList.First();
                List<Pattern> newPatterns = new List<Pattern>();
                foreach (Pattern targetPattern in targetPatterns)
                {
                    Pattern newPattern = SetPatternFromPattern(targetPattern, sourcePattern);
                    newPatterns.Add(newPattern);
                }
                Design.ReplacePatterns(targetPatterns.ToList(), newPatterns);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void pasteDefaultPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PointF newCenter;
                if (lockedCenter != null)
                    newCenter = (PointF)lockedCenter;
                else
                    newCenter = dragStart;
                var defaultPatternList = Design.DefaultPatternGroup.GetCopy();
                Ribbon ribbon = defaultPatternList.GetRibbon();
                if (ribbon != null)
                {
                    ribbon.MoveRibbonTo(newCenter);
                }
                else
                {
                    defaultPatternList.SetCenters(newCenter);
                }
                Design.AddPatterns(defaultPatternList.Patterns);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private List<Pattern> copiedPatterns { get; set; }
        private int topLeftCopiedPatternIndex { get; set; }

        private void copySelectedPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var selPatterns = Design.DesignPatterns.Where(ptn => ptn.Selected);
                if (selPatterns.Any())
                {
                    copiedPatterns = selPatterns.ToList();
                    topLeftCopiedPatternIndex = Tools.GetTopLeftIndex(copiedPatterns.Select(ptn => ptn.Center));
                }
                else
                    MessageBox.Show("Please select some patterns to copy.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void pasteCopiedPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (copiedPatterns == null || topLeftCopiedPatternIndex >= copiedPatterns.Count)
                    return;
                PointF topLeftCenter = copiedPatterns[topLeftCopiedPatternIndex].Center;
                PointF newCenter = dragStart;
                PointF centerDiff = new PointF(newCenter.X - topLeftCenter.X, newCenter.Y - topLeftCenter.Y);
                var newPatterns = copiedPatterns.Select(ptn => ptn.GetCopy()).ToList();
                foreach (Pattern pattern in newPatterns)
                {
                    PointF patCenter = new PointF(pattern.Center.X + centerDiff.X, pattern.Center.Y + centerDiff.Y);
                    var ribbon = pattern as Ribbon;
                    if (ribbon != null)
                        ribbon.MoveRibbonTo(patCenter);
                    else
                        pattern.Center = patCenter;
                }
                Design.AddPatterns(newPatterns);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PasteFill(bool toAllSelectedPatterns)
        {
            Pattern sourcePattern = Design.DefaultPatternGroup.Patterns.FirstOrDefault();
            if (sourcePattern == null)
            {
                MessageBox.Show("Please choose a default pattern.");
                return;
            }
            if (sourcePattern is PathPattern)
            {
                Ribbon pathRibbon = ((PathPattern)sourcePattern).PathRibbon;
                if (pathRibbon != null)
                    sourcePattern = pathRibbon;
                else
                    return;
            }
            IEnumerable<Pattern> targetPatterns;
            if (toAllSelectedPatterns)
            {
                targetPatterns = Design.DesignPatterns.Where(ptn => ptn.Selected);
            }
            else
            {
                PatternList patterns = GetPatternOrGroup(dragStart, "Paste fill to a pattern group?");
                if (patterns == null)
                    return;
                targetPatterns = patterns.PatternsList;
            }
            targetPatterns = targetPatterns.Where(ptn => !(ptn is PathPattern) || ((PathPattern)ptn).PathRibbon != null);
            if (!targetPatterns.Any())
                return;
            var newPatterns = new List<Pattern>();
            foreach (Pattern pattern in targetPatterns)
            {
                Pattern ptnCopy = pattern.GetCopy();
                PathPattern pathPattern = ptnCopy as PathPattern;
                Pattern targetPattern = ptnCopy;
                if (pathPattern != null)
                {
                    targetPattern = pathPattern.PathRibbon;
                }
                targetPattern.CopyFillInfo(sourcePattern);
                //targetPattern.FillInfo = sourcePattern.FillInfo.GetCopy(targetPattern);
                targetPattern.CopyPixelRendering(sourcePattern.PixelRendering);
                newPatterns.Add(ptnCopy);
            }
            Design.ReplacePatterns(targetPatterns.ToList(), newPatterns);
            RedrawPatterns();
        }

        private void pasteFillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PasteFill(toAllSelectedPatterns: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void fillSelectedPatternsFromDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PasteFill(toAllSelectedPatterns: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void pasteBasicOutlinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern sourcePattern = Design.DefaultPatternGroup.Patterns.FirstOrDefault();
                if (sourcePattern == null)
                {
                    MessageBox.Show("Please choose a default pattern.");
                    return;
                }
                PatternList patterns = GetPatternOrGroup(dragStart, "Paste basic outlines to a pattern group?");
                if (patterns == null)
                    return;
                List<Pattern> targetPatterns = patterns.PatternsList;
                var newPatterns = new List<Pattern>();
                foreach (Pattern pattern in targetPatterns)
                {
                    Pattern ptnCopy = pattern.GetCopy();
                    ptnCopy.BasicOutlines.Clear();
                    ptnCopy.BasicOutlines.AddRange(sourcePattern.BasicOutlines.Select(otl => (BasicOutline)otl.Clone()));
                    ptnCopy.ComputeSeedPoints();
                    newPatterns.Add(ptnCopy);
                }
                Design.ReplacePatterns(targetPatterns, newPatterns);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void PasteTransformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern sourcePattern = Design.DefaultPatternGroup.Patterns.FirstOrDefault();
                if (sourcePattern == null)
                {
                    MessageBox.Show("Please choose a default pattern.");
                    return;
                }
                PatternList patterns = GetPatternOrGroup(dragStart, "Paste transforms to a pattern group?");
                if (patterns == null)
                    return;
                List<Pattern> targetPatterns = patterns.PatternsList;
                var newPatterns = new List<Pattern>();
                foreach (Pattern pattern in targetPatterns)
                {
                    Pattern ptnCopy = pattern.GetCopy();
                    ptnCopy.Transforms.AddRange(sourcePattern.Transforms.Select(trn => new PatternTransform(trn, ptnCopy)));
                    ptnCopy.ComputeSeedPoints();
                    newPatterns.Add(ptnCopy);
                }
                Design.ReplacePatterns(targetPatterns, newPatterns);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private void lockCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lockCenterToolStripMenuItem.Checked)
                {
                    var ptnInfo = NearestPattern(dragStart);
                    Pattern ptn = ptnInfo?.Pattern;
                    if (ptn != null)
                        lockedCenter = new Point((int)ptn.Center.X, (int)ptn.Center.Y);
                }
                else
                    lockedCenter = null;

            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void lockImageCenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lockImageCenterToolStripMenuItem.Checked)
                    lockedCenter = GetPictureBoxCenter();
                else
                    lockedCenter = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private BackgroundForm backgroundForm = null;

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (backgroundForm == null || backgroundForm.IsDisposed)
                    backgroundForm = new BackgroundForm();
                backgroundForm.Initialize(Design);
                if (backgroundForm.ShowDialog() == DialogResult.OK)
                {
                    RedrawPatterns();
                }
                this.BringToFront();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public static void AddPatternGroupToChoices(PatternList patternGroup, PatternGroupList patternGroupList)
        {
            PatternList copy = patternGroup.GetCopy(copyKeyGuid: false);
            copy.ClearSelected();
            foreach (Pattern ptn in copy.Patterns)
            {
                ptn.PatternImproviseConfig = null;
            }
            patternGroupList.AddPatternGroup(copy);
        }

        public static void AddPatternGroupToChoices(PatternList patternGroup)
        {
            AddPatternGroupToChoices(patternGroup, PatternChoices);
        }

        private void addPatternToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList patternGroup = GetPatternOrGroup(dragStart, "Add a pattern group to choices?");
                if (patternGroup != null)
                {
                    AddPatternGroupToChoices(patternGroup);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void addPatternToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList patternGroup = GetPatternOrGroup(dragStart, "Add a pattern group to clipboard?");
                if (patternGroup != null)
                {
                    AddPatternGroupToChoices(patternGroup, ClipboardPatterns);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool ChooseDefaultPattern(SelectPatternForm selectPtnForm)
        {
            var cursor = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                selectPtnForm.Initialize();
            }
            finally
            {
                Cursor = cursor;
            }
            bool ok = selectPtnForm.ShowDialog() == DialogResult.OK;
            if (ok)
            {
                this.Design.DefaultPatternGroup =
                    selectPtnForm.SelectedPatternGroup.GetCopy(copyKeyGuid: false);
            }
            return ok;
        }

        private void choosePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ChooseDefaultPattern(selectPatternForm);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void chooseClipboardPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ChooseDefaultPattern(ClipboardPatternsForm);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void pauseImprovToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pauseImprovToolStripMenuItem.Checked)
                {
                    AnimationTimer.Stop();
                    RedrawPatterns();
                }
                else if (Animating || replayImprovisationsToolStripMenuItem.Checked)
                {
                    AnimationDrawDesign();
                    AnimationTimer.Start();
                    Animate(improvDesignsIndex);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void NextOrPreviousImprov(int indexIncrement)
        {
            try
            {
                int newInd = improvDesignsIndex + indexIncrement;
                if (newInd >= 0 && newInd < improvDesigns.Count)
                {
                    AnimationTimer.Stop();
                    pauseImprovToolStripMenuItem.Checked = true;
                    improvDesignsIndex = newInd;
                    ReplayImprovisation(improvDesignsIndex, replay: true);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void nextImprovToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NextOrPreviousImprov(1);
        }

        private void previousImprovToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NextOrPreviousImprov(-1);
        }

        private void newDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
           try
            {
                if (!CheckSaveDesign())
                    return;  //User cancelled.
                Design.ClearDesign();
                designLayersForm.SetDesign(Design);
                InitializeForDesign();
                StopAnimating();
                SetDesignStartupItems();
                RedrawPatterns();
                designFileName = null;
                Design.IsDirty = false;
                ClearTileGrid();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void WriteStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke((Action<string>)WriteStatus, message);
            }
            //string prevMessage = this.toolStripStatusLabel1.Text;
            this.toolStripStatusLabel1.Text = message;
            //return prevMessage;
        }

        private void WriteTitle(string fileName = null)
        {
            if (fileName == null)
                fileName = this.designFileName;
            this.Text = (Design != null && Design.IsDirty ? "*" : "") + 
                        (string.IsNullOrEmpty(fileName) ? "New Design" :
                         Path.GetFileNameWithoutExtension(fileName)) + " - Whorl";
        }

        private void showGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showGridToolStripMenuItem.Checked)
                StopAnimating();
            OnchangeGridSettings();
        }

        private void gridTypeSquareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridTypeCircularToolStripMenuItem.Checked = !gridTypeSquareToolStripMenuItem.Checked;
            OnchangeGridSettings();
            if (gridTypeSquareToolStripMenuItem.Checked)
                GridType = GridTypes.Square;
        }

        private void gridTypeCircularToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridTypeSquareToolStripMenuItem.Checked = !gridTypeCircularToolStripMenuItem.Checked;
            OnchangeGridSettings();
            if (gridTypeCircularToolStripMenuItem.Checked)
                GridType = GridTypes.Circular;
        }

        private void RepeatDefaultPatternAtVertices(PathPattern pathPattern, bool trackPathAngle)
        {
            if (pathPattern?.CurveVertexIndices == null)
                return;
            PatternList ptnList = Design.DefaultPatternGroup;
            List<PatternList> ptnCopies = new List<PatternList>();
            PointF pathCenter = pathPattern.Center;
            Complex zVec = Complex.Zero;
            var vertices = pathPattern.CurveVertexIndices.Select(i => pathPattern.CurvePoints[i]).ToArray();
            int startI = trackPathAngle ? Tools.GetTopLeftIndex(vertices) : 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                PointF center = vertices[(i + startI) % vertices.Length];
                PatternList ptnCopy = ptnList.GetCopy();
                ptnCopy.SetCenters(center);
                if (trackPathAngle)
                {
                    if (i == 0)
                    {
                        zVec = new Complex(center.X - pathCenter.X, center.Y - pathCenter.Y);
                        zVec.Normalize();
                    }
                    else
                    {
                        Complex zVec2 = new Complex(center.X - pathCenter.X, center.Y - pathCenter.Y);
                        zVec2.Normalize();
                        Complex zRotation = zVec2 / zVec;
                        foreach (Pattern ptn in ptnCopy.Patterns)
                        {
                            ptn.ZVector *= zRotation;
                        }
                    }
                }
                ptnCopies.Add(ptnCopy);
            }
            if (ptnCopies.Any())
            {
                Design.AddPatterns(ptnCopies.SelectMany(pl => pl.Patterns));
                RedrawPatterns();
            }
        }

        RepeatSettingsForm repeatSettingsForm = null;

        private void repeatPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
           try
            {
                PatternList patternGroup = GetPatternOrGroup(dragStart, 
                    "Repeat a pattern group?");
                if (patternGroup == null)
                    return;
                Pattern selPattern = patternGroup.Patterns.FirstOrDefault();
                if (selPattern == null)
                    return;
                Ribbon ribbon = patternGroup.GetRibbon();
                PointF center;
                if (ribbon != null && ribbon.RibbonPath.Count > 0)
                    center = ribbon.RibbonPath[0];
                else
                    center = selPattern.Center;
                if (repeatSettingsForm == null || repeatSettingsForm.IsDisposed)
                    repeatSettingsForm = new RepeatSettingsForm();
                repeatSettingsForm.Initialize(patternGroup);
                if (repeatSettingsForm.ShowDialog() == DialogResult.OK)
                {
                    if (repeatSettingsForm.RepeatAtVertices)
                    {
                        RepeatDefaultPatternAtVertices(patternGroup.Patterns.FirstOrDefault() as PathPattern,
                                                       repeatSettingsForm.TrackPathAngle);
                        return;
                    }
                    int steps;
                    Rectangle bounds = PictureBoxRectangle;
                    List<Pattern> patternList = new List<Pattern>();
                    bool entireRibbon = ribbon != null && repeatSettingsForm.EntireRibbon && repeatSettingsForm.GridInterval == 0;
                    switch (repeatSettingsForm.RepeatMode)
                    {
                        case RepeatSettingsForm.RepeatModes.Circular:
                            Point picCenter;
                            double radialAngle = GetGridRadialAngleAndInfo(out picCenter, out steps);
                            if (repeatSettingsForm.GridInterval == 0)
                            {
                                steps = 1 + (int)repeatSettingsForm.Repetitions;
                                if (repeatSettingsForm.UseSelectedPatternCenter)
                                {
                                    if (Design.DesignPatterns.Count(ptn => ptn.Selected) != 1)
                                    {
                                        MessageBox.Show("Please select one pattern for the repeat center.");
                                        return;
                                    }
                                    Pattern centerPattern = Design.FindPattern(ptn => ptn.Selected);
                                    picCenter = new Point((int)centerPattern.Center.X, (int)centerPattern.Center.Y);
                                }
                            }
                            else
                            {
                                radialAngle *= repeatSettingsForm.GridInterval;
                                steps = (int)Math.Round(2 * Math.PI / radialAngle);
                            }
                            radialAngle = 2 * Math.PI / (double)steps;
                            if (repeatSettingsForm.ReverseDirection)
                                radialAngle = -radialAngle;
                            if (!repeatSettingsForm.FillGrid && repeatSettingsForm.Repetitions != null)
                                steps = Math.Min(steps, 1 + (int)repeatSettingsForm.Repetitions);
                            if (ribbon != null && !entireRibbon)
                            {
                                ribbon.RibbonPath.Clear();  //Redraw first ribbon.
                                steps++;
                            }
                            Complex zRotation = Complex.CreateFromModulusAndArgument(1, radialAngle);
                            Complex zRad = new Complex(center.X - picCenter.X, center.Y - picCenter.Y);
                            Complex zRibbonRotation = zRotation;
                            Complex patternZVector = selPattern.ZVector;
                            for (int i = 0; i < steps; i++)
                            {
                                PointF newCenter = new PointF(picCenter.X + (float)zRad.Re, picCenter.Y + (float)zRad.Im);
                                if (entireRibbon && i > 0)
                                {
                                    Ribbon newRibbon = (Ribbon)ribbon.GetCopy();
                                    for (int j = 0; j < newRibbon.RibbonPath.Count; j++)
                                    {
                                        PointF p = newRibbon.RibbonPath[j];
                                        Complex zRibbonVec =  zRibbonRotation * new Complex(p.X - picCenter.X, p.Y - picCenter.Y);
                                        newRibbon.RibbonPath[j] = new PointF(picCenter.X + (float)(zRibbonVec.Re), picCenter.Y + (float)zRibbonVec.Im);
                                    }
                                    zRibbonRotation *= zRotation;
                                    patternList.Add(newRibbon);
                                }
                                else if (ribbon != null)
                                {
                                    if (i == 0)
                                        ribbon.Center = newCenter;
                                    else
                                        ribbon.AddToRibbonPath(newCenter);
                                }
                                else if (i > 0)    //First pattern already drawn.
                                {
                                    PatternList newPatternGroup = 
                                        patternGroup.GetCopy(copyKeyGuid: false);
                                    patternZVector = patternZVector * zRotation;
                                    newPatternGroup.SetZVectors(patternZVector);
                                    newPatternGroup.SetCenters(newCenter);
                                    foreach (Pattern newPattern in newPatternGroup.Patterns)
                                    {
                                        patternList.Add(newPattern);
                                    }
                                }
                                zRad = zRad * zRotation;
                            }
                            break;
                        case RepeatSettingsForm.RepeatModes.Radial:
                            picCenter = new Point(bounds.Width / 2, bounds.Height / 2);
                            Complex zVec = new Complex(center.X - picCenter.X, center.Y - picCenter.Y);
                            double radius = zVec.GetModulus();
                            Complex zUnit = zVec / radius;
                            double radiusInc = repeatSettingsForm.GridInterval * GridSize;
                            if (repeatSettingsForm.GridInterval == 0)
                            {
                                steps = 1 + (int)repeatSettingsForm.Repetitions;
                                radiusInc = radius / (double)steps;
                            }
                            else
                            {
                                steps = (int)Math.Floor(radius / radiusInc);
                                if (steps != 0)
                                    radiusInc = radius / (double)steps;
                            }
                            if (repeatSettingsForm.ReverseDirection)
                                radiusInc = -radiusInc;
                            if (!repeatSettingsForm.FillGrid && repeatSettingsForm.Repetitions != null)
                                steps = (int)repeatSettingsForm.Repetitions;
                            if (ribbon != null)
                            {
                                ribbon.RibbonPath.Clear();  //Redraw first ribbon.
                            }
                            if (radiusInc < 0)
                                zVec = new Complex(0, 0);
                            steps++;
                            for (int i = 0; i < steps; i++)
                            {
                                PointF newCenter = new PointF(picCenter.X + (float)zVec.Re, picCenter.Y + (float)zVec.Im);
                                if (ribbon != null)
                                {
                                    if (i == 0)
                                        ribbon.Center = newCenter;
                                    else
                                        ribbon.AddToRibbonPath(newCenter);
                                }
                                else if (i > 0)    //First pattern already drawn.
                                {
                                    PatternList newPatternGroup =
                                        patternGroup.GetCopy(copyKeyGuid: false);
                                    newPatternGroup.SetCenters(new PointF(picCenter.X + (float)zVec.Re, picCenter.Y + (float)zVec.Im));
                                    foreach (Pattern newPattern in newPatternGroup.Patterns)
                                    {
                                        patternList.Add(newPattern);
                                    }
                                    //Pattern newPattern = (Pattern)selPattern.Clone();
                                    //newPattern.Center = new PointF(picCenter.X + (float)zVec.Re, picCenter.Y + (float)zVec.Im);
                                    //patternList.Add(newPattern);
                                }
                                radius -= radiusInc;
                                zVec = zUnit * radius;
                            }
                            break;
                        case RepeatSettingsForm.RepeatModes.Horizontal:
                            float xInc = repeatSettingsForm.GridInterval * GridSize;
                            float range = Math.Abs((float)bounds.Width - 2 * center.X);
                            if (repeatSettingsForm.GridInterval == 0 || !repeatSettingsForm.FillGrid)
                            {
                                steps = (int)repeatSettingsForm.Repetitions;
                                if (repeatSettingsForm.FillGrid)
                                    xInc = range / (float)steps;
                            }
                            else
                            {
                                //if (range < 0)
                                //    xInc = -xInc;
                                steps = (int)Math.Round(Math.Abs(range / xInc));
                            }
                            steps++;
                            if (repeatSettingsForm.ReverseDirection)
                                xInc = -xInc;
                            if (ribbon != null && !entireRibbon)
                            {
                                ribbon.RibbonPath.Clear();  //Redraw first ribbon.
                            }
                            float x = center.X;
                            float y = center.Y;
                            for (int i = 0; i < steps; i++)
                            {
                                PointF newCenter = new PointF(x, y);
                                if (ribbon != null)
                                {
                                    if (entireRibbon)
                                    {
                                        if (i > 0)
                                        {
                                            var newRibbon = (Ribbon)ribbon.GetCopy();
                                            for (int j = 0; j < newRibbon.RibbonPath.Count; j++)
                                            {
                                                PointF p = newRibbon.RibbonPath[j];
                                                newRibbon.RibbonPath[j] = new PointF(p.X + i * xInc, p.Y);
                                            }
                                            patternList.Add(newRibbon);
                                        }
                                    }
                                    else
                                    {
                                        if (i == 0)
                                            ribbon.Center = newCenter;
                                        else
                                            ribbon.AddToRibbonPath(newCenter);
                                    }
                                }
                                else if (i > 0)    //First pattern already drawn.
                                {
                                    PatternList newPatternGroup =
                                        patternGroup.GetCopy(copyKeyGuid: false);
                                    newPatternGroup.SetCenters(new PointF(x, y));
                                    foreach (Pattern newPattern in newPatternGroup.Patterns)
                                    {
                                        patternList.Add(newPattern);
                                    }
                                }
                                x += xInc;
                                //Pattern newPattern = (Pattern)selPattern.Clone();
                                //x += xInc;
                                //newPattern.Center = new PointF(x, y);
                                //patternList.Add(newPattern);
                            }
                            break;
                        case RepeatSettingsForm.RepeatModes.Vertical:
                            float yInc = repeatSettingsForm.GridInterval * GridSize;
                            range = Math.Abs((float)bounds.Height - 2 * center.Y);
                            if (repeatSettingsForm.GridInterval == 0 || !repeatSettingsForm.FillGrid)
                            {
                                steps = (int)repeatSettingsForm.Repetitions;
                                if (repeatSettingsForm.FillGrid)
                                    yInc = range / (float)steps;
                            }
                            else
                            {
                                //if (range < 0)
                                //    yInc = -yInc;
                                steps = (int)Math.Round(Math.Abs(range / yInc));
                            }
                            steps++;
                            if (repeatSettingsForm.ReverseDirection)
                                yInc = -yInc;
                            if (ribbon != null && !entireRibbon)
                            {
                                ribbon.RibbonPath.Clear();  //Redraw first ribbon.
                            }
                            x = center.X;
                            y = center.Y;
                            for (int i = 0; i < steps; i++)
                            {
                                PointF newCenter = new PointF(x, y);
                                if (ribbon != null)
                                {
                                    if (entireRibbon)
                                    {
                                        if (i > 0)
                                        {
                                            var newRibbon = (Ribbon)ribbon.GetCopy();
                                            for (int j = 0; j < newRibbon.RibbonPath.Count; j++)
                                            {
                                                PointF p = newRibbon.RibbonPath[j];
                                                newRibbon.RibbonPath[j] = new PointF(p.X, p.Y + i * yInc);
                                            }
                                            patternList.Add(newRibbon);
                                        }
                                    }
                                    else
                                    {
                                        if (i == 0)
                                            ribbon.Center = newCenter;
                                        else
                                            ribbon.AddToRibbonPath(newCenter);
                                    }
                                }
                                else if (i > 0)    //First pattern already drawn.
                                {
                                    PatternList newPatternGroup =
                                        patternGroup.GetCopy(copyKeyGuid: false);
                                    newPatternGroup.SetCenters(new PointF(x, y));
                                    foreach (Pattern newPattern in newPatternGroup.Patterns)
                                    {
                                        patternList.Add(newPattern);
                                    }
                                }
                                y += yInc;
                                //Pattern newPattern = (Pattern)selPattern.Clone();
                                //y += yInc;
                                //newPattern.Center = new PointF(x, y);
                                //patternList.Add(newPattern);
                            }
                            break;
                    }
                    Design.AddPatterns(patternList);
                    RedrawPatterns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
           try
            {
                foreach (Pattern pattern in this.Design.DesignPatterns)
                {
                    pattern.Selected = selectAllToolStripMenuItem.Checked;
                }
                picDesign.Invalidate();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void WriteTrace(string message)
        {
            if (WhorlSettings.Instance.TraceMode)
                toolStripStatusLabel1.Text = message + " at " + DateTime.Now.ToString();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Design.Undo())
                    RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Design.Redo())
                    RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        SettingsForm settingsForm = null;

        private bool EditSettings()
        {
            string patternChoicesFilePath = PatternChoicesFilePath;
            if (settingsForm == null || settingsForm.IsDisposed)
                settingsForm = new SettingsForm();
            int prevDraftSize = WhorlSettings.Instance.DraftSize;
            settingsForm.Initialize();
            bool retVal = settingsForm.ShowDialog() == DialogResult.OK;
            if (retVal)
            {
                string newPatternChoicesFilePath = PatternChoicesFilePath;
                if (newPatternChoicesFilePath != patternChoicesFilePath)
                {
                    if (File.Exists(patternChoicesFilePath))
                    {
                        PatternChoices = new PatternGroupList(Design);
                        Tools.ReadFromXml(patternChoicesFilePath, PatternChoices, 
                                          "PatternChoices");
                        InitPatternChoiceSettings();
                    }
                }
                if (UseDraftMode && WhorlSettings.Instance.DraftSize != prevDraftSize)
                    RedrawPatterns();
            }
            return retVal;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
           try
            {
                EditSettings();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void graphWaveformToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    Pattern pattern = NearestPattern(dragStart);
        //    if (pattern != null)
        //    {
        //        WaveGraphForm frm = new WaveGraphForm();
        //        frm.Initialize(pattern);
        //        frm.ShowDialog();
        //    }
        //}

        private void showSelectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (showSelectionsToolStripMenuItem.Checked)
            //    showInfluencePointsToolStripMenuItem.Checked = false;
            picDesign.Invalidate();
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Pattern pattern in Design.DesignPatterns)
                pattern.Selected = false;
            Design.SelectedPatternIndex = -1;
            picDesign.Invalidate();
        }

        private bool FixPattern(Pattern pattern)
        {
            bool fixedPattern = true;
            pattern.ComputeSeedPoints();
            PathPattern pathPattern = pattern as PathPattern;
            if (pattern.SeedPointsNormalizationFactor != 0)
                pattern.ZVector /= pattern.SeedPointsNormalizationFactor;
            else
            {
                if (pathPattern?.CartesianPathOutline == null)
                    throw new Exception("Pattern had normalization factor = 0.");
            }
            Ribbon ribbon = pattern as Ribbon;
            if (pathPattern != null)
            {
                ribbon = pathPattern.PathRibbon;
                if (ribbon != null)
                    ribbon.ComputeSeedPoints();
            }
            if (ribbon != null)
                ribbon.PatternZVector /= ribbon.SeedPointsNormalizationFactor;

            //double unitFactor = pattern.UnitFactor;
            //foreach (BasicOutline otl in pattern.BasicOutlines)
            //{
            //    if (otl.customOutline == null || otl.UnitFactor == 0)
            //        continue;
            //    otl.AmplitudeFactor /= otl.UnitFactor;
            //    fixedPattern = true;
            //}
            //if (fixedPattern)
            //{
            //    double fixedUnitFactor = pattern.UnitFactor;
            //    pattern.ZVector *= unitFactor / fixedUnitFactor;
            //}
            return fixedPattern;
        }

        //private void fixDesignsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (!FilesFolderExists())
        //            return;
        //        string sourceFolder = WhorlSettings.Instance.FilesFolder;
        //        string outputFolder = Path.Combine(sourceFolder, "DesignsToFix");
        //        if (!Directory.Exists(outputFolder))
        //            Directory.CreateDirectory(outputFolder);
        //        List<string> fixedFileNames = new List<string>();
        //        List<string> unfixedFileNames = new List<string>();
        //        foreach (string designFileName in Directory.GetFiles(sourceFolder, "*.xml"))
        //        {
        //            WhorlDesign designToFix = new WhorlDesign();
        //            try
        //            {
        //                designToFix.ReadDesignFromXmlFile(designFileName);
        //            }
        //            catch (Exception ex)
        //            {
        //                if (MessageBox.Show("Invalid design file: " + designFileName + 
        //                                    "\n" + ex.Message,
        //                        "Message", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        //                {
        //                    break;
        //                }
        //                else
        //                    continue;
        //            }
        //            string designName = Path.GetFileNameWithoutExtension(designFileName);
        //            if (designToFix.XmlVersion >= 1F)
        //            {
        //                unfixedFileNames.Add(designName);
        //                continue;
        //            }
        //            bool fixedDesign = false;
        //            if (designToFix.XmlVersion < 1F)
        //            {
        //                foreach (Pattern pattern in designToFix.DesignPatterns)
        //                {
        //                    if (FixPattern(pattern))
        //                        fixedDesign = true;
        //                }
        //            }
        //            if (fixedDesign)
        //            {
        //                string outFileName = Path.Combine(outputFolder,
        //                                     Path.GetFileName(designFileName));
        //                designToFix.XmlPictureBoxSize = designToFix.PictureBoxSize;
        //                XmlTools.WriteToXml(outFileName, designToFix);
        //                fixedFileNames.Add(designName);
        //            }
        //            else
        //            {
        //                unfixedFileNames.Add(designName);
        //            }

        //            //if (designToFix.PreviousPictureSize != picDesign.ClientRectangle.Size)
        //            //{
        //            //    string outFileName = Path.Combine(outputFolder, Path.GetFileName(designFileName));
        //            //    Tools.WriteToXml(outFileName, designToFix);
        //            //}
        //        }
        //        MessageBox.Show($"Fixed {fixedFileNames.Count} design files."
        //            + Environment.NewLine + "Unfixed files: "
        //            + Environment.NewLine + string.Join(", ", unfixedFileNames)); 
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        string[] slideShowFileNames;
        int slideShowFileIndex;

        private void ShowSlide()
        {
            try
            {
                if (slideShowFileIndex >= slideShowFileNames.Length)
                    slideShowFileIndex = 0;
                string fileName = slideShowFileNames[slideShowFileIndex];
                WhorlDesign designToShow = new WhorlDesign(OnDistancePatternsCountChanged);
                try
                {
                    designToShow.ReadDesignFromXmlFile(fileName);
                }
                catch
                {
                    designToShow = null;
                }
                if (designToShow != null)
                {
                    SetDesign(designToShow);
                    WriteTitle(fileName);
                    DisplayDesign();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                slideShowFileIndex++;
            }
        }

        private void slideShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!slideShowToolStripMenuItem.Checked)
                {
                    AnimationTimer.Stop();
                    this.resetDesignToolStripMenuItem.Enabled = true;
                    slideShowFileNames = null;
                    return;
                }
                if (slideShowFileNames == null)
                {
                    string sourceFolder = WhorlSettings.Instance.FilesFolder;
                    if (Directory.Exists(sourceFolder))
                        slideShowFileNames = Directory.GetFiles(sourceFolder, "*.xml");
                    if (slideShowFileNames == null || slideShowFileNames.Length == 0)
                    {
                        slideShowToolStripMenuItem.Checked = false;
                        return;
                    }
                    if (!string.IsNullOrEmpty(this.designFileName))
                    {
                        slideShowFileIndex = 
                            1 + slideShowFileNames.ToList().IndexOf(designFileName);
                    }
                    else
                        slideShowFileIndex = 0;
                }
                this.origDesign = Design;
                AnimationTimer.Interval = 1500;
                AnimationTimer.Start();
                ShowSlide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private PointF TransformPoint(PointF p, PointF pScale, PointF prevCenter, PointF newCenter)
        {
            PointF pDiff = new PointF(p.X - prevCenter.X, p.Y - prevCenter.Y);
            return new PointF(newCenter.X + pScale.X * pDiff.X, newCenter.Y + pScale.Y * pDiff.Y);
        }

        private ImageSizeForm imageSizeForm = null;

        private void imageSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imageSizeForm == null || imageSizeForm.IsDisposed)
                    imageSizeForm = new ImageSizeForm();
                imageSizeForm.Initialize(picDesign.Size);
                if (imageSizeForm.ShowDialog() == DialogResult.OK)
                {
                    Size prevSize = picDesign.Size;
                    if (imageSizeForm.DockImage)
                    {
                        DockPictureBox();
                    }
                    else
                    {
                        SetPictureBoxSize(imageSizeForm.ImageSize);
                    }
                    if (imageSizeForm.ScaleDesign)
                    {
                        Design.ScaleDesignToFit(picDesign.Size);
                    }
                    else if (imageSizeForm.ResizeDesign)
                    {
                        PointF newCenter = new PointF(picDesign.Size.Width / 2, picDesign.Size.Height / 2);
                        PointF prevCenter = new PointF(prevSize.Width / 2, prevSize.Height / 2);
                        PointF deltaCenter = new PointF(newCenter.X - prevCenter.X, newCenter.Y - prevCenter.Y);
                        //PointF pScale = new PointF(1, 1);
                        foreach (Pattern ptn in Design.DesignPatterns)
                        {
                            ptn.Center = Tools.AddPoint(ptn.Center, deltaCenter);
                            Ribbon ribbon = ptn as Ribbon;
                            if (ribbon != null)
                            {
                                for (int i = 0; i < ribbon.RibbonPath.Count; i++)
                                {
                                    ribbon.RibbonPath[i] = Tools.AddPoint(ribbon.RibbonPath[i], deltaCenter);
                                }
                            }
                            //if (ptn.IsBackgroundPattern)
                            //{
                            //    AdjustBackgroundPattern(picDesign.Size, ptn);
                            //}
                        }
                    }
                    Design.PictureBoxSize = picDesign.Size;
                    Design.IsDirty = true;
                    ClearBackgroundGraphicsPath();
                    RedrawPatterns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void sizeDesignToBestFitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Design.ScaleDesignToFit(ContainerSize))
                {
                    RedrawPatterns();
                    WriteStatus("Resized design.");
                }
                else
                    WriteStatus("No need to resize.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setImageRectangleAsDefaultPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    double aspectRatio = (double)picDesign.Width / picDesign.Height;
            //    var rectOutline = new BasicOutline(BasicOutlineTypes.Rectangle);
            //    rectOutline.AmplitudeFactor = 1;
            //    rectOutline.Frequency = 1;
            //    rectOutline.AddDenom = aspectRatio;
            //    var rectPattern = new Pattern(FillInfo.FillTypes.Path);
            //    rectPattern.RotationSteps = 5000;
            //    rectPattern.BasicOutlines.Add(rectOutline);
            //    rectPattern.ComputeSeedPoints();
            //    rectPattern.BoundaryColor = Color.Red;
            //    rectPattern.CenterColor = Color.Yellow;
            //    var rectPtnGroup = new PatternList();
            //    rectPtnGroup.AddPattern(rectPattern);
            //    design.DefaultPatternGroup = rectPtnGroup;
            //}
            //catch (Exception ex)
            //{
            //    Tools.HandleException(ex);
            //}
        }

        private void SetAddDenomForBackgroundPattern(Size imgSize, BasicOutline basicOutline)
        {
            double aspectRatio = (double)imgSize.Width / imgSize.Height;
            if (basicOutline.BasicOutlineType == BasicOutlineTypes.Rectangle)
                basicOutline.AddDenom = aspectRatio;
            else if (basicOutline.BasicOutlineType == BasicOutlineTypes.NewEllipse)
            {
                if (aspectRatio < 1D)
                    aspectRatio = 1D / aspectRatio;
                basicOutline.AddDenom = 1D / Math.Max(aspectRatio - 1D, 0.0001D);
            }
        }

        private void setImageRectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var rectOutline = new BasicOutline(BasicOutlineTypes.Rectangle);
                rectOutline.AmplitudeFactor = 1;
                rectOutline.Frequency = 1;
                SetAddDenomForBackgroundPattern(picDesign.ClientSize, rectOutline);
                var rectPattern = new Pattern(Design, FillInfo.FillTypes.Path);
                rectPattern.RotationSteps = 5000;
                rectPattern.BasicOutlines.Add(rectOutline);
                rectPattern.ComputeSeedPoints();
                rectPattern.BoundaryColor = Color.Red;
                rectPattern.CenterColor = Color.Yellow;
                var rectPtnGroup = new PatternList(Design);
                rectPtnGroup.AddPattern(rectPattern);
                Design.DefaultPatternGroup = rectPtnGroup;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setImageEllipseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ellipseOutline = new BasicOutline(BasicOutlineTypes.NewEllipse);
                ellipseOutline.AmplitudeFactor = 1;
                ellipseOutline.Frequency = 1;
                SetAddDenomForBackgroundPattern(picDesign.ClientSize, ellipseOutline);
                var ellipsePattern = new Pattern(Design, FillInfo.FillTypes.Path);
                ellipsePattern.RotationSteps = 5000;
                ellipsePattern.BasicOutlines.Add(ellipseOutline);
                ellipsePattern.ComputeSeedPoints();
                ellipsePattern.BoundaryColor = Color.Red;
                ellipsePattern.CenterColor = Color.Yellow;
                var ellipsePtnGroup = new PatternList(Design);
                ellipsePtnGroup.AddPattern(ellipsePattern);
                Design.DefaultPatternGroup = ellipsePtnGroup;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void sizeImageToBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Design.BackgroundImageFileName))
                {
                    MessageBox.Show("The design does not have a background image.");
                    return;
                }
                Bitmap bmp = (Bitmap)Bitmap.FromFile(Design.BackgroundImageFileName);
                int newWidth = bmp.Width * (picDesign.ClientSize.Width / bmp.Width);
                int newHeight = bmp.Height * (picDesign.ClientSize.Height / bmp.Height);
                if (newWidth == 0 || newHeight == 0)
                    return;
                //picDesign.Dock = DockStyle.None;
                picDesign.ClientSize = new Size(newWidth, newHeight);
                //picDesign.Width = newWidth;
                //picDesign.Height = newHeight;
                ClearBackgroundGraphicsPath();
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ClearBackgroundGraphicsPath()
        {
            if (backgroundGrPath != null)
            {
                backgroundGrPath.Dispose();
                backgroundGrPath = null;
            }
        }

        //private List<Pattern> GetPatternList(WhorlDesign dgn, PointF p1, 
        //                                     double maxDistance = 20D)
        //{
        //    List<Pattern> patterns = dgn.DesignPatterns.Where(
        //        x => Tools.DistanceSquared(x.Center, p1) < maxDistance * maxDistance);
        //    return AdjustNearbyPatterns(patterns);
        //}

        private void fixPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int changedCount = 0;
                for (int ind = 0; ind < PatternChoices.PatternGroups.Count; ind++)
                {
                    var patternList = PatternChoices.PatternGroups[ind];
                    bool isChanged = false;
                    foreach (Pattern pattern in patternList.Patterns)
                    {
                        if (pattern.RotationSteps == 500)
                        {
                            pattern.RotationSteps = 5000;
                            isChanged = true;
                            changedCount++;
                        }
                    }
                    if (isChanged)
                    {
                        PatternChoices.SetPatternGroup(patternList, ind);
                    }
                }
                MessageBox.Show($"Set Precision to 5000 for {changedCount} patterns.");
                //int count = patternChoices.PatternGroups.SelectMany(pg => pg.Patterns)
                //                                        .SelectMany(ptn => ptn.BasicOutlines)
                //                                        .Where(bo => bo.BasicOutlineType == BasicOutlineTypes.Pointed4)
                //                                        .Count();
                //MessageBox.Show($"Found {count} Pointed4 BasicOutlines.");
                //for (int i = 0; i < patternChoices.PatternGroups.Count; i++)
                //{
                //    var ptnList = patternChoices.PatternGroups[i];
                //    if (ptnList.Patterns.SelectMany(ptn => ptn.BasicOutlines).Where(bo => bo.BasicOutlineType == BasicOutlineTypes.Pointed4).Any())
                //    {
                //        MessageBox.Show($"Pattern at index {i} has Pointed4 BasicOutline.");
                //    }
                //    //foreach (Pattern pattern in ptnList.Patterns)
                //    //{
                //    //    foreach (BasicOutline otl in pattern.BasicOutlines)
                //    //    {
                //    //        if (otl.customOutline == null || otl.UnitFactor == 0)
                //    //            continue;
                //    //        otl.AmplitudeFactor /= otl.UnitFactor;
                //    //    }
                //    //}
                //}
                //MessageBox.Show("Finished fixing Pattern Choices.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void scanForPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    List<string> messages = new List<string>();
            //    int patternsPerPage = selectPatternForm.BoxesAcross *
            //                          selectPatternForm.BoxesDown;
            //    foreach (PatternList ptnList in patternChoices.PatternGroups)
            //    {
            //        foreach (Pattern pattern in ptnList.Patterns)
            //        {
            //            foreach (BasicOutline otl in pattern.BasicOutlines)
            //            {
            //                if (otl.customOutline == null)
            //                    continue;
            //                if (otl.customOutline.AmplitudeSettings.FormulaExpression
            //                    .AssignsToVariable("angle"))
            //                {
            //                    int ptnInd = patternChoices.PatternGroups.IndexOf(ptnList);
            //                    int page = 1 + ptnInd / patternsPerPage;
            //                    int row = 1 + ptnInd % patternsPerPage / selectPatternForm.BoxesAcross;
            //                    int col = 1 + (ptnInd % patternsPerPage) % selectPatternForm.BoxesAcross;
            //                    int outlineNo = 1 + pattern.BasicOutlines.IndexOf(otl);
            //                    messages.Add($"Page {page}, row {row}, column {col}, outline #{outlineNo}");
            //                }
            //            }
            //        }
            //    }
            //    MessageBox.Show(string.Join(Environment.NewLine, messages));
            //}
            //catch (Exception ex)
            //{
            //    Tools.HandleException(ex);
            //}
        }

        //private void scanForPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        const string folderName = @"C:\WhorlFilesDev\ToScan";
        //        const double maxDistance = 20;
        //        List<string> fileNames = 
        //            Directory.EnumerateFiles(folderName, "*.xml").OrderBy
        //                        (x => File.GetLastWriteTime(x)).ToList();
        //        //MessageBox.Show(String.Join(", ", fileNames.Select(x => Path.GetFileName(x))), "Message");
        //        int addedPatternsCount = 0;
        //        foreach (PatternList prevPtnList in patternChoices.PatternGroups)
        //        {
        //            foreach (Pattern pattern in prevPtnList.Patterns)
        //                pattern.ComputeSeedPoints();
        //        }
        //        foreach (string fileName in fileNames)
        //        {
        //            WriteStatus(fileName);
        //            this.Refresh();
        //            WhorlDesign dgn = new WhorlDesign();
        //            dgn.ReadDesignFromXmlFile(fileName);
        //            List<Pattern> dgnPatterns = new List<Pattern>(dgn.DrawnPatterns);
        //            while (dgnPatterns.Count > 0)
        //            {
        //                Pattern ptn = dgnPatterns[0];
        //                List<Pattern> patterns = AdjustNearbyPatterns(
        //                    dgnPatterns.FindAll(
        //                        x => Tools.Distance(x.Center, ptn.Center) < maxDistance));
        //                dgnPatterns.RemoveAll(x => patterns.Contains(x));
        //                PatternList ptnList = new PatternList();
        //                ptnList.Patterns.AddRange(patterns);
        //                ptnList.SetProperties();
        //                foreach (Pattern pattern in ptnList.Patterns)
        //                    pattern.ComputeSeedPoints();
        //                bool isNew = true;
        //                foreach (PatternList prevPtnList in patternChoices.PatternGroups)
        //                {
        //                    if (ptnList.IsEquivalent(prevPtnList))
        //                    {
        //                        isNew = false;
        //                        break;
        //                    }
        //                }
        //                if (isNew)
        //                {
        //                    patternChoices.PatternGroups.Add(ptnList);
        //                    addedPatternsCount++;
        //                }
        //                ptnList.Dispose();
        //            }
        //        }
        //        MessageBox.Show($"Scanned {fileNames.Count} files; added {addedPatternsCount} patterns.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private void fillPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ptnInfo = NearestPattern(dragStart);
                PathPattern pathPattern = ptnInfo?.Pattern as PathPattern;
                if (pathPattern == null)
                {
                    MessageBox.Show("Please fill a path.");
                    return;
                }
                Ribbon ribbon = 
                    Design.DefaultPatternGroup.Patterns.FirstOrDefault() as Ribbon;
                if (ribbon == null)
                {
                    MessageBox.Show("Please choose a ribbon pattern to fill the path.");
                    return;
                }
                pathPattern.PathRibbon = ribbon;
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void setCenterPathToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var ptnInfo = NearestPattern(dragStart);
        //        var pathPattern = ptnInfo?.Pattern as PathPattern;
        //        if (pathPattern == null)
        //        {
        //            MessageBox.Show("Please set a path as center.");
        //            return;
        //        }
        //        foreach (Pattern pattern in design.DefaultPatternGroup.Patterns)
        //        {
        //            if (pattern.GetType() != typeof(Pattern))
        //            {
        //                MessageBox.Show("Please choose a pattern to have the path for its centers.");
        //                return;
        //            }
        //            var patternCopy = new Pattern(pattern);  //Copy pattern.
        //            patternCopy.CenterPathPattern = null;
        //            patternCopy.Center = pathPattern.Center;
        //            patternCopy.CenterPathPattern = pathPattern;
        //            design.AddPattern(patternCopy);
        //        }
        //        RedrawPatterns();
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private RenderDesignForm renderDesignForm = null;

        private void renderDesignToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentBitmap == null)
                    return;
                //if (renderDesignForm == null || renderDesignForm.IsDisposed)
                var renderDesignForm = new RenderDesignForm();
                string imgFileName = GetDefaultImageFileName();
                if (designFileName != null)
                    imgFileName = Path.Combine(Path.GetDirectoryName(designFileName), 
                                               imgFileName);
                renderDesignForm.Initialize(Design, currentBitmap.Size, imgFileName, 
                                            renderStainedToolStripMenuItem.Checked);
                Tools.DisplayForm(renderDesignForm, this);
                //if (renderDesignForm.ShowDialog() != DialogResult.OK)
                //    return;
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                //Size newSize;
                //if (renderDesignForm.QualityMode)
                //{
                //    newSize = new Size(WhorlSettings.Instance.QualitySize, 
                //        currentBitmap.Height * WhorlSettings.Instance.QualitySize / currentBitmap.Width);
                //}
                //else
                //{
                //    newSize = renderDesignForm.NewSize;
                //}
                //Bitmap bitmap = design.RenderDesign(currentBitmap.Size, 
                //                                    newSize, 
                //                                    renderDesignForm.ScalePenWidth,
                //                                    renderDesignForm.DraftMode,
                //                                    renderStainedToolStripMenuItem.Checked);
                //if (renderDesignForm.QualityMode)
                //{
                //    bitmap = (Bitmap)BitmapTools.ScaleImage(bitmap, renderDesignForm.NewSize);
                //}
                //Tools.SavePngOrJpegImageFile(renderDesignForm.FileName, bitmap);
                //bitmap.Dispose();
                //WriteStatus($"Rendered design in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private PatternZOrderForm patternZOrderForm = null;

        private void zOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ptnInfo = NearestPattern(dragStart);
                if (ptnInfo == null || ptnInfo.RecursiveLevel > 0)
                    return;
                Pattern pattern = ptnInfo?.Pattern;
                if (patternZOrderForm == null || patternZOrderForm.IsDisposed)
                    patternZOrderForm = new PatternZOrderForm();
                int zOrder = Design.IndexOfPattern(pattern);
                patternZOrderForm.Initialize(zOrder, Design.DesignPatterns.Count());
                if (patternZOrderForm.ShowDialog() == DialogResult.OK)
                {
                    if (patternZOrderForm.ZOrder != zOrder)
                    {
                        Design.AddChangeZOrderOperation(pattern, patternZOrderForm.ZOrder);
                        RedrawPatterns();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private async void RedrawPatterns_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => RedrawPatterns());
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pattern _moveTexturePattern = null;
        private Pattern moveTexturePattern
        {
            get { return _moveTexturePattern; }
            set
            {
                _moveTexturePattern = value;
                moveTextureToolStripMenuItem.Checked = _moveTexturePattern != null;
                if (_moveTexturePattern != null)
                {
                    TextureFillInfo txtFill = _moveTexturePattern.FillInfo as TextureFillInfo;
                    if (txtFill != null)
                        initialTextureOffset = txtFill.TextureOffset;
                }
            }
        }

        private Point initialTextureOffset;

        private void moveTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (moveTextureToolStripMenuItem.Checked)
                {
                    var nearbyPatternsInfo = GetNearbyPatterns(dragStart);
                    var ptnInfo = nearbyPatternsInfo.Where(
                            p => p.Pattern.FillInfo.FillType == FillInfo.FillTypes.Texture).FirstOrDefault();
                    moveTexturePattern = ptnInfo?.Pattern;
                }
                else
                    moveTexturePattern = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private ZoomFactorForm zoomFactorForm = null;

        private float GetZoomFactor(out bool keepCenters, out bool cancelled)
        {
            if (zoomFactorForm == null || zoomFactorForm.IsDisposed)
                zoomFactorForm = new ZoomFactorForm();
            cancelled = zoomFactorForm.ShowDialog() != DialogResult.OK;
            if (cancelled)
            {
                keepCenters = false;
                return 0;
            }
            else
            {
                keepCenters = zoomFactorForm.KeepCenters;
                return zoomFactorForm.ZoomFactors.X;
            }
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<Pattern> selectedPatterns = 
                    Design.DesignPatterns.Where(ptn => ptn.Selected).ToList();
                if (selectedPatterns.Count == 0)
                {
                    MessageBox.Show("Please select the patterns to zoom.");
                    return;
                }
                float factor = GetZoomFactor(out bool keepCenters, out bool cancelled);
                if (cancelled)
                    return;
                Size prevSize = picDesign.ClientRectangle.Size;
                Size newSize = new Size(
                    (int)(factor * prevSize.Width),
                    (int)(factor * prevSize.Height));
                List<Pattern> copiedPatterns = 
                    selectedPatterns.Select(ptn => ptn.GetCopy()).ToList();
                ScalePatternMode mode = keepCenters ? ScalePatternMode.KeepCenter : ScalePatternMode.OldCenter;
                Design.ScalePatterns(copiedPatterns, prevSize, newSize, mode: mode);
                Design.ReplacePatterns(selectedPatterns, copiedPatterns);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void deleteAllSelectedPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<Pattern> selectedPatterns = Design.DesignPatterns.Where(ptn => ptn.Selected).ToList();
                if (selectedPatterns.Count > 0)
                {
                    Design.RemovePatterns(selectedPatterns);
                    RedrawPatterns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void keepNewRandomSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Pattern pattern in Design.DesignPatterns)
                {
                    pattern.OrigRandomSeed = pattern.RandomGenerator.RandomSeed;
                }
                WriteStatus("Copied new random settings.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void revertToOriginalRandomSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Pattern pattern in Design.DesignPatterns)
                {
                    if (pattern.OrigRandomSeed != null && 
                        pattern.RandomGenerator.RandomSeed != pattern.OrigRandomSeed)
                    {
                        pattern.RandomGenerator.ReseedRandom(pattern.OrigRandomSeed);
                    }
                }
                RedrawPatterns(computeRandom: true);
                WriteStatus("Copied new random settings.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void selectRandomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Pattern pattern in Design.DesignPatterns)
                {
                    pattern.Selected = pattern.HasRandomElements;
                }
                picDesign.Invalidate();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool DrawUserVertices
        {
            get
            {
                return drawClosedCurveToolStripMenuItem.Checked ||
                       drawPolygonToolStripMenuItem.Checked;
            }
        }

        private void drawPolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawClosedCurveToolStripMenuItem.Checked = !drawPolygonToolStripMenuItem.Checked;
        }

        private void drawClosedCurveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawPolygonToolStripMenuItem.Checked = !drawClosedCurveToolStripMenuItem.Checked;
        }

        private DesignLayersForm designLayersForm = new DesignLayersForm();

        private void ShowDesignLayersForm(List<Pattern> selectedPatterns = null)
        {
            if (designLayersForm.IsDisposed)
            {
                designLayersForm = new DesignLayersForm();
                designLayersForm.SetDesign(this.Design);
            }
            if (selectedPatterns != null)
                designLayersForm.SelectedPatterns = selectedPatterns;
            Tools.DisplayForm(designLayersForm, this);
        }

        private void designLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ShowDesignLayersForm();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setPatternLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var ptnInfo = NearestPattern(dragStart);
                if (ptnInfo == null || ptnInfo.RecursiveLevel > 0)
                    return;
                Pattern pattern = ptnInfo.Pattern;
                ShowDesignLayersForm(new List<Pattern>() { pattern });
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setSelectedPatternsLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<Pattern> selectedPatterns = Design.DesignPatterns.Where(
                    ptn => ptn.Selected).ToList();
                if (selectedPatterns.Count > 0)
                    ShowDesignLayersForm(selectedPatterns);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private PathFillInfo GetSelectedPatternPathFillInfo()
        {
            Pattern pattern = Design.FindPattern(
                              ptn => ptn.Selected && ptn.FillInfo is PathFillInfo);
            PathFillInfo pathFillInfo = pattern?.FillInfo as PathFillInfo;
            if (pathFillInfo == null)
            {
                MessageBox.Show("Please select a pattern with Path fill type.");
                return null;
            }
            return pathFillInfo;
        }

        private void testRadialColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PathFillInfo pathFillInfo = GetSelectedPatternPathFillInfo();
                if (pathFillInfo == null)
                    return;
                pathFillInfo.ColorMode = FillInfo.PathColorModes.Radial;
                pathFillInfo.ColorInfo.AddOrSetColorAtPosition(Color.Yellow, 0.3F);
                pathFillInfo.ColorInfo.AddOrSetColorAtPosition(Color.Blue, 0.7F);
                RedrawPatterns();
                WriteStatus("Set pattern to use radial colors.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void testSurroundColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PathFillInfo pathFillInfo = GetSelectedPatternPathFillInfo();
                if (pathFillInfo == null)
                    return;
                pathFillInfo.ColorMode = FillInfo.PathColorModes.Surround;
                //pathFillInfo.BoundaryColor = Color.Red;
                pathFillInfo.ColorInfo.AddOrSetColorAtPosition(Color.Yellow, 0.3F);
                pathFillInfo.ColorInfo.AddOrSetColorAtPosition(Color.Blue, 0.7F);
                RedrawPatterns();
                WriteStatus("Set pattern to use surround colors.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void testPatternRecursionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = Design.FindPattern(ptn => ptn.Selected);
                if (pattern == null)
                    return;
                pattern.Recursion.Repetitions = 
                    pattern.BasicOutlines.Select(otl => (int)otl.Petals).Min();
                pattern.Recursion.Scale = 1F / 3F;
                pattern.Recursion.Depth = 2;
                pattern.Recursion.RotationOffsetRatio =
                    (float)pattern.MaxPointIndex / (pattern.SeedPoints.Length - 1);
                pattern.Recursion.IsRecursive = true;
                RedrawPatterns();
                WriteStatus("Set pattern to be recursive.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void MergeDesign(bool above)
        {
            OpenFileDialog dlg = GetOpenDesignFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                WhorlDesign mergedDesign = new WhorlDesign(OnDistancePatternsCountChanged);
                mergedDesign.ReadDesignFromXmlFile(dlg.FileName);
                Design.AddPatterns(mergedDesign.DesignPatterns, insertIndex: above ? -1 : 0);
                RedrawPatterns();
            }
        }

        private void mergeDesignBelowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MergeDesign(above: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void mergeDesignAboveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MergeDesign(above: true);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editImprovConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var frm = new ImprovConfigForm())
                {
                    frm.Initialize(Design);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void EditPatternImproviseConfig(Pattern pattern)
        {
            if (Design.ImproviseConfig == null)
                Design.ImproviseConfig = new ImproviseConfig();
            if (pattern.PatternImproviseConfig == null)
            {
                pattern.PatternImproviseConfig = new PatternImproviseConfig();
                pattern.PatternImproviseConfig.PopulateFromPattern(pattern);
            }
            pattern.PatternImproviseConfig.PopulateColorIndicesFromPattern(pattern, checkCounts: true);
            using (var frm = new PatternImprovConfigForm())
            {
                frm.Initialize(pattern);
                frm.ShowDialog();
            }
        }

        private void editPatternImproviseConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var nearbyPatterns = GetNearbyPatterns(dragStart);
                if (!nearbyPatterns.Any())
                    return;
                Pattern.PatternInfo pattern;
                if (nearbyPatterns.Count() == 1)
                    pattern = nearbyPatterns.First();
                else
                {
                    var selPatterns = nearbyPatterns.Where(ptn => ptn.Pattern.Selected);
                    if (selPatterns.Count() == 1)
                        pattern = selPatterns.First();
                    else
                    {
                        MessageBox.Show("Please select the target pattern.");
                        return;
                    }
                }
                EditPatternImproviseConfig(pattern.Pattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void recordVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (!VideoOps.IsRecording)
            //    {   //Start recording video
            //        if (currentBitmap == null)
            //            return;
            //        string videoFolder = Path.Combine(WhorlSettings.Instance.FilesFolder,
            //                                          WhorlSettings.Instance.VideosFolder);
            //        if (!Directory.Exists(videoFolder))
            //            Directory.CreateDirectory(videoFolder);
            //        SaveFileDialog dlg = new SaveFileDialog();
            //        dlg.Filter = "Video file (*.avi)|*.avi";
            //        dlg.InitialDirectory = videoFolder;
            //        if (!string.IsNullOrEmpty(VideoOps.FileName))
            //            dlg.FileName = Path.GetFileName(VideoOps.FileName);
            //        if (dlg.ShowDialog() != DialogResult.OK)
            //            return;  //User cancelled.
            //        VideoOps.OpenWriter(dlg.FileName, currentBitmap.Size);
            //        VideoOps.IsRecording = true;
            //        WriteStatus("Recording video.");
            //    }
            //    else
            //    {   //Stop recording video
            //        VideoOps.IsRecording = false;
            //        VideoOps.CloseWriter();
            //        WriteStatus("Stopped recording video.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tools.HandleException(ex);
            //}
        }

        private NamedRandomSeedForm randomSeedForm = null;

        private void editRandomSeedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (randomSeedForm == null || randomSeedForm.IsDisposed)
                    randomSeedForm = new NamedRandomSeedForm();
                randomSeedForm.Initialize(this.seededRandom, this.Design);
                randomSeedForm.ShowDialog();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void keepHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!keepHistoryToolStripMenuItem.Checked)
                {
                    ClearImprovDesigns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private async void saveAllDesignThumbnailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string xmlFilesFolder = WhorlSettings.Instance.FilesFolder;
                string thumbnailsFolder = Path.Combine(xmlFilesFolder, WhorlSettings.Instance.DesignThumbnailsFolder);
                if (!Directory.Exists(thumbnailsFolder))
                    Directory.CreateDirectory(thumbnailsFolder);
                List<string> messages = new List<string>();
                int count = 0;
                var xmlFileNames = Directory.EnumerateFiles(xmlFilesFolder, "*.xml");
                MessageBox.Show($"Found {xmlFileNames.Count()} XML design files.");
                foreach (string xmlFileName in xmlFileNames)
                {
                    string thumbnailFileName = Path.Combine(thumbnailsFolder, Path.GetFileNameWithoutExtension(xmlFileName) + ".jpg");
                    if (File.Exists(thumbnailFileName))
                        continue;
                    try
                    {
                        await OpenDesignFromXml(xmlFileName);
                    }
                    catch (Exception ex)
                    {
                        messages.Add($"{xmlFileName}: {ex.Message}");
                        if (messages.Count > 20)
                            break;
                        else
                            continue;
                    }
                    int height = WhorlSettings.Instance.DesignThumbnailHeight;
                    Size newSize = new Size(currentBitmap.Width * height / currentBitmap.Height, height);
                    var thumbnailBitmap = (Bitmap)BitmapTools.ScaleImage(currentBitmap, newSize);
                    Tools.SavePngOrJpegImageFile(thumbnailFileName, thumbnailBitmap);
                    DateTime xmlCreationTime = File.GetCreationTime(xmlFileName);
                    File.SetCreationTime(thumbnailFileName, xmlCreationTime);

                    count++;

                    //if (count >= 500)
                    //    break;
                }
                if (messages.Count > 0)
                {
                    MessageBox.Show(string.Join(Environment.NewLine, messages));
                }
                WriteStatus($"Saved {count} thumbnail image files.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void fixDesignsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string xmlFilesFolder = WhorlSettings.Instance.FilesFolder;
                int count = 0, fixedCount = 0;
                List<string> xmlFileNames = Directory.EnumerateFiles(xmlFilesFolder, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                xmlFileNames.AddRange(Directory.EnumerateFiles(
                                      Path.Combine(xmlFilesFolder, WhorlSettings.Instance.CustomDesignParentFolder), 
                                      "*.xml", SearchOption.AllDirectories));
                foreach (string xmlFileName in xmlFileNames)
                {
                    Design.ReadDesignFromXmlFile(xmlFileName, showWarnings: true);
                    count++;
                    if (!Design.ScaleToFit)
                    {
                        //design.PictureBoxSize = design.PreviousPictureSize;
                        Design.ScaleToFit = true;
                        Design.XmlPictureBoxSize = Design.PictureBoxSize;
                        XmlTools.WriteToXml(xmlFileName, Design);
                        fixedCount++;
                    }
                }
                WriteStatus($"Read {count} designs; fixed {fixedCount}.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Ribbon GetNearbyRibbon(PointF p, out PathPattern pathPattern)
        {
            Ribbon ribbon = null;
            pathPattern = null;
            var patterns = GetNearbyPatterns(p);
            if (!patterns.Any())
                return null;
            if (patterns.Count() == 1)
                ribbon = patterns.First().Pattern as Ribbon;
            else
            {
                var selPatterns = patterns.Where(ptn => ptn.Pattern.Selected);
                if (selPatterns.Count() == 1)
                    ribbon = selPatterns.First().Pattern as Ribbon;
            }
            if (ribbon == null)
            {
                var ptn = patterns.FirstOrDefault(pInfo => pInfo.Pattern is PathPattern);
                pathPattern = ptn?.Pattern as PathPattern;
                if (pathPattern != null)
                    ribbon = pathPattern.PathRibbon;
            }
            return ribbon;
        }

        private void reverseRibbonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbon = GetNearbyRibbon(dragStart, out PathPattern pathPattern);
                if (ribbon == null)
                {
                    MessageBox.Show("Please select the ribbon to reverse.");
                    return;
                }
                Ribbon ribbonCopy;
                Pattern oldPattern, newPattern;
                if (pathPattern != null)
                {
                    oldPattern = pathPattern;
                    var pathPatternCopy = pathPattern.GetCopy() as PathPattern;
                    ribbonCopy = pathPatternCopy.PathRibbon;
                    newPattern = pathPatternCopy;
                }
                else
                {
                    oldPattern = ribbon;
                    ribbonCopy = ribbon.GetCopy() as Ribbon;
                    newPattern = ribbonCopy;
                }
                ribbonCopy.DrawReversed = !ribbonCopy.DrawReversed;
                Design.ReplacePattern(oldPattern, newPattern);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Ribbon GetRibbonCopy(out Ribbon ribbon)
        {
            ribbon = GetNearbyRibbon(dragStart, out PathPattern pathPattern);
            if (ribbon == null || pathPattern != null)
                return null;
            return (Ribbon)ribbon.GetCopy();
        }

        private void recomputeRibbonPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbonCopy = GetRibbonCopy(out Ribbon ribbon);
                if (ribbonCopy == null)
                    return;
                ribbonCopy.RecomputeRibbonPath();
                Design.ReplacePattern(ribbon, ribbonCopy);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void flipRibbonVerticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbonCopy = GetRibbonCopy(out Ribbon ribbon);
                if (ribbonCopy == null)
                    return;
                ribbonCopy.FlipPathVertically();
                Design.AddPattern(ribbonCopy);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void flipRibbonHorizontallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbonCopy = GetRibbonCopy(out Ribbon ribbon);
                if (ribbonCopy == null)
                    return;
                ribbonCopy.FlipPathHorizontally();
                Design.AddPattern(ribbonCopy);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }



        private void scaleRibbonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbon = GetNearbyRibbon(dragStart, out PathPattern pathPattern);
                if (ribbon == null || pathPattern != null)
                {
                    MessageBox.Show("Please select the ribbon to scale.");
                    return;
                }
                Ribbon ribbonCopy = (Ribbon)ribbon.GetCopy();
                ribbonCopy.ScaleRibbonPath();
                Design.ReplacePattern(ribbon, ribbonCopy);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pattern tileInfoPattern { get; set; }

        private void showTileGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (showTileGridToolStripMenuItem.Checked)
                {
                    var patternInfo = GetNearbyPatterns(dragStart).FirstOrDefault();
                    if (patternInfo == null || !patternInfo.Pattern.PatternTileInfo.TilePattern)
                        return;
                    tileInfoPattern = patternInfo.Pattern;
                    showGridToolStripMenuItem.Checked = true;
                }
                else
                {
                    tileInfoPattern = null;
                    showGridToolStripMenuItem.Checked = false;
                }
                picDesign.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void drawLogoPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (PatternChoices.LogoPatternGroup == null)
                {
                    MessageBox.Show("Please set a pattern as the logo.");
                    return;
                }
                drawLogoPatternToolStripMenuItem.Checked = true;
                WriteStatus("Click on the point to place the logo.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setLogoPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList logoPatternGroup = GetPatternOrGroup(dragStart, "Set a pattern group as the logo?");
                if (logoPatternGroup != null)
                    PatternChoices.LogoPatternGroup = logoPatternGroup;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void AdjustBackgroundPattern(Size imgSize, Pattern pattern)
        {
            BasicOutline basicOutline = pattern.BasicOutlines.FirstOrDefault();
            if (basicOutline == null)
                return;
            SetAddDenomForBackgroundPattern(imgSize, basicOutline);
            Point imgCenter = new Point(imgSize.Width / 2, imgSize.Height / 2);
            double modulus = Math.Sqrt((double)imgCenter.X * imgCenter.X + (double)imgCenter.Y * imgCenter.Y);
            if (basicOutline != null && basicOutline.BasicOutlineType == BasicOutlineTypes.NewEllipse)
            {
                double angle = Math.Atan2(imgCenter.X, imgCenter.Y);
                double amplitude = basicOutline.ComputeAmplitude(angle);
                double zoomFactor = 1.01 * modulus / (amplitude * imgCenter.X);
                modulus = (double)imgCenter.X;
                pattern.ZoomFactor = zoomFactor;
            }
            else
            {
                modulus *= 1.02;
            }
            pattern.ZVector = new Complex(0, modulus);
            pattern.ComputeSeedPoints();
        }

        private void fillImageWithDefaultPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PatternList defaultPtn = Design.DefaultPatternGroup;
                if (defaultPtn == null)
                    return;
                PatternList ptnList = defaultPtn.GetCopy();
                Point imgCenter = GetPictureBoxCenter();
                ptnList.SetCenters(imgCenter);
                BasicOutline basicOutline = null;
                if (ptnList.PatternsList.Count == 1 && ptnList.PatternsList[0].BasicOutlines.Count == 1)
                {
                        basicOutline = ptnList.PatternsList[0].BasicOutlines[0];
                }
                double modulus = Math.Sqrt((double)imgCenter.X * imgCenter.X + (double)imgCenter.Y * imgCenter.Y);
                if (basicOutline != null && basicOutline.BasicOutlineType == BasicOutlineTypes.NewEllipse)
                {
                    double angle = Math.Atan2(imgCenter.X, imgCenter.Y);
                    double amplitude = basicOutline.ComputeAmplitude(angle);
                    double zoomFactor = 1.01 * modulus / (amplitude * imgCenter.X);
                    modulus = (double)imgCenter.X;
                    ptnList.PatternsList[0].ZoomFactor = zoomFactor;
                }
                else
                {
                    modulus *= 1.02;
                }
                ptnList.SetZVectors(new Complex(0, modulus));
                Pattern topBackgroundPattern = Design.DesignPatterns.Where(ptn => ptn.IsBackgroundPattern).LastOrDefault();
                int insertIndex = topBackgroundPattern == null ? 0 : 1 + Design.IndexOfPattern(topBackgroundPattern);
                foreach (Pattern pattern in ptnList.Patterns)
                {
                    pattern.IsBackgroundPattern = true;
                }
                Design.AddPatterns(ptnList.Patterns, insertIndex);
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void testShrinkingPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Ribbon ribbon = Design.FindPattern(ptn => ptn.Selected && ptn is Ribbon) as Ribbon;
                if (ribbon == null)
                {
                    PathPattern pathPattern = Design.FindPattern(ptn => ptn.Selected & ptn is PathPattern) as PathPattern;
                    if (pathPattern != null)
                        ribbon = pathPattern.PathRibbon;
                }
                if (ribbon == null)
                {
                    MessageBox.Show("Please select the ribbon.");
                    return;
                }
                ribbon.DrawReversed = !ribbon.DrawReversed;
                RedrawPatterns();
                //Pattern ptn = design.FindPattern(p => p.Selected);
                //if (ptn == null)
                //    return;
                //float padding = 0.02F;
                //if (ptn.SeedPoints == null)
                //    ptn.ComputeSeedPoints();
                //for (int i = 1; i < ptn.SeedPoints.Length; i++)
                //{
                //    PolarCoord pc1 = ptn.SeedPoints[i - 1];
                //    //if (pc1.Modulus <= 4F * padding)
                //    //    continue;
                //    PolarCoord pc2 = ptn.SeedPoints[i];
                //    PointF p1, p2;
                //    p1 = new PointF(pc1.Modulus * (float)Math.Cos(pc1.Angle), pc1.Modulus * (float)Math.Sin(pc1.Angle));
                //    p2 = new PointF(pc2.Modulus * (float)Math.Cos(pc2.Angle), pc2.Modulus * (float)Math.Sin(pc2.Angle));
                //    float dist = (float)Tools.Distance(p1, p2);
                //    PointF unitVec = new PointF((p2.X - p1.X) / dist, (p2.Y - p1.Y) / dist);
                //    PointF perpVec = new PointF(padding * unitVec.Y, -padding * unitVec.X);
                //    p1 = new PointF(p1.X - perpVec.X, p1.Y - perpVec.Y);
                //    var pcNew = new PolarCoord();
                //    pcNew.Angle = (float)Math.Atan2(p1.Y, p1.X);
                //    pcNew.Modulus = (float)Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);
                //    if (pcNew.Modulus < 1F - 6F * padding)
                //        ptn.SeedPoints[i - 1] = pcNew;
                //    else if (i > 1)
                //        ptn.SeedPoints[i - 1] = ptn.SeedPoints[i - 2];
                //}
                //RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void equalizeCentersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var nearbyPatterns = GetNearbyPatterns(dragStart).ToList();
                if (nearbyPatterns.Count < 2)
                    return;
                var firstPattern = nearbyPatterns[0];
                for (int i = 1; i < nearbyPatterns.Count; i++)
                {
                    nearbyPatterns[i].Pattern.Center = firstPattern.Center;
                }
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void equalizeSizesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var nearbyPatterns = GetNearbyPatterns(dragStart).ToList();
                if (nearbyPatterns.Count < 2)
                    return;
                var firstPattern = nearbyPatterns[0];
                for (int i = 1; i < nearbyPatterns.Count; i++)
                {
                    nearbyPatterns[i].Pattern.ZVector = firstPattern.ZVector;
                }
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setPatternsToHighResolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int changedCount = 0;
                foreach (Pattern pattern in Design.DesignPatterns)
                {
                    if (pattern.RotationSteps == 500)
                    {
                        pattern.RotationSteps = 5000;
                        changedCount++;
                    }
                }
                if (changedCount > 0)
                    RedrawPatterns();
                MessageBox.Show($"Changed {changedCount} patterns.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetAllAllowRandom(bool allow)
        {
            try
            {
                foreach (Pattern ptn in Design.DesignPatterns)
                {
                    ptn.AllowRandom = allow;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setAllowRandomOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAllAllowRandom(true);
        }

        private void setAllowRandomOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAllAllowRandom(false);
        }

        private static frmMessage frmMessage;

        public static void ViewDebugMessages()
        {
            if (frmMessage == null || frmMessage.IsDisposed)
            {
                frmMessage = new frmMessage();
            }
            Tools.DisplayForm(frmMessage);
            frmMessage.DisplayMessages();
        }

        private void viewDebugMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewDebugMessages();
        }

        //private float GetColorPosition()
        //{
        //    const double waves = 10;
        //    float position = pixelRenderInfo.DefaultGetPosition();
        //    double a = waves * Math.PI * position;
        //    //double a1 = waves * Math.PI * pixelRenderInfo.X / pixelRenderInfo.BoundsSize.Width;
        //    //double a2 = waves * Math.PI * pixelRenderInfo.Y / pixelRenderInfo.BoundsSize.Height;
        //    position += (float)(0.2 * (1 + Math.Sin(a)));
        //    return position;
        //}

        private void testStringPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var stringPattern = new StringPattern(Design, FillInfo.FillTypes.Path);
                stringPattern.KeepRightAngle = false;
                var pathFillInfo = stringPattern.FillInfo as PathFillInfo;
                if (pathFillInfo != null)
                {
                    pathFillInfo.BoundaryColor = Color.Red;
                    pathFillInfo.CenterColor = Color.Yellow;
                }
                stringPattern.Text = "Here is the Text.";
                stringPattern.ComputeSeedPoints();
                var patternList = new PatternList(Design);
                patternList.AddPattern(stringPattern);
                patternList.SetProperties();
                Design.DefaultPatternGroup = patternList;
                //stringPattern.Center = new PointF(250, 250);
                //stringPattern.ZVector = new Complex(200, 20);
                //stringPattern.ZVector = new Complex(-200, 100);
                //stringPattern = (StringPattern)stringPattern.GetCopy();
                //design.AddPattern(stringPattern);
                //RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void redisplayAllPatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                RedrawPatternsAsync();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void InferFormulaTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormulaTools formulaTools = new FormulaTools(Design);
                var changedFormulas = formulaTools.InferFormulaTypes();
                var formulaTypes = changedFormulas.Select(fe => fe.FormulaType).Distinct();
                string message = string.Join("; ", formulaTypes.Select(t => $"{t}: {changedFormulas.Count(fe => fe.FormulaType == t)}"));
                MessageBox.Show(message);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void CreateSettingsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = SettingsXML.GetSettingsXmlFilePath();
                if (File.Exists(fileName))
                {

                    if (MessageBox.Show("Overwrite settings file?", "Confirm", MessageBoxButtons.OKCancel) != DialogResult.OK)
                        return;
                }
                XmlDocument xmlDocument = SettingsXML.CreateSettingXML();
                SettingsXML.SaveSettingsXml(xmlDocument);
                MessageBox.Show("Saved the settings to XML.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void CreateSettingsCSharpCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = SettingsXML.GetSettingsXmlFilePath();
                if (!File.Exists(fileName))
                {
                    MessageBox.Show("Settings file does not exist.");
                    return;
                }
                XmlDocument xmlDocument = SettingsXML.ReadSettingsXML();
                string cSharpCode = SettingsXML.GetCSharpSettingsCode(xmlDocument, out List<string> errors);
                if (errors.Any())
                {
                    MessageBox.Show(string.Join(Environment.NewLine, errors));
                }
                using (var frm = new frmTextEditor())
                {
                    frm.DisplayText(cSharpCode);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void scaleToFitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Design.SetScaleToFit(scaleToFitToolStripMenuItem.Checked))
                    RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetImageSizesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var sizeSettings = WhorlSettings.Instance.SavedImageSizes.Split(',')
                    .Select(sz => ImageSizeForm.GetPrevImageSize(sz))
                    .Where(info => info != null)
                    .Select(info => info.GetSetting());
                WhorlSettings.Instance.ImageSizes = string.Join(",", sizeSettings);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ShowAppPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteStatus($"App path = {Application.StartupPath}");
        }

        private void viewDistanceOutlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                picDesign.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private async void setSeedPatternFromDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = FindTargetPattern(ptn => ptn.HasPixelRendering);
                if (pattern == null || Design.DefaultPatternGroup == null)
                    return;
                Pattern seedPattern = Design.DefaultPatternGroup.Patterns.FirstOrDefault();
                if (seedPattern != null)
                {
                    pattern.PixelRendering.SetSeedPattern(seedPattern);
                    await Task.Run(() => RedrawPatterns());
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private async void editSeedPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = FindTargetPattern(ptn => ptn.HasPixelRendering);
                if (pattern == null || pattern.PixelRendering.SeedPattern == null)
                    return;
                var ptnList = new PatternList(Design);
                ptnList.AddPattern(pattern.PixelRendering.SeedPattern);
                if (EditPatternGroup(ptnList))
                {
                    var seedPattern = patternForm.EditedPatternGroup.PatternsList.FirstOrDefault();
                    if (seedPattern != null)
                    {
                        pattern.PixelRendering.SetSeedPattern(seedPattern);
                        await Task.Run(() => RedrawPatterns());
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private Pattern distanceParentPattern { get; set; }
        private DistancePatternInfo distancePatternInfo { get; set; }
        private Pattern mouseDistanceParentPattern { get; set; }
        private DistancePatternInfo mouseDistancePatternInfo { get; set; }

        private void drawDistancePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                distanceParentPattern = FindTargetPattern(ptn => ptn.HasPixelRendering);
                distancePatternInfo = null;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void zoomDistancePatternsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                distanceParentPattern = FindTargetPattern(ptn => ptn.HasPixelRendering);
                if (distanceParentPattern == null) 
                    return;
                float factor = GetZoomFactor(out bool keepCenters, out bool cancelled);
                if (cancelled)
                    return;
                distanceParentPattern.PixelRendering.ZoomDistancePatterns(factor, zoomCenters: !keepCenters);
                distanceParentPattern.ClearRenderingCache();
                RedrawPatterns();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private DistancePatternInfo FindDistanceInfo(Point p, out Pattern parentPattern)
        {
            double maxDistanceSquared = WhorlSettings.Instance.BufferSize;
            maxDistanceSquared *= maxDistanceSquared;
            var infos = Design.DesignPatterns.Where(ptn => ptn.UsesDistancePattern)
                              .SelectMany(ptn => ptn.PixelRendering.GetDistancePatternInfos()
                              .Select(pi => new Tuple<Pattern, DistancePatternInfo, double>(
                                      ptn, pi, Tools.DistanceSquared(
                                           pi.GetDistancePatternCenter(ptn), p))))
                              .Where(tpl => tpl.Item3 <= maxDistanceSquared)
                              .OrderBy(tpl => tpl.Item3);
            var tuple = infos.FirstOrDefault();
            //if (tuple == null)
            //    MessageBox.Show("Didn't find Distance Pattern at mouse location.");
            parentPattern = tuple?.Item1;
            return tuple?.Item2;
        }

        private void redrawDistancePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (mouseDistancePatternInfo != null)
                {
                    distancePatternInfo = mouseDistancePatternInfo;
                    distanceParentPattern = mouseDistanceParentPattern;
                    redrawnPatternGroup = new PatternList(Design);
                    redrawnPatternGroup.AddPattern(distancePatternInfo.DistancePattern);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pattern CopyForDistanceInfo(Pattern parent, 
                                            DistancePatternInfo distancePatternInfo, 
                                            out DistancePatternInfo distancePatternInfoCopy)
        {
            Pattern parentCopy = parent.GetCopy();
            distancePatternInfoCopy = distancePatternInfo.FindByKeyGuid(parentCopy.PixelRendering.GetDistancePatternInfos());
            return parentCopy;
        }

        private void editDistancePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (mouseDistancePatternInfo != null)
                {
                    var ptnList = new PatternList(Design);
                    ptnList.AddPattern(mouseDistancePatternInfo.DistancePattern);
                    if (EditPatternGroup(ptnList))
                    {
                        var newDistPattern = patternForm.EditedPatternGroup.PatternsList.FirstOrDefault();
                        if (newDistPattern == null)
                            return;
                        Pattern ptnCopy = CopyForDistanceInfo(mouseDistanceParentPattern,
                                                              mouseDistancePatternInfo,
                                                              out var distInfoCopy);
                        //var distInfoCopy = ptnCopy.PixelRendering.GetDistancePatternInfos()
                        //                   .Where(pi => pi.Guid == mouseDistancePatternInfo.Guid).First();
                        distInfoCopy.SetDistancePattern(ptnCopy, newDistPattern, transform: false);
                        Design.ReplacePattern(mouseDistanceParentPattern, ptnCopy);
                        RedrawPatterns();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editDistancePatternSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (mouseDistancePatternInfo == null || mouseDistanceParentPattern == null)
                    return;
                using (var frm = new FrmEditDistancePatternSettings())
                {
                    frm.Initialize(mouseDistancePatternInfo.DistancePatternSettings, mouseDistanceParentPattern);
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        mouseDistanceParentPattern.ClearRenderingCache();
                        RedrawPatterns();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void deleteDistancePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (mouseDistancePatternInfo != null)
                {
                    Pattern ptnCopy = CopyForDistanceInfo(mouseDistanceParentPattern,
                                                          mouseDistancePatternInfo,
                                                          out var distInfoCopy);
                    bool setEditedPattern = mouseDistanceParentPattern == Design.EditedPattern;
                    if (setEditedPattern)
                        Design.EditedPattern = ptnCopy;
                    ptnCopy.PixelRendering.DeleteDistancePattern(distInfoCopy);
                    Design.ReplacePattern(mouseDistanceParentPattern, ptnCopy);
                    AddRenderingControls(ptnCopy);
                    RedrawPatterns();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Pattern showInfluencePointsDistancePattern { get; set; }

        private void showDistanceInfluencePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (showDistanceInfluencePointsToolStripMenuItem.Checked)
                {
                    if (mouseDistancePatternInfo == null || mouseDistanceParentPattern == null)
                        return;
                    showInfluencePointsDistancePattern = mouseDistancePatternInfo.GetDistancePattern(mouseDistanceParentPattern);
                }
                else
                    showInfluencePointsDistancePattern = null;
                picDesign.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }

        private void addDistancePatternToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (mouseDistancePatternInfo != null)
                {
                    var ptnList = new PatternList(Design);
                    ptnList.AddPattern(mouseDistancePatternInfo.DistancePattern);
                    AddPatternGroupToChoices(ptnList, ClipboardPatterns);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void cancelRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renderCaller.CancelRender = true;
        }

        private void btnCancelRender_Click(object sender, EventArgs e)
        {
            renderCaller.CancelRender = true;
        }

        private async void chkDraftMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (ignoreEvents)
                    return;
                await Task.Run(() => RedrawPatterns());
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private async void cboDraftSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ignoreEvents)
                    return;
                if (cboDraftSize.SelectedItem is int)
                {
                    int draftSize = (int)cboDraftSize.SelectedItem;
                    if (draftSize != WhorlSettings.Instance.DraftSize)
                    {
                        WhorlSettings.Instance.DraftSize = draftSize;
                        if (UseDraftMode)
                        {
                            await Task.Run(() => RedrawPatterns());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editRenderParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = FindTargetPattern(ptn => ptn.HasPixelRendering);
                if (pattern == null) return;
                Design.EditedPattern = pattern;
                AddRenderingControls(Design.EditedPattern);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editRenderingRandomSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Pattern pattern = FindTargetPattern(ptn => ptn.HasPixelRendering);
                if (pattern == null) return;
                using (var frm = new FrmEditPointsRandomOps())
                {
                    frm.Initialize(pattern.PixelRendering);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void erasePictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picDesign.Image = null;
        }

        private frmBlendImages frmBlendImages { get; set; }

        private void blendImagesFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (frmBlendImages == null || frmBlendImages.IsDisposed)
                    frmBlendImages = new frmBlendImages();
                Tools.DisplayForm(frmBlendImages);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void showInfluencePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //if (showInfluencePointsToolStripMenuItem.Checked)
                //{
                //    showSelectionsToolStripMenuItem.Checked = false;
                //}
                picDesign.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool EditInfluencePoint(InfluencePointInfo influencePointInfo)
        {
            using (var frm = new frmInfluencePoint())
            {
                frm.Initialize(influencePointInfo);
                return frm.ShowDialog() == DialogResult.OK;
            }
        }

        private InfluencePointInfo GetNearestInfluencePoint(PointF point, double bufferSize = 10.0)
        {
            if (influencePointsPattern == null)
                return null;
            bufferSize *= bufferSize;
            DoublePoint doublePoint = new DoublePoint(point.X - influencePointsPattern.Center.X, point.Y - influencePointsPattern.Center.Y);
            //INFMOD
            var sortedList = influencePointsPattern.InfluencePointInfoList.InfluencePointInfos.Select(
                             ip => new Tuple<InfluencePointInfo, double>(ip, doublePoint.DistanceSquared(ip.InfluencePoint)))
                             .Where(tpl => tpl.Item2 <= bufferSize)
                             .OrderBy(tpl => tpl.Item2);
            var tuple = sortedList.FirstOrDefault();
            return tuple?.Item1;
        }

        private void addInfluencePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (influencePointsPattern == null)
                    return;
                var influencePoint = new InfluencePointInfo();
                influencePoint.InfluencePoint = new DoublePoint(
                                                dragStart.X - influencePointsPattern.Center.X, 
                                                dragStart.Y - influencePointsPattern.Center.Y);
                if (EditInfluencePoint(influencePoint))
                {
                    //INFMOD
                    influencePoint.AddToPattern(influencePointsPattern);
                    Design.IsDirty = true;
                    picDesign.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editInfluencePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (nearestInfluencePoint == null) return;
                if (EditInfluencePoint(nearestInfluencePoint))
                {
                    Design.IsDirty = true;
                    RedrawInfluencePattern();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void deleteInfluencePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (nearestInfluencePoint == null || influencePointsPattern == null) 
                    return;
                if (MessageBox.Show("Delete Influence Point?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
                influencePointsPattern.InfluencePointInfoList.RemoveInfluencePointInfo(nearestInfluencePoint);
                Design.IsDirty = true;
                RedrawInfluencePattern();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void repeatInfluencePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (nearestInfluencePoint == null || influencePointsPattern == null)
                    return;
                if (nearestInfluencePoint.CopyRadially())
                {
                    Design.IsDirty = true;
                    RedrawInfluencePattern();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }


        private bool moveInfluencePoint { get; set; }

        private void moveInfluencePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (nearestInfluencePoint == null) return;
                nearestInfluencePoint.Selected = true;
                moveInfluencePoint = true;
                picDesign.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editLastInfluenceLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //INFMOD
                //if (influencePointsPattern == null)
                //    return;
                //InfluenceLink link = influencePointsPattern.LastEditedInfluenceLink;
                //var transformParent = link?.Parent as ParameterInfluenceLinkParent;
                //if (transformParent?.TargetParameter == null)
                //    return;
                //using (var frm = new frmInflenceLink())
                //{
                //    frm.Initialize(transformParent.ParentCollection.ParentPattern, transformParent.ParentCollection.PatternTransform, transformParent.TargetParameter);
                //    if (frm.ShowDialog() == DialogResult.OK)
                //    {
                //        design.IsDirty = true;
                //        await Task.Run(() => RedrawPatterns(computeSeedPoints: true));
                //    }
                //}
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editInfluencePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var patternInfo = NearestPattern(dragStart);
                Pattern pattern = patternInfo?.Pattern;
                if (pattern != null)
                {
                    influencePointsPattern = pattern;
                    showInfluencePointsToolStripMenuItem.Checked = true;
                    editInfluencePointsModeToolStripMenuItem.Checked = true;
                    picDesign.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private FrmEditKeyEnumParameters frmEditKeyEnumParameters { get; set; }

        private void editKeyEnumParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var patternInfo = NearestPattern(dragStart);
                Pattern pattern = patternInfo?.Pattern;
                if (pattern != null)
                {
                    if (frmEditKeyEnumParameters == null || frmEditKeyEnumParameters.IsDisposed)
                        frmEditKeyEnumParameters = new FrmEditKeyEnumParameters();
                    frmEditKeyEnumParameters.Initialize(pattern);
                    frmEditKeyEnumParameters.ShowDialog();
                    if (frmEditKeyEnumParameters.ParametersChanged)
                    {
                        Design.IsDirty = true;
                        pattern.ComputeSeedPoints();
                        RedrawPatterns();
                    }
                    if (frmEditKeyEnumParameters.ShouldDisplayParameters)
                    {
                        EditedFormulaSettings = frmEditKeyEnumParameters.FormulaSettings;
                        editedKeyEnumParameters = frmEditKeyEnumParameters.KeyParams;
                        Design.EditedPattern = pattern;
                        EditParameters();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void editInfluencePointsModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (editInfluencePointsModeToolStripMenuItem.Checked &&
                    !showInfluencePointsToolStripMenuItem.Checked)
                { 
                    showInfluencePointsToolStripMenuItem.Checked = true;
                    picDesign.Refresh();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private FillInfo copiedFillInfo;

        //private void copyFillToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Pattern pattern = NearestPattern(dragStart);
        //        if (pattern == null)
        //            return;
        //        copiedFillInfo = pattern.FillInfo;
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private bool EditInfluenceLink(string parameterKey)
        {
            Pattern pattern = Design.EditedPattern;
            FormulaSettings formulaSettings = EditedFormulaSettings;
            if (pattern == null || formulaSettings == null)
                return false;
            if (pattern.InfluencePointInfoList.Count == 0)
            {
                MessageBox.Show("Please add an influence point first.");
                return false;
            }
            using (var frm = new frmInfluenceLink())
            {
                frm.Initialize(pattern, formulaSettings, parameterKey);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    pattern.ComputeSeedPoints();
                    pattern.ClearRenderingCache();
                    RedrawPatterns();
                    return true;
                }
            }
            return false;
        }

        //private bool EditRenderingInfluenceLink(string parameterKey)
        //{
        //    Pattern pattern = design.EditedPattern;
        //    if (pattern == null || !pattern.HasPixelRendering)
        //        return false;
        //    if (pattern.InfluencePointInfoList.Count == 0)
        //    {
        //        MessageBox.Show("Please add an influence point first.");
        //        return false;
        //    }
        //    using (var frm = new frmInfluenceLink())
        //    {
        //        frm.Initialize(pattern, pattern.PixelRendering.FormulaSettings, parameterKey);
        //        if (frm.ShowDialog() == DialogResult.OK)
        //        {
        //            pattern.ClearRenderingCache();
        //            RedrawPatterns();
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private void ShowRenderingPanels(bool show)
        {
            pnlParameters.Visible = pnlSettings.Visible = showRenderingPanelsToolStripMenuItem.Checked = show;
            if (show)
            {
                Size maxSize = GetPictureBoxMaxSize();
                if (picDesign.Width > maxSize.Width)
                    picDesign.Width = maxSize.Width;
            }
        }

        private void showRenderingPanelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ShowRenderingPanels(showRenderingPanelsToolStripMenuItem.Checked);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }

        private void viewUserManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = Path.Combine(Application.StartupPath, "WhorlFiles", "WhorlUserManual.pdf");
                if (!File.Exists(fileName))
                {
                    MessageBox.Show("Didn't find the user manual PDF file.");
                    return;
                }
                System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void testDistancePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var selPatterns = Design.DesignPatterns.Where(p => p.Selected);
                if (selPatterns.Count() != 1)
                    return;
                Pattern distancePattern = selPatterns.First();
                var ptn = new Pattern(Design, FillInfo.FillTypes.Path);
                ptn.ZVector = new Complex(100.0, 0);
                ptn.Center = new PointF(100, 100);
                if (ptn.CheckCreatePixelRendering() != null)
                    return;
                var info = ptn.PixelRendering.AddDistancePattern(ptn, distancePattern);
                Pattern newDistancePattern = info.GetDistancePattern(ptn);
                newDistancePattern.Center = distancePattern.Center;
                Design.ReplacePattern(distancePattern, newDistancePattern);
                newDistancePattern.Selected = true;
                picDesign.Refresh();
                WriteStatus("Replaced selected pattern with distance pattern.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        PointF[] graphPoints { get; set; }
        PointF[] graphFitPoints { get; set; }

        private void testRandomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (testRandomToolStripMenuItem.Checked)
                {
                    var randomValues = new RandomValues();
                    randomValues.Settings.Weight = (float)picDesign.ClientSize.Height;
                    randomValues.Settings.Smoothness = 100F;
                    randomValues.Settings.XLength = picDesign.ClientSize.Height;
                    randomValues.Settings.ClipYValues = true;
                    randomValues.Settings.Closed = true;
                    randomValues.ComputeRandomValues();
                    float[] yValues = randomValues.YValues;
                    float[] xValues = randomValues.XValues;
                    float midY = 0.5F * picDesign.ClientSize.Height;
                    var polarPoints = Enumerable.Range(0, xValues.Length)
                                      .Select(i => new PolarCoord((float)i / xValues.Length * 2F * (float)Math.PI, 0.3F * (midY + yValues[i])));
                    PointF center = new PointF(0.5F * picDesign.ClientSize.Width, midY);
                    graphPoints = polarPoints.Select(pc => pc.ToRectangular()).Select(p => new PointF(p.X + center.X, p.Y + center.Y)).ToArray();
                    //graphPoints = Enumerable.Range(0, xValues.Length)
                    //              .Select(i => new PointF(xValues[i] + 5F, yValues[i] + midY)).ToArray();
                    graphFitPoints = randomValues.FittedPoints.Select(p => new PointF(p.X + 5F, p.Y + midY)).ToArray();
                }
                else
                    graphPoints = graphFitPoints = null;
                picDesign.Refresh();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
