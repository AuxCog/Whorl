using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ParserEngine;

namespace Whorl
{
    public class XmlTools
    {
        public XmlDocument XmlDoc { get; private set; }

        public XmlTools()
        {
        }

        public XmlTools(XmlDocument xmlDoc)
        {
            this.XmlDoc = xmlDoc;
        }

        public void CreateDocument()
        {
            XmlDoc = new XmlDocument();
        }

        public void CreateXml(IXml xmlObject, string topLevelNodeName = null)
        {
            CreateDocument();
            XmlDoc.AppendChild(xmlObject.ToXml(null, this, topLevelNodeName));
        }

        public void SaveXml(string fileName)
        {
            XmlDoc.Save(fileName);
        }

        public static void WriteToXml(string fileName, IXml obj, string topXmlNodeName = null)
        {
            var xmlTools = new XmlTools();
            xmlTools.CreateXml(obj, topXmlNodeName);
            xmlTools.SaveXml(fileName);
        }

        public void LoadXmlDocument(string fileName)
        {
            XmlDoc = new XmlDocument();
            XmlDoc.Load(fileName);
        }

        public void LoadXmlDocument(Stream stream)
        {
            XmlDoc = new XmlDocument();
            XmlDoc.Load(stream);
        }

        public void ReadXml(IXml xmlObject, string topLevelNodeName)
        {
            if (XmlDoc == null)
                throw new Exception("LoadXmlDocument was not called before ReadXml.");
            XmlNodeList nodes = XmlDoc.GetElementsByTagName(topLevelNodeName);
            if (nodes.Count == 1)
            {
                xmlObject.FromXml(nodes[0]);
            }
            else
            {
                throw new Exception("Invalid XML found for " + topLevelNodeName + ".");
            }
        }

        public XmlNode AppendToParent(XmlNode parentNode, XmlNode xmlNode)
        {
            if (parentNode != null)
                parentNode.AppendChild(xmlNode);
            return xmlNode;
        }

        public XmlNode CreateXmlNode(string name)
        {
            return XmlDoc.CreateNode("element", name, string.Empty);
        }

        public static void SetNodeValue(XmlNode xmlNode, string value)
        {
            xmlNode.InnerText = value;
        }

        public void AppendChildNode(XmlNode parentNode, string nodeName, string value)
        {
            XmlNode child = CreateXmlNode(nodeName);
            SetNodeValue(child, value);
            parentNode.AppendChild(child);
        }

        public void AppendAttributeChildNode(XmlNode parentNode, string nodeName, object value, string attrName = "Value")
        {
            if (value != null)
            {
                XmlNode child = CreateXmlNode(nodeName);
                AppendXmlAttribute(child, attrName, value);
                parentNode.AppendChild(child);
            }
        }

        public void AppendXmlAttribute(XmlNode xmlNode, string AttrName, object AttrValue)
        {
            if (AttrValue != null)
            {
                XmlAttribute attr = XmlDoc.CreateAttribute(AttrName);
                attr.Value = AttrValue.ToString();
                xmlNode.Attributes.Append(attr);
            }
        }

        public static string GetXmlAttribute(XmlNode node, string attrName, bool throwException = true)
        {
            var attr = node.Attributes[attrName];
            if (attr == null)
            {
                if (throwException)
                {
                    throw new Exception($"Attribute {attrName} of Xml node {node.Name} was not found.");
                }
                else
                {
                    return null;
                }
            }
            return attr.Value;
        }

