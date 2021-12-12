using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Security;
using System.Drawing.Imaging;
using ParserEngine;

namespace Whorl
{
    public static class Tools
    {
        public static void HandleException(Exception ex)
        {
            try
            {
                //if (ex is OutOfMemoryException)
                //{
                //    MessageBox.Show(ex.Message);
                //    Environment.Exit(1);
                //    return;
                //}
                //if (ex.Message == lastExceptionMessage)
                //    return;
                //lastExceptionMessage = ex.Message;
                StringBuilder sb = new StringBuilder();
                for (Exception ex1 = ex; ex1 != null; ex1 = ex1.InnerException)
                {
                    sb.AppendLine(ex1.Message);
                }
                if (!(ex is CustomException))
                    sb.AppendLine(ex.StackTrace);
                string message = sb.ToString();
                //if (message.Length > 200)
                //    message = message.Substring(0, 200);
                MessageBox.Show(message);
            }
            catch { }
        }

        public static void AsyncTaskFailed(Task task)
        {
            if (task.Exception != null)
                Tools.HandleException(task.Exception);
        }

        public static void DrawCurve(Graphics g, Pen pen, PointF[] points)
        {
            if (points.Length > 1)
                g.DrawCurve(pen, points);
        }

        public static double DistanceSquared(PointF p1, PointF p2)
        {
            PointF pDiff = new PointF(p1.X - p2.X, p1.Y - p2.Y);
            return pDiff.X * pDiff.X + pDiff.Y * pDiff.Y;
        }

        public static double Square(double x)
        {
            return x * x;
        }

