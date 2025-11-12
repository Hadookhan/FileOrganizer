using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

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
                return doc;
            }
        }

        public object DecideAction(string path, object[] meta, YamlDocument rules, string sourceDir, string targetDir)
        {
            string destination = "";
            string action = "SKIP";
            object[] context = { path, meta, sourceDir, targetDir };

            var mapping = (YamlMappingNode)rules.RootNode;

            var Rules = (YamlSequenceNode)mapping.Children[new YamlScalarNode("orderedRules")];

            var deserializer = new DeserializerBuilder().Build();

            foreach (var node in Rules)
            {

                var rule = (YamlMappingNode)node;
                if (Match(rule["when"], context))
                {
                    Console.WriteLine(rule["when"]);
                    if (!rule.Children.TryGetValue(new YamlScalarNode("then"), out var thenNode))
                    {
                        continue;
                    }

                    if (thenNode is YamlMappingNode thenMap)
                    {
                        
                        if (thenMap.Children.TryGetValue(new YamlScalarNode("action"), out var actionNode))
                        {
                            action = (string)actionNode;
                            Console.WriteLine(action);
                        }

                        if (thenMap.Children.TryGetValue(new YamlScalarNode("setDestinationTemplate"), out var DestinationTemplateNode))
                        {
                            destination = $"{DestinationTemplateNode}";
                            Console.WriteLine(destination);
                        }

                        if (thenMap.Children.TryGetValue(new YamlScalarNode("continue"), out var continueNode)){
                            bool is_continue = deserializer.Deserialize<bool>((string)continueNode);
                            if (!is_continue) { break; }
                        }

                    }
                }
                Console.WriteLine(action);
                Console.WriteLine(destination);
            }
            return (action, destination);
        }

        private bool Match(YamlNode condition, object[] context)
        {
            object[] meta = (object[])context[1];
            string path = (string)context[2];


            return CheckExt(condition["extensions"], (string)meta[2]);
        }

        private bool CheckExt(YamlNode extensions, string extension)
        {
            //Console.WriteLine(extensions);
            //Console.WriteLine(extension);
            return ((YamlSequenceNode)extensions).Contains((YamlNode)extension);
        }
    }
}
