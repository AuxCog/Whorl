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
    public partial class ZoomFactorForm : Form
    {
        public PointF ZoomFactors { get; private set; }
        public bool KeepCenters
        {
            get { return chkKeepCenters.Checked; }
        }

        public ZoomFactorForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string message = null;
                float factor = GetFactor(txtZoomFactor.Text, ref message);
                //float yFactor = GetFactor(txtZoomFactorY.Text, "height", ref message);
                if (message != null)
                {
                    MessageBox.Show(message);
                    return;
                }
                factor /= 100F;
                ZoomFactors = new PointF(factor, factor);
                DialogResult = DialogResult.OK;
                this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private float GetFactor(string sFac, ref string message)
        {
            if (message != null)
                return 0;
            float factor;
            if (!float.TryParse(sFac, out factor))
                factor = 0;
            if (factor <= 0)
            {
                message = "Please enter a percentage greater than 0 for Zoom Factor.";
            }
            return factor;
        }
    }
}
