using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class PatternImproviseConfig: BaseObject, IXml
    {
        //public long SharedPatternID { get; set; }

        private class ColorIndices
        {
            public int ColorCount { get; set; }
            public HashSet<int> IndexHashSet { get; } = new HashSet<int>();

            public void Initialize()
            {
                IndexHashSet.Clear();
                if (ColorCount > 0)
                    IndexHashSet.UnionWith(Enumerable.Range(0, ColorCount));
            }

            public ColorIndices GetCopy()
            {
                var copy = new ColorIndices();
                copy.ColorCount = ColorCount;
                copy.IndexHashSet.UnionWith(IndexHashSet);
                return copy;
            }
        }

        public bool Enabled { get; set; } = true;

        public List<ParameterImproviseConfig> ParameterConfigs { get; } =
           new List<ParameterImproviseConfig>();

        public ImproviseFlags ImproviseFlags { get; }

        private ColorIndices[] ColorIndicesByLayer { get; set; }

        public PatternImproviseConfig()
        {
            this.ImproviseFlags = new ImproviseFlags();
        }

        public PatternImproviseConfig(PatternImproviseConfig source)
        {
            this.Enabled = source.Enabled;
            if (source.ColorIndicesByLayer != null)
            {
                ColorIndicesByLayer = source.ColorIndicesByLayer.Select(ci => ci.GetCopy()).ToArray();
            }
            ParameterConfigs = source.ParameterConfigs.Select(
                               pc => new ParameterImproviseConfig(pc)).ToList();
            this.ImproviseFlags = source.ImproviseFlags.GetCopy();
        }

        public void PopulateFromPattern(Pattern pattern, bool reset = false)
        {
            //SharedPatternID = pattern.SharedPatternID;
            if (!reset)
                ParameterConfigs.Clear();
            foreach (var outline in pattern.BasicOutlines)
            {
                PathOutline pathOutline = outline as PathOutline;
                if (pathOutline == null || !pathOutline.UseVertices)
                {
                    if (outline.customOutline != null)
                        AddParameterConfigs(outline.customOutline.AmplitudeSettings,
                                            ImproviseParameterType.Outline, 
                                            reset);
                }
                else
                    AddParameterConfigs(pathOutline.VerticesSettings,
                                        ImproviseParameterType.Outline,
                                        reset);
            }
            foreach (var transform in pattern.Transforms)
            {
                AddParameterConfigs(transform.TransformSettings, 
                                    ImproviseParameterType.Transform,
                                    reset,
                                    transform.TransformName);
            }
        }

        private void AddParameterConfigs(FormulaSettings formulaSettings,
                                         ImproviseParameterType paramType,
                                         bool reset,
                                         string formulaName = null)
        {
            if (formulaSettings == null)
                return;
            foreach (var parameter in formulaSettings.Parameters)
            {
                ParameterImproviseConfig paramConfig = null;
                if (reset)
                    paramConfig = ParameterConfigs.Find(pc => pc.ParameterGuid == parameter.Guid);
                if (paramConfig == null)
                {
                    paramConfig = new ParameterImproviseConfig(paramType, parameter);
                    paramConfig.FormulaName = formulaName;
                    ParameterConfigs.Add(paramConfig);
                }
            }
        }

        public void PopulateColorIndicesFromPattern(Pattern pattern, bool checkCounts = false)
        {
            bool hasPixelRendering = pattern.PixelRendering != null && pattern.PixelRendering.Enabled &&
                                     pattern.PixelRendering.ColorNodes != null;
            if (checkCounts && ColorIndicesByLayer != null)
            {
                if (hasPixelRendering)
                {
                    if (ColorIndicesByLayer.Length == 1)
                    {
                        var colorIndices = ColorIndicesByLayer[0];
                        if (colorIndices.ColorCount == pattern.PixelRendering.ColorNodes.Count ||
                            colorIndices.ColorCount == -1)
                        {
                            if (colorIndices.ColorCount == -1)
                                colorIndices.ColorCount = pattern.PixelRendering.ColorNodes.Count;
                            return;
                        }
                    }
                }
                else
                {
                    if (ColorIndicesByLayer.Length == pattern.PatternLayers.PatternLayers.Count)
                    {
                        bool countsUnchanged = true;
                        for (int i = 0; countsUnchanged && i < ColorIndicesByLayer.Length; i++)
                        {
                            var colorIndices = ColorIndicesByLayer[i];
                            PatternLayer patternLayer = pattern.PatternLayers.PatternLayers[i];
                            PathFillInfo pathFillInfo = patternLayer.FillInfo as PathFillInfo;
                            int colorCount = pathFillInfo == null ? 1 : pathFillInfo.GetColorCount();
                            if (colorIndices.ColorCount != colorCount)
                            {
                                if (colorIndices.ColorCount == -1)
                                    colorIndices.ColorCount = colorCount;
                                else
                                    countsUnchanged = false;
                            }
                        }
                        if (countsUnchanged)
                            return;
                    }
                }
            }
            if (hasPixelRendering)
            {
                ColorIndicesByLayer = new ColorIndices[1];
                ColorIndicesByLayer[0] = new ColorIndices() { ColorCount = pattern.PixelRendering.ColorNodes.Count };
                ColorIndicesByLayer[0].Initialize();
            }
            else
            {
                ColorIndicesByLayer = new ColorIndices[pattern.PatternLayers.PatternLayers.Count];
                for (int i = 0; i < ColorIndicesByLayer.Length; i++)
                {
                    ColorIndicesByLayer[i] = new ColorIndices();
                    PatternLayer patternLayer = pattern.PatternLayers.PatternLayers[i];
                    PathFillInfo pathFillInfo = patternLayer.FillInfo as PathFillInfo;
                    if (pathFillInfo != null)
                        ColorIndicesByLayer[i].ColorCount = pathFillInfo.GetColorCount();
                    else
                        ColorIndicesByLayer[i].ColorCount = 1;  //Index 0 signifies the TextureFillInfo is enabled for improvising.
                    ColorIndicesByLayer[i].Initialize();
                }
            }
        }

        public HashSet<int> GetColorIndices(int layerIndex)
        {
            if (ColorIndicesByLayer == null)
                return null;
            else
                return (layerIndex >= 0 && layerIndex < ColorIndicesByLayer.Length) ? 
                        ColorIndicesByLayer[layerIndex].IndexHashSet : null;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "PatternImproviseConfig";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            if (ColorIndicesByLayer != null)
            {
                string colorIndicesList = string.Join(";", ColorIndicesByLayer.Select(
                       inds => $"{inds.ColorCount}:" + string.Join(",", inds.IndexHashSet)));
                xmlTools.AppendXmlAttribute(node, "ColorIndices", colorIndicesList);
            }
            xmlTools.AppendXmlAttribute(node, "Enabled", Enabled);
            ImproviseFlags.ToXml(node, xmlTools);
            foreach (var paramConfig in ParameterConfigs)
            {
                paramConfig.ToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            Tools.GetXmlAttributesExcept(this, node, excludedPropertyNames: new string[] { "SharedPatternID", "ColorIndices" });
            string colorIndicesByLayers = (string)Tools.GetXmlAttribute("ColorIndices", typeof(string), node, required: false);
            if (!string.IsNullOrEmpty(colorIndicesByLayers))
            {
                string[] colorIndicesLists = colorIndicesByLayers.Split(';');
                ColorIndicesByLayer = new ColorIndices[colorIndicesLists.Length];
                for (int i = 0; i < colorIndicesLists.Length; i++)
                {
                    ColorIndicesByLayer[i] = new ColorIndices();
                    string[] parts = colorIndicesLists[i].Split(':');
                    string indexList;
                    if (parts.Length == 2)
                    {
                        ColorIndicesByLayer[i].ColorCount = int.Parse(parts[0]);
                        indexList = parts[1];
                    }
                    else  //Legacy code.
                    {
                        ColorIndicesByLayer[i].ColorCount = -1;
                        indexList = parts[0];
                    }
                    if (!string.IsNullOrEmpty(indexList))
                    {
                        ColorIndicesByLayer[i].IndexHashSet.UnionWith(indexList.Split(',').Select(s => int.Parse(s)));
                    }
                }
            }
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "ParameterImproviseConfig":
                        var paramConfig = new ParameterImproviseConfig();
                        paramConfig.FromXml(childNode);
                        ParameterConfigs.Add(paramConfig);
                        break;
                    case "ImproviseFlags":
                        ImproviseFlags.FromXml(childNode);
                        break;
                    default:
                        throw new Exception($"Invalid XML for {GetType().Name}.");
                }
            }
        }
    }
}
