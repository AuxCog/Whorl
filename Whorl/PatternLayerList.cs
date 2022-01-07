using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Whorl
{
    public class PatternLayerList: BaseObject, IXml
    {
        private const string ModulusPercentageName = "Percentage";

        public DataTable LayersDataTable { get; private set; }
        public List<PatternLayer> PatternLayers { get; } =
           new List<PatternLayer>();
        private int _editedIndex;
        public int EditedIndex 
        {
            get => _editedIndex;
            set
            {
                _editedIndex = Math.Max(0, Math.Min(PatternLayers.Count - 1, value));
            }
        }
        public Pattern ParentPattern { get; }

        public EventHandler LayersChanged;

        public PatternLayerList(Pattern parentPattern)
        {
            this.ParentPattern = parentPattern;
        }

        public PatternLayer GetEditedPatternLayer()
        {
            if (EditedIndex >= 0 && EditedIndex < PatternLayers.Count)
                return PatternLayers[EditedIndex];
            else
                return null;
        }

        private void CreateLayersDataTable()
        {
            LayersDataTable = new DataTable();
            LayersDataTable.Columns.Add(ModulusPercentageName, typeof(float));
            LayersDataTable.Columns.Add(nameof(PatternLayer), typeof(PatternLayer));
        }

        public void PopulateLayersDataTable()
        {
            if (LayersDataTable == null)
                CreateLayersDataTable();
            else
                LayersDataTable.RowChanged -= LayersDataTable_RowChanged;
            try
            {
                LayersDataTable.Rows.Clear();
                foreach (var patternLayer in PatternLayers)
                {
                    patternLayer.LayerDataRow = 
                        LayersDataTable.Rows.Add(patternLayer.ModulusRatio * 100F,
                                                 patternLayer);
                }
            }
            finally
            {
                LayersDataTable.RowChanged += LayersDataTable_RowChanged;
            }
        }

        public void RaiseLayersChanged()
        {
            LayersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LayersDataTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            LayersDataTable.RowChanged -= LayersDataTable_RowChanged;
            try
            {
                bool changed = false;
                PatternLayer layer = e.Row[1] as PatternLayer;
                if ((e.Action & DataRowAction.Add) == DataRowAction.Add && layer == null)
                {
                    layer = new PatternLayer(this);
                    PatternLayer lastLayer = PatternLayers.LastOrDefault();
                    if (lastLayer != null)
                        layer.FillInfo = lastLayer.FillInfo.GetCopy(this.ParentPattern);
                    else
                        layer.FillInfo = new PathFillInfo(ParentPattern);
                    PatternLayers.Add(layer);
                    layer.LayerDataRow = e.Row;
                    e.Row[1] = layer;
                    changed = true;
                }
                //else if ((e.Action & DataRowAction.Delete) == DataRowAction.Delete)
                //{
                //    if (layer != null)
                //    {
                //        PatternLayers.Remove(layer);
                //        layer = null;
                //        changed = true;
                //    }
                //}
                object oModulusPercentage = e.Row[0];
                if (oModulusPercentage is float && layer != null)
                {
                    float modulusPercentage = (float)oModulusPercentage;
                    if (modulusPercentage != 100F * layer.ModulusRatio)
                    {
                        float? prevRatio =
                            layer.SetModulusRatio(modulusPercentage / 100F);
                        if (prevRatio != null)
                        {
                            //e.Row[0] = layer.ModulusRatio * 100F;
                            MessageBox.Show(
               $"{ModulusPercentageName} must be positive and not greater than {prevRatio * 100F}.");
                        }
                        else
                        {
                            changed = true;
                        }
                    }
                }
                if (changed)
                    RaiseLayersChanged();
            }
            finally
            {
                LayersDataTable.RowChanged += LayersDataTable_RowChanged;
            }
        }

        public PatternLayerList GetCopy(Pattern parentPattern)
        {
            PatternLayerList copy = new PatternLayerList(parentPattern);
            for (int i = 0; i < this.PatternLayers.Count; i++)
            {
                PatternLayer layer = PatternLayers[i];
                if (i == 0)
                    copy.PatternLayers.Add(layer.GetCopy(copy, parentPattern.FillInfo));
                else
                    copy.PatternLayers.Add(layer.GetCopy(copy, parentPattern));
            }
            copy.EditedIndex = EditedIndex;
            return copy;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(PatternLayerList);
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            if (EditedIndex > 0)
                xmlTools.AppendXmlAttribute(node, nameof(EditedIndex), EditedIndex);
            for (int i = 1; i < this.PatternLayers.Count; i++)
            {
                PatternLayer layer = this.PatternLayers[i];
                XmlNode childNode = xmlTools.CreateXmlNode(nameof(PatternLayer));
                xmlTools.AppendXmlAttribute(childNode, "ModulusRatio", layer.ModulusRatio);
                if (layer.FillInfo != null)
                {
                    layer.FillInfo.ToXml(childNode, xmlTools);
                }
                node.AppendChild(childNode);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            int editedIndex = Tools.GetXmlAttribute<int>(node, defaultValue: 0, nameof(EditedIndex));
            PatternLayer layer = new PatternLayer(this);
            layer.SetModulusRatio(1F);
            layer.FillInfo = this.ParentPattern.FillInfo;
            this.PatternLayers.Add(layer);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                layer = new PatternLayer(this);
                layer.SetModulusRatio(
                    (float)Tools.GetXmlAttribute("ModulusRatio", typeof(float), childNode));
                XmlNode fillInfoNode = childNode.FirstChild;
                if (fillInfoNode != null)
                {
                    switch (fillInfoNode.Name)
                    {
                        case nameof(PathFillInfo):
                            layer.FillInfo = new PathFillInfo(ParentPattern);
                            break;
                        case nameof(TextureFillInfo):
                            layer.FillInfo = new TextureFillInfo(this.ParentPattern);
                            break;
                        case nameof(BackgroundFillInfo):
                            layer.FillInfo = new BackgroundFillInfo(this.ParentPattern);
                            break;
                        default:
                            throw new Exception("Expecting PatternLayer FillInfo in XML.");
                    }
                    layer.FillInfo.FromXml(fillInfoNode);
                }
                this.PatternLayers.Add(layer);
            }
            EditedIndex = editedIndex;
        }
    }
}
