using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    /// <summary>
    /// A CustomException does not have its stack trace displayed.
    /// </summary>
    public class CustomException: Exception
    {
        public CustomException(string message): base(message)
        { }

        public CustomException(string message, Exception innerException): base(message, innerException)
        { }
    }
}
