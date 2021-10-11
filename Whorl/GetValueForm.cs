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
    public partial class GetValueForm : Form
    {
        public GetValueForm(string label, Func<string, string> validateFunc = null)
        {
            InitializeComponent();
            valueLabel = label;
            this.validateFunc = validateFunc;
        }

        public string ValueText => TextBox?.Text;

        private string valueLabel { get; }
        private Func<string, string> validateFunc { get; }
        private TextBox TextBox { get; set; }

        private void GetValueForm_Load(object sender, EventArgs e)
        {
            try
            {
                var label = new Label() { AutoSize = true };
                label.Text = valueLabel;
                layoutPanel.Controls.Add(label);
                TextBox = new TextBox { Width = 100 };
                layoutPanel.Controls.Add(TextBox);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (validateFunc != null)
                {
                    string errMessage = validateFunc(ValueText);
                    if (errMessage != null)
                    {
                        MessageBox.Show(errMessage);
                        return;
                    }
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }

        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
