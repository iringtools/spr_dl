using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bechtel.iRING.SPRUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            StreamWriter logFile=null;

            try
            {
                if (!File.Exists("Log.txt"))
                {
                    logFile = new StreamWriter("Log.txt");
                }
                else
                {
                    logFile = File.AppendText("Log.txt");
                }

                logFile.WriteLine("Start Time:" + DateTime.Now);
     
                Console.WriteLine("SPR File Updater...");
                SPRSynchronizationUtility syncUtility = new SPRSynchronizationUtility(logFile);
                syncUtility.MDBSynchronization();
                // syncUtility.PopulateSpool();

                Console.WriteLine("File Update complete.");
                logFile.WriteLine("Log file saved successfully!");
                logFile.WriteLine("End Time:" + DateTime.Now);
                logFile.Close();
            }
            catch (Exception ex)
            {
                logFile.WriteLine("Whoops! Please contact the developers with the following"
                         + " information:\n\n" + ex.Message + ex.StackTrace,ex.InnerException.ToString(),
                         "Fatal Error");
                logFile.Close();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                Console.WriteLine("Whoops! Please contact the developers with the following"
                      + " information:\n\n" + ex.Message + ex.StackTrace,
                      "Fatal Error");
            }
            finally
            {
                AppDomain.Unload(AppDomain.CurrentDomain);
            }
        }
    }
   
}
