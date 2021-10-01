using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public enum OperatorDefinitions
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Exponentiation,
        Modulo,
        UnaryMinus,
        IsEqual,
        NotEqual,
        LT,
        GT,
        LTE,
        GTE,
        Or,
        And,
        Not,
        Assignment,
        Dot,
        Comma,
        Cast
    }

    public class OperatorMethods
    {
        public static double Addition(double a, double b)
        {
            return a + b;
        }
        public static float Addition(float a, float b)
        {
            return a + b;
        }
        public static int Addition(int a, int b)
        {
            return a + b;
        }
        public static string Addition(string a, string b)
        {
            return a + b;
        }

        public static double Subtraction(double a, double b)
        {
            return a - b;
        }

        public static float Subtraction(float a, float b)
        {
            return a - b;
        }

        public static int Subtraction(int a, int b)
        {
            return a - b;
        }

        public static double Multiplication(double a, double b)
        {
            return a * b;
        }

        public static float Multiplication(float a, float b)
        {
            return a * b;
        }

        public static int Multiplication(int a, int b)
        {
            return a * b;
        }

        public static double Division(double a, double b)
        {
            return a / b;
        }

        public static float Division(float a, float b)
        {
            return a / b;
        }

        public static int Division(int a, int b)
        {
            return a / b;
        }

        public static double Exponentiation(double a, double b)
        {
            return Math.Pow(a, b);
        }

        public static double Modulo(double a, double b)
        {
            return a % b;
        }

        public static float Modulo(float a, float b)
        {
            return a % b;
        }

        public static int Modulo(int a, int b)
        {
            return a % b;
        }

        public static double UnaryMinus(double a)
        {
            return -a;
        }

        public static float UnaryMinus(float a)
        {
            return -a;
        }

        public static int UnaryMinus(int a)
        {
            return -a;
        }

        public static bool LT(double a, double b)
        {
            return a < b;
        }

        public static bool LT(float a, float b)
        {
            return a < b;
        }

        public static bool LT(int a, int b)
        {
            return a < b;
        }

        public static bool GT(double a, double b)
        {
            return a > b;
        }

        public static bool GT(float a, float b)
        {
            return a > b;
        }

        public static bool GT(int a, int b)
        {
            return a > b;
        }

        public static bool LTE(double a, double b)
        {
            return a <= b;
        }

        public static bool LTE(float a, float b)
        {
            return a <= b;
        }

        public static bool LTE(int a, int b)
        {
            return a <= b;
        }

        public static bool GTE(double a, double b)
        {
            return a >= b;
        }

        public static bool GTE(float a, float b)
        {
            return a >= b;
        }

        public static bool GTE(int a, int b)
        {
            return a >= b;
        }

        //public static Complex Comma(double re, double im)
        //{
        //    return new Complex(re, im);
        //}

        public static object Cast(object a, Type t)
        {
            return Convert.ChangeType(a, t);
        }

        //public static double Assignment(ref double a, double b)
        //{
        //    a = b;
        //    return a;
        //}

        public static bool IsEqual(object a, object b)
        {
            return object.Equals(a, b);
        }

        public static bool NotEqual(object a, object b)
        {
            return !object.Equals(a, b);
        }

        public static bool Or(bool a, bool b)
        {
            return a || b;
        }

        public static bool And(bool a, bool b)
        {
            return a && b;
        }

        public static bool Not(bool a)
        {
            return !a;
        }
    }
}
