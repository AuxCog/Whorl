using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class GetMethod
    {
        internal static MethodInfo GetMethodInfo(Type ownerType, string methodName, TypeTokenInfo[] argumentTypes,
                                                 bool isStatic = true, bool ignoreCase = true)
        {
            BindingFlags bindingFlags = BindingFlags.Public;
            if (isStatic)
                bindingFlags |= BindingFlags.Static;
            else
                bindingFlags |= BindingFlags.Instance;
            if (ignoreCase)
                bindingFlags |= BindingFlags.IgnoreCase;
            MethodInfo methodInfo;
            try
            {
                methodInfo = ownerType.GetMethod(methodName, bindingFlags, null,
                                                 argumentTypes.Select(at => at.Type).ToArray(), null);
            }
            catch
            {
                methodInfo = null;
            }
            if (methodInfo == null)
            {
                var methodsInfo = ownerType.GetMember(methodName, bindingFlags).Where(mi => mi.MemberType == MemberTypes.Method)
                                                                               .Select(mi => (MethodInfo)mi);
                methodInfo = methodsInfo.Where(mi => ParametersMatch(mi.GetParameters(), argumentTypes)).FirstOrDefault();
            }
            return methodInfo;
        }

        internal static bool MethodMatches(MethodInfo mi, TypeTokenInfo[] argumentTypes)
        {
            return ParametersMatch(mi.GetParameters(), argumentTypes);
        }

        internal static ConstructorInfo GetConstructorInfo(Type ownerType, TypeTokenInfo[] argumentTypes)
        {
            return ownerType.GetConstructors().Where(ci => ParametersMatch(ci.GetParameters(), argumentTypes)).FirstOrDefault();
        }

        private static bool ParameterHasParamsAttribute(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute(typeof(ParamArrayAttribute)) != null;
        }

        internal static ParameterInfo GetParamsParameter(ParameterInfo[] parameters)
        {
            ParameterInfo paramsParameter = parameters.LastOrDefault();
            if (paramsParameter != null)
            {
                if (!ParameterHasParamsAttribute(paramsParameter))
                    paramsParameter = null;
            }
            return paramsParameter;
        }

        private static bool ParametersMatch(ParameterInfo[] parameters, TypeTokenInfo[] argumentTypes)
        {
            if (parameters.Length == 0)
                return argumentTypes.Length == 0;
            ParameterInfo paramsParameter = GetParamsParameter(parameters);
            if (parameters.Length != argumentTypes.Length)
            {
                if (paramsParameter == null || argumentTypes.Length < parameters.Length - 1)
                    return false;
            }
            int paramIndex = 0;
            Type paramType = null;
            for (int i = 0; i < argumentTypes.Length; i++)
            {
                if (paramIndex < parameters.Length)
                {
                    paramType = parameters[paramIndex++].ParameterType;
                    if (paramType.IsByRef || parameters[i] == paramsParameter)
                        paramType = paramType.GetElementType();
                }
                if (!TypesAreCompatible(paramType, argumentTypes[i]))
                    return false;
            }
            return true;
        }

        internal static bool TypesAreCompatible(Type assignedType, TypeTokenInfo typeTokenInfo)
        {
            if (assignedType.IsAssignableFrom(typeTokenInfo.Type))
                return true;
            if ((assignedType == typeof(int) || assignedType == typeof(float)) && typeTokenInfo.Type == typeof(double))
            {
                OperandToken operandToken = typeTokenInfo.Token as OperandToken;
                if (operandToken != null && operandToken.OperandClass == IdentifierClasses.Literal &&
                    operandToken.OperandValue is double)
                {
                    try
                    {
                        operandToken.OperandValue = Convert.ChangeType(operandToken.OperandValue, assignedType);
                        operandToken.ReturnType = assignedType;
                        typeTokenInfo.Type = assignedType;
                        return true;
                    }
                    catch { }
                }
            }
            return false;

        }

        internal static object EvalMethod(object o, MethodInfo methodInfo, object[] parameters)
        {
            object val = methodInfo.Invoke(o, parameters);
            if (val is double)
            {
                double value = (double)val;
                if (double.IsNaN(value))
                    val = 0.0;
                else if (value > Expression.MaxDoubleValue)
                    val = Expression.MaxDoubleValue;
                else if (value < -Expression.MaxDoubleValue)
                    val = -Expression.MaxDoubleValue;
            }
            return val;
        }

        //internal static double FixDouble(double x)
        //{
        //    if (double.IsNaN(x))
        //        x = 0.0;
        //    else if (x > Expression.MaxDoubleValue)
        //        x = Expression.MaxDoubleValue;
        //    else if (x < -Expression.MaxDoubleValue)
        //        x = -Expression.MaxDoubleValue;
        //    return x;
        //}
    }
}
