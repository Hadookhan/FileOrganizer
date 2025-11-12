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
        public string rulesFile;
        public string[] mode = { "one-shot", "watch" }; // Organizes folder once or watches folder and waits for updates
        public string dryrun;

        public Organizer()
        {

        }
    }
}