        private static readonly BindingFlags DefaultBindingFlags = 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName, bool throwException = true)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, DefaultBindingFlags);
            if (propertyInfo == null && throwException)
                throw new Exception($"Couldn't find property {type.Name}.{propertyName}.");
            return propertyInfo;
        }

        public void AppendXmlAttributes(XmlNode xmlNode, object source, params string[] propertyNames)
        {
            Type sourceType = source.GetType();
            foreach (string propName in propertyNames)
            {
                PropertyInfo propertyInfo = GetPropertyInfo(sourceType, propName);
                object value = propertyInfo.GetValue(source);
                if (value != null)
                {
                    if (value is string s)
                    {
                        if (string.IsNullOrEmpty(s))
                            continue;
                    }
                    AppendXmlAttribute(xmlNode, propName, value);
                }
            }
        }

        public void AppendAllXmlAttributes(XmlNode xmlNode, object source)
        {
            AppendXmlAttributesExcept(xmlNode, source);
        }

        public void AppendXmlAttributesExcept(XmlNode xmlNode, object source, params string[] excludedPropertyNames)
        {
            Type sourceType = source.GetType();
            var excludedProps = new HashSet<string>(excludedPropertyNames);
            foreach (PropertyInfo propertyInfo in sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!(propertyInfo.CanWrite && propertyInfo.CanRead) 
                    || excludedProps.Contains(propertyInfo.Name)
                    || propertyInfo.GetIndexParameters().Any())
                {
                    continue;
                }
                object value = propertyInfo.GetValue(source);
                if (value != null)
                {
                    if (value is string s)
                    {
                        if (string.IsNullOrEmpty(s))
                            continue;
                    }
                    else if (!(value is Enum || value.GetType().IsPrimitive))
                        continue;
                    AppendXmlAttribute(xmlNode, propertyInfo.Name, value);
                }
            }
        }

        public XmlNode CreateXmlNode(string name, PointF point)
        {
            XmlNode xmlNode = CreateXmlNode(name);
            SetXmlAttributes(xmlNode, point);
            return xmlNode;
        }

        public XmlNode CreateXmlNode(string name, Point point)
        {
            XmlNode xmlNode = CreateXmlNode(name);
            SetXmlAttributes(xmlNode, point);
            return xmlNode;
        }

        public void SetXmlAttributes(XmlNode xmlNode, Point point)
        {
            AppendXmlAttribute(xmlNode, "X", point.X);
            AppendXmlAttribute(xmlNode, "Y", point.Y);
        }

        public void SetXmlAttributes(XmlNode xmlNode, PointF point)
        {
            AppendXmlAttribute(xmlNode, "X", Math.Round(point.X, 2));
            AppendXmlAttribute(xmlNode, "Y", Math.Round(point.Y, 2));
        }

        public XmlNode CreateXmlNode(string name, Size size)
        {
            XmlNode xmlNode = CreateXmlNode(name);
            SetXmlAttributes(xmlNode, size);
            return xmlNode;
        }

        public void SetXmlAttributes(XmlNode xmlNode, Size size)
        {
            AppendXmlAttribute(xmlNode, "Width", size.Width);
            AppendXmlAttribute(xmlNode, "Height", size.Height);
        }

        public XmlNode CreateXmlNode(string name, Color color, string attrName = "Color")
        {
            XmlNode xmlNode = CreateXmlNode(name);
            SetXmlAttributes(xmlNode, color, attrName);
            return xmlNode;
        }

        public void SetXmlAttributes(XmlNode xmlNode, Color color, string attrName = "Color")
        {
            AppendXmlAttribute(xmlNode, attrName, color.ToArgb());
        }

        public XmlNode CreateXmlNode(string name, Complex z)
        {
            XmlNode xmlNode = CreateXmlNode(name);
            SetXmlAttributes(xmlNode, z);
            return xmlNode;
        }

        public void SetXmlAttributes(XmlNode xmlNode, Complex z)
        {
            AppendXmlAttribute(xmlNode, "Re", z.Re);
            AppendXmlAttribute(xmlNode, "Im", z.Im);
        }

        public static PointF GetPointFFromXml(XmlNode xmlNode)
        {
            return new PointF((float)GetXmlAttribute("X", typeof(float), xmlNode),
                              (float)GetXmlAttribute("Y", typeof(float), xmlNode));
        }

        public static Size GetSizeFromXml(XmlNode xmlNode)
        {
            return new Size((int)GetXmlAttribute("Width", typeof(int), xmlNode),
                            (int)GetXmlAttribute("Height", typeof(int), xmlNode));
        }

        public static Complex GetComplexFromXml(XmlNode xmlNode)
        {
            return new Complex((double)GetXmlAttribute("Re", typeof(double), xmlNode),
                               (double)GetXmlAttribute("Im", typeof(double), xmlNode));
        }

        public static Color GetColorFromXml(XmlNode xmlNode)
        {
            return Color.FromArgb((int)GetXmlAttribute("Argb", typeof(int), xmlNode));
        }

        public static object ConvertXmlAttribute(XmlAttribute attr, Type targetType)
        {
            try
            {
                object oVal;
                if (targetType == typeof(string))
                    oVal = attr.Value;
                else
                    oVal = Convert.ChangeType(attr.Value, targetType);
                return oVal;
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting XML attribute " + attr.Name, ex);
            }
        }

        public static object GetXmlAttribute(string attrName, Type type, XmlNode xmlNode,
                             object defaultValue = null, bool required = true)
        {
            XmlAttribute attr = xmlNode.Attributes[attrName];
            object oVal = null;
            if (attr == null)
            {
                if (defaultValue != null || !required)
                    oVal = defaultValue;
                else
                    throw new Exception("Attribute " + attrName + " of Xml xmlNode " + xmlNode.Name + " not found.");
            }
            else
            {
                oVal = ConvertXmlAttribute(attr, type);
            }
            return oVal;
        }

        //public static XmlNode SerializeObjectToXmlNode(Object obj)
        //{
        //    if (obj == null)
        //        throw new ArgumentNullException("Argument cannot be null");

        //    XmlNode resultNode = null;
        //    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        try
        //        {
        //            xmlSerializer.Serialize(memoryStream, obj);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return null;
        //        }
        //        memoryStream.Position = 0;
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(memoryStream);
        //        resultNode = doc.DocumentElement;
        //    }
        //    return resultNode;
        //}

        //public static Object DeSerializeXmlNodeToObject(XmlNode xmlNode, Type objectType)
        //{
        //    if (xmlNode == null)
        //        throw new ArgumentNullException("Argument cannot be null");
        //    XmlSerializer xmlSerializer = new XmlSerializer(objectType);
        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.AppendChild(doc.ImportNode(xmlNode, true));
        //        doc.Save(memoryStream);
        //        memoryStream.Position = 0;
        //        XmlReader reader = XmlReader.Create(memoryStream);
        //        try
        //        {
        //            return xmlSerializer.Deserialize(reader);
        //        }
        //        catch
        //        {
        //            return objectType.IsByRef ? null : Activator.CreateInstance(objectType);
        //        }
        //    }
        //}
    }
}
