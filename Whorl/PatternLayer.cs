﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PatternLayer : IFillInfo
    {
        PatternLayerList PatternLayers { get; }

        public float ModulusRatio { get; private set; }

        private FillInfo _fillInfo;

        public FillInfo FillInfo
        {
            get { return _fillInfo; }
            set
            {
                if (_fillInfo != value)
                {
                    _fillInfo = value;
                    if (this == PatternLayers.PatternLayers.FirstOrDefault())
                        PatternLayers.ParentPattern.FillInfo = this.FillInfo;
                }
            }
        }

        public PolarCoord[] SeedPoints { get; set; }

        //public PolarCoord[] SeedPoints
        //{
        //    get
        //    {
        //        if (PatternLayers.ParentPattern.ShrinkPatternLayers)
        //            return seedPoints;
        //        else
        //            return PatternLayers.ParentPattern.SeedPoints;
        //    }
        //    set
        //    {
        //        seedPoints = value;
        //    }
        //}

        public void ApplyShrink(bool shrink = true)
        {
            if (PatternLayers.ParentPattern.SeedPoints == null)
                return;
            SeedPoints = (PolarCoord[])PatternLayers.ParentPattern.SeedPoints.Clone();
            float padding = shrink ? 0.5F * (1F - ModulusRatio) : 0;
            Pattern.ApplyPatternShrink(SeedPoints, padding, PatternLayers.ParentPattern.ShrinkClipFactor, 
                                       PatternLayers.ParentPattern.ShrinkClipCenterFactor,
                                       PatternLayers.ParentPattern.LoopFactor);
            if (PatternLayers.ParentPattern.LoopFactor > 0)
            {
                //Pattern.RemoveLoops(SeedPoints, PatternLayers.ParentPattern.LoopFactor);
            }
        }

        private PointF[] curvePoints;

        public PointF[] CurvePoints
        {
            get
            {
                if (PatternLayers.ParentPattern.ShrinkPatternLayers)
                    return PatternLayers.ParentPattern.CurvePoints;
                else
                    return curvePoints;
            }
            set
            {
                curvePoints = value;
            }
        }

        public DataRow LayerDataRow { get; set; }

        public PatternLayer(PatternLayerList patternLayers)
        {
            PatternLayers = patternLayers;
        }

        public float? SetModulusRatio(float ratio)
        {
            float? retVal = null;
            if (this.PatternLayers.PatternLayers.Count == 0)
            {
                if (ratio != 1D)
                    retVal = 1F;
                ModulusRatio = 1F;
            }
            else
            {
                int i = this.PatternLayers.PatternLayers.IndexOf(this);
                if (i == -1)
                    i = this.PatternLayers.PatternLayers.Count;
                i--;
                float prevRatio = (i < 0) ? 1F :
                                   this.PatternLayers.PatternLayers[i].ModulusRatio;
                if (ratio <= prevRatio && ratio > 0)
                    ModulusRatio = ratio;
                else
                    retVal = prevRatio;
            }
            if (LayerDataRow != null)
                LayerDataRow[0] = 100F * ModulusRatio;
            return retVal;
        }

        public PatternLayer GetCopy(PatternLayerList patternLayerList,
                                    FillInfo fillInfo)
        {
            PatternLayer copy = new PatternLayer(patternLayerList);
            copy.SetModulusRatio(this.ModulusRatio);
            copy.FillInfo = fillInfo;
            return copy;
        }

        public PatternLayer GetCopy(PatternLayerList patternLayerList,
                                    Pattern fillInfoParentPattern)
        {
            FillInfo fillInfo = this.FillInfo?.GetCopy(fillInfoParentPattern);
            return GetCopy(patternLayerList, fillInfo);
        }
    }
}
