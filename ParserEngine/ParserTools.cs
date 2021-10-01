using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public static class ParserTools
    {
        public static Attribute GetCustomAttribute<AttrType>(this System.Reflection.MemberInfo element,
                                                             Predicate<AttrType> predicate) where AttrType : Attribute
        {
            var attr = Attribute.GetCustomAttribute(element, typeof(AttrType)) as AttrType;
            if (attr != null)
            {
                if (!predicate(attr))
                    attr = null;
            }
            return attr;
        }

        public static bool IsOrIsSubclassOf(this Type t, Type t2)
        {
            return t == t2 || t.IsSubclassOf(t2);
        }

        public static MethodInfo GetTryParseMethod(this Type t)
        {
            return t.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, 
                                new Type[] { typeof(string), t.MakeByRefType() }, null);
        }

        public static double DegreesToRadians(double degrees)
        {
            return (Math.PI / 180.0) * degrees;
        }

        public static double RadiansToDegrees(double radians)
        {
            return (180.0 / Math.PI) * radians;
        }

        public static bool NumbersEqual(float n1, float n2, float tolerance = 0.00001F)
        {
            return Math.Abs(n1 - n2) <= tolerance;
        }

        public static bool NumbersEqual(double n1, double n2, double tolerance = 0.00001F)
        {
            return Math.Abs(n1 - n2) <= tolerance;
        }

        /// <summary>
        /// Return double >= 0 and < div.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static double Normalize(double x, double div)
        {
            x /= div;
            return div * (x - Math.Floor(x));
        }

    }
}
