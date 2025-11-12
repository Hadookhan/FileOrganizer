using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileOrganizer
{
    internal class Core
    {
        public Core() { }

        //string rules, string dryRun, string logger
        public void ProcessFile(string path, string source, string target)
        {
            if (Directory.Exists(path))
            {
                throw new FileNotFoundException("Path not found.");
            }
            if (!Directory.Exists(source) || !Directory.Exists(target))
            {
                throw new DirectoryNotFoundException("Source or Target directory not found.");
            }

            object[] meta = GetFileMetaData(path);
        }

        private object[] GetFileMetaData(string path)
        {
            object[] metaData = new object[5];

            string name = Path.GetFileName(path);
            long size = new FileInfo(path).Length;
            string ext = Path.GetExtension(path);
            DateTime creationTime = File.GetCreationTime(path);

            metaData[0] = name;
            metaData[1] = size;
            metaData[2] = ext;
            metaData[3] = creationTime;

            return metaData;

            //string[] DMY = creationTime.Date.ToString().Split(' ')[0].Split('/');
            //int day = 0;
            //int month = 0;
            //int year = 0;


            //Console.WriteLine(creationTime.Date.ToString().Split(' ')[0]); // Date of creation
            //Console.WriteLine(creationTime.TimeOfDay); // Time of creation

            //for (int i = 0; i < DMY.Length; i++) // Iterates through Day/Month/Year values
            //{
            //    if (i == 0)
            //    {
            //        day = int.Parse(DMY[i]);
            //    }
            //    else if (i == 1)
            //    {
            //        month = int.Parse(DMY[i]);
            //    }
            //    else
            //    {
            //        year = int.Parse(DMY[i]);
            //    }
            //}
            //Console.WriteLine($"Day: {day}\n" +
            //                  $"Month: {month}\n" +
            //                  $"Year: {year}");
            //Path.GetFileName(path);

        }
    }
}
