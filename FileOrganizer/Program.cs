using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "/Users/had/OneDrive - MMU/Desktop/test/sdfsdf.txt";
            Organizer fileOrg = new Organizer();
            
            Core core = new Core();

            core.ProcessFile(path, fileOrg.sourceDir, fileOrg.targetDir);
        }
    }
}
