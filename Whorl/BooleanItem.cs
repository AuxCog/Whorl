using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class BooleanItem
    {
        public bool? Value { get; }
        public string Text { get; }

        public BooleanItem(bool? value, string text)
        {
            Value = value;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }

        private static BooleanItem[] YesNoItems =
        {
            new BooleanItem(null, string.Empty),
            new BooleanItem(false, "No"),
            new BooleanItem(true, "Yes")
        };

        public static BooleanItem[] GetYesNoItems()
        {
            return (BooleanItem[])YesNoItems.Clone();
        }
    }
}
