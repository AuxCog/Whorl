using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class JoinedPatterns: Pattern
    {
        public Pattern Pattern1 { get; set; }
        public Pattern Pattern2 { get; set; }
        public int InsertIndex { get; set; } = -1;
        private List<PointF> seedCurvePoints { get; set; }

        public JoinedPatterns(WhorlDesign design): base(design)
        {
        }

        public JoinedPatterns(JoinedPatterns source, WhorlDesign design = null): base(design ?? source.Design)
        {
            CopyProperties(source);
            if (source.Pattern1 != null)
                Pattern1 = source.Pattern1.GetCopy(design: design);
            if (source.Pattern2 != null)
                Pattern2 = source.Pattern2.GetCopy(design: design);
            if (source.seedCurvePoints != null)
                seedCurvePoints = new List<PointF>(source.seedCurvePoints);
        }

        protected override IEnumerable<string> GetExtraExcludedCopyProperties()
        {
            return base.GetExtraExcludedCopyProperties().Concat(
                   new string[] { nameof(Pattern1), nameof(Pattern2) });
        }

        public bool IsValid()
        {
            bool isValid = Pattern1 != null && Pattern2 != null;
            if (isValid)
            {
                if (Pattern1.CurvePoints == null)
                    isValid = Pattern1.ComputeCurvePoints(Pattern1.ZVector);
                if (isValid && InsertIndex < 0 || InsertIndex >= Pattern1.CurvePoints.Length)
                    isValid = false;
            }
            return isValid;
        }

        public void SetPatternsEnabled(bool enabled)
        {
            if (Pattern1 != null)
                Pattern1.PatternIsEnabled = enabled;
            if (Pattern2 != null)
                Pattern2.PatternIsEnabled = enabled;
        }

        public bool FinishJoin()
        {
            if (!IsValid())
                return false;
            if (Pattern2.CurvePoints == null)
            {
                if (!Pattern2.ComputeCurvePoints(Pattern2.ZVector))
                    return false;
                if (Pattern2.CurvePoints.Length == 0)
                    return false;
            }
            Center = Pattern1.Center;
            ZVector = Pattern1.ZVector;
            FillInfo = Pattern1.FillInfo;
            return ComputeSeedCurvePoints();
        }

        private bool ComputeSeedCurvePoints()
        {
            if (!IsValid())
                return false;
            if (Pattern2.CurvePoints == null)
            {
                if (!Pattern2.ComputeCurvePoints(Pattern2.ZVector))
                    return false;
                if (Pattern2.CurvePoints.Length == 0)
                    return false;
            }
            seedCurvePoints = new List<PointF>();
            PointF pJoin = Pattern1.CurvePoints[InsertIndex];
            PointF p1 = Pattern2.CurvePoints[0];
            PointF pDelta = new PointF(pJoin.X - p1.X, pJoin.Y - p1.Y);
            seedCurvePoints.AddRange(Pattern1.CurvePoints.Take(InsertIndex));
            seedCurvePoints.AddRange(Pattern2.CurvePoints.Skip(1)
                                     .Select(p => new PointF(p.X + pDelta.X, p.Y + pDelta.Y)));
            seedCurvePoints.AddRange(Pattern1.CurvePoints.Skip(InsertIndex));
            var points2 = seedCurvePoints.Select(p => new PointF(p.X - Center.X, p.Y - Center.Y));
            float maxLen = (float)Math.Sqrt(points2.Select(p => p.X * p.X + p.Y * p.Y).Max());
            float fac = maxLen == 0 ? 1F : 1F / maxLen;
            seedCurvePoints = points2.Select(p => new PointF(fac * p.X, fac * p.Y)).ToList();
            return true;
        }

        public override bool ComputeCurvePoints(Complex zVector, bool recomputeInnerSection = true, bool forOutline = false)
        {
            bool success = seedCurvePoints != null && Pattern1 != null;
            if (success)
            {
                Complex zVec1 = Pattern1.ZVector;
                zVec1.Normalize();
                Complex zTransform = ZVector / zVec1;
                var pTransform = new PointF((float)zTransform.Re, (float)zTransform.Im);
                CurvePoints = seedCurvePoints.Select(p => Tools.RotatePoint(p, pTransform))
                                             .Select(p => new PointF(p.X + Center.X, p.Y + Center.Y))
                                             .ToArray();
            }
            else
                CurvePoints = new PointF[0];
            return success;
        }

        public static int FindInsertIndex(Pattern pattern, PointF joinPoint, double bufferSize = 30.0)
        {
            if (pattern.CurvePoints == null)
            {
                if (!pattern.ComputeCurvePoints(pattern.ZVector))
                    return -1;
            }
            var infos = Enumerable.Range(0, pattern.CurvePoints.Length)
                        .Select(i => new Tuple<int, double>(i, Tools.DistanceSquared(joinPoint, pattern.CurvePoints[i])))
                        .Where(tpl => tpl.Item2 <= bufferSize)
                        .OrderBy(tpl => tpl.Item2).ThenBy(tpl => tpl.Item1);
            if (infos.Any())
                return infos.First().Item1;
            else
                return -1;
        }

        private void AppendPatternXml(XmlNode parentNode, Pattern pattern, string nodeName, XmlTools xmlTools)
        {
            if (pattern == null)
                return;
            XmlNode xmlNode = xmlTools.CreateXmlNode(nodeName);
            pattern.ToXml(xmlNode, xmlTools);
            parentNode.AppendChild(xmlNode);
        }

        public override XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(JoinedPatterns);
            XmlNode xmlNode = base.ToXml(parentNode, xmlTools, xmlNodeName);
            xmlTools.AppendXmlAttribute(xmlNode, nameof(InsertIndex), InsertIndex);
            return xmlNode;
        }

        protected override void ExtraXml(XmlNode parentNode, XmlTools xmlTools)
        {
            base.ExtraXml(parentNode, xmlTools);
            AppendPatternXml(parentNode, Pattern1, nameof(Pattern1), xmlTools);
            AppendPatternXml(parentNode, Pattern2, nameof(Pattern2), xmlTools);
        }

        public override void FromXml(XmlNode node)
        {
            base.FromXml(node);
            ComputeSeedCurvePoints();
        }

        protected override bool FromExtraXml(XmlNode node)
        {
            bool retVal = true;
            switch (node.Name)
            {
                case nameof(Pattern1):
                    Pattern1 = CreatePatternFromXml(Design, node.FirstChild, throwOnError: true);
                    break;
                case nameof(Pattern2):
                    Pattern2 = CreatePatternFromXml(Design, node.FirstChild, throwOnError: true);
                    break;
                default:
                    retVal = base.FromExtraXml(node);
                    break;
            }
            return retVal;
        }
    }
}
