using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class UndoOperation: IDisposable
    {
        public enum OperationTypes
        {
            Create,
            //Update,
            Delete,
            Replace,
            Shift,
            Zoom
        }

        public delegate void OperationFunction(bool undo, OperationTypes opType, BaseFrame opFrame);

        public class BaseFrame: IDisposable
        {
            public object NewObject { get; set; }
            public object ParentObject { get; set; }
            public int PrevListIndex { get; set; }
            public int ListIndex { get; set; }
            public object PreviousObject { get; set; }
            public object Tag { get; set; }

            private void DisposeObject(object o)
            {
                IDisposable disp = o as IDisposable;
                if (disp != null)
                    disp.Dispose();
            }

            public bool Disposed { get; private set; }

            public void Dispose()
            {
                if (Disposed)
                    return;
                DisposeObject(NewObject);
                DisposeObject(ParentObject);
                DisposeObject(PreviousObject);
                Disposed = true;
            }
        }

        private OperationTypes operationType;
        private List<BaseFrame> operationFrames;

        public OperationFunction OperationFn { get; set; }

        public OperationTypes OperationType
        {
            get { return operationType; }
        }

        private List<BaseFrame> OperationFrames
        {
            get { return operationFrames; }
        }

        private bool IsUndone { get; set; }

        public UndoOperation(OperationTypes operationType, OperationFunction replaceFn)
        {
            this.operationType = operationType;
            this.OperationFn = replaceFn;
            IsUndone = false;
            operationFrames = new List<BaseFrame>();
        }

        public void AddOperationFrame(object parentObj, object newObj, 
                                      object previousObj = null, int listIndex = -1,
                                      int prevListIndex = -1, object tag = null)
        {
            BaseFrame opFrame = new BaseFrame();
            opFrame.PreviousObject = previousObj;
            opFrame.ParentObject = parentObj;
            opFrame.NewObject = newObj;
            opFrame.ListIndex = listIndex;
            opFrame.PrevListIndex = prevListIndex;
            opFrame.Tag = tag;
            OperationFrames.Add(opFrame);
        }

        public void Undo()
        {
            if (this.IsUndone)
                throw new Exception("Cannot undo an operation which has been undone.");
            for (int i = OperationFrames.Count - 1; i >= 0; i--)
            {
                OperationFn.Invoke(true, OperationType, OperationFrames[i]);
            }
            this.IsUndone = true;
        }

        public void Redo()
        {
            if (!this.IsUndone)
                throw new Exception("Cannot redo an operation which has not been undone.");
            for (int i = 0; i < OperationFrames.Count; i++)
            {
                OperationFn.Invoke(false, OperationType, OperationFrames[i]);
            }
            this.IsUndone = false;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
                return;
            foreach (BaseFrame frame in OperationFrames)
                frame.Dispose();
            OperationFrames.Clear();
            Disposed = true;
        }
    }
}
