using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class DesignLayer: BaseObject, IXml
    {
        public DesignLayerList DesignLayerList { get; }
        private List<long> patternIds { get; } = new List<long>();
        public IEnumerable<long> PatternIds
        {
            get { return patternIds; }
        }
        private ColorBlendTypes _colorBlendType = ColorBlendTypes.None;
        public ColorBlendTypes ColorBlendType
        {
            get { return _colorBlendType; }
            set
            {
                if (_colorBlendType != value)
                {
                    _colorBlendType = value;
                    LayerOnChanged();
                }
            }
        }
        private int _zOrder;
        public int ZOrder
        {
            get { return _zOrder; }
            set
            {
                if (_zOrder != value)
                {
                    _zOrder = value;
                    //LayerOnChanged();
                }
            }
        }
        private float _blendStrength = 1F;
        public float BlendStrength
        {
            get { return _blendStrength; }
            set
            {
                if (_blendStrength != value)
                {
                    _blendStrength = value;
                    LayerOnChanged();
                }
            }
        }
        private bool _visible = true;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    LayerOnChanged(whorlDesignChanged: false);
                }
            }
        }
        public string LayerName { get; set; }
        public delegate void LayerChangedDelegate(
               object sender, DesignLayerChangedEventArgs e);
        public event LayerChangedDelegate LayerChanged;
        public List<Pattern> Patterns { get; } = new List<Pattern>();

        public DesignLayer(DesignLayerList designLayerList)
        {
            this.DesignLayerList = designLayerList;
        }

        public DesignLayer(DesignLayerList designLayerList, DesignLayer source, List<Pattern> designPatterns,
                           List<Pattern> oldDesignPatterns)
        {
            if (designPatterns.Count != oldDesignPatterns.Count)
                throw new Exception(
                    "DesignLayer(): pattern lists have different counts.");
            this.DesignLayerList = designLayerList;
            this.ZOrder = source.ZOrder;
            this.ColorBlendType = source.ColorBlendType;
            this.BlendStrength = source.BlendStrength;
            this.Visible = source.Visible;
            foreach (long oldPatternId in source.PatternIds)
            {
                int index = oldDesignPatterns.FindIndex(ptn => ptn.PatternID == oldPatternId);
                if (index != -1)
                {
                    designPatterns[index].DesignLayer = this;
                    //patternIds.Add(designPatterns[index].PatternID);
                }
            }
        }

        private void LayerOnChanged(bool whorlDesignChanged = true)
        {
            if (LayerChanged != null)
                LayerChanged(this, new DesignLayerChangedEventArgs(whorlDesignChanged));
        }

        public void AddPatternID(long patternID)
        {
            if (!patternIds.Contains(patternID))
            {
                patternIds.Add(patternID);
                LayerOnChanged();
            }
        }

        public void RemovePatternID(long patternID)
        {
            if (patternIds.Remove(patternID))
                LayerOnChanged();
        }

        public void PopulatePatterns(IEnumerable<Pattern> designPatterns)
        {
            Patterns.Clear();
            foreach (long patternID in patternIds)
            {
                Pattern pattern = designPatterns.Where(ptn => ptn.PatternID == patternID).FirstOrDefault();
                //if (pattern == null)
                //    throw new Exception($"PatternID {patternID} not found in Design Patterns.");
                //else
                if (pattern != null)
                    Patterns.Add(pattern);
            }
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "DesignLayer";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            string patternIndicesList = string.Join(",", patternIds);
            xmlTools.AppendXmlAttributes(node, this, nameof(ZOrder), nameof(ColorBlendType), nameof(BlendStrength));
            xmlTools.AppendXmlAttribute(node, "PatternIndices", patternIndicesList);
            return xmlTools.AppendToParent(parentNode, node);
        }

        public static DesignLayer CreateFromXml(XmlNode node, DesignLayerList designLayerList)
        {
            DesignLayer designLayer = new DesignLayer(designLayerList);
            designLayer.FromXml(node);
            return designLayer;
        }

        public void FromXml(XmlNode node)
        {
            ZOrder = (int)Tools.GetXmlAttribute("ZOrder", typeof(int), node);
            ColorBlendType = Tools.GetEnumXmlAttr(node, "ColorBlendType", 
                                                  ColorBlendTypes.None);
            object oBlendStrength = Tools.GetXmlAttribute("BlendStrength", typeof(float), node,
                                                          required: false);
            if (oBlendStrength == null)
                BlendStrength = ColorBlendType == ColorBlendTypes.Contrast ? 3F : 1F;
            else if (DesignLayerList.Design.XmlVersion < WhorlDesign.NewBlendXmlVersion && ColorBlendType != ColorBlendTypes.Contrast)
                BlendStrength = 1F;
            else
                BlendStrength = (float)oBlendStrength;
            string patternIndicesList = (string)Tools.GetXmlAttribute("PatternIndices",
                                         typeof(string), node);
            patternIds.Clear();
            patternIds.AddRange(patternIndicesList.Split(new char[] { ',' }, 
                                StringSplitOptions.RemoveEmptyEntries)
                                .Select(ind => long.Parse(ind)));
        }

        internal void BeforeSaveToXml(List<Pattern> designPatterns)
        {
            patternIds.RemoveAll(id => !designPatterns.Exists(ptn => ptn.PatternID == id));
        }

        internal void AfterReadFromXml(List<Pattern> designPatterns)
        {
            Patterns.Clear();
            for (int i = 0; i < patternIds.Count; i++)
            {
                long xmlPatternID = patternIds[i];
                Pattern pattern = designPatterns.Find(ptn => ptn.XmlPatternID == xmlPatternID);
                //if (pattern == null)
                //    throw new Exception(
                //       $"Layer XmlPatternID {xmlPatternID} was not found.");
                //else
                if (pattern != null)
                {
                    patternIds[i] = pattern.PatternID;
                    Patterns.Add(pattern);
                }
            }
            foreach (Pattern pattern in this.Patterns)
                pattern.DesignLayer = this;
        }
    }
}
