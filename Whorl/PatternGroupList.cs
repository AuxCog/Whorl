using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Whorl
{
    public class PatternGroupList: BaseObject, IXml, ICloneable
    {
        public bool IsPatternChanged { get; private set; }
        public bool IsColorChanged { get; private set; }
        public bool IsPaletteChanged { get; private set; }
        public bool IsFormulaChanged { get; private set; }
        public bool IsChanged
        {
            get { return IsPatternChanged || IsColorChanged || IsPaletteChanged || IsFormulaChanged; }
        }

        public void FinalizeIsChanged()
        {
            if (PatternGroups != null && PatternGroups.Exists(pg => pg.IsChanged))
                IsPatternChanged = true;
            if (FormulaEntryList != null && FormulaEntryList.IsChanged)
                IsFormulaChanged = true;
        }

        public void ClearIsChanged()
        {
            IsPatternChanged = IsColorChanged = IsPaletteChanged = IsFormulaChanged = false;
            FormulaEntryList.AfterReadOrSave();
            foreach (var formulaEntry in FormulaEntryList.FormulaEntries)
            {
                formulaEntry.AfterReadOrSave();
            }
        }

        private PatternList _logoPatternGroup;
        public PatternList LogoPatternGroup
        {
            get { return _logoPatternGroup; }
            set
            {
                if (_logoPatternGroup != value)
                {
                    _logoPatternGroup = value;
                    IsPatternChanged = true;
                }
            }
        }
        public List<PatternList> PatternGroups { get; set; }
        public List<Color> ColorChoices { get; set; }
        public List<ColorNodeList> PaletteChoices { get; set; }
        public FormulaEntryList FormulaEntryList { get; set; }
        //DefaultPatternGroup does not affect IsChanged.
        public PatternList DefaultPatternGroup { get; set; }
        public WhorlDesign Design { get; }

        public PatternGroupList(WhorlDesign design, bool initLists = true)
        {
            if (design == null)
                throw new NullReferenceException("design cannot be null.");
            Design = design;
            if (initLists)
            {
                PatternGroups = new List<PatternList>();
                ColorChoices = new List<Color>();
                PaletteChoices = new List<ColorNodeList>();
                FormulaEntryList = new FormulaEntryList();
            }
        }

        public void AddPatternGroup(PatternList patternGroup)
        {
            PatternGroups.Add(patternGroup);
            IsPatternChanged = true;
        }

        public void RemovePatternGroup(PatternList patternGroup)
        {
            if (PatternGroups.Remove(patternGroup))
                IsPatternChanged = true;
        }

        public void SetPatternGroup(PatternList patternGroup, int index)
        {
            PatternGroups[index] = patternGroup;
            IsPatternChanged = true;
        }

        public bool AddColor(Color color)
        {
            bool retVal = !ColorChoices.Contains(color);
            if (retVal)
            {
                ColorChoices.Add(color);
                IsColorChanged = true;
            }
            return retVal;
        }

        public void RemoveColorAt(int index)
        {
            ColorChoices.RemoveAt(index);
            IsColorChanged = true;
        }

        public void SetColor(Color color, int index)
        {
            ColorChoices[index] = color;
            IsColorChanged = true;
        }

        public bool AddPalette(ColorNodeList palette)
        {
            bool retVal = !PaletteChoices.Exists(p => p.IsEqual(palette));
            if (retVal)
            {
                PaletteChoices.Add(palette);
                IsPaletteChanged = true;
            }
            return retVal;
        }

        public void RemovePaletteAt(int index)
        {
            PaletteChoices.RemoveAt(index);
            IsPaletteChanged = true;
        }

        public void SetPalette(ColorNodeList palette, int index)
        {
            PaletteChoices[index] = palette;
            IsPaletteChanged = true;
        }

        public bool SetPatternName(PatternList patternList, string name)
        {
            name = PatternList.GetPatternListName(name);
            if (patternList.PatternListName == name)
                return false;
            if (!string.IsNullOrWhiteSpace(name))
            {
                int seqNo = 1;
                string origName = name;
                while (PatternGroups.Exists(pl => pl != patternList && pl.PatternListName == name))
                {
                    ++seqNo;
                    name = $"{origName} {seqNo}";
                }
                if (seqNo > 1)
                {
                    if (MessageBox.Show($"'{origName}' is a duplicate.  Change to '{name}'?", "Confirm", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                    {
                        return false;
                    }
                }
            }
            patternList.PatternListName = name;
            return true;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "PatternGroupList";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            //if (DefaultPatternGroup != null)
            //    sb.AppendLine(DefaultPatternGroup.ToXml("DefaultPatternGroup"));
            if (LogoPatternGroup != null)
                LogoPatternGroup.ToXml(node, xmlTools, "LogoPatternGroup");
            foreach (var ptnGrp in PatternGroups)
            {
                ptnGrp.ToXml(node, xmlTools, "PatternGroup");
            }
            var childNode = xmlTools.CreateXmlNode("ColorChoices");
            foreach (Color clr in ColorChoices)
            {
                childNode.AppendChild(xmlTools.CreateXmlNode("Color", clr));
            }
            node.AppendChild(childNode);
            childNode = xmlTools.CreateXmlNode("PaletteChoices");
            foreach (ColorNodeList colorNodeList in PaletteChoices)
            {
                colorNodeList.ToXml(childNode, xmlTools, "Palette");
            }
            node.AppendChild(childNode);
            FormulaEntryList.ToXml(node, xmlTools, "FormulaEntryList");
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "PatternGroup" || 
                    childNode.Name == "DefaultPatternGroup" ||
                    childNode.Name == "LogoPatternGroup")
                {
                    PatternList patternList = new PatternList(Design);
                    patternList.FromXml(childNode);
                    switch (childNode.Name)
                    {
                        case "PatternGroup":
                            this.PatternGroups.Add(patternList);
                            break;
                        case "DefaultPatternGroup":
                            this.DefaultPatternGroup = patternList;
                            break;
                        case "LogoPatternGroup":
                            this.LogoPatternGroup = patternList;
                            break;
                    }
                }
                else if (childNode.Name == "ColorChoices")
                {
                    foreach (XmlNode colorNode in childNode.ChildNodes)
                    {
                        this.ColorChoices.Add(Tools.GetColorFromXml(colorNode));
                    }
                }
                else if (childNode.Name == "PaletteChoices")
                {
                    foreach (XmlNode paletteNode in childNode.ChildNodes)
                    {
                        var palette = new ColorNodeList();
                        palette.FromXml(paletteNode);
                        PaletteChoices.Add(palette);
                    }
                }
                else if (childNode.Name == "FormulaEntryList")
                    FormulaEntryList.FromXml(childNode);
                else
                {
                    throw new Exception("Invalid XML found for PatternGroupList.");
                }
            }
        }

        public object Clone()
        {
            PatternGroupList copy = new PatternGroupList(Design, initLists: false);
            if (LogoPatternGroup != null)
                copy.LogoPatternGroup = LogoPatternGroup.GetCopy();
            copy.PatternGroups = this.PatternGroups.Select(pg => pg.GetCopy()).ToList();
            copy.ColorChoices = new List<Color>(this.ColorChoices);
            copy.PaletteChoices = PaletteChoices.Select(p => new ColorNodeList(p.ColorNodes)).ToList();
            copy.FormulaEntryList = (FormulaEntryList)this.FormulaEntryList.Clone();
            copy.DefaultPatternGroup = this.DefaultPatternGroup.GetCopy();
            return copy;
        }
    }
}
