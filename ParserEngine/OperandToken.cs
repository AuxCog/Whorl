using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public enum OperandModifiers
    {
        None,
        RefParameter,
        ArrayLength,
        ParameterObject
    }

    public abstract class BaseTypeToken: TypedToken
    {
        public IdentifierClasses OperandClass { get; internal set; }

        public BaseTypeToken(Token token, Type type, IdentifierClasses operandClass)
               : base(token, token.TokenType, type)
        {
            OperandClass = operandClass;
        }
    }

    public class TypeToken: BaseTypeToken
    {
        public TypeToken(Token token, Type type): base(token, type, IdentifierClasses.Type)
        {
        }
    }

    public abstract class BaseOperandToken : BaseTypeToken, IDisposable
    {
        public Type OperandType
        {
            get { return ReturnType; }
        }

        public ValidIdentifier ValidIdentifier { get; protected set; }

        public BaseParameter BaseParameter { get; private set; }

        internal virtual void SetParameterObject(BaseParameter baseParameter)
        {
            this.BaseParameter = baseParameter;
            var varFnParameter = baseParameter as VarFunctionParameter;
            if (varFnParameter == null)
            {
                this.OperandClass = IdentifierClasses.Parameter;
                if (!(baseParameter is ArrayParameter))
                {
                    ReturnType = baseParameter.ValueType;
                }
                //if (baseParameter is BooleanParameter || baseParameter is ComplexParameter)
                //{
                //    ReturnType = baseParameter.ValueType;
                //}
            }
        }

        protected virtual void SetOperandValueFromParameter(object sender, EventArgs e)
        {
        }

        public BaseOperandToken(Token token, Type operandType = null)
               : base(token, operandType ?? typeof(double), IdentifierClasses.Literal)
        {
        }

        internal virtual void SetFromValidIdentifier(ValidIdentifier validIdentifier)
        {
            this.ValidIdentifier = validIdentifier;
            this.ReturnType = validIdentifier.IdentifierType;
            this.OperandClass = validIdentifier.TokenClass;
        }

        public override void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                if (this.BaseParameter != null)
                    this.BaseParameter.ValueChanged -= SetOperandValueFromParameter;
            }
        }
    }

    public class OperandToken : BaseOperandToken, IDisposable
    {
        public object OperandValue { get; internal set; }

        public OperandToken(Token token, Type operandType = null) 
               : base(token, operandType)
        {
            if (token.TokenType == TokenTypes.Number)
                this.OperandValue = double.Parse(token.Text);
            else if (token.TokenType == TokenTypes.String)
                OperandValue = ((StringToken)token).Value;
        }

        internal void SetAsConstant(object value)
        {
            OperandClass = IdentifierClasses.Constant;
            OperandValue = value;
        }

        internal override void SetFromValidIdentifier(ValidIdentifier validIdentifier)
        {
            base.SetFromValidIdentifier(validIdentifier);
            if (this.OperandValue == null)
            {
                this.OperandValue = validIdentifier.InitialValue;
            }
        }
    }

}
