using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static Whorl.ColorGradient;

namespace Whorl
{
    class BlendedImage: ChangeTracker, IXml
    {
        private const int ImageExtent = 1000;  //Max of width or height for scaled bitmap.
        public class ImageInfo: ChangeTracker
        {
            public ImageInfo(BlendedImage blendedImage)
            {
                BlendedImage = blendedImage;
            }

            public BlendedImage BlendedImage { get; }
            private string _fileName;
            public string FileName 
            { 
                get { return _fileName; }
                set
                {
                    SetProperty(ref _fileName, value);
                }
            }
            public Bitmap Bitmap { get; set; }
            public Size ImageSize { get; set; }
            public Size OriginalSize { get; set; }
            public FloatColor[] FloatPixels { get; set; }

            public void ClearSettings()
            {
                FileName = null;
                if (Bitmap != null)
                {
                    Bitmap.Dispose();
                    Bitmap = null;
                }
                ImageSize = Size.Empty;
                FloatPixels = null;
            }

            protected override void IsChangedChanged()
            {
                base.IsChangedChanged();
                if (IsChanged)
                    BlendedImage.IsChanged = true;
            }
        }

        public BlendedImage()
        {
            ImageInfo1 = new ImageInfo(this);
            ImageInfo2 = new ImageInfo(this);
            BlendedImageInfo = new ImageInfo(this);
        }

        private TextBox txtBlendDepthPct { get; set; }
        private TextBox txtSlopePct { get; set; }
        private TextBox txtXOffset { get; set; }
        private TextBox txtBlendOffsetPct { get; set; }
        private TextBox txtBlendAngle { get; set; }
        private PictureBox picBlendFunction { get; set; }

        public ImageInfo ImageInfo1 { get; }
        public ImageInfo ImageInfo2 { get; }
        public ImageInfo BlendedImageInfo { get; }

        public event EventHandler IsChangedChangedEvent;

        private string imagesFolder { get; set; } = null;

        private double _depth, _slope, _xOffset, _blendOffset, _blendAngle;

        public double Depth
        {
            get { return _depth; }
            set { SetProperty(ref _depth, value); }
        }

        public double Slope
        {
            get { return _slope; }
            set { SetProperty(ref _slope, value); }
        }

        public double XOffset
        {
            get { return _xOffset; }
            set { SetProperty(ref _xOffset, value); }
        }

        public double BlendOffset
        {
            get { return _blendOffset; }
            set { SetProperty(ref _blendOffset, value); }
        }

        public double BlendAngle
        {
            get { return _blendAngle; }
            set { SetProperty(ref _blendAngle, value); }
        }

        private double BlendOffsetSetting { get; set; }

        public void ClearIsChanged()
        {
            IsChanged = false;
            ImageInfo1.IsChanged = false;
            ImageInfo2.IsChanged = false;
            BlendedImageInfo.IsChanged = false;
        }

        protected override void IsChangedChanged()
        {
            base.IsChangedChanged();
            IsChangedChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void Initialize(TextBox txtBlendDepthPct,
                               TextBox txtSlopePct,
                               TextBox txtXOffset,
                               TextBox txtBlendOffsetPct,
                               TextBox txtBlendAngle,
                               PictureBox picBlendFunction = null)
        {
            this.txtBlendDepthPct = txtBlendDepthPct;
            this.txtSlopePct = txtSlopePct;
            this.txtXOffset = txtXOffset;
            this.txtBlendOffsetPct = txtBlendOffsetPct;
            this.txtBlendAngle = txtBlendAngle;
            this.picBlendFunction = picBlendFunction;
        }

        public bool ApplyBlend(bool parseSettings)
        {
            if (parseSettings)
            {
                if (!ParseSettings())
                    return false;
            }
            Cursor cursor = Cursor.Current;
            bool success;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (picBlendFunction != null)
                    DisplayBlendFunction();
                success = ApplyBlendToImages();
            }
            finally
            {
                Cursor.Current = cursor;
            }
            return success;
        }

