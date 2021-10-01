using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Whorl
{
    class BitmapTools
    {
        public static Bitmap CreateFormattedBitmap(Size size)
        {
            return new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
        }

        private static void CopyBitmapToFromColorArray(
                            Bitmap bitmap, int[] colorArray, bool copyToBitmap)
        {
            int bitmapLength = bitmap.Width * bitmap.Height;
            if (colorArray.Length != bitmapLength)
                throw new Exception("colorArray must be same size as bitmap.");
            BitmapData pdata = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppPArgb);
            try
            {
                IntPtr pscan0 = pdata.Scan0;
                if (copyToBitmap)
                    Marshal.Copy(colorArray, 0, pscan0, bitmapLength);
                else
                    Marshal.Copy(pscan0, colorArray, 0, bitmapLength);
            }
            finally
            {
                bitmap.UnlockBits(pdata);
            }
        }

        public static void CopyBitmapToColorArray(Bitmap bitmap, int[] colorArray)
        {
            CopyBitmapToFromColorArray(bitmap, colorArray, copyToBitmap: false);
        }

        public static void CopyColorArrayToBitmap(Bitmap bitmap, int[] colorArray)
        {
            CopyBitmapToFromColorArray(bitmap, colorArray, copyToBitmap: true);
        }

        public static Image ScaleImage(Image sourceImage, Size destSize)
        {
            //Bitmap toReturn = new Bitmap(sourceImage, destSize);
            Bitmap toReturn = CreateFormattedBitmap(destSize);

            //toReturn.SetResolution(sourceImage.HorizontalResolution,
            //                       sourceImage.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(toReturn))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (var ia = new ImageAttributes())
                {
                    ia.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(sourceImage,
                        new Rectangle(0, 0, destSize.Width, destSize.Height),
                        0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, ia);
                }
            }
            return toReturn;
        }

    }
}
