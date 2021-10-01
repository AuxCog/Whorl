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
    public partial class CustomPicturebox : PictureBox
    {
        public CustomPicturebox()
        {
            InitializeComponent();
        }

        public bool EnablePaint { get; set; } = true;

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (EnablePaint)
                base.OnPaint(pe);
        }
    }
}
