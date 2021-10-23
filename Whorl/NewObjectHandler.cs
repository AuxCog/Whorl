using System;
using System.Collections.Generic;
using System.Linq;

namespace Whorl
{
    public class NewObjectHandler<TObject> where TObject : class
    {
        private TObject _currentObject;
        public TObject CurrentObject
        {
            get => _currentObject;
            set
            {
                if (_currentObject != value)
                {
                    _currentObject = value;
                    CurrentObjectChanged = true;
                }
            }
        }
        public bool CurrentObjectChanged { get; set; }
        private Dictionary<TObject, TObject> oldObjectsByNewObject { get; } = new Dictionary<TObject, TObject>();
        private Dictionary<TObject, List<TObject>> newObjectsByOldObject { get; } = new Dictionary<TObject, List<TObject>>();

        public void AddNewObject(TObject oldObject, TObject newObject)
        {
            if (oldObject == null)
                throw new NullReferenceException("oldObject cannot be null.");
            if (newObject == null)
                throw new NullReferenceException("newObject cannot be null.");
            oldObjectsByNewObject.Add(newObject, oldObject);
            if (!newObjectsByOldObject.TryGetValue(oldObject, out List<TObject> objList))
            {
                objList = new List<TObject>();
                newObjectsByOldObject.Add(oldObject, objList);
            }
            objList.Add(newObject);
        }

        public void RemoveNewObject(TObject newObject)
        {
            if (newObject == null)
                throw new NullReferenceException("newObject cannot be null.");
            if (!oldObjectsByNewObject.TryGetValue(newObject, out TObject oldObject))
            {
                return;
            }
            List<TObject> objList = newObjectsByOldObject[oldObject];
            objList.Remove(newObject);
            oldObjectsByNewObject.Remove(newObject);
        }

        public void RemoveNewObjects(IEnumerable<TObject> newObjects)
        {
            foreach (TObject newObject in newObjects)
            {
                RemoveNewObject(newObject);
            }
        }

        public bool ReplaceCurrentObject()
        {
            TObject obj = CurrentObject;
            if (obj == null)
                return false;
            while (newObjectsByOldObject.TryGetValue(obj, out var objList))
            {
                if (objList.Count != 1)
                    throw new Exception($"New object list's count == {objList.Count}, and should be 1.");
                obj = objList[0];
            }
            bool replaced = CurrentObject != obj;
            if (replaced)
                CurrentObject = obj;
            return replaced;
        }

    }
}
