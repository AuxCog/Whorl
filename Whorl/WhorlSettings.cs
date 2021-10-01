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

        //Original value: 1
        private Int32 _animationRate;
        public Int32 AnimationRate
        {
            get { return _animationRate; }
            set { SetProperty(ref _animationRate, value); }
        }

        //Original value: 40
        private Int32 _bufferSize;
        public Int32 BufferSize
        {
            get { return _bufferSize; }
            set { SetProperty(ref _bufferSize, value); }
        }

        //Original value: DefaultPattern.config
        [ReadOnly]
        public String DefaultPatternFileName { get; private set; }

        //Original value: 809
        [ReadOnly]
        public Int32 DefaultPicHeight { get; private set; }

        //Original value: 1596
        [ReadOnly]
        public Int32 DefaultPicWidth { get; private set; }

        //Original value: DesignPatterns.config
        [ReadOnly]
        public String DesignPatternsFileName { get; private set; }

        //Original value: 150
        private Int32 _designThumbnailHeight;
        public Int32 DesignThumbnailHeight
        {
            get { return _designThumbnailHeight; }
            set { SetProperty(ref _designThumbnailHeight, value); }
        }

        //Original value: DesignThumbnails
        [ReadOnly]
        public String DesignThumbnailsFolder { get; private set; }

        //Original value: CustomFolders
        [InitialSettingValue("CustomFolders")]
        [ReadOnly]
        public string CustomDesignParentFolder { get; private set; }

        //Original value: 4
        private Int32 _draftSize;
        public Int32 DraftSize
        {
            get { return _draftSize; }
            set { SetProperty(ref _draftSize, value); }
        }

        //Original value: False
        private Boolean _exactOutline;
        public Boolean ExactOutline
        {
            get { return _exactOutline; }
            set { SetProperty(ref _exactOutline, value); }
        }

        //Original value: C:\WhorlFilesDev
        private String _filesFolder;
        public String FilesFolder
        {
            get { return _filesFolder; }
            set { SetProperty(ref _filesFolder, value); }
        }

        //Original value: 1
        private Double _improvDamping;
        public Double ImprovDamping
        {
            get { return _improvDamping; }
            set { SetProperty(ref _improvDamping, value); }
        }

        //Original value: 0.2
        private Double _improvisationLevel;
        public Double ImprovisationLevel
        {
            get { return _improvisationLevel; }
            set { SetProperty(ref _improvisationLevel, value); }
        }

        //Original value: False
        private Boolean _improviseBackground;
        public Boolean ImproviseBackground
        {
            get { return _improviseBackground; }
            set { SetProperty(ref _improviseBackground, value); }
        }

        //Original value: True
        private Boolean _improviseColors;
        public Boolean ImproviseColors
        {
            get { return _improviseColors; }
            set { SetProperty(ref _improviseColors, value); }
        }

        //Original value: False
        private Boolean _improviseOnOutlineType;
        public Boolean ImproviseOnOutlineType
        {
            get { return _improviseOnOutlineType; }
            set { SetProperty(ref _improviseOnOutlineType, value); }
        }

        //Original value: False
        private Boolean _improviseParameters;
        public Boolean ImproviseParameters
        {
            get { return _improviseParameters; }
            set { SetProperty(ref _improviseParameters, value); }
        }

        //Original value: False
        private Boolean _improvisePetals;
        public Boolean ImprovisePetals
        {
            get { return _improvisePetals; }
            set { SetProperty(ref _improvisePetals, value); }
        }

        //Original value: False
        private Boolean _improviseShapes;
        public Boolean ImproviseShapes
        {
            get { return _improviseShapes; }
            set { SetProperty(ref _improviseShapes, value); }
        }

        //Original value: True
        private Boolean _improviseTextures;
        public Boolean ImproviseTextures
        {
            get { return _improviseTextures; }
            set { SetProperty(ref _improviseTextures, value); }
        }

        //Original value: 1000
        private Int32 _maxLoopCount;
        public Int32 MaxLoopCount
        {
            get { return _maxLoopCount; }
            set { SetProperty(ref _maxLoopCount, value); }
        }

        //Original value: 100
        private Int32 _maxSavedImprovs;
        public Int32 MaxSavedImprovs
        {
            get { return _maxSavedImprovs; }
            set { SetProperty(ref _maxSavedImprovs, value); }
        }

        //Original value: 1000
        private Double _minPatternSize;
        public Double MinPatternSize
        {
            get { return _minPatternSize; }
            set { SetProperty(ref _minPatternSize, value); }
        }

        //Original value: True
        private Boolean _optimizeExpressions;
        public Boolean OptimizeExpressions
        {
            get { return _optimizeExpressions; }
            set { SetProperty(ref _optimizeExpressions, value); }
        }

        //Original value: PatternChoices.config
        [ReadOnly]
        public String PatternChoicesFileName { get; private set; }

        //Original value: 3000
        private Int32 _qualitySize;
        public Int32 QualitySize
        {
            get { return _qualitySize; }
            set { SetProperty(ref _qualitySize, value); }
        }

        //Original value: 5
        private Double _recomputeInterval;
        public Double RecomputeInterval
        {
            get { return _recomputeInterval; }
            set { SetProperty(ref _recomputeInterval, value); }
        }

        //Original value: C:\GreetingCards\Final
        private String _renderFilesFolder;
        public String RenderFilesFolder
        {
            get { return _renderFilesFolder; }
            set { SetProperty(ref _renderFilesFolder, value); }
        }

        //Original value: 2000
        private Int32 _renderWidth;
        public Int32 RenderWidth
        {
            get { return _renderWidth; }
            set { SetProperty(ref _renderWidth, value); }
        }

        //Original value: 1
        private Double _replayIntervalSeconds;
        public Double ReplayIntervalSeconds
        {
            get { return _replayIntervalSeconds; }
            set { SetProperty(ref _replayIntervalSeconds, value); }
        }

        //Original value: 0.1
        private Double _revolveRate;
        public Double RevolveRate
        {
            get { return _revolveRate; }
            set { SetProperty(ref _revolveRate, value); }
        }

        //Original value: True
        private Boolean _saveDesignThumbnails;
        public Boolean SaveDesignThumbnails
        {
            get { return _saveDesignThumbnails; }
            set { SetProperty(ref _saveDesignThumbnails, value); }
        }

        //Original value: 1090x1090:Square,1526x1090:7 x 5,1411x1090:11 x 8.5,1362x1090:10 x 8,1387x1090:11 x 14,727x1090:4 x 6,778x1090:5 x 7,856x1090:14 x 11,842x1090:8.5 x11
        private String _savedImageSizes;
        public String SavedImageSizes
        {
            get { return _savedImageSizes; }
            set { SetProperty(ref _savedImageSizes, value); }
        }


        private String _imageSizes;
        /// <summary>
        /// Comma separted list of AspectRatio:Description
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

        //Original value: 10
        private Int32 _smoothAnimationSteps;
        public Int32 SmoothAnimationSteps
        {
            get { return _smoothAnimationSteps; }
            set { SetProperty(ref _smoothAnimationSteps, value); }
        }

        //Original value: 1
        private Double _spinRate;
        public Double SpinRate
        {
            get { return _spinRate; }
            set { SetProperty(ref _spinRate, value); }
        }

        //Original value: C:\WhorlFilesDev\Textures
        private String _texturesFolder;
        public String TexturesFolder
        {
            get { return _texturesFolder; }
            set { SetProperty(ref _texturesFolder, value); }
        }

        //Original value: True
        private Boolean _traceMode;
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
