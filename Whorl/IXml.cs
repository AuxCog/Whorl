using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    public interface IXml
    {
        XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null);
        //string ToXml(string xmlNodeName = null);
        void FromXml(XmlNode node);
    }
}
