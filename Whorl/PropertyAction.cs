using ParserEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class SimplePropertyAction: BasePropertyAction
    {
        public object SourceObject { get; set; }
        public PropertyInfo PropertyInfo { get; protected set; }

        public void SetPropertyInfoAndObject(PropertyInfo propertyInfo, object obj)
        {
            PropertyInfo = propertyInfo;
            SourceObject = obj;
        }

        public override bool SetValue()
        {
            if (SourceObject == null || PropertyInfo == null)
                return false;
            PropertyValue = PropertyInfo.GetValue(SourceObject);
            return true;
        }

        public override string GetPropertyName()
        {
            return PropertyInfo?.Name;
        }

        public override IncrementStatus SetValue(object value)
        {
            if (!(value is double))
                return IncrementStatus.InvalidType;
            PropertyInfo.SetValue(SourceObject, value);
            PropertyValue = value;
            return IncrementStatus.Success;
        }

        protected override IncrementStatus SetNumericValue(double val)
        {
            PropertyInfo.SetValue(SourceObject, val);
            PropertyValue = val;
            return IncrementStatus.Success;
        }

        public override IncrementStatus GetStatus(bool checkValue = true)
        {
            IncrementStatus status;
            if (SourceObject == null || PropertyInfo == null || (checkValue && PropertyValue == null))
                status = IncrementStatus.ObjectIsNull;
            else if (PropertyInfo.PropertyType == typeof(double))
                status = IncrementStatus.Success;
            else
                status = IncrementStatus.InvalidType;
            return status;
        }
    }


    public class PropertyAction: SimplePropertyAction
    {
        public List<PropertyInfo> PropertyInfoPath { get; } = new List<PropertyInfo>();

        public static object GetSourceObject(List<PropertyInfo> path, object topObject)
        {
            object obj = topObject;
            for (int i = 0; i < path.Count - 1 && obj != null; i++)
            {
                obj = path[i].GetValue(obj);
            }
            return obj;
        }

        public void SetPathAndObject(IEnumerable<PropertyInfo> path, object topObject)
        {
            PropertyInfoPath.Clear();
            PropertyInfoPath.AddRange(path);
            PropertyInfo = PropertyInfoPath.LastOrDefault();
            PropertyActionAttribute attribute = null;
            if (PropertyInfo != null)
            {
                attribute = PropertyActionAttribute.GetPropertyAttribute(PropertyInfo);
            }
            if (attribute != null)
            {
                MaxValue = attribute.MaxValue;
                MinValue = attribute.MinValue;
            }
            else
            {
                MaxValue = double.MaxValue;
                MinValue = double.MinValue;
            }
            SetSourceObject(topObject);
        }

        public void SetSourceObject(object topObject)
        {
            SourceObject = GetSourceObject(PropertyInfoPath, topObject);
        }

        public override IncrementStatus SetValue(object value)
        {
            IncrementStatus status = GetStatus(checkValue: false);
            if (status == IncrementStatus.Success)
            {
                double dVal;
                if (value is double)
                    dVal = (double)value;
                else if (value is int)
                    dVal = Convert.ToDouble(value);
                else
                {
                    dVal = 0;
                    status = IncrementStatus.InvalidType;
                }
                if (status == IncrementStatus.Success)
                    status = SetNumericValue(dVal);
            }
            return status;
        }

        protected override IncrementStatus SetNumericValue(double val)
        {
            IncrementStatus status = IncrementStatus.Success;
            if (val > MaxValue)
                status = IncrementStatus.GreaterThanMax;
            else if (val < MinValue)
                status = IncrementStatus.LessThanMin;
            else if (PropertyInfo.PropertyType == typeof(double))
                SetPropertyValue(val);
            else if (PropertyInfo.PropertyType == typeof(int))
                SetPropertyValue(Convert.ToInt32(Math.Round(val)));
            else
                status = IncrementStatus.InvalidType;
            //if (status == IncrementStatus.Success)
            //    SetValue();
            return status;
        }

        private void SetPropertyValue(object value)
        {
            PropertyInfo.SetValue(SourceObject, value);
            PropertyValue = value;
        }

        public override IncrementStatus GetStatus(bool checkValue = true)
        {
            IncrementStatus status;
            if (SourceObject == null || PropertyInfo == null || (checkValue && PropertyValue == null))
                status = IncrementStatus.ObjectIsNull;
            else if (PropertyInfo.PropertyType == typeof(double) || PropertyInfo.PropertyType == typeof(int))
                status = IncrementStatus.Success;
            else
                status = IncrementStatus.InvalidType;
            return status;
        }
    }

    public class ArrayAction : BasePropertyAction
    {
        public Array Array { get; }
        public int Index { get; }
        public PropertyInfo PropertyInfo { get; }
        private string propertyName { get; }

        public ArrayAction(Array array, int index, PropertyInfo propertyInfo)
        {
            if (array == null)
                throw new NullReferenceException("array is null.");
            else if (array.GetType().GetElementType() != typeof(double))
                throw new Exception("Array elements must be of type double.");
            else if (index < 0 || index >= array.Length)
                throw new Exception($"Index {index} is out of bounds.  Array length = {array.Length}.");
            Array = array;
            Index = index;
            PropertyInfo = propertyInfo;
            propertyName = $"{propertyInfo.Name}[{index + 1}]";
        }

        public override string GetPropertyName()
        {
            return propertyName;
        }

        public override IncrementStatus GetStatus(bool checkValue = true)
        {
            return IncrementStatus.Success;
        }

        public override bool SetValue()
        {
            PropertyValue = Array.GetValue(Index);
            return true;
        }

        public override IncrementStatus SetValue(object value)
        {
            if (value is double)
            {
                SetNumericValue((double)value);
                return IncrementStatus.Success;
            }
            else
            {
                return IncrementStatus.InvalidType;
            }
        }

        protected override IncrementStatus SetNumericValue(double val)
        {
            Array.SetValue(val, Index);
            PropertyValue = val;
            return IncrementStatus.Success;
        }
    }

    public class ParameterAction: BasePropertyAction
    {
        private Parameter parameter;
        public Parameter Parameter
        {
            get { return parameter; }
            set
            {
                parameter = value;
                if (Parameter != null)
                {
                    MinValue = Parameter.MinValue ?? double.MinValue;
                    MaxValue = Parameter.MaxValue ?? double.MaxValue;
                }
            }
        }

        public override string GetPropertyName()
        {
            return Parameter?.ParameterName;
        }

        public override IncrementStatus GetStatus(bool checkValue = true)
        {
            if (Parameter == null)
                return IncrementStatus.ObjectIsNull;
            else if (checkValue && PropertyValue == null)
                return IncrementStatus.ObjectIsNull;
            else
                return IncrementStatus.Success;
        }

        public override bool SetValue()
        {
            if (Parameter != null)
                PropertyValue = Parameter.UsedValue;
            return Parameter != null;
        }

        public override IncrementStatus SetValue(object value)
        {
            if (value is double)
            {
                return SetNumericValue((double)value);
            }
            else
                return IncrementStatus.InvalidType;
        }

        protected override IncrementStatus SetNumericValue(double val)
        {
            if (val < MinValue)
                return IncrementStatus.LessThanMin;
            else if (val > MaxValue)
                return IncrementStatus.GreaterThanMax;
            else
            {
                Parameter.Value = val;
                PropertyValue = val;
                return IncrementStatus.Success;
            }
        }
    }

    public abstract class BasePropertyAction
    {
        public enum IncrementStatus
        {
            Success,
            ObjectIsNull,
            InvalidType,
            GreaterThanMax,
            LessThanMin
        }
        public double Increment { get; set; }
        public int Steps { get; set; }
        public int CurrentStep { get; set; }
        public double MaxValue { get; protected set; }
        public double MinValue { get; protected set; }
        public object Tag { get; set; }
        public object PropertyValue { get; set; }

        public abstract IncrementStatus GetStatus(bool checkValue = true);
        protected abstract IncrementStatus SetNumericValue(double val);
        public abstract string GetPropertyName();
        public abstract bool SetValue();
        public abstract IncrementStatus SetValue(object value);

        public IncrementStatus IncrementValue()
        {
            IncrementStatus status = GetStatus();
            if (status == IncrementStatus.Success)
            {
                double val = Convert.ToDouble(PropertyValue) + Increment;
                status = SetNumericValue(val);
            }
            return status;
        }
    }
}
