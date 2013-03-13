using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bechtel.iRING.SPRUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SPR File Updater...");

            SPRSynchronizationUtility syncUtility = new SPRSynchronizationUtility();
            syncUtility.MDBSynchronization();
         // syncUtility.PopulateSpool();

            Console.WriteLine("File Update complete.");
        }
    }
}
