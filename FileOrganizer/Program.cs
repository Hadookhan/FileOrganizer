using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace FileOrganizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "/Users/had/OneDrive - MMU/Desktop/test/sdfsdf.txt";
            string rulePath = "../../rules.yaml";
            Organizer fileOrg = new Organizer();
            
            Core core = new Core();
            Rule rule = new Rule(rulePath);

            core.ProcessFile(path, fileOrg.sourceDir, fileOrg.targetDir);
            YamlDocument rulesDoc = rule.LoadRules();
            rule.DecideAction(path, core.GetFileMetaData(path), rulesDoc, "", "");
        }
    }
}
