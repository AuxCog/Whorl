using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ParameterChoice
    {
        public object ParentObject { get; }
        public object Value => (getValueFunc == null || ParentObject == null) ? ParentObject : getValueFunc(ParentObject);
        public string Text { get; }
        private Func<object, object> getValueFunc { get; }

        public ParameterChoice(object parentObject, string text = null, Func<object, object> getValueFunc = null)
        {
            ParentObject = parentObject;
            this.getValueFunc = getValueFunc;
            if (text == null)
            {
                if (parentObject != null)
                    text = parentObject.ToString();
                else
                    throw new NullReferenceException("value cannot be null if text is null.");
            }
            Text = text;
        }
        public override string ToString()
        {
            return Text;
        }
    }
}
