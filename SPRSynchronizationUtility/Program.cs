using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Bechtel.iRING.SPRUtility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            int errorCode = 0;
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmSPRSynchronizationUtility());
                Application.UseWaitCursor = false;
                return errorCode;
            }
            else
            {
                //Console version
                FrmSPRSynchronizationUtility synchronizationUtility = new FrmSPRSynchronizationUtility();
                if (args.Length == 3)
                {
                     errorCode = synchronizationUtility.InitializeConsoleSPRutility(args[0], args[1], args[2]);
                }
                else
                    Console.WriteLine("Please pass the projectName, mdb filepath, and comma seperated list of components.");
                
                return errorCode;
            }
            
        }
    }
}
