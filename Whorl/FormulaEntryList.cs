using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Whorl
{
    /// <summary>
    /// List of FormulaEntry objects, for saving and retrieving formulas.
    /// </summary>
    public class FormulaEntryList: BaseObject, IXml, ICloneable
    {
        public enum AddFormulaStatus
        {
            Added,
            Replaced,
            Unchanged,
            Cancelled,
            Invalid
        }

        private Dictionary<string, FormulaEntry> formulaDict { get; }
        public int SavedFormulaCount { get; private set; }

        public FormulaEntryList()
        {
            formulaDict = new Dictionary<string, FormulaEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public FormulaEntryList(FormulaEntryList source)
        {
            formulaDict = new Dictionary<string, FormulaEntry>(source.formulaDict, StringComparer.OrdinalIgnoreCase);
            SavedFormulaCount = source.SavedFormulaCount;
        }

        public IEnumerable<FormulaEntry> FormulaEntries
        {
            get { return formulaDict.Values.OrderBy(fe => fe.FormulaName); }
        }
        
        public bool IsChanged
        {
            get { return SavedFormulaCount != formulaDict.Count || formulaDict.Values.Any(fe => fe.IsChanged); }
        }

        public bool HandleRename { get; private set; } = true;

        public FormulaEntry GetFormulaByName(string name)
        {
            FormulaEntry formulaEntry;
            if (!formulaDict.TryGetValue(name, out formulaEntry))
                formulaEntry = null;
            return formulaEntry;
        }

        public bool AddFormula(FormulaEntry formulaEntry)
        {
            bool isValid = !formulaDict.ContainsKey(formulaEntry.FormulaName);
            if (!isValid)
            {
                string newName = GetUniqueName(formulaEntry.FormulaName);
                if (MessageBox.Show($"Rename formula {formulaEntry.FormulaName} to {newName}?", "Confirm", 
                         MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SetFormulaName(formulaEntry, newName);
                    isValid = true;
                }
            }
            if (isValid)
                formulaDict.Add(formulaEntry.FormulaName, formulaEntry);
            return isValid;
        }

        public bool RemoveFormula(string formulaName)
        {
            FormulaEntry formulaEntry = GetFormulaByName(formulaName);
            if (formulaEntry != null)
            {
                if (formulaEntry.IsSystem)
                {
                    throw new CustomException($"The formula {formulaName} is system required, and cannot be removed.");
                }
                formulaDict.Remove(formulaEntry.FormulaName);
                return true;
            }
            else
                return false;
        }

        private void SetFormulaName(FormulaEntry formulaEntry, string name)
        {
            HandleRename = false;
            formulaEntry.FormulaName = name;
            HandleRename = true;
        }

        public bool RenameFormula(FormulaEntry formulaEntry, string newName, bool throwException = false)
        {
            if (formulaEntry.IsSystem)
                return false;
            var entry = GetFormulaByName(newName);
            if (entry != null && entry != formulaEntry)
            {
                if (throwException)
                    throw new ArgumentException($"{newName} is a duplicate.");
                else
                {
                    MessageBox.Show($"{newName} is a duplicate.");
                    return false;
                }
            }
            if (entry != formulaEntry)
            {
                RemoveFormula(formulaEntry.FormulaName);
                SetFormulaName(formulaEntry, newName);
                AddFormula(formulaEntry);
            }
            return true;
        }

        //public void SortByName()
        //{
        //    FormulaEntries = FormulaEntries.OrderBy(fe => fe.FormulaName).ToList();
        //}

        public IEnumerable<FormulaEntry> GetEntries(FormulaTypes formulaType)
        {
            return FormulaEntries.Where(fe => fe.FormulaType == formulaType);
        }

        public IEnumerable<FormulaEntry> GetSystemEntries(FormulaTypes formulaType)
        {
            return FormulaEntries.Where(fe => fe.IsSystem && fe.FormulaType == formulaType);
        }

        //public T GetNamedFormula<T>(string formulaName) where T: FormulaEntry
        //{
        //    return GetFormulaByName(formulaName) as T;
        //}

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = "FormulaEntryList";
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            foreach (var formulaEntry in FormulaEntries)
            {
                formulaEntry.ToXml(node, xmlTools);
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void AfterReadOrSave()
        {
            SavedFormulaCount = formulaDict.Count;
        }

        public void FromXml(XmlNode node)
        {
            formulaDict.Clear();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                FormulaTypes formulaType = FormulaTypes.Unknown;
                switch (childNode.Name)
                {
                    case "FormulaEntry":
                        break;
                    case "OutlineFormulaEntry":  //Legacy code.
                        //formulaType = FormulaTypes.Outline;
                        break;
                    case "TransformFormulaEntry":  //Legacy code.
                        formulaType = FormulaTypes.Transform;
                        break;
                    default:
                        throw new Exception("Invalid XML node: " + childNode.Name);
                }
                FormulaEntry entry = new FormulaEntry(formulaType);
                entry.FromXml(childNode);
                AddFormula(entry);
                //FormulaEntries.Add(entry);
            }
            //SortByName();
        }

        private string GetUniqueName(string baseName)
        {
            int suffix = 2;
            int i = baseName.Length - 1;
            while (i > 0 && Char.IsDigit(baseName[i]))
                i--;
            if (i < baseName.Length - 1)
            {
                i++;
                suffix = 1 + int.Parse(baseName.Substring(i));
                baseName = baseName.Substring(0, i);
            }
            string name = null;
            while (true)
            {
                name = baseName + suffix;
                if (!formulaDict.ContainsKey(name))
                    break;
                suffix++;
            }
            return name;
        }

        //private string GetNonDuplicateName(string baseName)
        //{
        //    //HashSet<string> entryNames = new HashSet<string>(
        //    //                             GetEntries<T>().Select(fe => fe.FormulaName), 
        //    //                             StringComparer.OrdinalIgnoreCase);
        //    if (!formulaDict.ContainsKey(baseName))
        //        return baseName;
        //    else
        //    {
        //        switch (MessageBox.Show($"Replace formula named {baseName}?", "Confirm",
        //                        MessageBoxButtons.YesNoCancel))
        //        {
        //            case DialogResult.Cancel:
        //                return null;
        //            case DialogResult.Yes:
        //                return baseName;
        //            default:
        //                string name = GetUniqueName(baseName);
        //                if (MessageBox.Show($"Rename duplicate to {name}?", "Confirm",
        //                                    MessageBoxButtons.YesNo) != DialogResult.Yes)
        //                {
        //                    name = null;
        //                }
        //                return name;
        //        }
        //    }
        //}

        public FormulaEntry AddFormulaEntry(FormulaTypes formulaType, string formulaName, string formula, bool isCSharp,
                                    out AddFormulaStatus status, string maxAmplitudeFormula = null, 
                                    FormulaUsages formulaUsage = FormulaUsages.Normal)
        {
            string errorMessage = null;
            if (string.IsNullOrWhiteSpace(formulaName))
                errorMessage = "Formula name cannot be blank.";
            else if (string.IsNullOrWhiteSpace(formula))
                errorMessage = "Formula cannot be blank.";
            else if (formulaType == FormulaTypes.Outline && string.IsNullOrWhiteSpace(maxAmplitudeFormula))
                errorMessage = "Max Amplitude Formula cannot be blank.";
            if (errorMessage != null)
            {
                status = AddFormulaStatus.Invalid;
                MessageBox.Show(errorMessage);
                return null;
            }
            status = AddFormulaStatus.Added;
            FormulaEntry formulaEntry = GetFormulaByName(formulaName);
            if (formulaEntry != null)
            {
                if (formulaType == formulaEntry.FormulaType &&
                    formula == formulaEntry.Formula &&
                    isCSharp == formulaEntry.IsCSharp &&
                    (formulaType != FormulaTypes.Outline || formulaEntry.MaxAmplitudeFormula == maxAmplitudeFormula) &&
                    formulaUsage == formulaEntry.FormulaUsage)
                {
                    status = AddFormulaStatus.Unchanged;
                }
                else
                {
                    bool replaceIsValid = (formulaType == formulaEntry.FormulaType) ||
                                          (formulaType != FormulaTypes.Transform && formulaEntry.FormulaType == FormulaTypes.Unknown);
                    if (replaceIsValid)
                    {
                        switch (MessageBox.Show($"Replace formula named {formulaName}?", "Confirm",
                                        MessageBoxButtons.YesNoCancel))
                        {
                            case DialogResult.Cancel:
                                status = AddFormulaStatus.Cancelled;
                                break;
                            case DialogResult.Yes:
                                status = AddFormulaStatus.Replaced;
                                break;
                            case DialogResult.No:
                                status = AddFormulaStatus.Added;
                                break;
                        }
                    }
                    if (status == AddFormulaStatus.Added)
                    {
                        //Make sure new formulaName is unique.
                        string newName = GetUniqueName(formulaName);
                        if (MessageBox.Show($"Rename duplicate to {newName}?", "Confirm",
                                            MessageBoxButtons.OKCancel) != DialogResult.OK)
                        {
                            status = AddFormulaStatus.Cancelled;
                        }
                        else
                        {
                            formulaName = newName;
                        }
                    }
                }
            }
            switch (status)
            {
                case AddFormulaStatus.Added:
                case AddFormulaStatus.Replaced:
                    if (status == AddFormulaStatus.Added)
                    {
                        formulaEntry = new FormulaEntry(formulaType);
                        SetFormulaName(formulaEntry, formulaName);
                    }
                    else
                        formulaEntry.FormulaType = formulaType;
                    formulaEntry.Formula = formula;
                    formulaEntry.IsCSharp = isCSharp;
                    formulaEntry.FormulaUsage = formulaUsage;
                    if (formulaType == FormulaTypes.Outline)
                        formulaEntry.MaxAmplitudeFormula = maxAmplitudeFormula;
                    if (status == AddFormulaStatus.Added)
                        AddFormula(formulaEntry);
                    break;
                case AddFormulaStatus.Unchanged:
                    if (formulaType != FormulaTypes.Outline)
                        MessageBox.Show("The formula is unchanged.");
                    else
                        MessageBox.Show("The formulas are unchanged.");
                    break;
                case AddFormulaStatus.Cancelled:
                    formulaEntry = null;
                    break;
            }
            return formulaEntry;
        }

        //public bool AddOutlineFormulaEntry(string formulaName, string formula, 
        //                                   string maxAmplitudeFormula)
        //{
        //    OutlineFormulaEntry ofe = AddFormulaEntry<OutlineFormulaEntry>(
        //                                              formulaName, formula, out bool added, out bool unchanged);
        //    if (unchanged && ofe.MaxAmplitudeFormula == maxAmplitudeFormula)
        //    {
        //        MessageBox.Show("The formulas are unchanged.");
        //    }
        //    else
        //    {
        //        ofe.MaxAmplitudeFormula = maxAmplitudeFormula;
        //    }
        //    return added;
        //}

        //public bool AddTransformFormulaEntry(string formulaName, string formula)
        //{
        //    AddFormulaEntry<TransformFormulaEntry>(formulaName, formula, out bool added, out bool unchanged);
        //    if (unchanged)
        //    {
        //        MessageBox.Show("The formula is unchanged.");
        //    }
        //    return added;
        //}

        public object Clone()
        {
            return new FormulaEntryList(this);
        }
    }
}