        private bool ParseSettings()
        {
            if (!double.TryParse(txtBlendDepthPct.Text, out double depth))
                return false;
            if (!double.TryParse(txtSlopePct.Text, out double slope))
                return false;
            if (!double.TryParse(txtXOffset.Text, out double xOffset))
                return false;
            if (!double.TryParse(txtBlendOffsetPct.Text, out double blendOff))
                return false;
            if (!double.TryParse(txtBlendAngle.Text, out double blendAngle))
                return false;
            if (depth > 100.0 || depth < 0.0)
            {
                depth = Math.Min(100.0, Math.Max(0.0, depth));
                txtBlendDepthPct.Text = depth.ToString();
            }
            if (blendOff > 100.0 || blendOff < 0.0)
            {
                blendOff = Math.Min(100.0, Math.Max(0.0, blendOff));
                txtBlendOffsetPct.Text = blendOff.ToString();
            }
            depth *= 0.01;
            slope *= 0.1;   //Multiply by 10.
            xOffset *= 0.01;
            blendOff *= 0.01;
            Depth = depth;
            Slope = slope;
            XOffset = xOffset;
            BlendOffsetSetting = blendOff;
            BlendAngle = Tools.DegreesToRadians(blendAngle);
            ComputeBlendOffset();
            return true;
        }

        private void ComputeBlendOffset()
        {
            BlendOffset = (1.0 - 2.0 * BlendOffsetSetting) * (1.0 - Depth);
        }

        private bool ApplyBlendToImages()
        {
            if (ImageInfo1.FloatPixels == null || ImageInfo2.FloatPixels == null)
                return false;
            if (ImageInfo1.ImageSize != ImageInfo2.ImageSize)
                throw new Exception("Image sizes not equal.");
            Size imgSize = ImageInfo1.ImageSize;
            BlendedImageInfo.ImageSize = imgSize;
            if (BlendedImageInfo.FloatPixels == null ||
                BlendedImageInfo.FloatPixels.Length != ImageInfo1.FloatPixels.Length)
            {
                BlendedImageInfo.FloatPixels = new FloatColor[ImageInfo1.FloatPixels.Length];
            }
            int maxExtent = Math.Max(imgSize.Width, imgSize.Height);
            int midX = maxExtent / 2;
            double xIncrement = 1.0 / midX;
            double dX = -xIncrement * midX;
            float[] factors = new float[maxExtent];
            for (int x = 0; x < maxExtent; x++)
            {
                factors[x] = 0.5F * (1F + (float)ComputeBlend(dX));
                dX += xIncrement;
            }
            Complex zVector = Complex.CreateFromModulusAndArgument(1.0, BlendAngle + 0.5 * Math.PI);
            Complex zInverse = Complex.One / zVector;
            double maxDistance;
            if (BlendAngle == 0)
                maxDistance = 0;
            else
            {
                var zCorners = new Complex[]
                {
                new Complex(0, 0),
                new Complex(imgSize.Width, 0),
                new Complex(imgSize.Width, imgSize.Height),
                new Complex(0, imgSize.Height)
                };
                maxDistance = zCorners.Select(z => Math.Abs((z * zInverse).Im)).Max();
            }
            int i = 0;
            for (int y = 0; y < imgSize.Height; y++)
            {
                for (int x = 0; x < imgSize.Width; x++)
                {
                    int index;
                    if (BlendAngle == 0)
                    {
                        index = x * maxExtent / imgSize.Width;
                    }
                    else
                    {
                        Complex zP = new Complex(x, y) * zInverse;   //Subtract angle.
                        double distance = Math.Abs(zP.Im);
                        if (distance > maxDistance)
                            throw new Exception("distance is out of range.");
                        index = (int)(distance / maxDistance * maxExtent);
                    }
                    BlendedImageInfo.FloatPixels[i] = FloatColor.InterpolateFloatColor(
                        ImageInfo2.FloatPixels[i], ImageInfo1.FloatPixels[i], factors[index]);
                    i++;
                }
            }
            if (BlendedImageInfo.Bitmap != null && BlendedImageInfo.Bitmap.Size != imgSize)
            {
                BlendedImageInfo.Bitmap.Dispose();
                BlendedImageInfo.Bitmap = null;
            }
            if (BlendedImageInfo.Bitmap == null)
                BlendedImageInfo.Bitmap = BitmapTools.CreateFormattedBitmap(imgSize);
            int[] colorArray = BlendedImageInfo.FloatPixels.Select(
                               fc => FloatColor.GetColor(fc).ToArgb()).ToArray();
            BitmapTools.CopyColorArrayToBitmap(BlendedImageInfo.Bitmap, colorArray);
            return true;
        }

