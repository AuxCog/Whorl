using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class FrmInitialSetup : Form
    {
        public FrmInitialSetup()
        {
            InitializeComponent();
        }

        private void FrmInitialSetup_Load(object sender, EventArgs e)
        {
            try
            {
                txtAppFilesFolder.Text = WhorlSettings.Instance.FilesFolder;
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
                if (string.IsNullOrWhiteSpace(txtAppFilesFolder.Text))
                {
                    MessageBox.Show("Please enter the application files folder.");
                    return;
                }
                MessageBox.Show("Will perform initial setup.");
                string filesFolder = Path.GetFullPath(txtAppFilesFolder.Text);
                if (WhorlSettings.Instance.FilesFolder != filesFolder)
                {
                    WhorlSettings.Instance.FilesFolder = filesFolder;
                    WhorlSettings.Instance.Save();
                }
                if (InitialSetup.PerformInitialSetup())
                    DialogResult = DialogResult.OK;
                else
                    DialogResult = DialogResult.Cancel;
                Close();
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
                Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
