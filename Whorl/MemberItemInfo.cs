using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class MemberInfoInfo
    {
        public MemberInfo MemberInfo { get; }

        public MemberInfoInfo(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }

        private string GetParameterText(ParameterInfo parameterInfo)
        {
            string text = parameterInfo.ParameterType.IsByRef ? "ref " : string.Empty;
            text += $"{parameterInfo.ParameterType.Name} {parameterInfo.Name}";
            return text;
        }

        public override string ToString()
        {
            string text;
            if (MemberInfo == null)
                text = string.Empty;
            else
            {
                switch (MemberInfo.MemberType)
                {
                    case MemberTypes.Method:
                        MethodInfo methodInfo = (MethodInfo)MemberInfo;
                        text = $"{methodInfo.ReturnType.Name} {methodInfo.Name}" + 
                            $"({string.Join(", ", methodInfo.GetParameters().Select(pi => GetParameterText(pi)))})";
                        break;
                    case MemberTypes.Constructor:
                        ConstructorInfo constructorInfo = (ConstructorInfo)MemberInfo;
                        text = $"new {constructorInfo.ReflectedType.Name}" +
                            $"({string.Join(", ", constructorInfo.GetParameters().Select(pi => GetParameterText(pi)))})";
                        break;
                    default:
                        text = MemberInfo.ToString();
                        break;
                }
            }
            return text;
        }
    }

    public class MemberItemInfo
    {
        public MemberInfoInfo MemberInfoInfo { get; }
        public MemberItemInfo(MemberInfo memberInfo)
        {
            MemberInfoInfo = new MemberInfoInfo(memberInfo);
        }
    }
}
