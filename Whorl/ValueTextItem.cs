using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class ValueTextItem<T>
    {
        public string Text { get; }
        public T Value { get; }

        public ValueTextItem(T value, string text = null)
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
