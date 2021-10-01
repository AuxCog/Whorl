using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParserEngine
{
    internal class ExceptionHandler
    {
        public static void HandleException(Exception ex)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                for (Exception ex1 = ex; ex1 != null; ex1 = ex1.InnerException)
                {
                    sb.AppendLine(ex1.Message);
                }
                sb.AppendLine(ex.StackTrace);
                MessageBox.Show(sb.ToString());
            }
            catch { }
        }

    }
}
