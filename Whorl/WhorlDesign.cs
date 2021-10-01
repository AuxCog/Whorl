using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;
using ParserEngine;

namespace Whorl
{
    public enum ScalePatternMode
    {
        OldCenter,
        NewCenter,
        ScaleCenter,
        KeepCenter
    }

    public class WhorlDesign : WhorlBaseObject
    {
        public const float CurrentXmlVersion = 1.1F;
        public const float NewBlendXmlVersion = 1.1F;
        private Color backgroundColor = Color.Empty;
        private GradientColors backgroundGradientColors = null;

        public string StartupAnimationNames { get; set; }
        public List<Pattern> designPatternsList { get; } = new List<Pattern>();
        public IEnumerable<Pattern> DesignPatterns
        {
            get { return designPatternsList; }
        }
        public int IndexOfPattern(Pattern pattern)
        {
            return designPatternsList.IndexOf(pattern);
        }
        public int FindPatternIndex(Predicate<Pattern> predicate)
        {
            return designPatternsList.FindIndex(predicate);
        }
        public Pattern FindPattern(Predicate<Pattern> predicate)
        {
            return designPatternsList.Find(predicate);
        }
        public Size PreviousPictureSize { get; private set; }
        public Size PictureBoxSize { get; set; }
        public Size XmlPictureBoxSize { get; set; } = Size.Empty;
        public static Size StaticPictureBoxSize { get; private set; }
        private bool _scaleToFit;
        public bool ScaleToFit
        {
            get { return _scaleToFit; }
            set
            {
                if (_scaleToFit != value)
                {
                    _scaleToFit = value;
                    IsDirty = true;
                }
            }
        }
        public EventHandler IsDirtyChanged;
        private bool _isDirty;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value && !readingFromXml)
                {
                    _isDirty = value;
                    if (IsDirtyChanged != null)
                        IsDirtyChanged(this, EventArgs.Empty);
                }
            }
        }
        public EventHandler OnDistancePatternsCountChanged { get; }
        private Pattern _editedPattern;
        public Pattern EditedPattern
        {
            get { return _editedPattern; }
            set
            {
                if (_editedPattern != value)
                {
                    if (OnDistancePatternsCountChanged != null && 
                        _editedPattern != null && _editedPattern.UsesDistancePattern)
                    {
                        _editedPattern.PixelRendering.DistancePatternsCountChanged -= OnDistancePatternsCountChanged;
                    }
                    _editedPattern = value;
                    if (OnDistancePatternsCountChanged != null &&
                        _editedPattern != null && _editedPattern.UsesDistancePattern)
                    {
                        _editedPattern.PixelRendering.DistancePatternsCountChanged += OnDistancePatternsCountChanged;
                    }
                }
            }
        }

        public float XmlVersion { get; private set; } = 0;
        public DesignLayerList DesignLayerList { get; private set; }
        public DesignLayer DefaultDesignLayer { get; set; }
        public bool Initializing { get; set; }
        private bool readingFromXml { get; set; }
        public ImproviseConfig ImproviseConfig { get; set; }
        //public InfluencePointInfoList InfluencePointInfoList { get; } = new InfluencePointInfoList();
        //public InfluenceLink LastEditedInfluenceLink { get; set; }
        //public InfluenceLink CopiedLastEditedInfluenceLink { get; set; }

        public HashSet<string> ReadWarnings { get; } = new HashSet<string>();

        /// <summary>
        /// Dictionary keyed by NamedRandomSeed.Name.
        /// </summary>
        public Dictionary<string, NamedRandomSeed> AnimationSeeds { get; } = 
           new Dictionary<string, NamedRandomSeed>();

        //Settings properties saved to design file:
        private static readonly string[] savedPropertyNames =
            { "ImproviseColors", "ImproviseShapes", "ImprovisePetals", "ImproviseBackground",
              "ImproviseOnPatternType", "AnimationRate", "SpinRate", "RevolveRate"};
        public static List<ToolStripMenuItem> SavedMenuItems { get; set; }
        public static UndoOperations.UndoInfoFn UndoHandlerFn { get; set; }

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                //if (backgroundColor != Color.Empty)
                //{
                //    UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Replace);
                //    op.AddOperationFrame(this, value, backgroundColor);
                //}
                BackgroundGradientColors.BoundaryColor = BackgroundGradientColors.CenterColor = value;
                backgroundColor = value;
                //if (pictureBox != null)
                //    pictureBox.BackColor = value;
            }
        }

        public GradientColors BackgroundGradientColors
        {
            get { return backgroundGradientColors; }
            set
            {
                if (backgroundGradientColors != null)
                {
                    UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Replace);
                    op.AddOperationFrame(this, value, backgroundGradientColors);
                }
                SetBackgroundGradientColorsNoUndo(value);
            }
        }

        public string BackgroundImageFileName { get; set; }
        public TextureImageModes BackgroundImageMode { get; set; } = TextureImageModes.Tile;

        //public void SetBackgroundColorNoUndo(Color color)
        //{
        //    backgroundColor = color;
        //    if (pictureBox != null)
        //        pictureBox.BackColor = color;
        //}

        public static PictureBox pictureBox { get; set; }

        public int SelectedPatternIndex { get; set; } = -1;

        public Pattern SelectedPattern
        {
            get
            {
                return (SelectedPatternIndex >= 0 &&
                        SelectedPatternIndex < designPatternsList.Count) ?
                     designPatternsList[SelectedPatternIndex] : null;
            }
        }

        public PatternList DefaultPatternGroup { get; set; }

        private UndoOperations UndoOps { get; } = new UndoOperations();

        //private Dictionary<string, ParserEngine.Parameter> parametersByGuidDict { get; } = 
        //    new Dictionary<string, ParserEngine.Parameter>();

        static WhorlDesign()
        {
            SavedMenuItems = new List<ToolStripMenuItem>();
        }

        public WhorlDesign(EventHandler onDistancePatternsCountChanged = null)
        {
            OnDistancePatternsCountChanged = onDistancePatternsCountChanged;
            if (UndoHandlerFn != null)
                UndoOps.UndoInfoChanged += UndoHandlerFn;
            DesignLayerList = new DesignLayerList(this);
            BackgroundGradientColors = new GradientColors();
            PreviousPictureSize = PictureBoxSize = pictureBox.ClientRectangle.Size;
        }

        /// <summary>
        /// Create copy of source design.
        /// </summary>
        /// <param name="source"></param>
        public WhorlDesign(WhorlDesign source)
        {
            try
            {
                this.Initializing = true;
                OnDistancePatternsCountChanged = source.OnDistancePatternsCountChanged;
                if (UndoHandlerFn != null)
                    UndoOps.UndoInfoChanged += UndoHandlerFn;
                Tools.CopyProperties(this, source, excludedPropertyNames:
                    new string[] { nameof(BackgroundColor), nameof(DesignLayerList),
                                   nameof(DefaultDesignLayer), nameof(Initializing), nameof(IsDirty) });
                this.designPatternsList.AddRange(source.designPatternsList.Select(ptn => ptn.GetCopy()));
                this.DesignLayerList = new DesignLayerList(this, source);
                if (source.DefaultDesignLayer != null)
                {
                    int index = source.DesignLayerList.GetDesignLayerIndex(
                                source.DefaultDesignLayer);
                    if (index >= 0)
                        this.DefaultDesignLayer = this.DesignLayerList.GetDesignLayer(index);
                }
                CopyNamedRandomSeeds(source);
            }
            finally
            {
                this.Initializing = false;
            }
        }

        public IEnumerable<Pattern.PatternInfo> GetAllPatternsInfo(bool includeRecursive = false)
        {
            var designPatternsInfo = designPatternsList.Select(ptn => new Pattern.PatternInfo(ptn));
            if (includeRecursive)
            {
                var subPatternsInfo = designPatternsList.Where(ptn => ptn.Recursion.IsRecursive)
                                                    .SelectMany(ptn => ptn.Recursion.RecursionPatternsInfo)
                                                    .SelectMany(lst => lst);
                designPatternsInfo = designPatternsInfo.Concat(subPatternsInfo);
            }
            return designPatternsInfo;
        }

        public NamedRandomSeed GetNamedRandomSeed(string name)
        {
            NamedRandomSeed randomSeed;
            if (AnimationSeeds.TryGetValue(name, out randomSeed))
                return randomSeed;
            else
                return null;
        }

        public void CopyNamedRandomSeeds(WhorlDesign sourceDesign)
        {
            AnimationSeeds.Clear();
            foreach (NamedRandomSeed randomSeed in sourceDesign.AnimationSeeds.Values)
            {
                //Add copies of source random seeds:
                this.AnimationSeeds.Add(randomSeed.Name, new NamedRandomSeed(randomSeed));
            }
        }

        public NamedRandomSeed AddNamedRandomSeed(RandomGenerator seededRandom)
        {
            seededRandom.ReseedRandom();
            var namedSeed = new NamedRandomSeed();
            namedSeed.Seed = seededRandom.RandomSeed;
            int i = AnimationSeeds.Count;
            do
            {
                namedSeed.Name = $"Seed{++i}";
            } while (AnimationSeeds.ContainsKey(namedSeed.Name));
            AnimationSeeds.Add(namedSeed.Name, namedSeed);
            return namedSeed;
        }

        public void SetBackgroundGradientColorsNoUndo(GradientColors colors)
        {
            backgroundGradientColors = colors;
            //if (pictureBox != null)
            //    pictureBox.BackColor = colors.BoundaryColor;
        }

        public void SelectPatterns(IEnumerable<Pattern> patterns, bool selected = true)
        {
            foreach (Pattern pattern in patterns)
            {
                pattern.Selected = selected;
            }
        }

        private Bitmap CreateDesignBitmapFromImageFile(int width, int height, float scaleX)
        {
            Bitmap bitmap;
            Image imgBitmap = TextureFillInfo.GetTextureImage(BackgroundImageFileName);
            if (BackgroundImageMode == TextureImageModes.Stretch)
                bitmap = (Bitmap)BitmapTools.ScaleImage(imgBitmap, new Size(width, height));
            else 
            {   //Tile background image:
                bitmap = BitmapTools.CreateFormattedBitmap(new Size(width, height));
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    using (var txtrBr = new TextureBrush(imgBitmap))
                    {
                        txtrBr.WrapMode = WrapMode.Tile;
                        float scaleY;
                        if (BackgroundImageMode == TextureImageModes.StretchTile)
                        {
                            float scaledWidth = scaleX * imgBitmap.Width;
                            float scaledHeight = scaleX * imgBitmap.Height;
                            float tilesAcross = (float)Math.Ceiling(width / scaledWidth);
                            scaledWidth = width / tilesAcross;
                            scaleX = scaledWidth / imgBitmap.Width;
                            float tilesDown = (float)Math.Ceiling(height / scaledHeight);
                            scaledHeight = height / tilesDown;
                            scaleY = scaledHeight / imgBitmap.Height;
                        }
                        else
                            scaleY = scaleX;
                        if (scaleX != 1F || scaleY != 1F)
                        {
                            txtrBr.ScaleTransform(scaleX, scaleY);
                        }
                        gr.FillRectangle(txtrBr, 0, 0, width - 1, height - 1);
                    }
                }
            }
            return bitmap;
        }

        private Bitmap CreateDesignGradientBitmap(int width, int height,
                                                  ref GraphicsPath backgroundGrPath,
                                                  ref PathGradientBrush backgroundPthGrBrush)
        {
            Bitmap bitmap = BitmapTools.CreateFormattedBitmap(new Size(width, height));
            //Draw background:
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Point picCenter = new Point(width / 2, height / 2);
                int radius = 10 + (int)Math.Sqrt(picCenter.X * picCenter.X
                                                 + picCenter.Y * picCenter.Y);
                var backgroundRect = new Rectangle(picCenter.X - radius,
                                                   picCenter.Y - radius,
                                                   2 * radius, 2 * radius);
                if (backgroundGrPath == null)
                {
                    backgroundGrPath = new GraphicsPath();
                    backgroundGrPath.AddEllipse(backgroundRect);
                    if (backgroundPthGrBrush != null)
                    {
                        backgroundPthGrBrush.Dispose();
                        backgroundPthGrBrush = null;
                    }
                }
                if (backgroundPthGrBrush == null)
                    backgroundPthGrBrush = new PathGradientBrush(backgroundGrPath);
                backgroundPthGrBrush.CenterColor = BackgroundGradientColors.CenterColor;
                Color[] colors = { BackgroundGradientColors.BoundaryColor };
                backgroundPthGrBrush.SurroundColors = colors;
                g.FillEllipse(backgroundPthGrBrush, backgroundRect);
            }
            return bitmap;
        }

        public Bitmap CreateDesignBitmap(int width, int height,
                                         ref GraphicsPath backgroundGrPath,
                                         ref PathGradientBrush backgroundPthGrBrush,
                                         float scaleFactor = 1F)
        {
            if (!string.IsNullOrEmpty(BackgroundImageFileName) &&
                          File.Exists(BackgroundImageFileName))
                return CreateDesignBitmapFromImageFile(width, height, scaleFactor);
            else
                return CreateDesignGradientBitmap(width, height,
                                                  ref backgroundGrPath,
                                                  ref backgroundPthGrBrush);
        }

        //private void RenderCallback(int steps, bool initial = false)
        //{

        //}

        //private void DrawPatterns(Bitmap bitmap, List<Pattern> patterns, IRenderCaller caller)
        //{
        //    using (Graphics g = Graphics.FromImage(bitmap))
        //    {
        //        foreach (Pattern pattern in patterns)
        //        {
        //            pattern.DrawFilled(g, caller, computeRandom: false);
        //        }
        //    }
        //}

        private PointF ScalePoint(PointF point, PointF picCenter,
                          PointF newPicCenter, PointF pScale, bool scaleCenter = false)
        {
            if (scaleCenter)
                return new PointF(pScale.X * point.X, pScale.Y * point.Y);
            else
                return new PointF(
                    newPicCenter.X + pScale.X * (point.X - picCenter.X),
                    newPicCenter.Y + pScale.Y * (point.Y - picCenter.Y));
        }

        public void ScalePatterns(List<Pattern> patterns, Size prevSize, Size size,
                                  bool scalePenWidth = false, ScalePatternMode mode = ScalePatternMode.NewCenter)
        {
            PointF pScale = new PointF((float)size.Width / prevSize.Width,
                                       (float)size.Height / prevSize.Height);
            float scaleFactor = Math.Min(pScale.X, pScale.Y);
            PointF picCenter = new PointF(prevSize.Width / 2, prevSize.Height / 2);
            PointF newPicCenter = (mode == ScalePatternMode.NewCenter) ? new PointF(size.Width / 2, size.Height / 2) : picCenter;
            foreach (Pattern pattern in patterns)
            {
                if (mode != ScalePatternMode.KeepCenter)
                    pattern.Center = ScalePoint(pattern.Center, picCenter, newPicCenter, pScale, mode == ScalePatternMode.ScaleCenter);
                pattern.ZVector *= (double)scaleFactor;
                pattern.FillInfo.ApplyTransforms();
                Ribbon ribbon;
                PathPattern pathPattern = pattern as PathPattern;
                if (pathPattern != null)
                    ribbon = pathPattern.PathRibbon;
                else
                    ribbon = pattern as Ribbon;
                if (ribbon != null)
                {
                    ribbon.PatternZVector *= (double)scaleFactor;
                    ribbon.RibbonDistance *= (double)scaleFactor;
                    if (scalePenWidth)
                        ribbon.PenWidth = Math.Max(1F, scaleFactor * ribbon.PenWidth);
                    if (mode == ScalePatternMode.KeepCenter && ribbon.RibbonPath.Count > 1)
                    {
                        List<PointF> newPath = new List<PointF>() { ribbon.RibbonPath[0] };
                        for (int i = 1; i < ribbon.RibbonPath.Count; i++)
                        {
                            PointF p1 = ribbon.RibbonPath[i - 1];
                            PointF p2 = ribbon.RibbonPath[i];
                            var pVec = new PointF(p2.X - p1.X, p2.Y - p1.Y);
                            PointF pNew = newPath[i - 1];
                            newPath.Add(new PointF(pNew.X + pScale.X * pVec.X, pNew.Y + pScale.Y * pVec.Y));
                        }
                        ribbon.RibbonPath.Clear();
                        ribbon.RibbonPath.AddRange(newPath);
                    }
                    else
                    {
                        for (int i = 0; i < ribbon.RibbonPath.Count; i++)
                        {
                            ribbon.RibbonPath[i] = ScalePoint(ribbon.RibbonPath[i], picCenter, newPicCenter, pScale, mode == ScalePatternMode.ScaleCenter);
                        }
                    }
                }
                StringPattern stringPattern = pattern as StringPattern;
                if (stringPattern == null && ribbon != null)
                    stringPattern = ribbon.CopiedPattern as StringPattern;
                if (stringPattern != null)
                {
                    float fontSize = Math.Max(6F, (float)Math.Round(scaleFactor * stringPattern.Font.SizeInPoints));
                    stringPattern.Font = new Font(stringPattern.Font.FontFamily, fontSize, stringPattern.Font.Style, GraphicsUnit.Point);
                }
            }
        }

        public Bitmap RenderDesign(Size prevSize, Size size, bool scalePenWidth,
                                   bool draftMode, bool renderStained = true, IRenderCaller caller = null)
        {
            //if (caller == null)
            //    caller = RenderCallback;
            float scaleFactor = (float)size.Width / prevSize.Width;
            using (WhorlDesign designCopy = new WhorlDesign(this))  //Clone design
            {
                var patterns = new List<Pattern>(designCopy.designPatternsList);
                ScalePatterns(patterns, prevSize, size, scalePenWidth);
                GraphicsPath grPath = null;
                PathGradientBrush pthGrBrush = null;
                Bitmap bitmap = CreateDesignBitmap(size.Width, size.Height,
                                                   ref grPath, ref pthGrBrush, scaleFactor);
                DrawDesign.DrawDesignLayers(designCopy, bitmap, caller, scaleFactor, enableCache: false, 
                                            draftMode: draftMode);
                //DrawPatterns(bitmap, copiedPatterns);
                if (renderStained)
                    Pattern.RenderStained(designCopy.designPatternsList, bitmap,
                                          squareSize: draftMode ? 5 : 1);
                return bitmap;
            }
        }
        
        //public async Task<Bitmap> RenderRawDesignAsync(Size size, bool scalePenWidth,
        //                          bool draftMode, float textureScale = 0, bool computeRandom = false,
        //                          IEnumerable<Pattern> overridePatterns = null, bool renderStained = true, 
        //                          IRenderCaller caller = null)
        //{
        //    GraphicsPath grPath = null;
        //    PathGradientBrush pthGrBrush = null;
        //    Bitmap bitmap = CreateDesignBitmap(size.Width, size.Height,
        //                                       ref grPath, ref pthGrBrush);
        //    await Task.Run(() => DrawDesign.DrawDesignLayers(this, bitmap, caller, textureScale, computeRandom, 
        //                                overridePatterns, draftMode: draftMode));
        //    return bitmap;
        //}

        public async Task RenderDesignAsync(string fileName, Size prevSize, Size size, bool scalePenWidth,
                          bool draftMode, IRenderCaller caller, int qualitySize = 0, bool renderStained = true)
        {
            Size newSize;
            if (qualitySize > 0)
            {
                newSize = new Size(WhorlSettings.Instance.QualitySize,
                                   prevSize.Height * qualitySize / prevSize.Width);
            }
            else
            {
                newSize = size;
            }
            Bitmap bitmap = await Task.Run(() => RenderDesign(prevSize, newSize, scalePenWidth,
                                                        draftMode, renderStained, caller));
            if (caller != null && caller.CancelRender)
                return;
            if (qualitySize > 0)
            {
                bitmap = (Bitmap)BitmapTools.ScaleImage(bitmap, size);
            }
            using (bitmap)
            {
                Tools.SavePngOrJpegImageFile(fileName, bitmap);
            }
        }

        private void ReplaceBackgroundColor(bool replaceNew, object prevColor, object newColor)
        {
            backgroundColor = (Color)(replaceNew ? prevColor : newColor);
        }

        //private void AddUndoIndexChangedHandler(UndoOperations.UndoInfoFn handlerFn)
        //{
        //    UndoOps.UndoInfoChanged += handlerFn;
        //}

        private void PerformUndoRedo(bool undo, UndoOperation.OperationTypes opType, UndoOperation.BaseFrame opFrame)
        {
            bool invalid = false;
            Pattern prevPattern = null;
            List<Pattern> patternList = opFrame.ParentObject as List<Pattern>;
            if (patternList != null)
            {
                switch (opType)
                {
                    case UndoOperation.OperationTypes.Create:
                        if (undo)
                            patternList.Remove((Pattern)opFrame.NewObject);
                        else
                        {
                            if (opFrame.ListIndex == -1)
                                patternList.Add((Pattern)opFrame.NewObject);
                            else
                                patternList.Insert(opFrame.ListIndex, (Pattern)opFrame.NewObject);
                        }
                        break;
                    case UndoOperation.OperationTypes.Delete:
                        prevPattern = (Pattern)opFrame.PreviousObject;
                        if (undo)
                            patternList.Insert(opFrame.ListIndex, prevPattern);
                        else
                            patternList.Remove(prevPattern);
                        break;
                    case UndoOperation.OperationTypes.Replace:
                        if (opFrame.NewObject is PointF)
                        {   //Move centers
                            PointF delta = (PointF)opFrame.NewObject;
                            if (undo)
                                delta = new PointF(-delta.X, -delta.Y);
                            foreach (Pattern pattern in patternList)
                            {
                                pattern.Center = new PointF(pattern.Center.X + delta.X, pattern.Center.Y + delta.Y);
                            }
                        }
                        else
                        {
                            Pattern ptn = undo ? (Pattern)opFrame.PreviousObject : (Pattern)opFrame.NewObject;
                            List<Pattern> ptnList = patternList;
                            if (ptn.Recursion.IsRecursivePattern)
                            {
                                ptnList = ptn.Recursion.ParentPattern.Recursion.RecursionPatterns;
                            }
                            else if (undo)
                            {
                                prevPattern = ptn;
                            }
                            if (undo)
                            {
                                ptnList[opFrame.ListIndex] = ptn;
                            }
                            else
                                ptnList[opFrame.ListIndex] = ptn;
                        }
                        break;
                    case UndoOperation.OperationTypes.Shift:
                        var pattern1 = (Pattern)opFrame.PreviousObject;
                        patternList.Remove(pattern1);
                        if (undo)
                            patternList.Insert(opFrame.PrevListIndex, pattern1);
                        else
                            patternList.Insert(opFrame.ListIndex, pattern1);
                        break;
                }
            }
            else if (opType == UndoOperation.OperationTypes.Replace)
            {
                if (opFrame.NewObject is GradientColors && opFrame.ParentObject is WhorlDesign)
                {
                    ((WhorlDesign)opFrame.ParentObject).SetBackgroundGradientColorsNoUndo(
                        undo ? (GradientColors)opFrame.PreviousObject : (GradientColors)opFrame.NewObject);
                }
                else if (opFrame.NewObject is ColorNodeList && opFrame.PreviousObject is ColorNodeList)
                {
                    var colorNodesParent = opFrame.ParentObject as IColorNodeList;
                    if (colorNodesParent != null)
                    {
                        colorNodesParent.ColorNodes = (ColorNodeList)(undo ? opFrame.PreviousObject : opFrame.NewObject);
                    }
                    else
                        invalid = true;
                }
                else
                    invalid = true;
            }
            else if (opType == UndoOperation.OperationTypes.Zoom)
            {
                var targetDesign = opFrame.ParentObject as WhorlDesign;
                if (targetDesign == null || !(opFrame.NewObject is PointF))
                    invalid = true;
                else
                {
                    PointF scaleFactors = (PointF)opFrame.NewObject;
                    if (undo)
                        scaleFactors = new PointF(1F / scaleFactors.X, 1F / scaleFactors.Y);
                    targetDesign.ScaleDesignNoUndo(scaleFactors);
                }
            }
            else
                invalid = true;
            if (invalid)
                throw new Exception("Invalid parameters for undo/redo.");
            if (prevPattern != null)
            {
                DesignLayer designLayer = opFrame.Tag as DesignLayer;
                if (designLayer != null && DesignLayerList.DesignLayers.Contains(designLayer))
                {
                    prevPattern.DesignLayer = designLayer;
                }
            }
        }

        private UndoOperation CreateUndoOperation(UndoOperation.OperationTypes opType)
        {
            UndoOperation op = new UndoOperation(opType, PerformUndoRedo);
            UndoOps.AddOperation(op);
            IsDirty = true;
            return op;
        }

        private void DoPatternOperation(UndoOperation op, Pattern newPattern, 
                                        Pattern previousPattern = null, int listIndex = -1)
        {
            try
            {
                Initializing = true;
                int prevListIndex = -1;
                object tag = null;
                if (previousPattern == null)
                {
                    if (listIndex == -1)
                        designPatternsList.Add(newPattern);
                    else
                        designPatternsList.Insert(listIndex, newPattern);
                }
                else
                {
                    switch (op.OperationType)
                    {
                        case UndoOperation.OperationTypes.Shift:
                            prevListIndex = designPatternsList.IndexOf(previousPattern);
                            designPatternsList.Remove(previousPattern);
                            designPatternsList.Insert(listIndex, previousPattern);
                            break;
                        case UndoOperation.OperationTypes.Replace:
                        case UndoOperation.OperationTypes.Delete:
                            listIndex = designPatternsList.IndexOf(previousPattern);
                            if (listIndex == -1)
                            {
                                if (op.OperationType != UndoOperation.OperationTypes.Replace)
                                    return;
                                else if (previousPattern.Recursion.IsRecursivePattern)
                                {
                                    var recursionPatterns = 
                                        previousPattern.Recursion.ParentPattern.Recursion.RecursionPatterns;
                                    listIndex = recursionPatterns.IndexOf(previousPattern);
                                    if (listIndex == -1)
                                        throw new Exception("Did not find recursion pattern in parent list.");
                                    recursionPatterns[listIndex] = newPattern;
                                }
                                else
                                    throw new Exception("Cannot replace pattern not in the design's list.");
                            }
                            else
                            {
                                DesignLayer designLayer = previousPattern.DesignLayer;
                                tag = designLayer;
                                previousPattern.DesignLayer = null;
                                if (op.OperationType == UndoOperation.OperationTypes.Replace)
                                {
                                    newPattern.DesignLayer = designLayer;
                                    designPatternsList[listIndex] = newPattern;
                                }
                                else
                                    designPatternsList.Remove(previousPattern);
                            }
                            previousPattern.SetLargePropertiesToNull();
                            break;
                    }
                }
                op.AddOperationFrame(designPatternsList, newPattern, previousPattern,
                                     listIndex, prevListIndex, tag);
            }
            finally
            {
                Initializing = false;
            }
        }

        public void AddPattern(Pattern pattern, int insertIndex = -1)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Create);
            DoPatternOperation(op, pattern, listIndex: insertIndex);
        }

        public void AddPatterns(IEnumerable<Pattern> patterns, int insertIndex = -1)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Create);
            foreach (Pattern pattern in patterns)
            {
                DoPatternOperation(op, pattern, listIndex: insertIndex);
                if (insertIndex != -1)
                    insertIndex++;
            }
        }

        //public void RemovePattern(Pattern pattern)
        //{
        //    UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Delete);
        //    DoPatternOperation(op, null, pattern);
        //}

        public void RemovePatterns(List<Pattern> patterns)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Delete);
            for (int i = patterns.Count - 1; i >= 0; i--)
            {
                DoPatternOperation(op, null, patterns[i]);
            }
        }

        public void ReplacePattern(Pattern origPattern, Pattern newPattern)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Replace);
            DoPatternOperation(op, newPattern, origPattern);
        }

        public void ReplacePatterns(List<Pattern> origPatterns, List<Pattern> newPatterns)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Replace);
            for (int i = 0; i < origPatterns.Count; i++)
            {
                DoPatternOperation(op, newPatterns[i], origPatterns[i]);
            }
        }

        public void ReplaceColorNodes(IColorNodeList colorNodesParent, ColorNodeList colorNodeList)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Replace);
            op.AddOperationFrame(colorNodesParent, colorNodeList, colorNodesParent.ColorNodes.GetCopy());
            colorNodesParent.ColorNodes = colorNodeList;
        }

        public void AddMovePatternsOperation(PointF delta, List<Pattern> selPatterns)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Replace);
            op.AddOperationFrame(selPatterns, delta);
        }

        public void AddMovePatternsOperation(PointF delta)
        {
            AddMovePatternsOperation(delta, designPatternsList.FindAll(x => x.Selected));
        }

        public void AddChangeZOrderOperation(Pattern pattern, int newListIndex)
        {
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Shift);
            DoPatternOperation(op, null, pattern, newListIndex);
        }

        public void ScaleDesignNoUndo(PointF scaleFactors)
        {
            Size prevSize = pictureBox.ClientRectangle.Size;
            Size newSize = new Size((int)(prevSize.Width * scaleFactors.X),
                                    (int)(prevSize.Height * scaleFactors.Y));
            ScalePatterns(designPatternsList, prevSize, newSize, scalePenWidth: true);
        }

        public void ScaleDesign(PointF scaleFactors)
        {
            ScaleDesignNoUndo(scaleFactors);
            UndoOperation op = CreateUndoOperation(UndoOperation.OperationTypes.Zoom);
            op.AddOperationFrame(parentObj: this, newObj: scaleFactors);
        }

        public bool SetScaleToFit(bool scaleToFit)
        {
            bool redraw = false;
            if (scaleToFit != ScaleToFit || readingFromXml)
            {
                ScaleToFit = scaleToFit;
                if (ScaleToFit)
                    redraw = ScaleDesignToFit(MainForm.ContainerSize);
                //else 
                //{
                //    var newSize = new Size(Math.Min(PictureBoxSize.Width, MainForm.ContainerSize.Width),
                //                           Math.Min(PictureBoxSize.Height, MainForm.ContainerSize.Height));
                //    if (newSize != pictureBox.ClientSize)
                //    {
                //        pictureBox.ClientSize = newSize;
                //        redraw = true;
                //    }
                //}
            }
            return redraw;
        }

        public static Size FitSizeToContainer(Size containerSize, Size size)
        {
            SizeF scaleFacs = new SizeF((float)containerSize.Width / size.Width,
                                        (float)containerSize.Height / size.Height);
            Size newSize;
            if (scaleFacs.Width <= scaleFacs.Height)
            {
                newSize = new Size(containerSize.Width, size.Height * containerSize.Width / size.Width);
            }
            else
            {
                newSize = new Size(size.Width * containerSize.Height / size.Height, containerSize.Height);
            }
            return newSize;
        }

        public bool ScaleDesignToFit(Size containerSize)
        {
            Size newSize = FitSizeToContainer(containerSize, PictureBoxSize);
            bool scaled = newSize != PictureBoxSize;
            if (scaled)
            {
                ScalePatterns(designPatternsList, PictureBoxSize, newSize, mode: ScalePatternMode.NewCenter);
                PictureBoxSize = newSize;
                pictureBox.Dock = DockStyle.None;
                pictureBox.Size = PictureBoxSize;
                IsDirty = true;
            }
            return scaled;
        }

        public bool Undo()
        {
            return UndoOps.UndoOperation();
        }

        public bool Redo()
        {
            return UndoOps.RedoOperation();
        }

        public void ClearDesign()
        {
            try
            {
                Initializing = true;
                this.RemovePatterns(this.designPatternsList);
                this.BackgroundImageFileName = null;
                this.StartupAnimationNames = string.Empty;
                this.DesignLayerList.ClearDesignLayers();
                this.DefaultDesignLayer = null;
                this.ImproviseConfig = null;
            }
            finally
            {
                Initializing = false;
            }
        }

        //public override object Clone()
        //{
        //    return new WhorlDesign(this);
        //    //try
        //    //{
        //    //    this.Initializing = true;
        //    //    WhorlDesign copy = new WhorlDesign();
        //    //    copy.BackgroundGradientColors = 
        //    //        (GradientColors)this.BackgroundGradientColors.Clone();
        //    //    copy.BackgroundImageFileName = this.BackgroundImageFileName;
        //    //    if (this.DefaultPatternGroup != null)
        //    //        copy.DefaultPatternGroup = (PatternList)this.DefaultPatternGroup.Clone();
        //    //    copy.designPatternsList.AddRange(designPatternsList.Select(
        //    //                                 ptn => (Pattern)ptn.Clone()));
        //    //    copy.DesignLayerList = new DesignLayerList(copy, this);
        //    //    copy.SelectedPatternIndex = this.SelectedPatternIndex;
        //    //    return copy;
        //    //}
        //    //finally
        //    //{
        //    //    this.Initializing = false;
        //    //}
        //}

        public void SetPreviousSize()
        {
            this.PreviousPictureSize = pictureBox.ClientRectangle.Size;
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "Design";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            Tools.SetXmlVersion(node, xmlTools);
            if (!string.IsNullOrEmpty(this.StartupAnimationNames))
                xmlTools.AppendXmlAttribute(node, "StartupAnimationNames", StartupAnimationNames);
            xmlTools.AppendXmlAttribute(node, "PictureWidth", PreviousPictureSize.Width);
            xmlTools.AppendXmlAttribute(node, "PictureHeight", PreviousPictureSize.Height);
            xmlTools.AppendXmlAttribute(node, "BackgroundImageMode", BackgroundImageMode);
            xmlTools.AppendXmlAttribute(node, "ScaleToFit", ScaleToFit);
            XmlNode childNode;
            if (!string.IsNullOrEmpty(this.StartupAnimationNames))
            {
                childNode = xmlTools.CreateXmlNode("Settings");
                foreach (string propName in savedPropertyNames)
                {
                    object propValue = WhorlSettings.Instance[propName];
                    xmlTools.AppendXmlAttribute(childNode, propName, propValue);
                }
                node.AppendChild(childNode);
                childNode = xmlTools.CreateXmlNode("CheckedMenuItems");
                foreach (ToolStripMenuItem itm in SavedMenuItems)
                {
                    xmlTools.AppendXmlAttribute(childNode, itm.Name, itm.Checked);
                }
                node.AppendChild(childNode);
            }
            Size picSize;
            if (XmlPictureBoxSize != Size.Empty)
            {
                picSize = XmlPictureBoxSize;
                XmlPictureBoxSize = Size.Empty;
            }
            else
                picSize = pictureBox.Size;
            node.AppendChild(xmlTools.CreateXmlNode(nameof(PictureBoxSize), picSize));
            BackgroundGradientColors.ToXml(node, xmlTools, nameof(BackgroundGradientColors));
            if (!string.IsNullOrEmpty(BackgroundImageFileName))
                xmlTools.AppendChildNode(node, nameof(BackgroundImageFileName), BackgroundImageFileName);
            if (this.DefaultPatternGroup != null)
                DefaultPatternGroup.ToXml(node, xmlTools, "DefaultPatternGroup");
            childNode = xmlTools.CreateXmlNode("DesignPatterns");
            foreach (Pattern ptn in designPatternsList)
            {
                ptn.ToXml(childNode, xmlTools);
            }
            node.AppendChild(childNode);
            if (DesignLayerList.DesignLayers.Any())
            {
                DesignLayerList.BeforeSaveToXml(designPatternsList);
                DesignLayerList.ToXml(node, xmlTools);
            }
            if (ImproviseConfig != null)
            {
                ImproviseConfig.ToXml(node, xmlTools);
            }
            foreach (var namedRandomSeed in AnimationSeeds.Values)
            {
                namedRandomSeed.ToXml(node, xmlTools);
            }
            //if (InfluencePointInfoList.Count != 0)
            //{
            //    InfluencePointInfoList.ToXml(node, xmlTools);
            //}
            return xmlTools.AppendToParent(parentNode, node);
        }

        public override void FromXml(XmlNode node)
        {
            this.Initializing = readingFromXml = true;
            this.ImproviseConfig = null;
            try
            {
                //InfluencePointInfoList.Clear();
                PictureBoxSize = Size.Empty;
                BackgroundImageFileName = null;
                ReadWarnings.Clear();
                this.XmlVersion = Tools.GetXmlVersion(node);
                this.StartupAnimationNames = (string)Tools.GetXmlAttribute(
                    "StartupAnimationNames", typeof(string), node, required: false);
                int picWidth = (int)Tools.GetXmlAttribute("PictureWidth", typeof(int), node);
                int picHeight = (int)Tools.GetXmlAttribute("PictureHeight", typeof(int), node);
                this.PreviousPictureSize = new Size((int)picWidth, (int)picHeight);
                this.BackgroundImageMode = Tools.GetEnumXmlAttr(node,
                                           "BackgroundImageMode", TextureImageModes.Stretch);
                bool scaleToFit = Tools.GetXmlAttribute(node, false, "ScaleToFit");
                this.DefaultDesignLayer = null;
                this.DesignLayerList.ClearDesignLayers();
                this.designPatternsList.Clear();
                this.AnimationSeeds.Clear();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case nameof(PictureBoxSize):
                            StaticPictureBoxSize = PictureBoxSize = Tools.GetSizeFromXml(childNode);
                            break;
                        case "BackgroundGradientColors":
                            BackgroundGradientColors.FromXml(childNode);
                            break;
                        case nameof(BackgroundImageFileName):
                            BackgroundImageFileName = Tools.GetXmlNodeValue(childNode);
                            break;
                        case "DefaultPatternGroup":
                            this.DefaultPatternGroup = new PatternList(this);
                            this.DefaultPatternGroup.FromXml(childNode);
                            break;
                        case "DesignPatterns":
                            foreach (XmlNode patternNode in childNode.ChildNodes)
                                AddPatternFromXml(patternNode);
                            break;
                        case "DesignLayerList":
                            this.DesignLayerList.FromXml(childNode);
                            this.DesignLayerList.AfterReadFromXml(designPatternsList);
                            break;
                        case "BackColor":
                            //Legacy code:
                            //BackgroundColor = Tools.GetColorFromXml(childNode);
                            break;
                        case "DefaultPattern":
                            //Legacy code:
                            break;
                        case "Settings":
                            foreach (XmlAttribute attr in childNode.Attributes)
                            {
                                var propertyInfo = WhorlSettings.Instance.GetProperty(attr.Name);
                                if (propertyInfo != null)
                                {
                                    WhorlSettings.Instance[attr.Name] =
                                        Tools.ConvertXmlAttribute(attr, propertyInfo.PropertyType);
                                }
                                //System.Configuration.SettingsProperty prp =
                                //    WhorlSettings.Instance.Properties[attr.Name];
                                //if (prp != null)
                                //{
                                //    WhorlSettings.Instance[attr.Name] =
                                //        Tools.ConvertXmlAttribute(attr, prp.PropertyType);
                                //}
                            }
                            break;
                        case "CheckedMenuItems":
                            foreach (XmlAttribute attr in childNode.Attributes)
                            {
                                ToolStripMenuItem itm = SavedMenuItems.Find(
                                                        x => x.Name == attr.Name);
                                if (itm != null)
                                {
                                    itm.Checked = bool.Parse(attr.Value);
                                }
                            }
                            break;
                        case "ImproviseConfig":
                            ImproviseConfig = new ImproviseConfig();
                            ImproviseConfig.FromXml(childNode);
                            break;
                        case nameof(NamedRandomSeed):
                            var namedSeed = new NamedRandomSeed();
                            namedSeed.FromXml(childNode);
                            if (!AnimationSeeds.ContainsKey(namedSeed.Name))
                                AnimationSeeds.Add(namedSeed.Name, namedSeed);
                            break;
                        case "InfluencePointInfoList":
                            //Legacy code.
                            //InfluencePointInfoList.FromXml(childNode);
                            break;
                        default:
                            //Legacy code:
                            AddPatternFromXml(childNode);
                            break;
                    }  //switch (childNode.Name)
                }  //foreach (childNode)
                if (PictureBoxSize == Size.Empty)
                    PictureBoxSize = PreviousPictureSize;
                SetScaleToFit(scaleToFit);
                //AfterFromXml();
            }
            finally
            {
                this.Initializing = readingFromXml = false;
            }
        }

        //private void AfterFromXml()
        //{
        //    foreach (Pattern pattern in designPatternsList)
        //    {
        //        foreach (PatternTransform transform in pattern.Transforms)
        //        {
        //            if (transform.InfluenceLinkParentCollection != null)
        //            {
        //                transform.InfluenceLinkParentCollection.ResolveReferences();
        //            }
        //        }
        //    }
        //}

        //public void CreateBackgroundPattern()
        //{
        //    Pattern prevBackgroundPattern = BackgroundPattern;
        //    FillInfo.FillTypes fillType;
        //    if (string.IsNullOrEmpty(BackgroundImageFileName))
        //        fillType = FillInfo.FillTypes.Path;
        //    else
        //        fillType = FillInfo.FillTypes.Texture;
        //    BackgroundPattern = new Pattern(fillType);
        //    PointF picCenter = new PointF(PictureBoxSize.Width / 2, PictureBoxSize.Height / 2);
        //    BackgroundPattern.Center = picCenter;
        //    float maxDimension = Math.Max(picCenter.X, picCenter.Y);
        //    float minDimension = Math.Min(picCenter.X, picCenter.Y);
        //    switch (fillType)
        //    {
        //        case FillInfo.FillTypes.Path:
        //            var ellipseOutline = new BasicOutline(BasicOutlineTypes.Ellipse);
        //            float scale = (float)Math.Sqrt(maxDimension * maxDimension + minDimension * minDimension) / minDimension;
        //            maxDimension = 10F + scale * maxDimension;
        //            minDimension = 10F + scale * minDimension;
        //            double m = 2D * Math.Max(1.01, maxDimension / minDimension);
        //            double b = 4 - 2 * m;
        //            double addDenom = (-b - Math.Sqrt(b * b - 8 * (2 - m))) / (4 - 2 * m);
        //            ellipseOutline.AddDenom = addDenom;
        //            double rMax = 1 / (1D - 1 / (1 + addDenom));
        //            double rMin = 1 / (1D + 1 / (1 + addDenom));
        //            double ellipseSize = (rMax + rMin) / rMax;
        //            double modulus = 2 * maxDimension / ellipseSize;
        //            double offset = 0.5 * maxDimension * (ellipseSize - 2 * rMin / rMax);
        //            ellipseOutline.AmplitudeFactor = 1D;
        //            ellipseOutline.Frequency = 1D;
        //            BackgroundPattern.BasicOutlines.Add(ellipseOutline);
        //            if (PictureBoxSize.Width >= PictureBoxSize.Height)
        //            {
        //                BackgroundPattern.ZVector = new ParserEngine.Complex(modulus, 0);
        //                BackgroundPattern.Center = new PointF(picCenter.X - (int)Math.Round(offset), picCenter.Y);
        //            }
        //            else
        //            {
        //                BackgroundPattern.ZVector = new ParserEngine.Complex(0, modulus);
        //                BackgroundPattern.Center = new PointF(picCenter.X, picCenter.Y - (int)Math.Round(offset));
        //            }
        //            if (prevBackgroundPattern == null)
        //            {
        //                PathFillInfo pathFillInfo = BackgroundPattern.FillInfo as PathFillInfo;
        //                if (pathFillInfo != null)
        //                {
        //                    pathFillInfo.BoundaryColor = BackgroundGradientColors.BoundaryColor;
        //                    pathFillInfo.CenterColor = BackgroundGradientColors.CenterColor;
        //                }
        //            }
        //            BackgroundPattern.SetCenterOffset(picCenter);
        //            break;
        //        case FillInfo.FillTypes.Texture:
        //            var rectOutline = new BasicOutline(BasicOutlineTypes.Rectangle);
        //            rectOutline.AddDenom = (double)PictureBoxSize.Height / PictureBoxSize.Width;
        //            rectOutline.AmplitudeFactor = 1D;
        //            rectOutline.Frequency = 1D;
        //            BackgroundPattern.RotationSteps = 5000;
        //            BackgroundPattern.BasicOutlines.Add(rectOutline);
        //            BackgroundPattern.ZVector = new ParserEngine.Complex(Math.Sqrt(picCenter.X * picCenter.X + picCenter.Y * picCenter.Y), 0);

        //            if (prevBackgroundPattern == null)
        //            {
        //                TextureFillInfo textureFillInfo = BackgroundPattern.FillInfo as TextureFillInfo;
        //                if (textureFillInfo != null)
        //                {
        //                    textureFillInfo.TextureImageFileName = BackgroundImageFileName;
        //                    textureFillInfo.ImageMode = BackgroundImageMode;
        //                }
        //            }
        //            break;
        //    }
        //    if (prevBackgroundPattern != null)
        //    {
        //        BackgroundPattern.CopyFillInfo(prevBackgroundPattern);
        //        prevBackgroundPattern.Dispose();
        //    }
        //}

        private void AddPatternFromXml(XmlNode patternNode)
        {
            Pattern pattern = Pattern.CreatePatternFromXml(this, patternNode);
            if (pattern == null)
                throw new Exception("Invalid pattern node in XML.");
            this.designPatternsList.Add(pattern);
        }

        public void ReadDesignFromXmlFile(string fileName, bool showWarnings = false)
        {
            Tools.ReadFromXml(fileName, this, "Design");
            IsDirty = false;
            if (showWarnings && ReadWarnings.Count > 0)
            {
                MessageBox.Show($"Warnings reading design file:{Environment.NewLine}" +
                                string.Join(Environment.NewLine, ReadWarnings));
            }
        }

        public static void WriteDesignListXml(string fileName, List<WhorlDesign> designList)
        {
            var xmlTools = new XmlTools();
            xmlTools.CreateDocument();
            var topNode = xmlTools.CreateXmlNode("Designs");
            foreach (WhorlDesign dgn in designList)
            {
                dgn.ToXml(topNode, xmlTools);
            }
            xmlTools.SaveXml(fileName);
        }

        public static List<WhorlDesign> ReadDesignListFromXmlFile(string fileName, Size containerSize)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            if (xmlDoc.FirstChild.Name != "Designs")
                throw new Exception("Invalid file format for a list of designs.");
            List<WhorlDesign> designList = new List<WhorlDesign>();
            foreach (XmlNode childNode in xmlDoc.FirstChild.ChildNodes)
            {
                WhorlDesign design = new WhorlDesign();
                design.FromXml(childNode);
                designList.Add(design);
            }
            return designList;
        }

        //    /// <summary>
        //    /// Adjust locations of displayed objects.
        //    /// </summary>
        //    public void FitDesignInPictureBox()
        //    {
        //        float widthFac = (float)pictureBox.ClientRectangle.Width / (float)PreviousPictureSize.Width;
        //        float heightFac = (float)pictureBox.ClientRectangle.Height / (float)PreviousPictureSize.Height;
        //        float scaleFac = Math.Min(widthFac, heightFac);
        //        foreach (Pattern pattern in this.DrawnPatterns)
        //        {
        //            pattern.Center = new PointF(pattern.Center.X * widthFac, pattern.Center.Y * heightFac);
        //            pattern.ZVector = pattern.ZVector * scaleFac;
        //            if (pattern is Ribbon)
        //            {
        //                Ribbon ribbon = (Ribbon)pattern;
        //                for (int i = 0; i < ribbon.RibbonPath.Count; i++)
        //                {
        //                    PointF p = ribbon.RibbonPath[i];
        //                    ribbon.RibbonPath[i] = new PointF(p.X * widthFac, p.Y * heightFac);
        //                }
        //            }
        //        }
        //        PreviousPictureSize = pictureBox.ClientRectangle.Size;
        //    }

        public override void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            foreach (Pattern pattern in designPatternsList)
            {
                pattern.Dispose();
            }
            designPatternsList.Clear();
            UndoOps.Dispose();
        }
    }
}
