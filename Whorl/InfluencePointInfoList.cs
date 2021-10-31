using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class InfluencePointInfoList : IXml
    {
        public Pattern ParentPattern { get; }
        private List<InfluencePointInfo> influencePointInfoList { get; } =
            new List<InfluencePointInfo>();
        public IEnumerable<InfluencePointInfo> InfluencePointInfos => influencePointInfoList;
        public int Count => influencePointInfoList.Count;
        public Dictionary<string, KeyEnumParameters> KeyEnumParamsDict { get; } = new Dictionary<string, KeyEnumParameters>();
        private XmlNode keyEnumParamsDictXmlNode { get; set; }

        public InfluencePointInfoList(Pattern pattern)
        {
            if (pattern == null)
                throw new NullReferenceException("pattern cannot be null.");
            ParentPattern = pattern;
        }

        /// <summary>
        /// Create copy of source.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        public InfluencePointInfoList(InfluencePointInfoList source, Pattern pattern) : this(pattern)
        {
            influencePointInfoList.AddRange(source.InfluencePointInfos.Select(ip => new InfluencePointInfo(ip, pattern, copyKeyParams: false)));
        }

        public void CopyKeyParams(InfluencePointInfoList source, Pattern pattern)
        {
            InfluencePointInfo.CopyKeyParamsDict(KeyEnumParamsDict, source.KeyEnumParamsDict, pattern);
            for (int i = 0; i < source.influencePointInfoList.Count; i++)
            {
                influencePointInfoList[i].CopyKeyParams(source.influencePointInfoList[i], pattern);
            }
        }

        public void AddInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            influencePointInfoList.Add(influencePointInfo);
        }

        public bool RemoveInfluencePointInfo(InfluencePointInfo influencePointInfo)
        {
            bool removed = influencePointInfoList.Remove(influencePointInfo);
            if (removed)
            {
                influencePointInfo.OnRemoved();  //Raises event.
            }
            return removed;
        }

        public IEnumerable<InfluencePointInfo> GetFilteredInfluencePointInfos(string key)
        {
            return InfluencePointInfos.Where(ip => key != null && ip.FilterKeys.Contains(key));
        }

        public IEnumerable<InfluencePointInfo> GetFilteredInfluencePointInfos(IEnumerable<string> keys, bool useOr = false, bool include = true)
        {
            return InfluencePointInfos.Where(ip => ip.EvalBool(keys, key => key != null && ip.FilterKeys.Contains(key), useOr) == include);
        }

        public double ComputeAverage(DoublePoint patternPoint, bool forRendering)
        {
            if (influencePointInfoList.Any())
                return influencePointInfoList.Select(ip => ip.ComputeValue(patternPoint, forRendering, forAverage: true)).Average();
            else
                return 0;
        }

        public void Clear()
        {
            influencePointInfoList.Clear();
        }

        public void TransformInfluencePoints(Complex zFactor)
        {
            foreach (InfluencePointInfo pointInfo in InfluencePointInfos)
            {
                Complex zP = zFactor * new Complex(pointInfo.InfluencePoint.X, pointInfo.InfluencePoint.Y);
                pointInfo.InfluencePoint = new DoublePoint(zP.Re, zP.Im);
            }
        }

        //public void TransformInfluencePoints(double scale, double rotation, bool setOrigPoints = false)
        //{
        //    var zFactor = Complex.CreateFromModulusAndArgument(scale, rotation);
        //    TransformInfluencePoints(zFactor, setOrigPoints);
        //}

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(InfluencePointInfoList);
            XmlNode xmlNode = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var info in influencePointInfoList)
            {
                info.ToXml(xmlNode, xmlTools);
            }
            InfluencePointInfo.AppendKeyParamsXml(xmlNode, KeyEnumParamsDict, xmlTools);
            return xmlTools.AppendToParent(parentNode, xmlNode);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == nameof(InfluencePointInfo))
                {
                    var influencePointInfo = new InfluencePointInfo();
                    influencePointInfo.FromXml(childNode);
                    influencePointInfo.AddToPattern(ParentPattern, setId: false);
                }
                else if (childNode.Name == nameof(KeyEnumParamsDict))
                {
                    keyEnumParamsDictXmlNode = childNode;
                }
                else
                    throw new Exception("Invalid XML.");
            }
        }

        public void FinishFromXml()
        {
            FinishFromXml(keyEnumParamsDictXmlNode, KeyEnumParamsDict);
        }

        public static void FinishFromXml(XmlNode keyEnumParamsDictXmlNode, Dictionary<string, KeyEnumParameters> keyEnumParamsDict)
        {
            if (keyEnumParamsDictXmlNode == null)
                return;
            foreach (XmlNode childNode in keyEnumParamsDictXmlNode.ChildNodes)
            {
                if (childNode.FirstChild?.Name == "Parameters")
                {
                    string enumKey = Tools.GetXmlAttribute<string>(childNode, "Value");
                    if (keyEnumParamsDict.TryGetValue(enumKey, out var keyParams))
                    {
                        if (keyParams.ParametersObject != null)
                        {
                                FormulaSettings.ParseCSharpParamsXml(childNode.FirstChild, keyParams.ParametersObject);
                        }
                    }
                }
            }
        }
    }
}
