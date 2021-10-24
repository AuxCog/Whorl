using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public abstract class GuidKey: BaseObject
    {
        public Guid KeyGuid { get; private set; }

        public GuidKey()
        {
            KeyGuid = Guid.NewGuid();
        }

        public GuidKey(GuidKey source)
        {
            KeyGuid = source.KeyGuid;
        }

        public void SetKeyGuid(GuidKey source)
        {
            KeyGuid = source.KeyGuid;
        }

        public void SetKeyGuid(Guid guid)
        {
            KeyGuid = guid;
        }

        public T FindByKeyGuid<T>(IEnumerable<T> list, bool throwException = false) where T: GuidKey
        {
            T obj = list.FirstOrDefault(o => o.KeyGuid == KeyGuid);
            if (obj == null && throwException)
                throw new NullReferenceException($"Couldn't find KeyGuid object of type {typeof(T).Name}.");
            return obj;
        }
        public T FindByKeyGuid<T, TParent>(IEnumerable<TParent> list, Func<TParent, IEnumerable<T>> func, 
                                           bool throwException = false) where T : GuidKey
        {
            T obj = null;
            foreach (TParent parent in list)
            {
                obj = FindByKeyGuid(func(parent), throwException: false);
                if (obj != null)
                    break;
            }
            if (obj == null && throwException)
                throw new NullReferenceException($"Couldn't find KeyGuid object of type {typeof(T).Name}.");
            return obj;
        }
    }
}
