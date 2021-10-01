using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public class DesignLayerList: BaseObject, IXml
    {
        public WhorlDesign Design { get; } //Set by constructor.
        private List<DesignLayer> designLayers { get; } =
           new List<DesignLayer>();

        public IEnumerable<DesignLayer> DesignLayers
        {
            get { return designLayers; }
        }

        public event DesignLayer.LayerChangedDelegate LayerChanged;

        public DesignLayerList(WhorlDesign design)
        {
            if (design == null)
                throw new NullReferenceException("design cannot be null.");
            Design = design;
        }

        public DesignLayerList(WhorlDesign design, WhorlDesign oldDesign): this(design)
        {
            foreach (DesignLayer designLayer in oldDesign.DesignLayerList.DesignLayers)
                this.AddDesignLayer(new DesignLayer(this, designLayer, 
                                    design.DesignPatterns.ToList(), oldDesign.DesignPatterns.ToList()));
        }

        public int GetDesignLayerIndex(DesignLayer designLayer)
        {
            return designLayer == null ? -1 : designLayers.IndexOf(designLayer);
        }

        public DesignLayer GetDesignLayer(int index)
        {
            if (index < 0 || index >= designLayers.Count)
                return null;
            else
                return designLayers[index];
        }

        public void ClearDesignLayers()
        {
            while (designLayers.Count > 0)
                RemoveDesignLayer(designLayers[0]);
        }

        public void AddDesignLayer(DesignLayer designLayer)
        {
            designLayer.ZOrder = designLayers.Count;
            designLayers.Add(designLayer);
            if (string.IsNullOrEmpty(designLayer.LayerName))
                designLayer.LayerName = "Layer" + designLayers.Count;
            designLayer.LayerChanged += LayerOnChanged;
        }

        public void ReorderLayer(DesignLayer layer, int zOrder)
        {
            zOrder = Math.Max(0, Math.Min(designLayers.Count, zOrder));
            layer.ZOrder = zOrder;
            designLayers.Remove(layer);
            designLayers.Insert(zOrder, layer);
            for (int i = zOrder + 1; i < designLayers.Count; i++)
                designLayers[i].ZOrder = i;
            RaiseLayerChangedEvent(layer, whorlDesignChanged: true);
        }

        public void RemoveDesignLayer(DesignLayer designLayer)
        {
            designLayer.LayerChanged -= LayerOnChanged;
            designLayers.Remove(designLayer);
            designLayer.PopulatePatterns(Design.DesignPatterns);
            foreach (Pattern pattern in designLayer.Patterns)
            {
                pattern.DesignLayer = null;
            }
            if (Design.DefaultDesignLayer == designLayer)
                Design.DefaultDesignLayer = null;
            RaiseLayerChangedEvent(designLayer, whorlDesignChanged: true);
        }

        public void RaiseLayerChangedEvent(DesignLayer designLayer, bool whorlDesignChanged)
        {
            LayerOnChanged(designLayer,
                           new DesignLayerChangedEventArgs(whorlDesignChanged));
        }

        private void LayerOnChanged(object sender, DesignLayerChangedEventArgs e)
        {
            if (LayerChanged != null && !Design.Initializing)
                LayerChanged(sender, e);
        }

        //public object Clone()
        //{
        //    DesignLayerList copy = new DesignLayerList(this.Design);
        //    foreach (DesignLayer designLayer in this.DesignLayers)
        //        copy.AddDesignLayer((DesignLayer)designLayer.Clone());
        //    return copy;
        //}

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "DesignLayerList";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var designLayer in DesignLayers)
            {
                designLayer.ToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            this.designLayers.Clear();
            foreach (XmlNode childNode in node.ChildNodes)
                AddDesignLayer(DesignLayer.CreateFromXml(childNode, this));
        }

        internal void BeforeSaveToXml(List<Pattern> designPatterns)
        {
            foreach (DesignLayer designLayer in designLayers)
            {
                designLayer.BeforeSaveToXml(designPatterns);
            }
        }

        internal void AfterReadFromXml(List<Pattern> designPatterns)
        {
            foreach (DesignLayer designLayer in designLayers)
            {
                designLayer.AfterReadFromXml(designPatterns);
            }
        }

    }
}
