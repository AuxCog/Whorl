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
        //private Panel parametersPanel { get; set; }

        public void Initialize(PropertyInfo propertyInfo, FormulaSettings formulaSettings, 
                               CSharpParameterDisplay.ParamChanged paramChanged, int index = -1)
        {
            try
            {
                if (!formulaSettings.IsCSharpFormula)
                    throw new Exception("formulaSettings must be for C# formula.");
                if (formulaSettings.EvalInstance?.ParamsObj == null)
                    throw new Exception("Parent parameters object cannot be null.");
                //parametersPanel = pnlParams;
                object paramsObj;
                string label = propertyInfo.Name;
                object propertyVal = propertyInfo.GetValue(formulaSettings.EvalInstance.ParamsObj);
                if (propertyVal == null)
                    paramsObj = null;
                else
                {
                    if (propertyVal.GetType().IsArray)
                    {
                        var array = (Array)propertyVal;
                        propertyVal = array.GetValue(index);
                        label += $"[{index + 1}]";
                    }
                    var fnParam = propertyVal as Func1Parameter<double>;
                    if (fnParam == null)
                        paramsObj = propertyVal;
                    else if (fnParam.Instances != null)
                        paramsObj = fnParam.Instances.FirstOrDefault();
                    else
                        paramsObj = null;
                }
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
            //try
            //{
            //    parametersPanel.HorizontalScroll.Value = 0;
            //}
            //catch { }
            Close();
        }
    }
}
