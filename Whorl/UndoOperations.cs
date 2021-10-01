using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class UndoOperations: IDisposable
    {
        public delegate void UndoInfoFn(int undoIndex, bool canUndo, bool canRedo);
        public event UndoInfoFn UndoInfoChanged;
        private List<UndoOperation> operations;
        private int undoIndex = -1;

        private int UndoIndex
        {
            get { return undoIndex; }
            set
            {
                if (undoIndex != value)
                {
                    undoIndex = value;
                    if (UndoInfoChanged != null)
                        UndoInfoChanged(undoIndex, CanUndo, CanRedo);
                }
            }
        }

        public UndoOperations()
        {
            operations = new List<UndoOperation>();
        }

        private List<UndoOperation> Operations
        {
            get { return operations; }
        }

        public void AddOperation(UndoOperation operation)
        {
            int undoIndex = UndoIndex + 1;
            if (undoIndex < Operations.Count)
                Operations.RemoveRange(undoIndex, Operations.Count - undoIndex);
            Operations.Add(operation);
            UndoIndex = undoIndex;
        }

        public UndoOperation LastOperation
        {
            get
            {
                if (Operations.Count != 0)
                    return Operations[Operations.Count - 1];
                else
                    throw new Exception("No LastOperation exists.");
            }
        }

        public void RemoveLastOperation()
        {
            Operations.Remove(LastOperation);
            UndoIndex--;
        }

        public bool CanUndo
        {
            get { return UndoIndex >= 0 && UndoIndex < Operations.Count; }
        }

        public bool CanRedo
        {
            get { return UndoIndex >= -1 && UndoIndex < Operations.Count - 1; }
        }

        public bool UndoOperation()
        {
            bool retVal = CanUndo;
            if (retVal)
            {
                UndoOperation operation = Operations[UndoIndex--];
                operation.Undo();
            }
            return retVal;
        }

        public bool RedoOperation()
        {
            bool retVal = CanRedo;
            if (retVal)
            {
                UndoOperation operation = Operations[++UndoIndex];
                operation.Redo();
            }
            return retVal;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            foreach (UndoOperation operation in Operations)
                operation.Dispose();
            Operations.Clear();
        }
    }
}
