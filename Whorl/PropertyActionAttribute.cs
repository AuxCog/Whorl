using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PropertyActionAttribute: Attribute
    {
        public string Name { get; set; }
        public bool Exclude { get; set; }
        public double MinValue { get; set; } = double.MinValue;
        public double MaxValue { get; set; } = double.MaxValue;

        public static PropertyActionAttribute GetPropertyAttribute(System.Reflection.PropertyInfo propInfo)
        {
            return propInfo.GetCustomAttribute<PropertyActionAttribute>();
        }
    }
}
