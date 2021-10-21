﻿using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public abstract class PixelRenderInfo
    {
        public PixelRenderInfo(Pattern.RenderingInfo parent)
        {
            this.parent = parent;
        }

        private Pattern.RenderingInfo parent { get; }
        public float Position { get; set; }
        public double Rotation { get; set; }
        public bool PolarTraversal { get; set; }
        public bool Normalize { get; set; }
        public bool NormalizeAngle { get; set; }
        public bool ComputeDistance { get; set; }
        public bool ComputeInfluence { get; set; }
        public double InfluenceValue { get; set; }
        public int DistanceCount { get; set; } = 5;
        public int DistanceRows { get; set; } = 10;
        public double SegmentLength { get; set; } = 0;

        public double DistanceToPath { get; protected set; }
        public double[] DistancesToPaths { get; protected set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public DoublePoint PointOffset { get; protected set; }
        public Point IntXY { get; protected set; }
        public PointF Center { get; protected set; }
        public double MaxModulus { get; protected set; }
        public double MaxPosition { get; protected set; }
        public SizeF BoundsSize { get; protected set; }
        public double PatternAngle { get; protected set; }
        public double DraftSize { get; protected set; }
        public int ModulusStep { get; protected set; }
        public int MaxModulusStep { get; protected set; }
        public int MaxAngleStep { get; protected set; }
        public int AngleStep { get; protected set; }
        public double ScaleFactor { get; protected set; }
        public bool HaveSeedPattern
        {
            get { return parent.SeedPattern != null; }
        }


        public Size GetXYBounds()
        {
            return new Size((int)Math.Ceiling(BoundsSize.Width), (int)Math.Ceiling(BoundsSize.Height));
        }

        public float DefaultGetPosition()
        {
            return (float)(Tools.Distance(Center, new PointF(X, Y)) / MaxPosition);
        }

        public PolarPoint GetPolar()
        {
            return GetPolar(Center, transformAngle: true);
        }

        public PolarPoint GetPolar(PointF center, bool transformAngle = false)
        {
            var doublePoint = new DoublePoint(X - center.X, Y - center.Y);
            PolarPoint polarPoint = doublePoint.ToPolar();
            if (!(NormalizeAngle && transformAngle))
                polarPoint.Angle -= PatternAngle;
            return polarPoint;
        }

        public PolarPoint GetScaledPolar()
        {
            return GetScaledPolar(Center, transformAngle: true);
        }

        public PolarPoint GetScaledPolar(PointF center, bool transformAngle = false)
        {
            PolarPoint polarPoint = GetPolar(center, transformAngle);
            polarPoint.Modulus *= ScaleFactor;
            return polarPoint;
        }

        public DoublePoint GetTransformedPoint()
        {
            return GetScaledPolar().ToRectangular();
        }

        public PolarPoint GetSeedPoint(double angle)
        {
            if (parent.SeedPattern == null)
                return new PolarPoint(0, 0);
            else
                return parent.SeedPattern.ComputeSeedPoint(angle);
        }

        public IEnumerable<InfluencePointInfo> GetInfluencePoints(object enumVal)
        {
            if (enumVal == null || !enumVal.GetType().IsEnum)
            {
                throw new Exception("enumVal must be of Enum type.");
            }
            if (parent.ParentPattern.InfluencePointInfoList == null)
                return new InfluencePointInfo[] { };
            var copiedPointsList = new InfluencePointInfoList(parent.ParentPattern.InfluencePointInfoList, parent.ParentPattern);
            string key = $"{enumVal.GetType().Name}.{enumVal}";
            return copiedPointsList.GetFilteredInfluencePointInfos(key);
        }
    }

}
