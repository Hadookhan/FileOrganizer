using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace FileOrganizer
{
    internal class Safety
    {
        public Safety() { }

        public string ResolveConflicts(string dest, object[] meta, YamlNode duplicatePolicyNode)
        {
            var policy = duplicatePolicyNode.ToString(); // however you read it

            // If no conflict, return dest as-is
            if (!File.Exists(dest)) return dest;

            var dir = Path.GetDirectoryName(dest) ?? "";
            var baseName = Path.GetFileNameWithoutExtension(dest);
            var ext = Path.GetExtension(dest);

            switch (policy)
            {
                case "keep_both_timestamp":
                    {
                        var alt = Path.Combine(dir, $"{baseName} ({DateTime.Now:yyyyMMdd-HHmmss}){ext}");
                        return alt;
                    }
                case "keep_both_counter":
                    {
                        int i = 1;
                        string alt;
                        do
                        {
                            alt = Path.Combine(dir, $"{baseName} ({i}){ext}");
                            i++;
                        } while (File.Exists(alt));
                        return alt;
                    }
                case "overwrite":
                    return dest;

                case "skip":
                default:
                    return "";
            }
        }

        public void SafeMove(string src, string dest)
        {
            try
            {
                // normalize + ensure parents
                dest = Path.GetFullPath(dest);
                var dir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                // try a straight move
                File.Move(src, dest);
            }
            catch (IOException ex) when (File.Exists(dest))
            {
                // destination exists → keep both with timestamp (or choose your policy)
                var d = Path.GetDirectoryName(dest);
                var baseName = Path.GetFileNameWithoutExtension(dest);
                var ext = Path.GetExtension(dest);
                var alt = Path.Combine(d, $"{baseName} ({DateTime.Now:yyyyMMdd-HHmmss}){ext}");
                Console.WriteLine($"[CONFLICT] {dest} exists. Using: {alt}");
                File.Move(src, alt);
            }
            catch (IOException ex)
            {
                // cross-volume or other IO → copy + delete
                Console.WriteLine($"[IO Fallback] {ex.Message}  → copy+delete");
                var input = File.Open(src, FileMode.Open, FileAccess.Read, FileShare.Read);
                var output = File.Open(dest, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                input.CopyTo(output);
                File.Delete(src);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SafeMove failed: {ex}");
                throw; // don’t hide it
            }
        }

        public void SafeCopy(string src, string dest)
        {
            var input = File.Open(src, FileMode.Open, FileAccess.Read, FileShare.Read);
            var output = File.Open(dest, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            input.CopyTo(output);
        }

        public void SafeDelete(string path)
        {
            File.Delete(path);
        }

        public bool TryEnsureAbsolutePath(string input, out string absolute)
        {
            absolute = string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var p = input.Trim().Trim('"'); // remove accidental quotes

            // quick invalid-char check
            if (p.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return false;

            // Windows: fix drive-relative like "C:folder" -> "C:\folder"
            if (Path.DirectorySeparatorChar == '\\'
                && p.Length >= 2 && char.IsLetter(p[0]) && p[1] == ':'
                && (p.Length == 2 || (p[2] != '\\' && p[2] != '/')))
            {
                p = p.Insert(2, "\\");
            }

            // Optional: reject stray ':' beyond the drive letter (e.g., "foo:bar")
            int colon = p.IndexOf(':');
            if (Path.DirectorySeparatorChar == '\\' && colon > 1) return false;

            try
            {
                absolute = Path.GetFullPath(p); // resolves .. and .
                return true;
            }
            catch
            {
                return false; // treat as unsupported/bad path
            }
        }

        public bool IsFullyQualifiedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            // Windows
            if (Path.DirectorySeparatorChar == '\\')
            {
                // UNC: \\server\share\...
                if (path.StartsWith(@"\\") || path.StartsWith("//")) return true;

                // Drive-rooted: C:\...
                return path.Length >= 3
                    && char.IsLetter(path[0])
                    && path[1] == ':'
                    && (path[2] == '\\' || path[2] == '/'); // must have the slash
            }

            // Unix/macOS: /...
            return path[0] == '/';
        }
    }
}
