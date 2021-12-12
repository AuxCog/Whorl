using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class DrawnPoint
    {
        public PointF Location { get; set; }
        public string IdText { get; set; }
        public bool Selected { get; set; }

        public void Draw(Graphics g, Bitmap designBitmap, Font font)
        {
            Draw(g, designBitmap, font, Location, IdText, Selected);
        }

        public static void Draw(Graphics g, Bitmap designBitmap, Font font, PointF p, string idText = null, bool selected = false)
        {
            const float crossWidth = 1F;
            const float rectWidth = 2F;
            Color penColor = Color.Black;
            if (designBitmap != null)
            {
                int pX = (int)p.X,
                    pY = (int)p.Y;
                if (pX >= 0 && pY >= 0 && pX < designBitmap.Width && pY < designBitmap.Height)
                {
                    if (!Tools.ColorIsLight(designBitmap.GetPixel(pX, pY)))
                        penColor = Color.White;
                }
            }
            using (var pen = new Pen(penColor))
            {
                var rectF = new RectangleF(new PointF(p.X - crossWidth, p.Y - crossWidth),
                                           new SizeF(rectWidth, rectWidth));
                if (!string.IsNullOrEmpty(idText))
                {
                    SizeF textSize = g.MeasureString(idText, font);
                    //Draw IdText:
                    using (var brush = new SolidBrush(penColor))
                    {
                        g.DrawString(idText, font, brush, new PointF(rectF.Left - textSize.Width, rectF.Top));
                    }
                }
                if (selected)
                {
                    //Draw a filled circle:
                    using (var brush = new SolidBrush(penColor))
                    {
                        g.FillEllipse(brush, new RectangleF(new PointF(p.X - 2, p.Y - 2), new SizeF(4, 4)));
                    }
                }
                else
                {
                    //Draw a cross at point's location:
                    g.DrawLine(pen,
                               new PointF(rectF.Left, p.Y),
                               new PointF(rectF.Right, p.Y));
                    g.DrawLine(pen,
                               new PointF(p.X, rectF.Top),
                               new PointF(p.X, rectF.Bottom));
                }
            }
        }
    }
}
