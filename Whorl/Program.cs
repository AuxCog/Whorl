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

            InitialSetup.InitializeSettings(out bool loadErrors);
            MainForm.LoadErrors = loadErrors;
            if (InitialSetup.InitialSetupNeeded())
            {
                var frm = new FrmInitialSetup();
                if (frm.ShowDialog() != DialogResult.OK)
                {
                    Application.Exit();
                }
            }
            string designFileName = args.FirstOrDefault();
            Application.Run(new MainForm(designFileName));
        }
    }
}
