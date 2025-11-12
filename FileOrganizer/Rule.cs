using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace FileOrganizer
{
    internal class Rule
    {
        string rulesFile;
        public Rule(string rulesFile) { this.rulesFile = rulesFile; }

        public YamlDocument LoadRules()
        {
            string[] rulesFileSplit = rulesFile.Split('.');
            if (rulesFile == null || rulesFileSplit[rulesFileSplit.Length - 1] != "yaml")
            {
                throw new Exception("Invalid rules file");
            }
            using (StreamReader sr = new StreamReader(rulesFile))
            {
                YamlStream yaml = new YamlStream();
                yaml.Load(sr); // Loads yaml rule file
                YamlDocument doc = yaml.Documents[0];
                //var mapping = (YamlMappingNode)doc.RootNode;

                //foreach (var entry in mapping.Children)
                //{
                //    Console.WriteLine(((YamlScalarNode)entry.Key).Value);
                //}
                //var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("orderedRules")];
                //foreach (var item in items)
                //{
                //    foreach (var entry in (YamlMappingNode)item)
                //    {
                //        Console.WriteLine(((YamlScalarNode)entry.Key).Value);
                //    }
                //}
                return doc;
            }
        }
    }
}
