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
    public partial class ImprovConfigForm : Form
    {
        public ImprovConfigForm()
        {
            InitializeComponent();
        }

        private WhorlDesign design { get; set; }
        private bool improvConfigWasNull { get; set; }

        public void Initialize(WhorlDesign design)
        {
            try
            {
                this.design = design;
                improvConfigWasNull = design.ImproviseConfig == null;
                if (design.ImproviseConfig == null)
                    design.ImproviseConfig = new ImproviseConfig();
                chkEnabled.Checked = design.ImproviseConfig.Enabled;
                chkImproviseOnAllPatterns.Checked = design.ImproviseConfig.ImproviseOnAllPatterns;
                chkDrawDesignLayers.Checked = design.ImproviseConfig.DrawDesignLayers;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                design.ImproviseConfig.Enabled = chkEnabled.Checked;
                design.ImproviseConfig.ImproviseOnAllPatterns = chkImproviseOnAllPatterns.Checked;
                design.ImproviseConfig.DrawDesignLayers = chkDrawDesignLayers.Checked;
                this.Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (improvConfigWasNull)
                    design.ImproviseConfig = null;
                this.Close();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
