using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class RandomValues : IXml, IDisposable
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

            public int? RandomSeed { get; private set; }
            public int XLength { get; set; } = DefaultXLength;
            public float Weight { get; set; }
            public float Smoothness { get; set; } = 20F;
            public bool ClipYValues { get; set; } = true;
            public bool Closed { get; set; } = true;
            public bool SaveRandomSeed { get; set; } = true;
            public RandomDomainTypes DomainType { get; set; } = RandomDomainTypes.Angle;
            public int? ReferenceXLength { get; set; }

            public RandomSettings()
            {
            }

            public RandomSettings(RandomSettings source)
            {
                Tools.CopyProperties(this, source);
            }

            public void ReseedRandom()
            {
                RandomSeed = GetNewSeed();
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                if (xmlNodeName == null)
                    xmlNodeName = nameof(RandomSettings);
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
                string[] excludedProperties = SaveRandomSeed ? new string[] { } : new string[] { nameof(RandomSeed) };
                xmlTools.AppendXmlAttributesExcept(xmlNode, this, excludedProperties);
                return xmlTools.AppendToParent(parentNode, xmlNode);
            }

            public void FromXml(XmlNode node)
            {
                Tools.GetXmlAttributesExcept(this, node);
            }

        }

        public RandomSettings Settings { get; }
        public RandomOps RandomOps { get; } = new RandomOps();

        public RandomValues(bool setNewSeed = true)
        {
            Settings = new RandomSettings();
            if (setNewSeed)
            {
                Settings.ReseedRandom();
            }
        }

        public RandomValues(RandomValues source)
        {
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

        public RandomValues(XmlNode xmlNode)
        {
            Settings = new RandomSettings();
            FromXml(xmlNode);
        }

        public void ResetSeed()
        {
            RandomOps.SetRandomSeed(Settings.RandomSeed, reset: true);
        }

        public void ComputeRandomValues()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name, "Operation not valid on disposed object.");
            RandomOps.SetRandomSeed(Settings.RandomSeed);
            float smoothness = Settings.Smoothness;
            if (Settings.ReferenceXLength != null)
            {
                smoothness *= (float)Settings.XLength / (float)Settings.ReferenceXLength;
            }    
            YValues = RandomOps.ComputeYValues(out float[] xVals, out PointF[] points, Settings.Weight, Settings.XLength,
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

        public static int GetNewSeed()
        {
            return Environment.TickCount;
        }

        public float GetYValue()
        {
            return GetYValue(CurrentXValue);
        }

        public float GetYValue(float x)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name, "Operation not valid on disposed object.");
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
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name, "Operation not valid on disposed object.");
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
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            YValues = null;
            XValues = null;
            FittedPoints = null;
        }
    }
}
