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
            string rulePath = @"..\..\rules.yaml";
            string sourceDir = @"C:\Users\had\OneDrive - MMU\Desktop\test";
            string targetDir = @"C:\Users\had\Organised";

            if (!Directory.Exists(sourceDir) || !Directory.Exists(targetDir))
            {
                throw new DirectoryNotFoundException("Source or Target directory not found.");
            }

            Core core = new Core();
            Rule rule = new Rule(rulePath);
            YamlDocument rulesDoc = rule.LoadRules();

            Traverse DFS = new Traverse();
            foreach (var path in DFS.EnumerateAllFiles(sourceDir))
            {
                core.ProcessFile(path, sourceDir, targetDir, rulesDoc);
            }
        }
    }
}
