using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class CopyPasteInfo
    {
        private List<object> copiedObjects { get; } = new List<object>();

        public int CurrentIndex { get; set; }

        public int Count => copiedObjects.Count;

        public object GetCopy()
        {
            return copiedObjects[CurrentIndex];
        }

        public void SetCopy(object obj)
        {
            if (CurrentIndex == copiedObjects.Count)
                copiedObjects.Add(obj);
            else
                copiedObjects[CurrentIndex] = obj;
        }

        public void Clear()
        {
            copiedObjects.Clear();
            CurrentIndex = 0;
        }
    }
}
