using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class PointsRandomOps
    {
        public class RandomPoint
        {
            /// <summary>
            /// Point.X and Y are normalized to be in range 0 to 1.
            /// </summary>
            public PointF Point { get; }

            /// <summary>
            /// RandomValue is in range 0 to 1.
            /// </summary>
            public double RandomValue { get; }

            public RandomPoint(PointF point, double randomValue)
            {
                Point = point;
                RandomValue = randomValue;
            }

            public RandomPoint(RandomPoint source)
            {
                Point = source.Point;
                RandomValue = source.RandomValue;
            }

            public RandomPoint(XmlNode xmlNode)
            {
                float X = Tools.GetXmlAttribute<float>(xmlNode, "X");
                float Y = Tools.GetXmlAttribute<float>(xmlNode, "Y");
                Point = new PointF(X, Y);
                RandomValue = Tools.GetXmlAttribute<double>(xmlNode, nameof(RandomValue));
            }

            public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
            {
                XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName ?? nameof(RandomPoint));
                xmlTools.AppendXmlAttribute(xmlNode, "X", Point.X);
                xmlTools.AppendXmlAttribute(xmlNode, "Y", Point.Y);
                xmlTools.AppendXmlAttribute(xmlNode, nameof(RandomValue), RandomValue);
                return xmlTools.AppendToParent(parentNode, xmlNode);
            }

        }

        public int HorizCount { get; set; } = 10;
        public int VertCount { get; set; } = 10;
        public double PointRandomWeight { get; set; } = 0.5;

        public double ValueWeight { get; set; }
        public double DistanceOffset { get; set; } = 0.005;
        public double DistancePower { get; set; } = 2.0;
        public Func1Parameter<double> RandomFunction { get; set; }
        public double InnerWeight { get; set; } = 1.0;
        public double InnerOffset { get; set; }

        public PointF UnitScalePoint { get; set; }
        public PointF PanPoint { get; set; }

        public RandomPoint[,] RandomPoints { get; private set; }

        public RandomOps PointRandomOps { get; }
        public RandomOps ValueRandomOps { get; }

        public PointsRandomOps()
        {
            PointRandomOps = new RandomOps();
            ValueRandomOps = new RandomOps();
        }

        public PointsRandomOps(PointsRandomOps source)
        {
            PointRandomOps = new RandomOps(source.PointRandomOps);
            ValueRandomOps = new RandomOps(source.ValueRandomOps);
            Tools.CopyProperties(this, source, excludedPropertyNames: new string[] { nameof(RandomPoints) });
            if (source.RandomPoints != null)
            {
                int length1 = source.RandomPoints.GetLength(0);
                int length2 = source.RandomPoints.GetLength(1);
                RandomPoints = new RandomPoint[length1, length2];
                for (int i = 0; i < length1; i++)
                {
                    for (int j = 0; j < length2; j++)
                    {
                        RandomPoints[i, j] = new RandomPoint(source.RandomPoints[i, j]);
                    }
                }
            }
        }

        public PointsRandomOps(XmlNode xmlNode): this() //Creates PointRandomOps and ValueRandomOps.
        {
            FromXml(xmlNode);
        }

        public void ClearValues()
        {
            RandomPoints = null;
        }

        public void ComputePoints()
        {
            if (HorizCount <= 0)
                throw new Exception("HorizCount must be positive.");
            if (VertCount <= 0)
                throw new Exception("VertCount must be positive.");
            if (PointRandomWeight < 0 || PointRandomWeight > 1)
                throw new Exception("PointRandomWeight must be between 0 and 1.");
            PointRandomOps.ResetSeed();
            ValueRandomOps.ResetSeed();
            RandomPoints = new RandomPoint[HorizCount, VertCount];
            float yInc = 1F / VertCount;
            float xInc = 1F / HorizCount;
            double pointWeightY = PointRandomWeight / VertCount;
            double pointWeightX = PointRandomWeight / HorizCount;
            float y = 0;
            for (int yi = 0; yi < VertCount; yi++)
            {
                float x = 0;
                for (int xi = 0; xi < HorizCount; xi++)
                {
                    float randomY = y + (float)(pointWeightY * PointRandomOps.GetNextDouble());
                    float randomX = x + (float)(pointWeightX * PointRandomOps.GetNextDouble());
                    double randomValue = ValueRandomOps.GetNextDouble();
                    RandomPoints[xi, yi] = new RandomPoint(new PointF(randomX, randomY), randomValue);
                    x += xInc;
                }
                y += yInc;
            }
        }

        public double ComputeDistanceValue(PointF point)
        {
            if (RandomPoints == null)
                throw new Exception("ComputePoints was not called.");
            PointF unitPoint = new PointF(UnitScalePoint.X * (PanPoint.X + point.X), 
                                          UnitScalePoint.Y * (PanPoint.Y + point.Y));
            double value = 0;
            double power = 0.5 * DistancePower;
            for (int yi = 0; yi < VertCount; yi++)
            {
                for (int xi = 0; xi < HorizCount; xi++)
                {
                    RandomPoint randomPoint = RandomPoints[xi, yi];
                    double distance = Tools.DistanceSquared(unitPoint, randomPoint.Point);
                    if (power != 1.0)
                        distance = Math.Pow(distance, power);
                    value += randomPoint.RandomValue / (DistanceOffset + distance);
                }
            }
            value = value * InnerWeight / (VertCount * HorizCount) + InnerOffset;
            if (RandomFunction != null)
                value = RandomFunction.Function(value);
            return ValueWeight * value;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PointsRandomOps);
            var xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendXmlAttributesExcept(xmlNode, this, 
                     nameof(RandomPoints), nameof(PointRandomOps), nameof(ValueRandomOps), 
                     nameof(UnitScalePoint), nameof(PanPoint), nameof(RandomFunction));
            PointRandomOps.ToXml(xmlNode, xmlTools, nameof(PointRandomOps));
            ValueRandomOps.ToXml(xmlNode, xmlTools, nameof(ValueRandomOps));
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        private void FromXml(XmlNode node)
        {
            Tools.GetAllXmlAttributes(this, node);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case nameof(PointRandomOps):
                        PointRandomOps.FromXml(childNode);
                        break;
                    case nameof(ValueRandomOps):
                        ValueRandomOps.FromXml(childNode);
                        break;
                    default:
                        throw new Exception($"Invalid XML node name: {childNode.Name}.");
                }
            }
        }
    }
}
