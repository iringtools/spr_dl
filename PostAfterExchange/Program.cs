using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostAfterExchange
{
    class Program
    {
        static void Main(string[] args)
        {
          Console.WriteLine("SPR File Updater...");

            Post ObjPost = new Post();
            ObjPost.TestPost();

            Console.WriteLine("File Update complete.");
        }
    }
}
