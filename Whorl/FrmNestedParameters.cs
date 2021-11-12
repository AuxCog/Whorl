using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class FrmNestedParameters : Form
    {
        public FrmNestedParameters()
        {
            InitializeComponent();
        }

        private CSharpParameterDisplay cSharpParameterDisplay { get; set; }

        public void Initialize(PropertyInfo propertyInfo, FormulaSettings formulaSettings, CSharpParameterDisplay.ParamChanged paramChanged)
        {
            try
            {
                if (!formulaSettings.IsCSharpFormula)
                    throw new Exception("formulaSettings must be for C# formula.");
                if (formulaSettings.EvalInstance?.ParamsObj == null)
                    throw new Exception("Parent parameters object cannot be null.");
                object paramsObj = propertyInfo.GetValue(formulaSettings.EvalInstance.ParamsObj);
                if (paramsObj == null)
                    throw new Exception("Target parameters object cannot be null.");
                lblParentParameterName.Text = propertyInfo.Name;
                cSharpParameterDisplay = new CSharpParameterDisplay(pnlParameters, paramChanged, singleColumn: true);
                cSharpParameterDisplay.UpdateParametersObject = false;
                cSharpParameterDisplay.SetParametersObject(paramsObj);
                cSharpParameterDisplay.AddAllParametersControls(formulaSettings);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
