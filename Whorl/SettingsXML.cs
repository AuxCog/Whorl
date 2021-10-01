using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace Whorl
{
    public class SettingsXML
    {
        public const string RootNodeName = "Settings";
        private const BindingFlags propertyBindingFlags = BindingFlags.Public | BindingFlags.Instance;

        public static string GetSettingsXmlFilePath()
        {
            return Path.Combine(Application.StartupPath, "WhorlSettings.XML");
        }

        public static void SaveSettingsXml(XmlDocument xmlDoc)
        {
            string filePath = GetSettingsXmlFilePath();
            xmlDoc.Save(filePath);
        }

        public static XmlDocument ReadSettingsXML()
        {
            string filePath = GetSettingsXmlFilePath();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            return xmlDoc;
        }

        public static XmlNode GetRootNode(XmlDocument xmlDocument)
        {
            XmlNode rootNode = xmlDocument.FirstChild;
            if (rootNode?.Name != RootNodeName)
                throw new Exception("Invalid settings XML document.");
            return rootNode;
        }

        /// <summary>
        /// Read settings from XML file.
        /// </summary>
        /// <param name="whorlSettings"></param>
        /// <param name="errors"></param>
        public static void PopulateSettingsFromXml(WhorlSettings whorlSettings, out List<string> errors)
        {
            errors = new List<string>();
            XmlDocument xmlDocument = ReadSettingsXML();
            XmlNode rootNode = GetRootNode(xmlDocument);
            var propertyNamesNotSet = new HashSet<string>(GetSettingsProperties().Select(pi => pi.Name));
            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                var propertyInfo = typeof(WhorlSettings).GetProperty(childNode.Name, propertyBindingFlags);
                if (propertyInfo == null)
                {
                    //Check for change of name:
                    foreach (PropertyInfo pi in typeof(WhorlSettings).GetProperties(propertyBindingFlags))
                    {
                        var attr = pi.GetCustomAttribute<PreviousNameAttribute>();
                        if (attr != null && attr.PreviousName == childNode.Name)
                        {
                            propertyInfo = pi;
                            break;
                        }
                    }
                    if (propertyInfo == null)
                    {
                        errors.Add($"Didn't find settings property {childNode.Name}.");
                        continue;
                    }
                }
                if (!propertyInfo.CanWrite)
                    continue;
                propertyNamesNotSet.Remove(propertyInfo.Name);
                Type valueType;
                string typeFullName = Tools.GetXmlAttribute<string>(childNode, "Type");
                if (typeFullName == "System.Drawing.Color")
                    valueType = typeof(System.Drawing.Color);
                else
                {
                    try
                    {
                        valueType = Type.GetType(typeFullName, throwOnError: true);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                        continue;
                    }
                }
                string valueString = Tools.GetXmlAttribute<string>(childNode, null, "Value");
                if (!GetPropertyValue(valueString, valueType, out object value, errors))
                    continue;
                if (propertyInfo.PropertyType != valueType)
                {
                    errors.Add($"Property {propertyInfo.Name} is not of type {valueType.Name}.");
                    continue;
                }
                try
                {
                    propertyInfo.SetValue(whorlSettings, value);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    continue;
                }
            }
            foreach (string propertyName in propertyNamesNotSet)
            {
                var propertyInfo = typeof(WhorlSettings).GetProperty(propertyName, propertyBindingFlags);
                var initAttribute = propertyInfo.GetCustomAttribute<InitialSettingValueAttribute>();
                if (initAttribute != null)
                {
                    if (GetPropertyValue(initAttribute.Value, propertyInfo.PropertyType, out object value, errors))
                    {
                        propertyInfo.SetValue(whorlSettings, value);
                    }
                }
                else
                {
                    errors.Add($"No InitialSettingValue attribute found for setting property {propertyInfo.Name}.");
                }
            }
            whorlSettings.AfterSaveOrRead();
        }

        private static bool GetPropertyValue(string valueString, Type valueType, out object value, List<string> errors)
        {
            if (valueType == typeof(string))
                value = valueString;
            else
            {
                try
                {
                    value = Convert.ChangeType(valueString, valueType);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    value = null;
                    return false;
                }
            }
            return true;
        }

        public static void SaveSettings(WhorlSettings whorlSettings)
        {
            var xmlTools = new XmlTools();
            xmlTools.CreateDocument();
            var topNode = xmlTools.CreateXmlNode(RootNodeName);
            xmlTools.XmlDoc.AppendChild(topNode);
            foreach (var propertyInfo in GetSettingsProperties().OrderBy(prp => prp.Name))
            {
                object val = propertyInfo.GetValue(whorlSettings);
                XmlNode childNode = xmlTools.CreateXmlNode(propertyInfo.Name);
                xmlTools.AppendXmlAttribute(childNode, "Type", propertyInfo.PropertyType.FullName);
                if (propertyInfo.GetCustomAttribute<ParserEngine.ReadOnlyAttribute>() != null)
                    xmlTools.AppendXmlAttribute(childNode, "IsReadOnly", true);
                if (val != null)
                    xmlTools.AppendXmlAttribute(childNode, "Value", val);
                topNode.AppendChild(childNode);
            }
            SaveSettingsXml(xmlTools.XmlDoc);
            whorlSettings.AfterSaveOrRead();
        }

        private static IEnumerable<PropertyInfo> GetSettingsProperties()
        {
            return typeof(WhorlSettings).GetProperties(propertyBindingFlags)
                   .Where(pi => pi.CanWrite && pi.GetIndexParameters().Length == 0);
        }

        /*** Tools only used once ***/

        private const string ReadonlyProperties =
            "DesignPatternsFileName,PatternChoicesFileName,DefaultPicWidth,DefaultPicHeight,DesignThumbnailsFolder,DefaultPatternFileName";

        public static XmlDocument CreateSettingXML()
        {
            var xmlTools = new XmlTools();
            xmlTools.CreateDocument();
            var topNode = xmlTools.CreateXmlNode(RootNodeName);
            xmlTools.XmlDoc.AppendChild(topNode);
            var readonlyPropertyNames = new HashSet<string>(ReadonlyProperties.Split(','));
            foreach (SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                object val = WhorlSettings.Instance[currentProperty.Name];
                if (val == null)
                    continue;
                var childNode = xmlTools.CreateXmlNode(currentProperty.Name);
                xmlTools.AppendXmlAttribute(childNode, "Type", val.GetType().FullName);
                if (readonlyPropertyNames.Contains(currentProperty.Name))
                    xmlTools.AppendXmlAttribute(childNode, "IsReadOnly", true);
                xmlTools.AppendXmlAttribute(childNode, "Value", val);
                topNode.AppendChild(childNode);
            }
            return xmlTools.XmlDoc;
        }

        private static string GetIndent(int indent)
        {
            return string.Empty.PadRight(4 * indent);
        }

        public static string GetCSharpSettingsCode(XmlDocument xmlDocument, out List<string> errors)
        {
            StringBuilder sbClass = new StringBuilder();
            sbClass.AppendLine(
@"public class WhorlSettings: ChangeTracker
{");
            errors = new List<string>();
            XmlNode rootNode = GetRootNode(xmlDocument);
            string indent = GetIndent(1);
            string indent2 = GetIndent(2);
            foreach (XmlNode childNode in (from XmlNode n in rootNode.ChildNodes orderby n.Name select n))
            {
                Type valueType;
                string typeFullName = Tools.GetXmlAttribute<string>(childNode, "Type");
                try
                {
                    valueType = Type.GetType(typeFullName, throwOnError: true);
                }
                catch (Exception ex)
                {
                    if (typeFullName == "System.Drawing.Color")
                        valueType = typeof(System.Drawing.Color);
                    else
                    {
                        errors.Add(ex.Message);
                        continue;
                    }
                }
                string valueString = Tools.GetXmlAttribute<string>(childNode, "Value");
                //object value;
                //if (valueType == typeof(string))
                //    value = "@\"" + valueString.Replace("\"", "\\\"") + "\"";
                //else
                //{
                //    try
                //    {
                //        value = Convert.ChangeType(valueString, valueType);
                //    }
                //    catch (Exception ex)
                //    {
                //        errors.Add(ex.Message);
                //        continue;
                //    }
                //}
                sbClass.AppendLine(indent + $"//Original value: {valueString}");
                bool isReadOnly = Tools.GetXmlAttribute<bool>(childNode, false, "IsReadOnly");
                string propertyName = childNode.Name;
                if (isReadOnly)
                {
                    sbClass.AppendLine(indent + $"public {valueType.Name} {propertyName}" + " { get; private set; }");
                }
                else
                {
                    string privateVarName = "_" + Char.ToLower(propertyName[0]) + propertyName.Substring(1);
                    sbClass.AppendLine(indent + $"private {valueType.Name} {privateVarName};");
                    sbClass.AppendLine(indent + $"public {valueType.Name} {propertyName}");
                    sbClass.AppendLine(indent + "{");
                    sbClass.AppendLine(indent2 + "get { " + $"return {privateVarName};" + " }");
                    sbClass.AppendLine(indent2 + "set { " + $"SetProperty(ref {privateVarName}, value);" + " }");
                    sbClass.AppendLine(indent + "}");
                }
                sbClass.AppendLine();
            }
            sbClass.AppendLine("}");
            return sbClass.ToString();
        }
    }
}
