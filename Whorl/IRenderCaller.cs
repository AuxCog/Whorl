using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public interface IRenderCaller
    {
        void RenderCallback(int step, bool initial = false);
        bool CancelRender { get; set; }
    }
}
