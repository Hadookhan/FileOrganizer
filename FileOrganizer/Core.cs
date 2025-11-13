using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace FileOrganizer
{
    internal class Core
    {
        public Core() { }

        //string rules, string dryRun, string logger
        public void ProcessFile(string path, string sourceDir, string targetDir, YamlDocument rules)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"File not found: {path}");


            var rule = new Rule("");
            YamlMappingNode map = (YamlMappingNode)rules.RootNode;

            object[] meta = GetFileMetaData(path);
            List<string> actions = rule.DecideAction(path, meta, rules, sourceDir, targetDir);

            Safety s = new Safety();
            string dest = actions.Count > 1 ? actions[1] : string.Empty;

            string destFinal = s.ResolveConflicts(dest, meta, map["duplicatePolicy"]);
            if (!s.TryEnsureAbsolutePath(destFinal, out destFinal))
            {
                Console.WriteLine($"[SKIP-BADPATH] {destFinal}");
                return;
            }
            if (!s.IsFullyQualifiedPath(destFinal))
                throw new InvalidOperationException($"Destination is not absolute: '{destFinal}'");

            if (string.IsNullOrWhiteSpace(destFinal) || destFinal.EndsWith(".SKIP", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[SKIP-CONFLICT] {path}");
                return;
            }

            var destDir = Path.GetDirectoryName(destFinal);
            if (!string.IsNullOrEmpty(destDir))
                Directory.CreateDirectory(destDir);

            Console.WriteLine($@"
                              SRC   : {path}
                              DEST  : {destFinal}
                              ROOT  : {Path.GetPathRoot(destFinal)}
                              FULL? : {s.IsFullyQualifiedPath(destFinal)}
                              ");

            switch (actions[0])
            {
                case "MOVE":
                    s.SafeMove(path, destFinal);
                    break;
                case "COPY":
                    s.SafeCopy(path, destFinal);
                    break;
                case "DELETE":
                    s.SafeDelete(path);
                    break;
                case "SKIP":
                    break;
                default:
                    throw new ArgumentException("Invalid action.");
            }
            Console.WriteLine((actions[0], actions[1]));

        }

        public object[] GetFileMetaData(string path)
        {
            var fi = new FileInfo(path);
            // Normalize the extension: no dot, lower-case
            string ext = fi.Extension.TrimStart('.').ToLowerInvariant();
            return new object[]
            {
                fi.Name,                // 0: name
                fi.Length,              // 1: size
                ext,                    // 2: extension (normalized)
                fi.CreationTimeUtc,     // 3: created (UTC)
                fi.LastWriteTimeUtc     // 4: modified (UTC)
            };
        }
    }
}
