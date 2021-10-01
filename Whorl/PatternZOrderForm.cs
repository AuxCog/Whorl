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
    public partial class PatternZOrderForm : Form
    {
        public int ZOrder { get; private set; }

        public PatternZOrderForm()
        {
            InitializeComponent();
        }

        public void Initialize(int zOrder, int patternCount)
        {
            this.cboZOrder.DataSource = Enumerable.Range(0, patternCount).ToList();
            this.cboZOrder.SelectedItem = zOrder;
            this.ZOrder = zOrder;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.ZOrder = (int)this.cboZOrder.SelectedItem;
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
    }
}
