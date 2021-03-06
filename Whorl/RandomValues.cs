using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class RandomValues : IXml
    {
        public enum RandomDomainTypes
        {
            Angle,
            X
        }
        public float[] XValues { get; private set; }
        public float[] YValues { get; private set; }
        public PointF[] FittedPoints { get; private set; }
        public float MinXValue { get; private set; }
        public float MaxXValue { get; private set; }

        public float CurrentXValue { get; set; }

        public class RandomSettings : IXml
        {
            public const int DefaultXLength = 500;

            public int? RandomSeed { get; set; }
            public int XLength { get; set; } = DefaultXLength;
            public float Weight { get; set; }
            public float Smoothness { get; set; } = 20F;
            public bool ClipYValues { get; set; } = true;
            public bool Closed { get; set; } = true;
            public RandomDomainTypes DomainType { get; set; } = RandomDomainTypes.Angle;
            public int? ReferenceXLength { get; set; }

            public RandomSettings()
            {
            }

            public RandomSettings(RandomSettings source)
            {
                Tools.CopyProperties(this, source);
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(RandomSettings);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                xmlTools.AppendAllXmlAttributes(xmlNode, this);
                return xmlTools.AppendToParent(parentNode, xmlNode);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetXmlAttributesExcept(this, node, "SaveRandomSeed");
            }
        }

        public RandomSettings Settings { get; }
        private RandomOps randomOps { get; }

        public RandomValues(bool setNewSeed = true)
        {
            randomOps = new RandomOps();
            Settings = new RandomSettings();
            if (setNewSeed)
            {
                SetNewSeed();
            }
        }

        public RandomValues(RandomValues source)
        {
            randomOps = new RandomOps(source.randomOps);
            Settings = new RandomSettings(source.Settings);
            if (source.YValues != null)
            {
                XValues = (float[])source.XValues.Clone();
                YValues = (float[])source.YValues.Clone();
                FittedPoints = (PointF[])source.FittedPoints.Clone();
                MinXValue = source.MinXValue;
                MaxXValue = source.MaxXValue;
            }
        }

        public RandomValues(XmlNode xmlNode): this(setNewSeed: false)
        {
            FromXml(xmlNode);
        }

        public void SetNewSeed()
        {
            randomOps.SetNewSeed();
            Settings.RandomSeed = randomOps.RandomSeed;
        }

        public void ResetSeed()
        {
            randomOps.SetRandomSeed(Settings.RandomSeed, reset: true);
        }

        public void ClearValues()
        {
            YValues = null;
            XValues = null;
            FittedPoints = null;
            CurrentXValue = MinXValue = MaxXValue = 0F;
        }

        public void ComputeRandomValues()
        {
            randomOps.SetRandomSeed(Settings.RandomSeed);
            float smoothness = Settings.Smoothness;
            if (Settings.ReferenceXLength != null)
            {
                smoothness *= (float)Settings.XLength / (float)Settings.ReferenceXLength;
            }    
            YValues = randomOps.ComputeYValues(out float[] xVals, out PointF[] points, Settings.Weight, Settings.XLength,
                                               smoothness, Settings.Closed, Settings.ClipYValues);
            XValues = xVals;
            FittedPoints = points;
            if (XValues.Any())
            {
                MinXValue = XValues.Min();
                MaxXValue = XValues.Max();
            }
            else
                MinXValue = MaxXValue = 0F;
        }

        public float GetYValue()
        {
            return GetYValue(CurrentXValue);
        }

        public float GetYValue(float x)
        {
            if (XValues == null)
                throw new Exception("ComputeRandom was not called.");
            if (!XValues.Any())
                return 0F;
            float range = MaxXValue - MinXValue;
            if (range == 0F)
                return YValues.First();
            float ratio = (x - MinXValue) / range;
            if (ratio < 0F || ratio > 1F)
            {
                ratio %= 1F;
                if (ratio < 0)
                    ratio += 1F;
            }
            float fIndex = ratio * (YValues.Length - 1);
            int index = (int)fIndex;
            float yVal = YValues[index];
            if (index < YValues.Length - 1)
            {
                //Do linear interpolation:
                yVal += (fIndex - index) * (YValues[index + 1] - yVal);
            }
            return yVal;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(RandomValues);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            Settings.ToXml(xmlNode, xmlTools);
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == nameof(RandomSettings))
                {
                    Settings.FromXml(childNode);
                }
                else
                {
                    throw new Exception($"Invalid XmlNode named {childNode.Name}.");
                }
            }
            ResetSeed();
        }

        //public bool Disposed { get; private set; }

        //public void Dispose()
        //{
        //    if (Disposed)
        //        return;
        //    Disposed = true;
        //    ClearValues();
        //}
    }
}
