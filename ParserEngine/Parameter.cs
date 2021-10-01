using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserEngine
{
    public enum CustomParameterTypes
    {
        None,
        RandomRange
    }

    public abstract class BaseParameter: IDisposable
    {
        public string ParameterName { get; }
        public virtual Type ValueType { get; } = typeof(object);
        public abstract bool IsArray { get; }
        public Expression ParentExpression { get; private set; }
        public string Label { get; internal set; }
        public bool IsOutputParameter { get; internal set; }
        public bool Visible { get; internal set; } = true;
        protected List<ParameterChoice> parameterChoices { get; set; }

        private object oValue;
        public virtual object Value
        {
            get { return oValue; }
            set
            {
                if (value != null && !(value.GetType() == ValueType || value.GetType().IsSubclassOf(ValueType)))
                {
                    throw new Exception($"Parameter value's type is invalid: {value}");
                }
                object oNewValue = ValidateValue(value, nameof(Value));
                if (!object.Equals(oValue, oNewValue))
                {
                    oValue = oNewValue;
                    OnValueChanged();
                }
            }
        }

        public virtual object UsedValue
        {
            get { return Value; }
            protected set { Value = value; }
        }

        public bool HasChoices
        {
            get { return ValueChoices != null; }
        }

        public virtual IEnumerable<ParameterChoice> ValueChoices
        {
            get { return parameterChoices; }
        }

        public ParameterChoice GetValueChoice(object value)
        {
            if (ValueChoices == null)
                return null;
            else
                return ValueChoices.Where(pc => object.Equals(value, pc.Value)).FirstOrDefault();
        }

        public virtual string ValueString
        {
            get { return Value?.ToString(); }
        }

        public string GetLabel()
        {
            return Label ?? ParameterName;
        }

        public EventHandler ValueChanged;

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
            EvaluateDependentExpressions();
        }

        internal void EvaluateDependentExpressions()
        {
            if (ParentExpression != null)
            {
                var dependentExpressions = GetDependentExpressions();
                if (dependentExpressions != null)
                {
                    foreach (Token[] expression in dependentExpressions)
                    {
                        ParentExpression.EvalExpression(expression);
                    }
                }
            }
        }

        public BaseParameter(string parameterName, Expression parentExpression)
        {
            if (parentExpression == null)
                throw new NullReferenceException("parentExpression cannot be null.");
            ParameterName = parameterName;
            ParentExpression = parentExpression;
        }

        internal virtual ParameterChoice AddParameterChoice(object value, string text)
        {
            if (value != null && value.GetType() != ValueType)
            {
                throw new Exception("Invalid value type for AddParameterChoice.");
            }
            if (parameterChoices == null)
                parameterChoices = new List<ParameterChoice>();
            else if (GetParameterChoice(text) != null)
            {
                throw new Exception($"Duplicate parameter choice: {text}");
            }
            ParameterChoice parameterChoice = new ParameterChoice() { Value = value, Text = text };
            parameterChoices.Add(parameterChoice);
            return parameterChoice;
        }

        internal virtual ParameterChoice AddParameterChoice(string text)
        {
            return AddParameterChoice(null, text);
        }

        public bool SetValueFromParameterChoice(ParameterChoice choice)
        {
            bool changed = !object.Equals(Value, choice.Value);
            if (changed)
            {
                Value = choice.Value;
            }
            return changed;
        }

        public virtual IEnumerable<Token[]> GetDependentExpressions()
        {
            return null;
        }

        public virtual IEnumerable<BaseParameter> GetParameters()
        {
            yield return this;
        }

        internal ParameterChoice GetParameterChoice(string text)
        {
            if (ValueChoices == null)
                return null;
            return ValueChoices.Where(pc => text.Equals(pc.Text, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }
        
        internal virtual void FinalizeParameter()
        {
        }

        protected virtual object ValidateValue(object value, string propertyName,
                                               bool forValueProperty = true)
        {
            if (ValueChoices != null)
            {
                if (!ValueChoices.Where(pc => object.Equals(value, pc.Value)).Any())
                {
                    var firstChoice = ValueChoices.FirstOrDefault();
                    if (firstChoice != null)
                    {
                        value = firstChoice.Value;
                    }
                }
            }
            return value;
        }

        public bool Disposed { get; private set; }

        public virtual void Dispose()
        {
            if (Disposed)
                return;
            ParentExpression = null;
            Disposed = true;
        }

        public override string ToString()
        {
            return ParameterName;
        }
    }

    public class ArrayParameter : BaseParameter
    {
        public override bool IsArray { get { return true; } }
        public ArrayDict ArrayValue { get; }

        public ArrayParameter(string parameterName, Expression parentExpression) : base(parameterName, parentExpression)
        {
            ArrayValue = new ArrayDict(parameterName);
        }
    }

    public class DoubleParameter: BaseParameter
    {
        public override bool IsArray { get { return false; } }
        public override Type ValueType
        {
            get { return typeof(double); }
        }

        public override object Value
        {
            get { return base.Value; }
            set
            {
                if (value != null && !(value is double))
                    throw new Exception($"Value is not of type double: {value}");
                base.Value = value;
            }
        }

        public DoubleParameter(string parameterName, Expression parentExpression) : base(parameterName, parentExpression)
        {
        }
    }

    public class BooleanParameter: BaseParameter, IDependentExpressions
    {
        public override bool IsArray { get { return false; } }

        public override Type ValueType
        {
            get { return typeof(bool); }
        }

        private bool initialized;

        public bool BooleanValue
        {
            get { return (bool?)Value == true; }
            set
            {
                Value = value;
                initialized = true;
            }
        }

        private bool defaultValue;
        public bool DefaultValue
        {
            get { return defaultValue; }
            set
            {
                defaultValue = value;
                if (!initialized)
                    Value = defaultValue;
            }
        }

        private List<Token[]> DependentExpressions { get; set; }

        public void AddDependentExpression(Token[] expression)
        {
            if (DependentExpressions == null)
                DependentExpressions = new List<Token[]>();
            DependentExpressions.Add(expression);
        }

        public override IEnumerable<Token[]> GetDependentExpressions()
        {
            return DependentExpressions;
        }

        public BooleanParameter(string parameterName, Expression parentExpression) : base(parameterName, parentExpression)
        {
            base.Value = false;
        }
    }

    public class Parameter: DoubleParameter, IDependentExpressions
    {
        private double? defaultValue;
        public int? DecimalPlaces { get; set; }

        public double? DefaultValue
        {
            get { return defaultValue; }
            set
            {
                ValidateValue(value, nameof(DefaultValue), forValueProperty: false);
                if (Value == null)
                {
                    //Set Value property to default value, after validating:
                    Value = value;
                }
                defaultValue = value;
            }
        }
        public bool HandleDependentExpressions { get; }
        private object _usedValue;
        public override object UsedValue 
        {
            get => _usedValue;
            protected set => _usedValue = value;
        }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? ImprovMinValue { get; set; }
        public double? ImprovMaxValue { get; set; }
        public bool Locked { get; set; }  //Locked to changes by improvisation.
        public Guid Guid { get; set; }

        private List<Token[]> DependentExpressions { get; set; }

        public void AddDependentExpression(Token[] expression)
        {
            if (HandleDependentExpressions)
            {
                if (DependentExpressions == null)
                    DependentExpressions = new List<Token[]>();
                DependentExpressions.Add(expression);
            }
        }

        public override IEnumerable<Token[]> GetDependentExpressions()
        {
            return DependentExpressions;
        }

        internal override ParameterChoice AddParameterChoice(object value, string text)
        {
            if (value == null)
            {
                if (parameterChoices != null && parameterChoices.Any())
                    value = 1D + parameterChoices.Select(ch => (double)ch.Value).Max();
                else
                    value = 1D;
            }
            return base.AddParameterChoice(value, text);
        }

        public Parameter(string parameterName, Expression parentExpression, bool handleDependentExpressions = true) 
             : base(parameterName, parentExpression)
        {
            HandleDependentExpressions = handleDependentExpressions;
            this.Guid = Guid.NewGuid();
        }

        public void SetUsedValue(double usedValue)
        {
            if (MinValue != null && usedValue < MinValue)
                usedValue = (double)MinValue;
            else if (MaxValue != null && usedValue > MaxValue)
                usedValue = (double)MaxValue;
            if (DecimalPlaces != null)
                usedValue = Math.Round(usedValue, (int)DecimalPlaces);
            UsedValue = usedValue;
        }

        protected override object ValidateValue(object value, string propertyName,
                                               bool forValueProperty = true)
        {
            object validValue = base.ValidateValue(value, propertyName, forValueProperty);
            if (validValue == null)
                UsedValue = null;
            else
            {
                double dVal = (double)validValue;
                if (MinValue != null && dVal < MinValue)
                {
                    throw new Exception(
                        $"@{ParameterName} {propertyName} cannot be less than {MinValue}.");
                }
                if (MaxValue != null && dVal > MaxValue)
                {
                    throw new Exception(
                        $"@{ParameterName} {propertyName} cannot be greater than {MaxValue}.");
                }
                if (DecimalPlaces == null)
                    UsedValue = dVal;
                else
                    UsedValue = Math.Round(dVal, (int)DecimalPlaces);
            }
            return validValue;
        }

        internal override void FinalizeParameter()
        {
            base.FinalizeParameter();
            if (MinValue != null && MaxValue != null && (double)MinValue > (double)MaxValue)
            {
                throw new Exception("Parameter MaxValue is less than MinValue.");
            }
            if (DefaultValue == null)
            {
                double val = 0;
                if (MinValue != null && val < (double)MinValue)
                    val = (double)MinValue;
                else if (MaxValue != null && val > (double)MaxValue)
                    val = (double)MaxValue;
                DefaultValue = val;
            }
            if (parameterChoices != null)
            {
                if (DefaultValue == null || GetValueChoice(DefaultValue) == null)
                {
                    ParameterChoice firstChoice = parameterChoices.FirstOrDefault();
                    if (firstChoice != null && firstChoice.Value is double)
                    {
                        Value = DefaultValue = (double)firstChoice.Value;
                    }
                }
            }
        }
    }

    public class ComplexParameter: BaseParameter, IDependentExpressions
    {
        public Parameter RealParam { get; }
        public Parameter ImagParam { get; }
        public override bool IsArray { get { return false; } }
        public override Type ValueType { get { return typeof(Complex); } }
        private Complex? _defaultValue;
        private bool handleEvents = true;
        public Complex? DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                if (!object.Equals(_defaultValue, value))
                {
                    _defaultValue = value;
                    if (value != null)
                    {
                        handleEvents = false;
                        try
                        {
                            Complex val = (Complex)value;
                            RealParam.DefaultValue = val.Re;
                            ImagParam.DefaultValue = val.Im;
                            RealParam.Value = val.Re;
                            ImagParam.Value = val.Im;
                            if (Value == null)
                            {
                                Value = val;
                            }
                        }
                        finally
                        {
                            handleEvents = true;
                        }
                    }
                }
            }
        }

        private void SetValue(Complex val)
        {
            handleEvents = false;
            try
            {
                Value = val;
            }
            finally { handleEvents = true; }
        }

        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            if (handleEvents && Value is Complex)
            {
                Complex val = (Complex)Value;
                RealParam.Value = val.Re;
                ImagParam.Value = val.Im;
            }
        }

        private List<Token[]> DependentExpressions { get; set; }

        public void AddDependentExpression(Token[] expression)
        {
            if (DependentExpressions == null)
                DependentExpressions = new List<Token[]>();
            DependentExpressions.Add(expression);
        }

        public override IEnumerable<Token[]> GetDependentExpressions()
        {
            return DependentExpressions;
        }

        public override IEnumerable<BaseParameter> GetParameters()
        {
            if (HasChoices)
            {
                yield return this;
            }
            else
            {
                yield return RealParam;
                yield return ImagParam;
            }
        }

        public ComplexParameter(string parameterName, Expression parentExpression) : base(parameterName, parentExpression)
        {
            RealParam = new Parameter(parameterName, parentExpression, handleDependentExpressions: false);
            ImagParam = new Parameter(parameterName, parentExpression, handleDependentExpressions: false);
            string label = Label ?? parameterName;
            RealParam.Label = label + ":Re";
            ImagParam.Label = label + ":Im";
            RealParam.ValueChanged += ParamValueChanged;
            ImagParam.ValueChanged += ParamValueChanged;
        }

        private void ParamValueChanged(object sender, EventArgs e)
        {
            if (RealParam.Value is double && ImagParam.Value is double)
                SetValue(new Complex((double)RealParam.Value, (double)ImagParam.Value));
        }

        public override void Dispose()
        {
            if (Disposed)
                return;
            base.Dispose();
            RealParam.ValueChanged -= ParamValueChanged;
            ImagParam.ValueChanged -= ParamValueChanged;
        }
    }

    public class CustomParameter: DoubleParameter
    {
        public object Context { get; }
        public CustomParameterTypes CustomType { get; }
        public PropertyInfo SelectedPropertyInfo { get; set; }

        public CustomParameter(string parameterName, CustomParameterTypes customType, Expression parentExpression) 
               : base(parameterName, parentExpression)
        {
            if (customType == CustomParameterTypes.None)
                throw new ArgumentException("CustomParameter cannot have type None.",
                                            nameof(customType));
            CustomType = customType;
            if (CustomType == CustomParameterTypes.RandomRange)
            {
                Context = new RandomRange();
            }
        }

        /// <summary>
        /// Return custom type enum for typeName, or None if not found.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        internal static CustomParameterTypes ParseType(string typeName)
        {
            CustomParameterTypes customType;
            if (!ExpressionParser.IgnoreCase)
            {
                if (!Enum.TryParse(typeName, out customType))
                    customType = CustomParameterTypes.None;
            }
            else
            {
                IEnumerable<CustomParameterTypes> type =
                    (from CustomParameterTypes t in
                     Enum.GetValues(typeof(CustomParameterTypes))
                     where typeName.Equals(t.ToString(), StringComparison.OrdinalIgnoreCase)
                     select t);
                if (type.Any())
                    customType = type.First();
                else
                    customType = CustomParameterTypes.None;
            }
            return customType;
        }
    }


    public class VarFunctionParameter: BaseParameter
    {
        public override bool IsArray { get { return false; } }

        public override Type ValueType
        {
            get { return typeof(MethodInfo); }
        }

        /// <summary>
        /// Dictionary of methods with 1 parameter.
        /// </summary>
        private static Dictionary<string, MethodInfo> staticMethodsDict;
        public List<Type> CustomMethodTypes { get; private set; }
        private Dictionary<string, MethodInfo> methodsDict = staticMethodsDict;

        static private void PopulateMethodsDict(Dictionary<string, MethodInfo> methodsDict, Type classType, int parameterCount)
        {
            foreach (var mi in classType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var parms = mi.GetParameters();
                if (mi.ReturnType == typeof(double) && parms.Length == parameterCount && 
                    !parms.Where(p => p.ParameterType != typeof(double)).Any())
                {
                    try
                    {
                        methodsDict.Add(mi.Name, mi);
                    }
                    catch
                    {
                        if (methodsDict.ContainsKey(mi.Name))
                        {
                            throw new Exception($"The method {mi.Name} has multiple signatures.");
                        }
                    }
                }
            }
        }

        static public void PopulateMethodsDict(Dictionary<string, MethodInfo> methodsDict, int parameterCount,
                                                IEnumerable<Type> methodTypes)
        {
            foreach (Type methodsType in methodTypes)
            {
                PopulateMethodsDict(methodsDict, methodsType, parameterCount);
            }
        }

        static VarFunctionParameter()
        {
            staticMethodsDict = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
            PopulateMethodsDict(staticMethodsDict, 1, ExpressionParser.StaticMethodsTypes);
        }

        //public IEnumerable<string> GetValidMethodNames()
        //{
        //    return methodsDict.Keys.OrderBy(n => n);
        //}

        public override object Value
        {
            get { return base.Value; }
            set
            {
                //Setting MemberInfo sets base.Value to method's MethodInfo.
                if (value is MethodInfo)
                    MethodInfo = (MethodInfo)value;
                else
                    MethodInfo = GetFunctionMethod(value as string);  //Never returns null.
                initialized = true;
            }
        }

        public string DefaultValue
        {
            get { return DefaultFunctionMethod?.Name; }
            private set
            {
                DefaultFunctionMethod = GetFunctionMethod(value);
                if (!initialized || MethodInfo == null)
                {
                    MethodInfo = DefaultFunctionMethod;
                }
            }
        }

        public int ParameterCount { get; private set; } = 1;

        public void SetProperties(int parameterCount = 1, string defaultValue = null, IEnumerable<Type> customMethodTypes = null)
        {
            if (parameterCount == 1 && customMethodTypes == null)
            {
                methodsDict = staticMethodsDict;
            }
            else
            {
                ParameterCount = parameterCount;
                if (customMethodTypes == null)
                {
                    customMethodTypes = ExpressionParser.StaticMethodsTypes;
                    this.CustomMethodTypes = null;
                }
                else
                    this.CustomMethodTypes = new List<Type>(customMethodTypes);
                methodsDict = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
                PopulateMethodsDict(methodsDict, parameterCount, customMethodTypes);
                if (parameterCount != 1)
                {
                    DefaultFunctionMethod = methodsDict.Values.FirstOrDefault();
                    if (DefaultFunctionMethod != null && (!initialized || MethodInfo == null))
                        MethodInfo = DefaultFunctionMethod;
                }
            }
            if (defaultValue != null)
                DefaultValue = defaultValue;
        }

        private MethodInfo _methodInfo;
        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
            private set
            {
                if (value == null)
                {
                    throw new NullReferenceException($"{nameof(MethodInfo)} cannot be null.");
                }
                if (value.GetParameters().Length != ParameterCount)
                {
                    throw new Exception($"The function {value.Name} does not have {ParameterCount} values.");
                }
                _methodInfo = value;
                base.Value = _methodInfo;
            }
        }

        public override string ValueString
        {
            get { return MethodInfo?.Name; }
        }

        public MethodInfo DefaultFunctionMethod { get; private set; }

        private bool initialized;

        public VarFunctionParameter(string parameterName, Expression parentExpression) : base(parameterName, parentExpression)
        {
            DefaultValue = nameof(EvalMethods.Ident);
        }

        internal override void FinalizeParameter()
        {
            base.FinalizeParameter();
            if (parameterChoices == null)
            {
                parameterChoices = methodsDict.Values.OrderBy(mi => mi.Name)
                                                     .Select(mi => new ParameterChoice() { Value = mi, Text = mi.Name }).ToList();
            }
            else
            {
                foreach (var paramChoice in parameterChoices)
                {
                    MethodInfo functionMethod = GetFunctionMethod(paramChoice.Text, throwException: false);
                    if (functionMethod == null)
                    {
                        throw new Exception($"Invalid choices function for parameter @{ParameterName}: {paramChoice.Text}");
                    }
                    else
                    {
                        paramChoice.Value = functionMethod;
                        paramChoice.Text = functionMethod.Name;
                    }
                }
                if (DefaultValue == null || GetValueChoice(DefaultValue) == null)
                {
                    ParameterChoice firstChoice = parameterChoices.FirstOrDefault();
                    if (firstChoice != null)
                    {
                        Value = DefaultValue = firstChoice.Text;
                    }
                }
            }
        }

        private MethodInfo GetFunctionMethod(string fnName, bool throwException = true)
        {
            MethodInfo fnMethod = null;
            if (fnName != null)
            {
                methodsDict.TryGetValue(fnName, out fnMethod);
            }
            if (fnMethod == null && throwException)
                throw new Exception($"Invalid VarFunctionParameter method name: {fnName}");
            return fnMethod;
        }

    }
}
