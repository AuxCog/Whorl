using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ParserEngine;

namespace Whorl
{
    public class WhorlSettings : ChangeTracker
    {
        public const string TexturesFolder = "Textures";
        public const string AllTexturesFolder = "AllTextures";
        public const string IncludeFilesFolder = "IncludeFiles";

        public static WhorlSettings Instance { get; } = new WhorlSettings();

        private WhorlSettings()
        {
        }

        public PropertyInfo GetProperty(string name)
        {
            return this.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        public object this[string propertyName]
        {
            get
            {
                object val;
                var propertyInfo = GetProperty(propertyName);
                if (propertyInfo == null)
                    val = null;
                else
                    val = propertyInfo.GetValue(this);
                return val;
            }
            set
            {
                var propertyInfo = GetProperty(propertyName);
                if (propertyInfo == null)
                    throw new Exception($"Setting property {propertyName} does not exist.");
                object val = value;
                if (val != null && val.GetType() != propertyInfo.PropertyType)
                    val = Convert.ChangeType(val, propertyInfo.PropertyType);
                propertyInfo.SetValue(this, val);
            }
        }

        public void Save(bool ifChanged = true)
        {
            if (IsChanged || !ifChanged)
            {
                SettingsXML.SaveSettings(this);
            }
        }

        public void AfterSaveOrRead()
        {
            IsChanged = false;
        }

        public HashSet<string> TextureFileNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private Int32 _animationRate;
        [InitialSettingValue("1")]
        public Int32 AnimationRate
        {
            get { return _animationRate; }
            set { SetProperty(ref _animationRate, value); }
        }

        private Int32 _bufferSize;
        [InitialSettingValue("40")]
        public Int32 BufferSize
        {
            get { return _bufferSize; }
            set { SetProperty(ref _bufferSize, value); }
        }

        private bool _chooseCopyPasteObjects;
        [InitialSettingValue("True")]
        public bool ChooseCopyPasteObjects
        {
            get => _chooseCopyPasteObjects;
            set => SetProperty(ref _chooseCopyPasteObjects, value);
        }

        [InitialSettingValue("CustomFolders")]
        [ReadOnly]
        public string CustomDesignParentFolder { get; private set; }

        [ReadOnly]
        [InitialSettingValue("DefaultPattern.config")]
        public String DefaultPatternFileName { get; private set; }

        [ReadOnly]
        [InitialSettingValue("809")]
        public Int32 DefaultPicHeight { get; private set; }

        [ReadOnly]
        [InitialSettingValue("1596")]
        public Int32 DefaultPicWidth { get; private set; }

        [ReadOnly]
        [InitialSettingValue("DesignPatterns.config")]
        public String DesignPatternsFileName { get; private set; }

        private Int32 _designThumbnailHeight;
        [InitialSettingValue("150")]
        public Int32 DesignThumbnailHeight
        {
            get { return _designThumbnailHeight; }
            set { SetProperty(ref _designThumbnailHeight, value); }
        }

        [ReadOnly]
        [InitialSettingValue("DesignThumbnails")]
        public String DesignThumbnailsFolder { get; private set; }

        private Int32 _draftSize;
        [InitialSettingValue("4")]
        public Int32 DraftSize
        {
            get { return _draftSize; }
            set { SetProperty(ref _draftSize, value); }
        }

        private Boolean _exactOutline;
        [InitialSettingValue("False")]
        public Boolean ExactOutline
        {
            get { return _exactOutline; }
            set { SetProperty(ref _exactOutline, value); }
        }

        private String _filesFolder;
        [InitialSettingValue(@"C:\WhorlFiles")]
        public String FilesFolder
        {
            get { return _filesFolder; }
            set { SetProperty(ref _filesFolder, value); }
        }

        private Double _improvDamping;
        [InitialSettingValue("1")]
        public Double ImprovDamping
        {
            get { return _improvDamping; }
            set { SetProperty(ref _improvDamping, value); }
        }

        private Double _improvisationLevel;
        [InitialSettingValue("0.2")]
        public Double ImprovisationLevel
        {
            get { return _improvisationLevel; }
            set { SetProperty(ref _improvisationLevel, value); }
        }

        private Boolean _improviseBackground;
        [InitialSettingValue("False")]
        public Boolean ImproviseBackground
        {
            get { return _improviseBackground; }
            set { SetProperty(ref _improviseBackground, value); }
        }

        private Boolean _improviseColors;
        [InitialSettingValue("True")]
        public Boolean ImproviseColors
        {
            get { return _improviseColors; }
            set { SetProperty(ref _improviseColors, value); }
        }

        private Boolean _improviseOnOutlineType;
        [InitialSettingValue("False")]
        public Boolean ImproviseOnOutlineType
        {
            get { return _improviseOnOutlineType; }
            set { SetProperty(ref _improviseOnOutlineType, value); }
        }

        private Boolean _improviseParameters;
        [InitialSettingValue("False")]
        public Boolean ImproviseParameters
        {
            get { return _improviseParameters; }
            set { SetProperty(ref _improviseParameters, value); }
        }

        private Boolean _improvisePetals;
        [InitialSettingValue("False")]
        public Boolean ImprovisePetals
        {
            get { return _improvisePetals; }
            set { SetProperty(ref _improvisePetals, value); }
        }

        private Boolean _improviseShapes;
        [InitialSettingValue("False")]
        public Boolean ImproviseShapes
        {
            get { return _improviseShapes; }
            set { SetProperty(ref _improviseShapes, value); }
        }

        private Boolean _improviseTextures;
        [InitialSettingValue("True")]
        public Boolean ImproviseTextures
        {
            get { return _improviseTextures; }
            set { SetProperty(ref _improviseTextures, value); }
        }

        private Int32 _maxLoopCount;
        [InitialSettingValue("1000")]
        public Int32 MaxLoopCount
        {
            get { return _maxLoopCount; }
            set { SetProperty(ref _maxLoopCount, value); }
        }

        private Int32 _maxSavedImprovs;
        [InitialSettingValue("100")]
        public Int32 MaxSavedImprovs
        {
            get { return _maxSavedImprovs; }
            set { SetProperty(ref _maxSavedImprovs, value); }
        }

        private Double _minPatternSize;
        [InitialSettingValue("1000")]
        public Double MinPatternSize
        {
            get { return _minPatternSize; }
            set { SetProperty(ref _minPatternSize, value); }
        }

        private Boolean _optimizeExpressions;
        [InitialSettingValue("True")]
        public Boolean OptimizeExpressions
        {
            get { return _optimizeExpressions; }
            set { SetProperty(ref _optimizeExpressions, value); }
        }

        [ReadOnly]
        [InitialSettingValue("PatternChoices.config")]
        public String PatternChoicesFileName { get; private set; }

        private Int32 _qualitySize;
        [InitialSettingValue("3000")]
        public Int32 QualitySize
        {
            get { return _qualitySize; }
            set { SetProperty(ref _qualitySize, value); }
        }

        private Double _recomputeInterval;
        [InitialSettingValue("5")]
        public Double RecomputeInterval
        {
            get { return _recomputeInterval; }
            set { SetProperty(ref _recomputeInterval, value); }
        }

        private String _renderFilesFolder;
        [InitialSettingValue(@"C:\GreetingCards\Final")]
        public String RenderFilesFolder
        {
            get { return _renderFilesFolder; }
            set { SetProperty(ref _renderFilesFolder, value); }
        }

        private Int32 _renderWidth;
        [InitialSettingValue("2000")]
        public Int32 RenderWidth
        {
            get { return _renderWidth; }
            set { SetProperty(ref _renderWidth, value); }
        }

        private Double _replayIntervalSeconds;
        [InitialSettingValue("1")]
        public Double ReplayIntervalSeconds
        {
            get { return _replayIntervalSeconds; }
            set { SetProperty(ref _replayIntervalSeconds, value); }
        }

        private Double _revolveRate;
        [InitialSettingValue("0.1")]
        public Double RevolveRate
        {
            get { return _revolveRate; }
            set { SetProperty(ref _revolveRate, value); }
        }

        private Boolean _saveDesignThumbnails;
        [InitialSettingValue("True")]
        public Boolean SaveDesignThumbnails
        {
            get { return _saveDesignThumbnails; }
            set { SetProperty(ref _saveDesignThumbnails, value); }
        }

        private String _savedImageSizes;
        [InitialSettingValue(@"1090x1090:Square,1526x1090:7 x 5,1411x1090:11 x 8.5,1362x1090:10 x 8,1387x1090:11 x 14,727x1090:4 x 6,778x1090:5 x 7,856x1090:14 x 11,842x1090:8.5 x11")]
        public String SavedImageSizes
        {
            get { return _savedImageSizes; }
            set { SetProperty(ref _savedImageSizes, value); }
        }


        private String _imageSizes;
        /// <summary>
        /// Comma separated list of AspectRatio:Description
        /// </summary>
        public string ImageSizes
        {
            get { return _imageSizes; }
            set { SetProperty(ref _imageSizes, value); }
        }

        private bool _parallelProcessing;
        public bool ParallelProcessing
        {
            get { return _parallelProcessing; }
            set { SetProperty(ref _parallelProcessing, value); }
        }

        private Int32 _smoothAnimationSteps;
        [InitialSettingValue("10")]
        public Int32 SmoothAnimationSteps
        {
            get { return _smoothAnimationSteps; }
            set { SetProperty(ref _smoothAnimationSteps, value); }
        }

        private Double _spinRate;
        [InitialSettingValue("1")]
        public Double SpinRate
        {
            get { return _spinRate; }
            set { SetProperty(ref _spinRate, value); }
        }

        private Boolean _traceMode;
        [InitialSettingValue("True")]
        public Boolean TraceMode
        {
            get { return _traceMode; }
            set { SetProperty(ref _traceMode, value); }
        }

        private bool _useNewPolygonVersion;
        public bool UseNewPolygonVersion
        {
            get { return _useNewPolygonVersion; }
            set { SetProperty(ref _useNewPolygonVersion, value); }
        }
    }
}
