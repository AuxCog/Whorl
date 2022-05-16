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
    public partial class frmTextEditor : Form
    {
        public frmTextEditor()
        {
            InitializeComponent();
        }

        private void frmTextEditor_Resize(object sender, EventArgs e)
        {
            try
            {
                int textTop = btnOK.Bottom + 10;
                int textHeight = ClientSize.Height - textTop;
                if (textHeight > 10)
                {
                    txtText.Height = textHeight;
                }
            }            
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public string EditedText
        {
            get { return txtText.Text; }
        }

        public void SizeToTextHeight()
        {
            string text = txtText.Text;
            if (string.IsNullOrEmpty(text))
                text = " ";
            int height = Math.Min(800, 20 + TextRenderer.MeasureText(text, txtText.Font).Height);
            ClientSize = new Size(ClientSize.Width, btnOK.Bottom + height);
        }

        public void DisplayText(string text, bool readOnly = false, bool? showOK = null,
                                bool autoSize = false)
        {
            txtText.Text = text;
            txtText.ReadOnly = readOnly;
            if (showOK == null)
                showOK = !readOnly;
            btnOK.Visible = showOK.Value;
            if (!showOK.Value)
                btnCancel.Text = "Close";
            if (autoSize)
                SizeToTextHeight();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}
