using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace FileOrganizer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string rulePath = @"..\..\rules.yaml";

            Core core = new Core();
            Rule rule = new Rule(rulePath);
            YamlDocument rulesDoc = rule.LoadRules();

            Traverse DFS = new Traverse();

            Console.WriteLine("------------------------------------------------------" +
                "\n                 GUIDELINE -\n" +
                "This program is used to MOVE files from your\n" +
                "specified directory to any folder of your choice.\n" +
                "Sub-directories will be created and categorise\n" +
                "your files by extension (e.g, .jpg, .png)\n" +
                "ANY FILES MOVED WILL BE DELETED FROM THEIR\n" +
                "               ORIGINAL FOLDER!\n" +
                "------------------------------------------------------");

            string fullPath = $"{getRoot()}";
            var rootStack = new Stack<string>();
            string sourceDir = null;
            string targetDir = null;

            while (true)
            {
                string newPath = Menu(fullPath);
                if (newPath == null)
                {
                    fullPath = $"{getRoot()}";
                    continue;
                }
                if (newPath == fullPath)
                {
                    continue;
                }
                rootStack.Push(newPath);
                fullPath = newPath;
                //Console.WriteLine(fullPath);
            }

            object getRoot()
            {
                return Directory.GetDirectoryRoot("~");
            }

            string Menu(string rootdir, int maxDepth = 1)
            {

                var stack = new Stack<string>();
                stack.Push(rootdir);
                int i = 0;
                var indexToDir = new Dictionary<int, string>();
                int depth = 0;

                while (stack.Count > 0 && depth < maxDepth)
                {
                    var dir = stack.Pop();
                    //Console.WriteLine($"[DIR] {dir}");

                    string[] subdirs;
                    try
                    {
                        subdirs = Directory.GetDirectories(dir);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //Console.WriteLine($"[SKIP] access denied: {dir}");
                        continue;
                    }
                    catch (IOException)
                    {
                        //Console.WriteLine($"[SKIP] IO error: {dir}");
                        continue;
                    }

                    Console.WriteLine($"------------------------------------------------------\n");
                    Console.WriteLine($"DIRECTORIES in {fullPath}\n");
                    foreach (var subdir in subdirs)
                    {
                        try
                        {
                            var attrs = File.GetAttributes(subdir);
                            bool isReparse = (attrs & FileAttributes.ReparsePoint) != 0;
                            bool isHidden = (attrs & FileAttributes.Hidden) != 0;
                            bool isSystem = (attrs & FileAttributes.System) != 0;
                            if (isReparse || isHidden || isSystem)
                            {
                                //Console.WriteLine($"[SKIP] {subdir}");
                                continue;
                            }
                            i++;
                            indexToDir[i] = subdir;
                            Console.WriteLine($"{i} -> {subdir} -> {attrs}");
                        }
                        catch { }

                        stack.Push(subdir);
                        depth++;
                    }
                    Console.WriteLine($"\n------------------------------------------------------");
                }
                int cmd = Option();

                switch (cmd)
                {
                    case 1:
                        if (rootStack.Count == 0)
                        {
                            Console.WriteLine("\nCURRENTLY AT ROOT DIRECTORY");
                            return null;
                        }
                        rootStack.Pop(); // popping current dir path (one before is the previous dir)
                        if (rootStack.Count == 0)
                        {
                            Console.WriteLine("\nCURRENTLY AT ROOT DIRECTORY");
                            return null;
                        }
                        return rootStack.Pop();
                    case 2:
                        if (indexToDir.Count <= 0)
                        {
                            Console.WriteLine("\nNO DIRECTORIES FOUND\n");
                            break;
                        }
                        Console.WriteLine("\n1. Enter directory number:\n");
                        do
                        {
                            if (int.TryParse(Console.ReadLine(), out cmd) && cmd > 0 && cmd <= indexToDir.Count)
                            {
                                break;
                            }
                            Console.WriteLine("Invalid input");
                        } while (true);
                        return indexToDir[cmd];
                    case 3:
                        int j = 0;
                        Console.WriteLine($"------------------------------------------------------\n");
                        Console.WriteLine($"FILES in {fullPath}\n");
                        string[] files = Directory.GetFiles(fullPath);
                        if (files.Length == 0)
                        {
                            Console.WriteLine("NO FILES FOUND");
                        }
                        foreach (var file in files)
                        {
                            i++;
                            Console.WriteLine($"{i} -> {file}");
                        }
                        Console.WriteLine($"\n------------------------------------------------------\n");
                        break;
                    case 4:
                        sourceDir = fullPath;
                        if (sourceDir == targetDir)
                        {
                            Console.WriteLine($"Source cannot be same path as Target!");
                            break;
                        }
                        Console.WriteLine($"Source Destination successfully set to: {sourceDir}");
                        break;
                    case 5:
                        targetDir = fullPath;
                        if (targetDir == sourceDir)
                        {
                            Console.WriteLine($"Target cannot be same path as Source!");
                            break;
                        }
                        Console.WriteLine($"Target Destination successfully set to: {targetDir}");
                        break;
                    case 6:
                        if (sourceDir == null || targetDir == null)
                        {
                            Console.WriteLine("\nSource or Target has not been specified!\n");
                            break;
                        }
                        if (!Directory.Exists(sourceDir) || !Directory.Exists(targetDir))
                        {
                            Console.WriteLine("Source or Target directory cannot be found!");
                            break;
                        }
                        Console.WriteLine($"------------------------------------------------------\n");
                        Console.WriteLine($"Source: {sourceDir}\n" +
                                          $"Target: {targetDir}\n" +
                                          $"Would you like to proceed to moving ALL files from {sourceDir} -> {targetDir}?\n\n" +
                                          $"1. Yes\n" +
                                          $"2. No");
                        Console.WriteLine($"\n------------------------------------------------------");
                        do
                        {
                            if (int.TryParse(Console.ReadLine(), out cmd) && cmd > 0 && cmd <= 2)
                            {
                                break;
                            }
                            Console.WriteLine("Invalid input");
                        } while (true);
                        if (cmd == 1)
                        {
                            Console.WriteLine("Press ENTER to continue or Close program to exit.");
                            Console.ReadKey();
                            
                            foreach (var path in DFS.EnumerateAllFiles(sourceDir))
                            {
                                core.ProcessFile(path, sourceDir, targetDir, rulesDoc);
                            }

                            Console.WriteLine($"Items successfully transferred to -> {targetDir}");
                            break;
                        }
                        break;
                }

                return fullPath;
            }

            int Option()
            {
                Console.WriteLine($"------------------------------------------------------\n");
                Console.WriteLine("         COMMANDS - \n");
                Console.WriteLine("1. Return to previous directory\n" +
                    "2. Select directory to enter\n" +
                    "3. Show files in current directory\n" +
                    "4. Mark current directory as source\n" +
                    "5. Mark current directory as target\n" +
                    "6. MOVE source files -> target directory (Organises files)\n");
                Console.WriteLine($"------------------------------------------------------\n");
                int cmd = 0;
                do
                {
                    if (int.TryParse(Console.ReadLine(), out cmd) && cmd > 0 && cmd <= 6)
                    {
                        break;
                    }
                    Console.WriteLine("Invalid input");
                } while (true);

                return cmd;
            }
        }
    }
}