        public static double GetModulus(PointF p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static double Distance(PointF p1, PointF p2)
        {
            PointF pDiff = new PointF(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(pDiff.X * pDiff.X + pDiff.Y * pDiff.Y);
        }

        public static PointF AddPoint(PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static PointF SubtractPoint(PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static double PathLength(PointF[] points, int startIndex, int segmentsCount)
        {
            return segmentsCount <= 0 || points.Length <= startIndex + segmentsCount ? 0 :
                   Enumerable.Range(startIndex, segmentsCount).Select(i => Distance(points[i], points[i + 1])).Sum();
        }

        public static double PathLength(List<PointF> points, int startIndex, int segmentsCount)
        {
            return segmentsCount <= 0 || points.Count <= startIndex + segmentsCount ? 0 :
                   Enumerable.Range(startIndex, segmentsCount).Select(i => Distance(points[i], points[i + 1])).Sum();
        }

        public static double PathLength(PointF[] points)
        {
            return PathLength(points, 0, points.Length - 1);
            //return points.Length < 2 ? 0 : Enumerable.Range(1, points.Length - 1).Select(i => Distance(points[i - 1], points[i])).Sum();
        }

        public static double PathLength(List<PointF> points)
        {
            return PathLength(points, 0, points.Count - 1);
            //return points.Count < 2 ? 0 : Enumerable.Range(1, points.Count - 1).Select(i => Distance(points[i - 1], points[i])).Sum();
        }

        public static PointF Centroid(IEnumerable<PointF> polygonPoints)
        {
            if (polygonPoints.Count() == 0)
                return PointF.Empty;
            else
                return new PointF(
                    polygonPoints.Select(p => p.X).Average(),
                    polygonPoints.Select(p => p.Y).Average());
        }

        public static bool PointsDiffer(PointF p1, PointF p2)
        {
            return Math.Round(p1.X) != Math.Round(p2.X) ||
                   Math.Round(p1.Y) != Math.Round(p2.Y);
        }

        public static List<PointF> DistinctPoints(PointF[] points)
        {
            List<PointF> distinctPoints = new List<PointF>();
            if (points.Length == 0) return distinctPoints;
            distinctPoints.Add(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                if (PointsDiffer(distinctPoints[distinctPoints.Count - 1], points[i]))
                {
                    distinctPoints.Add(points[i]);
                }
            }
            return distinctPoints;
        }

        public static void ClosePoints(List<PointF> points)
        {
            if (points == null || points.Count == 0)
                throw new Exception("Tools.ClosePoints: points are null or empty.");
            PointF firstVertex = points.First();
            if (points.Last() != firstVertex)
                points.Add(firstVertex);
        }

        public static List<PointF> GetCorners(Rectangle rectangle)
        {
            return new List<PointF>()
            {
                new PointF(rectangle.Left, rectangle.Top),
                new PointF(rectangle.Right, rectangle.Top),
                new PointF(rectangle.Right, rectangle.Bottom),
                new PointF(rectangle.Left, rectangle.Bottom)
            };
        }

        public static PointF ScalePoint(PointF p, float scaleFac, PointF center)
        {
            return new PointF(center.X + scaleFac * (p.X - center.X),
                              center.Y + scaleFac * (p.Y - center.Y));
        }

        public static PointF GetRotationVector(double angle)
        {
            return new PointF((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static PointF RotatePoint(PointF p, double angle)
        {
            PointF rotationVector = GetRotationVector(angle);
            return RotatePoint(p, rotationVector);
        }

        public static PointF RotatePoint(PointF p, PointF rotationVector)
        {
            return new PointF(p.X * rotationVector.X - p.Y * rotationVector.Y,
                              p.X * rotationVector.Y + p.Y * rotationVector.X);
        }

        public static IEnumerable<PointF> TranslatePoints(IEnumerable<PointF> points, PointF pDelta)
        {
            return points.Select(p => new PointF(p.X + pDelta.X, p.Y + pDelta.Y));
        }

        public static IEnumerable<PointF> RotatePoints(IEnumerable<PointF> points, PointF rotationVector)
        {
            return points.Select(p => RotatePoint(p, rotationVector));
        }


        /// <summary>
        /// Find closest point on line segment (A, B) to point P.
        /// </summary>
        /// <param name="P"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static PointF ClosestPointToSegment(PointF P, PointF A, PointF B)
        {
            var v = new PointF(B.X - A.X, B.Y - A.Y);
            var u = new PointF(A.X - P.X, A.Y - P.Y);
            float vu = v.X * u.X + v.Y * u.Y;
            float vv = VectorLength(v);
            float t = -vu / vv;
            if (t >= 0 && t <= 1)
                return VectorToSegment2D(t, PointF.Empty, A, B);
            float g0 = VectorLength(VectorToSegment2D(0, P, A, B));
            float g1 = VectorLength(VectorToSegment2D(1, P, A, B));
            return g0 <= g1 ? A : B;
        }

        private static PointF VectorToSegment2D(float t, PointF P, PointF A, PointF B)
        {
            return new PointF((1 - t) * A.X + t * B.X - P.X,
                              (1 - t) * A.Y + t * B.Y - P.Y);
        }

        public static float VectorLength(PointF P)
        {
            return P.X * P.X + P.Y * P.Y;
        }

        //Adjust angle to be >= 0.
        public static double AdjustAngle(double angle)
        {
            while (angle < 0)
                angle += 2D * Math.PI;
            return angle;
        }

        public static bool InBounds(Rectangle boundsRect, Point p)
        {
            return boundsRect.Contains(p);
        }

        public static RectangleF RectangleFromVertices(PointF topLeft, PointF bottomRight)
        {
            return new RectangleF(topLeft, new SizeF(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
        }

        public static RectangleF GetBoundingRectangle(IEnumerable<PointF> points)
        {
            if (!points.Any())
                return new RectangleF(0, 0, 0, 0);
            float xMin = points.Select(p => p.X).Min();
            float xMax = points.Select(p => p.X).Max();
            float yMin = points.Select(p => p.Y).Min();
            float yMax = points.Select(p => p.Y).Max();
            return RectangleFromVertices(new PointF(xMin, yMin), new PointF(xMax, yMax));
        }

        public static double DegreesToRadians(double degrees)
        {
            return (Math.PI / 180.0) * degrees;
        }

        public static double RadiansToDegrees(double radians)
        {
            return (180.0 / Math.PI) * radians;
        }

        public static Color InverseColor(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }

        public static bool ColorIsLight(Color color)
        {
            return (color.R + color.G + color.B > 300);
        }

        public static float Interpolate(float n1, float n2, float factor)
        {
            return n1 + factor * (n2 - n1);
        }

        public static Color InterpolateColor(Color color1, Color color2, float factor)
        {
            return Color.FromArgb((int)Interpolate(color1.A, color2.A, factor),
                                  (int)Interpolate(color1.R, color2.R, factor),
                                  (int)Interpolate(color1.G, color2.G, factor),
                                  (int)Interpolate(color1.B, color2.B, factor));
        }

        public static List<PointF> InterpolatePoints(PointF[] points)
        {
            const double distMin = 4D;
            var iPoints = new List<PointF>();
            if (points.Length == 0)
                return iPoints;
            PointF p1 = points[0];
            iPoints.Add(p1);
            for (int i = 1; i < points.Length; i++)
            {
                PointF p2 = points[i];
                PointF diff = new PointF(p2.X - p1.X, p2.Y - p1.Y);
                double distSquared = diff.X * diff.X + diff.Y * diff.Y;
                if (distSquared < distMin)
                {
                    iPoints.Add(p2);
                    p1 = p2;
                }
                else
                {
                    float dist = (float)Math.Sqrt(distSquared);
                    int steps = (int)Math.Floor(dist);
                    diff.X /= dist;
                    diff.Y /= dist;
                    for (int j = 0; j < steps; j++)
                    {
                        p1.X += diff.X;
                        p1.Y += diff.Y;
                        iPoints.Add(p1);
                    }
                }
            }
            return iPoints;
        }

        public static void DrawSquare(Graphics g, Color color, PointF p, int size = 3)
        {
            using (Brush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, p.X - size, p.Y - size, size * 2 + 1, size * 2 + 1);
            }
        }

        /// <summary>
        /// Return True if type passed is a Nullable type.
        /// </summary>
        /// <param name="myType"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetTypeOrUnderlyingType(Type type)
        {
            return IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;
        }

        public static object IfDbNull(object val, object valIfDbNull)
        {
            return val == DBNull.Value ? valIfDbNull : val;
        }

        public static double IncrementValue(double val, double increment)
        {
            return val >= 0 ? val + increment : val - increment;
        }

        public static void CopyProperties(object targetObject, object sourceObject,
                                          HashSet<string> excludedPropertyNames)
        {
            bool typesEqual = targetObject.GetType() == sourceObject.GetType();
            foreach (PropertyInfo prpSource in sourceObject.GetType().GetProperties())
            {
                if (excludedPropertyNames.Contains(prpSource.Name))
                    continue;
                PropertyInfo prpTarget;
                if (typesEqual)
                    prpTarget = prpSource;
                else
                    prpTarget = targetObject.GetType().GetProperty(prpSource.Name);
                if (prpTarget != null && prpTarget.CanWrite)
                {
                    object oVal = prpSource.GetValue(sourceObject);
                    var cloneVal = oVal as ICloneable;
                    if (cloneVal != null)
                        oVal = cloneVal.Clone();
                    prpTarget.SetValue(targetObject, oVal);
                }
            }
        }

        public static void CopyProperties(object targetObject, object sourceObject)
        {
            CopyProperties(targetObject, sourceObject, new HashSet<string>());
        }

        public static void CopyProperties(object targetObject, object sourceObject,
                                          string[] excludedPropertyNames)
        {
            HashSet<string> excludedProps = new HashSet<string>(excludedPropertyNames);
            CopyProperties(targetObject, sourceObject, excludedProps);
        }

        /// <summary>
        /// Retrieves value from DataRow, where val could be DBNull.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static T GetDBValue<T>(object val, T defaultVal)
        {
            if (val is T)
                return (T)val;
            else
                return defaultVal;
        }

        public static void SavePngOrJpegImageFile(string fileName, Bitmap bitmap)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            System.Drawing.Imaging.ImageFormat fmt =
                System.Drawing.Imaging.ImageFormat.Jpeg;
            if (extension == ".png")
            {
                bitmap.Save(fileName, ImageFormat.Png);
            }
            else
            {
                //Save as jpeg with quality 100%:
                var encoder = ImageCodecInfo.GetImageEncoders().First(
                              c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encParams = new EncoderParameters() {
                    Param = new[] { new EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, 100L) } };
                bitmap.Save(fileName, encoder, encParams);
            }
        }

        public static uint GetBitmapBit(int i)
        {
            return 1u << (i & 0x1F);
        }

        public static uint[] GetBitmap(int[] array, Predicate<int> predicate)
        {
            uint[] bitmap = new uint[1 + (array.Length >> 5)];
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate.Invoke(array[i]))
                {
                    int ind = i >> 5;
                    //uint bit = 1u << (i & 0x1FF);
                    bitmap[ind] |= GetBitmapBit(i);
                }
            }
            return bitmap;
        }

        public static bool BitIsSet(uint[] bitmap, int bitInd)
        {
            return (bitmap[bitInd >> 5] & GetBitmapBit(bitInd)) != 0;
        }

        public static void SetBit(uint[] bitmap, int bitInd)
        {
            bitmap[bitInd >> 5] |= GetBitmapBit(bitInd);
        }

        public static void ClearBit(uint[] bitmap, int bitInd)
        {
            bitmap[bitInd >> 5] &= ~GetBitmapBit(bitInd);
        }

        public static Dictionary<string, T> GetCaseInsensitiveEnumDictionary<T>() where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new Exception($"{typeof(T).FullName} is not an enum type.");
            var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (T val in Enum.GetValues(typeof(T)))
            {
                dict.Add(val.ToString(), val);
            }
            return dict;
        }

        public static object GetCSharpParameterValue(string text, Type paramType, out bool isValid)
        {
            object val;
            MethodInfo tryParseMethod = paramType.GetTryParseMethod();
            if (tryParseMethod != null)
            {
                var oParams = new object[] { text, null };
                isValid = (bool)tryParseMethod.Invoke(null, oParams);
                if (isValid)
                    val = oParams[1];
                else
                    val = null;
            }
            else
            {
                try
                {
                    val = Convert.ChangeType(text, paramType);
                    isValid = true;
                }
                catch
                {
                    val = null;
                    isValid = false;
                }
            }
            return val;
        }

        //public static string GetXml(Point point, string nodeName)
        //{
        //    return "<" + nodeName + " X='" + point.X.ToString() + "' Y='" + point.Y.ToString() + "'/>";
        //}

        //public static string GetXml(PointF point, string nodeName)
        //{
        //    return "<" + nodeName + " X='" + point.X.ToString() + "' Y='" + point.Y.ToString() + "'/>";
        //}

        //public static string GetXml(Size size, string nodeName)
        //{
        //    return $"<{nodeName} Width='{size.Width}' Height='{size.Height}'/>";
        //}

        //public static string GetXml(Complex z, string nodeName)
        //{
        //    return "<" + nodeName + " Re='" + z.Re.ToString() + "' Im='" + z.Im.ToString() + "'/>";
        //}

        //public static string GetXml(Color color, string nodeName)
        //{
        //    return "<" + nodeName + " Color='" + color.ToArgb().ToString() +  "'/>";
        //}

        public static Point GetPointFromXml(XmlNode node)
        {
            return new Point((int)GetXmlAttribute("X", typeof(int), node), (int)GetXmlAttribute("Y", typeof(int), node));
        }

        public static PointF GetPointFFromXml(XmlNode node)
        {
            return new PointF((float)GetXmlAttribute("X", typeof(float), node),
                              (float)GetXmlAttribute("Y", typeof(float), node));
        }

        public static Size GetSizeFromXml(XmlNode node)
        {
            return new Size((int)GetXmlAttribute("Width", typeof(int), node),
                            (int)GetXmlAttribute("Height", typeof(int), node));
        }

        public static Complex GetComplexFromXml(XmlNode node)
        {
            return new Complex((double)GetXmlAttribute("Re", typeof(double), node),
                               (double)GetXmlAttribute("Im", typeof(double), node));
        }

        public static Color GetColorFromXml(XmlNode node)
        {
            return Color.FromArgb((int)GetXmlAttribute("Color", typeof(int), node));
        }

        //public static void AppendXmlAttribute(StringBuilder sb, string attrName, object attrValue)
        //{
        //    if (attrValue != null)
        //    {
        //        sb.Append(" " + attrName + "='" + attrValue.ToString() + "'");
        //    }
        //}

        //public static void AppendXmlAttributes(object o, StringBuilder sb, 
        //                                       string[] excludedProperties = null)
        //{
        //    foreach (PropertyInfo prp in o.GetType().GetProperties())
        //    {
        //        if (excludedProperties != null && excludedProperties.Contains(prp.Name))
        //            continue;
        //        if (!(prp.CanRead && prp.CanWrite))
        //            continue;
        //        object prpVal = prp.GetValue(o);
        //        if (prpVal is String || prpVal is Enum ||
        //           (prpVal != null && prpVal.GetType().IsPrimitive))
        //            sb.Append($" {prp.Name}='{prpVal}'");
        //    }
        //}

        //public static void AppendXmlNode(StringBuilder sb, string nodeName, string nodeValue)
        //{
        //    nodeValue = SecurityElement.Escape(nodeValue);
        //    sb.AppendLine($"<{nodeName}>{nodeValue}</{nodeName}>");
        //}
        //public static void AppendXmlAttributes(object o, StringBuilder sb, string[] propertyNames)
        //{
        //    foreach (string prpName in propertyNames)
        //    {
        //        PropertyInfo prp = o.GetType().GetProperty(prpName);
        //        if (prp == null)
        //            throw new Exception("Property " + prpName + " was not found.");
        //        object prpVal = prp.GetValue(o);
        //        if (prpVal != null)
        //            sb.Append(" " + prp.Name + "='" + prpVal.ToString() + "'");
        //    }
        //}

        public static void GetXmlAttributes(object o, XmlNode node, params string[] propertyNames)
        {
            foreach (string propName in propertyNames)
            {
                XmlAttribute attr = node.Attributes[propName];
                if (attr != null)
                {
                    PropertyInfo prp = o.GetType().GetProperty(propName);
                    if (prp != null && prp.CanWrite)
                    {
                        object val = ConvertXmlAttribute(attr, prp.PropertyType);
                        prp.SetValue(o, val);
                    }
                    else
                        throw new Exception($"Invalid property name {propName} specified.");
                }
            }
        }

        public static void GetAllXmlAttributes(object o, XmlNode node)
        {
            GetXmlAttributesExcept(o, node);
        }

        public static void GetXmlAttributesExcept(object o, XmlNode node,
                                                  params string[] excludedPropertyNames)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                if (excludedPropertyNames.Contains(attr.Name))
                    continue;
                PropertyInfo prp = o.GetType().GetProperty(attr.Name);
                if (prp != null)
                {
                    if (prp.CanWrite && !prp.SetMethod.IsStatic)
                    {
                        object val = ConvertXmlAttribute(attr, prp.PropertyType);
                        prp.SetValue(o, val);
                    }
                }
                else
                    throw new Exception($"Invalid XML attribute {attr.Name} found.");
            }
        }

        public static EnumT GetEnumXmlAttr<EnumT>(XmlNode node, string attrName,
                                                  EnumT defaultValue) where EnumT : struct
        {
            XmlAttribute attr = node.Attributes[attrName];
            EnumT val;
            if (attr == null || !Enum.TryParse(attr.Value, out val))
                val = defaultValue;
            return val;
        }

        public static string GetXmlNodeValue(XmlNode xmlNode)
        {
            if (xmlNode.FirstChild == null)
                return null;
            return xmlNode.FirstChild.Value;
        }

        public static object ConvertXmlAttribute(XmlAttribute attr, Type targetType)
        {
            try
            {
                object oVal;
                if (targetType == typeof(string))
                    oVal = attr.Value;
                else
                {
                    targetType = GetTypeOrUnderlyingType(targetType);
                    if (targetType.IsEnum)
                        oVal = Enum.Parse(targetType, attr.Value);
                    else
                        oVal = Convert.ChangeType(attr.Value, targetType);
                }
                return oVal;
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting XML attribute " + attr.Name, ex);
            }
        }

        public static float GetXmlVersion(XmlNode node, float defaultValue = 0F)
        {
            return GetXmlAttribute(node, defaultValue, "XmlVersion");
        }

        public static void SetXmlVersion(XmlNode node, XmlTools xmlTools)
        {
            xmlTools.AppendXmlAttribute(node, "XmlVersion", WhorlDesign.CurrentXmlVersion);
        }

        public static T GetXmlAttribute<T>(XmlNode node, string attrName = "Value")
        {
            return (T)GetXmlAttribute(attrName, typeof(T), node);
        }

        public static T GetXmlAttribute<T>(XmlNode node, T defaultValue, string attrName = "Value")
        {
            return (T)GetXmlAttribute(attrName, typeof(T), node, required: false, defaultValue);
        }

        public static object GetXmlAttribute(string attrName, Type type, XmlNode node, bool required = true,
                                             object defaultValue = null)
        {
            XmlAttribute attr = node.Attributes[attrName];
            object oVal = null;
            if (attr == null)
            {
                if (defaultValue != null)
                    oVal = defaultValue;
                else if (required)
                    throw new Exception("Attribute " + attrName + " of Xml node " + node.Name + " not found.");
            }
            else
            {
                oVal = ConvertXmlAttribute(attr, type);
            }
            return oVal;
        }

        //public static void WriteToXml(string fileName, IXml obj, string topLevelNodeName = null)
        //{
        //    WriteToXml(fileName, obj.ToXml(topLevelNodeName));
        //}

        //public static void WriteToXml(string fileName, string xml)
        //{
        //    using (TextWriter w = GetTextWriter(fileName))
        //    {
        //        w.WriteLine(XmlHeader);
        //        w.Write(xml);
        //        w.Flush();
        //    }
        //}

        public static string GetTexturesFolder()
        {
            return Path.Combine(WhorlSettings.Instance.FilesFolder, WhorlSettings.TexturesFolder);
        }

        public static void ReadFromXml(Stream s, IXml obj, string topLevelNodeName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(s);
            XmlNode topNode = null;
            for (int i = 0; i < xmlDoc.ChildNodes.Count; i++)
            {
                XmlNode n = xmlDoc.ChildNodes[i];
                if (n.Name != "xml")
                {
                    if (topLevelNodeName == null || n.Name == topLevelNodeName)
                    {
                        topNode = n;
                        break;
                    }
                }
            }
            if (topNode == null)
                throw new Exception($"Couldn't find XML node named {topLevelNodeName}");
            obj.FromXml(topNode);
        }

        public static void ReadFromXmlResource(string fileName, IXml obj, string topLevelNodeName)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);
            if (s == null)
            {
                MessageBox.Show("Couldn't load resource: " + fileName, "Message");
            }
            else
            {
                ReadFromXml(s, obj, topLevelNodeName);
            }
        }