        private double ComputeBlend(double x)
        {
            return -Depth * Math.Tanh(Slope * (x - XOffset)) - BlendOffset;
        }

        private void DisplayBlendFunction()
        {
            const int deltaX = 2;
            var img = new Bitmap(picBlendFunction.ClientSize.Width, picBlendFunction.ClientSize.Height);
            Point center = new Point(img.Width / 2, img.Height / 2);
            double xFactor = 2.0 / img.Width;
            double yFactor = 0.5 * img.Height;
            var points = new List<Point>();
            for (int x = 0; x < img.Width; x += deltaX)
            {
                double dX = xFactor * (x - center.X);
                double dY = -ComputeBlend(dX);
                int y = center.Y + (int)(yFactor * dY);
                points.Add(new Point(x, y));
            }
            using (Graphics g = Graphics.FromImage(img))
            {
                g.DrawLines(Pens.Red, points.ToArray());
                DisplayBlendAngle(g, img.Size);
            }
            if (picBlendFunction.Image != null)
                picBlendFunction.Image.Dispose();
            picBlendFunction.Image = img;
        }

        private void DisplayBlendAngle(Graphics g, Size imgSize)
        {
            PointF center = new PointF(imgSize.Width / 2, imgSize.Height / 2);
            float arrowLength = Math.Min(center.X, center.Y) - 3F;
            PointF[] arrowPoints = new PointF[]
            {
                new PointF(0, 0),
                new PointF(arrowLength, 0),
                new PointF(arrowLength - 3F, 3F),
                new PointF(arrowLength, 0),
                new PointF(arrowLength - 3F, -3F)
            };
            var zVector = Complex.CreateFromModulusAndArgument(1.0, BlendAngle);
            var pRotation = new PointF((float)zVector.Re, (float)zVector.Im);
            var arrow = Tools.RotatePoints(arrowPoints, pRotation)
                             .Select(p => Tools.AddPoint(p, center))
                             .Select(p => new PointF(p.X, imgSize.Height - p.Y));
            g.DrawLines(Pens.Blue, arrow.ToArray());

            Complex zInverse = Complex.One / zVector;
            PointF pA = new PointF(center.X + 20, 20);
            Complex zCenter = new Complex(center.X, center.Y);
            Complex zP = new Complex(pA.X, pA.Y) - zCenter;
            Complex rotatedZP = zP * zInverse;
            double distance = Math.Abs(rotatedZP.Im);
            double length = rotatedZP.Re;
            Complex p1 = length * zVector;
            Complex zDiff = zP - p1;
            zDiff.Normalize();
            zDiff *= distance;
            p1 += zCenter;
            Complex p2 = p1 + zDiff;
            PointF[] lines = new PointF[]
            {
                new PointF((float)p1.Re, imgSize.Height - (float)p1.Im),
                new PointF((float)p2.Re, imgSize.Height - (float)p2.Im),
            };
            g.DrawLines(Pens.Black, lines);
            Tools.DrawSquare(g, Color.Green, new PointF(pA.X, imgSize.Height - pA.Y), size: 2);
        }

