using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public class ValidIdentifier
    {
        public string Name { get; private set; }
        public Type IdentifierType { get; private set; }
        public IdentifierClasses TokenClass { get; private set; }
        public bool IsGlobal { get; private set; }
        public bool IsExternal { get; }
        public virtual bool IsArray { get { return false; } }
        public bool IsReadOnly { get; set; }
        public bool RemoveOnParse { get; set; }
        public object InitialValue { get; }
        public object CurrentValue { get; internal set; }
        public MethodInfo MemberInfo { get; set; }
        public object FunctionMethodObject { get; set; }
        public int ParameterCount { get; private set; }
        public FunctionCategories FunctionCategory { get; }
        public bool TypeIsAllowedInDeclarations { get; }
        public Token[] InitializationExpression { get; internal set; }
        public Token SourceToken { get; internal set; }

        public ValidIdentifier(string name, Type identifierType, IdentifierClasses identifierClass, bool isGlobal,
                               int parameterCount = 0, object initialValue = null, 
                               object functionMethodObject = null, MethodInfo fnMethod = null, bool isReadOnly = false,
                               FunctionCategories functionCategory = FunctionCategories.Normal, bool? removeOnParse = null,
                               bool typeIsAllowedInDeclarations = true, bool isExternal = false)
        {
            if (removeOnParse == null)
                removeOnParse = !isGlobal;
            this.Name = name;
            this.IdentifierType = identifierType;
            this.TokenClass = identifierClass;
            this.IsGlobal = isGlobal;
            this.CurrentValue = this.InitialValue = initialValue;
            this.MemberInfo = fnMethod;
            this.FunctionMethodObject = functionMethodObject;
            this.ParameterCount = parameterCount;
            IsReadOnly = isReadOnly;
            FunctionCategory = functionCategory;
            RemoveOnParse = (bool)removeOnParse;
            TypeIsAllowedInDeclarations = typeIsAllowedInDeclarations;
            IsExternal = isExternal;
        }

        public void SetCurrentValue(object val)
        {
            if (val != null && IdentifierType != null &&
                !IdentifierType.IsAssignableFrom(val.GetType()))
            {
                throw new Exception($"Invalid type of value, {val.GetType().FullName} for {IdentifierType.FullName} {Name}");
            }
            CurrentValue = val;
        }

        //public ValidIdentifier GetValidIdentifier()
        //{
        //    var validIdentifier = this as ValidIdentifier;
        //    if (validIdentifier == null)
        //        throw new Exception($"{Name} is an array variable.");
        //    return validIdentifier;
        //}

    }

    public class ArrayVariable: ValidIdentifier
    {
        public override bool IsArray { get { return true; } }
        public ArrayDict ArrayValue { get; }

        public ArrayVariable(string name, bool isGlobal):
                    base(name, typeof(double), IdentifierClasses.Variable, isGlobal)
        {
            this.ArrayValue = new ArrayDict(name);
        }
    }

}
