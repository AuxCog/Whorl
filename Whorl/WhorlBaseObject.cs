using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Whorl
{
    /// <summary>
    /// Base class for Pattern and WhorlDesign classes.
    /// </summary>
    public abstract class WhorlBaseObject: GuidKey, IXml, IDisposable
    {
        public WhorlBaseObject(): base()
        {
        }

        public WhorlBaseObject(WhorlBaseObject source): base(source)
        {
        }

        public abstract XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null);

        public abstract void FromXml(XmlNode node);

        public bool Disposed { get; protected set; }

        public abstract void Dispose();
    }
}
