using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    class RenderCaller: IRenderCaller
    {
        public delegate void delRenderCallback(int step, bool initial);

        public RenderCaller(delRenderCallback renderCallback)
        {
            this.renderCallback = renderCallback;
        }

        public bool CancelRender { get; set; }

        public void RenderCallback(int step, bool initial = false)
        {
            renderCallback(step, initial);
        }

        private delRenderCallback renderCallback { get; }
    }
}
