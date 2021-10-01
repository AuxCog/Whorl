using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public class DesignLayerChangedEventArgs: EventArgs
    {
        public bool WhorlDesignChanged { get; }

        public DesignLayerChangedEventArgs(bool whorlDesignChanged)
        {
            this.WhorlDesignChanged = whorlDesignChanged;
        }
    }
}
