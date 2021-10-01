using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public abstract class ChangeTracker : BaseObject
    {
        private bool _isChanged;
        public bool IsChanged 
        { 
            get { return _isChanged; }
            set
            {
                if (_isChanged != value)
                {
                    _isChanged = value;
                    IsChangedChanged();
                }
            }
        }

        protected void SetProperty<T>(ref T Var, T value)
        {
            if (!object.Equals(Var, value))
            {
                Var = value;
                IsChanged = true;
            }
        }

        protected virtual void IsChangedChanged()
        {
        }

    }
}
