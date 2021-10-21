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
        public object ParametersObject { get; }

        public KeyEnumInfo(object enumValue, object parametersObject)
        {
            if (enumValue == null || !enumValue.GetType().IsEnum)
            {
                throw new Exception("enumValue must be of Enum type.");
            }
            if (parametersObject == null)
                throw new NullReferenceException("parametersObject cannot be null.");
            EnumValue = enumValue;
            ParametersObject = parametersObject;
        }

    }
}
