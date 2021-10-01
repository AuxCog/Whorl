using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class FormulaTools
    {
        private PathOutline pathOutline;
        private Pattern pattern;
        private Ribbon ribbon;

        public FormulaTools(WhorlDesign design)
        {
            pathOutline = new PathOutline();
            pattern = new Pattern(design, FillInfo.FillTypes.Path);
            ribbon = new Ribbon(pattern);
            ribbon.InitFormulaSettings();
        }

        public List<FormulaEntry> InferFormulaTypes()
        {
            var retList = new List<FormulaEntry>();
            foreach (FormulaEntry formulaEntry in MainForm.FormulaEntryList.GetEntries(FormulaTypes.Unknown))
            {
                FormulaTypes formulaType = InferType(formulaEntry);
                if (formulaType != FormulaTypes.Unknown)
                {
                    if (formulaType == FormulaTypes.Outline && string.IsNullOrEmpty(formulaEntry.MaxAmplitudeFormula))
                        formulaEntry.MaxAmplitudeFormula = "maxAmplitude = 1;";
                    formulaEntry.FormulaType = formulaType;
                    retList.Add(formulaEntry);
                }
            }
            return retList;
        }

        private FormulaTypes InferType(FormulaEntry formulaEntry)
        {
            string formula = formulaEntry.Formula;
            FormulaTypes formulaType = FormulaTypes.Unknown;
            if (FormulaReferencesName(formula, pathOutline.VerticesSettings, "AddVertex"))
                formulaType = FormulaTypes.PathVertices;
            else if (FormulaReferencesName(formula, ribbon.FormulaSettings, "RibbonInfo"))
                formulaType = FormulaTypes.Ribbon;
            else if (FormulaReferencesName(formula, pathOutline.customOutline.AmplitudeSettings, "amplitude"))
                formulaType = FormulaTypes.Outline;
            return formulaType;
        }

        private bool FormulaReferencesName(string formula, FormulaSettings formulaSettings, string name)
        {
            bool retVal = false;
            if (formulaSettings.Parser.ParseStatements(formula))
            {
                ValidIdentifier valIdent = formulaSettings.Parser.GetValidIdentifier(name);
                if (valIdent == null)
                    throw new Exception($"Couldn't get ValidIdentifier for {name}.");
                if (formulaSettings.Parser.Expression.ReferencesValidIdentifier(valIdent))
                    retVal = true;
            }
            return retVal;
        }
    }
}
