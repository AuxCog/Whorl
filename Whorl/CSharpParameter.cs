using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public abstract class BaseCSharpParameter
    {
    }

    public interface IOptionsParameter
    {
        List<string> OptionTexts { get; }
        object SelectedOptionObject { get; set; }
        string SelectedText { get; set; }
        object GetOptionByText(string text);
        object[] GetInstances();
    }

    public class ParamOption<TValue>
    {
        public TValue Value { get; }
        public string Text { get; }

        public ParamOption(TValue value, string text)
        {
            Value = value;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public abstract class BaseOptionsParameter<TValue>: BaseCSharpParameter, IOptionsParameter
    {
        public List<ParamOption<TValue>> Options { get; protected set; }

        private ParamOption<TValue> _selectedOption;
        public ParamOption<TValue> SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("SelectedOption cannot be null.");
                if (!object.Equals(_selectedOption, value))
                {
                    _selectedOption = value;
                    SelectedText = _selectedOption.Text;
                    SelectedOptionChanged();
                }
            }
        }

        public bool SortOptionsByText { get; set; }

        public TValue SelectedValue
        {
            get
            {
                return SelectedOption.Value;
            }
        }

        public string SelectedText { get; set; }

        public List<string> OptionTexts { get; private set; }

        public virtual object[] GetInstances()
        {
            return null;
        }

        protected void FinishOptions(bool setSelected = true)
        {
            if (Options.Count == 0)
                throw new Exception("Options list is empty.");
            OptionTexts = Options.Select(opt => opt.Text).ToList();
            if (SortOptionsByText)
                OptionTexts.Sort();
            if (setSelected && SelectedOption == null)
                SelectedOption = Options.First();
        }

        public object SelectedOptionObject
        {
            get { return SelectedOption; }
            set
            {
                SelectedOption = value as ParamOption<TValue>;
            }
        }

        public object GetOptionByText(string text)
        {
            return Options?.Find(opt => opt.Text == text);
        }

        private string _defaultOptionText;
        public string DefaultOptionText
        {
            get { return _defaultOptionText; }
            set
            {
                _defaultOptionText = value;
                ParamOption<TValue> option;
                if (_defaultOptionText == null)
                    option = null;
                else
                    option = Options.Find(opt => opt.Text == _defaultOptionText);
                if (option == null)
                    option = Options.FirstOrDefault();
                if (option != null)
                    SelectedOption = option;
            }
        }

        public BaseOptionsParameter(bool sortOptionsByText = true)
        {
            SortOptionsByText = sortOptionsByText;
        }

        protected virtual void SelectedOptionChanged()
        {
        }

        public override string ToString()
        {
            return SelectedText;
        }
    }

    public class OptionsParameter<TValue>: BaseOptionsParameter<TValue> where TValue: class
    {
        public string NullText { get; }

        public OptionsParameter(string nullText = null) : base(sortOptionsByText: false)
        {
            NullText = nullText;
            if (NullText != null)
            {
                Options = new List<ParamOption<TValue>>() { new ParamOption<TValue>(null, NullText) };
                FinishOptions();
            }
        }

        public void SetOptions(IEnumerable<TValue> options, string defaultText = null, bool setSelected = true)
        {
            SetOptions(options.Select(v => new ParamOption<TValue>(v, v == null ? NullText : v.ToString())), 
                       defaultText, setSelected);
        }

        public void SetOptions(IEnumerable<ParamOption<TValue>> options, string defaultText = null, bool sortOptionsByText = false, bool setSelected = true)
        {
            Options = options.ToList();
            if (defaultText != null)
                DefaultOptionText = defaultText;
            FinishOptions(setSelected);
        }

    }

    public class EnumValuesParameter<TEnum>: BaseOptionsParameter<TEnum> where TEnum: struct
    {
        public EnumValuesParameter(TEnum? defaultValue = null, bool sortOptionsByText = false) : base(sortOptionsByText)
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enum type.");
            Options = (from TEnum v in Enum.GetValues(typeof(TEnum)) select new ParamOption<TEnum>(v, v.ToString())).ToList();
            DefaultOptionText = defaultValue?.ToString();
            FinishOptions();
        }
    }

    public class DerivFunc<T>
    {
        public Func<T,T> Fn { get; }
        public Func<T,T> DerivFn { get; }

        public DerivFunc(Func<T, T> fn, Func<T, T> derivFn)
        {
            Fn = fn;
            DerivFn = derivFn;
        }
    }

    public abstract class BaseFuncParameter<FuncT, ParamType>: BaseOptionsParameter<FuncT> //, IFuncParameter
    {
        public int ParamCount { get; }
        public FuncT Function { get; protected set; }
        public object[] Instances { get; }

        protected override void SelectedOptionChanged()
        {
            base.SelectedOptionChanged();
            Function = SelectedOption.Value;
        }

        public MathFunctionTypes? MathFunctionType { get; }

        public BaseFuncParameter(int paramCount, Type[] methodTypes, string defaultFunctionName, 
                                 MathFunctionTypes? mathFunctionType = null, object[] instances = null,
                                 bool addDefaultMethodTypes = true, bool addIdent = false)
        {
            ParamCount = paramCount;
            MathFunctionType = mathFunctionType;
            var typesList = new List<Type>();
            if (instances != null)
            {
                if (instances.Contains(null))
                    throw new NullReferenceException("instances cannot contain null.");
                typesList.AddRange(instances.Select(o => o.GetType()));
            }
            Instances = instances;
            if (methodTypes != null && methodTypes.Length > 0)
                typesList.AddRange(methodTypes);
            else if (addDefaultMethodTypes)
            {
                typesList.Add(typeof(CMath));
                if (MathFunctionType == null)
                    typesList.Add(typeof(Math));
            }
            Options = new List<ParamOption<FuncT>>();
            for (int i = 0; i < typesList.Count; i++)
            {
                Type type = typesList[i];
                object instance = (instances != null && i < instances.Length) ? instances[i] : null;
                var newMethods = GetValidMethods(type, forInstance: instance != null);
                if (i > 0)
                {
                    newMethods = newMethods.Where(mi => !Options.Exists(opt => opt.Text == GetMethodName(mi)));
                }
                Options.AddRange(newMethods.Select(mi => new ParamOption<FuncT>(GetFunction(mi, instance), GetMethodName(mi))));
            }
            if (addIdent)
            {
                var identMethodInfo = typeof(CMath).GetMethod(CMath.IdentName, BindingFlags.Public | BindingFlags.Static);
                if (!Options.Exists(o => o.Text == CMath.IdentName))
                {
                    Options.Add(new ParamOption<FuncT>(GetFunction(identMethodInfo), GetMethodName(identMethodInfo)));
                }
                if (defaultFunctionName == null)
                    defaultFunctionName = GetMethodName(identMethodInfo);
            }
            if (Options.Count == 0)
                throw new Exception($"No methods found for function parameter.");
            DefaultOptionText = defaultFunctionName;
            FinishOptions();
        }

        public BaseFuncParameter(int paramCount)
        {
            ParamCount = paramCount;
        }

        public override object[] GetInstances()
        {
            return Instances;
        }

        protected IEnumerable<MethodInfo> GetValidMethods(Type methodType, bool forInstance = false)
        {
            BindingFlags bindingFlags;
            if (forInstance)
                bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            else
                bindingFlags = BindingFlags.Public | BindingFlags.Static;
            return methodType.GetMethods(bindingFlags).Where(mi => MethodInfoIsValid(mi));
        }

        protected virtual FuncT GetFunction(MethodInfo methodInfo, object instance = null)
        {
            return default(FuncT);
        }

        protected virtual bool MethodInfoIsValid(MethodInfo methodInfo)
        {
            var parms = methodInfo.GetParameters();
            if (parms.Length != ParamCount || parms.Any(p => !p.ParameterType.IsAssignableFrom(typeof(ParamType))))
                return false;
            if (!typeof(ParamType).IsAssignableFrom(methodInfo.ReturnType))
                return false;
            if (MathFunctionType != null)
            {   
                if (methodInfo.GetCustomAttribute<MathFunctionAttribute>(attr => attr.MathFunctionType == MathFunctionType) == null)
                    return false;
            }
            if (methodInfo.GetCustomAttribute<ExcludeMethodAttribute>() != null)
                return false;
            return !methodInfo.IsSpecialName || methodInfo.GetCustomAttribute<MethodNameAttribute>() != null;
        }

        protected string GetMethodName(MethodInfo methodInfo)
        {
            var nameAttr = methodInfo.GetCustomAttribute<MethodNameAttribute>();
            return nameAttr != null ? nameAttr.Name : methodInfo.Name;
        }

        //private MethodInfo GetMethodInfo(string methodName)
        //{
        //    Type[] paramTypes = new Type[ParamCount];
        //    for (int i = 0; i < paramTypes.Length; i++)
        //        paramTypes[i] = typeof(ParamType);
        //    foreach (Type type in MethodTypes)
        //    {
        //        MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase, 
        //                                               null, CallingConventions.Standard, paramTypes, null);
        //        if (methodInfo != null)
        //            return methodInfo;
        //    }
        //    throw new Exception($"Function {methodName} was not found.");
        //}

        protected static Type[] GetMethodTypesArray(Type type)
        {
            return type == null ? null : new Type[] { type };
        }
    }

    public class DoubleFuncParameter: Func1Parameter<double>, IRenderingValues
    {
        public DoubleFuncLibrary FuncLibrary { get; }
        public RenderingValues RenderingValues
        {
            get => FuncLibrary?.RenderingValues;
            set
            {
                if (FuncLibrary != null)
                    FuncLibrary.RenderingValues = value;
            }
        }

        public DoubleFuncParameter(string defaultFunctionName = null, Type methodType = null, MathFunctionTypes? mathFunctionType = null, object[] instances = null,
                                   bool addDefaultMethodTypes = true)
                                   : base(defaultFunctionName, methodType, mathFunctionType, instances, addDefaultMethodTypes)
        {
            if (instances != null)
                FuncLibrary = instances.Select(o => o as DoubleFuncLibrary).FirstOrDefault(o => o != null);
        }

        protected override void SelectedOptionChanged()
        {
            if (SelectedOption == null) return;
            bool isSet = false;
            if (FuncLibrary != null)
            {
                if (!Instances.SelectMany(o => GetValidMethods(o.GetType(), forInstance: true))
                                         .Any(m => m.Name == SelectedText))
                {
                    FuncLibrary.SetBaseFunction(SelectedOption.Value);
                    Function = FuncLibrary.DefaultFunction;
                    isSet = true;
                }
            }
            if (!isSet)
                base.SelectedOptionChanged();
        }
    }

    public class Func1Parameter<T>: BaseFuncParameter<Func<T, T>, T>
    {
        public Func1Parameter(string defaultFunctionName = null, Type methodType = null, MathFunctionTypes? mathFunctionType = null, object[] instances = null,
                              bool addDefaultMethodTypes = true) 
            : base(paramCount: 1, GetMethodTypesArray(methodType), defaultFunctionName, mathFunctionType, instances, 
                   addDefaultMethodTypes, addIdent: typeof(T) == typeof(double))
        {
        }

        protected override Func<T, T> GetFunction(MethodInfo methodInfo, object instance = null)
        {
            return (Func<T, T>)Delegate.CreateDelegate(typeof(Func<T, T>), instance, methodInfo);
        }
    }

    public class Func2Parameter<T> : BaseFuncParameter<Func<T, T, T>, T>
    {
        public Func2Parameter(string defaultFunctionName = null, Type methodType = null, MathFunctionTypes? mathFunctionType = null, object[] instances = null,
                              bool addDefaultMethodTypes = true)
            : base(paramCount: 2, GetMethodTypesArray(methodType), defaultFunctionName, mathFunctionType, instances, addDefaultMethodTypes)
        {
        }

        protected override Func<T, T, T> GetFunction(MethodInfo methodInfo, object instance = null)
        {
            return (Func<T, T, T>)Delegate.CreateDelegate(typeof(Func<T, T, T>), instance, methodInfo);
        }
    }

    public class DerivFuncParameter<T>: BaseFuncParameter<DerivFunc<T>, T>
    {
        public Func<T, T> Fn { get; private set; }
        public Func<T, T> DerivFn { get; private set; }

        protected override void SelectedOptionChanged()
        {
            base.SelectedOptionChanged();
            if (Function != null)
            {
                Fn = Function.Fn;
                DerivFn = Function.DerivFn;
            }
        }

        public DerivFuncParameter(string defaultFunctionName = null, Type methodType = null) : base(paramCount: 1)
        {
            if (methodType == null)
                methodType = typeof(T);
            Options = new List<ParamOption<DerivFunc<T>>>();
            foreach (MethodInfo methodInfo in GetValidMethods(methodType))
            {
                var attr = methodInfo.GetCustomAttribute<DerivMethodNameAttribute>();
                if (attr == null)
                    continue;
                MethodInfo derivMethod = methodType.GetMethod(attr.DerivMethodName, 
                                         BindingFlags.Public | BindingFlags.Static);
                if (derivMethod == null)
                    continue;
                var fn = (Func<T, T>)Delegate.CreateDelegate(typeof(Func<T, T>), methodInfo);
                var fnDeriv = (Func<T, T>)Delegate.CreateDelegate(typeof(Func<T, T>), derivMethod);
                Options.Add(new ParamOption<DerivFunc<T>>(new DerivFunc<T>(fn, fnDeriv), GetMethodName(methodInfo)));
            }
            DefaultOptionText = defaultFunctionName;
            FinishOptions();
        }

    }

    public class RandomParameter: BaseCSharpParameter
    {
        public double Value { get; set; }
        public RandomRange RandomRange { get; } = new RandomRange();
    }
}
