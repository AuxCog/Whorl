using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public enum ColorBlendTypes
    {
        None,
        Add,
        Average,
        Subtract,
        //Difference,
        Multiply,
        And,
        Or,
        XOr,
        Contrast
    }

    public struct IntColor
    {
        public int A;
        public int R;
        public int G;
        public int B;

        public IntColor(int a, int r, int g, int b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public IntColor(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public static int ClipColor(int colorVal)
        {
            return Math.Min(255, Math.Abs(colorVal));
        }

        public Color GetColor()
        {
            return Color.FromArgb(ClipColor(A),
                                  ClipColor(R),
                                  ClipColor(G),
                                  ClipColor(B));
        }
    }
}
