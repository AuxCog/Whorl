using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class ImageModifier
    {
        public enum ColorModes
        {
            Set,
            Add,
            Subtract,
            Multiply
        }
        public enum BoundModes
        {
            Inside,
            Outside,
            Boundary
        }

        private Bitmap _imageBitmap;
        public Bitmap ImageBitmap 
        {
            get => _imageBitmap;
            set
            {
                if (_imageBitmap != null)
                    _imageBitmap.Dispose();
                _imageBitmap = value;
            }
        }

        public Color ModifiedColor { get; set; }

        public void ModifyColors(WhorlDesign design, ColorModes colorMode, BoundModes boundMode, 
                                 IEnumerable<Pattern> outlinePatterns, float scale) //, Size pictureBoxSize)
        {
            if (ImageBitmap == null)
                throw new NullReferenceException("ImageBitmap cannot be null.");
            ColorGradient.FloatColor floatColor, setColor;
            if (colorMode != ColorModes.Set)
            {
                floatColor = new ColorGradient.FloatColor();
                floatColor.SetFromColor(ModifiedColor);
                if (colorMode == ColorModes.Multiply)
                {
                    floatColor.Red /= 255F;
                    floatColor.Green /= 255F;
                    floatColor.Blue /= 255F;
                }
            }
            else
                floatColor = null;
            setColor = new ColorGradient.FloatColor();
            using (var mergedPattern = new MergedPattern(design))
            {
                mergedPattern.SetRawPatterns(outlinePatterns);
                //PointF picCenter = new PointF(0.5F * pictureBoxSize.Width, 0.5F * pictureBoxSize.Height);
                mergedPattern.ScaleRawPatterns(scale); // , picCenter);
                mergedPattern.GetBoundsPixels();
                var pixArray = new int[ImageBitmap.Width * ImageBitmap.Height];
                BitmapTools.CopyBitmapToColorArray(ImageBitmap, pixArray);
                int modArgb = ModifiedColor.ToArgb();
                for (int y = 0; y < ImageBitmap.Height; y++)
                {
                    for (int x = 0; x < ImageBitmap.Width; x++)
                    {
                        Point p = new Point(x - mergedPattern.BoundingRect.X,
                                            y - mergedPattern.BoundingRect.Y);
                        bool changeColor;
                        switch (boundMode)
                        {
                            case BoundModes.Outside:
                            default:
                                changeColor = !mergedPattern.IsPatternPixel(p);
                                break;
                            case BoundModes.Inside:
                                changeColor = mergedPattern.IsPatternPixel(p);
                                break;
                            case BoundModes.Boundary:
                                changeColor = mergedPattern.IsBoundaryPoint(p);
                                break;
                        }
                        if (changeColor)
                        {
                            int pixInd = y * ImageBitmap.Width + x;
                            if (colorMode == ColorModes.Set)
                                pixArray[pixInd] = modArgb;
                            else
                            {
                                Color color = Color.FromArgb(pixArray[pixInd]);
                                setColor.SetFromColor(color);
                                switch (colorMode)
                                {
                                    case ColorModes.Add:
                                    case ColorModes.Subtract:
                                        float sign = colorMode == ColorModes.Subtract ? -1F : 1F;
                                        setColor.Red += sign * floatColor.Red;
                                        setColor.Green += sign * floatColor.Green;
                                        setColor.Blue += sign * floatColor.Blue;
                                        break;
                                    case ColorModes.Multiply:
                                        setColor.Red *= floatColor.Red;
                                        setColor.Green *= floatColor.Green;
                                        setColor.Blue *= floatColor.Blue;
                                        break;
                                }
                                color = ColorGradient.FloatColor.GetColor(setColor);
                                pixArray[pixInd] = color.ToArgb();
                            }
                        }
                    }
                }
                BitmapTools.CopyColorArrayToBitmap(ImageBitmap, pixArray);
            }
        }
    }
}
