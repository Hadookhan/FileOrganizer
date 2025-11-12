using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileOrganizer
{
    internal class Core
    {
        public Core() { }

        //string rules, string dryRun, string logger
        public void ProcessFile(string path, string source, string target)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            FileAttributes meta = File.GetAttributes(path);
            
            Console.WriteLine(meta);
        }
    }
}
