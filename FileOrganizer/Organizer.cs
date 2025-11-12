using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizer
{

    internal class Organizer
    {
        public string sourceDir;
        public string targetDir;
        private string rulesFile;
        //public string dryrun;

        public Organizer()
        {

        }
        public Organizer(string sourceDir, string targetDir, string rulesFile)
        {
            this.sourceDir = sourceDir;
            this.targetDir = targetDir;
            this.rulesFile = rulesFile;
        }
    }
}
