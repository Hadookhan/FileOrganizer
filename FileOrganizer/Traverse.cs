using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizer
{
    internal class Traverse
    {

        public Traverse() { }

        public IEnumerable<string> EnumerateAllFiles(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                IEnumerable<string> files = Array.Empty<string>();
                IEnumerable<string> subdirs = Array.Empty<string>();

                try { files = Directory.EnumerateFiles(dir); } catch { }
                foreach (var f in files) yield return f;

                try { subdirs = Directory.EnumerateDirectories(dir); } catch { }
                foreach (var d in subdirs)
                {
                    try
                    {
                        var attr = File.GetAttributes(d);
                        if ((attr & FileAttributes.ReparsePoint) != 0) continue;
                    }
                    catch { }

                    stack.Push(d);
                }
            }
        }
    }
}
