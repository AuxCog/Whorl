using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class RandomOps: IXml
    {
        public int? RandomSeed { get; private set; }
        private Random randomGenerator { get; set; }

        public RandomOps(bool setNewSeed = true)
        {
            if (setNewSeed)
                SetNewSeed();
        }

        public RandomOps(RandomOps source)
        {
            SetRandomSeed(source.RandomSeed, reset: true);
        }

        public void SetRandomSeed(int? seed, bool reset = false)
        {
            if (!reset && RandomSeed == seed && randomGenerator != null)
                return;
            if (seed == null)
                randomGenerator = new Random();
            else
                randomGenerator = new Random((int)seed);
            RandomSeed = seed;
        }

        public void SetNewSeed()
        {
            SetRandomSeed(GetNewSeed(), reset: true);
        }

        public void ResetSeed()
        {
            SetRandomSeed(RandomSeed, reset: true);
        }

        public static int GetNewSeed()
        {
            return Environment.TickCount;
        }

        public double GetNextDouble()
        {
            return randomGenerator.NextDouble();
        }

        public float[] ComputeYValues(out float[] xValues, out PointF[] points, float weight, int xLength, float smoothness, bool closed, bool clipYValues)
        {
            if (randomGenerator == null)
                randomGenerator = new Random();
            if (xLength <= 0)
                throw new Exception("XLength must be positive.");
            smoothness = Math.Max(1F, smoothness);
            int xCount = (int)Math.Ceiling((double)xLength / smoothness);
            if (closed)
                xCount++;
            int usedXLength = (int)(smoothness * (xCount - 1) + 1F);
            var xVals = Enumerable.Range(0, xCount).Select(i => smoothness * i).ToArray();
            var vals = Enumerable.Range(0, xCount).Select(i => (float)randomGenerator.NextDouble() - 0.5F).ToArray();
            xValues = Enumerable.Range(0, usedXLength).Select(i => (float)i).ToArray();
            float startSlope, endSlope;
            if (closed)
            {
                startSlope = endSlope = 0F;
                vals[vals.Length - 1] = vals.First();
            }
            else
                startSlope = endSlope = float.NaN;
            points = Enumerable.Range(0, xVals.Length).Select(i => new PointF(xVals[i], weight * vals[i])).ToArray();
            IEnumerable<float> yVals = CubicSpline.Compute(xVals, vals, xValues, startSlope, endSlope);
            if (clipYValues)
            {
                yVals = yVals.Select(y => 0.5F * (float)Math.Tanh(2F * y));
            }
            return yVals.Select(y => weight * y).ToArray();
        }

        /// <summary>
        /// Saves RandomSeed.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="xmlTools"></param>
        /// <param name="xmlNodeName"></param>
        /// <returns></returns>
        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(RandomOps);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            xmlTools.AppendAllXmlAttributes(xmlNode, this);
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        /// <summary>
        /// Reads RandomSeed.
        /// </summary>
        /// <param name="node"></param>
        public void FromXml(XmlNode node)
        {
            Tools.GetAllXmlAttributes(this, node);
            SetRandomSeed(RandomSeed, reset: true);
        }
    }
}
