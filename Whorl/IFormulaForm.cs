using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public interface IFormulaForm
    {
        void SelectError(ParserEngine.Token token);
        void SetFormula(FormulaEntry formulaEntry);
    }
}