        public Bitmap OpenImage(ImageInfo imageInfo, string fileName, Size? scaledSize = null,
                                int imageExtent = ImageExtent)
        {
            if (imageInfo.FileName == fileName && imageInfo.FloatPixels != null
                && (scaledSize == null || imageInfo.ImageSize == scaledSize))
            {
                return imageInfo.Bitmap;  //Still valid.
            }
            Cursor cursor = Cursor.Current;
            Bitmap img;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                imageInfo.FileName = fileName;
                Size size;
                using (var bitmapTemp = new Bitmap(fileName))
                {
                    //Get scaled, formatted bitmap with max extent == ImageExtent:
                    if (scaledSize != null)
                        size = (Size)scaledSize;  //Use size of first image.
                    else
                    {
                        if (bitmapTemp.Width > bitmapTemp.Height)
                        {
                            size = new Size(imageExtent,
                                            imageExtent * bitmapTemp.Height / bitmapTemp.Width);
                        }
                        else
                        {
                            size = new Size(imageExtent * bitmapTemp.Width / bitmapTemp.Height,
                                            imageExtent);
                        }
                    }
                    imageInfo.OriginalSize = bitmapTemp.Size;
                    if (bitmapTemp.Size == size)
                        img = new Bitmap(bitmapTemp);
                    else
                        img = (Bitmap)BitmapTools.ScaleImage(bitmapTemp, size);
                    if (imageInfo.Bitmap != null)
                        imageInfo.Bitmap.Dispose();
                    imageInfo.Bitmap = img;
                    imageInfo.ImageSize = size;
                }
                int[] colorArray = new int[img.Width * img.Height];
                BitmapTools.CopyBitmapToColorArray(img, colorArray);
                imageInfo.FloatPixels = colorArray.Select(
                          i => new FloatColor(Color.FromArgb(i))).ToArray();
            }
            finally
            {
                Cursor.Current = cursor;
            }
            return img;
        }

        public Bitmap OpenImage(ImageInfo imageInfo, Size? scaledSize = null)
        {
            Bitmap img = null;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image file (*.jpg;*.png)|*.jpg;*.jpeg;*.png";
                if (imagesFolder != null)
                    dlg.InitialDirectory = imagesFolder;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imagesFolder = Path.GetDirectoryName(dlg.FileName);
                    img = OpenImage(imageInfo, dlg.FileName, scaledSize);
                }
            }
            return img;
        }

        public string ValidateForSave()
        {
            string message = null;
            if (ImageInfo1.FloatPixels == null)
                message = "Please browse to Image 1.";
            else if (ImageInfo2.FloatPixels == null)
                message = "Please browse to Image 2.";
            return message;
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
                xmlNodeName = nameof(BlendedImage);
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            //        private double depth, slope, xOffset, blendOffset;
            xmlTools.AppendXmlAttribute(node, "Depth", Depth);
            xmlTools.AppendXmlAttribute(node, "Slope", Slope);
            xmlTools.AppendXmlAttribute(node, "XOffset", XOffset);
            xmlTools.AppendXmlAttribute(node, "BlendOffset", BlendOffsetSetting);
            xmlTools.AppendXmlAttribute(node, "BlendAngle", BlendAngle);
            XmlNode childNode = xmlTools.CreateXmlNode("Image1FilePath");
            XmlTools.SetNodeValue(childNode, ImageInfo1.FileName);
            node.AppendChild(childNode);
            childNode = xmlTools.CreateXmlNode("Image2FilePath");
            XmlTools.SetNodeValue(childNode, ImageInfo2.FileName);
            node.AppendChild(childNode);
            ClearIsChanged();
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            Depth = Tools.GetXmlAttribute<double>(node, "Depth");
            Slope = Tools.GetXmlAttribute<double>(node, "Slope");
            XOffset = Tools.GetXmlAttribute<double>(node, "XOffset");
            BlendOffsetSetting = Tools.GetXmlAttribute<double>(node, "BlendOffset");
            BlendAngle = Tools.GetXmlAttribute<double>(node, 0.0, "BlendAngle");
            ComputeBlendOffset();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "Image1FilePath")
                    ImageInfo1.FileName = Tools.GetXmlNodeValue(childNode);
                else if (childNode.Name == "Image2FilePath")
                    ImageInfo2.FileName = Tools.GetXmlNodeValue(childNode);
            }
            DisplayPercentage(txtBlendDepthPct, Depth);
            DisplayPercentage(txtSlopePct, 0.1 * Slope);  //Slope is multiplied by 10 when parsed.
            DisplayPercentage(txtXOffset, XOffset);
            DisplayPercentage(txtBlendOffsetPct, BlendOffsetSetting);
            txtBlendAngle.Text = Tools.RadiansToDegrees(BlendAngle).ToString("0.#");
            OpenImage(ImageInfo1, ImageInfo1.FileName);
            OpenImage(ImageInfo2, ImageInfo2.FileName, ImageInfo1.ImageSize);
            ClearIsChanged();
        }

        private void DisplayPercentage(TextBox textBox, double val)
        {
            textBox.Text = (100.0 * val).ToString("0.#");
        }
    }
}
