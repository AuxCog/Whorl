using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class RandomOps
    {
        //public float Weight { get; set; }
        //public int XLength { get; set; }
        //public int Smoothness { get; set; }
        //public bool ClipYValues { get; set; }
        //public bool Closed { get; set; }

        private int? randomSeed { get; set; }
        private Random randomGenerator { get; set; }

        public RandomOps()
        {
        }

        //public RandomOps(XmlNode xmlNode)
        //{
        //    FromXml(xmlNode);
        //}

        public void SetRandomSeed(int? seed, bool reset = false)
        {
            if (!reset && randomSeed == seed && randomGenerator != null)
                return;
            if (seed == null)
                randomGenerator = new Random();
            else
                randomGenerator = new Random((int)seed);
            randomSeed = seed;
        }

        public float[] ComputeYValues(out float[] xValues, out PointF[] points, float weight, int xLength, float smoothness, bool closed, bool clipYValues)
        {
            if (randomGenerator == null)
                randomGenerator = new Random();
            if (xLength <= 0)
                throw new Exception("XLength must be positive.");
            if (smoothness <= 0)
                throw new Exception("Smoothness must be positive.");
            int xCount = (int)Math.Ceiling((double)xLength / smoothness);
            if (closed)
                xCount++;
            int usedXLength = (int)(smoothness * (xCount - 1) + 1F);
            float xScale = (float)smoothness;
            var xVals = Enumerable.Range(0, xCount).Select(i => xScale * i).ToList();
            var vals = Enumerable.Range(0, xCount).Select(i => (float)randomGenerator.NextDouble() - 0.5F).ToList();
            var xs = Enumerable.Range(0, usedXLength).Select(i => (float)i).ToList();
            float startSlope, endSlope;
            if (closed)
            {
                startSlope = endSlope = 0F;
                vals[vals.Count - 1] = vals.First();
            }
            else
                startSlope = endSlope = float.NaN;
            xValues = xs.ToArray();
            points = Enumerable.Range(0, xVals.Count).Select(i => new PointF(xVals[i], weight * vals[i])).ToArray();
            IEnumerable<float> yVals = CubicSpline.Compute(xVals.ToArray(), vals.ToArray(), xValues, startSlope, endSlope);
            if (clipYValues)
            {
                yVals = yVals.Select(y => 0.5F * (float)Math.Tanh(2F * y));
            }
            return yVals.Select(y => weight * y).ToArray();
        }

        //public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        //{
        //    if (xmlNodeName == null)
        //        xmlNodeName = nameof(RandomOps);
        //    var xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
        //    xmlTools.AppendXmlAttributesExcept(xmlNode, this, new string[] { });
        //    return xmlTools.AppendToParent(parentNode, xmlNode);
        //}

        //public void FromXml(XmlNode node)
        //{
        //    Tools.GetXmlAttributesExcept(this, node, new string[] { });
        //    //SetRandomSeed(RandomSeed);
        //}

    }
}
