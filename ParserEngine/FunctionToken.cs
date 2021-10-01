using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public enum FunctionCategories: byte
    {
        Normal,
        Custom,
        Method,
        Constructor
    }
    public class FunctionToken: BaseOperandToken
    {
        /// <summary>
        /// RequiredFunctionParameterCount < 0 for variable number of parameters.
        /// </summary>
        public int RequiredFunctionParameterCount
        {
            get
            {
                return ValidIdentifier == null ? 0 : ValidIdentifier.ParameterCount;
            }
        }
        public int FunctionArgumentCount { get; internal set; }
        public FunctionCategories FunctionCategory { get; private set; }
        public MemberInfo MemberInfo { get; internal set; }
        public object FunctionMethodObject { get; internal set; }
        public bool EvalMethodObject { get; internal set; } = false;

        public FunctionToken(Token token, Type operandType = null, FunctionCategories functionCategory = FunctionCategories.Normal,
                             IdentifierClasses operandClass = IdentifierClasses.Function): 
            base(token, operandType)
        {
            OperandClass = operandClass;
            FunctionCategory = functionCategory;
            if (FunctionCategory == FunctionCategories.Method)
                EvalMethodObject = true;
        }

        internal override void SetFromValidIdentifier(ValidIdentifier validIdentifier)
        {
            base.SetFromValidIdentifier(validIdentifier);
            MemberInfo = validIdentifier.MemberInfo;
            FunctionCategory = validIdentifier.FunctionCategory;
            FunctionMethodObject = validIdentifier.FunctionMethodObject;
        }

        internal override void SetParameterObject(BaseParameter baseParameter)
        {
            base.SetParameterObject(baseParameter);
            var varFnParameter = baseParameter as VarFunctionParameter;
            if (varFnParameter != null)
            {
                MemberInfo = varFnParameter.MethodInfo;
                //this.OperandClass = IdentifierClasses.Function;
                ValidIdentifier = new ValidIdentifier(varFnParameter.MethodInfo.Name, typeof(double),
                                  IdentifierClasses.Function, false, varFnParameter.ParameterCount, null, null,
                                  varFnParameter.MethodInfo);
                varFnParameter.ValueChanged += SetOperandValueFromParameter;
            }
        }

        protected override void SetOperandValueFromParameter(object sender, EventArgs e)
        {
            base.SetOperandValueFromParameter(sender, e);
            var fnParam = sender as VarFunctionParameter;
            if (fnParam != null)
            {
                MemberInfo = fnParam.MethodInfo;
            }
        }
    }
}
