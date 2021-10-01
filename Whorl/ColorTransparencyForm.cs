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
    public partial class ColorTransparencyForm : Form
    {
        private IColor callingForm { get; set; }

        public ColorTransparencyForm()
        {
            InitializeComponent();
        }

        private int TransparencyToAlpha(int transparency)
        {
            return 255 * (100 - transparency) / 100;
        }

        private int AlphaToTransparency(int alpha)
        {
            return 100 * (255 - alpha) / 255;
        }

        public void Initialize(IColor callingForm)
        {
            this.callingForm = callingForm;
            int transparency = AlphaToTransparency(callingForm.TransparencyColor.A);
            hscrlTransparency.Value = transparency;
            txtTransparency.Text = transparency.ToString();
        }

        private void SetTransparency(int transparency)
        {
            Color color = Color.FromArgb(TransparencyToAlpha(transparency), 
                                         callingForm.TransparencyColor);
            callingForm.TransparencyColor = color;
        }

        private void hscrlTransparency_Scroll(object sender, ScrollEventArgs e)
        {
            txtTransparency.Text = hscrlTransparency.Value.ToString();
            SetTransparency(hscrlTransparency.Value);
        }

        private void txtTransparency_TextChanged(object sender, EventArgs e)
        {
            int transparency;
            if (int.TryParse(txtTransparency.Text, out transparency))
            {
                transparency = Math.Max(0, Math.Min(100, transparency));
                hscrlTransparency.Value = transparency;
                SetTransparency(transparency);
            }
        }
    }
}
