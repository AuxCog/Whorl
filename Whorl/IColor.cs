using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public enum PathColorTypes
    {
        Center,
        Boundary
    }

    public interface IColor
    {
        Color TransparencyColor { get; set; }
    }
}
