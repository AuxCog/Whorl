using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class ValueTextItem
    {
        public string Text { get; }
        public object Value { get; }

        public ValueTextItem(object value, string text = null)
        {
            if (text == null && value == null)
                throw new Exception("value cannot be null if text is null.");
            Text = text ?? value.ToString();
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
