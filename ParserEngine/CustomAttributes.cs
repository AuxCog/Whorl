using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public enum MathFunctionTypes
    {
        Normal,
        Trigonometric
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ReadOnlyAttribute : Attribute
    {
    }

    public class MathFunctionAttribute: Attribute
    {
        public MathFunctionTypes MathFunctionType { get; set; }

        public MathFunctionAttribute(MathFunctionTypes type)
        {
            MathFunctionType = type;
        }
    }

    public class ExcludeMethodAttribute: Attribute
    {
    }

    public class DerivMethodNameAttribute : Attribute
    {
        public string DerivMethodName { get; set; }

        public DerivMethodNameAttribute(string name)
        {
            DerivMethodName = name;
        }
    }

    public class MethodNameAttribute: Attribute
    {
        public string Name { get; set; }

        public MethodNameAttribute(string name)
        {
            Name = name;
        }
    }

}
