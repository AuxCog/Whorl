using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class KeyEnumInfo
    {
        public object EnumValue { get; }
        public string EnumKey { get; }
        public Type ParametersClassType { get; }
        public bool ParametersAreGlobal { get; }
        public bool IsExclusive { get; }
        public FormulaSettings FormulaSettings { get; set; }

        public KeyEnumInfo(object enumValue, Type parametersClassType, bool paramsAreGlobal, bool isExclusive, FormulaSettings formulaSettings)
        {
            if (formulaSettings == null)
            {
                throw new NullReferenceException("formulaSettings cannot be null.");
            }
            EnumKey = Tools.GetEnumKey(enumValue);  //Validates enumValue is an enum value.
            EnumValue = enumValue;
            ParametersClassType = parametersClassType;
            ParametersAreGlobal = paramsAreGlobal;
            IsExclusive = isExclusive;
            FormulaSettings = formulaSettings;
        }

        public KeyEnumInfo(KeyEnumInfo source, FormulaSettings formulaSettings) 
               : this(source.EnumValue, source.ParametersClassType, 
                      source.ParametersAreGlobal, source.IsExclusive, formulaSettings)
        {
        }

        public object CreateParametersObject()
        {
            return Activator.CreateInstance(ParametersClassType);
        }
    }

    public class KeyEnumParameters: GuidKey
    {
        public KeyEnumInfo Parent { get; }
        public object ParametersObject { get; set; }
        public bool IsEnabled { get; set; } = true;

        public KeyEnumParameters(KeyEnumInfo parent, object parametersObject, bool createParamsObject = true)
        {
            if (parent == null)
                throw new NullReferenceException("parent cannot be null.");
            Parent = parent;
            if (parametersObject == null && createParamsObject)
            {
                parametersObject = Parent.CreateParametersObject();
            }
            ParametersObject = parametersObject;
        }

        public KeyEnumParameters(KeyEnumParameters source, FormulaSettings formulaSettings) : base(source)
        {
            Parent = new KeyEnumInfo(source.Parent, formulaSettings);
            IsEnabled = source.IsEnabled;
            if (source.ParametersObject != null)
            {
                ParametersObject = Parent.CreateParametersObject();
                formulaSettings.CopyCSharpParameters(source.ParametersObject, ParametersObject);
            }
        }

        public void UpdateDictionary(Dictionary<string, KeyEnumParameters> dict)
        {
            if (ParametersObject != null)
            {
                if (dict.TryGetValue(Parent.EnumKey, out var keyParams))
                {
                    SetKeyGuid(keyParams);
                    if (keyParams.ParametersObject != null)
                    {
                        //Copy parameters from keyParams.
                       Parent.FormulaSettings.CopyCSharpParameters(keyParams.ParametersObject, ParametersObject);
                    }
                }
            }
            dict[Parent.EnumKey] = this;
        }

        public override string ToString()
        {
            return Parent.EnumKey;
        }
    }
}
