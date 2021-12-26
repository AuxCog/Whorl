using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public abstract class BaseInfluenceParameter
    {
        public string PropertyName { get; set; }
        public PropertyInfo PropertyInfo { get; protected set; }
        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (value < 0)
                    throw new Exception("Index cannot be negative.");
                _index = value;
            }
        }
        public double ParameterValue
        {
            get => _ParameterValue;
            set
            {
                double val = value;
                if (ParameterInfoAttribute != null)
                {
                    if (val < ParameterInfoAttribute.MinValue)
                        val = ParameterInfoAttribute.MinValue;
                    else if (val > ParameterInfoAttribute.MaxValue)
                        val = ParameterInfoAttribute.MaxValue;
                }
                _ParameterValue = val;
            }
        }
        protected abstract double _ParameterValue { get; set; }
        public object ParamsObject { get; set; }
        public ParameterInfoAttribute ParameterInfoAttribute { get; private set; }
        public bool HaveReferences { get; protected set; }

        protected abstract string _Initialize();

        public virtual string Initialize(object paramsObject)
        {
            string errMessage = null;
            HaveReferences = true;
            ParamsObject = paramsObject;
            if (ParamsObject == null)
                errMessage = "Formula has no parameters.";
            else
            {
                PropertyInfo = ParamsObject.GetType().GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (PropertyInfo == null)
                {
                    errMessage = $"Didn't find parameter property named {PropertyName}.";
                }
                else
                {
                    errMessage = _Initialize();
                    if (errMessage == null)
                    {
                        ParameterInfoAttribute = PropertyInfo.GetCustomAttribute<ParameterInfoAttribute>();
                    }
                }
            }
            if (errMessage != null)
                HaveReferences = false;
            return errMessage;
        }
    }

    public class InfluenceParameter : BaseInfluenceParameter
    {
        protected override double _ParameterValue
        {
            get => (double)PropertyInfo.GetValue(ParamsObject);
            set => PropertyInfo.SetValue(ParamsObject, value);
        }

        protected override string _Initialize()
        {
            if (PropertyInfo.PropertyType != typeof(double))
            {
                Type type = PropertyInfo.PropertyType;
                PropertyInfo = null;
                return $"Parameter property named {PropertyName} is of type {type.FullName}, not double.";
            }
            return null;
        }
        public override string ToString()
        {
            return PropertyName;
        }
    }

    public class InfluenceArrayParameter : BaseInfluenceParameter
    {
        private double[] paramArray { get; set; }

        protected override double _ParameterValue
        {
            get => paramArray[Index];
            set => paramArray[Index] = value;
        }

        protected override string _Initialize()
        {
            if (PropertyInfo.PropertyType != typeof(double[]))
            {
                PropertyInfo = null;
                return $"Parameter property named {PropertyName} is not of type double[].";
            }
            paramArray = (double[])PropertyInfo.GetValue(ParamsObject);
            if (paramArray == null)
            {
                return "Parameter array is null.";
            }
            if (Index >= paramArray.Length)
                HaveReferences = false;
            return null;
        }

        public override string ToString()
        {
            return $"{PropertyName}[{Index + 1}]";
        }
    }
}
