using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public enum ParameterSources
    {
        None,
        DistanceCount
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterLabelAttribute : Attribute
    {
        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value ?? string.Empty;
            }
        }

        public ParameterLabelAttribute(string label)
        {
            Label = label;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterInfoAttribute : Attribute
    {
        public double MinValue { get; set; } = double.MinValue;
        public double MaxValue { get; set; } = double.MaxValue;
        private ParameterSources _parameterSource = ParameterSources.None;
        public ParameterSources ParameterSource
        {
            get { return _parameterSource; }
            set
            {
                _parameterSource = value;
                if (_parameterSource != ParameterSources.None)
                    Enabled = false;
            }
        }
        public bool UpdateParametersOnChange { get; set; }
        public bool Enabled { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ArrayBaseNameAttribute : Attribute
    {
        public string BaseName { get; set; }
        public int StartNumber { get; set; }
        public ArrayBaseNameAttribute(string baseName, int startNumber = 1)
        {
            BaseName = baseName;
            StartNumber = startNumber;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PreviousNameAttribute : Attribute
    {
        public string PreviousName { get; set; }

        public PreviousNameAttribute(string previousName)
        {
            PreviousName = previousName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ParmsPropertyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class InitialSettingValueAttribute : Attribute
    {
        public string Value { get; set; }

        public InitialSettingValueAttribute(string value)
        {
            Value = value;
        }
    }
}
