using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class PropertyInfoPath
    {
        public class PropInfo
        {
            public PropertyInfo PropertyInfo { get; set; }
            public int Index { get; set; } = -1;

            public PropInfo(PropertyInfo propInfo, int index)
            {
                PropertyInfo = propInfo;
                Index = index;
            }
        }

        public List<PropInfo> Path { get; private set; }
        public object Target2 { get; private set; }

        private BindingFlags bindingFlags;
        private object target;
        private HashSet<object> traversedObjects;

        public bool FindPropertyInfoPath(object o1, object o2, object target, bool includeNonPublic = false)
        {
            if (o1.GetType() != o2.GetType())
                throw new ArgumentException("o1 and o2 are not of same type.");
            this.target = target;
            Path = new List<PropInfo>();
            bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (includeNonPublic)
                bindingFlags |= BindingFlags.NonPublic;
            traversedObjects = new HashSet<object>();
            bool success = FindPath(o1, o2);
            traversedObjects.Clear();
            if (success)
                Path.Reverse();
            else
                Target2 = null;
            return success;
        }

        private bool FindPath(object o1, object o2)
        {
            if (o1 == null || o2 == null)
                return false;
            if (o1 == target)
            {
                Target2 = o2;
                return true;
            }
            if (!traversedObjects.Add(o1))
                return false;
            bool success = false;
            foreach (PropertyInfo prpInfo in o1.GetType().GetProperties(bindingFlags))
            {
                if (prpInfo.PropertyType.IsClass)
                {
                    Type elType;
                    if (Tools.IsListType(prpInfo.PropertyType))
                        elType = Tools.GetListType(prpInfo.PropertyType);
                    else
                        elType = prpInfo.PropertyType;
                    if (!typeof(BaseObject).IsAssignableFrom(elType))
                        continue;
                    if (prpInfo.GetIndexParameters().Length != 0)
                        continue;
                    object val1 = prpInfo.GetValue(o1);
                    object val2 = prpInfo.GetValue(o2);
                    if (val1 == null || val2 == null)
                        continue;
                    var list1 = val1 as System.Collections.IList;
                    int index = -1;
                    if (list1 != null)
                    {
                        var list2 = val2 as System.Collections.IList;
                        int iMax = Math.Min(list1.Count, list2.Count);
                        for (int i = 0; i < iMax; i++)
                        {
                            if (FindPath(list1[i], list2[i]))
                            {
                                success = true;
                                index = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (FindPath(val1, val2))
                        {
                            success = true;
                        }
                    }
                    if (success)
                    {
                        Path.Add(new PropInfo(prpInfo, index));
                        break;
                    }
                }
            }
            return success;
        }


    }
}