        public static void ReadFromXml(string fileName, IXml obj, string topLevelNodeName = null)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                ReadFromXml(fs, obj, topLevelNodeName);
            }
        }

        public static TextWriter GetTextWriter(string fileName)
        {
            Stream stream = File.Open(fileName, FileMode.Create);
            TextWriter w = new StreamWriter(stream, Encoding.UTF8);
            return w;
        }

        public static bool NumbersEqual(float n1, float n2, float tolerance = 0.00001F)
        {
            return Math.Abs(n1 - n2) <= tolerance;
        }

        public static bool NumbersEqual(double n1, double n2, double tolerance = 0.00001)
        {
            return Math.Abs(n1 - n2) <= tolerance;
        }

        public static int BinarySearch<TElem, TVal>(TElem[] array, TVal val,
                                                    Func<TVal, TElem, double> fnCompare)
        {
            if (array.Length == 0)
                return -1;
            int iMax = array.Length - 1;
            int iMin = 0;
            int index, prevIndex = -1;
            double compare = 0D;
            while (true)
            {
                index = iMin + (iMax - iMin) / 2;
                if (index == prevIndex)
                    break;
                prevIndex = index;
                compare = fnCompare(val, array[index]);
                if (compare > 0)
                    iMin = index;
                else if (compare < 0)
                    iMax = index;
                else
                    break;
            }
            if (compare > 0 && index > 0)
            {
                if (Math.Abs(fnCompare(val, array[index - 1])) < compare)
                    index--;
            }
            else if (compare < 0 && index < array.Length - 1)
            {
                if (Math.Abs(fnCompare(val, array[index + 1])) < -compare)
                    index++;
            }
            return index;
        }

        public static int GetIndexInRange(int index, int count)
        {
            if (count > 0 && (index < 0 || index >= count))
            {
                index = index % count;
                if (index < 0)
                    index += count;
                //while (index < 0)
                //    index += count;
                //while (index >= count)
                //    index -= count;
            }
            return index;
        }

        public static object ConvertNumericInput(string textInput, Type targetType, string label,
                       ref string errorMessage, double? minValue = null, double? maxValue = null, object defaultValue = null)
        {
            object retVal = null;
            if (errorMessage == null)
            {
                if (string.IsNullOrWhiteSpace(textInput) && defaultValue != null)
                    return defaultValue;
                bool valid = false;
                try
                {
                    retVal = Convert.ChangeType(textInput, targetType);
                }
                catch
                {
                    retVal = null;
                }
                if (retVal != null)
                {
                    double dblVal = Convert.ToDouble(retVal);
                    if ((minValue == null || dblVal >= minValue) && (maxValue == null || dblVal <= maxValue))
                        valid = true;
                    else
                        retVal = null;
                }
                if (!valid)
                {
                    errorMessage = "Please enter " + (targetType == typeof(int) ? "an integer" : "a number");
                    if (minValue != null)
                    {
                        if (maxValue != null)
                            errorMessage += " between " + minValue.ToString() + " and " + maxValue.ToString();
                        else
                            errorMessage += " greater than " + minValue.ToString();
                    }
                    else if (maxValue != null)
                        errorMessage += " less than " + maxValue.ToString();
                    errorMessage += " for " + label + ".";
                }
            }
            return retVal;
        }

        public static object GetCSharpParameterValue(object oParam, int index = -1)
        {
            if (index >= 0)
            {
                var paramArray = oParam as Array;
                if (paramArray != null)
                    oParam = paramArray.GetValue(index);
                else
                    throw new Exception("Array cannot be null.");
            }
            var iOptParam = oParam as IOptionsParameter;
            if (iOptParam != null)
                return iOptParam.SelectedText;
            return (oParam is RandomParameter) ? null : oParam;
        }

        public static bool TypesMatch(Type t1, Type t2)
        {
            if (t1 == t2)
                return true;
            if (t1 != null && t2 != null && t1.IsGenericType && t2.IsGenericType)
            {
                if (t1.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition())
                    return true;
            }
            return false;
        }

        public static T[] RedimArray<T>(T[] array, int length)
        {
            //if (length <= array.Length)
            //    return array;
            T[] newArray = new T[length];
            int maxI = Math.Min(length, array.Length);
            for (int i = 0; i < maxI; i++)
                newArray[i] = array[i];
            return newArray;
        }

        public static T[] RedimArray<T>(T[] array, int length, T defaultValue)
        {
            T[] newArray = RedimArray(array, length);
            for (int i = array.Length; i < newArray.Length; i++)
                newArray[i] = defaultValue;
            return newArray;
        }

        public static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static Type GetListType(Type type)
        {
            if (!IsListType(type))
                throw new ArgumentException("type", "Type must be List<>, but was " + type.FullName);
            return type.GetGenericArguments()[0];
        }

        public static int GetTopLeftIndex(IEnumerable<PointF> points)
        {
            Point[] iPoints = points.Select(p => new Point((int)Math.Round(p.X), (int)Math.Round(p.Y))).ToArray();
            if (iPoints.Length == 0)
                return -1;
            var infoList = Enumerable.Range(0, iPoints.Length).Select(i => new Tuple<int, PointF>(i, iPoints[i]));
            var info = infoList.OrderBy(tpl => tpl.Item2.Y).ThenBy(tpl => tpl.Item2.X).First();
            return info.Item1;
        }

        public static string GetSaveXmlFileName(string description, string folder, string currentFileName)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = description + "|*.xml";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            dlg.InitialDirectory = folder;
            if (currentFileName != null)
                dlg.FileName = Path.GetFileName(currentFileName);
            bool selectedFile = dlg.ShowDialog() == DialogResult.OK;
            return selectedFile ? dlg.FileName : null;
        }

        public static void DisplayForm(Form frm, Form owner = null)
        {
            frm.Show();
            if (frm.WindowState == FormWindowState.Minimized)
                frm.WindowState = FormWindowState.Normal;
            frm.BringToFront();
        }

        public static void InsertTextInTextBox(TextBox txtBox, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            string txtBoxText = txtBox.Text;
            if (txtBoxText == null)
                txtBoxText = string.Empty;
            int caretPos = txtBox.SelectionStart;
            if (caretPos > txtBoxText.Length)
                return;
            txtBoxText = txtBoxText.Substring(0, caretPos) + text + txtBoxText.Substring(caretPos);
            txtBox.Text = txtBoxText;
            txtBox.SelectionStart = caretPos + text.Length;
            txtBox.ScrollToCaret();
        }

        public static bool EvalBool<TVal, TArg>(this TVal val, IEnumerable<TArg> args, Func<TArg, bool> predicate, bool useOr = false)
        {
            return args.Any() && args.Any(a => predicate(a) == useOr) == useOr;
        }

        public static string GetEnumKey(object enumVal)
        {
            if (enumVal == null || !enumVal.GetType().IsEnum)
            {
                throw new Exception("enumVal must be of Enum type.");
            }
            return $"{enumVal.GetType().Name}.{enumVal}";
        }

        public static string GetEnglishPhrase(IEnumerable<string> texts, bool useOr = true)
        {
            var list = texts.ToList();
            if (list.Count > 1)
            {
                list[list.Count - 2] += $" {(useOr ? "or" : "and")} {list[list.Count - 1]}";
                list.RemoveAt(list.Count - 1);
            }
            return string.Join(", ", list);
        }
    }
}
