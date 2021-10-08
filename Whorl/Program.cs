using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!InitialSetup.InitializeSettings(out bool loadErrors))
            {
                return;
            }
            MainForm.LoadErrors = loadErrors;
            if (InitialSetup.InitialSetupNeeded())
            {
                var frm = new FrmInitialSetup();
                if (frm.ShowDialog() != DialogResult.OK)
                {
                   return;
                }
            }
            string designFileName = args.FirstOrDefault();
            Application.Run(new MainForm(designFileName));
        }
    }
}
