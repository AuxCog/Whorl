using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PatternBoundsInfo
    {

        public Rectangle BoundingRectangle { get ; set; }
        public Size BoundsSize { get; }
        public int PixelCount { get; }
        public uint[] BoundsBitmap { get; }
        public byte[,] BoundsArray { get; }

        public PatternBoundsInfo(Size size, int pixelCount, uint[] bitmap, byte[,] array = null)
        {
            BoundsSize = size;
            PixelCount = pixelCount;
            BoundsBitmap = bitmap;
            BoundsArray = array;
        }

    }
}
