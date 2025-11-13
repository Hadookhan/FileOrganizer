using System;
using System.IO;

namespace FileOrganizer
{
    public class DestinationContext
    {
        public string SourcePath { get; private set; }
        public string SourceDir { get; private set; }
        public string TargetDir { get; private set; }
        public string FileName { get; private set; }
        public string FileNameNoExt { get; private set; }
        public string Extension { get; private set; } // without dot, lower
        public long SizeBytes { get; private set; }
        public DateTime CreatedAt { get; private set; } // UTC
        public DateTime ModifiedAt { get; private set; } // UTC
        public string Mime { get; private set; }

        public DestinationContext(
            string sourcePath, string sourceDir, string targetDir,
            string fileName, string fileNameNoExt, string extension,
            long sizeBytes, DateTime createdAt, DateTime modifiedAt, string mime)
        {
            SourcePath = sourcePath;
            SourceDir = sourceDir;
            TargetDir = targetDir;
            FileName = fileName;
            FileNameNoExt = fileNameNoExt;
            Extension = extension;
            SizeBytes = sizeBytes;
            CreatedAt = createdAt;
            ModifiedAt = modifiedAt;
            Mime = mime ?? string.Empty;
        }

        public static DestinationContext FromFile(string srcPath, string sourceDir, string targetDir, string mime = null)
        {
            var fi = new FileInfo(srcPath);
            var ext = fi.Extension.TrimStart('.').ToLowerInvariant();
            return new DestinationContext(
                srcPath,
                sourceDir,
                targetDir,
                fi.Name,
                Path.GetFileNameWithoutExtension(srcPath),
                ext,
                fi.Exists ? fi.Length : 0L,
                fi.Exists ? fi.CreationTimeUtc : DateTime.UtcNow,
                fi.Exists ? fi.LastWriteTimeUtc : DateTime.UtcNow,
                mime
            );
        }
    }
}
