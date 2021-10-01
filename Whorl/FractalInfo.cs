using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class FractalInfo
    {
        public int[] IterArray { get; private set; }
        internal int IterIndex { get; set; }
        public double IterI
        {
            get { return (double)IterIndex; }
        }
        private Size _imgSize;
        public Size ImgSize
        {
            get { return _imgSize; }
            set
            {
                _imgSize = value;
                IterArray = new int[_imgSize.Width * _imgSize.Height];
            }
        }
        public double reStart { get; set; }
        public double imStart { get; set; }
        public double zIncX { get; set; }
        public double zIncY { get; set; }
        internal int DraftSize { get; set; }

        public double GetIter(double index)
        {
            int i = (int)index;
            return (double)(i < IterArray.Length ? IterArray[i] : 0);
        }
    }
}
