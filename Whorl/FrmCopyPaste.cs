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
    public partial class FrmCopyPaste : Form
    {
        public FrmCopyPaste()
        {
            InitializeComponent();
        }

        private CopyPasteInfo copyPasteInfo { get; set; }

        public void Initialize(bool forPaste, CopyPasteInfo copyPasteInfo)
        {
            try
            {
                this.copyPasteInfo = copyPasteInfo;
                var items = new List<string>();
                items.AddRange(Enumerable.Range(1, copyPasteInfo.Count).Select(i => i.ToString()));
                if (!forPaste)
                {
                    items.Add($"{copyPasteInfo.Count + 1} (New)");
                }
                cboCopyId.DataSource = items;
                if (copyPasteInfo.CurrentIndex < items.Count)
                    cboCopyId.SelectedIndex = copyPasteInfo.CurrentIndex;
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
                copyPasteInfo.CurrentIndex = cboCopyId.SelectedIndex;
                DialogResult = DialogResult.OK;
                Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
