using ParserEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class ParserErrorsForm : Form
    {
        private ParserErrorsForm()
        {
            InitializeComponent();
        }

        private static Lazy<ParserErrorsForm> lazy = new Lazy<ParserErrorsForm>(() => new ParserErrorsForm());

        public static ParserErrorsForm DefaultForm
        {
            get
            {
                var frm = lazy.Value;
                if (frm.IsDisposed)
                {
                    lazy = new Lazy<ParserErrorsForm>(() => new ParserErrorsForm());
                    frm = lazy.Value;
                }
                return frm;
            }
        }

        public static void ResetForm()
        {
            var frm = DefaultForm;
            if (!frm.IsDisposed && frm.IsInitialized)
            {
                frm.Close();
                frm.Dispose();
            }
        }

        private FormulaSettings formulaSettings { get; set; }
        private IFormulaForm formulaForm { get; set; }
        private List<ParserEngine.ErrorInfo> errorInfoList { get; set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(FormulaSettings formulaSettings, IFormulaForm formulaForm, 
                               bool preprocessorErrors = false, CSharpSharedCompiledInfo sharedCompiledInfo = null,
                               string cSharpCode = null)
        {
            try
            {
                this.formulaSettings = formulaSettings;
                this.formulaForm = formulaForm;
                if (preprocessorErrors)
                {
                    errorInfoList = CSharpPreprocessor.Instance.ErrorMessages;
                }
                else if (formulaSettings.IsCSharpFormula)
                {
                    if (sharedCompiledInfo == null)
                        throw new Exception("sharedCompiledInfo == null");
                    errorInfoList = new List<ErrorInfo>();
                    int currLineNo = 1;
                    int currIndex = 0;
                    int nextIndex = -1;
                    string code = cSharpCode ?? formulaSettings.Formula;
                    int codeLength = code.Length;
                    foreach (var errInfo in sharedCompiledInfo.Errors
                             .OrderBy(ei => ei.Line).ThenBy(ei => ei.Column))
                    {
                        while (currLineNo < errInfo.Line && currIndex < codeLength)
                        {
                            nextIndex = code.IndexOf(Environment.NewLine, currIndex);
                            if (nextIndex == -1)
                                break;
                            currIndex = nextIndex + Environment.NewLine.Length;
                            currLineNo++;
                        }
                        string errText;
                        if (currIndex < codeLength)
                        {
                            nextIndex = code.IndexOf(Environment.NewLine, currIndex);
                            if (nextIndex == -1)
                                nextIndex = codeLength;
                            currIndex = Math.Max(0, Math.Min(currIndex + errInfo.Column - 1, nextIndex - 1));
                            errText = code.Substring(currIndex, nextIndex - currIndex);
                        }
                        else
                            errText = string.Empty;
                        var token = new Token(errText, Token.TokenTypes.None, currIndex);
                        var parseErrInfo = new ErrorInfo(token, errInfo.Message);
                        errorInfoList.Add(parseErrInfo);
                    }
                }
                else
                {
                    errorInfoList = formulaSettings.Parser.GetErrorInfoList().ToList();
                }
                dgvErrorInfo.DataSource = errorInfoList;
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvErrorInfo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex != SelectButtonColumn.Index)
                    return;
                ErrorInfo errorInfo = errorInfoList[e.RowIndex];
                if (errorInfo.Token != null)
                {
                    formulaForm.SelectError(errorInfo.Token);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
