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
    public partial class FrmRotation : Form
    {
        public FrmRotation()
        {
            InitializeComponent();
        }

        public bool UseImageCenter => chkUseImageCenter.Checked;
        public double RotationAngle { get; private set; }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(txtRotationDegrees.Text, out double val))
                {
                    MessageBox.Show("Please enter a number for Rotation Degrees.");
                    return;
                }
                RotationAngle = Tools.DegreesToRadians(val);
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
